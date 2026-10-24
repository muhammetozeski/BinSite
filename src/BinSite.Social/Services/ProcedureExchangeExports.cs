using System.Net;
using System.Text;
namespace BinSite.Social.Services;

/// <summary>Creates portable, encoded reports with a deliberate boundary around private execution records.</summary>
public static class ProcedureExchangeExports
{
    /// <summary>Exports reusable instructions and their source without private run fields or external dependencies.</summary>
    public static string MethodHtml(ProcedureWorkspace workspace)
    {
        var document=ProcedureExchangeRules.PublicDocument(workspace);var body=new StringBuilder();
        body.Append("<p class='badge'>Reusable method</p>");Header(body,document,workspace.Source);Instructions(body,document);
        return Page(document.Title,body.ToString());
    }
    /// <summary>Exports a run snapshot, optionally including explicitly selected private inputs, notes and evidence.</summary>
    public static string RunHtml(ProcedureWorkspace workspace,Guid runId,bool includePrivate)
    {
        ProcedureExchangeRules.Validate(workspace);var run=workspace.Runs.FirstOrDefault(item=>item.Id==runId)??throw new ProcedureValidationException("Select a local run before exporting its report.");var body=new StringBuilder();
        body.Append("<p class='badge'>Local run report · user-declared checks and result</p>");Header(body,run.Snapshot,run.Source);
        if(run.DerivedFrom is not null)body.Append($"<p>Draft derived from: {E(ProcedureExchangeRules.SourceLabel(run.DerivedFrom))}</p>");
        body.Append($"<dl><dt>Run</dt><dd>{run.Id}</dd><dt>State declared locally</dt><dd>{E(run.State.ToString())}</dd><dt>Started</dt><dd>{E(run.StartedAt.ToString("u"))}</dd><dt>Last saved</dt><dd>{E(run.UpdatedAt.ToString("u"))}</dd><dt>Snapshot hash</dt><dd>{E(run.SnapshotHash)}</dd></dl>");
        if(includePrivate){body.Append("<h2>Private execution records included by the exporter</h2><h3>Actual inputs</h3><dl>");for(var index=0;index<run.Snapshot.Inputs.Count;index++)body.Append($"<dt>{E(run.Snapshot.Inputs[index])}</dt><dd>{Block(run.InputValues[index])}</dd>");body.Append("</dl>");}
        else body.Append("<p><strong>Private inputs, notes, evidence, artifact text and result note were omitted.</strong></p>");
        body.Append("<h2>Recorded step checks</h2><ol>");foreach(var step in run.Snapshot.Steps){var record=run.Steps.Single(item=>item.StepId==step.Id);body.Append($"<li><h3>{E(step.Title)}</h3><p>{Block(step.Action)}</p><p><strong>Done when:</strong> {Block(step.DoneWhen)}</p><p>Declared observed: {(record.Done?"Yes":"No")}</p>");if(includePrivate)body.Append($"<p><strong>Note:</strong> {Block(record.Note)}</p><p><strong>Evidence:</strong> {Block(record.Evidence)}</p>");body.Append("</li>");}body.Append("</ol>");
        if(includePrivate)body.Append($"<h2>Actual artifact</h2><pre>{E(run.ArtifactText)}</pre><h2>Result note</h2><p>{Block(run.ResultNote)}</p>");
        return Page(run.Snapshot.Title+" — Run report",body.ToString());
    }
    /// <summary>Returns the actual text recorded as an artifact rather than generating a claim of completion.</summary>
    public static string Artifact(ProcedureWorkspace workspace,Guid runId){ProcedureExchangeRules.Validate(workspace);var run=workspace.Runs.FirstOrDefault(item=>item.Id==runId)??throw new ProcedureValidationException("Select a run first.");if(string.IsNullOrWhiteSpace(run.ArtifactText))throw new ProcedureValidationException("Record the actual artifact text before downloading it.");return run.ArtifactText;}
    /// <summary>Writes shared document identity and attribution using HTML text encoding.</summary>
    static void Header(StringBuilder body,ProcedureDocument document,ProcedureSource source){body.Append($"<h1>{E(document.Title)}</h1><p>{Block(document.Summary)}</p><p>{E(ProcedureExchangeRules.SourceLabel(source))}</p><p>Category: {E(document.Category)} · Tags: {E(string.Join(", ",document.Tags))}</p>");}
    /// <summary>Writes original instructions, expected inputs and output with no execution values.</summary>
    static void Instructions(StringBuilder body,ProcedureDocument document){body.Append("<h2>Required inputs</h2><ul>");foreach(var input in document.Inputs)body.Append($"<li>{E(input)}</li>");body.Append($"</ul><h2>Expected artifact</h2><p>{Block(document.Artifact)}</p><h2>Steps</h2><ol>");foreach(var step in document.Steps)body.Append($"<li><h3>{E(step.Title)}</h3><p>{Block(step.Action)}</p><p><strong>Done when:</strong> {Block(step.DoneWhen)}</p><p>Estimate: {step.Minutes} minutes</p></li>");body.Append("</ol>");}
    /// <summary>Builds a standalone, script-free report with printable structure.</summary>
    static string Page(string title,string body)=>"<!doctype html><html lang='en'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><meta name='referrer' content='no-referrer'><title>"+E(title)+"</title><style>body{font:16px/1.55 system-ui,sans-serif;max-width:900px;margin:32px auto;padding:0 20px;color:#141b22;background:white}h1,h2,h3{line-height:1.2}h2{border-top:2px solid #172939;padding-top:18px}li{margin:14px 0}dt{font-weight:700}dd{margin:0 0 12px;overflow-wrap:anywhere}pre{white-space:pre-wrap;overflow-wrap:anywhere;border:1px solid #87949f;padding:14px}.badge{font-weight:700;color:#164d76}@media print{body{margin:0;max-width:none}li{break-inside:avoid}h2,h3{break-after:avoid}}</style></head><body>"+body+"</body></html>";
    /// <summary>Encodes all authored text before placing it into an HTML document.</summary>
    static string E(string value)=>WebUtility.HtmlEncode(value);
    /// <summary>Preserves authored line breaks after encoding rather than interpreting authored markup.</summary>
    static string Block(string value)=>E(value).Replace("\r\n","\n").Replace("\n","<br>");
}
