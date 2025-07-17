using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Responses
{
    /// <summary>
    /// Adds a Response to the database.
    /// </summary>
    /// <param name="userResponsibleId">The creator of the Response.</param>
    /// <param name="trigger">The trigger of the Response.</param>
    /// <param name="userId">The user to respond to.</param>
    /// <param name="channelId">The channel to respond in.</param>
    /// <param name="response">The text to respond with.</param>
    /// <param name="mediaAlias">The media alias to respond with.</param>
    /// <param name="mediaCategory">The media category to respond with.</param>
    /// <param name="exactTrigger">Whether the trigger must equal the message content.</param>
    /// <param name="enabled">Whether to enable the Response.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddResponse(ulong userResponsibleId, string trigger, ulong? userId = null,
        ulong? channelId = null, string? response = null, string? mediaAlias = null, string? mediaCategory = null,
        bool exactTrigger = false, bool enabled = true)
    {
        if (response is null && mediaAlias is null && mediaCategory is null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "INSERT INTO responses (created_at, created_by, user_id, channel_id, trigger, response, media_alias, media_category, " +
            "exact, enabled) VALUES (CURRENT_TIMESTAMP, @createdBy, @userId, @channelId, @trigger, @response, @mediaAlias, " +
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
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    /// <summary>
    /// Removes a Response from the database.
    /// </summary>
    /// <param name="responseId">The ID of the Response.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveResponse(Guid responseId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "DELETE FROM responses WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = responseId });
        
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
    /// Modifies a Response in the database.
    /// </summary>
    /// <param name="responseId">The ID of the Response.</param>
    /// <param name="trigger">The trigger of the Response.</param>
    /// <param name="userId">The user to respond to.</param>
    /// <param name="channelId">The channel to respond in.</param>
    /// <param name="response">The text to respond with.</param>
    /// <param name="mediaAlias">The media alias to respond with.</param>
    /// <param name="mediaCategory">The media category to respond with.</param>
    /// <param name="exact">Whether the trigger must match the message content.</param>
    /// <param name="enabled">Whether the Response is enabled.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyResponse(Guid responseId, string? trigger = null, ulong? userId = null,
        ulong? channelId = null, string? response = null, string? mediaAlias = null, string? mediaCategory = null,
        bool? exact = null, bool? enabled = null)
    {
        if (trigger is null && response is null && mediaAlias is null && mediaCategory is null && userId is null
            && exact is null && enabled is null) return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query = "UPDATE responses SET";

        if (trigger is not null)
        {
            query += " trigger=@trigger,";
            command.Parameters.Add(new NpgsqlParameter("trigger", NpgsqlDbType.Text) { Value = trigger });
        }
        
        if (userId is not null)
        {
            query += " user_id=@userId,";
            command.Parameters.Add(userId is 0
            ? new NpgsqlParameter("userId", NpgsqlDbType.Numeric) { Value = DBNull.Value }
            : new NpgsqlParameter("userId", NpgsqlDbType.Numeric) { Value = (long)userId });
        }
        
        if (channelId is not null)
        {
            query += " channel_id=@channelId,";
            command.Parameters.Add(channelId is 0
                ? new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = DBNull.Value }
                : new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });        }
        
        if (response is not null)
        {
            query += " response=@response,";
            command.Parameters.Add(new NpgsqlParameter("response", NpgsqlDbType.Text) { Value = response });
        }
        
        if (mediaAlias is not null)
        {
            query += " media_alias=@mediaAlias,";
            command.Parameters.Add(new NpgsqlParameter("mediaAlias", NpgsqlDbType.Text) { Value = mediaAlias });
        }
        
        if (mediaCategory is not null)
        {
            query += " media_category=@mediaCategory,";
            command.Parameters.Add(new NpgsqlParameter("mediaCategory", NpgsqlDbType.Text) { Value = mediaCategory });
        }
        
        if (exact is not null)
        {
            query += " exact=@exact,";
            command.Parameters.Add(new NpgsqlParameter("exact", NpgsqlDbType.Boolean) { Value = exact });
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
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = responseId });
        
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
    /// Gets the ID for a specific Response.
    /// </summary>
    /// <param name="trigger">The trigger for the Response.</param>
    /// <param name="userId">The user to respond to.</param>
    /// <param name="channelId">The channel to respond in.</param>
    /// <param name="response">The text to respond with.</param>
    /// <param name="mediaAlias">The media alias to respond with.</param>
    /// <param name="mediaCategory">The media category to respond with.</param>
    /// <param name="exact">Whether the trigger should equal the message content.</param>
    /// <param name="enabled">Whether the Response is enabled.</param>
    /// <returns>The ID of the Response (if it exists).</returns>
    public static async Task<Guid?> GetResponseId(string trigger, ulong? userId = null, ulong? channelId = null,
        string? response = null, string? mediaAlias = null, string? mediaCategory = null, bool? exact = null,
        bool? enabled = null)
    {
        if (response is null && userId is null && channelId is null && mediaAlias is null && mediaCategory is null 
            && exact is null && enabled is null) return null;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query =
            "SELECT id FROM responses WHERE trigger LIKE @trigger";
        
        if (userId is not null)
        {
            query += " AND user_id=@userId";
            command.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Numeric) { Value = (long)userId });
        }
        
        if (channelId is not null)
        {
            query += " AND channel_id=@channelId";
            command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        }

        if (response is not null)
        {
            query += " AND response LIKE @response";
            command.Parameters.Add(new NpgsqlParameter("response", NpgsqlDbType.Text) { Value = response });
        }
        
        if (mediaAlias is not null)
        {
            query += " AND media_alias LIKE @mediaAlias";
            command.Parameters.Add(new NpgsqlParameter("mediaAlias", NpgsqlDbType.Text) { Value = mediaAlias });
        }
        
        if (mediaCategory is not null)
        {
            query += " AND media_category LIKE @mediaCategory";
            command.Parameters.Add(new NpgsqlParameter("mediaCategory", NpgsqlDbType.Text) { Value = mediaCategory });
        }
        
        if (exact is not null)
        {
            query += " AND exact=@exact";
            command.Parameters.Add(new NpgsqlParameter("exact", NpgsqlDbType.Boolean) { Value = exact });
        }
        
        if (enabled is not null)
        {
            query += " AND enabled=@enabled";
            command.Parameters.Add(new NpgsqlParameter("enabled", NpgsqlDbType.Boolean) { Value = enabled });
        }

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("trigger", NpgsqlDbType.Text) { Value = trigger });

        return (Guid)command.ExecuteScalar()!;
    }
    
    /// <summary>
    /// Gets a Response with its corresponding ID.
    /// </summary>
    /// <param name="responseId">The ID of the Response.</param>
    /// <returns>The Response.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the Response doesn't exist.</exception>
    public static async Task<ResponseRow> GetResponse(Guid responseId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM responses WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = responseId });
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
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
            
            
            return new ResponseRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UserId = userResponseId,
                ChannelId = channelResponseId,
                TriggerMessage = reader.GetString(reader.GetOrdinal("trigger")),
                ResponseMessage = responseMessage,
                MediaAlias = mediaAlias,
                MediaCategory = mediaCategory,
                ExactTrigger = reader.GetBoolean(reader.GetOrdinal("exact")),
                Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"))
            };
        }

        throw new KeyNotFoundException($"No Response exists with ID: {responseId}");
    }
    
    /// <summary>
    /// Gets a List of Responses related to the User from the database.
    /// </summary>
    /// <param name="userId">The ID of the User.</param>
    /// <returns>The List of Responses.</returns>
    public static async Task<List<ResponseRow>> GetUserResponses(ulong userId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM responses WHERE user_id=@userId OR user_id IS NULL";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Numeric) { Value = (long)userId });

        List<ResponseRow> responses = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
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
            
            
            responses.Add(new ResponseRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UserId = userResponseId,
                ChannelId = channelResponseId,
                TriggerMessage = reader.GetString(reader.GetOrdinal("trigger")),
                ResponseMessage = responseMessage,
                MediaAlias = mediaAlias,
                MediaCategory = mediaCategory,
                ExactTrigger = reader.GetBoolean(reader.GetOrdinal("exact")),
                Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"))
            });
        }

        return responses;
    }
    
    /// <summary>
    /// Gets a List of Responses related to a Channel from the database.
    /// </summary>
    /// <param name="channelId">The ID of the Channel.</param>
    /// <returns>The List of Responses.</returns>
    public static async Task<List<ResponseRow>> GetChannelResponses(ulong? channelId = null)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM responses WHERE channel_id=@channelId";

        command.CommandText = query;
        command.Parameters.Add(channelId is null ?
            new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = DBNull.Value } :
            new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });

        List<ResponseRow> responses = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
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
            
            
            responses.Add(new ResponseRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UserId = userResponseId,
                ChannelId = channelResponseId,
                TriggerMessage = reader.GetString(reader.GetOrdinal("trigger")),
                ResponseMessage = responseMessage,
                MediaAlias = mediaAlias,
                MediaCategory = mediaCategory,
                ExactTrigger = reader.GetBoolean(reader.GetOrdinal("exact")),
                Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"))
            });
        }

        return responses;
    }

    /// <summary>
    /// Gets a List of Responses related to a User and a Channel from the database.
    /// </summary>
    /// <param name="userId">The ID of the User.</param>
    /// <param name="channelId">The ID of the Channel.</param>
    /// <returns>The List of Responses.</returns>
    public static async Task<List<ResponseRow>> GetUserChannelResponses(ulong userId, ulong channelId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM responses WHERE user_id=@userId OR channel_id=@channelId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });

        List<ResponseRow> responses = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
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
            
            responses.Add(new ResponseRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UserId = userResponseId,
                ChannelId = channelResponseId,
                TriggerMessage = reader.GetString(reader.GetOrdinal("trigger")),
                ResponseMessage = responseMessage,
                MediaAlias = mediaAlias,
                MediaCategory = mediaCategory,
                ExactTrigger = reader.GetBoolean(reader.GetOrdinal("exact")),
                Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"))
            });
        }

        return responses;
    }
    
    /// <summary>
    /// Gets a List of global Responses from the database.
    /// </summary>
    /// <returns>The List of Responses.</returns>
    public static async Task<List<ResponseRow>> GetGlobalResponses()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM responses WHERE user_id IS NULL AND channel_id IS NULL";

        command.CommandText = query;

        List<ResponseRow> responses = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
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
            
            
            responses.Add(new ResponseRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UserId = null,
                ChannelId = null,
                TriggerMessage = reader.GetString(reader.GetOrdinal("trigger")),
                ResponseMessage = responseMessage,
                MediaAlias = mediaAlias,
                MediaCategory = mediaCategory,
                ExactTrigger = reader.GetBoolean(reader.GetOrdinal("exact")),
                Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"))
            });
        }

        return responses;
    }
}