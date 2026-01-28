namespace QuePoro.Database.Types;

public class RoleReactionRow
{
    public required Guid Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required ulong CreatedBy { get; set; }
    public required ulong GuildId { get; set; }
    public required ulong ChannelId { get; set; }
    public required string MessageLink { get; set; }
    public required ulong RoleId { get; set; }
    public required string ReactionCode { get; set; }
}