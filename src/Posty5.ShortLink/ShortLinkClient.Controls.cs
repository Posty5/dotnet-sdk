using Posty5.ShortLink.Helpers;
using Posty5.ShortLink.Models;

namespace Posty5.ShortLink;

/// <summary>Short link controls: tags, destination health checks and rules.</summary>
public partial class ShortLinkClient
{
    /// <summary>
    /// The caller's distinct tags (an API key: its own links' tags), sorted
    /// (<c>GET /api/short-link/tags</c>).
    /// </summary>
    /// <param name="term">Optional case-insensitive prefix</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Up to 200 tags</returns>
    public async Task<List<string>> ListTagsAsync(string? term = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(term))
            query["term"] = term;

        var response = await _http.GetAsync<List<string>>(
            $"{ShortLinkConst.BasePath}{ShortLinkConst.TagsPath}", query, cancellationToken);
        return response.Result ?? new List<string>();
    }

    /// <summary>
    /// Queue one destination health check of a link (<c>POST /api/short-link/{id}/health-check</c>, 202).
    /// Limited to one per link per 10 minutes. Feature key <c>healthMonitor</c>.
    /// </summary>
    /// <param name="id">Short link ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CheckHealthAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        await _http.PostAsync<object>(
            $"{ShortLinkConst.BasePath}/{id}{ShortLinkConst.HealthCheckPath}", new { }, cancellationToken);
    }

    /// <summary>
    /// Set a link's rules (access, routing, variants, UTM, pixels) through the update route.
    /// Partial: a <c>null</c> section is untouched; an empty list or a <c>Clear</c> flag clears it.
    /// The link is read first only when <see cref="LinkRulesUpdateModel.BaseUrl"/> or
    /// <see cref="LinkRulesUpdateModel.TemplateId"/> is not set, since the update requires them.
    /// </summary>
    /// <param name="id">Short link ID</param>
    /// <param name="rules">The sections to set</param>
    /// <param name="version">The link's version as last read, sent as <c>If-Match</c></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated short link</returns>
    public async Task<ShortLinkModel> SetRulesAsync(
        string id,
        LinkRulesUpdateModel rules,
        long version,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(rules);

        var baseUrl = rules.BaseUrl;
        var templateId = rules.TemplateId;
        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(templateId))
        {
            var stored = await GetAsync(id, cancellationToken);
            if (string.IsNullOrEmpty(baseUrl)) baseUrl = stored.BaseUrl ?? string.Empty;
            if (string.IsNullOrEmpty(templateId)) templateId = stored.TemplateId ?? stored.Template?.Id ?? string.Empty;
        }

        var data = new Dictionary<string, object?>
        {
            ["baseUrl"] = baseUrl,
            ["templateId"] = templateId,
        };
        ShortLinkRequestBodyHelper.AddRuleSections(data, rules);
        data["templateType"] = ShortLinkConst.TemplateType;

        var response = await _http.PutAsync<ShortLinkModel>($"{ShortLinkConst.BasePath}/{id}", data, version, cancellationToken);
        return response.Result ?? throw new InvalidOperationException("Failed to update short link rules");
    }
}
