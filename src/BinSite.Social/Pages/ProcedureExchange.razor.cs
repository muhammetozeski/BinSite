using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace BinSite.Social.Pages;

/// <summary>Independent authoring, pinned local execution and explicit real-account publication workflows.</summary>
public partial class ProcedureExchange
{
    ProcedureWorkspace Workspace=ProcedureExchangeRules.Create();
    ProcedureExchangeClient Client=>new(Api);
    ProcedureExchangeVaultClient Vault=>new(Api);
    IJSObjectReference? Module;
    ElementReference RestoreInput;
    string View="author",Status="",TagsDraft="",InputsDraft="",AuthorAlias="",Search="",SearchCategory="",OutcomeKind="",OutcomeNote="",ReportReason="spam",ReportNote="",VaultLabel="";
    bool HasError,Busy,ReuseConsent,DirectoryLoaded,HasMore,MineOnly,IncludePrivateReport,VaultLoaded,PublicRefreshPending;
    int SearchOffset,RevisionToRead=1;
    string? VaultId;
    List<string> Differences=[];
    List<ProcedurePublicItem> PublicItems=[];
    List<ProcedureVaultHeader> VaultItems=[];
    ProcedurePublicDetail? PublicDetail;
    ProcedureRun? CurrentRun=>Workspace.Runs.FirstOrDefault(item=>item.Id==Workspace.CurrentRunId);
    string PublicLink=>PublicDetail is null?"":new Uri(new Uri(Navigation.BaseUri),"procedure-exchange/?playbook="+Uri.EscapeDataString(PublicDetail.Playbook.Id)+"&revision="+PublicDetail.Playbook.Revision).AbsoluteUri;

    /// <summary>Loads only an explicit public revision link; the initial editor otherwise has no network dependency.</summary>
    protected override async Task OnInitializedAsync()
    {
        var query=ParseQuery(Navigation.ToAbsoluteUri(Navigation.Uri).Query);if(query.TryGetValue("playbook",out var id)&&id.Length is >0 and <=100){int? revision=query.TryGetValue("revision",out var raw)&&int.TryParse(raw,out var parsed)&&parsed>0?parsed:null;await ReadPublicAsync(id,revision);}
    }
    /// <summary>Loads the narrow local-file/download bridge only when the browser needs it.</summary>
    async Task<IJSObjectReference> BridgeAsync()=>Module??=await Javascript.InvokeAsync<IJSObjectReference>("import","/procedure-exchange.js");
    /// <summary>Synchronizes input descriptions and tags into a validated private draft before domain actions.</summary>
    void SyncDraft(){Workspace.Document.Tags=Lines(TagsDraft);Workspace.Document.Inputs=Lines(InputsDraft);Workspace.Document=ProcedureExchangeRules.CloneDocument(Workspace.Document,false);}
    /// <summary>Refreshes editable text buffers and revokes publication consent after changing project identity.</summary>
    void RefreshDraft(){TagsDraft=string.Join("\n",Workspace.Document.Tags);InputsDraft=string.Join("\n",Workspace.Document.Inputs);ReuseConsent=false;Differences=Workspace.ComparisonBase is null?[]:ProcedureExchangeRules.Differences(Workspace.ComparisonBase,Workspace.Document);}
    /// <summary>Prevents accidental replacement of a method and its in-memory runs.</summary>
    async Task<bool> MayReplaceAsync()=>!HasLocalWork||await(await BridgeAsync()).InvokeAsync<bool>("confirmReplace","Replace this project? Download it first to keep its draft and runs.");
    bool HasLocalWork=>Workspace.Runs.Count>0||Workspace.Document.Steps.Count>0||Workspace.Source.Kind!="local"||new[]{Workspace.Document.Title,Workspace.Document.Summary,Workspace.Document.Artifact,TagsDraft,InputsDraft}.Any(value=>!string.IsNullOrWhiteSpace(value));
    /// <summary>Starts an empty editable method after an explicit local replacement decision.</summary>
    Task NewAsync()=>ActionAsync(async()=>{if(!await MayReplaceAsync())return;Workspace=ProcedureExchangeRules.Create();VaultId=null;VaultLabel="";View="author";RefreshDraft();Say("New method ready. Add inputs, an output and the steps.");});
    /// <summary>Copies a clearly labelled original starter into local editable state.</summary>
    Task LoadStarterAsync(ProcedureStarter starter)=>ActionAsync(async()=>{if(!await MayReplaceAsync())return;Workspace=ProcedureExchangeRules.Create(starter.Document,new ProcedureSource{Kind="editorial",Id=starter.Id,Title=starter.Document.Title,AuthorAlias="BinSite editorial"});VaultId=null;VaultLabel="";View="author";RefreshDraft();Say("Starting method opened. Edit it or start a run.");});
    /// <summary>Checks reusable method completeness without publishing or inferring execution.</summary>
    void CheckDraft()=>Action(()=>{SyncDraft();_=ProcedureExchangeRules.PublicDocument(Workspace);Say("Method is ready to use or publish.");});
    /// <summary>Adds a distinct stable step identity to this draft only.</summary>
    void AddStep()=>Action(()=>{if(Workspace.Document.Steps.Count>=40)throw new ProcedureValidationException("A method can contain up to 40 steps.");Workspace.Document.Steps.Add(new());});
    /// <summary>Moves an instruction while preserving the identity used in fork comparisons.</summary>
    void MoveStep(int index,int direction)=>Action(()=>{var target=index+direction;if(index<0||index>=Workspace.Document.Steps.Count||target<0||target>=Workspace.Document.Steps.Count)return;(Workspace.Document.Steps[index],Workspace.Document.Steps[target])=(Workspace.Document.Steps[target],Workspace.Document.Steps[index]);});
    /// <summary>Removes an editable instruction without rewriting prior run snapshots.</summary>
    void RemoveStep(int index)=>Action(()=>Workspace.Document.Steps.RemoveAt(index));
    /// <summary>Recomputes changes from the exact stored source snapshot.</summary>
    void CompareDraft()=>Action(()=>{SyncDraft();Differences=Workspace.ComparisonBase is null?[]:ProcedureExchangeRules.Differences(Workspace.ComparisonBase,Workspace.Document);Say(Differences.Count==0?"The draft matches the stored source instructions.":$"{Differences.Count} changes from the source.");});
    /// <summary>Detaches an explicit update target; original source attribution is retained for a possible fork.</summary>
    void DetachTarget()=>Action(()=>{Workspace.PublicationTarget=null;ReuseConsent=false;Say("Detached. The next publication will be a new method.");});
    /// <summary>Starts a detached local-draft run, never mislabelling edited work as an exact public revision.</summary>
    void StartDraftRun()=>Action(()=>{SyncDraft();Workspace=ProcedureExchangeRules.StartRun(Workspace,Workspace.Document,Workspace.Source);View="run";IncludePrivateReport=false;Say("Run started. Its instructions stay fixed.");});
    /// <summary>Starts an exact method revision read from the real public API.</summary>
    void StartPublishedRun()=>Action(()=>{if(PublicDetail is null)return;SyncDraft();var published=PublicDetail.Playbook;Workspace=ProcedureExchangeRules.StartRun(Workspace,published.Document,SourceOf(published),true);View="run";IncludePrivateReport=false;Say($"Run started from revision {published.Revision}.");});
    /// <summary>Selects an existing private local run without replacing another run's fields.</summary>
    void SelectRun(Guid id){if(Busy)return;Workspace.CurrentRunId=id;IncludePrivateReport=false;}
    /// <summary>Validates and saves actual execution fields for the active local run.</summary>
    void SaveRunFields(){if(CurrentRun is not {} run)return;if(run.State==ProcedureRunState.Active)Workspace=ProcedureExchangeRules.SaveRun(Workspace,run.Id,run.InputValues,run.Steps,run.ArtifactText,run.ResultNote);}
    /// <summary>Saves only the local record; persistence still requires a JSON download or explicit private save.</summary>
    void SaveCurrentRun()=>Action(()=>{SaveRunFields();Say("Entries checked.");});
    /// <summary>Records an explicit local result after observable-check validation.</summary>
    void FinishRun(ProcedureRunState state)=>Action(()=>{SaveRunFields();if(CurrentRun is {} run)Workspace=ProcedureExchangeRules.FinishRun(Workspace,run.Id,state);Say($"Run marked {state.ToString().ToLowerInvariant()}.");});
    /// <summary>Explicitly reopens a local record; public reports remain separate.</summary>
    void ReopenRun()=>Action(()=>{if(CurrentRun is {} run)Workspace=ProcedureExchangeRules.ReopenRun(Workspace,run.Id);Say("Run reopened.");});
    /// <summary>Downloads the complete private document, including incomplete draft and all local runs.</summary>
    Task DownloadProjectAsync()=>ActionAsync(async()=>{SyncDraft();SaveRunFields();await DownloadAsync("procedure-project.json",ProcedureExchangeRules.Serialize(Workspace),"application/json");Say("Project downloaded, including private runs.");});
    /// <summary>Downloads only reusable canonical instructions and retained source attribution.</summary>
    Task ExportPackageAsync()=>ActionAsync(async()=>{SyncDraft();await DownloadAsync("reusable-method.json",ProcedureExchangeRules.MethodPackage(Workspace),"application/json");Say("Method JSON downloaded. Private runs are excluded.");});
    /// <summary>Downloads a standalone encoded method document with no private execution data.</summary>
    Task ExportMethodAsync()=>ActionAsync(async()=>{SyncDraft();await DownloadAsync("procedure-method.html",ProcedureExchangeExports.MethodHtml(Workspace),"text/html;charset=utf-8");Say("Printable method downloaded.");});
    /// <summary>Restores strict JSON only after the entire candidate validates and the visitor accepts replacement.</summary>
    Task RestoreAsync(ChangeEventArgs _)=>ActionAsync(async()=>{var text=await(await BridgeAsync()).InvokeAsync<string?>("readText",RestoreInput,ProcedureExchangeRules.MaximumLocalBytes);if(text is null)return;var candidate=ProcedureExchangeRules.Restore(text);if(!await MayReplaceAsync())return;Workspace=candidate;VaultId=null;VaultLabel="";RefreshDraft();View="author";Say("Project opened. Reconnect a publication before updating its version.");});
    /// <summary>Exports a precise pinned run report with private text excluded unless explicitly selected.</summary>
    Task ExportRunAsync()=>ActionAsync(async()=>{var includePrivate=IncludePrivateReport;SyncDraft();SaveRunFields();if(CurrentRun is not {} run)throw new ProcedureValidationException("Choose a local run first.");await DownloadAsync("procedure-run-report.html",ProcedureExchangeExports.RunHtml(Workspace,run.Id,includePrivate),"text/html;charset=utf-8");Say(includePrivate?"Report downloaded with private data included.":"Report downloaded with private data omitted.");});
    /// <summary>Downloads the actual recorded artifact text without adding a fabricated completion statement.</summary>
    Task ExportArtifactAsync()=>ActionAsync(async()=>{SyncDraft();SaveRunFields();if(CurrentRun is not {} run)throw new ProcedureValidationException("Choose a local run first.");await DownloadAsync("procedure-artifact.txt",ProcedureExchangeExports.Artifact(Workspace,run.Id),"text/plain;charset=utf-8");Say("Result text downloaded.");});
    /// <summary>Opens discovery and searches actual public records on first use.</summary>
    async Task OpenPublicAsync(){if(Busy)return;View="public";if(!DirectoryLoaded)await SearchPublicAsync();}
    /// <summary>Starts an explicit directory search; first-page failure does not invent empty success.</summary>
    Task SearchPublicAsync()=>ActionAsync(async()=>{SearchOffset=0;await FetchDirectoryAsync();});
    /// <summary>Reads the previous actual directory page.</summary>
    Task PreviousPageAsync()=>ActionAsync(async()=>{var previous=SearchOffset;SearchOffset=Math.Max(0,SearchOffset-20);try{await FetchDirectoryAsync();}catch{SearchOffset=previous;throw;}});
    /// <summary>Reads the next actual directory page within the server's documented listing bound.</summary>
    Task NextPageAsync()=>ActionAsync(async()=>{if(SearchOffset>=2000)throw new ProcedureValidationException("Narrow your search to browse more methods.");var previous=SearchOffset;SearchOffset+=20;try{await FetchDirectoryAsync();}catch{SearchOffset=previous;throw;}});
    /// <summary>Commits only a successfully read public directory result.</summary>
    async Task FetchDirectoryAsync(){var result=await Client.SearchAsync(Search,SearchCategory,SearchOffset,MineOnly);PublicItems=result.Items;HasMore=result.HasMore;DirectoryLoaded=true;Say($"{PublicItems.Count} methods on this page.");}
    /// <summary>Reads the requested immutable publication and its real revision-specific outcomes.</summary>
    Task ReadPublicAsync(string id,int? revision)=>ActionAsync(async()=>{var result=await Client.ReadAsync(id,revision);_=ProcedureExchangeRules.CloneDocument(result.Playbook.Document);PublicDetail=result;PublicRefreshPending=false;RevisionToRead=result.Playbook.Revision;OutcomeKind="";OutcomeNote="";ReportNote="";View="public";Say($"Opened revision {result.Playbook.Revision}.");});
    /// <summary>Reads the exact manually selected immutable revision.</summary>
    Task ReadRevisionAsync()=>PublicDetail is null?Task.CompletedTask:ReadPublicAsync(PublicDetail.Playbook.Id,RevisionToRead);
    /// <summary>Copies a public source into an explicitly new fork or owner update draft.</summary>
    Task UsePublicAsync(bool ownerUpdate)=>ActionAsync(async()=>{if(PublicDetail is not {} detail)return;var published=detail.Playbook;if(ownerUpdate&&!published.Owned)throw new ProcedureValidationException("Only the actual publication owner can prepare its next version.");if(!await MayReplaceAsync())return;Workspace=ProcedureExchangeRules.Create(published.Document,SourceOf(published));if(ownerUpdate)Workspace.PublicationTarget=new(published.Id,published.Revision);AuthorAlias=published.AuthorAlias;VaultId=null;VaultLabel="";RefreshDraft();View="author";Say(ownerUpdate?"Update draft opened. Review it before publishing.":"Method ready to adapt.");});
    /// <summary>Reconnects a saved local draft to a verified owned publication without discarding the local work.</summary>
    Task ArmSelectedUpdateAsync()=>ActionAsync(async()=>{if(PublicDetail?.Playbook is not {} published||!published.Owned||published.Withdrawn||published.Hidden)throw new ProcedureValidationException("Select an available publication owned by your actual account.");SyncDraft();if(!await(await BridgeAsync()).InvokeAsync<bool>("confirmReplace",$"Use your current local draft as the next version of '{published.Document.Title}', based on version {published.Revision}? Your draft and runs remain; publishing is a separate action."))return;Workspace.PublicationTarget=new(published.Id,published.Revision);Workspace.Source=SourceOf(published);Workspace.ComparisonBase=ProcedureExchangeRules.CloneDocument(published.Document);AuthorAlias=published.AuthorAlias;RefreshDraft();View="author";Say("Update target connected. Review the comparison before publishing.");});
    /// <summary>Publishes canonical instructions only; a 409 preserves the local draft for deliberate comparison.</summary>
    Task PublishAsync()=>ActionAsync(async()=>{SyncDraft();var published=await Client.PublishAsync(Workspace,AuthorAlias,ReuseConsent);Workspace.Source=SourceOf(published);Workspace.ComparisonBase=ProcedureExchangeRules.CloneDocument(published.Document);Workspace.PublicationTarget=new(published.Id,published.Revision);ReuseConsent=false;Differences=[];PublicDetail=null;RevisionToRead=published.Revision;OutcomeKind="";OutcomeNote="";await RefreshPublicAfterAsync(published,$"Published revision {published.Revision}.");});
    /// <summary>Submits only an explicit bounded public outcome, with no implicit projection from local run fields.</summary>
    Task SubmitOutcomeAsync()=>ActionAsync(async()=>{if(PublicDetail is not {} detail)return;await Client.OutcomeAsync(detail.Playbook,OutcomeKind,OutcomeNote);OutcomeKind="";OutcomeNote="";await RefreshPublicAfterAsync(detail.Playbook,"Public outcome saved.");});
    /// <summary>Submits an account-authorized report without pretending the item was automatically hidden.</summary>
    Task ReportAsync()=>ActionAsync(async()=>{if(PublicDetail is null)return;await Client.ReportAsync(PublicDetail.Playbook.Id,ReportReason,ReportNote);ReportNote="";Say("Report saved.");});
    /// <summary>Requests owner withdrawal only after a specific confirmation.</summary>
    Task WithdrawAsync()=>ActionAsync(async()=>{if(PublicDetail?.Playbook is not {} published||!published.Owned)return;if(!await(await BridgeAsync()).InvokeAsync<bool>("confirmReplace","Withdraw this publication? Downloaded copies remain independent."))return;await Client.WithdrawAsync(published.Id);PublicDetail=PublicDetail! with{Playbook=published with{Withdrawn=true}};await RefreshPublicAfterAsync(published,"Publication withdrawn.");});
    async Task RefreshPublicAfterAsync(ProcedurePublished published,string success)
    {
        Say(success);
        try{PublicDetail=await Client.ReadAsync(published.Id,published.Revision);PublicRefreshPending=false;}
        catch(Exception error)
        {
            PublicRefreshPending=true;
            Say(success+(PublicDetail is null?" Details could not refresh; find the method in Published methods.":" Details could not refresh; use Open version to refresh them."));
            Logger.LogWarning("Procedure detail refresh failed after completed mutation; type={Type}",error.GetType().Name);
        }
    }
    /// <summary>Copies an immutable link, with a visible selectable fallback when clipboard permission is unavailable.</summary>
    Task CopyLinkAsync()=>ActionAsync(async()=>{var copied=await(await BridgeAsync()).InvokeAsync<bool>("copy",PublicLink);Say(copied?"Link copied.":"Select and copy the link shown above.");});
    /// <summary>Lists only actual owner vault records of this product kind.</summary>
    Task ListVaultAsync()=>ActionAsync(async()=>{await FetchVaultAsync();Say($"{VaultItems.Count} saved copies.");});
    /// <summary>Reads vault headers without changing local draft ownership or publication targets.</summary>
    async Task FetchVaultAsync(){var result=await Vault.ListAsync();VaultItems=result.Projects.Where(item=>item.Kind=="procedure").ToList();VaultLoaded=true;}
    /// <summary>Loads a validated owner-only project after explicit replacement confirmation.</summary>
    Task LoadVaultAsync(string id)=>ActionAsync(async()=>{var saved=await Vault.LoadAsync(id);if(!await MayReplaceAsync())return;Workspace=saved.Workspace;VaultId=id;VaultLabel=saved.Label;RefreshDraft();Say("Private copy opened. Reconnect a publication before updating it.");});
    /// <summary>Saves a new private body under a separate visible label; the API enforces account and shared quota.</summary>
    Task SaveVaultNewAsync()=>SaveVaultAsync(false);
    /// <summary>Replaces only the explicitly selected private owner copy after confirmation.</summary>
    Task SaveVaultExistingAsync()=>SaveVaultAsync(true);
    /// <summary>Preserves full local title and run data inside the bounded owner vault body.</summary>
    Task SaveVaultAsync(bool replace)=>ActionAsync(async()=>{SyncDraft();SaveRunFields();if(replace&&VaultId is null)throw new ProcedureValidationException("Select a saved copy before replacing it.");if(replace&&!await(await BridgeAsync()).InvokeAsync<bool>("confirmReplace","Replace the selected private saved copy with this complete local project? Download the earlier copy first if you want to keep it."))return;VaultId=await Vault.SaveAsync(Workspace,VaultLabel,replace?VaultId:null);Say("Private copy saved.");});
    /// <summary>Detaches the private saved-copy target while preserving all local work.</summary>
    void DetachVault(){if(Busy)return;VaultId=null;Say("Saved copy unchanged. You are editing a local draft.");}
    /// <summary>Downloads a file via a local Blob without sending its contents to a server.</summary>
    async Task DownloadAsync(string name,string content,string mime)=>await(await BridgeAsync()).InvokeVoidAsync("download",name,content,mime);
    /// <summary>Runs a local synchronous product action and surfaces validation errors without logging private content.</summary>
    void Action(Action operation){if(Busy)return;try{HasError=false;Status="";operation();}catch(Exception error){Failure(error);}}
    /// <summary>Serializes asynchronous UI actions and surfaces recoverable API/browser failures for explicit retry.</summary>
    async Task ActionAsync(Func<Task> operation){if(Busy)return;Busy=true;HasError=false;Status="";try{await operation();}catch(Exception error){Failure(error);}finally{Busy=false;}}
    /// <summary>Shows a concrete result once, without treating an exception as a successful operation.</summary>
    void Say(string message){Status=message;HasError=false;}
    /// <summary>Logs exception type and status only; submitted content, private notes and links are excluded.</summary>
    void Failure(Exception error){Logger.LogWarning("Procedure Exchange action failed; type={ErrorType}; status={Status}",error.GetType().Name,error is ApiFailure failureStatus?(int)failureStatus.Status:0);HasError=true;Status=error switch{ProcedureValidationException validation=>validation.Message,ApiFailure{Status:HttpStatusCode.Conflict}=>"Saved content changed or a limit was reached. Keep your local project and open the latest version before trying again.",ApiFailure responseFailure=>responseFailure.Message,JSException=>"The browser could not finish the local file, download or clipboard action. Check the selected file and try the action explicitly again.",_=>"The action did not finish. Your current local draft remains in this tab; export it before refreshing."};}
    /// <summary>Builds attribution only from an actual public response selected by the visitor.</summary>
    static ProcedureSource SourceOf(ProcedurePublished published)=>new(){Kind="published",Id=published.Id,Revision=published.Revision,Title=published.Document.Title,AuthorAlias=published.AuthorAlias};
    /// <summary>Reads list editors as bounded nonempty lines while preserving internal authored text.</summary>
    static List<string> Lines(string text)=>text.Split('\n').Select(item=>item.Trim()).Where(item=>item.Length>0).ToList();
    /// <summary>Parses only the two expected share-link fields without storing private execution values in URLs.</summary>
    static Dictionary<string,string> ParseQuery(string query){Dictionary<string,string> result=new(StringComparer.Ordinal);foreach(var pair in query.TrimStart('?').Split('&',StringSplitOptions.RemoveEmptyEntries)){var parts=pair.Split('=',2);if(parts.Length==2&&parts[0] is "playbook" or "revision")result[parts[0]]=Uri.UnescapeDataString(parts[1].Replace('+',' '));}return result;}
    /// <summary>Releases the local browser bridge when navigating away from the product.</summary>
    public async ValueTask DisposeAsync(){if(Module is not null)try{await Module.DisposeAsync();}catch(JSDisconnectedException){Logger.LogDebug("Procedure Exchange bridge released after browser disconnection.");}}
}
