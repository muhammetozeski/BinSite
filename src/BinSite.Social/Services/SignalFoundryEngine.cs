using System.Text.Json;
using BinSite.Social.Models;

namespace BinSite.Social.Services;

/// <summary>Pure sf-v1 interpreter and finite-family analysis; no random inference or executable user code.</summary>
public static class SignalFoundryEngine
{
    public const string Version = "sf-v1";
    public static readonly string[] OperationNames = ["Rotate 90° clockwise", "Rotate 180°", "Mirror left / right", "Reflect across main diagonal", "Shift right with wrap", "Shift down with wrap", "Cycle red → blue → yellow → red", "Swap red and blue"];
    static readonly SignalMapping[] Family = CreateFamily();
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static int[] ValidateBoard(int[]? board)
    {
        if (board is null || board.Length != 9 || board.Any(cell => cell is < 0 or > 3)) throw new FormatException("A board must contain exactly nine cells: 0 empty, 1 red, 2 blue or 3 yellow.");
        return board;
    }

    public static string Program(string? program)
    {
        var value = program?.Trim() ?? "";
        if (value.Length is < 1 or > 8 || value.Any(stamp => stamp is < 'A' or > 'D')) throw new FormatException("Use one to eight uppercase stamps A, B, C or D without spaces.");
        return value;
    }

    public static SignalMapping Mapping(int[]? mapping)
    {
        if (mapping is null || mapping.Length != 4 || mapping.Any(operation => operation is < 0 or > 7) || mapping.Distinct().Count() != 4) throw new FormatException("Assign four different operations, one to each stamp.");
        return new(mapping[0], mapping[1], mapping[2], mapping[3]);
    }

    public static int[] Execute(int[] input, string program, int[] mapping) => Execute(input, program, Mapping(mapping));

    public static int[] Execute(int[] input, string program, SignalMapping mapping)
    {
        var output = (int[])ValidateBoard(input).Clone();
        foreach (var stamp in Program(program)) output = Apply(output, mapping[stamp - 'A']);
        return output;
    }

    public static int[] Apply(int[] board, int operation)
    {
        ValidateBoard(board);
        if (operation is < 0 or > 7) throw new FormatException("The operation is outside the sf-v1 family.");
        var output = new int[9];
        for (var row = 0; row < 3; row++)
        for (var column = 0; column < 3; column++)
        {
            var value = board[row * 3 + column];
            switch (operation)
            {
                case 0: output[column * 3 + 2 - row] = value; break;
                case 1: output[(2 - row) * 3 + 2 - column] = value; break;
                case 2: output[row * 3 + 2 - column] = value; break;
                case 3: output[column * 3 + row] = value; break;
                case 4: output[row * 3 + (column + 1) % 3] = value; break;
                case 5: output[((row + 1) % 3) * 3 + column] = value; break;
                case 6: output[row * 3 + column] = value == 0 ? 0 : value % 3 + 1; break;
                case 7: output[row * 3 + column] = value switch { 1 => 2, 2 => 1, _ => value }; break;
            }
        }
        return output;
    }

    public static SignalAnalysis Analyze(IEnumerable<SignalObservation> observations, SignalTarget target)
    {
        ValidateBoard(target.Input);
        Program(target.Program);
        var examples = observations.ToArray();
        if (examples.Length > 12) throw new FormatException("A challenge can expose at most six examples and six experiments.");
        foreach (var example in examples) { ValidateBoard(example.Input); ValidateBoard(example.Output); Program(example.Program); }
        var candidates = Family.Where(mapping => examples.All(example => Execute(example.Input, example.Program, mapping).SequenceEqual(example.Output))).ToArray();
        var outputs = candidates.Select(mapping => Execute(target.Input, target.Program, mapping)).GroupBy(Key).Select(group => new SignalOutcome(group.First(), group.Count())).OrderBy(outcome => Key(outcome.Board), StringComparer.Ordinal).ToArray();
        return new(candidates, outputs);
    }

    public static SignalAssessment Assess(IEnumerable<SignalObservation> observations, SignalTarget target, int[] answer, int[] mapping)
    {
        ValidateBoard(answer);
        var secret = Mapping(mapping);
        var analysis = Analyze(observations, target);
        if (!analysis.Candidates.Contains(secret)) throw new FormatException("The observations contradict the answer key. Repair this challenge before using it.");
        var output = Execute(target.Input, target.Program, secret);
        var consistent = analysis.Outcomes.Any(outcome => outcome.Board.SequenceEqual(answer));
        return new() { Verdict = consistent ? analysis.TargetOutcomeCount == 1 ? "deduced" : "consistent" : "inconsistent", ActualMatch = output.SequenceEqual(answer), CandidateCount = analysis.CandidateCount, TargetOutcomeCount = analysis.TargetOutcomeCount, ActualOutput = output, Answer = (int[])answer.Clone(), Mapping = secret.ToArray() };
    }

    public static List<SignalObservation> Examples(SignalAuthorProject project)
    {
        ValidateProject(project);
        return project.Examples.Select(example => new SignalObservation { Input = (int[])example.Input.Clone(), Program = Program(example.Program), Output = Execute(example.Input, example.Program, project.Mapping) }).ToList();
    }

    public static void ValidateProject(SignalAuthorProject project)
    {
        if (project.EngineVersion != Version) throw new FormatException("This project uses a different engine version.");
        if (string.IsNullOrWhiteSpace(project.Title) || project.Title.Trim().Length > 120) throw new FormatException("Use a title between 1 and 120 characters.");
        if (string.IsNullOrWhiteSpace(project.Description) || project.Description.Trim().Length > 900) throw new FormatException("Use a description between 1 and 900 characters.");
        if (string.IsNullOrWhiteSpace(project.AuthorAlias) || project.AuthorAlias.Trim().Length > 40) throw new FormatException("Use a public author alias between 1 and 40 characters.");
        Mapping(project.Mapping);
        if (project.Examples is null || project.Examples.Count is < 1 or > 6) throw new FormatException("Include one to six worked examples.");
        foreach (var example in project.Examples) { ValidateBoard(example.Input); Program(example.Program); }
        if (project.Target is null) throw new FormatException("A final reconstruction target is required.");
        ValidateBoard(project.Target.Input); Program(project.Target.Program);
        if (project.QueryLimit is < 0 or > 6) throw new FormatException("Allow between zero and six experiments.");
        if (project.ParentId is { Length: > 80 }) throw new FormatException("The source challenge identifier is too long.");
    }

    public static string Serialize(SignalAuthorProject project) { ValidateProject(project); return JsonSerializer.Serialize(project, Json); }
    public static SignalAuthorProject Deserialize(string json)
    {
        if (json.Length > 50000) throw new FormatException("A local challenge project must be smaller than 50,000 characters.");
        var project = JsonSerializer.Deserialize<SignalAuthorProject>(json, Json) ?? throw new FormatException("The project is empty.");
        ValidateProject(project);
        return project;
    }

    public static string Key(int[] board) => string.Concat(board.Select(value => (char)('0' + value)));

    static SignalMapping[] CreateFamily()
    {
        var mappings = new List<SignalMapping>(1680);
        for (var a = 0; a < 8; a++)
        for (var b = 0; b < 8; b++)
        for (var c = 0; c < 8; c++)
        for (var d = 0; d < 8; d++)
            if (a != b && a != c && a != d && b != c && b != d && c != d) mappings.Add(new(a, b, c, d));
        return mappings.ToArray();
    }
}
