using Posty5.Core.Converts;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Posty5.HtmlHostingFormSubmission.Models;

// ============================================================================
// HELPER MODELS
// ============================================================================

/// <summary>
/// HTML hosting object (populated)
/// </summary>
public class HtmlHostingModel
{
    /// <summary>
    /// HTML hosting ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// HTML hosting name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Custom landing ID
    /// </summary>
    public string? CustomLandingId { get; set; }
}

/// <summary>
/// Form configuration object (populated)
/// </summary>
public class FormModel
{
    /// <summary>
    /// Form ID
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// Form name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Form fields configuration
    /// </summary>
    public List<object>? Fields { get; set; }
}

/// <summary>
/// Status history grouped by day
/// </summary>
public class StatusHistoryGroupedDay
{
    /// <summary>
    /// Date
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Status entries for that day
    /// </summary>
    public List<object> StatusHistory { get; set; } = new();
}

// ============================================================================
// REQUEST MODELS
// ============================================================================

/// <summary>
/// Parameters for listing/filtering form submissions
/// </summary>
public class HtmlHostingFormSubmissionListParamsModel
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
    public HtmlHostingFormSubmissionFormStatusType? Status { get; set; }
    
    /// <summary>
    /// Filtered fields for search (optional) - comma-separated field names
    /// Example: "name,email,phone"
    /// </summary>
    public string? FilteredFields { get; set; }
}

/// <summary>
/// Request to change form submission status
/// </summary>
public class HtmlHostingFormSubmissionChangeStatusRequestModel
{
    /// <summary>
    /// New status value for the submission
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Rejection reason (optional, typically used when status is 'Rejected')
    /// </summary>
    [JsonPropertyName("rejectedReason")]
    public string? RejectedReason { get; set; }

    /// <summary>
    /// Additional notes about the status change
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

// ============================================================================
// RESPONSE MODELS
// ============================================================================

/// <summary>
/// Status history entry for a form submission
/// </summary>
public class HtmlHostingFormSubmissionStatusHistoryEntryModel
{
    /// <summary>
    /// Status value
    /// </summary>
    public HtmlHostingFormSubmissionFormStatusType Status { get; set; } = HtmlHostingFormSubmissionFormStatusType.New;
    
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
public class HtmlHostingFormSubmissionSyncingStatusModel
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
public class HtmlHostingFormSubmissionModel
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
    /// External reference ID
    /// </summary>
    public string? RefId { get; set; }
    
    /// <summary>
    /// Custom tag
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    public HtmlHostingFormSubmissionFormStatusType Status { get; set; } = HtmlHostingFormSubmissionFormStatusType.New;
}

/// <summary>
/// HTML hosting form submission full details model (from GET by ID)
/// </summary>
public class HtmlHostingFormSubmissionFullDetailsModel : HtmlHostingFormSubmissionModel
{
    /// <summary>
    /// HTML hosting object (populated)
    /// </summary>
    public HtmlHostingModel? HtmlHosting { get; set; }
    
    /// <summary>
    /// Form configuration object (populated)
    /// </summary>
    public FormModel? Form { get; set; }
    
    /// <summary>
    /// Status history grouped by day
    /// </summary>
    public List<StatusHistoryGroupedDay>? StatusHistoryGrouped { get; set; }   /// </summary>
    public List<HtmlHostingFormSubmissionStatusHistoryEntryModel> StatusHistory { get; set; } = new();
    
    /// <summary>
    /// Syncing status
    /// </summary>
    public HtmlHostingFormSubmissionSyncingStatusModel Syncing { get; set; } = new();
    
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
public class HtmlHostingFormSubmissionNextPreviousSubmissionModel
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
public class HtmlHostingFormSubmissionNextPreviousResponseModel
{
    /// <summary>
    /// Previous submission (if exists)
    /// </summary>
    public HtmlHostingFormSubmissionNextPreviousSubmissionModel? Previous { get; set; }
    
    /// <summary>
    /// Next submission (if exists)
    /// </summary>
    public HtmlHostingFormSubmissionNextPreviousSubmissionModel? Next { get; set; }
}

/// <summary>
/// Response from changing form submission status
/// </summary>
public class HtmlHostingFormSubmissionChangeStatusResponseModel
{
    /// <summary>
    /// Success message
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Updated status history (grouped by status)
    /// </summary>
    [JsonPropertyName("statusHistory")]
    public List<object>? StatusHistory { get; set; }
}


[JsonConverter(typeof(StringValueObjectConverter<HtmlHostingFormSubmissionFormStatusType>))]

public readonly record struct HtmlHostingFormSubmissionFormStatusType (string Value)
{
    public static readonly HtmlHostingFormSubmissionFormStatusType New = new("new");
    public static readonly HtmlHostingFormSubmissionFormStatusType PendingReview = new("pendingReview");
    public static readonly HtmlHostingFormSubmissionFormStatusType InProgress = new("inProgress");
    public static readonly HtmlHostingFormSubmissionFormStatusType OnHold = new("onHold");
    public static readonly HtmlHostingFormSubmissionFormStatusType NeedMoreInfo = new("needMoreInfo");
    public static readonly HtmlHostingFormSubmissionFormStatusType Approved = new("approved");
    public static readonly HtmlHostingFormSubmissionFormStatusType PartiallyApproved = new("partiallyApproved");
    public static readonly HtmlHostingFormSubmissionFormStatusType Rejected = new("rejected");
    public static readonly HtmlHostingFormSubmissionFormStatusType Completed = new("completed");
    public static readonly HtmlHostingFormSubmissionFormStatusType Archived = new("archived");
    public static readonly HtmlHostingFormSubmissionFormStatusType Cancelled = new("cancelled");

    //private static readonly HashSet<string> Allowed = new()
    //{
    //    "new",
    //    "pendingReview",
    //    "inProgress",
    //    "onHold",
    //    "needMoreInfo",
    //    "approved",
    //    "partiallyApproved",
    //    "rejected",
    //    "completed",
    //    "archived",
    //    "cancelled"
    //};

    //public static FormStatusType From (string value)
    //{
    //    if (!Allowed.Contains(value))
    //        throw new ArgumentException($"Invalid FormStatusType: {value}");

    //    return new FormStatusType(value);
    //}

    public override string ToString ( ) => Value;
}

/// <summary>
/// Delete response from delete submission operation
/// </summary>
public class DeleteResponse
{
    /// <summary>
    /// Message confirming deletion
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
 


