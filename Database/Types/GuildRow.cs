namespace QuePoro.Database.Types;

public class GuildRow
{
    public required ulong Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Name { get; set; }
    public required bool Tracked { get; set; }
    public required ulong TempVcChannel { get; set; }
    public required int TempVcMemberDefault { get; set; }
    public required int TempVcBitrateDefault { get; set; }
    public required ulong RobloxAlertChannel { get; set; }
    public required int RobloxAlertInterval { get; set; }
}