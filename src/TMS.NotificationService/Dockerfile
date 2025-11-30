# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Установка postgresql-client для pg_isready
RUN apt-get update && \
    apt-get install -y postgresql-client && \
    rm -rf /var/lib/apt/lists/*

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
COPY ["src/TMS.NotificationService/TMS.NotificationService.csproj", "src/TMS.NotificationService/"]
COPY ["src/TMS.Common/TMS.Common.csproj", "src/TMS.Common/"]

# Восстанавливаем зависимости
RUN dotnet restore "./src/TMS.NotificationService/TMS.NotificationService.csproj"

# Копируем только исходный код нужных проектов
COPY ["src/TMS.NotificationService/", "src/TMS.NotificationService/"]
COPY ["src/TMS.Common/", "src/TMS.Common/"]

WORKDIR "/src/src/TMS.NotificationService"
RUN dotnet build "./TMS.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TMS.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Устанавливаем необходимые пакеты в final образ
RUN apt-get update && \
    apt-get install -y curl postgresql-client && \
    rm -rf /var/lib/apt/lists/*

# Копируем опубликованное приложение
COPY --from=publish /app/publish .

# Копируем entrypoint скрипт (исправленный путь)
COPY ../../scripts/entrypoint.sh .
RUN chmod +x entrypoint.sh

# Создаём папку для хранения "почтовых" сообщений
RUN mkdir -p /app/emails/events && chmod 755 /app/emails/events
VOLUME /app/emails/events

ENTRYPOINT ["./entrypoint.sh"]