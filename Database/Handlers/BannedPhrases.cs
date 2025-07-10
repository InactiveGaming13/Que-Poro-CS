using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class BannedPhrases
{
    /// <summary>
    /// Adds a Banned Phrase to the database.
    /// </summary>
    /// <param name="creatorId">The ID of the User adding the Banned Phrase.</param>
    /// <param name="severity">The Severity of the Banned Phrase.</param>
    /// <param name="phrase">The Banned Phrase.</param>
    /// <param name="enabled">Whether to enable the Banned Phrase.</param>
    /// <param name="reason">The reason for the Banned Phrase.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddBannedPhrase(ulong creatorId, int severity, string phrase, string? reason = null,
        bool enabled = true)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        const string query =
            "INSERT INTO banned_phrases (created_at, created_by, severity, phrase, enabled, reason) " +
            "VALUES (CURRENT_TIMESTAMP, @createdBy, @severity, @phrase, @enabled, @reason)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric) { Value = (long)creatorId });
        command.Parameters.Add(new NpgsqlParameter("severity", NpgsqlDbType.Integer) { Value = severity });
        command.Parameters.Add(new NpgsqlParameter("phrase", NpgsqlDbType.Text) { Value = phrase });
        command.Parameters.Add(new NpgsqlParameter("enabled", NpgsqlDbType.Boolean) { Value = enabled });
        command.Parameters.Add(new NpgsqlParameter("reason", NpgsqlDbType.Text)
        { Value = reason is null ? DBNull.Value : reason });

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception e)
        {
            if (e.Message.Contains("banned_phrases_phrase_key", StringComparison.CurrentCultureIgnoreCase))
                Console.WriteLine("Phrase already exists!");

            Console.WriteLine(e);
            return false;
        }
    }

    /// <summary>
    /// Removes a Banned Phrase from the database.
    /// </summary>
    /// <param name="id">The ID of the Banned Phrase.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveBannedPhrase(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "DELETE FROM banned_phrases WHERE id=@id";
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    /// <summary>
    /// Modifies a Banned Phrase in the database.
    /// </summary>
    /// <param name="id">The ID of the Banned Phrase.</param>
    /// <param name="severity">The Severity of the Banned Phrase.</param>
    /// <param name="phrase">The Banned Phrase.</param>
    /// <param name="reason">The reason for the Banned Phrase.</param>
    /// <param name="enabled">Whether to enable the Banned Phrase.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyBannedPhrase(Guid id, int? severity = null, string? phrase = null,
        string? reason = null, bool? enabled = null)
    {
        if (severity is null && phrase is null && reason is null && enabled is null)
            return false;

        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        string query = "UPDATE banned_phrases SET";

        if (severity is not null)
        {
            query += " severity=@severity,";
            command.Parameters.Add(new NpgsqlParameter("severity", NpgsqlDbType.Integer) { Value = severity });
        }

        if (phrase is not null)
        {
            query += " phrase=@phrase,";
            command.Parameters.Add(new NpgsqlParameter("phrase", NpgsqlDbType.Text) { Value = phrase });
        }
        
        if (reason is not null)
        {
            query += " reason=@reason,";
            command.Parameters.Add(new NpgsqlParameter("reason", NpgsqlDbType.Text) { Value = reason });
        }

        if (enabled is not null)
        {
            query += " enabled=@enabled";
            command.Parameters.Add(new NpgsqlParameter("enabled", NpgsqlDbType.Boolean) { Value = enabled });
        }

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    /// <summary>
    /// Gets the ID of a Banned Phrase from the database.
    /// </summary>
    /// <param name="phrase">The Banned Phrase.</param>
    /// <returns>The ID of the Banned Phrase.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Banned Phrase doesn't exist.</exception>
    public static async Task<Guid> GetBannedPhraseId(string phrase)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "SELECT id FROM banned_phrases WHERE phrase=@phrase";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("phrase", NpgsqlDbType.Text) { Value = phrase });

        if (command.ExecuteScalar() is not null)
            return (Guid)command.ExecuteScalar()!;

        throw new KeyNotFoundException($"No Banned Phrase exists with Phrase: {phrase}");
    }

    /// <summary>
    /// Gets a Banned Phrase from the database.
    /// </summary>
    /// <param name="id">The ID of the Banned Phrase.</param>
    /// <returns>The Banned Phrase.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Banned Phrase doesn't exist.</exception>
    public static async Task<BannedPhraseRow?> GetBannedPhrase(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT * FROM banned_phrases WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return new BannedPhraseRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                Severity = reader.GetInt32(reader.GetOrdinal("severity")),
                Phrase = reader.GetString(reader.GetOrdinal("phrase")),
                Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"))
            };
        }

        throw new KeyNotFoundException($"No Banned Phrase exists with ID: {id}");
    }
    
    /// <summary>
    /// Gets a List of all Banned Phrases in the database.
    /// </summary>
    /// <returns>The List of Banned Phrases.</returns>
    public static async Task<List<BannedPhraseRow>> GetAllBannedPhrases()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT * FROM banned_phrases";

        command.CommandText = query;

        List<BannedPhraseRow> bannedPhrases = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string? reason = null;
            try
            {
                reason = reader.GetString(reader.GetOrdinal("reason"));
            }
            catch (Exception)
            {
                // ignore
            }
            bannedPhrases.Add(new BannedPhraseRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                Severity = reader.GetInt32(reader.GetOrdinal("severity")),
                Phrase = reader.GetString(reader.GetOrdinal("phrase")),
                Reason = reason,
                Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"))
            });
        }

        return bannedPhrases;
    }
    
    /// <summary>
    /// Checks if a Banned Phrase exists in the database.
    /// </summary>
    /// <param name="phrase">The Banned Phrase.</param>
    /// <returns>Whether the Channel exists.</returns>
    public static async Task<bool> BannedPhraseExists(string phrase)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM banned_phrases WHERE phrase=@phrase";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("phrase", NpgsqlDbType.Text) { Value = phrase });

        return command.ExecuteScalar() is not null;
    }
}