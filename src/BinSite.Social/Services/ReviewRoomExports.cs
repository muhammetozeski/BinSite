using System.Globalization;
using System.Net;
using System.Text;

namespace BinSite.Social.Services;

/// <summary>Exports standalone review documents without media bytes, executable user markup or spreadsheet formulas.</summary>
public static class ReviewRoomExports
{
    /// <summary>Builds a self-contained HTML report from actual saved versions, notes and decisions.</summary>
    public static string Html(ReviewProject project, Guid? compareAsset = null, Guid? leftVersion = null, Guid? rightVersion = null)
    {
        ReviewRoomRules.Validate(project);
        var html = new StringBuilder("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><meta name=\"referrer\" content=\"no-referrer\"><meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'\"><title>");
        html.Append(E(project.Title)).Append(" — Review report</title><style>body{font:16px/1.6 system-ui,sans-serif;color:#e7ede9;background:#172128;max-width:1100px;margin:auto;padding:24px}h1{font-size:2rem}h2{border-top:2px solid #6a8388;padding-top:16px}table{width:100%;border-collapse:collapse}th,td{text-align:left;vertical-align:top;padding:8px;border:1px solid #6a8388;overflow-wrap:anywhere}pre{white-space:pre-wrap;overflow-wrap:anywhere;font:inherit}.small{font-size:13px;color:#c7d2cd}.open{border-left:4px solid #c60000;padding-left:12px}.resolved{border-left:4px solid #00652a;padding-left:12px}@media print{body{padding:0;color:#111;background:#fff}.small{color:#444}article,tr{break-inside:avoid}}</style></head><body>");
        html.Append("<h1>").Append(E(project.Title)).Append("</h1><p>").Append(Lines(project.Brief)).Append("</p><p class=\"small\">Media files are supplied separately.</p>");
        if (project.IsEditorialExample) html.Append("<p><strong>Example review</strong></p>");
        html.Append("<p class=\"small\">Updated: ").Append(E(project.UpdatedAt.ToString("u"))).Append("</p>");
        if (compareAsset is Guid assetId && leftVersion is Guid left && rightVersion is Guid right && left != right)
        {
            var asset = project.Assets.FirstOrDefault(item => item.Id == assetId);
            var a = asset?.Versions.FirstOrDefault(item => item.Id == left);
            var b = asset?.Versions.FirstOrDefault(item => item.Id == right);
            if (asset is not null && a is not null && b is not null)
            {
                html.Append("<h2>Version comparison: ").Append(E(asset.Title)).Append("</h2><table><thead><tr><th>Measure</th><th>").Append(E(a.Label)).Append("</th><th>").Append(E(b.Label)).Append("</th></tr></thead><tbody>");
                ComparisonRow(html, "File", a.FileName, b.FileName);
                ComparisonRow(html, "Recorded state", ReviewRoomRules.StateLabel(ReviewRoomRules.CurrentDecision(project, left)), ReviewRoomRules.StateLabel(ReviewRoomRules.CurrentDecision(project, right)));
                ComparisonRow(html, "Open comments", project.Comments.Count(item => item.VersionId == left && !item.Resolved).ToString(), project.Comments.Count(item => item.VersionId == right && !item.Resolved).ToString());
                ComparisonRow(html, "Resolved comments", project.Comments.Count(item => item.VersionId == left && item.Resolved).ToString(), project.Comments.Count(item => item.VersionId == right && item.Resolved).ToString());
                ComparisonRow(html, "Open blockers", project.Comments.Count(item => item.VersionId == left && !item.Resolved && item.Priority == ReviewCommentPriority.Blocking).ToString(), project.Comments.Count(item => item.VersionId == right && !item.Resolved && item.Priority == ReviewCommentPriority.Blocking).ToString());
                html.Append("</tbody></table>");
            }
        }
        foreach (var asset in project.Assets)
        {
            html.Append("<h2>").Append(E(asset.Title)).Append("</h2><p>").Append(E(asset.Kind.ToString())).Append(" · ").Append(asset.Versions.Count).Append(" version(s)</p>");
            foreach (var version in asset.Versions)
            {
                html.Append("<section><h3>").Append(E(version.Label)).Append(" — ").Append(E(ReviewRoomRules.StateLabel(ReviewRoomRules.CurrentDecision(project, version.Id)))).Append("</h3><table><tbody>");
                MetadataRow(html, "File", version.FileName);
                MetadataRow(html, "Size", version.SizeBytes >= 1048576 ? $"{version.SizeBytes / 1048576d:0.0} MB" : $"{version.SizeBytes / 1024d:0.0} KB");
                MetadataRow(html, "Media", version.DurationSeconds is double duration ? ReviewRoomRules.TimeLabel(duration) : $"{version.Width} × {version.Height} pixels");
                html.Append("</tbody></table>");
                var comments = project.Comments.Where(item => item.VersionId == version.Id).ToList();
                if (comments.Count == 0) html.Append("<p>No comments recorded for this version.</p>");
                foreach (var comment in comments)
                {
                    html.Append("<article class=\"").Append(comment.Resolved ? "resolved" : "open").Append("\"><h4>").Append(E(comment.Category.ToString())).Append(" · ").Append(comment.Priority == ReviewCommentPriority.Blocking ? "Blocking" : "Note").Append(" · ").Append(comment.Resolved ? "Resolved" : "Open").Append("</h4><p class=\"small\">").Append(E(ReviewRoomRules.AnchorLabel(comment))).Append(" · ").Append(E(comment.CreatedAt.ToString("u"))).Append(comment.Source == ReviewEntrySource.EditorialExample ? " · Example" : "").Append("</p><p>").Append(Lines(comment.Body)).Append("</p></article>");
                }
                html.Append("<h4>Recorded decisions</h4><ol>");
                foreach (var decision in project.Decisions.Where(item => item.VersionId == version.Id)) html.Append("<li><strong>").Append(E(ReviewRoomRules.StateLabel(decision.State))).Append("</strong> · ").Append(E(decision.CreatedAt.ToString("u"))).Append("<p>").Append(Lines(decision.Note)).Append("</p></li>");
                html.Append("</ol></section>");
            }
        }
        return html.Append("</body></html>").ToString();
    }

    /// <summary>Exports actual open editing work, including precise file/version and anchor references.</summary>
    public static string Worklist(ReviewProject project)
    {
        ReviewRoomRules.Validate(project);
        var csv = new StringBuilder("Project,Asset,Version,File,SHA-256,Category,Priority,Anchor,Comment,Source\r\n");
        foreach (var comment in project.Comments.Where(item => !item.Resolved))
        {
            var asset = project.Assets.First(item => item.Id == comment.AssetId);
            var version = asset.Versions.First(item => item.Id == comment.VersionId);
            string[] values = [project.Title, asset.Title, version.Label, version.FileName, version.Fingerprint, comment.Category.ToString(), comment.Priority.ToString(), ReviewRoomRules.AnchorLabel(comment), comment.Body, comment.Source.ToString()];
            csv.AppendLine(string.Join(',', values.Select(Cell)));
        }
        return "\uFEFF" + csv;
    }

    /// <summary>Escapes all user-authored text before inserting it into report markup.</summary>
    static string E(string value) => WebUtility.HtmlEncode(value);
    /// <summary>Preserves authored line breaks while keeping markup encoded.</summary>
    static string Lines(string value) => E(value).Replace("\r\n", "\n").Replace("\n", "<br>");
    /// <summary>Writes a report metadata row with encoded values.</summary>
    static void MetadataRow(StringBuilder html, string label, string value) => html.Append("<tr><th>").Append(E(label)).Append("</th><td>").Append(E(value)).Append("</td></tr>");
    /// <summary>Writes an encoded side-by-side version comparison row.</summary>
    static void ComparisonRow(StringBuilder html, string label, string left, string right) => html.Append("<tr><th>").Append(E(label)).Append("</th><td>").Append(E(left)).Append("</td><td>").Append(E(right)).Append("</td></tr>");
    /// <summary>Quotes CSV fields and neutralizes formula-leading user text for spreadsheet imports.</summary>
    static string Cell(string value)
    {
        var trimmed = value.TrimStart();
        if (trimmed.Length > 0 && "=+-@".Contains(trimmed[0])) value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
