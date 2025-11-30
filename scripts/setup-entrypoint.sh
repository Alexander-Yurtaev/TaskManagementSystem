#!/bin/bash
set -e

# Configuration
SERVICE_NAME=${SERVICE_NAME:-"Unknown Service"}
DLL_NAME=${DLL_NAME:-"app.dll"}
APPLY_MIGRATIONS=${APPLY_MIGRATIONS:-"false"}
MAX_DB_RETRIES=${MAX_DB_RETRIES:-30}
DB_RETRY_INTERVAL=${DB_RETRY_INTERVAL:-2}

echo "=========================================="
echo "Initializing: $SERVICE_NAME"
echo "DLL: $DLL_NAME"
echo "Apply Migrations: $APPLY_MIGRATIONS"
echo "=========================================="

# Database waiting logic
wait_for_database() {
    if [ -n "$POSTGRES_HOST" ]; then
        echo "Waiting for database: $POSTGRES_HOST"
        echo "Database user: $POSTGRES_USER"
        echo "Database name: $POSTGRES_DB"
        
        local counter=0
        until pg_isready -h "$POSTGRES_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB" || [ $counter -eq $MAX_DB_RETRIES ]; do
            counter=$((counter + 1))
            echo "Database not ready (attempt $counter/$MAX_DB_RETRIES) - retrying in ${DB_RETRY_INTERVAL}s..."
            sleep $DB_RETRY_INTERVAL
        done
        
        if [ $counter -eq $MAX_DB_RETRIES ]; then
            echo "ERROR: Database not ready after $MAX_DB_RETRIES attempts"
            exit 1
        fi
        
        echo "✓ Database is ready"
    else
        echo "No POSTGRES_HOST specified, skipping database wait"
    fi
}

# Migration logic using --setup flag
run_migrations() {
    if [ "$APPLY_MIGRATIONS" = "true" ]; then
        echo "Applying database migrations using --setup flag..."
        
        # Проверяем, существует ли DLL файл
        if [ ! -f "$DLL_NAME" ]; then
            echo "ERROR: Application DLL '$DLL_NAME' not found for migrations!"
            exit 1
        fi
        
        # Запускаем миграции с флагом --setup
        echo "Executing: dotnet $DLL_NAME --setup"
        if dotnet "$DLL_NAME" --setup; then
            echo "✓ Migrations completed successfully"
        else
            echo "✗ Migrations failed with exit code $?"
            exit 1
        fi
    else
        echo "Migrations disabled (APPLY_MIGRATIONS=false)"
    fi
}

# Health check preparation
setup_health_check() {
    echo "Setting up health check endpoints..."
    mkdir -p /tmp/health
    echo "OK" > /tmp/health/startup
}

# Main execution flow
main() {
    echo "Step 1: Waiting for database..."
    wait_for_database

    echo "Step 2: Running migrations..."
    run_migrations

    echo "Step 3: Preparing health checks..."
    setup_health_check

    echo "=========================================="
    echo "Step 4: Starting application: $DLL_NAME"
    echo "=========================================="

    # Проверяем, существует ли DLL файл
    if [ ! -f "$DLL_NAME" ]; then
        echo "ERROR: Application DLL '$DLL_NAME' not found!"
        echo "Available files:"
        ls -la *.dll 2>/dev/null || echo "No DLL files found"
        exit 1
    fi

    # Запускаем приложение БЕЗ флага setup
    echo "Launching: dotnet $DLL_NAME"
    exec dotnet "$DLL_NAME"
}

# Запускаем основную функцию
main "$@"