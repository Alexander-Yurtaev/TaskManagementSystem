@echo off
REM fix-and-build.bat - запускайте из папки scripts

echo Running from: %~dp0
cd /d "%~dp0.."
echo Changed to: %CD%

echo.
echo 1. Fixing line endings in setup-entrypoint.sh...
powershell -Command "(Get-Content 'scripts\setup-entrypoint.sh' -Raw) -replace \"`r`n\", \"`n\" | Set-Content 'scripts\setup-entrypoint.sh' -NoNewline -Encoding UTF8"

echo.
echo 2. Running docker-compose build...
docker-compose build

echo.
if %errorlevel% equ 0 (
    echo SUCCESS! Run 'docker-compose up -d' to start containers.
) else (
    echo BUILD FAILED! Check errors above.
)

pause