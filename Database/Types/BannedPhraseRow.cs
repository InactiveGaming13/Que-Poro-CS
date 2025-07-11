﻿namespace QuePoro.Database.Types;

public class BannedPhraseRow
{
    public required Guid Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required ulong CreatedBy { get; set; }
    public required int Severity { get; set; }
    public required string Phrase { get; set; }
    public string? Reason { get; set; }
    public required bool Enabled { get; set; }
}