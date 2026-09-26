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
using Posty5.Store;
using Posty5.Store.Models;

namespace Posty5.Examples;

/// <summary>
/// Comprehensive examples for using the Posty5 .NET SDK
/// </summary>
public class Examples
{
    public static async Post Main(string[] args)
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
        
        // Store dropshipping (suppliers) Examples
        await SuppliersExample(httpClient);
    }
    
    static async Post QRCodeExamples(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== QR Code Examples ===\n");
        
        var qrCodeClient = new QRCodeClient(httpClient);
        
        // Create a URL QR code
        var urlQr = await qrCodeClient.CreateUrlAsync(new CreateUrlQRCodeRequest
        {
            Name = "Website QR Code",
            QrCodeTarget = new UrlQRTarget { Url = "https://posty5.com" }
        });
        Console.WriteLine($"Created URL QR Code: {urlQr.QrCodeLandingPageURL}");
        
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
        Console.WriteLine($"Created WiFi QR Code: {wifiQr.QrCodeLandingPageURL}");
        
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
        Console.WriteLine($"Created Email QR Code: {emailQr.QrCodeLandingPageURL}");
        
        // List QR codes
        var qrCodes = await qrCodeClient.ListAsync(
            pagination: new PaginationParams { PageNumber = 0, PageSize = 10 }
        );
        Console.WriteLine($"Found {qrCodes.TotalCount} QR codes");
    }
    
    static async Post ShortLinkExamples(Posty5HttpClient httpClient)
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
    
    static async Post HtmlHostingExamples(Posty5HttpClient httpClient)
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
    
    static async Post SocialPublisherExamples(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== Social Publisher Examples ===\n");
        
        var workspaceClient = new WorkspaceClient(httpClient);
        var postClient = new PostClient(httpClient);
        
        // Create a workspace
        var workspace = await workspaceClient.CreateAsync(new CreateWorkspaceRequest
        {
            Name = "Marketing Team",
            Description = "Social media campaigns for Q1 2026"
        });
        Console.WriteLine($"Created Workspace: {workspace.Name}");
        
        // Create a scheduled post
        var post = await postClient.CreateAsync(new CreatePostRequest
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
        Console.WriteLine($"Created Post: {post.Title} (scheduled for {post.ScheduledAt})");
        
        // List posts in workspace
        var posts = await postClient.ListAsync(new ListPostsParams
        {
            WorkspaceId = workspace.Id,
            Status = PostStatus.Scheduled
        });
        Console.WriteLine($"Found {posts.TotalCount} scheduled posts");
        
        // Publish immediately
        await postClient.PublishAsync(post.Id!);
        Console.WriteLine("Post published immediately!");
    }
    
    /// <summary>
    /// Dropshipping on an online store: read the suppliers, browse and preview an
    /// import, then work the queue of supplier orders that need a person.
    /// Connecting a supplier is left to the cPanel (or the README walk-through):
    /// this example starts from a connection that already exists and never spends
    /// money — Preview charges nothing, and only a cost change is accepted.
    /// </summary>
    static async Task SuppliersExample(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== Store Suppliers (Dropshipping) Examples ===\n");
        
        var store = new StoreClient(httpClient);
        var storeId = Environment.GetEnvironmentVariable("POSTY5_STORE_ID") ?? "your-store-id";
        
        // The suppliers this store can connect, and the ones it has
        var catalogue = await store.Suppliers.GetCatalogueAsync(storeId);
        Console.WriteLine($"Suppliers offered: {string.Join(", ", catalogue!.Items!.Select(s => s.Key))}");
        
        var connections = await store.Suppliers.ListAsync(storeId);
        var connection = connections.FirstOrDefault(c => c.Enabled);
        if (connection == null)
        {
            Console.WriteLine("No supplier connected yet — connect one in the store's control panel first.");
            return;
        }
        Console.WriteLine($"Using {connection.SupplierKey} ({connection.Mode}), automation: {connection.Automation?.Mode}");
        
        // Browse the supplier's catalogue — paged by number, at most 48 a page
        var page = await store.Suppliers.BrowseProductsAsync(storeId, connection.Id!,
            new BrowseSupplierProductsParams { Q = "mug" }, page: 1, pageSize: 12);
        Console.WriteLine($"Found {page!.Total ?? page.Items!.Count} products matching \"mug\"");
        
        // Preview an import: prices, credits and duplicates — nothing is created or charged
        if (page.Items!.Count > 0)
        {
            var preview = await store.Suppliers.PreviewImportAsync(storeId, connection.Id!, new ImportSupplierProductsInput
            {
                Items = { new ImportSupplierProductItem { SupplierProductId = page.Items[0].SupplierProductId! } }
            });
            var row = preview!.Rows![0];
            Console.WriteLine(row.DuplicateOf == null
                ? $"Would import \"{row.Name}\" for {preview.Totals!.Credits} credits"
                : $"\"{row.Name}\" is already in the store");
        }
        
        // Supplier orders waiting on a person
        var queue = await store.Suppliers.ListSupplierOrdersAsync(storeId,
            new SupplierOrderSearchParams { NeedsReview = true }, page: 1, pageSize: 20);
        Console.WriteLine($"{queue!.Total} supplier orders need review");
        
        foreach (var supplierOrder in queue.Items!)
        {
            Console.WriteLine($"Order {supplierOrder.OrderNumber}: {supplierOrder.ReviewReason} — {supplierOrder.ReviewMessage}");
            
            // A price change at the supplier: accept it and send again (audited with the caller)
            if (supplierOrder.ReviewReason == SupplierReviewReasons.CostChanged)
            {
                try
                {
                    var result = await store.Suppliers.RetryAsync(storeId, supplierOrder.Id!, acceptCost: true);
                    Console.WriteLine($"  Retried: {result!.Status}");
                }
                catch (Posty5.Core.Exceptions.Posty5ValidationException paused)
                {
                    // A pause is answered as a 400 — the message names the reason
                    Console.WriteLine($"  Still paused: {paused.Message}");
                }
            }
        }
        
        // The parts of one order, with their tracking once shipped
        var firstOrderId = queue.Items!.FirstOrDefault()?.OrderId;
        if (firstOrderId != null)
        {
            var order = await store.Orders.GetAsync(storeId, firstOrderId);
            foreach (var part in order!.FulfilmentGroups ?? new())
            {
                Console.WriteLine($"  {part.Label}: {part.Status} {part.Shipment?.TrackingNumber}");
            }
        }
    }
}
