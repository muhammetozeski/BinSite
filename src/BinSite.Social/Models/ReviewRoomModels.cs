using System.Text.Json.Serialization;

namespace BinSite.Social.Models;

/// <summary>The locally supported media families.</summary>
public enum ReviewMediaKind { Image, Video }
/// <summary>A useful action category, independent of a comment's workflow state.</summary>
public enum ReviewCommentCategory { Content, Layout, Accessibility, Timing, Technical, Other }
/// <summary>Blocking comments must be resolved before recording approval.</summary>
public enum ReviewCommentPriority { Note, Blocking }
/// <summary>Recorded local review decisions; they are not authenticated customer signatures.</summary>
public enum ReviewDecisionState { Draft, Ready, ChangesRequested, Approved, Delivered }
/// <summary>Distinguishes authored examples from a person's actual project entries.</summary>
public enum ReviewEntrySource { Local, EditorialExample }

/// <summary>A portable project document containing metadata and notes, never local media bytes or URLs.</summary>
public sealed class ReviewProject
{
    [JsonRequired] public int FormatVersion { get; set; } = 1;
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Title { get; set; } = "Untitled review";
    [JsonRequired] public string Brief { get; set; } = "";
    [JsonRequired] public bool IsEditorialExample { get; set; }
    [JsonRequired] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [JsonRequired] public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    [JsonRequired] public List<ReviewAsset> Assets { get; set; } = [];
    [JsonRequired] public List<ReviewComment> Comments { get; set; } = [];
    [JsonRequired] public List<ReviewDecision> Decisions { get; set; } = [];
}
/// <summary>A logical delivery with versions of one media kind.</summary>
public sealed class ReviewAsset
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Title { get; set; } = "";
    [JsonRequired] public ReviewMediaKind Kind { get; set; }
    [JsonRequired] public List<ReviewVersion> Versions { get; set; } = [];
}
/// <summary>A particular file revision identified by its complete SHA-256 digest and byte count.</summary>
public sealed class ReviewVersion
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Label { get; set; } = "Version 1";
    [JsonRequired] public string FileName { get; set; } = "";
    [JsonRequired] public string MimeType { get; set; } = "";
    [JsonRequired] public long SizeBytes { get; set; }
    [JsonRequired] public string Fingerprint { get; set; } = "";
    [JsonRequired] public int? Width { get; set; }
    [JsonRequired] public int? Height { get; set; }
    [JsonRequired] public double? DurationSeconds { get; set; }
    [JsonRequired] public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}
/// <summary>A note tied to exactly one asset, version and normalized image point or actual video time.</summary>
public sealed class ReviewComment
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public Guid AssetId { get; set; }
    [JsonRequired] public Guid VersionId { get; set; }
    [JsonRequired] public string Body { get; set; } = "";
    [JsonRequired] public ReviewCommentCategory Category { get; set; }
    [JsonRequired] public ReviewCommentPriority Priority { get; set; }
    [JsonRequired] public bool Resolved { get; set; }
    [JsonRequired] public double? X { get; set; }
    [JsonRequired] public double? Y { get; set; }
    [JsonRequired] public double? TimeSeconds { get; set; }
    [JsonRequired] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [JsonRequired] public DateTimeOffset? ResolvedAt { get; set; }
    [JsonRequired] public ReviewEntrySource Source { get; set; }
}
/// <summary>An append-only decision record for a particular delivery revision.</summary>
public sealed class ReviewDecision
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public Guid AssetId { get; set; }
    [JsonRequired] public Guid VersionId { get; set; }
    [JsonRequired] public ReviewDecisionState State { get; set; }
    [JsonRequired] public string Note { get; set; } = "";
    [JsonRequired] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [JsonRequired] public ReviewEntrySource Source { get; set; }
}
/// <summary>Ephemeral browser file metadata; Token is never included in a saved project document.</summary>
public sealed class ReviewPickedFile
{
    public string Token { get; set; } = "";
    public string Name { get; set; } = "";
    public string MimeType { get; set; } = "";
    public string Kind { get; set; } = "";
    public long SizeBytes { get; set; }
    public string Fingerprint { get; set; } = "";
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? DurationSeconds { get; set; }
}
/// <summary>A browser picker result with a specific user-facing failure instead of a fabricated asset.</summary>
public sealed class ReviewPickResult
{
    public List<ReviewPickedFile> Files { get; set; } = [];
    public string? Error { get; set; }
}
/// <summary>A bounded file read result used for explicit project restoration.</summary>
public sealed class ReviewTextResult
{
    public string? Text { get; set; }
    public string? Error { get; set; }
}
/// <summary>An original authored demonstration with two locally generated image versions.</summary>
public sealed record ReviewExample(string Id,string Title,string Brief,string FirstNote,string SecondNote,double X,double Y);
/// <summary>A normalized point captured from the actual displayed image rectangle.</summary>
public sealed record ReviewPoint(double X,double Y);
