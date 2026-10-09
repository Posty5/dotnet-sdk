using Posty5.Core.Exceptions;
using Posty5.ShortLink;
using Posty5.ShortLink.Models;
using Posty5.Store;
using Posty5.Store.Models;
using Xunit;

namespace Posty5.Tests;

/// <summary>
/// Live optimistic concurrency against the configured API: a write with the
/// version just read succeeds and returns the next one; the same write replayed
/// with the now-stale version is refused with <see cref="Posty5ConflictException"/>
/// carrying the stored <see cref="Posty5ConflictException.CurrentVersion"/>.
/// Reported as skipped when the fixture variables are not set.
/// </summary>
[Collection("Sequential")]
public class VersionedWritesLiveTests : IDisposable
{
    private readonly ShortLinkClient _shortLinks = new(TestConfig.CreateHttpClient());
    private readonly StoreClient _store = new(TestConfig.CreateHttpClient());
    private string? _linkId;

    /// <summary>Removes the short link a fact created, with its current version.</summary>
    public void Dispose()
    {
        if (_linkId is null) return;
        try
        {
            var current = _shortLinks.GetAsync(_linkId).GetAwaiter().GetResult();
            _shortLinks.DeleteAsync(_linkId, current.Version).GetAwaiter().GetResult();
        }
        catch { /* already gone */ }
    }

    [ApiKeyFact]
    public async Task ShortLink_CurrentVersionSucceeds_StaleVersionConflicts()
    {
        var created = await _shortLinks.CreateAsync(new ShortLinkCreateRequestModel
        {
            Name = $"Versioned write {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            BaseUrl = "https://posty5.com",
            TemplateId = TestConfig.TemplateId
        });
        _linkId = created.Id!;

        var read = await _shortLinks.GetAsync(_linkId);
        var update = new ShortLinkUpdateRequestModel
        {
            Name = read.Name + " (edited)",
            BaseUrl = read.BaseUrl!,
            TemplateId = TestConfig.TemplateId
        };

        // Current version: applied, and the version moves on.
        var updated = await _shortLinks.UpdateAsync(_linkId, update, read.Version);
        Assert.True(updated.Version > read.Version, $"expected a version after {read.Version}, got {updated.Version}");

        // The version read before that write is now stale.
        var conflict = await Assert.ThrowsAsync<Posty5ConflictException>(
            () => _shortLinks.UpdateAsync(_linkId, update, read.Version));
        Assert.Equal(409, conflict.StatusCode);
        Assert.Equal(updated.Version, conflict.CurrentVersion);

        // A stale delete is refused too; the current one goes through.
        await Assert.ThrowsAsync<Posty5ConflictException>(() => _shortLinks.DeleteAsync(_linkId, read.Version));
        await _shortLinks.DeleteAsync(_linkId, updated.Version);
        _linkId = null;
    }

    /// <summary>
    /// A section PATCH on the fixture product. It writes the price section back
    /// unchanged, so the product's data is left as it was; only its version moves.
    /// </summary>
    [StoreFixtureFact(TestConfig.ProductIdVar)]
    public async Task StoreProduct_SectionPatch_CurrentVersionSucceeds_StaleVersionConflicts()
    {
        var product = await _store.Products.GetAsync(TestConfig.StoreId, TestConfig.ProductId);
        Assert.NotNull(product);
        var samePrice = new ProductPriceInput { Price = product!.Price, CompareAtPrice = product.CompareAtPrice };

        var written = await _store.Products.UpdatePriceAsync(TestConfig.StoreId, TestConfig.ProductId, samePrice, product.Version);
        Assert.NotNull(written);
        Assert.True(written!.Version > product.Version, $"expected a version after {product.Version}, got {written.Version}");

        var conflict = await Assert.ThrowsAsync<Posty5ConflictException>(
            () => _store.Products.UpdatePriceAsync(TestConfig.StoreId, TestConfig.ProductId, samePrice, product.Version));
        Assert.Equal(written.Version, conflict.CurrentVersion);
        Assert.Equal(TestConfig.ProductId, conflict.ResourceId);
    }
}
