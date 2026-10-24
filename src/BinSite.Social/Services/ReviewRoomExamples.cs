namespace BinSite.Social.Services;

/// <summary>Three authored delivery/revision packages; their generated media and example notes are explicitly editorial.</summary>
public static class ReviewRoomExamples
{
    public static readonly ReviewExample[] All =
    [
        new("workshop", "Workshop Launch Banner", "announce an evening making workshop. Version 1 places the date below the main action; version 2 improves reading order and gives the date its own clear line.", "Move the date into the primary reading order so people see when the workshop happens before the call to action.", "The revised date line is clear. Check this position when adapting the banner to another aspect ratio.", .72, .72),
        new("product", "Desk Organizer Feature Card", "present a modular desk organizer with three features. Version 1 gives the headline too little space; version 2 separates the title, feature labels and action.", "The headline and feature labels compete in this area. Give the title its own space and align the feature labels.", "The feature labels are now separated from the headline. Keep the same alignment in the next export size.", .36, .3),
        new("schedule", "Community Event Schedule", "publish a short event schedule. Version 1 crowds the session names; version 2 introduces rows and a dedicated time column.", "Separate time values from session names. This crowded area makes it hard to scan the next activity.", "The new time column makes the order easier to read. Keep this column visible in a narrow layout.", .52, .48)
    ];

    /// <summary>Builds a real local project from two generated image files and clearly labelled example records.</summary>
    public static ReviewProject Create(string exampleId, IReadOnlyList<ReviewPickedFile> media)
    {
        var example = All.FirstOrDefault(item => item.Id == exampleId) ?? throw new ReviewValidationException("The editorial example was not found.");
        if (media.Count != 2) throw new ReviewValidationException("An example needs its two locally generated media files.");
        var project = ReviewRoomRules.Create(example.Title, example.Brief);
        project.IsEditorialExample = true;
        project = ReviewRoomRules.AddAssets(project, [media[0]]);
        var assetId = project.Assets[0].Id;
        project = ReviewRoomRules.AddVersion(project, assetId, media[1], "Version 2 — revised");
        var first = project.Assets[0].Versions[0].Id;
        var second = project.Assets[0].Versions[1].Id;
        project = ReviewRoomRules.AddComment(project, assetId, first, example.FirstNote, ReviewCommentCategory.Layout, ReviewCommentPriority.Blocking, example.X, example.Y, null, ReviewEntrySource.EditorialExample);
        project = ReviewRoomRules.Decide(project, assetId, first, ReviewDecisionState.Ready, "first delivery prepared for review.", ReviewEntrySource.EditorialExample);
        project = ReviewRoomRules.Decide(project, assetId, first, ReviewDecisionState.ChangesRequested, "the marked layout change is requested in a new revision.", ReviewEntrySource.EditorialExample);
        project = ReviewRoomRules.AddComment(project, assetId, second, example.SecondNote, ReviewCommentCategory.Layout, ReviewCommentPriority.Note, example.X, example.Y, null, ReviewEntrySource.EditorialExample);
        project = ReviewRoomRules.Resolve(project, project.Comments[^1].Id, true);
        project = ReviewRoomRules.Decide(project, assetId, second, ReviewDecisionState.Ready, "the revised delivery is ready for review.", ReviewEntrySource.EditorialExample);
        return ReviewRoomRules.Decide(project, assetId, second, ReviewDecisionState.Approved, "the revised layout is approved.", ReviewEntrySource.EditorialExample);
    }
}
