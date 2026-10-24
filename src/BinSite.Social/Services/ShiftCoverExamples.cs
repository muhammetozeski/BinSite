namespace BinSite.Social.Services;

/// <summary>Eight fictional editable planning briefs, with dated availability and different real constraint patterns.</summary>
public static class ShiftCoverExamples
{
    public static readonly IReadOnlyList<ShiftCoverExample> All = new[]
    {
        new ShiftCoverExample("gallery", "Gallery weekend", "Two desks, staggered availability, and a small pool of floor hosts."),
        new("studio", "Studio changeovers", "Short recording support slots compete for two sound-capable people."),
        new("conference", "Community conference", "Check-in and room support need different capabilities during overlapping sessions."),
        new("makers", "Maker space opening", "A deliberately tight project-hour budget leaves some coverage unresolved."),
        new("film", "Overnight film desk", "Dated overnight slots test overlap and entered rest across midnight."),
        new("library", "Library pop-up", "Adjacent availability windows can jointly cover a shift; gaps cannot."),
        new("market", "Weekend market", "Two people per seat group, mixed skills, and a reserved manual assignment."),
        new("festival", "Small arts festival", "A three-day timetable balances short daily blocks across a larger fictional team.")
    };

    /// <summary>Creates a detached roster with explicitly fictional labels; no example assignment represents an actual worker.</summary>
    public static ShiftCoverProject Create(string id)
    {
        var example = All.FirstOrDefault(value => value.Id == id) ?? throw new InvalidOperationException("Choose an existing fictional example.");
        var project = new ShiftCoverProject { Title = example.Title, ExampleOrigin = example.Title, Notes = "Fictional practice roster. Replace the names, availability and limits with your team's agreed plan.", UtcOffsetMinutes = id switch { "film" => -300, "library" => 330, "festival" => 60, _ => 0 } };
        var roles = id switch
        {
            "gallery" => new[] { "Welcome desk", "Floor host" }, "studio" => ["Session desk", "Sound support"],
            "conference" => ["Check-in", "Room support", "Speaker desk"], "makers" => ["Front desk", "Workshop host"],
            "film" => ["Production desk", "Media handoff"], "library" => ["Loan desk", "Reading host"],
            "market" => ["Welcome", "Stock desk", "Collection desk"], _ => ["Entry desk", "Venue host", "Artist desk"]
        };
        project.Roles = roles.Select(name => new ShiftCoverRole { Name = name }).ToList();
        var names = new[] { "Aster", "Birch", "Cedar", "Dune", "Elm", "Flint", "Grove", "Hazel", "Indigo", "Juniper", "Kite", "Linden" };
        var count = id == "festival" ? 12 : id == "conference" ? 9 : id == "makers" ? 4 : 6;
        var date = new DateTime(2030, 4, 6);
        for (var index = 0; index < count; index++)
        {
            var person = new ShiftCoverPerson { Label = names[index] + " (fictional)", MaximumTotalHours = id == "makers" ? 3 : id == "festival" ? 12 : 10, MaximumShiftHours = id == "studio" ? 3 : id == "film" ? 6 : 5, MinimumRestMinutes = id == "film" ? 600 : id == "studio" ? 30 : 60 };
            person.RoleIds.Add(project.Roles[index % roles.Length].Id);
            if (index % 3 == 0) person.RoleIds = project.Roles.Select(role => role.Id).ToList();
            var days = id == "festival" ? 3 : 2;
            for (var day = 0; day < days; day++)
            {
                var start = date.AddDays(day).AddHours(8 + index % 3);
                var end = date.AddDays(day).AddHours(index % 2 == 0 ? 18 : 15);
                if (id == "film") { start = date.AddDays(day).AddHours(18 + index % 2); end = date.AddDays(day + 1).AddHours(8); }
                person.Availability.Add(Window(start, end));
            }
            project.People.Add(person);
        }
        if (id == "library")
        {
            project.People[0].Availability = [Window(date.AddHours(8), date.AddHours(12)), Window(date.AddHours(12), date.AddHours(16))];
            project.People[1].Availability = [Window(date.AddHours(8), date.AddHours(11)), Window(date.AddHours(12), date.AddHours(16))];
        }
        var dayCount = id == "festival" ? 3 : 2;
        for (var day = 0; day < dayCount; day++)
        {
            for (var block = 0; block < (id == "studio" ? 4 : 2); block++)
            {
                for (var role = 0; role < roles.Length; role++)
                {
                    var start = date.AddDays(day).AddHours(id == "film" ? 20 + block * 6 : id == "studio" ? 9 + block * 2 : 9 + block * 5);
                    var hours = id == "film" ? 5 : id == "studio" ? 2 : 4;
                    project.Shifts.Add(new() { Title = roles[role] + $" · day {day + 1}, block {block + 1}", RoleId = project.Roles[role].Id, Start = ShiftCoverRules.ClockText(start), End = ShiftCoverRules.ClockText(start.AddHours(hours)), RequiredPeople = id is "market" or "conference" && role == 0 ? 2 : 1 });
                }
            }
        }
        if (id == "market")
        {
            var shift = project.Shifts[0];
            project.Assignments.Add(new() { ShiftId = shift.Id, Seat = 0, PersonId = project.People[0].Id, Locked = true });
        }
        _ = ShiftCoverRules.Serialize(project);
        return project;
    }

    /// <summary>Builds a dated minute-resolution example window.</summary>
    static ShiftCoverWindow Window(DateTime start, DateTime end) => new() { Start = ShiftCoverRules.ClockText(start), End = ShiftCoverRules.ClockText(end) };
}
