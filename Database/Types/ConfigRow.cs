namespace QuePoro.Database.Types;

public class ConfigRow
{
    public required DateTime CreatedAt { get; set; }
    public required DateTime LastModified { get; set; }
    public required short StatusType { get; set; }
    public required string StatusMessage { get; set; }
    public required ulong LogChannel { get; set; }
    public required bool TempVcEnabled { get; set; }
    public required short TempVcDefaultMemberLimit { get; set; }
    public required int TempVcDefaultBitrate { get; set; }
    public required bool RobloxAlertsEnabled { get; set; }
    public required bool RepliesEnabled { get; set; }
    public required bool TestersEnabled { get; set; }
    public required ulong ShutdownChannel { get; set; }
    public required ulong ShutdownMessage { get; set; }
}