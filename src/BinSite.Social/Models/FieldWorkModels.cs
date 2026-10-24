namespace BinSite.Social.Models;

/// <summary>Describes the local work-order lifecycle; completion is an explicit review action.</summary>
public enum FieldWorkStatus { Plan, InProgress, ReadyForReview, Complete }

/// <summary>Identifies a confirmation, bounded measurement, or actual raster-photo evidence field.</summary>
public enum FieldWorkFieldKind { Confirmation, Measurement, Photo }

/// <summary>Contains the templates and work orders exported together as one portable local project.</summary>
public sealed class FieldWorkWorkspace
{
    public int Version { get; set; } = 1;
    public string Title { get; set; } = "My field workspace";
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<FieldWorkTemplate> Templates { get; set; } = [];
    public List<FieldWorkOrder> Orders { get; set; } = [];
}

/// <summary>Stores an editable checklist definition; orders retain a separate versioned snapshot.</summary>
public sealed class FieldWorkTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Revision { get; set; } = 1;
    public string Title { get; set; } = "New checklist";
    public string Description { get; set; } = "";
    public List<FieldWorkField> Fields { get; set; } = [];
}

/// <summary>Defines one evidence requirement and its optional numeric bounds.</summary>
public sealed class FieldWorkField
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Label { get; set; } = "New confirmation";
    public string Instructions { get; set; } = "";
    public FieldWorkFieldKind Kind { get; set; }
    public bool Required { get; set; } = true;
    public string Unit { get; set; } = "";
    public decimal? Minimum { get; set; }
    public decimal? Maximum { get; set; }
}

/// <summary>Holds a job, its frozen checklist, supplied answers, and explicit status history.</summary>
public sealed class FieldWorkOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Reference { get; set; } = "";
    public string CustomerLabel { get; set; } = "";
    public string ReviewerLabel { get; set; } = "";
    public string Notes { get; set; } = "";
    public Guid TemplateId { get; set; }
    public string TemplateTitle { get; set; } = "";
    public int TemplateRevision { get; set; }
    public List<FieldWorkField> Fields { get; set; } = [];
    public List<FieldWorkAnswer> Answers { get; set; } = [];
    public FieldWorkStatus Status { get; set; }
    public int Revision { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public List<FieldWorkEvent> History { get; set; } = [];
}

/// <summary>Preserves raw numeric input, a confirmation, notes, and a locally attached photo.</summary>
public sealed class FieldWorkAnswer
{
    public Guid FieldId { get; set; }
    public bool Confirmed { get; set; }
    public string NumberText { get; set; } = "";
    public string Note { get; set; } = "";
    public FieldWorkPhoto? Photo { get; set; }
}

/// <summary>Embeds a selected raster image; a filename or fingerprint alone is not photo evidence.</summary>
public sealed class FieldWorkPhoto
{
    public string FileName { get; set; } = "";
    public string DataUrl { get; set; } = "";
    public string ContentType { get; set; } = "image/jpeg";
    public string SourceSha256 { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public int ByteLength { get; set; }
}

/// <summary>Records a local workflow action without claiming third-party approval or an authenticated signature.</summary>
public sealed class FieldWorkEvent
{
    public DateTimeOffset At { get; set; } = DateTimeOffset.UtcNow;
    public string Action { get; set; } = "";
    public string Detail { get; set; } = "";
}

/// <summary>Identifies an actual missing or invalid requirement shown in the editor and report.</summary>
public sealed record FieldWorkIssue(Guid? FieldId, string Label, string Message);

/// <summary>Reports whether a requested workflow transition changed the order.</summary>
public sealed record FieldWorkTransition(bool Succeeded, IReadOnlyList<FieldWorkIssue> Issues);
