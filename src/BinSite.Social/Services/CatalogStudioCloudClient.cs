using System.Text.Json;
using BinSite.Social.Models;

namespace BinSite.Social.Services;

/// <summary>Uses the existing authenticated API for revisioned catalog metadata without transferring local image bytes.</summary>
public sealed class CatalogStudioCloudClient(SocialApi api)
{
    const string Prefix = "commerce/workspaces";

    /// <summary>Loads real account limits and accessible workspace headers after an explicit user action.</summary>
    public Task<CatalogStudioCloudDirectory> ListAsync() => api.GetAsync<CatalogStudioCloudDirectory>(Prefix);

    /// <summary>Creates one metadata-only catalog under the account's server-enforced shared quota.</summary>
    public async Task<CatalogStudioCloudWorkspace> CreateAsync(CatalogStudioProject project, string? workspaceTitle = null)
    {
        var response = await api.PostAsync<CatalogStudioCloudEnvelope>(Prefix, new { kind = "catalog", title = Label(workspaceTitle ?? project.Title), body = Metadata(project) });
        return RequireCatalog(response.Workspace);
    }

    /// <summary>Reads one authorized catalog; server permissions remain authoritative.</summary>
    public async Task<CatalogStudioCloudWorkspace> OpenAsync(string id)
    {
        var response = await api.GetAsync<CatalogStudioCloudEnvelope>(Path(id));
        return RequireCatalog(response.Workspace);
    }

    /// <summary>Sends the known revision without retries; a stale revision preserves the caller's local changes.</summary>
    public async Task<CatalogStudioCloudWorkspace> SaveAsync(CatalogStudioCloudWorkspace workspace, CatalogStudioProject project, string? workspaceTitle = null)
    {
        if (workspace.Role is not ("owner" or "editor")) throw new InvalidOperationException("This workspace is read-only. Save a local project or create your own workspace.");
        var response = await api.PostAsync<CatalogStudioCloudEnvelope>(Path(workspace.Id), new { revision = workspace.Revision, title = Label(workspaceTitle ?? workspace.Title), body = Metadata(project) });
        return RequireCatalog(response.Workspace);
    }

    /// <summary>Creates a real owner-authorized viewer/editor invitation; it is never logged or sent to another person automatically.</summary>
    public Task<CatalogStudioCloudInvite> InviteAsync(CatalogStudioCloudWorkspace workspace, string role)
    {
        if (workspace.Role != "owner" || role is not ("viewer" or "editor")) throw new InvalidOperationException("Only the owner can invite a viewer or editor.");
        return api.PostAsync<CatalogStudioCloudInvite>(Path(workspace.Id) + "/invites", new { role });
    }

    /// <summary>Explicitly accepts a private membership token using the current account.</summary>
    public Task<CatalogStudioCloudJoin> JoinAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 300) throw new FormatException("Enter a valid workspace invitation token.");
        return api.PostAsync<CatalogStudioCloudJoin>(Prefix + "/join", new { token = token.Trim() });
    }

    /// <summary>Builds the exact cloud body and strips every image payload before the API receives it.</summary>
    public static JsonElement Metadata(CatalogStudioProject project)
    {
        using var document = JsonDocument.Parse(CatalogStudioWorkspace.Serialize(project, metadataOnly: true));
        return document.RootElement.Clone();
    }

    /// <summary>Restores product metadata and reattaches only photos already present on this device or bundled demo assets.</summary>
    public static CatalogStudioProject Rebind(JsonElement body, CatalogStudioProject local)
    {
        if (body.ValueKind != JsonValueKind.Object) throw new FormatException("The workspace does not contain a catalog project.");
        CatalogStudioProject project;
        try { project = body.Deserialize<CatalogStudioProject>(CatalogStudioWorkspace.JsonOptions) ?? throw new FormatException("The catalog metadata is empty."); }
        catch (JsonException error) { throw new FormatException("The workspace catalog metadata is invalid.", error); }
        if (project.Images is null) throw new FormatException("The workspace image references are invalid.");
        foreach (var image in project.Images)
        {
            if (image is null) throw new FormatException("The workspace contains an invalid image reference.");
            image.DataUrl = "";
        }
        CatalogStudioWorkspace.EnsureShape(project);
        var demo = CatalogStudioSamples.Create();
        foreach (var image in project.Images)
        {
            if (image.Fingerprint.Length == 0) continue;
            var attached = local.Images.FirstOrDefault(item => item.Fingerprint == image.Fingerprint && item.Attached)
                ?? demo.Images.FirstOrDefault(item => item.Fingerprint == image.Fingerprint);
            if (attached is null) continue;
            image.DataUrl = attached.DataUrl; image.Width = attached.Width; image.Height = attached.Height;
        }
        return project;
    }

    /// <summary>Rejects another product's workspace rather than interpreting its schema as catalog data.</summary>
    static CatalogStudioCloudWorkspace RequireCatalog(CatalogStudioCloudWorkspace workspace)
    {
        if (workspace.Kind != "catalog") throw new FormatException("This workspace belongs to another product.");
        return workspace;
    }

    /// <summary>Encodes a workspace identifier as one path segment.</summary>
    static string Path(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 100) throw new FormatException("The workspace identifier is invalid.");
        return Prefix + "/" + Uri.EscapeDataString(id);
    }

    /// <summary>Keeps the server's 100-character workspace label independent of the full document title.</summary>
    static string Label(string title)
    {
        var label = title.Trim();
        if (label.Length is < 1 or > 100) throw new FormatException("Use 1–100 characters for the shared project name.");
        return label;
    }
}
