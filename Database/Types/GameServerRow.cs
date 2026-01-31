namespace QuePoro.Database.Types;

public class GameServerRow
{
    public required string Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string ServerName { get; set; }
    public required string ServerDescription { get; set; }
    public required bool Restartable { get; set; }
    public required string ScreenName { get; set; }
    public required string RestartMethod { get; set; }
    public required string ShutdownMethod { get; set; }
}