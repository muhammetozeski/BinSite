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
