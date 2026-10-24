namespace BinSite.Social.Services;

/// <summary>Provides editable administrative examples, not technical, medical, or safety instructions.</summary>
public static class FieldWorkExamples
{
    /// <summary>Creates a new workspace with eight distinct checklists and no fabricated completed jobs.</summary>
    public static FieldWorkWorkspace CreateWorkspace() => new() { Templates = Templates() };

    /// <summary>Returns independent example definitions that users may adapt to their agreed work scope.</summary>
    public static List<FieldWorkTemplate> Templates() =>
    [
        Template("Equipment handover", "Record the agreed inventory and visible condition of an equipment handover.",
            Confirm("Inventory list checked", "Compare the items with the list agreed for this handover."),
            Measure("Accessories delivered", "items", 0, 500),
            Photo("Equipment overview", "Attach a photo of the actual items being handed over."),
            Confirm("Condition notes recorded", "Use the notes to record visible differences, or confirm that none were reported."),
            Photo("Packing overview", "Optional image of the packaging or cases.", false)),
        Template("Studio session preparation", "Document a booked studio layout and the materials prepared for a session.",
            Confirm("Booking requirements checked", "Compare the requested room layout with the booking notes."),
            Measure("Seats prepared", "seats", 1, 200),
            Photo("Prepared room", "Show the actual room layout before the session."),
            Confirm("Session materials present", "Check the materials listed by the session organizer."),
            Measure("Printed session packs", "packs", 0, 500, false)),
        Template("Exhibition setup record", "Keep a record of the approved display arrangement and labels.",
            Confirm("Display list checked", "Use the approved exhibit list as the reference."),
            Measure("Display panels installed", "panels", 1, 300),
            Photo("Exhibition overview", "Record the completed display arrangement."),
            Confirm("Titles match the approved labels", "Record any naming differences in notes."),
            Photo("Label example", "Optional close-up of a finished display label.", false)),
        Template("Office move handoff", "Count packed items and record room labels without requiring a personal address.",
            Confirm("Destination labels checked", "Use room codes or labels chosen by your team."),
            Measure("Cartons handed over", "cartons", 1, 3000),
            Photo("Labeled cartons", "Attach a photo of the cartons included in this handoff."),
            Confirm("Agreed inventory reconciled", "Check quantities against the agreed move list."),
            Measure("Reusable containers returned", "containers", 0, 3000, false)),
        Template("Event welcome desk", "Document materials prepared for a welcome desk, without collecting attendee details.",
            Confirm("Organizer checklist checked", "Compare desk materials with the organizer’s list."),
            Measure("Information packs ready", "packs", 1, 5000),
            Photo("Welcome desk layout", "Record the actual prepared desk."),
            Confirm("Current program available", "Check the requested program version and note any exceptions."),
            Measure("Spare blank badges", "badges", 0, 5000, false)),
        Template("Rental return record", "Record returned items and their visible condition against the agreed rental list.",
            Confirm("Return list reconciled", "List missing or additional items in the work-order notes."),
            Measure("Cases returned", "cases", 1, 1000),
            Photo("Returned items", "Show the actual returned items or cases."),
            Confirm("Visible condition documented", "Add notes describing any reported differences."),
            Photo("Condition detail", "Optional detail image relevant to the return.", false)),
        Template("Print order pickup", "Record the quantity, artwork version, and samples collected for a print order.",
            Confirm("Artwork version checked", "Compare the printed sample with the version agreed for this order."),
            Measure("Copies collected", "copies", 1, 100000),
            Photo("Actual printed sample", "Attach an image of the collected print sample."),
            Confirm("Packaging count reconciled", "Record the agreed packages and any differences in notes."),
            Measure("Packages collected", "packages", 1, 10000, false)),
        Template("Pop-up stock delivery", "Document a small stock transfer using item codes and local photo evidence.",
            Confirm("Item codes match the manifest", "Use the manifest chosen for this stock transfer."),
            Measure("Units delivered", "units", 1, 100000),
            Photo("Delivery overview", "Show the actual stock or labeled delivery containers."),
            Confirm("Quantity differences recorded", "Record differences in the notes, or confirm that none were found."),
            Measure("Display trays supplied", "trays", 0, 1000, false))
    ];

    /// <summary>Creates one example from its own title, scope, and fields.</summary>
    static FieldWorkTemplate Template(string title, string description, params FieldWorkField[] fields) => new()
    { Title = title, Description = description, Fields = [.. fields] };

    /// <summary>Defines an explicit confirmation whose unchecked state remains incomplete when required.</summary>
    static FieldWorkField Confirm(string label, string instructions) => new()
    { Label = label, Instructions = instructions, Kind = FieldWorkFieldKind.Confirmation };

    /// <summary>Defines a measurable quantity with editable inclusive bounds and units.</summary>
    static FieldWorkField Measure(string label, string unit, decimal minimum, decimal maximum, bool required = true) => new()
    { Label = label, Unit = unit, Minimum = minimum, Maximum = maximum, Kind = FieldWorkFieldKind.Measurement, Required = required };

    /// <summary>Requires a selected local image rather than a filename-only assertion.</summary>
    static FieldWorkField Photo(string label, string instructions, bool required = true) => new()
    { Label = label, Instructions = instructions, Kind = FieldWorkFieldKind.Photo, Required = required };
}
