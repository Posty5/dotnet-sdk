using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.QRCode;
using Posty5.QRCode.Models;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Posty5.HtmlHosting;
using Posty5.HtmlHosting.Models;
using Posty5.SocialPublisher;
using Posty5.SocialPublisher.Models;

namespace Posty5.Examples;

/// <summary>
/// Comprehensive examples for using the Posty5 .NET SDK
/// </summary>
public class Examples
{
    public static async Task Main(string[] args)
    {
        // Initialize the HTTP client
        var options = new Posty5Options
        {
            BaseUrl = "https://api.posty5.com",
            ApiKey = Environment.GetEnvironmentVariable("POSTY5_API_KEY") ?? "your-api-key",
            Debug = true
        };
        
        var httpClient = new Posty5HttpClient(options);
        
        // QR Code Examples
        await QRCodeExamples(httpClient);
        
        // Short Link Examples
        await ShortLinkExamples(httpClient);
        
        // HTML Hosting Examples
        await HtmlHostingExamples(httpClient);
        
        // Social Publisher Examples
        await SocialPublisherExamples(httpClient);
    }
    
    static async Task QRCodeExamples(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== QR Code Examples ===\n");
        
        var qrCodeClient = new QRCodeClient(httpClient);
        
        // Create a URL QR code
        var urlQr = await qrCodeClient.CreateUrlAsync(new CreateUrlQRCodeRequest
        {
            Name = "Website QR Code",
            QrCodeTarget = new UrlQRTarget { Url = "https://posty5.com" }
        });
        Console.WriteLine($"Created URL QR Code: {urlQr.QrCodeLandingPage}");
        
        // Create a WiFi QR code
        var wifiQr = await qrCodeClient.CreateWifiAsync(new CreateWifiQRCodeRequest
        {
            Name = "Office WiFi",
            QrCodeTarget = new WifiQRTarget
            {
                Ssid = "OfficeNetwork",
                Password = "SecurePassword123",
                SecurityType = "WPA",
                Hidden = false
            }
        });
        Console.WriteLine($"Created WiFi QR Code: {wifiQr.QrCodeLandingPage}");
        
        // Create an Email QR code
        var emailQr = await qrCodeClient.CreateEmailAsync(new CreateEmailQRCodeRequest
        {
            Name = "Contact Email",
            QrCodeTarget = new EmailQRTarget
            {
                Email = "contact@example.com",
                Subject = "Hello",
                Body = "I'd like to get in touch"
            }
        });
        Console.WriteLine($"Created Email QR Code: {emailQr.QrCodeLandingPage}");
        
        // List QR codes
        var qrCodes = await qrCodeClient.ListAsync(
            pagination: new PaginationParams { PageNumber = 0, PageSize = 10 }
        );
        Console.WriteLine($"Found {qrCodes.TotalCount} QR codes");
    }
    
    static async Task ShortLinkExamples(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== Short Link Examples ===\n");
        
        var shortLinkClient = new ShortLinkClient(httpClient);
        
        // Create a short link
        var shortLink = await shortLinkClient.CreateAsync(new CreateShortLinkRequest
        {
            Name = "Marketing Campaign",
            TargetUrl = "https://example.com/very-long-url-with-parameters?utm_source=campaign",
            CustomSlug = "summer-sale"
        });
        Console.WriteLine($"Created Short Link: {shortLink.ShortUrl}");
        
        // Get short link details
        var details = await shortLinkClient.GetAsync(shortLink.Id!);
        Console.WriteLine($"Clicks: {details.ClickCount}");
        
        // Update short link
        await shortLinkClient.UpdateAsync(shortLink.Id!, new UpdateShortLinkRequest
        {
            Name = "Updated Campaign Name"
        });
        Console.WriteLine("Updated short link");
        
        // List short links
        var shortLinks = await shortLinkClient.ListAsync(
            new ListShortLinksParams { Search = "campaign" }
        );
        Console.WriteLine($"Found {shortLinks.TotalCount} short links");
    }
    
    static async Task HtmlHostingExamples(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== HTML Hosting Examples ===\n");
        
        var htmlHostingClient = new HtmlHostingClient(httpClient);
        
        // Create an HTML page
        var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>My Landing Page</title>
    <style>
        body { font-family: Arial; text-align: center; padding: 50px; }
        h1 { color: #333; }
    </style>
</head>
<body>
    <h1>Welcome to My Page</h1>
    <p>This is hosted via Posty5!</p>
</body>
</html>";
        
        var page = await htmlHostingClient.CreateAsync(new CreateHtmlHostingRequest
        {
            Name = "Landing Page",
            HtmlContent = htmlContent
        });
        Console.WriteLine($"Created HTML Page: {page.PublicUrl}");
        
        // Update the page
        await htmlHostingClient.UpdateAsync(page.Id!, new UpdateHtmlHostingRequest
        {
            HtmlContent = htmlContent.Replace("Welcome", "Hello")
        });
        Console.WriteLine("Updated HTML content");
        
        // List pages
        var pages = await htmlHostingClient.ListAsync();
        Console.WriteLine($"Found {pages.TotalCount} HTML pages");
    }
    
    static async Task SocialPublisherExamples(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== Social Publisher Examples ===\n");
        
        var workspaceClient = new WorkspaceClient(httpClient);
        var taskClient = new TaskClient(httpClient);
        
        // Create a workspace
        var workspace = await workspaceClient.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Marketing Team",
            Description = "Social media campaigns for Q1 2026"
        });
        Console.WriteLine($"Created Workspace: {workspace.Name}");
        
        // Create a scheduled task
        var task = await taskClient.CreateAsync(new CreateTaskRequest
        {
            WorkspaceId = workspace.Id!,
            Title = "Product Launch Announcement",
            Content = "🚀 Exciting news! Our new product is launching today! Check it out at our website.",
            Platforms = new List<SocialPlatform> 
            { 
                SocialPlatform.Facebook, 
                SocialPlatform.Twitter,
                SocialPlatform.LinkedIn
            },
            ScheduledAt = DateTime.UtcNow.AddHours(3)
        });
        Console.WriteLine($"Created Task: {task.Title} (scheduled for {task.ScheduledAt})");
        
        // List tasks in workspace
        var tasks = await taskClient.ListAsync(new ListTasksParams
        {
            WorkspaceId = workspace.Id,
            Status = TaskStatus.Scheduled
        });
        Console.WriteLine($"Found {tasks.TotalCount} scheduled tasks");
        
        // Publish immediately
        await taskClient.PublishAsync(task.Id!);
        Console.WriteLine("Task published immediately!");
    }
}
