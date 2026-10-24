using System.Text.Json.Serialization;
using System.Text.Json;

namespace BinSite.Social.Models;

/// <summary>Contains one portable catalog, including local image copies when explicitly downloaded.</summary>
public sealed class CatalogStudioProject
{
    public int FormatVersion { get; set; } = 1;
    public string Title { get; set; } = "My product catalog";
    public string Description { get; set; } = "";
    public string Currency { get; set; } = "USD";
    public string DecimalSeparator { get; set; } = ".";
    public List<CatalogStudioProduct> Products { get; set; } = [];
    public List<CatalogStudioImage> Images { get; set; } = [];
}

/// <summary>Retains source row and raw numeric fields so invalid imported values remain repairable.</summary>
public sealed class CatalogStudioProduct
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int SourceRow { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string Price { get; set; } = "";
    public string Stock { get; set; } = "";
    public string ImageHint { get; set; } = "";
    public string? ImageId { get; set; }
    public bool Included { get; set; } = true;
}

/// <summary>Stores an image only in the browser project; fingerprints can rebind metadata-only cloud projects.</summary>
public sealed class CatalogStudioImage
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string DataUrl { get; set; } = "";
    public string Fingerprint { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    [JsonIgnore] public bool Attached => DataUrl.Length > 0;
}

/// <summary>Identifies a repairable issue using the original CSV row and affected product or column.</summary>
public sealed record CatalogStudioIssue(int Row, string Field, string Message, bool IsError, string? ProductId = null);

/// <summary>Preserves a CSV record's physical starting line, including records with quoted newlines.</summary>
public sealed record CatalogStudioCsvRow(int Line, IReadOnlyList<string> Cells);

/// <summary>Returns a parsed table and structural errors without altering the active project.</summary>
public sealed record CatalogStudioCsvTable(char Delimiter, IReadOnlyList<string> Headers,
    IReadOnlyList<CatalogStudioCsvRow> Rows, IReadOnlyList<CatalogStudioIssue> Issues);

/// <summary>Maps catalog fields to zero-based source columns; minus one means unmapped.</summary>
public sealed class CatalogStudioColumnMap
{
    public int Sku { get; set; } = -1;
    public int Name { get; set; } = -1;
    public int Price { get; set; } = -1;
    public int Description { get; set; } = -1;
    public int Category { get; set; } = -1;
    public int Stock { get; set; } = -1;
    public int Image { get; set; } = -1;
}

/// <summary>Reports local file processing outcomes without sending image contents to a network service.</summary>
public sealed class CatalogStudioImageBatch
{
    public List<CatalogStudioImage> Images { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}

/// <summary>Transfers a user-selected text file from the browser without uploading it.</summary>
public sealed record CatalogStudioTextFile(string Name, string Text);

/// <summary>Represents a real server workspace revision; body contains metadata rather than local photos.</summary>
public sealed class CatalogStudioCloudWorkspace
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Role { get; set; } = "";
    public long Revision { get; set; }
    public long UpdatedAt { get; set; }
    public JsonElement Body { get; set; }
}

/// <summary>Shows the account's shared workspace quota returned by the server.</summary>
public sealed class CatalogStudioCloudAccount
{
    public bool Premium { get; set; }
    public int ProjectLimit { get; set; }
    public int UsedProjects { get; set; }
    public int MaxBodyBytes { get; set; }
    public int MemberLimit { get; set; }
    public int HistoryLimit { get; set; }
}

/// <summary>Contains actual account quotas and accessible workspace headers.</summary>
public sealed record CatalogStudioCloudDirectory(List<CatalogStudioCloudWorkspace> Workspaces, CatalogStudioCloudAccount Account);
/// <summary>Unwraps a server-created or updated catalog workspace.</summary>
public sealed record CatalogStudioCloudEnvelope(CatalogStudioCloudWorkspace Workspace);
/// <summary>Contains an owner-created private membership invitation.</summary>
public sealed record CatalogStudioCloudInvite(string Token, long ExpiresAt);
/// <summary>Identifies the workspace reached by explicitly accepting an invitation.</summary>
public sealed record CatalogStudioCloudJoin(string WorkspaceId, string Role);
