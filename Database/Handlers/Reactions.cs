using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Reactions
{
    public static async Task<bool> AddReaction(ulong userResponsibleId, string emoji, ulong? userReactsId = null,
        string? triggerMessage = null, bool exactTrigger = false)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "INSERT INTO reactions (created_by, emoji_code, reacts_to, trigger, exact_trigger) " +
            "VALUES (@createdBy, @emojiCode, @reactsTo, @trigger, @exactTrigger)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric) { Value = (long)userResponsibleId });
        command.Parameters.Add(new NpgsqlParameter("emojiCode", NpgsqlDbType.Text) { Value = emoji });
        command.Parameters.Add(userReactsId is null ?
            new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = DBNull.Value } :
            new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = (long)userReactsId });
        command.Parameters.Add(triggerMessage is null ?
            new NpgsqlParameter("trigger", NpgsqlDbType.Text) { Value = DBNull.Value } :
            new NpgsqlParameter("trigger", NpgsqlDbType.Text) { Value = triggerMessage });
        command.Parameters.Add(new NpgsqlParameter("exactTrigger", NpgsqlDbType.Boolean) { Value = exactTrigger });
        
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
    /// Removes a reaction.
    /// </summary>
    /// <param name="reactionId">The Guid of the reaction to remove.</param>
    public static async Task<bool> RemoveReaction(Guid reactionId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "DELETE FROM reactions WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = reactionId });
        
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
    /// Gets a reaction ID from a user ID and emoji.
    /// </summary>
    /// <param name="userId">The ID of the user to query.</param>
    /// <param name="emoji">The emoji to query.</param>
    /// <returns>The ID of the reaction.</returns>
    public static async Task<Guid?> GetReactionId(ulong userId, string emoji)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query =
            "SELECT id FROM reactions WHERE reacts_to=@reactsTo AND emoji_code=@emojiCode";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("emojiCode", NpgsqlDbType.Text) { Value = emoji });
        command.Parameters.Add(new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = (long)userId });

        return (Guid?)command.ExecuteScalar()!;
    }
    
    public static async Task<List<ReactionRow>> GetReactions(ulong userId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM reactions WHERE reacts_to=@reactsTo OR reacts_to IS NULL";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = (long)userId });

        List<ReactionRow> reactions = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ulong? userReactsId = null;
            try
            {
                userReactsId = (ulong)reader.GetInt64(reader.GetOrdinal("reacts_to"));
            }
            catch (Exception)
            {
                // ignore
            }
            string? triggerMessage = null;
            try
            {
                triggerMessage = reader.GetString(reader.GetOrdinal("trigger"));
            }
            catch (Exception)
            {
                // ignore
            }
            reactions.Add(new ReactionRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                Emoji = reader.GetString(reader.GetOrdinal("emoji_code")),
                UserId = userReactsId,
                TriggerMessage = triggerMessage,
                ExactTrigger = reader.GetBoolean(reader.GetOrdinal("exact_trigger"))
            });
        }

        return reactions;
    }
}