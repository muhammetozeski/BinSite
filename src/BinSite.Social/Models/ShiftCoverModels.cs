using System.Text.Json;
using System.Text.Json.Serialization;
namespace BinSite.Social.Models;

/// <summary>A portable local roster; wall-clock values share one explicitly selected fixed UTC offset.</summary>
public sealed class ShiftCoverProject
{
    [JsonRequired] public int FormatVersion { get; set; } = 1;
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Title { get; set; } = "Untitled roster";
    [JsonRequired] public string Notes { get; set; } = "";
    [JsonRequired] public string ExampleOrigin { get; set; } = "";
    [JsonRequired] public int UtcOffsetMinutes { get; set; }
    [JsonRequired] public int Revision { get; set; } = 1;
    [JsonRequired] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [JsonRequired] public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    [JsonRequired] public List<ShiftCoverRole> Roles { get; set; } = [];
    [JsonRequired] public List<ShiftCoverPerson> People { get; set; } = [];
    [JsonRequired] public List<ShiftCoverShift> Shifts { get; set; } = [];
    [JsonRequired] public List<ShiftCoverAssignment> Assignments { get; set; } = [];
}

/// <summary>A user-defined capability required by one or more shift seats.</summary>
public sealed class ShiftCoverRole
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Name { get; set; } = "";
}

/// <summary>Organizer-entered availability and limits, without implying a person's consent or legal compliance.</summary>
public sealed class ShiftCoverPerson
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Label { get; set; } = "";
    [JsonRequired] public List<Guid> RoleIds { get; set; } = [];
    [JsonRequired] public decimal MaximumTotalHours { get; set; } = 24;
    [JsonRequired] public decimal MaximumShiftHours { get; set; } = 8;
    [JsonRequired] public int MinimumRestMinutes { get; set; } = 480;
    [JsonRequired] public List<ShiftCoverWindow> Availability { get; set; } = [];
}

/// <summary>A dated minute-resolution window in the project's fixed-offset wall clock.</summary>
public sealed class ShiftCoverWindow
{
    [JsonRequired] public string Start { get; set; } = "";
    [JsonRequired] public string End { get; set; } = "";
}

/// <summary>A role, dated interval and explicit number of independent required seats.</summary>
public sealed class ShiftCoverShift
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Title { get; set; } = "";
    [JsonRequired] public Guid RoleId { get; set; }
    [JsonRequired] public string Start { get; set; } = "";
    [JsonRequired] public string End { get; set; } = "";
    [JsonRequired] public int RequiredPeople { get; set; } = 1;
}

/// <summary>One person assigned to one numbered seat; locking preserves the assignment during automatic suggestions.</summary>
public sealed class ShiftCoverAssignment
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public Guid ShiftId { get; set; }
    [JsonRequired] public int Seat { get; set; }
    [JsonRequired] public Guid PersonId { get; set; }
    [JsonRequired] public bool Locked { get; set; }
}

/// <summary>A failed actual constraint for a candidate or an existing assignment.</summary>
public sealed record ShiftCoverIssue(string Code, string Message, Guid? AssignmentId = null);
/// <summary>A candidate's actual current eligibility after removing any occupant of the selected seat.</summary>
public sealed record ShiftCoverCandidate(Guid PersonId, string Label, decimal AssignedHours, IReadOnlyList<ShiftCoverIssue> Issues);
/// <summary>A bounded deterministic search result; coverage is measured, not an optimality or infeasibility proof.</summary>
public sealed record ShiftCoverPlan(ShiftCoverProject Project, int CoveredSeats, int RequiredSeats, int Passes, int CandidateChecks, bool BudgetReached);
/// <summary>An English fictional example descriptor; opening it produces editable local data.</summary>
public sealed record ShiftCoverExample(string Id, string Title, string Description);
/// <summary>Owner-only legacy vault metadata returned by the actual account service.</summary>
public sealed record ShiftCoverVaultItem(string Id, string Title, string Kind, long UpdatedAt);
/// <summary>The actual authenticated legacy project list.</summary>
public sealed record ShiftCoverVaultList(List<ShiftCoverVaultItem> Projects);
/// <summary>A full owner-only saved roster JSON document.</summary>
public sealed record ShiftCoverVaultProject(string Id, string Title, string Kind, JsonElement Body);
/// <summary>A typed legacy vault read response.</summary>
public sealed record ShiftCoverVaultEnvelope(ShiftCoverVaultProject Project);
/// <summary>The identifier acknowledged by a real create or update.</summary>
public sealed record ShiftCoverVaultSaved(string Id);
