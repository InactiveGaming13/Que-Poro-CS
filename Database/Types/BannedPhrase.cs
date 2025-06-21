namespace QuePoro.Database.Types;

public class BannedPhrase
{
    public Guid? Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? GuildId { get; set; }
    public int? Severity { get; set; }
    public string? Phrase { get; set; }
    public bool? Enabled { get; set; }
}