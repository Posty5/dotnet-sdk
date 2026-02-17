# PowerShell Script to Rename Model References in Client Files
# This script updates all client files to use the new renamed model classes

$ErrorActionPreference = "Stop"

# Define the SDK root path
$sdkRoot = "d:\posty5\Posty5-WEB\posty5-dotnet-sdk\src"

# Define replacements for each module
$replacements = @{
    "Posty5.SocialPublisherWorkspace" = @{
        "AccountDetails" = "SocialPublisherWorkspaceAccountDetailsModel"
        "WorkspaceAccount" = "SocialPublisherWorkspaceWorkspaceAccountModel"
        "WorkspaceModel" = "SocialPublisherWorkspaceWorkspaceModel"
        "WorkspaceSampleDetails" = "SocialPublisherWorkspaceWorkspaceSampleDetailsModel"
        "UploadImageConfig" = "SocialPublisherWorkspaceUploadImageConfigModel"
        "CreateWorkspaceResponse" = "SocialPublisherWorkspaceCreateWorkspaceResponseModel"
        "WorkspaceRequest" = "SocialPublisherWorkspaceWorkspaceRequestModel"
        "CreateWorkspaceRequest" = "SocialPublisherWorkspaceCreateWorkspaceRequestModel"
        "UpdateWorkspaceRequest" = "SocialPublisherWorkspaceUpdateWorkspaceRequestModel"
        "ListWorkspacesParams" = "SocialPublisherWorkspaceListWorkspacesParamsModel"
        "WorkspaceForNewPostModel" = "SocialPublisherWorkspaceWorkspaceForNewPostModel"
    }
}

function Update-ClientFile {
    param(
        [string]$FilePath,
        [hashtable]$Replacements
    )
    
    Write-Host "Processing: $FilePath"
    
    $content = Get-Content $FilePath -Raw
    $originalContent = $content
    
    foreach ($key in $Replacements.Keys) {
        $oldName = $key
        $newName = $Replacements[$key]
        
        # Replace in type declarations and generic parameters
        $content = $content -replace "([<,\s])$oldName([>,\s])", "`$1$newName`$2"
        $content = $content -replace "([<,\s])$oldName(\?)", "`$1$newName`$2"
        $content = $content -replace "Task<$oldName>", "Task<$newName>"
        $content = $content -replace "PaginationResponse<$oldName>", "PaginationResponse<$newName>"
    }
    
    if ($content -ne $originalContent) {
        Set-Content $FilePath -Value $content -NoNewline
        Write-Host "  ✓ Updated" -ForegroundColor Green
        return $true
    } else {
        Write-Host "  - No changes needed" -ForegroundColor Gray
        return $false
    }
}

# Process each module
foreach ($module in $replacements.Keys) {
    $modulePath = Join-Path $sdkRoot $module
    $clientFile = Get-ChildItem -Path $modulePath -Filter "*Client.cs" -Recurse | Select-Object -First 1
    
    if ($clientFile) {
        Write-Host "`nModule: $module" -ForegroundColor Cyan
        $updated = Update-ClientFile -FilePath $clientFile.FullName -Replacements $replacements[$module]
        
        if ($updated) {
            Write-Host "Building module..." -ForegroundColor Yellow
            Push-Location $modulePath
            $buildResult = dotnet build 2>&1
            Pop-Location
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✓ Build successful" -ForegroundColor Green
            } else {
                Write-Host "  ✗ Build failed" -ForegroundColor Red
                Write-Host $buildResult
            }
        }
    }
}

Write-Host "`nScript completed!" -ForegroundColor Cyan
