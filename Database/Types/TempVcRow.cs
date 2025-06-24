namespace QuePoro.Database.Types;

public class TempVcRow
{
    public required Guid Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required ulong CreatedBy { get; set; }
    public required ulong GuildId { get; set; }
    public required ulong Master { get; set; }
    public required string Name { get; set; }
    public required int Bitrate { get; set; }
    public required int UserLimit { get; set; }
    public required int UserCount { get; set; }
}