@echo off
echo Checking for .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo .NET SDK is not installed. Please install it from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo Building the project...
dotnet build
if errorlevel 1 (
    echo Build failed.
    pause
    exit /b 1
)

echo Running the project...
dotnet run

pause
