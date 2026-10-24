using System.Text.Json;
using System.Text.Json.Serialization;
namespace BinSite.Social.Models;

/// <summary>The exact reusable public schema; no run values, private notes or evidence belong here.</summary>
public sealed class ProcedureDocument
{
    [JsonRequired] public int FormatVersion { get; set; } = 1;
    [JsonRequired] public string Title { get; set; } = "";
    [JsonRequired] public string Summary { get; set; } = "";
    [JsonRequired] public string Category { get; set; } = "work";
    [JsonRequired] public List<string> Tags { get; set; } = [];
    [JsonRequired] public List<string> Inputs { get; set; } = [];
    [JsonRequired] public string Artifact { get; set; } = "";
    [JsonRequired] public List<ProcedureStep> Steps { get; set; } = [];
}
/// <summary>An actionable instruction and a separately observable completion check.</summary>
public sealed class ProcedureStep
{
    [JsonRequired] public string Id { get; set; } = "step_" + Guid.NewGuid().ToString("N");
    [JsonRequired] public string Title { get; set; } = "";
    [JsonRequired] public string Action { get; set; } = "";
    [JsonRequired] public string DoneWhen { get; set; } = "";
    [JsonRequired] public int Minutes { get; set; }
}
/// <summary>A published source revision or an explicitly labelled editorial starter.</summary>
public sealed class ProcedureSource
{
    public string Kind { get; set; } = "local";
    public string Id { get; set; } = "";
    public int Revision { get; set; }
    public string AuthorAlias { get; set; } = "";
    public string Title { get; set; } = "";
}
/// <summary>A known owner publication target; optimistic revision belongs to the real server record.</summary>
public sealed record ProcedurePublicationTarget(string Id,int Revision);
/// <summary>A person's local declaration, separate from the optional public outcome service.</summary>
public enum ProcedureRunState { Active, Completed, Partial, Blocked }
/// <summary>Private execution data for one immutable step snapshot.</summary>
public sealed class ProcedureRunStep
{
    [JsonRequired] public string StepId { get; set; } = "";
    [JsonRequired] public bool Done { get; set; }
    [JsonRequired] public string Note { get; set; } = "";
    [JsonRequired] public string Evidence { get; set; } = "";
}
/// <summary>A portable local run, pinned to its actual method snapshot rather than a mutable editor.</summary>
public sealed class ProcedureRun
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public ProcedureDocument Snapshot { get; set; } = new();
    [JsonRequired] public string SnapshotHash { get; set; } = "";
    [JsonRequired] public ProcedureSource Source { get; set; } = new();
    public ProcedureSource? DerivedFrom { get; set; }
    [JsonRequired] public List<string> InputValues { get; set; } = [];
    [JsonRequired] public List<ProcedureRunStep> Steps { get; set; } = [];
    [JsonRequired] public string ArtifactText { get; set; } = "";
    [JsonRequired] public string ResultNote { get; set; } = "";
    [JsonRequired] public ProcedureRunState State { get; set; }
    [JsonRequired] public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    [JsonRequired] public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
/// <summary>A complete private project; only Document may be projected into the public API.</summary>
public sealed class ProcedureWorkspace
{
    [JsonRequired] public int FileVersion { get; set; } = 1;
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public ProcedureDocument Document { get; set; } = new();
    [JsonRequired] public ProcedureSource Source { get; set; } = new();
    public ProcedurePublicationTarget? PublicationTarget { get; set; }
    public ProcedureDocument? ComparisonBase { get; set; }
    [JsonRequired] public List<ProcedureRun> Runs { get; set; } = [];
    public Guid? CurrentRunId { get; set; }
}
/// <summary>A reusable method package with source attribution and no private execution data.</summary>
public sealed class ProcedurePackage
{
    [JsonRequired] public int PackageVersion { get; set; } = 1;
    [JsonRequired] public ProcedureDocument Document { get; set; } = new();
    [JsonRequired] public ProcedureSource Source { get; set; } = new();
}
/// <summary>A clearly labelled original editorial procedure, not an invented user publication.</summary>
public sealed record ProcedureStarter(string Id,ProcedureDocument Document);
/// <summary>A real public directory record returned by the server.</summary>
public sealed record ProcedurePublicItem(string Id,string Title,string Summary,string Category,List<string> Tags,int Revision,string? ParentId,int? ParentRevision,string AuthorAlias,long CreatedAt,long UpdatedAt,bool Owned,bool Withdrawn,bool Hidden=false);
/// <summary>A paged public search result; no invented total is supplied.</summary>
public sealed record ProcedurePublicDirectory(List<ProcedurePublicItem> Items,bool HasMore);
/// <summary>An immutable published revision and its actual provenance and visibility flags.</summary>
public sealed record ProcedurePublished(string Id,int Revision,ProcedureDocument Document,string? ParentId,int? ParentRevision,string AuthorAlias,long CreatedAt,long UpdatedAt,bool Owned,bool Withdrawn,bool Hidden=false);
/// <summary>Actual account-submitted outcome counts, not inferred run success or verified people.</summary>
public sealed record ProcedureOutcomeCounts(int Completed,int Partial,int Blocked);
/// <summary>This account's explicit report for the selected published revision.</summary>
public sealed record ProcedureOutcome(string Kind,string Note);
/// <summary>A full public read response with actual outcomes and optional report availability.</summary>
public sealed record ProcedurePublicDetail(ProcedurePublished Playbook,ProcedureOutcomeCounts Outcomes,ProcedureOutcome? MyOutcome,bool? ReportsOpen=null);
/// <summary>The create/update response for one actual publication.</summary>
public sealed record ProcedurePublishedEnvelope(ProcedurePublished Playbook);
/// <summary>A server acknowledgement with no fabricated activity counters.</summary>
public sealed record ProcedureAcknowledgement(bool Ok);
/// <summary>An existing account's owner-only private vault header.</summary>
public sealed record ProcedureVaultHeader(string Id,string Title,string Kind,long UpdatedAt);
/// <summary>The actual owner's vault list.</summary>
public sealed record ProcedureVaultDirectory(List<ProcedureVaultHeader> Projects);
/// <summary>An owner-only private JSON project, without invitation or history promises.</summary>
public sealed record ProcedureVaultProject(string Id,string Title,string Kind,JsonElement Body,long UpdatedAt);
/// <summary>The private vault read response.</summary>
public sealed record ProcedureVaultEnvelope(ProcedureVaultProject Project);
/// <summary>The existing vault create/update response.</summary>
public sealed record ProcedureVaultSaved(string Id);
