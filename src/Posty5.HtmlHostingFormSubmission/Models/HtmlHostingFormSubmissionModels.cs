using System.Text.Json.Serialization;

namespace Posty5.HtmlHostingFormSubmission.Models;

// ============================================================================
// REQUEST MODELS
// ============================================================================

/// <summary>
/// Parameters for listing/filtering form submissions
/// </summary>
public class ListFormSubmissionsParams
{
    /// <summary>
    /// HTML hosting ID (required) - the target page landing ID
    /// </summary>
    public string HtmlHostingId { get; set; } = string.Empty;
    
    /// <summary>
    /// Form ID filter (optional) - HTML form ID to track submissions
    /// </summary>
    public string? FormId { get; set; }
    
    /// <summary>
    /// Submission numbering filter (optional)
    /// </summary>
    public string? Numbering { get; set; }
    
    /// <summary>
    /// Status filter (optional) - "New", "Viewed", "Approved", "Rejected"
    /// </summary>
    public FormStatus? Status { get; set; }
    
    /// <summary>
    /// Filtered fields for search (optional) - comma-separated field names
    /// Example: "name,email,phone"
    /// </summary>
    public string? FilteredFields { get; set; }
}

// ============================================================================
// RESPONSE MODELS
// ============================================================================

/// <summary>
/// Status history entry for a form submission
/// </summary>
public class StatusHistoryEntry
{
    /// <summary>
    /// Status value
    /// </summary>
    public FormStatus Status { get; set; } = FormStatus.New;
    
    /// <summary>
    /// Rejection reason (if rejected)
    /// </summary>
    public string? RejectedReason { get; set; }
    
    /// <summary>
    /// Date when status changed
    /// </summary>
    public DateTime ChangedAt { get; set; }
    
    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Syncing status for form submission
/// </summary>
public class SyncingStatus
{
    /// <summary>
    /// Whether syncing is complete
    /// </summary>
    public bool IsDone { get; set; }
    
    /// <summary>
    /// Last error message (if any)
    /// </summary>
    public string? LastError { get; set; }
    
    /// <summary>
    /// Last attempt timestamp  
    /// </summary>
    public DateTime? LastAttemptAt { get; set; }
}

/// <summary>
/// HTML hosting form submission model
/// </summary>
public class FormSubmissionModel
{
    /// <summary>
    /// Submission ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// HTML hosting ID reference
    /// </summary>
    public string HtmlHostingId { get; set; } = string.Empty;
    
    /// <summary>
    /// Form ID reference
    /// </summary>
    public string FormId { get; set; } = string.Empty;
    
    /// <summary>
    /// Visitor ID reference
    /// </summary>
    public string VisitorId { get; set; } = string.Empty;
    
    /// <summary>
    /// Submission numbering
    /// </summary>
    public string Numbering { get; set; }
    
    /// <summary>
    /// Form data as key-value pairs (dynamic form fields)
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
    
    /// <summary>
    /// Form fields list
    /// </summary>
    public List<string>? Fields { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    public FormStatus Status { get; set; } = FormStatus.New;
    
    /// <summary>
    /// Status history
    /// </summary>
    public List<StatusHistoryEntry> StatusHistory { get; set; } = new();
    
    /// <summary>
    /// Syncing status
    /// </summary>
    public SyncingStatus Syncing { get; set; } = new();
    
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
/// Next or previous submission reference for navigation
/// </summary>
public class NextPreviousSubmission
{
    /// <summary>
    /// Submission ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Submission numbering (padded string format)
    /// </summary>
    public string Numbering { get; set; } = string.Empty;
}

/// <summary>
/// Next and previous submissions for navigation
/// </summary>
public class NextPreviousSubmissionsResponse
{
    /// <summary>
    /// Previous submission (if exists)
    /// </summary>
    public NextPreviousSubmission? Previous { get; set; }
    
    /// <summary>
    /// Next submission (if exists)
    /// </summary>
    public NextPreviousSubmission? Next { get; set; }
}

public enum FormStatus
{
   New=1,
   PendingReview,
   InProgress             ,
   OnHold                 ,
   NeedMoreInfo           ,
   Approved               ,
   PartiallyApproved      ,
   Rejected               ,
   Completed              ,
   Archived               ,
   Cancelled             ,

}