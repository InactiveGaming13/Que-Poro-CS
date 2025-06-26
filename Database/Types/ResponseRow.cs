namespace QuePoro.Database.Types;

public class ResponseRow
{
    public required Guid Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public ulong? UserId { get; set; }
    public ulong? ChannelId { get; set; }
    public required string TriggerMessage { get; set; }
    public string? ResponseMessage { get; set; }
    public string? MediaAlias { get; set; }
    public string? MediaCategory { get; set; }
    public required bool ExactTrigger { get; set; }
    public required bool Enabled { get; set; }
}