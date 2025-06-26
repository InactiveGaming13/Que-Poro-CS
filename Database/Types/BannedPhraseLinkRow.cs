namespace QuePoro.Database.Types;

public class BannedPhraseLinkRow
{
    public required Guid Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required Guid BannedPhraseId { get; set; }
    public required ulong ChannelId { get; set; }
    public required ulong GuildId { get; set; }
}