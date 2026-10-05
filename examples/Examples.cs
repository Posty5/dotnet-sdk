using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.Core.Models;
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

        // Account, discovery, text post and story Examples
        await AccountAndDiscoveryExample(httpClient);
    }
    
    static async Task QRCodeExamples(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== QR Code Examples ===\n");

        var qrCodeClient = new QRCodeClient(httpClient);
        const string templateId = "your-template-id"; // required for API-key calls

        // Create a URL QR code
        var urlQr = await qrCodeClient.CreateURLAsync(new QRCodeCreateURLRequestModel
        {
            Name = "Website QR Code",
            TemplateId = templateId,
            Url = new QRCodeUrlTargetModel { Url = "https://posty5.com" }
        });
        Console.WriteLine($"Created URL QR Code: {urlQr.QrCodeLandingPageURL}");

        // Create a WiFi QR code (the API builds and escapes the encoded text)
        var wifiQr = await qrCodeClient.CreateWifiAsync(new QRCodeCreateWifiRequestModel
        {
            Name = "Office WiFi",
            TemplateId = templateId,
            Wifi = new QRCodeWifiTargetModel
            {
                Name = "OfficeNetwork",
                Password = "SecurePassword123",
                AuthenticationType = "WPA"
            }
        });
        Console.WriteLine($"Created WiFi QR Code: {wifiQr.QrCodeLandingPageURL}");

        // Create an Email QR code
        var emailQr = await qrCodeClient.CreateEmailAsync(new QRCodeCreateEmailRequestModel
        {
            Name = "Contact Email",
            TemplateId = templateId,
            Email = new QRCodeEmailTargetModel
            {
                Email = "contact@example.com",
                Subject = "Hello",
                Body = "I'd like to get in touch"
            }
        });
        Console.WriteLine($"Created Email QR Code: {emailQr.QrCodeLandingPageURL}");

        // List QR codes (cursor pagination)
        var qrCodes = await qrCodeClient.ListAsync(
            pagination: new PaginationParams { PageSize = 10 }
        );
        Console.WriteLine($"Found {qrCodes.Items.Count} QR codes on this page");
    }

    static async Task ShortLinkExamples(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== Short Link Examples ===\n");

        var shortLinkClient = new ShortLinkClient(httpClient);
        const string templateId = "your-template-id"; // required for API-key calls

        // Create a short link
        var shortLink = await shortLinkClient.CreateAsync(new ShortLinkCreateRequestModel
        {
            Name = "Marketing Campaign",
            BaseUrl = "https://example.com/very-long-url-with-parameters?utm_source=campaign",
            TemplateId = templateId,
            CustomLandingId = "summer-sale", // Starter plan and above
            RefId = "CAMPAIGN-001",
            Tag = "campaign"
        });
        Console.WriteLine($"Created Short Link: {shortLink.ShorterLink}");

        // Get short link details
        var details = await shortLinkClient.GetAsync(shortLink.Id!);
        Console.WriteLine($"Visits: {details.NumberOfVisitors}");

        // Update short link (BaseUrl and TemplateId are required on every update)
        await shortLinkClient.UpdateAsync(shortLink.Id!, new ShortLinkUpdateRequestModel
        {
            Name = "Updated Campaign Name",
            BaseUrl = details.BaseUrl!,
            TemplateId = templateId
        });
        Console.WriteLine("Updated short link");

        // List short links by reference ID
        var shortLinks = await shortLinkClient.ListAsync(
            new ShortLinkListParamsModel { RefId = "CAMPAIGN-001" }
        );
        Console.WriteLine($"Found {shortLinks.Items.Count} short links on this page");
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
        // (cursor-paged: pass Pagination.NextCursor back while Pagination.HasMore)
        var queue = await store.Suppliers.ListSupplierOrdersAsync(storeId,
            new SupplierOrderSearchParams { NeedsReview = true }, new PaginationParams { PageSize = 20 });
        Console.WriteLine($"{queue!.Items.Count} supplier orders need review on this page" +
            (queue.Pagination.HasMore ? " (more follow)" : ""));
        
        foreach (var supplierOrder in queue.Items)
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

    /// <summary>
    /// Who the key is, what it can spend and what things cost; then find the ids
    /// the other calls take — stores, connected accounts, QR templates — and
    /// publish a text post and a story by URL. Reads are free; the two posts
    /// are charged at the price GetOperationCostsAsync reports.
    /// </summary>
    static async Task AccountAndDiscoveryExample(Posty5HttpClient httpClient)
    {
        Console.WriteLine("\n=== Account, discovery, text post and story ===\n");

        // Who am I, and what can I spend? (an unknown or revoked key throws Posty5AuthenticationException)
        var account = new Posty5.Account.AccountClient(httpClient);
        var me = await account.GetCurrentAsync();
        Console.WriteLine($"{me.User.UserName} on {me.Plan?.Name ?? "no plan"}: {me.Credits.Spendable} credits spendable");

        // Prices change — read them rather than hard-coding them
        var costs = await account.GetOperationCostsAsync();
        var textPostCost = costs.Modules.SelectMany(m => m.Operations)
            .FirstOrDefault(o => o.FeaturePath == "socialMediaPublisher.textPost");
        Console.WriteLine($"A text post costs {textPostCost?.Cost} {costs.Currency}");

        // The ids the other calls take
        var stores = await new StoreClient(httpClient).ListStoresAsync();
        Console.WriteLine($"Stores: {string.Join(", ", stores.Select(s => $"{s.Name} ({s.Id})"))}");

        var templates = await new QRCodeTemplateClient(httpClient).ListPublicTemplatesAsync();
        Console.WriteLine($"First public QR template: {templates.Items.FirstOrDefault()?.Name}");

        var accounts = await new Posty5.SocialPublisherWorkspace.SocialPublisherAccountClient(httpClient)
            .ListAsync(new Posty5.SocialPublisherWorkspace.Models.SocialPublisherAccountListParamsModel { Platform = "facebook", Status = "active" });
        var page = accounts.Items.FirstOrDefault();
        if (page == null)
        {
            Console.WriteLine("No active Facebook Page connected — connect one in the dashboard first.");
            return;
        }

        // A text post, then a story, to that Page
        var posts = new Posty5.SocialPublisherPost.SocialPublisherPostClient(httpClient);
        var text = await posts.CreateTextPostToAccountAsync(new Posty5.SocialPublisherPost.Models.CreateTextPostToAccountRequest
        {
            AccountId = page.Id,
            Caption = "Our autumn menu is out.",
            Facebook = new Posty5.SocialPublisherPost.Models.TextPostFacebookConfig
            {
                Description = "Our autumn menu is out. Take a look:",
                Link = "https://example.com/menu"
            }
        });
        Console.WriteLine($"Text post {text.Id} queued");

        var story = await posts.CreateStoryPostToAccountAsync(new Posty5.SocialPublisherPost.Models.CreateStoryPostToAccountRequest
        {
            AccountId = page.Id,
            Kind = Posty5.SocialPublisherPost.Models.StoryKinds.Image,
            Image = new Posty5.SocialPublisherPost.Models.ImageRequest
            {
                Source = Posty5.SocialPublisherPost.Models.ImageSource.ImageUrl,
                ExternalUrl = "https://example.com/menu-story.jpg"
            }
        });

        // Status lives at /{id}/status (fixed in Posty5.SocialPublisherPost 4.6.0)
        var status = await posts.GetStatusAsync(story.Id);
        Console.WriteLine($"Story {story.Id}: {status.CurrentStatus}, expires {status.StoryExpiresAt}");
    }
}
