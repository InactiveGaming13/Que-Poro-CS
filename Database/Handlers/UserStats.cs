using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class UserStats
{
    /// <summary>
    /// Adds a User Statistic to the database.
    /// </summary>
    /// <param name="userId">The ID of the User to track.</param>
    /// <param name="channelId">The ID of the Channel to track.</param>
    /// <param name="guildId">The ID of the Guild to track.</param>
    /// <param name="sent">The number of sent messages in a channel.</param>
    /// <param name="deleted">The number of deleted messages in a channel.</param>
    /// <param name="edited">The number of edited messages in a channel.</param>
    /// <param name="tempVcCreated">The number of Temporary VCs created.</param>
    /// <param name="modActions">The number of bot moderator actions against the User.</param>
    /// <param name="strikes">The number of strikes against the User.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddStat(ulong userId, ulong channelId, ulong guildId, int sent = 0, int deleted = 0,
        int edited = 0, int tempVcCreated = 0, int modActions = 0, int strikes = 0)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "INSERT INTO user_stats (id, created_at, channel_id, guild_id, sent, deleted, edited, temp_vc_created, " +
            "mod_actions, strikes) VALUES (@id, CURRENT_TIMESTAMP, @channelId, @guildId, @sent, @deleted, @edited, " +
            "@tempVcCreated, @modActions, @strikes)";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("sent", NpgsqlDbType.Integer) { Value = sent });
        command.Parameters.Add(new NpgsqlParameter("deleted", NpgsqlDbType.Integer) { Value = deleted });
        command.Parameters.Add(new NpgsqlParameter("edited", NpgsqlDbType.Integer) { Value = edited });
        command.Parameters.Add(new NpgsqlParameter("tempVcCreated", NpgsqlDbType.Integer) { Value = tempVcCreated });
        command.Parameters.Add(new NpgsqlParameter("modActions", NpgsqlDbType.Integer) { Value = modActions });
        command.Parameters.Add(new NpgsqlParameter("strikes", NpgsqlDbType.Integer) { Value = strikes });

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
    /// Gets a User Statistic from the database.
    /// </summary>
    /// <param name="userId">The ID of the User to track.</param>
    /// <param name="channelId">The ID of the Channel to track.</param>
    /// <param name="guildId">The ID of the Guild to track.</param>
    /// <returns>The User Statistic.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the User Statistic doesn't exist.</exception>
    public static async Task<UserStatRow> GetStat(ulong userId, ulong channelId, ulong guildId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM user_stats WHERE id=@id AND channel_id=@channelId AND guild_id=@guildId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return new UserStatRow
            {
                UserId = (ulong)reader.GetInt64(reader.GetOrdinal("id")),
                SentMessages = reader.GetInt32(reader.GetOrdinal("sent")),
                DeletedMessages = reader.GetInt32(reader.GetOrdinal("deleted")),
                EditedMessages = reader.GetInt32(reader.GetOrdinal("edited")),
                TempVcsCreated = reader.GetInt32(reader.GetOrdinal("temp_vc_created")),
                ModeratorActions = reader.GetInt32(reader.GetOrdinal("mod_actions")),
                ModeratorStrikes = reader.GetInt32(reader.GetOrdinal("strikes"))
            };
        }

        throw new KeyNotFoundException($"No User Stat was found with id: {userId}");
    }
    
    /// <summary>
    /// Gets a List of User Statistics from the database.
    /// </summary>
    /// <param name="userId">The ID of the User to track.</param>
    /// <param name="channelId">The ID of the Channel to track.</param>
    /// <param name="guildId">The ID of the Guild to track.</param>
    /// <returns>The List of User Statistics.</returns>
    /// <exception cref="NullReferenceException">Thrown when all variables are null.</exception>
    public static async Task<List<UserStatRow>> GetStats(ulong? userId = null, ulong? channelId = null,
        ulong? guildId = null)
    {
        if (userId is null && channelId is null && guildId is null)
            throw new NullReferenceException("At least 1 parameter is required.");
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query = "SELECT * FROM user_stats WHERE";

        if (userId is not null)
        {
            query += " id=@id,";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });
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
        
        command.CommandText = query;

        List<UserStatRow> userStats = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            userStats.Add(new UserStatRow
            {
                UserId = (ulong)reader.GetInt64(reader.GetOrdinal("id")),
                SentMessages = reader.GetInt32(reader.GetOrdinal("sent")),
                DeletedMessages = reader.GetInt32(reader.GetOrdinal("deleted")),
                EditedMessages = reader.GetInt32(reader.GetOrdinal("edited")),
                TempVcsCreated = reader.GetInt32(reader.GetOrdinal("temp_vc_created")),
                ModeratorActions = reader.GetInt32(reader.GetOrdinal("mod_actions")),
                ModeratorStrikes = reader.GetInt32(reader.GetOrdinal("strikes"))
            });
        }

        return userStats;
    }

    /// <summary>
    /// Modifies a User Statistic in the database.
    /// </summary>
    /// <param name="userId">The ID of the User to track.</param>
    /// <param name="channelId">The ID of the Channel to track.</param>
    /// <param name="guildId">The ID of the Guild to track.</param>
    /// <param name="sent">The number of sent messages in a channel.</param>
    /// <param name="deleted">The number of deleted messages in a channel.</param>
    /// <param name="edited">The number of edited messages in a channel.</param>
    /// <param name="tempVcCreated">The number of Temporary VCs created.</param>
    /// <param name="modActions">The number of bot moderator actions against the User.</param>
    /// <param name="strikes">The number of strikes against the User.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyStat(ulong userId, ulong channelId, ulong guildId, int? sent = null,
        int? deleted = null, int? edited = null, int? tempVcCreated = null, int? modActions = null, int? strikes = null)
    {
        if (sent is null && deleted is null && edited is null && tempVcCreated is null && modActions is null &&
            strikes is null) return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query = "UPDATE user_stats SET";

        if (sent != null)
        {
            query += " sent=@sent,";
            command.Parameters.Add(new NpgsqlParameter("sent", NpgsqlDbType.Integer) { Value = sent });
        }

        if (deleted != null)
        {
            query += " deleted=@deleted,";
            command.Parameters.Add(new NpgsqlParameter("deleted", NpgsqlDbType.Integer) { Value = deleted });
        }

        if (edited != null)
        {
            query += " edited=@edited,";
            command.Parameters.Add(new NpgsqlParameter("edited", NpgsqlDbType.Integer) { Value = edited });
        }

        if (tempVcCreated != null)
        {
            query += " temp_vc_created=@tempVcCreated,";
            command.Parameters.Add(new NpgsqlParameter("tempVcCreated", NpgsqlDbType.Integer) { Value = tempVcCreated });
        }

        if (modActions != null)
        {
            query += " mod_actions=@modActions,";
            command.Parameters.Add(new NpgsqlParameter("modActions", NpgsqlDbType.Integer) { Value = modActions });
        }

        if (strikes != null)
        {
            query += " strikes=@strikes";
            command.Parameters.Add(new NpgsqlParameter("strikes", NpgsqlDbType.Integer) { Value = strikes });
        }
        
        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=@id AND channel_id=@channelId AND guild_id=@guildId";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });
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
    /// Checks if a User Statistic exists for a guild and channel.
    /// </summary>
    /// <param name="userId">The ID of the User to track.</param>
    /// <param name="channelId">The ID of the Channel to track.</param>
    /// <param name="guildId">The ID of the Guild to track.</param>
    /// <returns>Whether the User Statistic exists.</returns>
    public static async Task<bool> StatExists(ulong userId, ulong channelId, ulong guildId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "SELECT created_at FROM user_stats WHERE id=@id AND channel_id=@channelId AND guild_id=@guildId";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });

        return command.ExecuteScalar() is not null;
    }
}