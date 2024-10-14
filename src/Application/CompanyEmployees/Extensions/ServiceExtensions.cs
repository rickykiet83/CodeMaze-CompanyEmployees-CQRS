using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using CompanyEmployees.Presentation.Controllers;
using Contracts;
using Entities.ConfigurationModels;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Service;
using Service.Contracts;

namespace CompanyEmployees.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("X-Pagination")
            );
        });

    public static void ConfigureIISIntegration(this IServiceCollection services) =>
        services.Configure<IISOptions>(options => { });

    public static void ConfigureLoggerService(this IServiceCollection services) =>
        services.AddSingleton<ILoggerManager, LoggerManager>();

    public static void ConfigureRepositoryManager(this IServiceCollection services) =>
        services.AddScoped<IRepositoryManager, RepositoryManager>();

    public static void ConfigureServiceManager(this IServiceCollection services) =>
        services.AddScoped<IServiceManager, ServiceManager>();

    public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<RepositoryContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("sqlConnection")));

    public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
        builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

    public static void AddCustomMediaTypes(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(config =>
        {
            var systemTextJsonOutputFormatter = config.OutputFormatters
                .OfType<SystemTextJsonOutputFormatter>()?
                .FirstOrDefault();

            if (systemTextJsonOutputFormatter != null)
            {
                systemTextJsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.hateoas+json");
                systemTextJsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.apiroot+json");
            }

            var xmlOutputFormatter = config.OutputFormatters
                .OfType<XmlDataContractSerializerOutputFormatter>()?
                .FirstOrDefault();

            if (xmlOutputFormatter != null)
            {
                xmlOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.hateoas+xml");
                xmlOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.apiroot+xml");
            }
        });
    }

    public static void ConfigureVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(opt =>
        {
            opt.ReportApiVersions = true;
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.DefaultApiVersion = new ApiVersion(1, 0);
            opt.ApiVersionReader = new HeaderApiVersionReader("api-version");
            // opt.ApiVersionReader = new HeaderApiVersionReader("v");
            // opt.ApiVersionReader = new QueryStringApiVersionReader("v");
        }).AddMvc(opt =>
        {
            opt.Conventions.Controller<CompaniesController>()
                .HasApiVersion(new ApiVersion(1, 0));
            opt.Conventions.Controller<CompaniesV2Controller>()
                .HasDeprecatedApiVersion(new ApiVersion(2, 0));
        });
    }

    public static void ConfigureResponseCaching(this IServiceCollection services) => services.AddResponseCaching();

    public static void ConfigureOutputCaching(this IServiceCollection services) =>
        services.AddOutputCache(opt =>
        {
            // opt.AddBasePolicy(bp => bp.Expire(TimeSpan.FromSeconds(10)));
            opt.AddPolicy("120SecondsDuration", p => p.Expire(TimeSpan.FromSeconds(120)));
        });

    public static void ConfigureRateLimitingOptions(this IServiceCollection services)
    {
        // AddRateLimiter is an extension method from the RateLimiting package
        services.AddRateLimiter(opt =>
        {
            // By configuring the GlobalLimiter property, we want our limit options to apply to all endpoints. So,
            // we use the PartitionedRateLimiter.Create method to create a default limiting implementation
            opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter("GlobalLimiter",
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        // QueueLimit = 0, // No queuing
                        QueueLimit = 2, // Queuing
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // First in, first out
                        Window = TimeSpan.FromMinutes(1)
                    }
                )
            );
            // We can also create a specific limiting implementation for a specific endpoint. We use the AddPolicy
            // method to create a specific limiting implementation. We set the PermitLimit to 3 and the Window to 10
            // seconds.
            opt.AddPolicy("SpecificPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter("SpecificLimiter",
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 3,
                        Window = TimeSpan.FromSeconds(10)
                    }));

            // The OnRejected property is a callback that is called when the rate limit is exceeded. We set the status
            // code to 429 (Too Many Requests) and write a message to the response body. We also check if the lease
            // contains the RetryAfter metadata and write the value to the response body.
            opt.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    await context.HttpContext.Response
                        .WriteAsync($"Too many requests. Please try again after {retryAfter.TotalSeconds} second(s).",
                            token);
                else
                    await context.HttpContext.Response
                        .WriteAsync("Too many requests. Please try again later.", token);
            };
        });
    }
    
    public static void ConfigureIdentity(this IServiceCollection services)
    {
        var builder = services.AddIdentity<User, IdentityRole>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 10;
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<RepositoryContext>()
            .AddDefaultTokenProviders();
    }
    
    public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfiguration = new JwtConfiguration();
        configuration.Bind(jwtConfiguration.Section, jwtConfiguration);
        
        var secretKey = configuration["JWT_SECRET"];

        services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtConfiguration.ValidIssuer,
                    ValidAudience = jwtConfiguration.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
    }

    public static void AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["JWT_SECRET"];
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentNullException("JWT_SECRET");
        
        // Bind the JwtSettings section from the configuration
        services.Configure<JwtConfiguration>(config =>
        {
            configuration.GetSection("JwtSettings").Bind(config); // Binds all JwtSettings properties
            config.Secret = secret;  // Manually set secret as it's not part of JwtSettings
        });
    }
    
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "Code Maze API", 
                Version = "v1",
                Description = "CompanyEmployees API by CodeMaze",
                TermsOfService = new Uri("https://example.com/terms"),
                Contact = new OpenApiContact
                {
                    Name = "John Doe",
                    Email = "John.Doe@gmail.com",
                    Url = new Uri("https://twitter.com/johndoe"),
                },
                License = new OpenApiLicense
                {
                    Name = "CompanyEmployees API LICX",
                    Url = new Uri("https://example.com/license"),
                }
            });
            s.SwaggerDoc("v2", new OpenApiInfo { Title = "Code Maze API", Version = "v2" });

            var xmlFile = $"{typeof(Presentation.AssemblyReference).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                s.IncludeXmlComments(xmlPath);

            s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Place to add JWT with Bearer",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            s.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Name = "Bearer",
                    },
                    new List<string>()
                }
            });
        });
    }
}