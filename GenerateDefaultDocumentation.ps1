# FlowT Documentation Generation Script
# Generates API documentation using DefaultDocumentation tool

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "FlowT Documentation Generation" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

$solutionRoot = $PSScriptRoot
$projectPath = Join-Path $solutionRoot "src\FlowT"
$xmlDocPath = Join-Path $solutionRoot "src\FlowT\bin\Release\net10.0\FlowT.xml"
$configPath = Join-Path $solutionRoot "DefaultDocumentation.json"
$outputPath = Join-Path $solutionRoot "docs\api"

# Check if project directory exists
if (-not (Test-Path $projectPath)) {
    Write-Host "ERROR: Source directory not found!" -ForegroundColor Red
    Write-Host "Expected: $projectPath" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Check if XML documentation file exists
if (-not (Test-Path $xmlDocPath)) {
    Write-Host "WARNING: XML documentation not found. Building project first..." -ForegroundColor Yellow
    
    $csprojPath = Join-Path $projectPath "FlowT.csproj"
    dotnet build $csprojPath -c Release
    
    if (-not (Test-Path $xmlDocPath)) {
        Write-Host "ERROR: Build failed or XML documentation not generated!" -ForegroundColor Red
        Write-Host "Make sure GenerateDocumentationFile is enabled in FlowT.csproj" -ForegroundColor Yellow
        Read-Host "Press Enter to exit"
        exit 1
    }
}

# Check if DefaultDocumentation tool is installed
$toolInstalled = dotnet tool list -g | Select-String "defaultdocumentation"
if (-not $toolInstalled) {
    Write-Host "DefaultDocumentation tool not installed. Installing..." -ForegroundColor Yellow
    dotnet tool install -g DefaultDocumentation.Console
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install DefaultDocumentation tool!" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
}

# Check if configuration file exists
if (-not (Test-Path $configPath)) {
    Write-Host "ERROR: DefaultDocumentation.json not found!" -ForegroundColor Red
    Write-Host "Expected: $configPath" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Generating documentation..." -ForegroundColor Green
Write-Host ""

# Generate documentation
defaultdocumentation --ConfigurationFilePath $configPath

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Documentation generation failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Documentation generated successfully!" -ForegroundColor Green
Write-Host "Output: $outputPath" -ForegroundColor Cyan
Write-Host "Links: $(Join-Path $solutionRoot 'docs\FlowT.links')" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  - Review generated docs in: $outputPath" -ForegroundColor White
Write-Host "  - Commit changes to version control" -ForegroundColor White
Write-Host ""
Read-Host "Press Enter to exit"
