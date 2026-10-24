using System.Text.Json;
namespace BinSite.Social.Services;

/// <summary>Optional real-account synchronization of review JSON; local media bytes are never passed to this client.</summary>
public sealed class ReviewRoomCloudClient(SocialApi api)
{
    const string Prefix = "commerce/workspaces";

    /// <summary>Lists only metadata that the actual account session is authorized to access.</summary>
    public Task<ReviewCloudList> ListAsync() => api.GetAsync<ReviewCloudList>(Prefix);

    /// <summary>Reads an actual workspace and checks that it belongs to this product.</summary>
    public async Task<ReviewCloudWorkspace> LoadAsync(string id)
    {
        var result = await api.GetAsync<ReviewCloudEnvelope>(Prefix + "/" + Uri.EscapeDataString(id));
        EnsureReview(result.Workspace);
        return result.Workspace;
    }

    /// <summary>Creates or revision-checks a metadata-only workspace using the server's account, role and quota controls.</summary>
    public async Task<ReviewCloudWorkspace> SaveAsync(ReviewProject project, ReviewCloudWorkspace? current, string workspaceLabel)
    {
        var label = workspaceLabel.Trim();
        if (label.Length is < 1 or > 100) throw new ReviewValidationException("Use a cloud workspace label of 1–100 characters. The full local project title remains unchanged.");
        using var document = JsonDocument.Parse(ReviewRoomRules.Serialize(project));
        var body = document.RootElement.Clone();
        ReviewCloudEnvelope result;
        if (current is null) result = await api.PostAsync<ReviewCloudEnvelope>(Prefix, new { kind = "review", title = label, body });
        else
        {
            EnsureReview(current);
            if (current.Role is not "owner" and not "editor") throw new ReviewValidationException("This cloud workspace is read-only for your account. Make a local copy to edit your own document.");
            result = await api.PostAsync<ReviewCloudEnvelope>(Prefix + "/" + Uri.EscapeDataString(current.Id), new { revision = current.Revision, title = label, body });
        }
        EnsureReview(result.Workspace);
        return result.Workspace;
    }

    /// <summary>Requests a viewer/editor invitation only for a server-authorized owner.</summary>
    public Task<ReviewCloudInvite> InviteAsync(ReviewCloudWorkspace workspace, string role)
    {
        EnsureReview(workspace);
        if (workspace.Role != "owner" || role is not "viewer" and not "editor") throw new ReviewValidationException("Only the workspace owner can create a viewer or editor invitation.");
        return api.PostAsync<ReviewCloudInvite>(Prefix + "/" + Uri.EscapeDataString(workspace.Id) + "/invites", new { role });
    }

    /// <summary>Accepts an invitation through the actual account session without pretending to sign anyone in.</summary>
    public Task<ReviewCloudJoin> JoinAsync(string token) => api.PostAsync<ReviewCloudJoin>(Prefix + "/join", new { token });

    /// <summary>Rejects a different product kind before treating its JSON as a review document.</summary>
    static void EnsureReview(ReviewCloudWorkspace workspace)
    {
        if (workspace.Kind != "review" || workspace.Role is not "owner" and not "editor" and not "viewer") throw new ReviewValidationException("This is not a supported Review Room workspace.");
    }
}
