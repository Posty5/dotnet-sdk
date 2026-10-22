using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using Posty5.Core.Http;
using Posty5.HtmlHosting;
using Posty5.HtmlHosting.Models;
using Posty5.QRCode;
using Posty5.QRCode.Models;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Posty5.SocialPublisherPost;
using Posty5.SocialPublisherPost.Models;
using Xunit;

namespace Posty5.Tests.Unit;

/// <summary>
/// Offline: the created-at window (<c>fromDate</c> / <c>toDate</c>) on the four
/// list methods (redesign-dashboards SDK-02), pinned against a
/// <see cref="RecordingServer"/>. Both keys go out as <c>yyyy-MM-dd</c> in the
/// invariant culture when set, and neither when unset.
/// </summary>
public class ListDateFilterTests : IDisposable
{
    private static readonly DateTime From = new(2026, 10, 1);
    private static readonly DateTime To = new(2026, 10, 7);

    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    /// <summary>One list call per client, with or without the window.</summary>
    public static TheoryData<string> Clients => new() { "shortLink", "qrCode", "htmlHosting", "socialPost" };

    private Task ListAsync(string client, DateTime? from, DateTime? to)
    {
        Posty5HttpClient http = _server.Http();
        return client switch
        {
            "shortLink" => new ShortLinkClient(http).ListAsync(new ShortLinkListParamsModel { RefId = "R1", FromDate = from, ToDate = to }),
            "qrCode" => new QRCodeClient(http).ListAsync(new QRCodeListParamsModel { RefId = "R1", FromDate = from, ToDate = to }),
            "htmlHosting" => new HtmlHostingClient(http).ListAsync(new HtmlHostingListPagesParamsModel { RefId = "R1", FromDate = from, ToDate = to }),
            "socialPost" => new SocialPublisherPostClient(http).ListAsync(new ListPostsParams { RefId = "R1", FromDate = from, ToDate = to }),
            _ => throw new ArgumentOutOfRangeException(nameof(client), client, null),
        };
    }

    private NameValueCollection LastQuery() =>
        HttpUtility.ParseQueryString(new Uri(new Uri("http://localhost"), _server.Requests.Single().PathAndQuery).Query);

    [Theory]
    [MemberData(nameof(Clients))]
    public async Task ListAsync_SendsBothDates_AsYyyyMmDd(string client)
    {
        await ListAsync(client, From, To);

        var query = LastQuery();
        Assert.Equal("2026-10-01", query["fromDate"]);
        Assert.Equal("2026-10-07", query["toDate"]);
        Assert.Equal("R1", query["refId"]);
        Assert.Contains("fromDate=2026-10-01&toDate=2026-10-07", _server.Requests.Single().PathAndQuery);
    }

    [Theory]
    [MemberData(nameof(Clients))]
    public async Task ListAsync_SendsNeitherKey_WhenTheDatesAreUnset(string client)
    {
        await ListAsync(client, null, null);

        var query = LastQuery();
        Assert.Null(query["fromDate"]);
        Assert.Null(query["toDate"]);
        Assert.Equal("R1", query["refId"]);
    }

    [Theory]
    [MemberData(nameof(Clients))]
    public async Task ListAsync_DatesIgnoreTheCurrentCulture(string client)
    {
        var culture = CultureInfo.CurrentCulture;
        try
        {
            // ar-EG writes Arabic-Indic digits and its own date pattern by default.
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG");
            await ListAsync(client, From, To);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }

        var query = LastQuery();
        Assert.Equal("2026-10-01", query["fromDate"]);
        Assert.Equal("2026-10-07", query["toDate"]);
    }
}
