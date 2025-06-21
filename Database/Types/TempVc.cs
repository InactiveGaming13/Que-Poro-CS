namespace QuePoro.Database.Types;

public class TempVc
{
    public Guid? Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public ulong? CreatedBy { get; set; }
    public ulong? GuildId { get; set; }
    public ulong? Master { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public int? Bitrate { get; set; }
    public int? UserLimit { get; set; }
    public int? UserCount { get; set; }
}