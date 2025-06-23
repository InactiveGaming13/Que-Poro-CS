namespace QuePoro.Database.Types;

public class ChannelRow
{
    public required ulong Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Name { get; set; }
    public required bool Tracked { get; set; }
    public required ulong GuildId { get; set; }
    public string? Description { get; set; }
    public required int Messages { get; set; }
}