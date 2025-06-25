namespace QuePoro.Database.Types;

public class GuildRow
{
    public required ulong Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Name { get; set; }
    public required bool Tracked { get; set; }
    public ulong? TempVcChannel { get; set; }
    public required bool TempVcEnabled { get; set; }
    public required int TempVcDefaultMemberLimit { get; set; }
    public required int TempVcDefaultBitrate { get; set; }
    public ulong? RobloxAlertChannel { get; set; }
    public required bool RobloxAlertEnabled { get; set; }
    public required int RobloxAlertInterval { get; set; }
}