using System.Text.Json;
namespace BinSite.Social.Models;
/// <summary>A typed server-filtered collaboration view with its authoritative revision and current role.</summary>
public sealed record Space<T>(string Id,string Title,int Revision,long CreatedAt,int Mine,string? MemberId,T State,JsonElement? Feedback);
/// <summary>A bounded directory item without user identity or private state.</summary>
public sealed record SpaceItem(string Id,string Title,int Revision,long CreatedAt);
/// <summary>A directory response for one product's workspaces.</summary>
public sealed record SpaceList(List<SpaceItem> Items);
/// <summary>A public member handle without its cookie hash.</summary>
public sealed record Participant(string Id,string Name,int Progress=0);
/// <summary>An elimination match whose result can only be confirmed by its organizer.</summary>
public sealed record TournamentMatch(string Id,int Round,int Slot,string? A,string? B,string? Winner);
/// <summary>An actual tournament bracket and participant roster.</summary>
public sealed record TournamentState(int Capacity,string Status,List<Participant> Members,List<TournamentMatch> Matches,string? Champion);
/// <summary>A chapter-specific club contribution.</summary>
public sealed record ChapterComment(string Id,int Chapter,string MemberId,string Body,long CreatedAt);
/// <summary>A reading club with self-reported progress and chapter discussion.</summary>
public sealed record ReadingState(int Book,List<Participant> Members,List<ChapterComment> Discussion);
/// <summary>An original authored short fiction, not fabricated user activity.</summary>
public sealed record FictionBook(string Title,string Description,string[] Chapters);
/// <summary>A lesson view with correct answers withheld from non-authors and explanations gated by completion.</summary>
public sealed record LessonInfo(string Id,string Title,string Text,string Question,string[] Options,int? Answer,string? Explanation);
/// <summary>A learner-filtered course state.</summary>
public sealed record AcademyState(string Intro,string Status,int StudentCount,List<string> Completed,List<LessonInfo> Lessons);
/// <summary>An explicitly editorial mini-lesson template for an author's unpublished draft.</summary>
public sealed record LessonDraft(string Title,string Text,string Question,string[] Options,int Answer,string Explanation);
/// <summary>A recorded stage completion in a relay team.</summary>
public sealed record RelayStep(int Stage,string MemberId,long At);
/// <summary>An asynchronous shared quiz with one current flag holder and hidden future stages.</summary>
public sealed record RelayState(List<Participant> Members,string Status,int Stage,string Runner,List<RelayStep> History,string? Question);
/// <summary>A volunteer task delivery.</summary>
public sealed record JamSubmission(string Body,string Link,long At);
/// <summary>A task with claim, submit and owner-review transitions.</summary>
public sealed record JamTask(string Id,string Title,string Spec,string Status,string? Assignee,JamSubmission? Submission,string Review);
/// <summary>A time-bounded creative collaboration project.</summary>
public sealed record JamState(string Theme,string Brief,long Deadline,List<Participant> Members,List<JamTask> Tasks);
/// <summary>A measured ingredient whose unit is preserved during group aggregation.</summary>
public sealed record RecipeIngredient(string Name,string Unit,double Quantity);
/// <summary>A branch in an actual recipe version tree.</summary>
public sealed record RecipeVersion(string Id,string? ParentId,string Title,int Servings,List<RecipeIngredient> Ingredients,string Steps,string MemberId,long At);
/// <summary>One participant's selected version and desired serving count.</summary>
public sealed record RecipePlan(string MemberId,string VersionId,int Servings);
/// <summary>Shared recipe branches and server-computed group ingredient totals.</summary>
public sealed record RecipeLabState(List<Participant> Members,List<RecipeVersion> Versions,List<RecipePlan> Plans,List<RecipeIngredient> Totals);
/// <summary>A bounded procedural sound patch containing parameters only, never binary media.</summary>
public sealed record SoundPatch(string Id,string? ParentId,string MemberId,string Title,string Wave,double Frequency,double Duration,double Gain,int[] Sequence,long At);
/// <summary>Actual shared sound patches with remix lineage.</summary>
public sealed record SoundState(List<Participant> Members,List<SoundPatch> Patches);
/// <summary>An older authored version of a fictional location.</summary>
public sealed record PlaceHistory(string Title,string Description,string Editor,long At);
/// <summary>A fictional grid location and its bounded revision history.</summary>
public sealed record WorldPlace(string Id,int X,int Y,string Title,string Description,string Editor,long At,List<PlaceHistory> History);
/// <summary>A shared fictional world map, without real-world location collection.</summary>
public sealed record WorldState(string Theme,List<Participant> Members,List<WorldPlace> Places);
/// <summary>A clue whose text is omitted until a distinct-member threshold is reached.</summary>
public sealed record CrowdHint(int Index,int Threshold,int Votes,string? Text,bool MyVote);
/// <summary>A collective riddle view with its private solution excluded.</summary>
public sealed record CrowdState(int Scene,List<Participant> Members,bool Solved,List<CrowdHint> Hints);
/// <summary>A turn-authorized fictional letter in an invited group chain.</summary>
public sealed record ChainEntry(string Id,string MemberId,string Body,long At);
/// <summary>A membership-filtered narrative chain and persistent turn index.</summary>
public sealed record LetterState(string Opening,int Capacity,int Length,string Status,int MemberCount,List<Participant> Members,List<ChainEntry> Entries,int Turn);
