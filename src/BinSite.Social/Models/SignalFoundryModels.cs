using BinSite.Social.Services;

namespace BinSite.Social.Models;

/// <summary>A visible, deterministic experiment in the sf-v1 board language.</summary>
public sealed class SignalObservation
{
    public int[] Input { get; set; } = new int[9];
    public string Program { get; set; } = "A";
    public int[] Output { get; set; } = new int[9];
}

/// <summary>A board and program whose result must be reconstructed.</summary>
public sealed class SignalTarget
{
    public int[] Input { get; set; } = new int[9];
    public string Program { get; set; } = "A";
}

/// <summary>Public challenge data deliberately excludes its hidden operation mapping.</summary>
public sealed class SignalChallenge
{
    public string Id { get; set; } = "";
    public string EngineVersion { get; set; } = SignalFoundryEngine.Version;
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Hint { get; set; } = "";
    public List<SignalObservation> Examples { get; set; } = [];
    public SignalTarget Target { get; set; } = new();
    public int QueryLimit { get; set; } = 2;
    public string? ParentId { get; set; }
    public string AuthorAlias { get; set; } = "Editorial";
    public bool AllowReuse { get; set; }
    public bool Owned { get; set; }
    public bool Withdrawn { get; set; }
    public long CreatedAt { get; set; }
}

/// <summary>A private local author document; its exported JSON contains the answer key.</summary>
public sealed class SignalAuthorProject
{
    public string EngineVersion { get; set; } = SignalFoundryEngine.Version;
    public string Title { get; set; } = "A language worth testing";
    public string Description { get; set; } = "Find the rule behind four stamps, then reconstruct the final delivery.";
    public string AuthorAlias { get; set; } = "Independent maker";
    public int[] Mapping { get; set; } = [0, 4, 6, 7];
    public List<SignalTarget> Examples { get; set; } = [new() { Input = [1, 0, 2, 0, 3, 1, 2, 0, 0], Program = "A" }];
    public SignalTarget Target { get; set; } = new() { Input = [0, 2, 1, 3, 0, 0, 1, 0, 2], Program = "AB" };
    public int QueryLimit { get; set; } = 2;
    public string? ParentId { get; set; }
    public bool AllowReuse { get; set; } = true;
}

/// <summary>Server-owned guest attempt state; observations contain purchased experiments only.</summary>
public sealed class SignalAttempt
{
    public string AttemptId { get; set; } = "";
    public string AttemptKey { get; set; } = "";
    public SignalChallenge Challenge { get; set; } = new();
    public List<SignalObservation> Observations { get; set; } = [];
    public int QueriesRemaining { get; set; }
    public bool Finished { get; set; }
    public long ExpiresAt { get; set; }
    public SignalAssessment? Result { get; set; }
}

/// <summary>Fairness assessment separates an identifiable result from another consistent interpretation.</summary>
public sealed class SignalAssessment
{
    public string Verdict { get; set; } = "inconsistent";
    public bool ActualMatch { get; set; }
    public int CandidateCount { get; set; }
    public int TargetOutcomeCount { get; set; }
    public int[] ActualOutput { get; set; } = new int[9];
    public int[]? Answer { get; set; }
    public int[] Mapping { get; set; } = [];
    public string Explanation { get; set; } = "";
}

/// <summary>An immutable mapping prevents a caller from modifying the shared 1,680-candidate family.</summary>
public readonly record struct SignalMapping(int A, int B, int C, int D)
{
    public int this[int stamp] => stamp switch { 0 => A, 1 => B, 2 => C, 3 => D, _ => throw new ArgumentOutOfRangeException(nameof(stamp)) };
    public int[] ToArray() => [A, B, C, D];
}

public sealed record SignalOutcome(int[] Board, int MappingCount);
public sealed record SignalAnalysis(IReadOnlyList<SignalMapping> Candidates, IReadOnlyList<SignalOutcome> Outcomes)
{
    public int CandidateCount => Candidates.Count;
    public int TargetOutcomeCount => Outcomes.Count;
}

public sealed class SignalQueryResult
{
    public SignalObservation Observation { get; set; } = new();
    public int QueriesRemaining { get; set; }
    public int CandidateCount { get; set; }
    public int TargetOutcomeCount { get; set; }
}
