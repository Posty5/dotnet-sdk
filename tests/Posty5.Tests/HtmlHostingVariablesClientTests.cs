using Xunit;
using Posty5.HtmlHostingVariables;
using Posty5.HtmlHostingVariables.Models;
using Posty5.Core.Models;

namespace Posty5.Tests.Integration;

[Collection("Sequential")]
public class HtmlHostingVariablesClientTests : IDisposable
{
    private readonly HtmlHostingVariablesClient _client;

    public HtmlHostingVariablesClientTests()
    {
        var httpClient = TestConfig.CreateHttpClient();
        _client = new HtmlHostingVariablesClient(httpClient);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidKey_ShouldSucceed()
    {
        // Arrange
        var request = new HtmlHostingVariablesCreateRequestModel
        {
            Name = $"API Key - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Key = $"pst5_test_key_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "test_value_123",
            Tag = "test"
        };

        // Act & Assert (no exception means success)
        await _client.CreateAsync(request);
    }

    [Fact]
    public async Task Create_WithInvalidKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Invalid Key Variable",
            Key = "invalid_key", // Missing pst5_ prefix
            Value = "test_value"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _client.CreateAsync(request)
        );

        Assert.Contains("Key must start with 'pst5_'", exception.Message);
        Assert.Contains("pst5_invalid_key", exception.Message);
    }

    [Fact]
    public async Task Create_WithTagAndRefId_ShouldSucceed()
    {
        // Arrange
        var request = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Variable with Metadata",
            Key = $"pst5_metadata_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "metadata_value",
            Tag = "production",
            RefId = "ref-001"
        };

        // Act & Assert
        await _client.CreateAsync(request);
    }

    [Fact]
    public async Task Create_EmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Empty Key",
            Key = "",
            Value = "value"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _client.CreateAsync(request)
        );

        Assert.Contains("Key must start with 'pst5_'", exception.Message);
    }

    [Fact]
    public async Task Create_KeyStartingWithPst5_ShouldSucceed()
    {
        // Arrange
        var request = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Correct Prefix",
            Key = $"pst5_correct_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "correct_value"
        };

        // Act & Assert
        await _client.CreateAsync(request);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithValidId_ShouldReturnVariable()
    {
        // Arrange - Create a variable first
        var createRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Variable for Get Test",
            Key = $"pst5_get_test_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "get_test_value",
            Tag = "get-test"
        };

        await _client.CreateAsync(createRequest);

        // Get the variable by listing and finding it
        var list = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { Tag = "get-test" }
        );

        Assert.NotEmpty(list.Items);
        var createdId = list.Items[0].Id;

        // Act
        var result = await _client.GetAsync(createdId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdId, result.Id);
        Assert.Equal("Variable for Get Test", result.Name);
        Assert.StartsWith("pst5_get_test_", result.Key);
        Assert.Equal("get_test_value", result.Value);
        Assert.Equal("get-test", result.Tag);

        TestConfig.CreatedResources.HtmlHostings.Add(createdId);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidKey_ShouldSucceed()
    {
        // Arrange - Create a variable first
        var createRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Original Variable",
            Key = $"pst5_update_test_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "original_value"
        };

        await _client.CreateAsync(createRequest);

        var list = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel 
            { 
                Key = createRequest.Key 
            }
        );

        Assert.NotEmpty(list.Items);
        var variableId = list.Items[0].Id;

        // Act
        var updateRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Updated Variable",
            Key = createRequest.Key,
            Value = "updated_value"
        };

        await _client.UpdateAsync(variableId, updateRequest);

        // Assert
        var updated = await _client.GetAsync(variableId);
        Assert.Equal("Updated Variable", updated.Name);
        Assert.Equal("updated_value", updated.Value);

        TestConfig.CreatedResources.HtmlHostings.Add(variableId);
    }

    [Fact]
    public async Task Update_WithInvalidKey_ShouldThrowArgumentException()
    {
        // Arrange - Create a variable first
        var createRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Variable to Update",
            Key = $"pst5_before_update_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "value"
        };

        await _client.CreateAsync(createRequest);

        var list = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { Key = createRequest.Key }
        );

        Assert.NotEmpty(list.Items);
        var variableId = list.Items[0].Id;

        // Act & Assert
        var updateRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Updated",
            Key = "invalid_key", // Missing pst5_ prefix
            Value = "new_value"
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _client.UpdateAsync(variableId, updateRequest)
        );

        Assert.Contains("Key must start with 'pst5_'", exception.Message);

        TestConfig.CreatedResources.HtmlHostings.Add(variableId);
    }

    [Fact]
    public async Task Update_ChangeTagAndRefId_ShouldSucceed()
    {
        // Arrange - Create a variable
        var createRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Metadata Update Test",
            Key = $"pst5_metadata_update_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "value",
            Tag = "old-tag",
            RefId = "old-ref"
        };

        await _client.CreateAsync(createRequest);

        var list = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { Tag = "old-tag" }
        );

        Assert.NotEmpty(list.Items);
        var variableId = list.Items[0].Id;

        // Act
        var updateRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = createRequest.Name,
            Key = createRequest.Key,
            Value = "value",
            Tag = "new-tag",
            RefId = "new-ref"
        };

        await _client.UpdateAsync(variableId, updateRequest);

        // Assert
        var updated = await _client.GetAsync(variableId);
        Assert.Equal("new-tag", updated.Tag);
        Assert.Equal("new-ref", updated.RefId);

        TestConfig.CreatedResources.HtmlHostings.Add(variableId);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldSucceed()
    {
        // Arrange - Create a variable
        var createRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Variable to Delete",
            Key = $"pst5_delete_test_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "delete_value"
        };

        await _client.CreateAsync(createRequest);

        var list = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { Key = createRequest.Key }
        );

        Assert.NotEmpty(list.Items);
        var variableId = list.Items[0].Id;

        // Act
        await _client.DeleteAsync(variableId);

        // Assert - No exception means success
    }

    #endregion

    #region List Tests

    [Fact]
    public async Task List_WithoutFilters_ShouldReturnPaginatedResults()
    {
        // Act
        var result = await _client.ListAsync(
            pagination: new PaginationParams { PageSize = 10 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count <= 10);
    }

    [Fact]
    public async Task List_FilterByTag_ShouldReturnMatchingVariables()
    {
        // Arrange
        var uniqueTag = $"tag_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        
        await _client.CreateAsync(new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Tagged Variable 1",
            Key = $"pst5_tagged1_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "value1",
            Tag = uniqueTag
        });

        await _client.CreateAsync(new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Tagged Variable 2",
            Key = $"pst5_tagged2_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "value2",
            Tag = uniqueTag
        });

        // Act
        var result = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { Tag = uniqueTag }
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count >= 2);
        Assert.All(result.Items, item => Assert.Equal(uniqueTag, item.Tag));
    }

    [Fact]
    public async Task List_FilterByKey_ShouldReturnMatchingVariables()
    {
        // Arrange
        var keyPrefix = $"pst5_key_filter_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        
        await _client.CreateAsync(new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Key Filter Test",
            Key = $"{keyPrefix}_001",
            Value = "value"
        });

        // Act
        var result = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { Key = keyPrefix }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => Assert.Contains(keyPrefix, item.Key));
    }

    [Fact]
    public async Task List_FilterByName_ShouldReturnMatchingVariables()
    {
        // Arrange
        var uniqueName = $"NameFilter_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        
        await _client.CreateAsync(new HtmlHostingVariablesCreateRequestModel
        {
            Name = uniqueName,
            Key = $"pst5_name_filter_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "value"
        });

        // Act
        var result = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { Name = uniqueName }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task List_FilterByRefId_ShouldReturnMatchingVariables()
    {
        // Arrange
        var uniqueRef = $"ref_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        
        await _client.CreateAsync(new HtmlHostingVariablesCreateRequestModel
        {
            Name = "RefId Test",
            Key = $"pst5_refid_test_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "value",
            RefId = uniqueRef
        });

        // Act
        var result = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { RefId = uniqueRef }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => Assert.Equal(uniqueRef, item.RefId));
    }

    [Fact]
    public async Task List_WithMultipleFilters_ShouldCombineFilters()
    {
        // Arrange
        var tag = $"multi_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var keyPrefix = $"pst5_multi_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        await _client.CreateAsync(new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Multi Filter Test",
            Key = $"{keyPrefix}_001",
            Value = "value",
            Tag = tag
        });

        // Act
        var result = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel
            {
                Tag = tag,
                Key = keyPrefix
            }
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item =>
        {
            Assert.Equal(tag, item.Tag);
            Assert.Contains(keyPrefix, item.Key);
        });
    }

    [Fact]
    public async Task List_WithPagination_ShouldRespectPageSize()
    {
        // Act
        var result = await _client.ListAsync(
            pagination: new PaginationParams { PageSize = 5 }
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count <= 5);
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_CreateGetUpdateDelete_ShouldSucceed()
    {
        // Create
        var createRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Workflow Test",
            Key = $"pst5_workflow_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "original_value",
            Tag = "workflow-test"
        };

        await _client.CreateAsync(createRequest);

        // Get by listing
        var list = await _client.ListAsync(
            new HtmlHostingVariablesListParamsModel { Tag = "workflow-test" }
        );

        Assert.NotEmpty(list.Items);
        var variableId = list.Items[0].Id;

        // Get by ID
        var retrieved = await _client.GetAsync(variableId);
        Assert.Equal("Workflow Test", retrieved.Name);
        Assert.Equal("original_value", retrieved.Value);

        // Update
        var updateRequest = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Workflow Test Updated",
            Key = createRequest.Key,
            Value = "updated_value",
            Tag = "workflow-test"
        };

        await _client.UpdateAsync(variableId, updateRequest);

        // Verify update
        var updated = await _client.GetAsync(variableId);
        Assert.Equal("Workflow Test Updated", updated.Name);
        Assert.Equal("updated_value", updated.Value);

        // Delete
        await _client.DeleteAsync(variableId);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Create_WithSpecialCharactersInValue_ShouldSucceed()
    {
        // Arrange
        var request = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Special Characters",
            Key = $"pst5_special_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = "value with symbols: !@#$%^&*()_+-=[]{}|;:',.<>?/~`"
        };

        // Act & Assert
        await _client.CreateAsync(request);
    }

    [Fact]
    public async Task Create_WithLongValue_ShouldSucceed()
    {
        // Arrange
        var longValue = new string('x', 1000);
        var request = new HtmlHostingVariablesCreateRequestModel
        {
            Name = "Long Value",
            Key = $"pst5_long_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Value = longValue
        };

        // Act & Assert
        await _client.CreateAsync(request);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup is handled by test framework if needed
    }
}

