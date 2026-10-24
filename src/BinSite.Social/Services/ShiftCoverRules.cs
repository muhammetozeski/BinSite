using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BinSite.Social.Services;

/// <summary>One rule engine for imported definitions, automatic candidates, manual assignments and local swaps.</summary>
public static class ShiftCoverRules
{
    public const int MaximumProjectBytes = 2 * 1024 * 1024;
    public const int MaximumPeople = 60;
    public const int MaximumShifts = 120;
    public const int MaximumSeats = 240;
    const string ClockFormat = "yyyy-MM-dd'T'HH:mm";
    static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false, MaxDepth = 16, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    /// <summary>Creates an empty independent local roster without sample staff or assumed availability.</summary>
    public static ShiftCoverProject Create() => new();

    /// <summary>Reads a strict minute-resolution wall clock without consulting the computer's time zone.</summary>
    public static DateTime Clock(string value)
    {
        if (!DateTime.TryParseExact(value, ClockFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time) || time.Year < 2020 || time.Year > 2100)
            throw new InvalidOperationException("Use a complete local date and time between 2020 and 2100, with minute precision.");
        return DateTime.SpecifyKind(time, DateTimeKind.Unspecified);
    }

    /// <summary>Formats input values independently of browser or operating-system locale.</summary>
    public static string ClockText(DateTime time) => time.ToString(ClockFormat, CultureInfo.InvariantCulture);

    /// <summary>Converts a project wall-clock value to an actual UTC instant using its explicit fixed offset.</summary>
    public static DateTime Utc(string value, int offsetMinutes) => new DateTimeOffset(Clock(value), TimeSpan.FromMinutes(offsetMinutes)).UtcDateTime;

    /// <summary>Displays the fixed-offset choice, including negative and fractional-hour offsets.</summary>
    public static string OffsetLabel(int minutes) => $"UTC{(minutes < 0 ? "−" : "+")}{Math.Abs(minutes) / 60:00}:{Math.Abs(minutes) % 60:00}";

    /// <summary>Returns exact minute duration; all accepted shifts end strictly after they start.</summary>
    public static int Minutes(ShiftCoverShift shift) => (int)(Clock(shift.End) - Clock(shift.Start)).TotalMinutes;

    /// <summary>Serializes structurally valid data, retaining current conflicts as visible drafts rather than deleting them.</summary>
    public static string Serialize(ShiftCoverProject project)
    {
        ValidateStructure(project);
        var json = JsonSerializer.Serialize(project, JsonOptions);
        if (Encoding.UTF8.GetByteCount(json) > MaximumProjectBytes) throw new InvalidOperationException("The local project exceeds the 2 MB JSON limit. Export a smaller planning period.");
        return json;
    }

    /// <summary>Rejects malformed, unknown, oversized or referentially inconsistent imports before replacing local data.</summary>
    public static ShiftCoverProject Restore(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || Encoding.UTF8.GetByteCount(json) > MaximumProjectBytes) throw new InvalidOperationException("Choose a nonempty Shift & Cover project JSON file no larger than 2 MB.");
        try
        {
            var project = JsonSerializer.Deserialize<ShiftCoverProject>(json, JsonOptions) ?? throw new InvalidOperationException("The roster document is empty.");
            ValidateStructure(project);
            return project;
        }
        catch (JsonException) { throw new InvalidOperationException("The JSON does not match the complete Shift & Cover format. The current project was kept."); }
    }

    /// <summary>Produces a detached copy so a failed edit or solver pass cannot partly modify the current roster.</summary>
    public static ShiftCoverProject Clone(ShiftCoverProject project) => Restore(Serialize(project));

    /// <summary>Commits a structurally valid detached change and advances its export revision.</summary>
    public static ShiftCoverProject Change(ShiftCoverProject project, Action<ShiftCoverProject> change)
    {
        var next = Clone(project);
        change(next);
        next.Revision = checked(next.Revision + 1);
        next.UpdatedAt = DateTimeOffset.UtcNow;
        _ = Serialize(next);
        return next;
    }

    /// <summary>Checks bounded definitions, globally unique identities, references and minute-resolution time windows.</summary>
    public static void ValidateStructure(ShiftCoverProject project)
    {
        if (project is null || project.FormatVersion != 1 || project.Id == Guid.Empty || project.Revision < 1 || project.CreatedAt == default || project.UpdatedAt < project.CreatedAt) throw new InvalidOperationException("Unsupported or incomplete roster metadata.");
        Text(project.Title, "Project title", 1, 140); Text(project.Notes, "Project notes", 0, 6000); Text(project.ExampleOrigin, "Example origin", 0, 140);
        if (project.UtcOffsetMinutes < -720 || project.UtcOffsetMinutes > 840 || project.UtcOffsetMinutes % 15 != 0) throw new InvalidOperationException("Choose a fixed UTC offset from −12:00 to +14:00 in 15-minute steps.");
        if (project.Roles is null || project.People is null || project.Shifts is null || project.Assignments is null || project.Roles.Count > 16 || project.People.Count > MaximumPeople || project.Shifts.Count > MaximumShifts || project.Assignments.Count > MaximumSeats) throw new InvalidOperationException("Limits: 16 roles, 60 people, 120 shifts and 240 seats per project.");
        var ids = new HashSet<Guid> { project.Id };
        void Unique(Guid id) { if (id == Guid.Empty || !ids.Add(id)) throw new InvalidOperationException("Every project record needs a distinct nonempty identity."); }
        foreach (var role in project.Roles) { if (role is null) throw new InvalidOperationException("A role is missing."); Unique(role.Id); Text(role.Name, "Role", 1, 80); }
        if (project.Roles.GroupBy(value => value.Name.Trim(), StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1)) throw new InvalidOperationException("Use distinct role names.");
        var roles = project.Roles.Select(value => value.Id).ToHashSet();
        var dates = new List<DateTime>();
        foreach (var person in project.People)
        {
            if (person is null) throw new InvalidOperationException("A person is missing.");
            Unique(person.Id); Text(person.Label, "Person label", 1, 100);
            if (person.RoleIds is null || person.RoleIds.Count > 16 || person.RoleIds.Distinct().Count() != person.RoleIds.Count || person.RoleIds.Any(id => !roles.Contains(id))) throw new InvalidOperationException("A person's skills must reference distinct existing roles.");
            if (person.MaximumTotalHours < 0 || person.MaximumTotalHours > 2000 || person.MaximumShiftHours < 0.25m || person.MaximumShiftHours > 24 || person.MinimumRestMinutes < 0 || person.MinimumRestMinutes > 2880) throw new InvalidOperationException("Use total hours from 0 to 2,000, shift hours from 0.25 to 24, and rest from 0 to 2,880 minutes.");
            if (person.Availability is null || person.Availability.Count > 24) throw new InvalidOperationException("A person supports up to 24 explicit availability windows.");
            foreach (var window in person.Availability)
            {
                if (window is null) throw new InvalidOperationException("An availability window is missing.");
                var start = Clock(window.Start); var end = Clock(window.End);
                if (end <= start || end - start > TimeSpan.FromDays(62)) throw new InvalidOperationException("Availability must end after it starts, within a 62-day planning period.");
                dates.Add(start); dates.Add(end);
            }
        }
        if (project.People.GroupBy(value => value.Label.Trim(), StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1)) throw new InvalidOperationException("Use distinct person labels so assignments remain unambiguous.");
        var requiredSeats = 0;
        foreach (var shift in project.Shifts)
        {
            if (shift is null) throw new InvalidOperationException("A shift is missing.");
            Unique(shift.Id); Text(shift.Title, "Shift title", 1, 140);
            var start = Clock(shift.Start); var end = Clock(shift.End);
            if (!roles.Contains(shift.RoleId) || shift.RequiredPeople < 1 || shift.RequiredPeople > 12 || end <= start || end - start > TimeSpan.FromHours(24)) throw new InvalidOperationException("A shift needs an existing role, 1–12 seats, and an interval longer than zero and no longer than 24 hours.");
            dates.Add(start); dates.Add(end); requiredSeats += shift.RequiredPeople;
        }
        if (requiredSeats > MaximumSeats) throw new InvalidOperationException("A project supports up to 240 required seats. Use a shorter planning period.");
        if (dates.Count > 0 && dates.Max() - dates.Min() > TimeSpan.FromDays(62)) throw new InvalidOperationException("Keep all shifts and availability within one 62-day planning period.");
        var people = project.People.Select(value => value.Id).ToHashSet();
        var shifts = project.Shifts.ToDictionary(value => value.Id);
        var seats = new HashSet<(Guid, int)>();
        foreach (var assignment in project.Assignments)
        {
            if (assignment is null) throw new InvalidOperationException("An assignment is missing.");
            Unique(assignment.Id);
            if (!people.Contains(assignment.PersonId) || !shifts.TryGetValue(assignment.ShiftId, out var shift) || assignment.Seat < 0 || assignment.Seat >= shift.RequiredPeople || !seats.Add((assignment.ShiftId, assignment.Seat))) throw new InvalidOperationException("An assignment has an unknown person, shift, duplicate seat or invalid seat number.");
        }
    }

    /// <summary>Checks one candidate against actual skills, unioned availability, duration, total hours, overlap and rest.</summary>
    public static List<ShiftCoverIssue> CandidateIssues(ShiftCoverProject project, ShiftCoverShift shift, ShiftCoverPerson person, Guid? ignoreAssignment = null, bool baselineOnly = false)
    {
        var issues = new List<ShiftCoverIssue>();
        var start = Clock(shift.Start); var end = Clock(shift.End);
        var minutes = Minutes(shift);
        if (!person.RoleIds.Contains(shift.RoleId)) issues.Add(new("skill", "Required role is not in this person's skills."));
        if (!Available(person, start, end)) issues.Add(new("availability", "The complete shift is outside the entered availability windows."));
        if (minutes > person.MaximumShiftHours * 60) issues.Add(new("shift-hours", $"Shift duration exceeds the entered {Hours(person.MaximumShiftHours)}-hour single-shift limit."));
        if (baselineOnly) return issues;
        var existing = project.Assignments.Where(value => value.PersonId == person.Id && value.Id != ignoreAssignment).Select(value => project.Shifts.First(item => item.Id == value.ShiftId)).ToArray();
        if (existing.Sum(Minutes) + minutes > person.MaximumTotalHours * 60) issues.Add(new("total-hours", $"Assignment exceeds the entered {Hours(person.MaximumTotalHours)}-hour project total."));
        foreach (var other in existing)
        {
            var otherStart = Clock(other.Start); var otherEnd = Clock(other.End);
            if (start < otherEnd && otherStart < end) { issues.Add(new("overlap", $"Overlaps {other.Title} ({Display(other.Start)} to {Display(other.End)}).")); continue; }
            var gap = start >= otherEnd ? start - otherEnd : otherStart - end;
            if (gap.TotalMinutes < person.MinimumRestMinutes) issues.Add(new("rest", $"Only {gap.TotalMinutes:0} minutes separate this shift from {other.Title}; the entered rest minimum is {person.MinimumRestMinutes}."));
        }
        return issues;
    }

    /// <summary>Merges touching availability windows; a gap of even one minute cannot cover a full shift.</summary>
    static bool Available(ShiftCoverPerson person, DateTime start, DateTime end)
    {
        var cursor = start;
        foreach (var window in person.Availability.OrderBy(value => value.Start, StringComparer.Ordinal))
        {
            var from = Clock(window.Start); var to = Clock(window.End);
            if (to <= cursor) continue;
            if (from > cursor) return false;
            cursor = to;
            if (cursor >= end) return true;
        }
        return false;
    }

    /// <summary>Returns every current assignment conflict without converting an unfilled seat into an infeasibility claim.</summary>
    public static List<ShiftCoverIssue> AssignmentIssues(ShiftCoverProject project)
    {
        ValidateStructure(project);
        var result = new List<ShiftCoverIssue>();
        foreach (var assignment in project.Assignments)
        {
            var shift = project.Shifts.First(value => value.Id == assignment.ShiftId);
            var person = project.People.First(value => value.Id == assignment.PersonId);
            result.AddRange(CandidateIssues(project, shift, person, assignment.Id).Select(issue => issue with { AssignmentId = assignment.Id }));
        }
        return result;
    }

    /// <summary>Explains current eligible and blocked people after treating the selected seat as vacant.</summary>
    public static List<ShiftCoverCandidate> Candidates(ShiftCoverProject project, Guid shiftId, int seat)
    {
        var shift = project.Shifts.FirstOrDefault(value => value.Id == shiftId) ?? throw new InvalidOperationException("Select an existing shift.");
        var occupant = project.Assignments.FirstOrDefault(value => value.ShiftId == shiftId && value.Seat == seat);
        return project.People.OrderBy(value => value.Label, StringComparer.Ordinal).Select(person => new ShiftCoverCandidate(person.Id, person.Label,
            project.Assignments.Where(value => value.PersonId == person.Id && value.Id != occupant?.Id).Sum(value => Minutes(project.Shifts.First(item => item.Id == value.ShiftId))) / 60m,
            CandidateIssues(project, shift, person, occupant?.Id))).ToList();
    }

    /// <summary>Replaces or clears one seat atomically; invalid replacements preserve its previous occupant.</summary>
    public static ShiftCoverProject SetSeat(ShiftCoverProject project, Guid shiftId, int seat, Guid? personId)
    {
        return Change(project, next =>
        {
            var shift = next.Shifts.FirstOrDefault(value => value.Id == shiftId) ?? throw new InvalidOperationException("Select an existing shift.");
            if (seat < 0 || seat >= shift.RequiredPeople) throw new InvalidOperationException("Choose an existing numbered seat.");
            next.Assignments.RemoveAll(value => value.ShiftId == shiftId && value.Seat == seat);
            if (personId is null) return;
            var person = next.People.FirstOrDefault(value => value.Id == personId) ?? throw new InvalidOperationException("Select an existing person.");
            var issues = CandidateIssues(next, shift, person);
            if (issues.Count > 0) throw new InvalidOperationException("Assignment rejected: " + string.Join(" ", issues.Select(value => value.Message)));
            next.Assignments.Add(new() { ShiftId = shiftId, Seat = seat, PersonId = person.Id });
        });
    }

    /// <summary>Swaps two occupied seats only when both resulting assignments pass the same candidate rules.</summary>
    public static ShiftCoverProject Swap(ShiftCoverProject project, Guid firstId, Guid secondId)
    {
        if (firstId == secondId) throw new InvalidOperationException("Choose two different occupied seats.");
        return Change(project, next =>
        {
            var first = next.Assignments.FirstOrDefault(value => value.Id == firstId) ?? throw new InvalidOperationException("The first assignment no longer exists.");
            var second = next.Assignments.FirstOrDefault(value => value.Id == secondId) ?? throw new InvalidOperationException("The second assignment no longer exists.");
            if (first.PersonId == second.PersonId) throw new InvalidOperationException("These seats already belong to the same person.");
            (first.PersonId, second.PersonId) = (second.PersonId, first.PersonId);
            var changed = new[] { first, second };
            var issues = changed.SelectMany(value => CandidateIssues(next, next.Shifts.First(item => item.Id == value.ShiftId), next.People.First(item => item.Id == value.PersonId), value.Id)).ToList();
            if (issues.Count > 0) throw new InvalidOperationException("Swap rejected: " + string.Join(" ", issues.Select(value => value.Message).Distinct()));
        });
    }

    /// <summary>Formats user-facing dates without relying on a Turkish or operating-system culture.</summary>
    public static string Display(string value) => Clock(value).ToString("ddd dd MMM HH:mm", CultureInfo.InvariantCulture);
    /// <summary>Formats decimal hours consistently in English UI and exports.</summary>
    public static string Hours(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    /// <summary>Rejects control characters while allowing normal multiline notes and international labels.</summary>
    static void Text(string value, string label, int minimum, int maximum)
    {
        if (value is null || value.Trim().Length < minimum || value.Length > maximum || value.Any(character => char.IsControl(character) && character is not '\n' and not '\r' and not '\t')) throw new InvalidOperationException($"{label} must contain {minimum}–{maximum} characters without control codes.");
    }
}
