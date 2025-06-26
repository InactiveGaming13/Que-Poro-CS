using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public class BannedPhraseLinks
{
    /// <summary>
    /// Adds a Banned Phrase Link between a Channel and a Guild to the database.
    /// </summary>
    /// <param name="bannedPhraseId">The ID of the Banned Phrase to link to.</param>
    /// <param name="channelId">The ID of the Channel to link to.</param>
    /// <param name="guildId">The ID of the Guild to link to.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddBannedPhraseLink(Guid bannedPhraseId, ulong channelId, ulong guildId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        const string query = "INSERT INTO banned_phrase_links (created_at, banned_phrase_id, channel_id, guild_id) VALUES " +
                             "(CURRENT_TIMESTAMP, @bannedPhraseId, @channelId, @guildId)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("bannedPhraseId", NpgsqlDbType.Uuid) { Value = bannedPhraseId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        
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
    /// Removes a Banned Phrase Link from the database.
    /// </summary>
    /// <param name="id">The ID of the Banned Phrase Link.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveBannedPhraseLink(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "DELETE FROM banned_phrase_links WHERE id=@id";
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
    /// Modifies a Banned Phrase Link in the database.
    /// </summary>
    /// <param name="bannedPhraseLinkId"></param>
    /// <param name="bannedPhraseId"></param>
    /// <param name="channelId"></param>
    /// <param name="guildId"></param>
    /// <returns></returns>
    public static async Task<bool> ModifyBannedPhraseLink(Guid bannedPhraseLinkId, Guid? bannedPhraseId = null,
        ulong? channelId = null, ulong? guildId = null)
    {
        if (bannedPhraseId is null && channelId is null && guildId is null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        string query = "MODIFY banned_phrase_links SET";

        if (bannedPhraseId is not null)
        {
            query += " banned_phrase_id=@bannedPhraseId,";
            command.Parameters.Add(new NpgsqlParameter("bannedPhraseId", NpgsqlDbType.Uuid) { Value = bannedPhraseId });
        }

        if (channelId is not null)
        {
            query += " channel_id=@channelId,";
            command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        }
        
        if (guildId is not null)
        {
            query += " guild_id=@guildId";
            command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        }

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = bannedPhraseLinkId });
        
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
    /// Gets a Banned Phrase Link from the database.
    /// </summary>
    /// <param name="id">The ID of the Banned Phrase Link.</param>
    /// <returns>The Banned Phrase Link.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Banned Phrase Link doesn't exist.</exception>
    public static async Task<BannedPhraseLinkRow> GetBannedPhraseLink(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM banned_phrase_links WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return new BannedPhraseLinkRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                BannedPhraseId = reader.GetGuid(reader.GetOrdinal("banned_phrase_id")),
                ChannelId = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id")),
                GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"))
            };
        }

        throw new KeyNotFoundException($"No Banned Phrase Link exists with ID: {id}");
    }
    
    /// <summary>
    /// Gets a List of Guild Banned Phrase Links from the Database.
    /// </summary>
    /// <param name="guildId">The ID of the Guild.</param>
    /// <returns>The List of Banned Phrase Links.</returns>
    public static async Task<List<BannedPhraseLinkRow>> GetBannedPhraseLinks(ulong guildId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM banned_phrase_links WHERE guild_id=@guildId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });

        List<BannedPhraseLinkRow> bannedPhraseLinks = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bannedPhraseLinks.Add(new BannedPhraseLinkRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                BannedPhraseId = reader.GetGuid(reader.GetOrdinal("banned_phrase_id")),
                ChannelId = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id")),
                GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"))
            });
        }

        return bannedPhraseLinks;
    }
    
    /// <summary>
    /// Gets the ID of a Banned Phrase Link from the database.
    /// </summary>
    /// <param name="bannedPhraseId">The ID of the Banned Phrase.</param>
    /// <param name="channelId">The ID of the Channel.</param>
    /// <param name="guildId">The ID of the Guild.</param>
    /// <returns>The ID of the Banned Phrase Link.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Banned Phrase doesn't exist.</exception>
    public static async Task<Guid> GetBannedPhraseLinkId(Guid bannedPhraseId, ulong channelId,
        ulong guildId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "SELECT id FROM banned_phrase_links WHERE id=@id AND channel_id=@channelId AND guild_id=@guildId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = bannedPhraseId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = channelId });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = guildId });

        if (command.ExecuteScalar() is not null)
            return (Guid)command.ExecuteScalar()!;

        throw new KeyNotFoundException($"No Banned Phrase exists with ID: {bannedPhraseId}");
    }
    
    /// <summary>
    /// Checks if a Banned Phrase Link exists in the database.
    /// </summary>
    /// <param name="id">The ID of the Banned Phrase Link.</param>
    /// <returns>Whether the Banned Phrase Link Exists.</returns>
    public static async Task<bool> BannedPhraseLinkExists(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM banned_phrase_links WHERE id=@id";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });

        return command.ExecuteScalar() is not null;
    }
    
    public static async Task<bool> BannedPhraseLinkExists(Guid bannedPhraseId, ulong channelId, ulong guildId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "SELECT created_at FROM banned_phrase_links WHERE banned_phrase_id=@bannedPhraseId AND channel_id=@channelId " + 
            "AND guild_id=@guildId";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("bannedPhraseId", NpgsqlDbType.Uuid) { Value = bannedPhraseId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });

        return command.ExecuteScalar() is not null;
    }
}