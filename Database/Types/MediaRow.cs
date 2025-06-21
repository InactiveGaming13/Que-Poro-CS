namespace QuePoro.Database.Types;

public class MediaRow
{
    public required Guid Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required ulong CreatedBy { get; set; }
    public required string Alias { get; set; }
    public string? Category { get; set; }
    public required string Url { get; set; }
}