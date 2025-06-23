﻿namespace QuePoro.Database.Types;

public class ReactionRow
{
    public required Guid Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required ulong CreatedBy { get; set; }
    public required ulong UserId { get; set; }
    public string? TriggerMessage { get; set; }
    public required bool ExactTrigger { get; set; }
    public required string Emoji { get; set; }
}