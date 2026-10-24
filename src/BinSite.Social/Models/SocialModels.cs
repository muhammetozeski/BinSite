namespace BinSite.Social.Models;

/// <summary>A public story opening from the server directory.</summary>
public sealed record StorySummary(string Id, string Title, long CreatedAt, string Opening, int NodeCount);
/// <summary>A story title and creation time, without private author identifiers.</summary>
public sealed record StoryInfo(string Id, string Title, long CreatedAt);
/// <summary>A paragraph; hidden content is returned as null by the server.</summary>
public sealed record StoryNode(string Id, string? ParentId, string? Body, int Depth, long CreatedAt, int Hidden);
/// <summary>A complete bounded story graph returned by the API.</summary>
public sealed record StoryDetail(StoryInfo Story, List<StoryNode> Nodes);
/// <summary>A directory response containing only published stories.</summary>
public sealed record StoriesResponse(List<StorySummary> Stories);
/// <summary>A public two-option poll.</summary>
public sealed record PollInfo(string Id, string Question, string OptionA, string OptionB, long CreatedAt);
/// <summary>A directory poll with the actual recorded number of votes.</summary>
public sealed record PollSummary(string Id, string Question, string OptionA, string OptionB, long CreatedAt, int VoteCount);
/// <summary>A directory response containing visible polls.</summary>
public sealed record PollsResponse(List<PollSummary> Polls);
/// <summary>Server-aggregated option counts.</summary>
public sealed record VoteTotals(int A, int B);
/// <summary>A poll with aggregate results and this cookie identity's optional vote.</summary>
public sealed record PollDetail(PollInfo Poll, VoteTotals Totals, int? MyVote);
/// <summary>A capsule envelope; Body is null until the server unlock time.</summary>
public sealed record CapsuleInfo(string Id, string Title, long UnlockAt, long CreatedAt, string? Body);
/// <summary>A capsule response with the server's lock decision and current clock.</summary>
public sealed record CapsuleDetail(CapsuleInfo Capsule, bool Locked, long ServerNow);
/// <summary>An identifier returned after a successful create operation.</summary>
public sealed record CreatedResponse(string Id, string? NodeId = null, long? UnlockAt = null);
/// <summary>A report acknowledgement; it is not an assertion of human review.</summary>
public sealed record ReportResponse(bool Reported);
/// <summary>An authored opening offered as an unpublished writing suggestion.</summary>
public sealed record StoryPrompt(string Title, string Opening);
/// <summary>An authored poll suggestion, not a fabricated user poll.</summary>
public sealed record PollPrompt(string Question, string OptionA, string OptionB);
