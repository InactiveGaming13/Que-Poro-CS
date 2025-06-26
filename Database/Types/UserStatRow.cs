namespace QuePoro.Database.Types;

public class UserStatRow
{
    public required ulong UserId { get; set; }
    public required int SentMessages { get; set; }
    public required int DeletedMessages { get; set; }
    public required int EditedMessages { get; set; }
    public required int TempVcsCreated { get; set; }
    public required int ModeratorActions { get; set; }
    public required int ModeratorStrikes { get; set; }
}