## Docker commands:
- Cd to root directory of the project
- run docker compose: ```docker compose -f docker-compose.yml -p company-employee-cqrs up -d```

## Migration Commands:
- Cd to main project directory (where CompanyEmployees.csproj is located)
- Create Migration: ```dotnet ef migrations add <MigrationName>``` ()
- Update Database: ```dotnet ef database update```

## Secret Key Commands:
- Reference Source: [Link](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=linux)
- ```dotnet user-secrets set "JWT_SECRET" "YourSecretKey"```
- ```dotnet user-secrets list```
- ```dotnet user-secrets remove "JWT_SECRET"```
- ```dotnet user-secrets clear```
