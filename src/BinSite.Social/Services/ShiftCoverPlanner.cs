using System.Security.Cryptography;
using System.Text;
namespace BinSite.Social.Services;

/// <summary>Eight deterministic greedy passes with an explicit candidate-check budget; no optimality or infeasibility claim.</summary>
public static class ShiftCoverPlanner
{
    public const int MaximumPasses = 8;
    public const int MaximumCandidateChecks = 80000;

    /// <summary>Preserves valid locked seats, tries different deterministic priorities and retains the most-covered valid result.</summary>
    public static ShiftCoverPlan Suggest(ShiftCoverProject project)
    {
        ShiftCoverRules.ValidateStructure(project);
        if (project.Shifts.Count == 0) throw new InvalidOperationException("Add at least one shift requirement before requesting a schedule.");
        var baseline = ShiftCoverRules.Clone(project);
        baseline.Assignments.RemoveAll(value => !value.Locked);
        if (ShiftCoverRules.AssignmentIssues(baseline).Count > 0) throw new InvalidOperationException("A locked assignment conflicts with the current limits. Fix or unlock it before generating suggestions.");
        var seats = project.Shifts.SelectMany(shift => Enumerable.Range(0, shift.RequiredPeople).Select(seat => (Shift: shift, Seat: seat))).Where(value => !baseline.Assignments.Any(item => item.ShiftId == value.Shift.Id && item.Seat == value.Seat)).ToArray();
        var eligible = project.Shifts.ToDictionary(shift => shift.Id, shift => project.People.Where(person => ShiftCoverRules.CandidateIssues(baseline, shift, person, baselineOnly: true).Count == 0).OrderBy(person => person.Label, StringComparer.Ordinal).ThenBy(person => person.Id).ToArray());
        var best = baseline;
        var bestLoad = decimal.MaxValue;
        var checks = 0; var passes = 0;
        for (var pass = 0; pass < MaximumPasses && checks < MaximumCandidateChecks; pass++)
        {
            var next = ShiftCoverRules.Clone(baseline);
            var byTime = pass % 4 == 1;
            var byDuration = pass % 4 == 2;
            var sequence = seats.OrderBy(value => byTime ? ShiftCoverRules.Clock(value.Shift.Start).Ticks : byDuration ? -ShiftCoverRules.Minutes(value.Shift) : eligible[value.Shift.Id].Length)
                .ThenBy(value => byTime || byDuration ? eligible[value.Shift.Id].Length : ShiftCoverRules.Clock(value.Shift.Start).Ticks)
                .ThenByDescending(value => ShiftCoverRules.Minutes(value.Shift)).ThenBy(value => value.Shift.Id).ThenBy(value => value.Seat).ToArray();
            foreach (var seat in sequence)
            {
                ShiftCoverPerson? selected = null; decimal selectedScore = decimal.MaxValue;
                var people = eligible[seat.Shift.Id];
                for (var index = 0; index < people.Length && checks < MaximumCandidateChecks; index++)
                {
                    var person = people[(index + pass / 4) % people.Length];
                    checks++;
                    if (ShiftCoverRules.CandidateIssues(next, seat.Shift, person).Count != 0) continue;
                    var assigned = next.Assignments.Where(value => value.PersonId == person.Id).Sum(value => ShiftCoverRules.Minutes(next.Shifts.First(shift => shift.Id == value.ShiftId)));
                    var score = pass % 4 == 3 ? assigned : assigned / Math.Max(1m, person.MaximumTotalHours * 60);
                    if (score >= selectedScore) continue;
                    selected = person; selectedScore = score;
                }
                if (selected is not null) next.Assignments.Add(new() { Id = AssignmentId(project.Id, seat.Shift.Id, seat.Seat, selected.Id), ShiftId = seat.Shift.Id, Seat = seat.Seat, PersonId = selected.Id });
                if (checks >= MaximumCandidateChecks) break;
            }
            passes++;
            var load = next.People.Sum(person => { var minutes = next.Assignments.Where(value => value.PersonId == person.Id).Sum(value => ShiftCoverRules.Minutes(next.Shifts.First(shift => shift.Id == value.ShiftId))); return (decimal)minutes * minutes; });
            if (next.Assignments.Count > best.Assignments.Count || next.Assignments.Count == best.Assignments.Count && load < bestLoad) { best = next; bestLoad = load; }
        }
        best.Revision = checked(project.Revision + 1);
        best.UpdatedAt = DateTimeOffset.UtcNow;
        _ = ShiftCoverRules.Serialize(best);
        if (ShiftCoverRules.AssignmentIssues(best).Count > 0) throw new InvalidOperationException("The proposed roster failed validation. The current roster was retained.");
        return new(best, best.Assignments.Count, project.Shifts.Sum(value => value.RequiredPeople), passes, checks, checks >= MaximumCandidateChecks);
    }

    /// <summary>Makes generated assignment identities stable for the same project, seat and person across repeated suggestions.</summary>
    static Guid AssignmentId(Guid project, Guid shift, int seat, Guid person) => new(SHA256.HashData(Encoding.UTF8.GetBytes($"{project:N}/{shift:N}/{seat}/{person:N}"))[..16]);
}
