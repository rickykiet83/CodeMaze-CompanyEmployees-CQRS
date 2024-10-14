FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Application/CompanyEmployees/CompanyEmployees.csproj", "src/Application/CompanyEmployees/"]
COPY ["src/Infrastructure/Service/Service.csproj", "src/Infrastructure/Service/"]
COPY ["src/Domain/Shared/Shared.csproj", "src/Domain/Shared/"]
COPY ["src/Domain/Entities/Entities.csproj", "src/Domain/Entities/"]
COPY ["src/Domain/Contracts/Contracts.csproj", "src/Domain/Contracts/"]
COPY ["src/Application/Service.Contracts/Service.Contracts.csproj", "src/Application/Service.Contracts/"]
COPY ["src/Application/CompanyEmployees.Presentation/CompanyEmployees.Presentation.csproj", "src/Application/CompanyEmployees.Presentation/"]
COPY ["src/Infrastructure/Repository/Repository.csproj", "src/Infrastructure/Repository/"]
COPY ["src/Infrastructure/LoggerService/LoggerService.csproj", "src/Infrastructure/LoggerService/"]
RUN dotnet restore "src/Application/CompanyEmployees/CompanyEmployees.csproj"
COPY . .
WORKDIR "/src/src/Application/CompanyEmployees"
RUN dotnet build "CompanyEmployees.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CompanyEmployees.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CompanyEmployees.dll"]
