# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Устанавливаем curl для health checks
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем файлы пакетов
COPY ["Directory.Packages.props", "."]
COPY ["src/TMS.ApiGateway/TMS.ApiGateway.csproj", "src/TMS.ApiGateway/"]
COPY ["src/TMS.Common/TMS.Common.csproj", "src/TMS.Common/"]
COPY ["src/TMS.Contracts/TMS.Contracts.csproj", "src/TMS.Contracts/"]

# Восстанавливаем зависимости
RUN dotnet restore "./src/TMS.ApiGateway/TMS.ApiGateway.csproj"

# Копируем исходный код всех необходимых проектов
COPY ["src/TMS.ApiGateway/", "src/TMS.ApiGateway/"]
COPY ["src/TMS.Common/", "src/TMS.Common/"]
COPY ["src/TMS.Contracts/", "src/TMS.Contracts/"]

WORKDIR "/src/src/TMS.ApiGateway"
RUN dotnet build "./TMS.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TMS.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TMS.ApiGateway.dll"]