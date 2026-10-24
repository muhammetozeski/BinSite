namespace BinSite.Social.Models;
public sealed class YouTubeLinkRow
{
    public int Index { get; set; }
    public string Input { get; set; } = "";
    public string? Id { get; set; }
    public string? Url { get; set; }
    public int? DuplicateOf { get; set; }
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
    public int? HttpStatus { get; set; }
    public string? Title { get; set; }
    public string? Channel { get; set; }
    public string? ChannelUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? CheckedAt { get; set; }
}
public sealed class YouTubeLinkBatch
{
    public List<YouTubeLinkRow> Rows { get; set; } = [];
    public bool Canceled { get; set; }
    public string CheckedAt { get; set; } = "";
}
public sealed class YouTubeImageOutput
{
    public bool Canceled { get; set; }
    public int Count { get; set; }
    public int Skipped { get; set; }
}
