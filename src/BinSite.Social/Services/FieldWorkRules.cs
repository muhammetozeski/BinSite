using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BinSite.Social.Services;

/// <summary>Owns work-order validation, workflow transitions, and the portable project format.</summary>
public static class FieldWorkRules
{
    public const int MaximumProjectBytes = 25_000_000;
    public const int MaximumPhotoBytes = 1_500_000;
    public const int MaximumOrders = 30;
    public const int MaximumFields = 40;
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        MaxDepth = 32,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    /// <summary>Returns a stable English label for the persisted lifecycle value.</summary>
    public static string StatusLabel(FieldWorkStatus status) => status switch
    {
        FieldWorkStatus.Plan => "Plan",
        FieldWorkStatus.InProgress => "In progress",
        FieldWorkStatus.ReadyForReview => "Ready for review",
        FieldWorkStatus.Complete => "Complete",
        _ => "Unknown"
    };

    /// <summary>Parses an English decimal input without silently converting invalid input to zero.</summary>
    public static bool ReadNumber(string? text, out decimal value) => decimal.TryParse(text,
        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
        CultureInfo.InvariantCulture, out value);

    /// <summary>Checks checklist labels, unique field identities, and measurement bounds.</summary>
    public static List<FieldWorkIssue> DefinitionIssues(string title, IReadOnlyList<FieldWorkField> fields)
    {
        List<FieldWorkIssue> issues = [];
        if (string.IsNullOrWhiteSpace(title)) issues.Add(new(null, "Checklist title", "Enter a title."));
        if (title?.Length > 120) issues.Add(new(null, "Checklist title", "Use at most 120 characters."));
        if (fields.Count is < 1 or > MaximumFields) issues.Add(new(null, "Checklist", $"Use 1–{MaximumFields} fields."));
        HashSet<Guid> ids = [];
        foreach (var field in fields)
        {
            if (field is null) { issues.Add(new(null, "Checklist", "A field is missing.")); continue; }
            if (field.Id == Guid.Empty || !ids.Add(field.Id)) issues.Add(new(field.Id, "Field identity", "Every field must have a unique identity."));
            if (string.IsNullOrWhiteSpace(field.Label) || field.Label.Length > 160) issues.Add(new(field.Id, "Field label", "Use 1–160 characters."));
            if (!Enum.IsDefined(field.Kind)) issues.Add(new(field.Id, field.Label, "Unknown field type."));
            if (field.Instructions is null || field.Instructions.Length > 1000 || field.Unit is null || field.Unit.Length > 30) issues.Add(new(field.Id, field.Label, "Instructions or unit exceed their limit."));
            if (field.Kind == FieldWorkFieldKind.Measurement && field.Minimum > field.Maximum) issues.Add(new(field.Id, field.Label, "Minimum cannot exceed maximum."));
        }
        return issues;
    }

    /// <summary>Checks actual answers, including present image bytes; filenames never satisfy a photo field.</summary>
    public static List<FieldWorkIssue> Issues(FieldWorkOrder order)
    {
        var issues = DefinitionIssues(order.TemplateTitle, order.Fields);
        if (string.IsNullOrWhiteSpace(order.Title)) issues.Add(new(null, "Work order", "Enter a work-order title."));
        foreach (var field in order.Fields)
        {
            var answer = order.Answers.FirstOrDefault(value => value.FieldId == field.Id);
            if (field.Kind == FieldWorkFieldKind.Confirmation)
            {
                if (field.Required && answer?.Confirmed != true) issues.Add(new(field.Id, field.Label, "Confirmation is required."));
            }
            else if (field.Kind == FieldWorkFieldKind.Measurement)
            {
                var raw = answer?.NumberText;
                if (string.IsNullOrWhiteSpace(raw)) { if (field.Required) issues.Add(new(field.Id, field.Label, "Enter the measurement.")); continue; }
                if (!ReadNumber(raw, out var value)) { issues.Add(new(field.Id, field.Label, "Enter a decimal number using a dot.")); continue; }
                if (field.Minimum is { } minimum && value < minimum) issues.Add(new(field.Id, field.Label, $"Must be at least {minimum.ToString(CultureInfo.InvariantCulture)} {field.Unit}."));
                if (field.Maximum is { } maximum && value > maximum) issues.Add(new(field.Id, field.Label, $"Must be at most {maximum.ToString(CultureInfo.InvariantCulture)} {field.Unit}."));
            }
            else if (field.Kind == FieldWorkFieldKind.Photo)
            {
                if (answer?.Photo is null || string.IsNullOrEmpty(answer.Photo.DataUrl))
                {
                    if (field.Required) issues.Add(new(field.Id, field.Label, answer?.Photo is null ? "Attach a photo." : "Choose the original photo on this device."));
                }
                else if (!HasRasterEvidence(answer.Photo)) issues.Add(new(field.Id, field.Label, "The attached image data is invalid. Replace the photo."));
            }
        }
        return issues;
    }

    /// <summary>Checks bounded embedded raster bytes; browser decoding additionally validates imports and selections.</summary>
    public static bool HasRasterEvidence(FieldWorkPhoto photo)
    {
        if (photo.Width is < 1 or > 4096 || photo.Height is < 1 or > 4096 || photo.FileName is null || photo.FileName.Length > 200) return false;
        if (!Regex.IsMatch(photo.SourceSha256 ?? "", "^[0-9a-f]{64}$")) return false;
        var prefix = photo.ContentType switch { "image/jpeg" => "data:image/jpeg;base64,", "image/png" => "data:image/png;base64,", "image/webp" => "data:image/webp;base64,", _ => "" };
        if (prefix.Length == 0 || photo.DataUrl is null || !photo.DataUrl.StartsWith(prefix, StringComparison.Ordinal) || photo.DataUrl.Length > MaximumPhotoBytes * 4 / 3 + 100) return false;
        byte[] bytes;
        try { bytes = Convert.FromBase64String(photo.DataUrl[prefix.Length..]); }
        catch (FormatException) { return false; }
        if (bytes.Length is < 24 or > MaximumPhotoBytes || bytes.Length != photo.ByteLength) return false;
        return photo.ContentType switch
        {
            "image/jpeg" => bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255 && bytes[^2] == 255 && bytes[^1] == 217,
            "image/png" => bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "image/webp" => Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP",
            _ => false
        };
    }

    /// <summary>Creates independent order definitions and empty answers from the selected template revision.</summary>
    public static FieldWorkOrder CreateOrder(FieldWorkTemplate template, string title = "New work order")
    {
        var issues = DefinitionIssues(template.Title, template.Fields);
        if (issues.Count > 0) throw new InvalidOperationException(issues[0].Message);
        var order = new FieldWorkOrder
        {
            Title = title, TemplateId = template.Id, TemplateTitle = template.Title, TemplateRevision = template.Revision,
            Fields = template.Fields.Select(field => Clone(field)).ToList()
        };
        order.Answers = order.Fields.Select(field => new FieldWorkAnswer { FieldId = field.Id }).ToList();
        Record(order, "Created", $"Checklist: {template.Title}, version {template.Revision}.");
        return order;
    }

    /// <summary>Advances exactly one lifecycle step, refusing review or completion while requirements remain unresolved.</summary>
    public static FieldWorkTransition Advance(FieldWorkOrder order)
    {
        var issues = order.Status == FieldWorkStatus.Plan ? DefinitionIssues(order.TemplateTitle, order.Fields) : Issues(order);
        if (string.IsNullOrWhiteSpace(order.Title)) issues.Add(new(null, "Work order", "Enter a work-order title."));
        if (order.Status == FieldWorkStatus.Complete) issues.Add(new(null, "Status", "Reopen the order before making another change."));
        if (issues.Count > 0) return new(false, issues);
        order.Status = order.Status switch
        {
            FieldWorkStatus.Plan => FieldWorkStatus.InProgress,
            FieldWorkStatus.InProgress => FieldWorkStatus.ReadyForReview,
            FieldWorkStatus.ReadyForReview => FieldWorkStatus.Complete,
            _ => order.Status
        };
        order.CompletedAt = order.Status == FieldWorkStatus.Complete ? DateTimeOffset.UtcNow : null;
        Record(order, StatusLabel(order.Status), order.Status == FieldWorkStatus.Complete ? "Review recorded." : order.Status == FieldWorkStatus.ReadyForReview ? "All required items are filled." : "Work started.");
        return new(true, []);
    }

    /// <summary>Returns a reviewed or completed order to editable work without deleting its answers or history.</summary>
    public static void Reopen(FieldWorkOrder order)
    {
        order.Status = FieldWorkStatus.InProgress;
        order.CompletedAt = null;
        Record(order, "Reopened", "Review and completion must be performed again after changes.");
    }

    /// <summary>Updates the local revision and modification time after an editable change.</summary>
    public static void Touch(FieldWorkOrder order)
    {
        if (order.Status is FieldWorkStatus.ReadyForReview or FieldWorkStatus.Complete) Reopen(order);
        else { order.Revision++; order.UpdatedAt = DateTimeOffset.UtcNow; }
    }

    /// <summary>Serializes the entire local project, including selected image bytes, with a bounded portable format.</summary>
    public static string Serialize(FieldWorkWorkspace workspace)
    {
        ValidateWorkspace(workspace);
        var json = JsonSerializer.Serialize(workspace, JsonOptions);
        if (Encoding.UTF8.GetByteCount(json) > MaximumProjectBytes) throw new InvalidOperationException("This project exceeds 25 MB. Export separate work orders or remove unused photos.");
        return json;
    }

    /// <summary>Rejects malformed imports before any current local project is replaced.</summary>
    public static FieldWorkWorkspace Deserialize(string json)
    {
        if (Encoding.UTF8.GetByteCount(json) > MaximumProjectBytes) throw new InvalidOperationException("Project files may be at most 25 MB.");
        var workspace = JsonSerializer.Deserialize<FieldWorkWorkspace>(json, JsonOptions) ?? throw new InvalidOperationException("The project file is empty.");
        ValidateWorkspace(workspace);
        return workspace;
    }

    /// <summary>Copies a definition or project using the same schema as portable exports.</summary>
    public static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions) ?? throw new InvalidOperationException("The item could not be copied.");

    /// <summary>Checks collection bounds, nested identities, stored text, and review-state consistency.</summary>
    public static void ValidateWorkspace(FieldWorkWorkspace workspace)
    {
        if (workspace.Version != 1 || workspace.Title is null || workspace.Title.Length > 120 || workspace.Templates is null || workspace.Orders is null || workspace.Templates.Count > 50 || workspace.Orders.Count > MaximumOrders) throw new InvalidOperationException("Unsupported or oversized Field Work Pack project.");
        if (workspace.Templates.Any(template => template is null) || workspace.Orders.Any(order => order is null)) throw new InvalidOperationException("A project item is missing.");
        if (workspace.Templates.Select(template => template.Id).Distinct().Count() != workspace.Templates.Count || workspace.Orders.Select(order => order.Id).Distinct().Count() != workspace.Orders.Count) throw new InvalidOperationException("Duplicate template or work-order identity.");
        foreach (var template in workspace.Templates)
        {
            if (template.Id == Guid.Empty || template.Revision < 1 || template.Description is null || template.Description.Length > 2000 || template.Fields is null) throw new InvalidOperationException("Invalid checklist metadata.");
            var issues = DefinitionIssues(template.Title, template.Fields);
            if (issues.Count > 0) throw new InvalidOperationException($"Checklist {template.Title}: {issues[0].Message}");
        }
        foreach (var order in workspace.Orders)
        {
            if (order.Id == Guid.Empty || order.Fields is null || order.Answers is null || order.History is null || order.History.Count > 200 || !Enum.IsDefined(order.Status)) throw new InvalidOperationException("Invalid work-order structure.");
            if (new[] { order.Title, order.Reference, order.CustomerLabel, order.ReviewerLabel, order.TemplateTitle }.Any(value => value is null || value.Length > 160) || order.Notes is null || order.Notes.Length > 6000) throw new InvalidOperationException("A work-order label or note exceeds its limit.");
            if (order.Answers.Count != order.Fields.Count || order.Answers.Any(answer => answer is null || answer.NumberText is null || answer.NumberText.Length > 80 || answer.Note is null || answer.Note.Length > 2000) || order.Answers.Select(answer => answer.FieldId).Distinct().Count() != order.Answers.Count || order.Answers.Any(answer => !order.Fields.Any(field => field.Id == answer.FieldId))) throw new InvalidOperationException("Answers do not match the checklist.");
            if (order.History.Any(entry => entry is null || entry.Action is null || entry.Action.Length > 100 || entry.Detail is null || entry.Detail.Length > 500)) throw new InvalidOperationException("Invalid workflow history.");
            var definitions = DefinitionIssues(order.TemplateTitle, order.Fields);
            if (definitions.Count > 0) throw new InvalidOperationException(definitions[0].Message);
            foreach (var photo in order.Answers.Where(answer => answer.Photo is not null).Select(answer => answer.Photo!))
                if (!string.IsNullOrEmpty(photo.DataUrl) && !HasRasterEvidence(photo)) throw new InvalidOperationException("An embedded photo is invalid. The current workspace was not replaced.");
            if (order.Status is FieldWorkStatus.ReadyForReview or FieldWorkStatus.Complete && Issues(order).Count > 0) throw new InvalidOperationException("A reviewed or completed order contains unresolved evidence requirements.");
        }
    }

    /// <summary>Appends a bounded local audit entry and updates the order revision.</summary>
    static void Record(FieldWorkOrder order, string action, string detail)
    {
        order.Revision++; order.UpdatedAt = DateTimeOffset.UtcNow;
        order.History.Add(new() { Action = action, Detail = detail, At = order.UpdatedAt });
        if (order.History.Count > 200) order.History.RemoveAt(0);
    }
}
