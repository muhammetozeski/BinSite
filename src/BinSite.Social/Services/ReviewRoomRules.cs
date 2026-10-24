using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BinSite.Social.Services;

/// <summary>A specific local document or workflow validation failure.</summary>
public sealed class ReviewValidationException(string message) : Exception(message);

/// <summary>Validates portable review documents and applies copy-on-success local state transitions.</summary>
public static class ReviewRoomRules
{
    public const int MaximumProjectBytes = 4 * 1024 * 1024;
    public const long MaximumMediaBytes = 128 * 1024 * 1024;
    static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    static readonly HashSet<string> ImageTypes = ["image/png", "image/jpeg", "image/webp", "image/gif", "image/avif"];
    static readonly HashSet<string> VideoTypes = ["video/mp4", "video/webm", "video/ogg"];

    /// <summary>Uses strict, portable string enums and rejects unknown document fields.</summary>
    static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true, MaxDepth = 20, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        return options;
    }

    /// <summary>Creates an empty local project without a server account or invented participants.</summary>
    public static ReviewProject Create(string title = "Untitled review", string brief = "")
    {
        var project = new ReviewProject { Title = Clean(title, "Project title", 1, 140), Brief = Clean(brief, "Project brief", 0, 3000) };
        Validate(project);
        return project;
    }

    /// <summary>Serializes metadata and comments only, after checking the complete document and size bound.</summary>
    public static string Serialize(ReviewProject project)
    {
        Validate(project);
        var text = JsonSerializer.Serialize(project, JsonOptions);
        if (Encoding.UTF8.GetByteCount(text) > MaximumProjectBytes) throw new ReviewValidationException("The project exceeds the 4 MB document limit.");
        return text;
    }

    /// <summary>Reads a strict project document without changing current state or binding arbitrary media URLs.</summary>
    public static ReviewProject Restore(string json)
    {
        if (Encoding.UTF8.GetByteCount(json) > MaximumProjectBytes) throw new ReviewValidationException("Choose a project JSON file no larger than 4 MB.");
        try
        {
            var project = JsonSerializer.Deserialize<ReviewProject>(json, JsonOptions) ?? throw new ReviewValidationException("The file does not contain a review project.");
            Validate(project);
            return project;
        }
        catch (JsonException) { throw new ReviewValidationException("The project JSON has an unsupported field, value or document structure."); }
    }

    /// <summary>Edits the project heading while retaining all file identities and review records.</summary>
    public static ReviewProject Rename(ReviewProject project, string title, string brief) => Change(project, next => { next.Title = Clean(title, "Project title", 1, 140); next.Brief = Clean(brief, "Project brief", 0, 3000); });

    /// <summary>Adds actual selected media as new assets, committing the entire batch only when all records validate.</summary>
    public static ReviewProject AddAssets(ReviewProject project, IReadOnlyList<ReviewPickedFile> files) => Change(project, next =>
    {
        if (files.Count == 0) throw new ReviewValidationException("Select at least one supported image or video.");
        foreach (var file in files)
        {
            var version = FromFile(file, "Version 1");
            next.Assets.Add(new ReviewAsset { Title = file.Name, Kind = ParseKind(file.Kind), Versions = [version] });
        }
    });

    /// <summary>Adds one genuinely different file revision to an existing logical asset.</summary>
    public static ReviewProject AddVersion(ReviewProject project, Guid assetId, ReviewPickedFile file, string label) => Change(project, next =>
    {
        var asset = Asset(next, assetId);
        if (ParseKind(file.Kind) != asset.Kind) throw new ReviewValidationException("A version must use the same media kind as its asset.");
        if (asset.Versions.Any(version => version.Fingerprint == file.Fingerprint && version.SizeBytes == file.SizeBytes)) throw new ReviewValidationException("This exact file is already a version of this asset. Reconnect it instead of adding a duplicate.");
        asset.Versions.Add(FromFile(file, Clean(label, "Version label", 1, 80)));
    });

    /// <summary>Finds exact saved revisions by digest, size and kind; matching does not rely on a file's display name.</summary>
    public static List<Guid> MatchingVersions(ReviewProject project, ReviewPickedFile file)
    {
        var kind = ParseKind(file.Kind);
        ValidateFile(file);
        return project.Assets.Where(asset => asset.Kind == kind).SelectMany(asset => asset.Versions).Where(version => version.SizeBytes == file.SizeBytes && version.Fingerprint == file.Fingerprint).Select(version => version.Id).ToList();
    }

    /// <summary>Adds a version-specific normalized image marker or actual video timestamp.</summary>
    public static ReviewProject AddComment(ReviewProject project, Guid assetId, Guid versionId, string body, ReviewCommentCategory category, ReviewCommentPriority priority, double? x, double? y, double? seconds, ReviewEntrySource source = ReviewEntrySource.Local) => Change(project, next =>
    {
        var asset = Asset(next, assetId); _ = Version(asset, versionId);
        if (priority == ReviewCommentPriority.Blocking && CurrentDecision(next, versionId) is ReviewDecisionState.Approved or ReviewDecisionState.Delivered) throw new ReviewValidationException("Reopen the review before adding a blocking comment to an approved or delivered version.");
        next.Comments.Add(new ReviewComment { AssetId = assetId, VersionId = versionId, Body = Clean(body, "Comment", 3, 2000), Category = category, Priority = priority, X = x, Y = y, TimeSeconds = seconds, Source = source });
    });

    /// <summary>Changes one comment's resolution while preventing a silently contradictory approved state.</summary>
    public static ReviewProject Resolve(ReviewProject project, Guid commentId, bool resolved) => Change(project, next =>
    {
        var comment = next.Comments.FirstOrDefault(item => item.Id == commentId) ?? throw new ReviewValidationException("The comment was not found.");
        if (!resolved && comment.Priority == ReviewCommentPriority.Blocking && CurrentDecision(next, comment.VersionId) is ReviewDecisionState.Approved or ReviewDecisionState.Delivered) throw new ReviewValidationException("Reopen the review before reopening a blocking comment.");
        comment.Resolved = resolved;
        comment.ResolvedAt = resolved ? DateTimeOffset.UtcNow : null;
    });

    /// <summary>Records an explicit local workflow decision; approval requires no open blocking comments.</summary>
    public static ReviewProject Decide(ReviewProject project, Guid assetId, Guid versionId, ReviewDecisionState state, string note, ReviewEntrySource source = ReviewEntrySource.Local) => Change(project, next =>
    {
        _ = Version(Asset(next, assetId), versionId);
        var current = CurrentDecision(next, versionId);
        if (!Allowed(current, state)) throw new ReviewValidationException($"Cannot change {StateLabel(current)} directly to {StateLabel(state)}.");
        if (state == ReviewDecisionState.Approved && next.Comments.Any(comment => comment.VersionId == versionId && comment.Priority == ReviewCommentPriority.Blocking && !comment.Resolved)) throw new ReviewValidationException("Resolve every blocking comment before recording approval.");
        next.Decisions.Add(new ReviewDecision { AssetId = assetId, VersionId = versionId, State = state, Note = Clean(note, "Decision note", 3, 1200), Source = source });
    });

    /// <summary>Returns the latest recorded decision, or draft when no decision exists.</summary>
    public static ReviewDecisionState CurrentDecision(ReviewProject project, Guid versionId) => project.Decisions.LastOrDefault(item => item.VersionId == versionId)?.State ?? ReviewDecisionState.Draft;

    /// <summary>Provides stable, human-readable workflow labels.</summary>
    public static string StateLabel(ReviewDecisionState state) => state switch { ReviewDecisionState.Draft => "Draft", ReviewDecisionState.Ready => "Ready for review", ReviewDecisionState.ChangesRequested => "Changes requested", ReviewDecisionState.Approved => "Approved", ReviewDecisionState.Delivered => "Delivered", _ => "Unknown" };

    /// <summary>Formats a saved anchor without implying that media was uploaded or embedded.</summary>
    public static string AnchorLabel(ReviewComment comment) => comment.TimeSeconds is double seconds ? TimeLabel(seconds) : $"x {comment.X * 100:0.0}%, y {comment.Y * 100:0.0}%";

    /// <summary>Formats real video time including milliseconds for precise editing notes.</summary>
    public static string TimeLabel(double seconds) => TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss\.fff", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Validates all references, file identities, anchors, enum values and decision histories.</summary>
    public static void Validate(ReviewProject project)
    {
        if (project.FormatVersion != 1 || project.Id == Guid.Empty) throw new ReviewValidationException("This project format is not supported.");
        _ = Clean(project.Title, "Project title", 1, 140); _ = Clean(project.Brief, "Project brief", 0, 3000);
        if (project.Assets is null || project.Comments is null || project.Decisions is null || project.Assets.Count > 30 || project.Comments.Count > 1000 || project.Decisions.Count > 1000) throw new ReviewValidationException("The project exceeds its asset or review-record limits.");
        HashSet<Guid> ids = [project.Id];
        Dictionary<Guid, (ReviewAsset Asset, ReviewVersion Version)> versions = [];
        foreach (var asset in project.Assets)
        {
            if (asset is null || !Enum.IsDefined(asset.Kind) || asset.Versions is null || asset.Versions.Count is < 1 or > 20) throw new ReviewValidationException("An asset has invalid versions or media kind.");
            Unique(ids, asset.Id); _ = Clean(asset.Title, "Asset title", 1, 255);
            HashSet<string> hashes = [];
            foreach (var version in asset.Versions)
            {
                if (version is null) throw new ReviewValidationException("A version record is missing.");
                Unique(ids, version.Id); _ = Clean(version.Label, "Version label", 1, 80); _ = Clean(version.FileName, "File name", 1, 255);
                ValidateVersion(asset.Kind, version);
                if (!hashes.Add(version.Fingerprint + ":" + version.SizeBytes)) throw new ReviewValidationException("An asset contains duplicate file revisions.");
                versions.Add(version.Id, (asset, version));
            }
        }
        if (versions.Count > 100) throw new ReviewValidationException("A project supports at most 100 media versions.");
        foreach (var comment in project.Comments)
        {
            if (comment is null) throw new ReviewValidationException("A comment record is missing.");
            Unique(ids, comment.Id); _ = Clean(comment.Body, "Comment", 3, 2000);
            if (!Enum.IsDefined(comment.Category) || !Enum.IsDefined(comment.Priority) || !Enum.IsDefined(comment.Source) || !versions.TryGetValue(comment.VersionId, out var target) || target.Asset.Id != comment.AssetId) throw new ReviewValidationException("A comment refers to an invalid asset, version or category.");
            if (comment.Resolved != comment.ResolvedAt.HasValue) throw new ReviewValidationException("A comment has an inconsistent resolution record.");
            if (target.Asset.Kind == ReviewMediaKind.Image)
            {
                if (!UnitPoint(comment.X) || !UnitPoint(comment.Y) || comment.TimeSeconds is not null) throw new ReviewValidationException("Image comments require normalized x and y coordinates between zero and one.");
            }
            else if (comment.X is not null || comment.Y is not null || comment.TimeSeconds is not double time || !double.IsFinite(time) || time < 0 || time > target.Version.DurationSeconds.GetValueOrDefault() + .05) throw new ReviewValidationException("A video comment must be within the actual version's duration.");
        }
        Dictionary<Guid, ReviewDecisionState> states = [];
        foreach (var decision in project.Decisions)
        {
            if (decision is null) throw new ReviewValidationException("A decision record is missing.");
            Unique(ids, decision.Id); _ = Clean(decision.Note, "Decision note", 3, 1200);
            if (!Enum.IsDefined(decision.State) || !Enum.IsDefined(decision.Source) || !versions.TryGetValue(decision.VersionId, out var target) || target.Asset.Id != decision.AssetId || !Allowed(states.GetValueOrDefault(decision.VersionId), decision.State)) throw new ReviewValidationException("A decision history has an invalid reference or transition.");
            states[decision.VersionId] = decision.State;
        }
        if (project.Comments.Any(comment => !comment.Resolved && comment.Priority == ReviewCommentPriority.Blocking && states.GetValueOrDefault(comment.VersionId) is ReviewDecisionState.Approved or ReviewDecisionState.Delivered)) throw new ReviewValidationException("An approved or delivered version cannot have an open blocking comment.");
    }

    /// <summary>Applies an operation to a detached copy, so invalid input never corrupts the current document.</summary>
    static ReviewProject Change(ReviewProject project, Action<ReviewProject> action)
    {
        var next = Restore(Serialize(project));
        action(next);
        next.UpdatedAt = DateTimeOffset.UtcNow;
        _ = Serialize(next);
        return next;
    }

    /// <summary>Constructs a saved revision from actual browser metadata, excluding its ephemeral token.</summary>
    static ReviewVersion FromFile(ReviewPickedFile file, string label)
    {
        ValidateFile(file);
        return new ReviewVersion { Label = label, FileName = file.Name, MimeType = file.MimeType, SizeBytes = file.SizeBytes, Fingerprint = file.Fingerprint, Width = file.Width, Height = file.Height, DurationSeconds = file.DurationSeconds };
    }

    /// <summary>Checks browser metadata using the same document rules as restored versions.</summary>
    static void ValidateFile(ReviewPickedFile file)
    {
        if (string.IsNullOrEmpty(file.Token)) throw new ReviewValidationException("A local file handle is missing.");
        _ = Clean(file.Name, "File name", 1, 255);
        ValidateVersion(ParseKind(file.Kind), new ReviewVersion { FileName = file.Name, MimeType = file.MimeType, SizeBytes = file.SizeBytes, Fingerprint = file.Fingerprint, Width = file.Width, Height = file.Height, DurationSeconds = file.DurationSeconds });
    }

    /// <summary>Validates an actual media identity and finite metadata before it can be saved or rebound.</summary>
    static void ValidateVersion(ReviewMediaKind kind, ReviewVersion version)
    {
        if (version.SizeBytes is < 1 or > MaximumMediaBytes || version.Fingerprint is null || !Regex.IsMatch(version.Fingerprint, "^[a-f0-9]{64}$", RegexOptions.CultureInvariant)) throw new ReviewValidationException("A media version needs a valid SHA-256 fingerprint and a file no larger than 128 MB.");
        if (kind == ReviewMediaKind.Image)
        {
            if (!ImageTypes.Contains(version.MimeType) || version.Width is not > 0 or > 50000 || version.Height is not > 0 or > 50000 || version.DurationSeconds is not null) throw new ReviewValidationException("Invalid image type or dimensions.");
        }
        else if (!VideoTypes.Contains(version.MimeType) || version.DurationSeconds is not double duration || !double.IsFinite(duration) || duration <= 0 || duration > 86400 || version.Width is not > 0 or > 50000 || version.Height is not > 0 or > 50000) throw new ReviewValidationException("A video needs a supported video stream and a finite duration of at most 24 hours.");
    }

    /// <summary>Defines the complete permitted review state graph.</summary>
    static bool Allowed(ReviewDecisionState from, ReviewDecisionState to) => (from, to) switch { (ReviewDecisionState.Draft, ReviewDecisionState.Ready) => true, (ReviewDecisionState.Ready, ReviewDecisionState.Approved or ReviewDecisionState.ChangesRequested) => true, (ReviewDecisionState.ChangesRequested or ReviewDecisionState.Approved or ReviewDecisionState.Delivered, ReviewDecisionState.Ready) => true, (ReviewDecisionState.Approved, ReviewDecisionState.Delivered) => true, _ => false };
    /// <summary>Rejects missing identifiers and cross-record duplicate identities.</summary>
    static void Unique(HashSet<Guid> ids, Guid value) { if (value == Guid.Empty || !ids.Add(value)) throw new ReviewValidationException("Project records need distinct, non-empty identifiers."); }
    /// <summary>Accepts only finite normalized image coordinates.</summary>
    static bool UnitPoint(double? value) => value is double point && double.IsFinite(point) && point is >= 0 and <= 1;
    /// <summary>Resolves a known asset within its owning document.</summary>
    static ReviewAsset Asset(ReviewProject project, Guid id) => project.Assets.FirstOrDefault(item => item.Id == id) ?? throw new ReviewValidationException("The asset was not found.");
    /// <summary>Resolves a version only inside its declared asset.</summary>
    static ReviewVersion Version(ReviewAsset asset, Guid id) => asset.Versions.FirstOrDefault(item => item.Id == id) ?? throw new ReviewValidationException("The version does not belong to this asset.");
    /// <summary>Recognizes the browser metadata's explicit media family.</summary>
    static ReviewMediaKind ParseKind(string kind) => kind switch { "image" => ReviewMediaKind.Image, "video" => ReviewMediaKind.Video, _ => throw new ReviewValidationException("Choose a supported image or video.") };
    /// <summary>Bounds user-authored text without accepting control characters or interpreting markup.</summary>
    static string Clean(string? value, string label, int min, int max)
    {
        if (value is null) throw new ReviewValidationException($"{label} is missing.");
        var clean = value.Trim();
        if (clean.Length < min || clean.Length > max || clean.Any(character => char.IsControl(character) && character is not '\n' and not '\r' and not '\t')) throw new ReviewValidationException($"{label} must contain {min}–{max} characters without unsupported control characters.");
        return clean;
    }
}
