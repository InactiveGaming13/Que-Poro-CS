namespace QuePoro.Database.Types;

public class Media
{
    public Guid? Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public ulong? CreatedBy { get; set; }
    public string? Alias { get; set; }
    public string? Category { get; set; }
    public string? Url { get; set; }
}