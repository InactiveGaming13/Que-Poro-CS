namespace QuePoro.Database.Types;

public class TempVcRow
{
    public required ulong Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required ulong CreatedBy { get; set; }
    public required ulong GuildId { get; set; }
    public required ulong Master { get; set; }
    public required string Name { get; set; }
    public required int Bitrate { get; set; }
    public required int UserLimit { get; set; }
    public required int UserCount { get; set; }
    public required List<ulong> UserQueue { get; set; }
}