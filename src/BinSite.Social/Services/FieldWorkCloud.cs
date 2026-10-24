using System.Text;
using System.Text.Json;

namespace BinSite.Social.Services;

/// <summary>Builds metadata-only cloud documents and reattaches only images already present in this browser.</summary>
public static class FieldWorkCloud
{
    /// <summary>Removes photo bytes and invalidates review where required images must be relinked by a recipient.</summary>
    public static JsonElement Metadata(FieldWorkWorkspace workspace)
    {
        FieldWorkRules.ValidateWorkspace(workspace);
        var copy = FieldWorkRules.Clone(workspace);
        RemoveImageData(copy);
        using var document = JsonDocument.Parse(FieldWorkRules.Serialize(copy));
        return document.RootElement.Clone();
    }

    /// <summary>Measures the actual UTF-8 metadata payload used by the authenticated workspace API.</summary>
    public static int MetadataBytes(FieldWorkWorkspace workspace) => Encoding.UTF8.GetByteCount(Metadata(workspace).GetRawText());

    /// <summary>Restores metadata and binds matching actual local images by original source fingerprint.</summary>
    public static FieldWorkCloudRestore Restore(JsonElement body, FieldWorkWorkspace local)
    {
        var restored = FieldWorkRules.Deserialize(body.GetRawText());
        RemoveImageData(restored);
        var localImages = local.Orders.SelectMany(order => order.Answers).Where(answer => answer.Photo is { } photo && FieldWorkRules.HasRasterEvidence(photo))
            .Select(answer => answer.Photo!).GroupBy(photo => photo.SourceSha256).ToDictionary(group => group.Key, group => group.First());
        var missing = 0;
        foreach (var answer in restored.Orders.SelectMany(order => order.Answers))
        {
            if (answer.Photo is not { } reference) continue;
            if (localImages.TryGetValue(reference.SourceSha256, out var actual)) answer.Photo = FieldWorkRules.Clone(actual);
            else missing++;
        }
        FieldWorkRules.ValidateWorkspace(restored);
        return new(restored, missing);
    }

    /// <summary>Strips every image payload, retaining fingerprints while keeping unbound required photos incomplete.</summary>
    static void RemoveImageData(FieldWorkWorkspace workspace)
    {
        foreach (var order in workspace.Orders)
        {
            foreach (var answer in order.Answers) if (answer.Photo is { } photo) photo.DataUrl = "";
            var unbound = order.Fields.Any(field => field.Kind == FieldWorkFieldKind.Photo && field.Required && order.Answers.FirstOrDefault(answer => answer.FieldId == field.Id)?.Photo is { } photo && string.IsNullOrEmpty(photo.DataUrl));
            if (unbound && order.Status is FieldWorkStatus.ReadyForReview or FieldWorkStatus.Complete) FieldWorkRules.Reopen(order);
        }
    }
}

/// <summary>Returns a restored project together with the number of images not available in this browser.</summary>
public sealed record FieldWorkCloudRestore(FieldWorkWorkspace Workspace, int PhotosToRelink);

/// <summary>Describes actual server access, storage quota, and saved workspace listings.</summary>
public sealed record FieldWorkCloudList(List<FieldWorkCloudSummary>? Workspaces, FieldWorkCloudAccount? Account);

/// <summary>Represents quota values returned by the authenticated server rather than a local premium flag.</summary>
public sealed record FieldWorkCloudAccount(bool Premium, int ProjectLimit, int UsedProjects, int MaxBodyBytes, int MemberLimit, int HistoryLimit);

/// <summary>Contains list metadata without private document content.</summary>
public sealed record FieldWorkCloudSummary(string Id, string Title, string Kind, int Revision, long UpdatedAt, string Role);

/// <summary>Wraps a versioned workspace response from the first-party API.</summary>
public sealed record FieldWorkCloudResponse(FieldWorkCloudDocument Workspace);

/// <summary>Stores a server-reported role and revision alongside the private metadata document.</summary>
public sealed record FieldWorkCloudDocument(string Id, string Title, string Kind, JsonElement Body, int Revision, string Role, bool Archived);

/// <summary>Returns a private, single-use invitation and its actual server expiry.</summary>
public sealed record FieldWorkCloudInvite(string Token, long ExpiresAt);

/// <summary>Identifies the workspace and assigned role after a real invitation is claimed.</summary>
public sealed record FieldWorkCloudJoined(string WorkspaceId, string Role);

/// <summary>Lists server-retained metadata revisions for an owner or editor.</summary>
public sealed record FieldWorkCloudHistory(List<FieldWorkCloudVersion>? Versions);

/// <summary>Contains revision metadata without pretending it is a local photo backup.</summary>
public sealed record FieldWorkCloudVersion(int Revision, string Title, long SavedAt);

/// <summary>Wraps an older saved metadata document for an explicit local restore.</summary>
public sealed record FieldWorkCloudVersionResponse(FieldWorkCloudVersionBody Version);

/// <summary>Contains the selected saved body and its historical revision.</summary>
public sealed record FieldWorkCloudVersionBody(int Revision, string Title, JsonElement Body, long SavedAt);
