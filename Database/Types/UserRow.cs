namespace QuePoro.Database.Types;

public class UserRow
{
    public required ulong Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Username { get; set; }
    public string? GlobalName { get; set; }
    public required bool Admin { get; set; }
    public required bool RepliedTo { get; set; }
    public required bool ReactedTo { get; set; }
    public required bool Tracked { get; set; }
    public required bool Banned { get; set; }
}