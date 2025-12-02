@echo off
REM fix-and-build.bat - запускайте из папки scripts

echo Running from: %~dp0
cd /d "%~dp0.."
echo Changed to: %CD%

echo.
echo 1. Fixing line endings in setup-entrypoint.sh...
"C:\Program Files\Git\bin\bash.exe" -c "dos2unix scripts/setup-entrypoint.sh"

echo.
echo 2. Running docker-compose build...
docker-compose build

echo.
if %errorlevel% equ 0 (
    echo SUCCESS! Run 'docker-compose up -d' to start containers.
    docker-compose up -d
) else (
    echo BUILD FAILED! Check errors above.
)

pause