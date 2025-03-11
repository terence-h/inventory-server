# ***********************************************
# * Check and install .NET SDK and PostgreSQL   *
# ***********************************************

# Define installer file names and URLs
$sdkInstaller = "dotnet-sdk-8.0.406-win-x64.exe"
$sdkUrl = "https://download.visualstudio.microsoft.com/download/pr/bd44cdb8-dcac-4f1f-8246-1ee392c68dac/ba818a6e513c305d4438c7da45c2b085/$sdkInstaller"

$pgInstaller = "postgresql-17.4-1-windows-x64.exe"
$pgUrl = "https://get.enterprisedb.com/postgresql/$pgInstaller"

# --- Check for .NET SDK ---
Write-Host "Checking for .NET SDK..."
$sdkOutput = & dotnet --list-sdks 2>$null
if ([string]::IsNullOrWhiteSpace($sdkOutput)) {
    Write-Host ".NET SDK not found. Installing .NET 8.0 SDK..."
    
    # Check if the SDK installer already exists in the script root
    $sdkInstallerPath = Join-Path $PSScriptRoot $sdkInstaller
    if (-Not (Test-Path $sdkInstallerPath)) {
        Write-Host "Downloading .NET 8.0 SDK installer..."
        Invoke-WebRequest -Uri $sdkUrl -OutFile $sdkInstallerPath
    }
    else {
        Write-Host "SDK installer already exists. Skipping download."
    }
    
    Write-Host "Installing .NET 8.0 SDK silently..."
    Start-Process -FilePath (Join-Path $PSScriptRoot $sdkInstaller) -ArgumentList "/install", "/quiet", "/norestart" -Wait
    
    Write-Host "Cleaning up SDK installer..."
    Remove-Item $sdkInstallerPath -Force
} 
else {
    Write-Host ".NET SDK is already installed."
}

# --- Check for PostgreSQL ---
Write-Host "Checking for PostgreSQL..."

# Use service check instead of Get-Command for PostgreSQL
try {
    $pgService = Get-Service -Name "postgresql-17" -ErrorAction Stop
    Write-Host "PostgreSQL is already installed (service found)."
}
catch {
    Write-Host "PostgreSQL not found. Installing PostgreSQL..."
    
    # Check if the PostgreSQL installer exists in the script root
    $pgInstallerPath = Join-Path $PSScriptRoot $pgInstaller
    if (-Not (Test-Path $pgInstallerPath)) {
        Write-Host "Downloading PostgreSQL installer..."
        Invoke-WebRequest -Uri $pgUrl -OutFile $pgInstallerPath
    }
    else {
        Write-Host "PostgreSQL installer already exists. Skipping download."
    }
    
    Write-Host "Installing PostgreSQL silently..."
    $pgArgs = @(
        "--mode", "unattended",
        "--unattendedmodeui", "minimal",
        "--superpassword", "password",
        "--servicename", "postgresql-17",
        "--serverport", "5432",
        "--prefix", "`"C:\Program Files\PostgreSQL\17`"",
        "--datadir", "`"C:\Program Files\PostgreSQL\17\data`""
    )
    Start-Process -FilePath (Join-Path $PSScriptRoot $pgInstaller) -ArgumentList $pgArgs -Wait
    
    Write-Host "Cleaning up PostgreSQL installer..."
    Remove-Item $pgInstallerPath -Force
}

# ***********************************************
# *         Build and run the project         *
# ***********************************************

# Set the project path.
# Change this to the directory containing your .csproj or solution file.
$projectPath = $PSScriptRoot

Write-Host "Changing directory to project path: $projectPath"
Set-Location $projectPath

Write-Host "Building the project..."
& dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed."
    Read-Host -Prompt "Press Enter to exit"
    exit 1
}

Write-Host "Running the project..."
& dotnet run

# Pause so the window remains open after execution.
Read-Host -Prompt "Press Enter to exit"
