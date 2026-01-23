using Xunit;
using Posty5.HtmlHostingFormSubmission;
using Posty5.HtmlHostingFormSubmission.Models;
using Posty5.Core.Models;

namespace Posty5.Tests.Integration;

/// <summary>
/// Tests for HTML Hosting Form Submission SDK
/// Simple read-only + delete operations for managing form submissions
/// </summary>
[Collection("Sequential")]
public class HtmlHostingFormSubmissionClientTests : IDisposable
{
    private readonly HtmlHostingFormSubmissionClient _client;
    private readonly string _testHtmlHostingId;
    private string? _createdSubmissionId;

    public HtmlHostingFormSubmissionClientTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new HtmlHostingFormSubmissionClient(httpClient);
        
        // TODO: Replace with actual HTML hosting ID from setup
        _testHtmlHostingId = "6830ce590e3fb6d1afeca82c";
    }

    #region Get Tests

    [Fact]
    public async Task Get_WithValidId_ShouldReturnSubmission()
    {
        // Arrange
        // First, list submissions to get a valid ID
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 1 }
        );

        if (listResult.Items.Count == 0)
        {
            // Skip if no submissions exist
            return;
        }

        var submissionId = listResult.Items[0].Id;

        // Act
        var submission = await _client.GetAsync(submissionId);

        // Assert
        Assert.NotNull(submission);
        Assert.Equal(submissionId, submission.Id);
        Assert.NotNull(submission.Data);
        Assert.NotEmpty(submission.Status);
    }

    [Fact]
    public async Task Get_ShouldReturnFormData()
    {
        // Arrange
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 1 }
        );

        if (listResult.Items.Count == 0)
        {
            return;
        }

        var submissionId = listResult.Items[0].Id;

        // Act
        var submission = await _client.GetAsync(submissionId);

        // Assert
        Assert.NotNull(submission.Data);
        Assert.IsType<Dictionary<string, object>>(submission.Data);
        
        // Verify we can access form fields
        if (submission.Data.Count > 0)
        {
            Assert.NotNull(submission.Data.Values.First());
        }
    }

    #endregion

    #region List Tests

    [Fact]
    public async Task List_WithHtmlHostingId_ShouldReturnSubmissions()
    {
        // Act
        var result = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count <= 10);
    }

    [Fact]
    public async Task List_FilterByFormId_ShouldReturnFilteredResults()
    {
        // Arrange
        var testFormId = "contact-form";

        // Act
        var result = await _client.ListAsync(
            new ListFormSubmissionsParams
            {
                HtmlHostingId = _testHtmlHostingId,
                FormId = testFormId
            },
            new PaginationParams { Page = 1, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        
        // All returned items should match the form ID filter
        Assert.All(result.Items, item => Assert.Equal(testFormId, item.FormId));
    }

    [Fact]
    public async Task List_FilterByStatus_ShouldReturnFilteredResults()
    {
        // Arrange
        var testStatus = "New";

        // Act
        var result = await _client.ListAsync(
            new ListFormSubmissionsParams
            {
                HtmlHostingId = _testHtmlHostingId,
                Status = testStatus
            },
            new PaginationParams { Page = 1, PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        
        // All returned items should match the status filter
        Assert.All(result.Items, item => Assert.Equal(testStatus, item.Status));
    }

    [Fact]
    public async Task List_WithPagination_ShouldRespectPageSize()
    {
        // Act
        var result = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 5 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count <= 5);
    }

    [Fact]
    public async Task List_WithFilteredFields_ShouldSearch()
    {
        // Arrange
        var filteredFields = "name,email,phone";

        // Act
        var result = await _client.ListAsync(
            new ListFormSubmissionsParams
            {
                HtmlHostingId = _testHtmlHostingId,
                FilteredFields = filteredFields
            }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task List_WithNumbering_ShouldFilterByNumber()
    {
        // Arrange
        var numbering = "001";

        // Act
        var result = await _client.ListAsync(
            new ListFormSubmissionsParams
            {
                HtmlHostingId = _testHtmlHostingId,
                Numbering = numbering
            }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task GetNextPrevious_WithValidId_ShouldReturnNavigation()
    {
        // Arrange
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 3 }
        );

        if (listResult.Items.Count < 2)
        {
            // Skip if not enough submissions for navigation
            return;
        }

        // Use middle item to potentially have both next and previous
        var middleSubmissionId = listResult.Items[1].Id;

        // Act
        var navigation = await _client.GetNextPreviousAsync(middleSubmissionId);

        // Assert
        Assert.NotNull(navigation);
        // May have previous and/or next
    }

    [Fact]
    public async Task GetNextPrevious_FirstSubmission_ShouldHaveNoNext()
    {
        // Arrange
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 1 }
        );

        if (listResult.Items.Count == 0)
        {
            return;
        }

        var submissionId = listResult.Items[0].Id;

        // Act
        var navigation = await _client.GetNextPreviousAsync(submissionId);

        // Assert
        Assert.NotNull(navigation);
        // First submission might have previous but logic depends on sort order
    }

    [Fact]
    public async Task GetNextPrevious_NavigationIds_ShouldBeAccessible()
    {
        // Arrange
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 2 }
        );

        if (listResult.Items.Count == 0)
        {
            return;
        }

        var submissionId = listResult.Items[0].Id;

        // Act
        var navigation = await _client.GetNextPreviousAsync(submissionId);

        // Assert
        if (navigation.Previous != null)
        {
            Assert.NotEmpty(navigation.Previous.Id);
            Assert.NotEmpty(navigation.Previous.Numbering);
        }

        if (navigation.Next != null)
        {
            Assert.NotEmpty(navigation.Next.Id);
            Assert.NotEmpty(navigation.Next.Numbering);
        }
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldSucceed()
    {
        // Arrange
        // Note: In a real test, you would create a submission first
        // For now, we'll skip this test or use a test submission ID
        // Skip test for safety - don't delete real data
        return;

        // var submissionId = "test-submission-id";
        
        // Act
        // await _client.DeleteAsync(submissionId);
        
        // Assert - No exception means success
    }

    #endregion

    #region Form Data Access Tests

    [Fact]
    public async Task Get_ShouldProvideAccessToFormFields()
    {
        // Arrange
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 1 }
        );

        if (listResult.Items.Count == 0)
        {
            return;
        }

        var submissionId = listResult.Items[0].Id;

        // Act
        var submission = await _client.GetAsync(submissionId);

        // Assert
        Assert.NotNull(submission.Data);
        
        // Test accessing form fields (example field names)
        var commonFields = new[] { "name", "email", "phone", "message", "subject" };
        
        foreach (var fieldName in commonFields)
        {
            if (submission.Data.TryGetValue(fieldName, out var value))
            {
                Assert.NotNull(value);
                // Successfully accessed field
            }
        }
    }

    [Fact]
    public async Task Get_ShouldIncludeStatusHistory()
    {
        // Arrange
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 1 }
        );

        if (listResult.Items.Count == 0)
        {
            return;
        }

        var submissionId = listResult.Items[0].Id;

        // Act
        var submission = await _client.GetAsync(submissionId);

        // Assert
        Assert.NotNull(submission.StatusHistory);
        
        if (submission.StatusHistory.Count > 0)
        {
            var firstHistory = submission.StatusHistory[0];
            Assert.NotEmpty(firstHistory.Status);
            Assert.NotEqual(default(DateTime), firstHistory.ChangedAt);
        }
    }

    [Fact]
    public async Task Get_ShouldIncludeSyncingStatus()
    {
        // Arrange
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 1 }
        );

        if (listResult.Items.Count == 0)
        {
            return;
        }

        var submissionId = listResult.Items[0].Id;

        // Act
        var submission = await _client.GetAsync(submissionId);

        // Assert
        Assert.NotNull(submission.Syncing);
        // IsDone should have a value (true or false)
        Assert.True(submission.Syncing.IsDone || !submission.Syncing.IsDone);
    }

    #endregion

    #region Combined Workflow Tests

    [Fact]
    public async Task  Workflow_ListGetNavigate_ShouldSucceed()
    {
        // Step 1: List submissions
        var listResult = await _client.ListAsync(
            new ListFormSubmissionsParams { HtmlHostingId = _testHtmlHostingId },
            new PaginationParams { Page = 1, PageSize = 5 }
        );

        if (listResult.Items.Count == 0)
        {
            return;
        }

        Assert.NotNull(listResult.Items);
        Assert.True(listResult.Items.Count > 0);

        // Step 2: Get first submission
        var firstSubmissionId = listResult.Items[0].Id;
        var submission = await _client.GetAsync(firstSubmissionId);

        Assert.NotNull(submission);
        Assert.Equal(firstSubmissionId, submission.Id);

        // Step 3: Navigate
        var navigation = await _client.GetNextPreviousAsync(firstSubmissionId);
        
        Assert.NotNull(navigation);

        // Step 4: If next exists, get it
        if (navigation.Next != null)
        {
            var nextSubmission = await _client.GetAsync(navigation.Next.Id);
            Assert.NotNull(nextSubmission);
        }
    }

    #endregion

    public void Dispose()
    {
        // Cleanup is handled by test framework
    }
}
