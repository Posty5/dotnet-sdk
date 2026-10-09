using Posty5.Core.Configuration;
using Posty5.Core.Http;
using Posty5.Core.Models;
using Posty5.ShortLink.Helpers;
using Posty5.ShortLink.Models;

namespace Posty5.ShortLink;

/// <summary>
/// Link campaigns: named groups of short links with optional default UTM (copied
/// into a link's empty UTM fields when it is saved with the campaign). Create and
/// update need the <c>urlShortener.campaigns</c> feature; a plan without it gets
/// the API's 403.
/// </summary>
public class LinkCampaignClient
{
    private readonly Posty5HttpClient _http;

    /// <summary>Creates a new link campaign client</summary>
    /// <param name="httpClient">HTTP client instance from Posty5.Core</param>
    public LinkCampaignClient(Posty5HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>List the caller's campaigns.</summary>
    public async Task<PaginationResponse<LinkCampaignModel>> ListAsync(
        LinkCampaignListParamsModel? listParams = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>();
        if (listParams?.Archived != null)
            query["archived"] = listParams.Archived.Value;
        if (!string.IsNullOrEmpty(listParams?.Term))
            query["term"] = listParams.Term;
        if (pagination != null)
        {
            if (!string.IsNullOrEmpty(pagination.Cursor))
                query["cursor"] = pagination.Cursor;
            query["pageSize"] = pagination.PageSize;
        }

        var response = await _http.GetAsync<PaginationResponse<LinkCampaignModel>>(
            ShortLinkConst.LinkCampaignBasePath, query, cancellationToken);
        return response.Result ?? new PaginationResponse<LinkCampaignModel>();
    }

    /// <summary>Get a campaign with <c>LinkCount</c> and <c>TotalVisits</c>.</summary>
    public async Task<LinkCampaignDetailsModel> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync<LinkCampaignDetailsModel>(
            $"{ShortLinkConst.LinkCampaignBasePath}/{id}", cancellationToken: cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Link campaign not found");
    }

    /// <summary>Create a campaign.</summary>
    public async Task<LinkCampaignModel> CreateAsync(
        LinkCampaignCreateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var data = new
        {
            request.Name,
            request.Description,
            request.Color,
            request.Utm,
            request.Archived,
            CreatedFrom = _http.ResolveCreatedFrom(CreatedFromDefaults.Package)
        };
        var response = await _http.PostAsync<LinkCampaignModel>(ShortLinkConst.LinkCampaignBasePath, data, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to create link campaign");
    }

    /// <summary>
    /// Update a campaign; a <c>null</c> property keeps the stored value.
    /// <paramref name="version"/> is the campaign's <see cref="LinkCampaignModel.Version"/> as last read.
    /// </summary>
    public async Task<LinkCampaignModel> UpdateAsync(
        string id,
        LinkCampaignUpdateRequestModel request,
        long version,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var data = new Dictionary<string, object?>();
        ShortLinkRequestBodyHelper.AddIfSet(data, "name", request.Name);
        ShortLinkRequestBodyHelper.AddIfSet(data, "description", request.Description);
        ShortLinkRequestBodyHelper.AddIfSet(data, "color", request.Color);
        if (request.ClearUtm)
            data["utm"] = null;
        else
            ShortLinkRequestBodyHelper.AddIfSet(data, "utm", request.Utm);
        ShortLinkRequestBodyHelper.AddIfSet(data, "archived", request.Archived);

        var response = await _http.PutAsync<LinkCampaignModel>($"{ShortLinkConst.LinkCampaignBasePath}/{id}", data, version, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update link campaign");
    }

    /// <summary>
    /// Delete a campaign. The API refuses while links use it unless
    /// <paramref name="detach"/> is <c>true</c>, which detaches the links first.
    /// </summary>
    /// <returns>The deleted campaign's id; <see cref="VersionedWriteResult.Version"/> is null after a delete.</returns>
    public async Task<VersionedWriteResult> DeleteAsync(string id, long version, bool detach = false, CancellationToken cancellationToken = default)
    {
        var path = $"{ShortLinkConst.LinkCampaignBasePath}/{id}" + (detach ? "?detach=true" : string.Empty);
        var response = await _http.DeleteAsync<object>(path, version, cancellationToken);
        return new VersionedWriteResult { Id = id, Message = response.Message };
    }
}
