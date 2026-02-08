using System.Text.Json.Serialization;

namespace Posty5.HtmlHostingVariables.Models;
  
/// <summary>
/// HTML hosting variable model
/// </summary>
public class HtmlHostingVariablesVariableModel
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Variable name (human-readable)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Variable key (must start with pst5_)
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Variable value
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
 
 
       /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }


}

/// <summary>
/// Create/Update HTML hosting variable request
/// </summary>
public class HtmlHostingVariablesCreateRequestModel
{
    /// <summary>
    /// Variable name (human-readable, required)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Variable key (required, must start with pst5_)
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Variable value (required)
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom tag for filtering/categorization
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// External reference ID for filtering/tracking
    /// </summary>
    public string? RefId { get; set; }
}

/// <summary>
/// Parameters for listing/filtering HTML hosting variables
/// </summary>
public class HtmlHostingVariablesListParamsModel
{
    /// <summary>
    /// Filter by variable name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Filter by variable key
    /// </summary>
    public string? Key { get; set; }
    
    /// <summary>
    /// Filter by variable value
    /// </summary>
    public string? Value { get; set; }
    
    /// <summary>
    /// Filter by custom tag
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Filter by external reference ID
    /// </summary>
    public string? RefId { get; set; }
}

/// <summary>
/// Delete response model
/// </summary>
public class DeleteResponse
{
    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
