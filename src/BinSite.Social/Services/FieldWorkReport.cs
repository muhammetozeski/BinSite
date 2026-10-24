using System.Globalization;
using System.Net;
using System.Text;

namespace BinSite.Social.Services;

/// <summary>Creates a self-contained HTML handoff report with real embedded photos and unresolved requirements.</summary>
public static class FieldWorkReport
{
    /// <summary>Renders one work order without scripts, external assets, or a false claim of verified completion.</summary>
    public static string Create(FieldWorkOrder order)
    {
        var issues = FieldWorkRules.Issues(order);
        var complete = order.Status == FieldWorkStatus.Complete && issues.Count == 0;
        var html = new StringBuilder("<!doctype html><html lang=\"en\"><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>");
        html.Append(E(order.Title)).Append(" — Field Work Pack</title><style>body{font:15px/1.5 system-ui,sans-serif;color:#111;background:#fff;max-width:1000px;margin:25px auto;padding:20px}h1{font-size:27px}h2{font-size:20px}table{width:100%;border-collapse:collapse}th,td{border:1px solid #555;padding:9px;text-align:left;vertical-align:top}th{width:26%}.notice{border:2px solid #111;padding:12px}article{border-top:2px solid #111;margin:20px 0;padding-top:12px;break-inside:avoid}img{display:block;max-width:100%;max-height:600px;object-fit:contain;margin:10px 0}.note{white-space:pre-wrap}.muted{color:#444}li{margin:6px 0}@media print{body{margin:0;padding:8mm;max-width:none}a{color:#111}img{max-height:160mm}article{break-inside:avoid}h1,h2,h3{break-after:avoid}}</style><body><header><p>FIELD WORK PACK · HANDOFF REPORT</p><h1>");
        html.Append(E(order.Title)).Append("</h1><p class=\"notice\">").Append(complete ? "Complete — review recorded" : "Draft — not a completed handoff").Append("</p></header><table>");
        Row(html, "Status", FieldWorkRules.StatusLabel(order.Status));
        if (!string.IsNullOrWhiteSpace(order.Reference)) Row(html, "Work order reference", order.Reference);
        if (!string.IsNullOrWhiteSpace(order.CustomerLabel)) Row(html, "Customer / site label", order.CustomerLabel);
        Row(html, "Checklist", $"{order.TemplateTitle} · revision {order.TemplateRevision}");
        Row(html, "Order revision", order.Revision.ToString(CultureInfo.InvariantCulture));
        Row(html, "Created", order.CreatedAt.ToString("u", CultureInfo.InvariantCulture));
        Row(html, "Last changed", order.UpdatedAt.ToString("u", CultureInfo.InvariantCulture));
        if (order.CompletedAt is { } finished) Row(html, "Completed", finished.ToString("u", CultureInfo.InvariantCulture));
        if (!string.IsNullOrWhiteSpace(order.ReviewerLabel)) Row(html, "Reviewer label", order.ReviewerLabel);
        html.Append("</table><h2>Outstanding items</h2>");
        if (issues.Count == 0) html.Append("<p>No unresolved checklist requirements.</p>");
        else { html.Append("<ul>"); foreach (var issue in issues) html.Append("<li><strong>").Append(E(issue.Label)).Append(":</strong> ").Append(E(issue.Message)).Append("</li>"); html.Append("</ul>"); }
        html.Append("<h2>Recorded work</h2>");
        foreach (var field in order.Fields)
        {
            var answer = order.Answers.FirstOrDefault(value => value.FieldId == field.Id);
            html.Append("<article><h3>").Append(E(field.Label)).Append(field.Required ? " · Required" : " · Optional").Append("</h3>");
            if (!string.IsNullOrWhiteSpace(field.Instructions)) html.Append("<p>").Append(E(field.Instructions)).Append("</p>");
            if (field.Kind == FieldWorkFieldKind.Confirmation) html.Append("<p><strong>").Append(answer?.Confirmed == true ? "Confirmed" : "Not confirmed").Append("</strong></p>");
            else if (field.Kind == FieldWorkFieldKind.Measurement)
            {
                html.Append("<p>Recorded value: <strong>").Append(E(string.IsNullOrWhiteSpace(answer?.NumberText) ? "Not entered" : answer.NumberText)).Append(" ").Append(E(field.Unit)).Append("</strong></p><p class=\"muted\">Allowed range: ")
                    .Append(field.Minimum?.ToString(CultureInfo.InvariantCulture) ?? "No minimum").Append(" to ").Append(field.Maximum?.ToString(CultureInfo.InvariantCulture) ?? "No maximum").Append("</p>");
            }
            else if (answer?.Photo is { } photo && FieldWorkRules.HasRasterEvidence(photo))
            {
                html.Append("<p>").Append(E(photo.FileName)).Append(" · ").Append(photo.Width).Append(" × ").Append(photo.Height).Append(" pixels</p><img src=\"").Append(photo.DataUrl).Append("\" alt=\"").Append(E(field.Label)).Append("\">");
            }
            else html.Append("<p><strong>No photo attached.</strong></p>");
            if (!string.IsNullOrWhiteSpace(answer?.Note)) html.Append("<p class=\"note\">").Append(E(answer.Note)).Append("</p>");
            foreach (var issue in issues.Where(value => value.FieldId == field.Id)) html.Append("<p><strong>Unresolved:</strong> ").Append(E(issue.Message)).Append("</p>");
            html.Append("</article>");
        }
        if (!string.IsNullOrWhiteSpace(order.Notes)) html.Append("<h2>Work-order notes</h2><p class=\"note\">").Append(E(order.Notes)).Append("</p>");
        html.Append("<h2>Work history</h2><ol>");
        foreach (var entry in order.History) html.Append("<li>").Append(E(entry.At.ToString("u", CultureInfo.InvariantCulture))).Append(" · <strong>").Append(E(entry.Action)).Append("</strong> · ").Append(E(entry.Detail)).Append("</li>");
        html.Append("</ol><footer><p>This report records the author’s review. It is not a signed or independent inspection.</p><p>Attached photos are included in this file.</p></footer></body></html>");
        return html.ToString();
    }

    /// <summary>Encodes all user-entered text before inclusion in exported markup.</summary>
    static string E(string? value) => WebUtility.HtmlEncode(value ?? "");

    /// <summary>Writes an encoded metadata row to the handoff table.</summary>
    static void Row(StringBuilder html, string label, string value) => html.Append("<tr><th>").Append(E(label)).Append("</th><td>").Append(E(value)).Append("</td></tr>");
}
