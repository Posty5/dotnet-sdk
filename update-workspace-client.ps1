# Comprehensive Model Renaming Script for .NET SDK
# This script renames ALL model classes across all modules

$ErrorActionPreference = "Stop"
$sdkRoot = "d:\posty5\Posty5-WEB\posty5-dotnet-sdk\src"

Write-Host "=== .NET SDK Model Renaming Script ===" -ForegroundColor Cyan
Write-Host ""

# Function to replace text in file
function Replace-InFile {
    param(
        [string]$FilePath,
        [string]$OldText,
        [string]$NewText
    )
    
    $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { return $false }
    
    if ($content -match [regex]::Escape($OldText)) {
        $content = $content -replace [regex]::Escape($OldText), $NewText
        Set-Content $FilePath -Value $content -NoNewline
        return $true
    }
    return $false
}

# SocialPublisherWorkspace Client Updates
Write-Host "Updating SocialPublisherWorkspace Client..." -ForegroundColor Yellow
$clientFile = "$sdkRoot\Posty5.SocialPublisherWorkspace\SocialPublisherWorkspaceClient.cs"

$replacements = @(
    @("PaginationResponse<WorkspaceSampleDetails>", "PaginationResponse<SocialPublisherWorkspaceWorkspaceSampleDetailsModel>"),
    @("ListWorkspacesParams", "SocialPublisherWorkspaceListWorkspacesParamsModel"),
    @("Task<WorkspaceModel>", "Task<SocialPublisherWorkspaceWorkspaceModel>"),
    @("GetAsync<WorkspaceModel>", "GetAsync<SocialPublisherWorkspaceWorkspaceModel>"),
    @("Task<WorkspaceForNewTaskModel>", "Task<SocialPublisherWorkspaceWorkspaceForNewTaskModel>"),
    @("GetAsync<WorkspaceForNewTaskModel>", "GetAsync<SocialPublisherWorkspaceWorkspaceForNewTaskModel>"),
    @("CreateWorkspaceRequest", "SocialPublisherWorkspaceCreateWorkspaceRequestModel"),
    @("UpdateWorkspaceRequest", "SocialPublisherWorkspaceUpdateWorkspaceRequestModel"),
    @("PostAsync<CreateWorkspaceResponse>", "PostAsync<SocialPublisherWorkspaceCreateWorkspaceResponseModel>"),
    @("PutAsync<CreateWorkspaceResponse>", "PutAsync<SocialPublisherWorkspaceCreateWorkspaceResponseModel>")
)

foreach ($pair in $replacements) {
    $updated = Replace-InFile -FilePath $clientFile -OldText $pair[0] -NewText $pair[1]
    if ($updated) {
        Write-Host "  ✓ Replaced: $($pair[0])" -ForegroundColor Green
    }
}

# Build the module
Write-Host "Building SocialPublisherWorkspace..." -ForegroundColor Yellow
Push-Location "$sdkRoot\Posty5.SocialPublisherWorkspace"
$result = dotnet build 2>&1
Pop-Location

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Build successful!" -ForegroundColor Green
} else {
    Write-Host "  ✗ Build failed!" -ForegroundColor Red
    Write-Host $result
}

Write-Host ""
Write-Host "Script completed!" -ForegroundColor Cyan
