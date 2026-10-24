using System.Net;
using System.Text;
using BinSite.Social.Models;

namespace BinSite.Social.Services;

/// <summary>Exports only an attempt's visible evidence; text is encoded and no remote resource is referenced.</summary>
public static class SignalFoundryExports
{
    public static string Report(SignalAttempt attempt, int[] answer, string notes)
    {
        SignalFoundryEngine.ValidateBoard(answer);
        var body = new StringBuilder();
        body.Append("<!doctype html><html lang=\"en\"><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width\"><meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'\"><title>").Append(E(attempt.Challenge.Title)).Append(" — Signal Foundry record</title><style>body{font:16px system-ui;max-width:1000px;margin:24px auto;padding:16px;color:#111}section{border-top:2px solid #111;padding:12px 0;break-inside:avoid}.row{display:flex;gap:24px;align-items:center;flex-wrap:wrap}.board{display:grid;grid-template-columns:repeat(3,42px);gap:2px}.cell{height:42px;display:grid;place-items:center;border:1px solid #111;font-weight:bold}.c0{background:#fff}.c1{background:#c42020;color:#fff}.c2{background:#164fc7;color:#fff}.c3{background:#f0ca00;color:#111}pre{white-space:pre-wrap;font:inherit}code{font-size:18px}h1{font-size:28px}@media print{body{margin:0}}</style><h1>").Append(E(attempt.Challenge.Title)).Append("</h1><p>Signal Foundry · ").Append(attempt.Finished ? "Finished attempt" : "Unfinished working record").Append(" · ").Append(E(attempt.Challenge.EngineVersion)).Append("</p><p>").Append(E(attempt.Challenge.Description)).Append("</p><p>R = red · B = blue · Y = yellow · dot = empty. Programs apply left to right.</p>");
        foreach (var example in attempt.Challenge.Examples) AddExample(body, "Worked example", example);
        foreach (var example in attempt.Observations) AddExample(body, "Requested experiment", example);
        body.Append("<section><h2>Final reconstruction</h2><div class=\"row\">").Append(Board(attempt.Challenge.Target.Input)).Append("<code>").Append(E(attempt.Challenge.Target.Program)).Append(" →</code>").Append(Board(answer)).Append("</div></section>");
        if (attempt.Finished && attempt.Result is { } result)
        {
            body.Append("<section><h2>").Append(E(result.Verdict)).Append("</h2><p>").Append(result.CandidateCount).Append(" consistent languages; ").Append(result.TargetOutcomeCount).Append(" distinct final outcomes. Matched the hidden mapping: ").Append(result.ActualMatch ? "yes" : "no").Append(".</p><h3>Hidden mapping's actual result</h3>").Append(Board(result.ActualOutput)).Append("<p>").Append(E(result.Explanation)).Append("</p><ul>");
            for (var index = 0; index < result.Mapping.Length; index++) if (result.Mapping[index] is >= 0 and < 8) body.Append("<li>").Append((char)('A' + index)).Append(": ").Append(E(SignalFoundryEngine.OperationNames[result.Mapping[index]])).Append("</li>");
            body.Append("</ul></section>");
        }
        body.Append("<section><h2>My notes</h2><pre>").Append(E(notes)).Append("</pre></section><p>This is a personal play record, not a verified contest result. Built-in missions are editorial content. Local previews are labelled in the application.</p></html>");
        return body.ToString();
    }

    static void AddExample(StringBuilder body, string title, SignalObservation example) => body.Append("<section><h2>").Append(title).Append("</h2><div class=\"row\">").Append(Board(example.Input)).Append("<code>").Append(E(example.Program)).Append(" →</code>").Append(Board(example.Output)).Append("</div></section>");
    static string Board(int[] cells)
    {
        SignalFoundryEngine.ValidateBoard(cells);
        return "<div class=\"board\">" + string.Concat(cells.Select(cell => $"<span class=\"cell c{cell}\">{new[] { "·", "R", "B", "Y" }[cell]}</span>")) + "</div>";
    }
    static string E(string? value) => WebUtility.HtmlEncode(value ?? "");
}
