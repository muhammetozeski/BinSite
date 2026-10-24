using System.Text;
using System.Text.Json;
namespace BinSite.Social.Services;

/// <summary>Connects public versioned procedures to the existing account API without projecting private run fields.</summary>
public sealed class ProcedureExchangeClient(SocialApi api)
{
    const string Prefix="commerce/procedures";
    /// <summary>Searches actual public records or the signed-in owner's publications in pages of twenty.</summary>
    public Task<ProcedurePublicDirectory> SearchAsync(string search,string category,int offset,bool mine)=>api.GetAsync<ProcedurePublicDirectory>(Prefix+"?search="+Uri.EscapeDataString(search)+"&category="+Uri.EscapeDataString(category)+"&offset="+Math.Max(0,offset)+(mine?"&mine=true":""));
    /// <summary>Reads exactly the requested immutable revision when one is supplied.</summary>
    public Task<ProcedurePublicDetail> ReadAsync(string id,int? revision=null)=>api.GetAsync<ProcedurePublicDetail>(Prefix+"/"+Uri.EscapeDataString(id)+(revision is null?"":"?revision="+revision.Value));
    /// <summary>Explicitly publishes only reusable method fields and source pins, never local execution data.</summary>
    public async Task<ProcedurePublished> PublishAsync(ProcedureWorkspace workspace,string authorAlias,bool allowReuse)
    {
        if(!allowReuse)throw new ProcedureValidationException("Confirm permission to copy, export and fork the method with source attribution before publishing.");var alias=authorAlias.Trim();if(alias.Length is <1 or >40)throw new ProcedureValidationException("Use a published name of 1–40 characters.");
        var document=ProcedureExchangeRules.PublicDocument(workspace);ProcedurePublishedEnvelope result;
        if(workspace.PublicationTarget is {} target)result=await api.PostAsync<ProcedurePublishedEnvelope>(Prefix+"/"+Uri.EscapeDataString(target.Id),new{revision=target.Revision,document,authorAlias=alias,allowReuse=true});
        else {object? parent=workspace.Source.Kind=="published"?new{id=workspace.Source.Id,revision=workspace.Source.Revision}:null;result=await api.PostAsync<ProcedurePublishedEnvelope>(Prefix,new{document,authorAlias=alias,allowReuse=true,parent});}
        return result.Playbook;
    }
    /// <summary>Upserts one explicit account-reported result for the exact selected published revision.</summary>
    public Task<ProcedureAcknowledgement> OutcomeAsync(ProcedurePublished published,string kind,string note){if(kind is not("completed" or "partial" or "blocked")||note.Length>1000)throw new ProcedureValidationException("Choose an explicit outcome and a note of up to 1,000 characters.");return api.PostAsync<ProcedureAcknowledgement>(Prefix+"/"+Uri.EscapeDataString(published.Id)+"/outcomes",new{revision=published.Revision,kind,note});}
    /// <summary>Records a concrete report without claiming the publication has been hidden automatically.</summary>
    public Task<ProcedureAcknowledgement> ReportAsync(string id,string reason,string note){if(reason is not("spam" or "unsafe" or "rights" or "privacy")||note.Length>1000)throw new ProcedureValidationException("Choose a report reason and a note of up to 1,000 characters.");return api.PostAsync<ProcedureAcknowledgement>(Prefix+"/"+Uri.EscapeDataString(id)+"/report",new{reason,note});}
    /// <summary>Requests owner withdrawal; the server decides ownership and visibility.</summary>
    public Task<ProcedureAcknowledgement> WithdrawAsync(string id)=>api.PostAsync<ProcedureAcknowledgement>(Prefix+"/"+Uri.EscapeDataString(id)+"/withdraw",new{});
}

/// <summary>Optional owner-only private draft storage; no invitations, history or revision guarantees are implied.</summary>
public sealed class ProcedureExchangeVaultClient(SocialApi api)
{
    const string Prefix="commerce/projects";
    /// <summary>Lists the actual account vault and lets the caller filter the product kind.</summary>
    public Task<ProcedureVaultDirectory> ListAsync()=>api.GetAsync<ProcedureVaultDirectory>(Prefix);
    /// <summary>Loads a private procedure project and clears any imported publication overwrite target.</summary>
    public async Task<(ProcedureWorkspace Workspace,string Label)> LoadAsync(string id){var response=await api.GetAsync<ProcedureVaultEnvelope>(Prefix+"/"+Uri.EscapeDataString(id));if(response.Project.Kind!="procedure")throw new ProcedureValidationException("This saved item is not a Procedure Exchange project.");return(ProcedureExchangeRules.Restore(response.Project.Body.GetRawText()),response.Project.Title);}
    /// <summary>Saves one explicit owner copy under a separate 100-character label, preserving the full method title.</summary>
    public async Task<string> SaveAsync(ProcedureWorkspace workspace,string label,string? id)
    {
        label=label.Trim();if(label.Length is <1 or >100)throw new ProcedureValidationException("Use 1–100 characters for the saved copy name.");using var parsed=JsonDocument.Parse(ProcedureExchangeRules.Serialize(workspace));var body=parsed.RootElement.Clone();if(Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(body))>ProcedureExchangeRules.MaximumVaultBytes)throw new ProcedureValidationException("The private copy exceeds the 64 KiB vault limit. Download the complete local JSON file instead.");var response=await api.PostAsync<ProcedureVaultSaved>(Prefix+(id is null?"":"/"+Uri.EscapeDataString(id)),new{title=label,kind="procedure",body});return response.Id;
    }
}
