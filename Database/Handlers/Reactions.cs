using System.Data;
using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Reactions
{
    /// <summary>
    /// Adds a user emoji reaction to the database.
    /// </summary>
    /// <param name="userResponsibleId">The user who is adding the reaction.</param>
    /// <param name="userReactsId">The user who is being affected by this command.</param>
    /// <param name="emoji">The emoji to add to the user.</param>
    public static async Task AddReaction(ulong userResponsibleId, ulong userReactsId, string emoji)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "INSERT INTO reactions (created_by, emoji_code, reacts_to) VALUES (@createdBy, @emojiCode, @reactsTo)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric) { Value = userResponsibleId });
        command.Parameters.Add(new NpgsqlParameter("emojiCode", NpgsqlDbType.Text) { Value = emoji });
        command.Parameters.Add(new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = userReactsId });
        
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (PostgresException e)
        {
            Console.WriteLine($"{e.ErrorCode} | {e.Message}");
        }
    }

    /// <summary>
    /// Removes a reaction.
    /// </summary>
    /// <param name="reactionId">The Guid of the reaction to remove.</param>
    public static async Task RemoveReaction(Guid reactionId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "DELETE FROM reactions WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = reactionId });
        
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (PostgresException e)
        {
            Console.WriteLine($"{e.ErrorCode} | {e.Message}");
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
        await using var command = connection.CreateCommand();
        
        string query =
            "SELECT id FROM reactions WHERE reacts_to=@reactsTo AND emoji_code=@emojiCode";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("emojiCode", NpgsqlDbType.Text) { Value = emoji });
        command.Parameters.Add(new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = userId });

        return (Guid)command.ExecuteScalar()!;
    }
    
    public static async Task<List<ReactionRow>> GetReactions(ulong userId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query =
            "SELECT * FROM reactions WHERE reacts_to=@reactsTo";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = userId });

        List<ReactionRow> reactions = [];
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reactions.Add(new ReactionRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                Emoji = reader.GetString(reader.GetOrdinal("emoji_code")),
                UserId = (ulong)reader.GetInt64(reader.GetOrdinal("reacts_to"))
            });
        }

        return reactions;
    }
}