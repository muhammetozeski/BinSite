using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
namespace BinSite.Social.Services;

/// <summary>A clear local authoring, run or portable-file validation failure.</summary>
public sealed class ProcedureValidationException(string message):Exception(message);

/// <summary>Separates reusable public methods from private execution values and preserves pinned run snapshots.</summary>
public static class ProcedureExchangeRules
{
    public const int MaximumPublicBytes=48*1024;
    public const int MaximumLocalBytes=4*1024*1024;
    public const int MaximumVaultBytes=64*1024;
    public static readonly string[] Categories=["work","creative","learning","community","everyday"];
    public static readonly JsonSerializerOptions JsonOptions=Options();
    static readonly JsonSerializerOptions CompactOptions=Options(false);

    /// <summary>Uses strict field names and string enums for portable private project documents.</summary>
    static JsonSerializerOptions Options(bool indented=true){var options=new JsonSerializerOptions(JsonSerializerDefaults.Web){MaxDepth=24,WriteIndented=indented,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase,false));return options;}

    /// <summary>Creates a local editable method while keeping its source attribution separate.</summary>
    public static ProcedureWorkspace Create(ProcedureDocument? document=null,ProcedureSource? source=null)=>new(){Document=document is null?new():CloneDocument(document,false),Source=source is null?new():CopySource(source),ComparisonBase=document is not null&&source?.Kind is "editorial" or "published"?CloneDocument(document):null};

    /// <summary>Returns only the canonical public fields, regardless of what private runs exist beside the draft.</summary>
    public static ProcedureDocument PublicDocument(ProcedureWorkspace workspace)=>CloneDocument(workspace.Document,true);

    /// <summary>Clones the whitelisted method schema and validates the public byte bound when complete.</summary>
    public static ProcedureDocument CloneDocument(ProcedureDocument document,bool complete=true)
    {
        ValidateDocument(document,complete);
        var result=new ProcedureDocument{FormatVersion=1,Title=document.Title.Trim(),Summary=document.Summary.Trim(),Category=document.Category,Tags=document.Tags.Select(item=>item.Trim()).ToList(),Inputs=document.Inputs.Select(item=>item.Trim()).ToList(),Artifact=document.Artifact.Trim(),Steps=document.Steps.Select(step=>new ProcedureStep{Id=step.Id,Title=step.Title.Trim(),Action=step.Action.Trim(),DoneWhen=step.DoneWhen.Trim(),Minutes=step.Minutes}).ToList()};
        if(complete&&Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(result,CompactOptions))>MaximumPublicBytes)throw new ProcedureValidationException("The reusable procedure exceeds the 48 KiB public-document limit.");
        return result;
    }

    /// <summary>Allows incomplete private drafts but requires complete actions and done checks before running or publishing.</summary>
    public static void ValidateDocument(ProcedureDocument document,bool complete=true)
    {
        if(document is null||document.FormatVersion!=1||!Categories.Contains(document.Category)||document.Tags is null||document.Inputs is null||document.Steps is null||document.Tags.Count>6||document.Inputs.Count>20||document.Steps.Count>40||(complete&&document.Steps.Count==0))throw new ProcedureValidationException("Use a supported method with up to 6 tags, 20 inputs and 1–40 steps.");
        Text(document.Title,"Method title",complete?1:0,120);Text(document.Summary,"Method summary",complete?1:0,600);Text(document.Artifact,"Expected artifact",complete?1:0,1200);
        foreach(var tag in document.Tags)Text(tag,"Tag",1,24);
        if(complete&&document.Tags.Select(tag=>tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=document.Tags.Count)throw new ProcedureValidationException("Use distinct tags; capitalization does not make a second tag.");
        foreach(var input in document.Inputs)Text(input,"Required input",complete?1:0,300);
        HashSet<string> ids=[];
        foreach(var step in document.Steps){if(step is null||step.Id is null||!Regex.IsMatch(step.Id,"^[A-Za-z0-9_-]{1,64}$")||!ids.Add(step.Id)||step.Minutes is <0 or >1440)throw new ProcedureValidationException("Each step needs a distinct simple ID and an estimate between 0 and 1,440 minutes.");Text(step.Title,"Step title",complete?1:0,120);Text(step.Action,"Step action",complete?1:0,2000);Text(step.DoneWhen,"Observable done check",complete?1:0,500);}
    }

    /// <summary>Captures a fully detached method revision and initializes private run data.</summary>
    public static ProcedureWorkspace StartRun(ProcedureWorkspace workspace,ProcedureDocument document,ProcedureSource source,bool exactPublished=false)=>Change(workspace,next=>
    {
        if(next.Runs.Count>=20)throw new ProcedureValidationException("This local project has 20 runs. Export it and start another project before adding more.");
        var snapshot=CloneDocument(document);var runSource=exactPublished?CopySource(source):new ProcedureSource{Kind="local",Title=snapshot.Title,AuthorAlias="Local draft"};
        if(exactPublished&&runSource.Kind!="published")throw new ProcedureValidationException("An exact published run requires an actual published source revision.");
        ValidateSource(runSource);
        var run=new ProcedureRun{Snapshot=snapshot,SnapshotHash=Digest(snapshot),Source=runSource,DerivedFrom=!exactPublished&&source.Kind!="local"?CopySource(source):null,InputValues=snapshot.Inputs.Select(_=>"").ToList(),Steps=snapshot.Steps.Select(step=>new ProcedureRunStep{StepId=step.Id}).ToList()};next.Runs.Add(run);next.CurrentRunId=run.Id;
    });

    /// <summary>Saves private values, declared step checks, notes and evidence without changing the reusable method.</summary>
    public static ProcedureWorkspace SaveRun(ProcedureWorkspace workspace,Guid runId,IReadOnlyList<string> inputs,IReadOnlyList<ProcedureRunStep> steps,string artifact,string resultNote)=>Change(workspace,next=>
    {
        var run=FindRun(next,runId);if(run.State!=ProcedureRunState.Active)throw new ProcedureValidationException("Reopen this local run before changing its execution records.");
        run.InputValues=inputs.ToList();run.Steps=steps.Select(item=>new ProcedureRunStep{StepId=item.StepId,Done=item.Done,Note=item.Note,Evidence=item.Evidence}).ToList();run.ArtifactText=artifact;run.ResultNote=resultNote;run.UpdatedAt=DateTimeOffset.UtcNow;
    });

    /// <summary>Records an explicit local result; checkboxes alone never finish a run or submit a public outcome.</summary>
    public static ProcedureWorkspace FinishRun(ProcedureWorkspace workspace,Guid runId,ProcedureRunState state)=>Change(workspace,next=>
    {
        var run=FindRun(next,runId);if(run.State!=ProcedureRunState.Active||state==ProcedureRunState.Active||!Enum.IsDefined(state))throw new ProcedureValidationException("Choose completed, partial or blocked for an active run.");
        if(state==ProcedureRunState.Completed&&(run.Steps.Any(step=>!step.Done)||string.IsNullOrWhiteSpace(run.ArtifactText)))throw new ProcedureValidationException("To declare completion, mark each observed done check and record the actual artifact you produced.");
        Text(run.ResultNote,"Your result note",3,4000);run.State=state;run.UpdatedAt=DateTimeOffset.UtcNow;
    });

    /// <summary>Explicitly reopens a local result without changing any separate public outcome report.</summary>
    public static ProcedureWorkspace ReopenRun(ProcedureWorkspace workspace,Guid runId)=>Change(workspace,next=>{var run=FindRun(next,runId);run.State=ProcedureRunState.Active;run.UpdatedAt=DateTimeOffset.UtcNow;});

    /// <summary>Serializes the complete private project, including incomplete drafts and actual local runs.</summary>
    public static string Serialize(ProcedureWorkspace workspace)
    {
        Validate(workspace);var text=JsonSerializer.Serialize(workspace,JsonOptions);if(Encoding.UTF8.GetByteCount(text)>MaximumLocalBytes)throw new ProcedureValidationException("This project exceeds the 4 MB local-file limit.");return text;
    }

    /// <summary>Restores a private project or attributed reusable method package; file content cannot arm an update to a server publication.</summary>
    public static ProcedureWorkspace Restore(string text)
    {
        if(Encoding.UTF8.GetByteCount(text)>MaximumLocalBytes)throw new ProcedureValidationException("Choose a method/project JSON file no larger than 4 MB.");
        try
        {
            using var json=JsonDocument.Parse(text);ProcedureWorkspace workspace;
            if(json.RootElement.TryGetProperty("fileVersion",out _))workspace=JsonSerializer.Deserialize<ProcedureWorkspace>(text,JsonOptions)??throw new ProcedureValidationException("The project is empty.");
            else if(json.RootElement.TryGetProperty("packageVersion",out _)){var package=JsonSerializer.Deserialize<ProcedurePackage>(text,JsonOptions)??throw new ProcedureValidationException("The package is empty.");if(package.PackageVersion!=1)throw new ProcedureValidationException("The method package version is unsupported.");workspace=Create(package.Document,package.Source);}
            else workspace=Create(JsonSerializer.Deserialize<ProcedureDocument>(text,JsonOptions)??throw new ProcedureValidationException("The method is empty."));
            Validate(workspace);workspace.PublicationTarget=null;return workspace;
        }
        catch(JsonException){throw new ProcedureValidationException("The file has an unsupported method/project structure or field value.");}
    }

    /// <summary>Exports an attributed reusable package while deliberately excluding every private run field.</summary>
    public static string MethodPackage(ProcedureWorkspace workspace)=>JsonSerializer.Serialize(new ProcedurePackage{Document=PublicDocument(workspace),Source=CopySource(workspace.Source)},JsonOptions);

    /// <summary>Checks private run references, immutable snapshot hashes and declaration consistency.</summary>
    public static void Validate(ProcedureWorkspace workspace)
    {
        if(workspace is null||workspace.FileVersion!=1||workspace.Id==Guid.Empty||workspace.Runs is null||workspace.Runs.Count>20)throw new ProcedureValidationException("Unsupported local procedure project.");
        ValidateDocument(workspace.Document,false);ValidateSource(workspace.Source);if(workspace.ComparisonBase is not null)ValidateDocument(workspace.ComparisonBase);HashSet<Guid> ids=[];
        if(workspace.PublicationTarget is {} target&&(string.IsNullOrWhiteSpace(target.Id)||target.Id.Length>100||target.Revision<1))throw new ProcedureValidationException("Invalid publication target.");
        foreach(var run in workspace.Runs)
        {
            if(run is null||run.Id==Guid.Empty||!ids.Add(run.Id)||!Enum.IsDefined(run.State)||run.InputValues is null||run.Steps is null)throw new ProcedureValidationException("A local run has invalid identity or state.");
            ValidateDocument(run.Snapshot);ValidateSource(run.Source);if(run.DerivedFrom is not null)ValidateSource(run.DerivedFrom);
            if(run.SnapshotHash!=Digest(run.Snapshot))throw new ProcedureValidationException("A run's pinned method snapshot changed. Its recorded snapshot hash no longer matches.");
            if(run.InputValues.Count!=run.Snapshot.Inputs.Count||run.Steps.Count!=run.Snapshot.Steps.Count||run.Steps.Any(step=>step is null)||run.Steps.Select(step=>step.StepId).Distinct().Count()!=run.Steps.Count||run.Steps.Any(step=>!run.Snapshot.Steps.Any(snapshot=>snapshot.Id==step.StepId)))throw new ProcedureValidationException("A run's inputs or step records do not match its immutable method snapshot.");
            foreach(var input in run.InputValues)Text(input,"Private input value",0,4000);
            foreach(var step in run.Steps){Text(step.Note,"Private step note",0,4000);Text(step.Evidence,"Private evidence",0,4000);}
            Text(run.ArtifactText,"Actual artifact",0,16000);Text(run.ResultNote,"Local result note",0,4000);
            if(run.State==ProcedureRunState.Completed&&(run.Steps.Any(step=>!step.Done)||string.IsNullOrWhiteSpace(run.ArtifactText)||run.ResultNote.Trim().Length<3))throw new ProcedureValidationException("A completed declaration needs all done checks, an artifact and a result note.");
        }
        if(workspace.CurrentRunId is Guid selected&&!ids.Contains(selected))throw new ProcedureValidationException("The selected run does not belong to this project.");
    }

    /// <summary>Returns a stable hash of the public snapshot; it detects accidental drift, not authenticated proof of execution.</summary>
    public static string Digest(ProcedureDocument document)=>Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(document,CompactOptions))));
    /// <summary>Copies only source attribution metadata.</summary>
    public static ProcedureSource CopySource(ProcedureSource source)=>new(){Kind=source.Kind,Id=source.Id,Revision=source.Revision,AuthorAlias=source.AuthorAlias,Title=source.Title};
    /// <summary>Formats provenance without inventing a publisher identity or published status for local work.</summary>
    public static string SourceLabel(ProcedureSource source)=>source.Kind switch{"published"=>$"Published method {source.Id}, revision {source.Revision}, publisher alias: {source.AuthorAlias}","editorial"=>$"Original editorial starter: {source.Title}",_=>"Local draft; not a published revision"};
    /// <summary>Compares authored instructions by stable step ID, including reordering and observable checks.</summary>
    public static List<string> Differences(ProcedureDocument before,ProcedureDocument after)
    {
        ValidateDocument(before);ValidateDocument(after,false);List<string> changes=[];
        if(before.Title!=after.Title)changes.Add("Method title changed.");if(before.Summary!=after.Summary)changes.Add("Summary changed.");if(before.Category!=after.Category)changes.Add("Category changed.");if(!before.Tags.SequenceEqual(after.Tags))changes.Add("Tags changed.");if(!before.Inputs.SequenceEqual(after.Inputs))changes.Add("Required inputs changed.");if(before.Artifact!=after.Artifact)changes.Add("Expected artifact changed.");
        foreach(var step in before.Steps){var next=after.Steps.FirstOrDefault(item=>item.Id==step.Id);if(next is null){changes.Add($"Removed step: {step.Title}");continue;}if(step.Title!=next.Title)changes.Add($"Renamed step: {step.Title} → {next.Title}");if(step.Action!=next.Action)changes.Add($"Action changed: {next.Title}");if(step.DoneWhen!=next.DoneWhen)changes.Add($"Done check changed: {next.Title}");if(step.Minutes!=next.Minutes)changes.Add($"Time estimate changed: {next.Title}");}
        foreach(var step in after.Steps.Where(item=>!before.Steps.Any(old=>old.Id==item.Id)))changes.Add($"Added step: {step.Title}");
        var sharedBefore=before.Steps.Select(item=>item.Id).Where(id=>after.Steps.Any(item=>item.Id==id));var sharedAfter=after.Steps.Select(item=>item.Id).Where(id=>before.Steps.Any(item=>item.Id==id));if(!sharedBefore.SequenceEqual(sharedAfter))changes.Add("Existing steps reordered.");return changes;
    }
    /// <summary>Resolves only a run owned by the current local project.</summary>
    static ProcedureRun FindRun(ProcedureWorkspace workspace,Guid id)=>workspace.Runs.FirstOrDefault(item=>item.Id==id)??throw new ProcedureValidationException("This run was not found in the current project.");
    /// <summary>Commits a private action only after its entire candidate document validates.</summary>
    static ProcedureWorkspace Change(ProcedureWorkspace workspace,Action<ProcedureWorkspace> change){Validate(workspace);var next=JsonSerializer.Deserialize<ProcedureWorkspace>(JsonSerializer.Serialize(workspace,JsonOptions),JsonOptions)??throw new ProcedureValidationException("Project copy failed.");change(next);_=Serialize(next);return next;}
    /// <summary>Checks attribution shape without treating a local file as a proof of public ownership.</summary>
    static void ValidateSource(ProcedureSource source){if(source is null||source.Kind is not("local" or "editorial" or "published")||source.Id is null||source.Id.Length>100||source.Revision<0)throw new ProcedureValidationException("Invalid method source attribution.");Text(source.Title,"Source title",0,120);Text(source.AuthorAlias,"Publisher alias",0,40);if(source.Kind=="published"&&(source.Id.Length==0||source.Revision<1))throw new ProcedureValidationException("A published source needs its actual ID and immutable revision.");}
    /// <summary>Bounds text and rejects unsupported controls while retaining authored line breaks.</summary>
    static void Text(string? value,string name,int minimum,int maximum){if(value is null||value.Trim().Length<minimum||value.Length>maximum||value.Any(character=>char.IsControl(character)&&character is not '\n' and not '\r' and not '\t'))throw new ProcedureValidationException($"{name} must contain {minimum}–{maximum} characters without unsupported controls.");}
}
