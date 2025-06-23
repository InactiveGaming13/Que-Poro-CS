using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Responses
{
    public static async Task AddResponse(ulong userResponsibleId, string trigger, ulong? userId = null,
        ulong? channelId = null, string? response = null, string? mediaAlias = null, string? mediaCategory = null,
        bool exactTrigger = false, bool enabled = true)
    {
        if (response is null && mediaAlias is null && mediaCategory is null)
            return;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "INSERT INTO responses (created_by, user_id, channel_id, trigger, response, media_alias, media_category, " +
            "exact, enabled) VALUES (@createdBy, @userId, @channelId, @trigger, @response, @mediaAlias, " +
            "@mediaCategory, @exactTrigger, @enabled)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric) { Value = (long)userResponsibleId });
        command.Parameters.Add(userId is null
            ? new NpgsqlParameter("userId", NpgsqlDbType.Numeric) { Value = DBNull.Value }
            : new NpgsqlParameter("userId", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(channelId is null
            ? new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = DBNull.Value }
            : new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("trigger", NpgsqlDbType.Text) { Value = trigger });
        command.Parameters.Add(response is null
            ? new NpgsqlParameter("response", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("response", NpgsqlDbType.Text) { Value = response });
        command.Parameters.Add(mediaAlias is null
            ? new NpgsqlParameter("mediaAlias", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("mediaAlias", NpgsqlDbType.Text) { Value = mediaAlias });
        command.Parameters.Add(mediaCategory is null
            ? new NpgsqlParameter("mediaCategory", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("mediaCategory", NpgsqlDbType.Text) { Value = mediaCategory });
        command.Parameters.Add(new NpgsqlParameter("exactTrigger", NpgsqlDbType.Boolean) { Value = exactTrigger });
        command.Parameters.Add(new NpgsqlParameter("enabled", NpgsqlDbType.Boolean) { Value = enabled });

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// Removes a reaction.
    /// </summary>
    /// <param name="responseId">The Guid of the response to remove.</param>
    public static async Task RemoveResponse(Guid responseId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "DELETE FROM responses WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = responseId });
        
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// Gets a reaction ID from a user ID and emoji.
    /// </summary>
    /// <param name="trigger">The trigger phrase to query.</param>
    /// <returns>The ID of the reaction.</returns>
    public static async Task<Guid?> GetResponseId(string trigger)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "SELECT id FROM responses WHERE trigger=@trigger";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("trigger", NpgsqlDbType.Text) { Value = trigger });

        return (Guid)command.ExecuteScalar()!;
    }
    
    public static async Task<List<ResponseRow>> GetUserResponses(ulong userId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "SELECT * FROM responses WHERE user_id=@userId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Numeric) { Value = (long)userId });

        List<ResponseRow> responses = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Guid id = reader.GetGuid(reader.GetOrdinal("id"));
            DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            ulong? userResponseId = null;
            try
            {
                userResponseId = (ulong)reader.GetInt64(reader.GetOrdinal("reacts_to"));
            }
            catch (Exception)
            {
                // ignored
            }
            ulong? channelResponseId = null;
            try
            {
                channelResponseId = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id"));
            }
            catch (Exception)
            {
                // ignored
            }
            string triggerMessage = reader.GetString(reader.GetOrdinal("trigger"));
            string? responseMessage = null;
            try
            {
                responseMessage = reader.GetString(reader.GetOrdinal("response"));
            }
            catch (Exception)
            {
                // ignored
            }
            string? mediaAlias = null;
            try
            {
                mediaAlias = reader.GetString(reader.GetOrdinal("media_alias"));
            }
            catch (Exception)
            {
                // ignored
            }
            string? mediaCategory = null;
            try
            {
                mediaCategory = reader.GetString(reader.GetOrdinal("media_category"));
            }
            catch (Exception)
            {
                // ignored
            }
            bool exactTrigger = reader.GetBoolean(reader.GetOrdinal("exact"));
            bool enabled = reader.GetBoolean(reader.GetOrdinal("enabled"));
            
            
            responses.Add(new ResponseRow
            {
                Id = id,
                CreatedAt = createdAt,
                UserId = userResponseId,
                ChannelId = channelResponseId,
                TriggerMessage = triggerMessage,
                ResponseMessage = responseMessage,
                MediaAlias = mediaAlias,
                MediaCategory = mediaCategory,
                ExactTrigger = exactTrigger,
                Enabled = enabled
            });
        }

        return responses;
    }
    
    public static async Task<List<ResponseRow>> GetChannelResponses(ulong? channelId = null)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "SELECT * FROM responses WHERE channel_id=@channelId";

        command.CommandText = query;
        command.Parameters.Add(channelId is null ?
            new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = DBNull.Value } :
            new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });

        List<ResponseRow> responses = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Guid id = reader.GetGuid(reader.GetOrdinal("id"));
            DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            ulong? userResponseId = null;
            try
            {
                userResponseId = (ulong)reader.GetInt64(reader.GetOrdinal("reacts_to"));
            }
            catch (Exception)
            {
                // ignored
            }
            ulong? channelResponseId = null;
            try
            {
                channelResponseId = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id"));
            }
            catch (Exception)
            {
                // ignored
            }
            string triggerMessage = reader.GetString(reader.GetOrdinal("trigger"));
            string? responseMessage = null;
            try
            {
                responseMessage = reader.GetString(reader.GetOrdinal("response"));
            }
            catch (Exception)
            {
                // ignored
            }
            string? mediaAlias = null;
            try
            {
                mediaAlias = reader.GetString(reader.GetOrdinal("media_alias"));
            }
            catch (Exception)
            {
                // ignored
            }
            string? mediaCategory = null;
            try
            {
                mediaCategory = reader.GetString(reader.GetOrdinal("media_category"));
            }
            catch (Exception)
            {
                // ignored
            }
            bool exactTrigger = reader.GetBoolean(reader.GetOrdinal("exact"));
            bool enabled = reader.GetBoolean(reader.GetOrdinal("enabled"));
            
            
            responses.Add(new ResponseRow
            {
                Id = id,
                CreatedAt = createdAt,
                UserId = userResponseId,
                ChannelId = channelResponseId,
                TriggerMessage = triggerMessage,
                ResponseMessage = responseMessage,
                MediaAlias = mediaAlias,
                MediaCategory = mediaCategory,
                ExactTrigger = exactTrigger,
                Enabled = enabled
            });
        }

        return responses;
    }
    
    public static async Task<List<ResponseRow>> GetGlobalResponses()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "SELECT * FROM responses WHERE channel_id=@null AND user_id=@null";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("null", NpgsqlDbType.Numeric) { Value = DBNull.Value });

        List<ResponseRow> responses = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Guid id = reader.GetGuid(reader.GetOrdinal("id"));
            DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            ulong? userResponseId = null;
            ulong? channelResponseId = null;
            string triggerMessage = reader.GetString(reader.GetOrdinal("trigger"));
            string? responseMessage = null;
            try
            {
                responseMessage = reader.GetString(reader.GetOrdinal("response"));
            }
            catch (Exception)
            {
                // ignored
            }
            string? mediaAlias = null;
            try
            {
                mediaAlias = reader.GetString(reader.GetOrdinal("media_alias"));
            }
            catch (Exception)
            {
                // ignored
            }
            string? mediaCategory = null;
            try
            {
                mediaCategory = reader.GetString(reader.GetOrdinal("media_category"));
            }
            catch (Exception)
            {
                // ignored
            }
            bool exactTrigger = reader.GetBoolean(reader.GetOrdinal("exact"));
            bool enabled = reader.GetBoolean(reader.GetOrdinal("enabled"));
            
            
            responses.Add(new ResponseRow
            {
                Id = id,
                CreatedAt = createdAt,
                UserId = userResponseId,
                ChannelId = channelResponseId,
                TriggerMessage = triggerMessage,
                ResponseMessage = responseMessage,
                MediaAlias = mediaAlias,
                MediaCategory = mediaCategory,
                ExactTrigger = exactTrigger,
                Enabled = enabled
            });
        }

        return responses;
    }
}