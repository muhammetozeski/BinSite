using System.Globalization;
using System.Net;
using System.Text;
namespace BinSite.Social.Services;

/// <summary>Portable schedules containing actual local assignments and explicit partial/conflict status.</summary>
public static class ShiftCoverExports
{
    /// <summary>Explains why a seat remains open using the current roster, without claiming that no other roster can fill it.</summary>
    public static string UnfilledReason(ShiftCoverProject project, Guid shiftId, int seat)
    {
        var candidates = ShiftCoverRules.Candidates(project, shiftId, seat);
        if (candidates.Count == 0) return "No people have been entered. Add people, skills and availability.";
        var eligible = candidates.Count(value => value.Issues.Count == 0);
        if (eligible > 0) return $"{eligible} currently eligible person(s). This seat was left open by the current bounded search or manual plan; assigning someone can affect other seats.";
        var counts = candidates.SelectMany(value => value.Issues.Select(issue => issue.Code).Distinct()).GroupBy(code => code).OrderBy(group => group.Key, StringComparer.Ordinal).Select(group => $"{group.Key}: {group.Count()}");
        return "No currently eligible person in this partial schedule. People blocked by " + string.Join("; ", counts) + ". A person may have multiple blockers. Different earlier assignments or changed inputs may help; this is not an infeasibility proof.";
    }

    /// <summary>Exports every required seat, including open seats and actual constraint conflicts, with spreadsheet formula protection.</summary>
    public static string Csv(ShiftCoverProject project, Guid? personId = null)
    {
        ShiftCoverRules.ValidateStructure(project);
        if (personId.HasValue && !project.People.Any(value => value.Id == personId)) throw new InvalidOperationException("Choose an existing person.");
        var issues = ShiftCoverRules.AssignmentIssues(project);
        var output = new StringBuilder("\uFEFF");
        Row(output, "Project", "Fixed offset", "Shift", "Role", "Start local", "End local", "Start UTC", "End UTC", "Seat", "Person label", "Hours", "Status", "Locked", "Details");
        foreach (var shift in project.Shifts.OrderBy(value => value.Start, StringComparer.Ordinal).ThenBy(value => value.Id))
        {
            for (var seat = 0; seat < shift.RequiredPeople; seat++)
            {
                var assignment = project.Assignments.FirstOrDefault(value => value.ShiftId == shift.Id && value.Seat == seat);
                if (personId.HasValue && assignment?.PersonId != personId) continue;
                var errors = issues.Where(value => value.AssignmentId == assignment?.Id).ToArray();
                Row(output, project.Title, ShiftCoverRules.OffsetLabel(project.UtcOffsetMinutes), shift.Title, project.Roles.First(value => value.Id == shift.RoleId).Name,
                    shift.Start, shift.End, Stamp(ShiftCoverRules.Utc(shift.Start, project.UtcOffsetMinutes)), Stamp(ShiftCoverRules.Utc(shift.End, project.UtcOffsetMinutes)), (seat + 1).ToString(CultureInfo.InvariantCulture),
                    assignment is null ? "" : project.People.First(value => value.Id == assignment.PersonId).Label, ShiftCoverRules.Hours(ShiftCoverRules.Minutes(shift) / 60m),
                    assignment is null ? "UNFILLED" : errors.Length > 0 ? "CONFLICT" : "LOCAL PLAN", assignment?.Locked == true ? "Yes" : "No",
                    assignment is null ? UnfilledReason(project, shift.Id, seat) : errors.Length > 0 ? string.Join(" ", errors.Select(value => value.Message)) : "Organizer-entered plan; no employee acceptance or legal compliance is implied.");
            }
        }
        return output.ToString();
    }

    /// <summary>Creates UTC iCalendar events for actual assignments, refusing current conflicts and excluding open seats.</summary>
    public static string Calendar(ShiftCoverProject project, Guid? personId = null)
    {
        ShiftCoverRules.ValidateStructure(project);
        if (personId.HasValue && !project.People.Any(value => value.Id == personId)) throw new InvalidOperationException("Choose an existing person.");
        if (ShiftCoverRules.AssignmentIssues(project).Count > 0) throw new InvalidOperationException("Resolve the current assignment conflicts before exporting calendars. JSON and the conflict-labelled CSV remain available.");
        var selected = project.Assignments.Where(value => !personId.HasValue || value.PersonId == personId).OrderBy(value => project.Shifts.First(shift => shift.Id == value.ShiftId).Start, StringComparer.Ordinal).ThenBy(value => value.ShiftId).ThenBy(value => value.Seat).ToArray();
        if (selected.Length == 0) throw new InvalidOperationException("There are no actual assignments to include in this calendar.");
        var output = new StringBuilder();
        Line(output, "BEGIN:VCALENDAR"); Line(output, "VERSION:2.0"); Line(output, "PRODID:-//BinSite//Shift and Cover Desk//EN"); Line(output, "CALSCALE:GREGORIAN");
        var unfilled = project.Shifts.Sum(value => value.RequiredPeople) - project.Assignments.Count;
        foreach (var assignment in selected)
        {
            var shift = project.Shifts.First(value => value.Id == assignment.ShiftId);
            var person = project.People.First(value => value.Id == assignment.PersonId);
            var role = project.Roles.First(value => value.Id == shift.RoleId);
            Line(output, "BEGIN:VEVENT");
            Line(output, $"UID:{project.Id:N}-{shift.Id:N}-{assignment.Seat}-{person.Id:N}@shift-cover-desk");
            Line(output, "DTSTAMP:" + Stamp(project.UpdatedAt.UtcDateTime));
            Line(output, "SEQUENCE:" + project.Revision.ToString(CultureInfo.InvariantCulture));
            Line(output, "DTSTART:" + Stamp(ShiftCoverRules.Utc(shift.Start, project.UtcOffsetMinutes)));
            Line(output, "DTEND:" + Stamp(ShiftCoverRules.Utc(shift.End, project.UtcOffsetMinutes)));
            Line(output, "SUMMARY:" + CalendarText(shift.Title + " — " + person.Label));
            Line(output, "DESCRIPTION:" + CalendarText($"{project.Title}\nRole: {role.Name}\nPerson label: {person.Label}\nPlanning clock: {ShiftCoverRules.OffsetLabel(project.UtcOffsetMinutes)} (fixed offset; no daylight-saving conversion).\nSeat {assignment.Seat + 1}. {unfilled} seat(s) remain open across this roster.\nOrganizer-entered local plan, not an employee acceptance. These imported files do not automatically update or delete previously imported events."));
            Line(output, "STATUS:TENTATIVE"); Line(output, "TRANSP:OPAQUE"); Line(output, "END:VEVENT");
        }
        Line(output, "END:VCALENDAR");
        return output.ToString();
    }

    /// <summary>Creates a standalone print view with the actual per-person timetable and every unresolved seat.</summary>
    public static string Html(ShiftCoverProject project)
    {
        ShiftCoverRules.ValidateStructure(project);
        var issues = ShiftCoverRules.AssignmentIssues(project);
        var needed = project.Shifts.Sum(value => value.RequiredPeople);
        var output = new StringBuilder("<!doctype html><html lang=\"en\"><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none';style-src 'unsafe-inline'\"><title>Shift &amp; Cover schedule</title><style>body{font:15px Arial,sans-serif;color:#111;background:#fff;max-width:1100px;margin:24px auto;padding:0 16px}h1,h2,h3{margin:20px 0 10px}table{border-collapse:collapse;width:100%;margin:12px 0 24px}th,td{padding:8px;border:1px solid #555;text-align:left;vertical-align:top}.warning{padding:12px;border:3px solid #a23b00}pre{white-space:pre-wrap}article{break-inside:avoid}@media print{body{margin:0}h2{break-after:avoid}}</style><body>");
        output.Append("<h1>").Append(E(project.Title)).Append("</h1><p>Fixed planning clock: ").Append(E(ShiftCoverRules.OffsetLabel(project.UtcOffsetMinutes))).Append(". Revision ").Append(project.Revision).Append(".</p><p class=\"warning\">").Append(project.Assignments.Count).Append(" / ").Append(needed).Append(" seats assigned; ").Append(issues.Count).Append(" constraint issue(s). Organizer-entered local plan; no worker consent or legal compliance is implied.</p>");
        if (!string.IsNullOrEmpty(project.ExampleOrigin)) output.Append("<p>Started from the fictional editable example: ").Append(E(project.ExampleOrigin)).Append(".</p>");
        output.Append("<pre>").Append(E(project.Notes)).Append("</pre>");
        foreach (var person in project.People.OrderBy(value => value.Label, StringComparer.Ordinal))
        {
            var assignments = project.Assignments.Where(value => value.PersonId == person.Id).OrderBy(value => project.Shifts.First(shift => shift.Id == value.ShiftId).Start, StringComparer.Ordinal).ToArray();
            output.Append("<article><h2>").Append(E(person.Label)).Append("</h2><p>").Append(ShiftCoverRules.Hours(assignments.Sum(value => ShiftCoverRules.Minutes(project.Shifts.First(shift => shift.Id == value.ShiftId))) / 60m)).Append(" / ").Append(ShiftCoverRules.Hours(person.MaximumTotalHours)).Append(" entered project hours; minimum rest ").Append(person.MinimumRestMinutes).Append(" minutes.</p><table><thead><tr><th>Shift / role</th><th>Start</th><th>End</th><th>Seat</th><th>Validation</th></tr></thead><tbody>");
            foreach (var assignment in assignments)
            {
                var shift = project.Shifts.First(value => value.Id == assignment.ShiftId);
                var messages = issues.Where(value => value.AssignmentId == assignment.Id).Select(value => value.Message).ToArray();
                output.Append("<tr><td>").Append(E(shift.Title + " / " + project.Roles.First(value => value.Id == shift.RoleId).Name)).Append("</td><td>").Append(E(ShiftCoverRules.Display(shift.Start))).Append("</td><td>").Append(E(ShiftCoverRules.Display(shift.End))).Append("</td><td>").Append(assignment.Seat + 1).Append("</td><td>").Append(E(messages.Length > 0 ? string.Join(" ", messages) : "Passes entered constraints")).Append("</td></tr>");
            }
            if (assignments.Length == 0) output.Append("<tr><td colspan=\"5\">No assigned shifts.</td></tr>");
            output.Append("</tbody></table></article>");
        }
        output.Append("<h2>Open seats</h2><ul>");
        foreach (var shift in project.Shifts.OrderBy(value => value.Start, StringComparer.Ordinal))
            for (var seat = 0; seat < shift.RequiredPeople; seat++)
                if (!project.Assignments.Any(value => value.ShiftId == shift.Id && value.Seat == seat)) output.Append("<li><strong>").Append(E(shift.Title)).Append(" — seat ").Append(seat + 1).Append("</strong>: ").Append(E(UnfilledReason(project, shift.Id, seat))).Append("</li>");
        return output.Append("</ul></body></html>").ToString();
    }

    /// <summary>Writes a quoted spreadsheet row, neutralizing formula-leading labels.</summary>
    static void Row(StringBuilder output, params string[] cells) => output.AppendLine(string.Join(",", cells.Select(value => { var text = value ?? ""; if (text.TrimStart().FirstOrDefault() is '=' or '+' or '-' or '@') text = "'" + text; return "\"" + text.Replace("\"", "\"\"") + "\""; })));
    /// <summary>Uses UTC date-time format required by the generated calendar's Z-suffixed properties.</summary>
    static string Stamp(DateTime time) => time.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
    /// <summary>Escapes iCalendar TEXT delimiters and line breaks before folding.</summary>
    static string CalendarText(string value) => value.Replace("\\", "\\\\").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n").Replace(";", "\\;").Replace(",", "\\,");
    /// <summary>Folds at 75 UTF-8 octets, counting the continuation space and never splitting a Unicode scalar.</summary>
    static void Line(StringBuilder output, string value)
    {
        var bytes = 0;
        foreach (var rune in value.EnumerateRunes())
        {
            var text = rune.ToString(); var count = Encoding.UTF8.GetByteCount(text);
            if (bytes + count > 75) { output.Append("\r\n "); bytes = 1; }
            output.Append(text); bytes += count;
        }
        output.Append("\r\n");
    }
    /// <summary>Encodes user text for standalone reports.</summary>
    static string E(string text) => WebUtility.HtmlEncode(text);
}
