using System.Text;
using System.Text.Json;
namespace BinSite.Social.Services;

/// <summary>Optional owner-only roster snapshots in the existing account vault, without collaborative workspace promises.</summary>
public sealed class ShiftCoverVault(SocialApi api)
{
    public const int MaximumBodyBytes = 65536;
    const string Prefix = "commerce/projects";

    /// <summary>Lists records that the authenticated account actually owns.</summary>
    public Task<ShiftCoverVaultList> ListAsync() => api.GetAsync<ShiftCoverVaultList>(Prefix);

    /// <summary>Saves a new independent snapshot using an explicit 100-character outer label and 64 KiB body limit.</summary>
    public async Task<ShiftCoverVaultSaved> SaveAsync(ShiftCoverProject project, string label)
    {
        if (string.IsNullOrWhiteSpace(label) || label.Trim().Length > 100 || label.Any(char.IsControl)) throw new InvalidOperationException("Use a snapshot name between 1 and 100 characters, without special control characters.");
        var json = ShiftCoverRules.Serialize(project);
        if (Encoding.UTF8.GetByteCount(json) > MaximumBodyBytes) throw new InvalidOperationException("This roster exceeds the 64 KiB snapshot limit. Save a project file or use a shorter planning period. Membership adds more snapshots; it does not increase this file limit.");
        using var document = JsonDocument.Parse(json);
        return await api.PostAsync<ShiftCoverVaultSaved>(Prefix, new { title = label.Trim(), kind = "roster", body = document.RootElement.Clone() });
    }

    /// <summary>Reads and validates a complete owner-only roster before the editor considers replacing local state.</summary>
    public async Task<ShiftCoverProject> LoadAsync(string id)
    {
        var result = await api.GetAsync<ShiftCoverVaultEnvelope>(Prefix + "/" + Uri.EscapeDataString(id));
        if (result.Project.Kind != "roster") throw new InvalidOperationException("This saved snapshot is not a Shift & Cover roster.");
        return ShiftCoverRules.Restore(result.Project.Body.GetRawText());
    }

    /// <summary>Deletes only a server-authorized owned snapshot after the caller's explicit confirmation.</summary>
    public Task<JsonElement> DeleteAsync(string id) => api.PostAsync<JsonElement>(Prefix + "/" + Uri.EscapeDataString(id) + "/delete", new { });
}
