namespace QuePoro.Database.Types;

public class Reactions
{
    public Guid? Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public ulong? CreatedBy { get; set; }
    public ulong? UserId { get; set; }
    public string? Emoji { get; set; }
}