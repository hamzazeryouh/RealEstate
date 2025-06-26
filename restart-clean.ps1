# PowerShell script to restart the application cleanly without warnings
Write-Host "🔧 Fixing Razor warnings and restarting application..." -ForegroundColor Green

# Stop any running RealEstate processes
Write-Host "⏹️ Stopping any running RealEstate processes..." -ForegroundColor Yellow
Get-Process -Name "*RealEstate*" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Clean build artifacts
Write-Host "🧹 Cleaning build artifacts..." -ForegroundColor Yellow
if (Test-Path "src\**\bin") { Remove-Item -Recurse -Force "src\**\bin" -ErrorAction SilentlyContinue }
if (Test-Path "src\**\obj") { Remove-Item -Recurse -Force "src\**\obj" -ErrorAction SilentlyContinue }
if (Test-Path "tests\**\bin") { Remove-Item -Recurse -Force "tests\**\bin" -ErrorAction SilentlyContinue }
if (Test-Path "tests\**\obj") { Remove-Item -Recurse -Force "tests\**\obj" -ErrorAction SilentlyContinue }

# Clear NuGet cache
Write-Host "📦 Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals http-cache --clear

# Restore packages
Write-Host "📥 Restoring packages..." -ForegroundColor Yellow
dotnet restore RealEstate.sln --force --no-cache --verbosity minimal

# Build solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
dotnet build RealEstate.sln --no-restore --verbosity minimal

Write-Host "✅ Setup complete! Starting application..." -ForegroundColor Green
Write-Host "🌐 Application will be available at:" -ForegroundColor Cyan
Write-Host "   HTTPS: https://localhost:53835" -ForegroundColor Cyan
Write-Host "   HTTP:  http://localhost:53836" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
Write-Host ""

# Start the application
dotnet run --project src/RealEstate.Web.Host 