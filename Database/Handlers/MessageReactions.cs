using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class MessageReactions
{
    /// <summary>
    /// Adds a Message Reaction to the database.
    /// </summary>
    /// <param name="userResponsibleId">The ID of the User who added the Reaction.</param>
    /// <param name="emoji">The Emoji of the Reaction.</param>
    /// <param name="userReactsId">The ID of the User who the Reaction applies to.</param>
    /// <param name="triggerMessage">The message that triggers the Reaction.</param>
    /// <param name="exactTrigger">Whether the trigger must equal the message content.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddMessageReaction(ulong userResponsibleId, string emoji, ulong? userReactsId = null,
        string? triggerMessage = null, bool exactTrigger = false)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query =
            "INSERT INTO message_reactions (created_by, emoji_code, reacts_to, trigger, exact_trigger) " +
            "VALUES (@createdBy, @emojiCode, @reactsTo, @trigger, @exactTrigger)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric)
            { Value = (long)userResponsibleId });
        command.Parameters.Add(new NpgsqlParameter("emojiCode", NpgsqlDbType.Text) { Value = emoji });
        command.Parameters.Add(userReactsId is null
            ? new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = DBNull.Value }
            : new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = (long)userReactsId });
        command.Parameters.Add(triggerMessage is null
            ? new NpgsqlParameter("trigger", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("trigger", NpgsqlDbType.Text) { Value = triggerMessage });
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
    /// Removes a Message reaction from the database.
    /// </summary>
    /// <param name="reactionId">The Guid of the Message Reaction to remove.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveMessageReaction(Guid reactionId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "DELETE FROM message_reactions WHERE id=@id";

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
    /// Gets the ID of a Message Reaction from the database.
    /// </summary>
    /// <param name="emoji">The Emoji of the Reaction.</param>
    /// <param name="userId">The ID of the User.</param>
    /// <param name="trigger">The trigger message of the Reaction.</param>
    /// <returns>The ID of the Message Reaction.</returns>
    /// <exception cref="NullReferenceException">Thrown if all optional values are null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the Message Reaction doesn't exist.</exception>
    public static async Task<Guid> GetMessageReactionId(string emoji, ulong? userId = null, string? trigger = null)
    {
        if (userId is null && trigger is null)
            throw new NullReferenceException("At least 1 parameter is required.");

        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query =
            "SELECT id FROM message_reactions WHERE reacts_to=@reactsTo AND trigger=@trigger AND emoji_code=@emojiCode";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric)
            { Value = userId is null ? DBNull.Value : (long)userId });
        command.Parameters.Add(new NpgsqlParameter("trigger", NpgsqlDbType.Text)
            { Value = trigger is null ? DBNull.Value : trigger });
        command.Parameters.Add(new NpgsqlParameter("emojiCode", NpgsqlDbType.Text) { Value = emoji });

        if (await command.ExecuteScalarAsync() is not null)
            return (Guid)(await command.ExecuteScalarAsync())!;

        throw new KeyNotFoundException($"No Message Reaction exists with Emoji: {emoji} User ID: {userId} Trigger: {trigger}");
    }

    /// <summary>
    /// Gets a List of Message Reactions from the database.
    /// </summary>
    /// <param name="userId">The ID of the User.</param>
    /// <returns>The List of Message Reactions.</returns>
    public static async Task<List<MessageReactionRow>> GetMessageReactions(ulong userId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT * FROM message_reactions WHERE reacts_to=@reactsTo OR reacts_to IS NULL";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("reactsTo", NpgsqlDbType.Numeric) { Value = (long)userId });

        List<MessageReactionRow> reactions = [];

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

            reactions.Add(new MessageReactionRow
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

    /// <summary>
    /// Checks if a Message Reaction exists in the database.
    /// </summary>
    /// <param name="id">The ID of the Reaction.</param>
    /// <returns>Whether the Reaction exists.</returns>
    public static async Task<bool> MessageReactionExists(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT created_at FROM message_reactions WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });

        return command.ExecuteScalar() is not null;
    }

    /// <summary>
    /// Checks if a Message Reaction exists in the database.
    /// </summary>
    /// <param name="emoji">The emoji of the Reaction.</param>
    /// <param name="userId">The ID of the User.</param>
    /// <param name="trigger">The trigger message of the Reaction.</param>
    /// <returns>Whether the Reaction exists.</returns>
    public static async Task<bool> MessageReactionExists(string emoji, ulong? userId = null, string? trigger = null)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query =
            "SELECT created_at FROM message_reactions WHERE emoji_code=@emojiCode AND reacts_to=@userId AND trigger=@trigger";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("emojiCode", NpgsqlDbType.Text) { Value = emoji });
        command.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Numeric)
            { Value = userId is null ? DBNull.Value : (long)userId });
        command.Parameters.Add(new NpgsqlParameter("trigger", NpgsqlDbType.Text)
            { Value = trigger is null ? DBNull.Value : trigger });

        return command.ExecuteScalar() is not null;
    }
}