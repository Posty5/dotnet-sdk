using Posty5.Core.Configuration;
using System.Text.Json;
using Posty5.SocialPublisherPost;
using Posty5.SocialPublisherPost.Models;
using Posty5.SocialPublisherWorkspace;
using Posty5.SocialPublisherWorkspace.Models;
using Xunit;

namespace Posty5.Tests.Unit;

/// <summary>
/// Offline: workspace default comments and the post flag that turns them off, pinned
/// against a <see cref="RecordingServer"/>.
/// </summary>
public class WorkspaceDefaultCommentsTests : IDisposable
{
    private const string Base = "/api/social-publisher-workspace";
    private readonly RecordingServer _server = new();

    public void Dispose() => _server.Dispose();

    private (string Route, JsonElement Body) Last()
    {
        var request = _server.Requests.Last();
        using var json = JsonDocument.Parse(string.IsNullOrEmpty(request.Body) ? "{}" : request.Body);
        return ($"{request.Method} {request.PathAndQuery}", json.RootElement.Clone());
    }

    [Fact]
    public async Task SetDefaultCommentsAsync_ReplacesTheListUnderTheWorkspaceVersion()
    {
        _server.ResultJson = """[{ "_id": "d1", "text": "Hi", "postTypes": ["image"], "order": 0 }]""";
        var client = new SocialPublisherWorkspaceClient(_server.Http());

        var saved = await client.SetDefaultCommentsAsync("w1", new[]
        {
            new WorkspaceDefaultCommentModel { Text = "Hi", PostTypes = { CommentPostTypes.Image }, PostToInstagram = false },
        }, 3);

        var (route, body) = Last();
        Assert.Equal($"PUT {Base}/w1/default-comments", route);
        Assert.Equal(Posty5ConcurrencyConst.IfMatchValue(3), _server.Headers.Last()[Posty5ConcurrencyConst.IfMatchHeader]);
        var comment = body.GetProperty("comments")[0];
        Assert.Equal("Hi", comment.GetProperty("text").GetString());
        Assert.Equal("image", comment.GetProperty("postTypes")[0].GetString());
        Assert.False(comment.GetProperty("postToInstagram").GetBoolean());
        Assert.False(comment.TryGetProperty("_id", out _));
        Assert.Equal("d1", saved.Comments.Single().Id);
    }

    [Fact]
    public async Task ResolveDefaultCommentsAsync_AsksForOnePostType()
    {
        _server.ResultJson = """{ "comments": [{ "text": "Hi", "defaultCommentId": "d1", "defaultCommentSource": "workspace", "postToFacebook": true }], "warnings": [] }""";
        var client = new SocialPublisherWorkspaceClient(_server.Http());

        var resolved = await client.ResolveDefaultCommentsAsync("w1", CommentPostTypes.Text);

        Assert.Equal($"GET {Base}/w1/default-comments/resolved?postType=text", Last().Route);
        Assert.Equal("workspace", resolved.Comments.Single().DefaultCommentSource);
    }

    [Fact]
    public async Task CreateTextPost_SendsAllowPostDefaultCommentsOnlyWhenSet()
    {
        _server.ResultJson = """{ "_id": "p1" }""";
        var posts = new SocialPublisherPostClient(_server.Http());

        await posts.CreateTextPostToWorkspaceAsync(new CreateTextPostToWorkspaceRequest { WorkspaceId = "w1", Caption = "Hello", AllowPostDefaultComments = false });
        Assert.False(Last().Body.GetProperty("allowPostDefaultComments").GetBoolean());

        await posts.CreateTextPostToWorkspaceAsync(new CreateTextPostToWorkspaceRequest { WorkspaceId = "w1", Caption = "Hello" });
        Assert.False(Last().Body.TryGetProperty("allowPostDefaultComments", out _));
    }
}
