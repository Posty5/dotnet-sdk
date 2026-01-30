$testFiles = Get-ChildItem -Path "tests\Posty5.Tests" -Filter "*Tests.cs"

foreach ($file in $testFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # HtmlHosting
    $content = $content -replace 'CreateHtmlPageFileRequest', 'HtmlHostingCreatePageFileRequestModel'
    $content = $content -replace 'CreateHtmlPageGithubRequest', 'HtmlHostingCreatePageGithubRequestModel'
    $content = $content -replace 'UpdateHtmlPageFileRequest', 'HtmlHostingUpdatePageFileRequestModel'
    $content = $content -replace 'UpdateHtmlPageGithubRequest', 'HtmlHostingUpdatePageGithubRequestModel'
    $content = $content -replace 'ListHtmlPagesParams', 'HtmlHostingListParamsModel'
    $content = $content -replace '\bHtmlPageModel\b', 'HtmlHostingPageModel'
    
    # HtmlHostingFormSubmission
    $content = $content -replace 'ListFormSubmissionsParams', 'HtmlHostingFormSubmissionListParamsModel'
    $content = $content -replace '\bChangeStatusRequest\b', 'HtmlHostingFormSubmissionChangeStatusRequestModel'
    $content = $content -replace '\bFormSubmissionModel\b', 'HtmlHostingFormSubmissionModel'
    
    # QRCode
    $content = $content -replace 'CreateFreeTextQRCodeRequest', 'QRCodeCreateFreeTextRequestModel'
    $content = $content -replace 'UpdateFreeTextQRCodeRequest', 'QRCodeUpdateFreeTextRequestModel'
    $content = $content -replace 'CreateEmailQRCodeRequest', 'QRCodeCreateEmailRequestModel'
    $content = $content -replace 'UpdateEmailQRCodeRequest', 'QRCodeUpdateEmailRequestModel'
    $content = $content -replace 'CreateWifiQRCodeRequest', 'QRCodeCreateWifiRequestModel'
    $content = $content -replace 'UpdateWifiQRCodeRequest', 'QRCodeUpdateWifiRequestModel'
    $content = $content -replace 'CreateURLQRCodeRequest', 'QRCodeCreateURLRequestModel'
    $content = $content -replace 'UpdateURLQRCodeRequest', 'QRCodeUpdateURLRequestModel'
    $content = $content -replace 'CreateCallQRCodeRequest', 'QRCodeCreateCallRequestModel'
    $content = $content -replace 'UpdateCallQRCodeRequest', 'QRCodeUpdateCallRequestModel'
    $content = $content -replace 'CreateSMSQRCodeRequest', 'QRCodeCreateSMSRequestModel'
    $content = $content -replace 'UpdateSMSQRCodeRequest', 'QRCodeUpdateSMSRequestModel'
    $content = $content -replace 'CreateGeolocationQRCodeRequest', 'QRCodeCreateGeolocationRequestModel'
    $content = $content -replace 'UpdateGeolocationQRCodeRequest', 'QRCodeUpdateGeolocationRequestModel'
    $content = $content -replace 'ListQRCodesParams', 'QRCodeListParamsModel'
    $content = $content -replace '\bQRCodeFreeTextTarget\b', 'QRCodeFreeTextTargetModel'
    $content = $content -replace '\bQRCodeEmailTarget\b', 'QRCodeEmailTargetModel'
    $content = $content -replace '\bQRCodeWifiTarget\b', 'QRCodeWifiTargetModel'
    $content = $content -replace '\bQRCodeUrlTarget\b', 'QRCodeUrlTargetModel'
    $content = $content -replace '\bQRCodeCallTarget\b', 'QRCodeCallTargetModel'
    $content = $content -replace '\bQRCodeSmsTarget\b', 'QRCodeSmsTargetModel'
    $content = $content -replace '\bQRCodeGeolocationTarget\b', 'QRCodeGeolocationTargetModel'
    
    # ShortLink
    $content = $content -replace '\bCreateShortLinkRequest\b', 'ShortLinkCreateRequestModel'
    $content = $content -replace '\bUpdateShortLinkRequest\b', 'ShortLinkUpdateRequestModel'
    $content = $content -replace 'ListShortLinksParams', 'ShortLinkListParamsModel'
    
    # SocialPublisherWorkspace
    $content = $content -replace '\bCreateWorkspaceRequest\b', 'SocialPublisherWorkspaceCreateRequestModel'
    $content = $content -replace '\bUpdateWorkspaceRequest\b', 'SocialPublisherWorkspaceUpdateRequestModel'
    $content = $content -replace 'ListWorkspacesParams', 'SocialPublisherWorkspaceListParamsModel'
    $content = $content -replace '\bWorkspaceModel\b', 'SocialPublisherWorkspaceModel'
    
    # HtmlHostingVariables
    $content = $content -replace 'CreateHtmlHostingVariableRequest', 'HtmlHostingVariablesCreateRequestModel'
    $content = $content -replace 'ListHtmlHostingVariablesParams', 'HtmlHostingVariablesListParamsModel'
    $content = $content -replace '\bHtmlHostingVariableModel\b', 'HtmlHostingVariablesVariableModel'
    
    # HtmlHosting - Additional models
    $content = $content -replace '\bGithubInfoModel\b', 'HtmlHostingGithubInfoModel'
    $content = $content -replace '\bHtmlPageLookupItem\b', 'HtmlHostingPageLookupItemModel'
    $content = $content -replace '\bFormLookupItem\b', 'HtmlHostingFormLookupItemModel'
    
    # HtmlHostingFormSubmission - Additional
    $content = $content -replace '\bFormStatusType\b', 'HtmlHostingFormSubmissionFormStatusType'
    
    # SocialPublisherTask - Additional
    $content = $content -replace '\bSocialPublisherTaskStatus\b', 'SocialPublisherTaskStatusType'
    
    Set-Content $file.FullName $content -NoNewline
}

Write-Host "Test files updated successfully"
