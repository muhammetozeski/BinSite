namespace BinSite.Social.Models;

/// <summary>A shared canvas header whose revision changes for each accepted cell batch.</summary>
public sealed record CanvasInfo(string Id,string Title,int Revision);
/// <summary>A non-default cell currently stored on the shared canvas.</summary>
public sealed record PixelCell(int X,int Y,int Color,int Revision=0);
/// <summary>The actual bounded canvas directory.</summary>
public sealed record CanvasList(List<CanvasInfo> Canvases);
/// <summary>A canvas snapshot and its dimensions.</summary>
public sealed record CanvasDetail(CanvasInfo Canvas,List<PixelCell> Cells,int Width,int Height);
/// <summary>A debate proposition and its explicit context.</summary>
public sealed record DebateInfo(string Id,string Title,string Context);
/// <summary>A debate directory item with an actual argument count.</summary>
public sealed record DebateSummary(string Id,string Title,string Context,int ArgumentCount);
/// <summary>A directory of actual submitted propositions.</summary>
public sealed record DebateList(List<DebateSummary> Debates);
/// <summary>A structured argument with a peer rubric rather than a popularity vote.</summary>
public sealed record ArgumentInfo(string Id,string? ParentId,int Side,string Claim,string Reasoning,string EvidenceUrl,int Mine,int ReviewCount,double Clarity,double Evidence,int Reviewed);
/// <summary>A proposition and its bounded argument map.</summary>
public sealed record DebateDetail(DebateInfo Debate,List<ArgumentInfo> Arguments);
/// <summary>An editorial proposition offered only as an unpublished draft.</summary>
public sealed record DebatePrompt(string Title,string Context);
/// <summary>A public skill offer or request whose contact conversation remains private.</summary>
public sealed record ExchangeInfo(string Id,string Kind,string Title,string Skill,string Description,int Minutes,string Status,int Mine);
/// <summary>The public skill marketplace directory.</summary>
public sealed record ExchangeList(List<ExchangeInfo> Exchanges);
/// <summary>An exchange conversation visible only to its two browser identities.</summary>
public sealed record ThreadSummary(string Id,string ExchangeId,string Status,string Title,int IsOwner);
/// <summary>The current browser identity's private conversations.</summary>
public sealed record ThreadList(List<ThreadSummary> Threads);
/// <summary>A private exchange's authoritative state and two-party completion flags.</summary>
public sealed record ThreadInfo(string Id,string ExchangeId,string Status,int OwnerDone,int RequesterDone,string Title,string Skill,int Minutes,int IsOwner);
/// <summary>A private conversation message.</summary>
public sealed record ExchangeMessage(string Id,string Body,long CreatedAt,int Mine);
/// <summary>A private conversation with bounded message history.</summary>
public sealed record ThreadDetail(ThreadInfo Thread,List<ExchangeMessage> Messages);
/// <summary>A museum collection visible in the discovery directory.</summary>
public sealed record MuseumSummary(string Id,string Title,string Theme,string Description,int ObjectCount);
/// <summary>A directory of published museum collections.</summary>
public sealed record MuseumList(List<MuseumSummary> Museums);
/// <summary>A museum header with curator access and optimistic ordering revision.</summary>
public sealed record MuseumInfo(string Id,string Title,string Theme,string Description,int Revision,int Mine);
/// <summary>A contributed object story; pending items are visible only to their author and curator.</summary>
public sealed record MuseumObject(string Id,string Title,string Story,string Era,string Status,int Position,int Mine);
/// <summary>A museum and the objects this browser identity is permitted to see.</summary>
public sealed record MuseumDetail(MuseumInfo Museum,List<MuseumObject> Objects);
/// <summary>An authored example object, not a fabricated user contribution.</summary>
public sealed record EditorialObject(string Title,string Era,string Story);
/// <summary>An explicitly editorial read-only exhibition.</summary>
public sealed record EditorialExhibit(string Title,string Theme,string Description,EditorialObject[] Objects);
/// <summary>A cooperative room header; solution and other roles' clues are absent.</summary>
public sealed record RoomInfo(string Id,string Title,int Scenario,string Status,long CreatedAt,long ExpiresAt);
/// <summary>A room role occupancy without a private identity identifier.</summary>
public sealed record RoomPlayer(int Role,int Mine);
/// <summary>The current participant's one private role clue.</summary>
public sealed record RoomClue(int Role,string Clue);
/// <summary>A message visible only after joining the room.</summary>
public sealed record RoomMessage(string Id,int Role,string Body,long CreatedAt);
/// <summary>A room view filtered to the current browser identity.</summary>
public sealed record RoomDetail(RoomInfo Room,List<RoomPlayer> Players,RoomClue? Clue,List<RoomMessage> Messages,long ServerNow);
/// <summary>The authoritative answer result, without the stored solution hash.</summary>
public sealed record AnswerResult(bool Solved);
