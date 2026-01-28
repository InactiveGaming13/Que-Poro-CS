using System.Data;
using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public class RoleReactions
{
    /// <summary>
    /// Adds a Role Reaction to the database.
    /// </summary>
    /// <param name="userResponsibleId">The ID of the User who added the Role Reaction.</param>
    /// <param name="guildId">The ID of the Guild the Role Reaction belongs to.</param>
    /// <param name="channelId">The ID of the Channel the Role Reaction belongs to.</param>
    /// <param name="messageLink">The Link of the Message the Role Reaction belongs to.</param>
    /// <param name="roleId">The ID of the role to give.</param>
    /// <param name="reactionCode">The Unicode of the emoji to react with.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddRoleReaction(ulong userResponsibleId, ulong guildId, ulong channelId,
        string messageLink, ulong roleId, string reactionCode)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query =
            "INSERT INTO role_reactions (created_by, guild_id, channel_id, message_link, role_id, reaction_code)" +
            "VALUES (@createdBy, @guildId, @channelId, @messageLink, @roleId, @reactionCode)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric)
            { Value = (long)userResponsibleId });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("messageLink", NpgsqlDbType.Text) { Value = messageLink });
        command.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Numeric) { Value = (long)roleId });
        command.Parameters.Add(new NpgsqlParameter("reactionCode", NpgsqlDbType.Text) { Value = reactionCode });

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
    /// Removes a Role reaction from the database.
    /// </summary>
    /// <param name="reactionId">The Guid of the Role Reaction to remove.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveRoleReaction(Guid reactionId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "DELETE FROM role_reactions WHERE id=@id";

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
    /// Gets the ID of a Role Reaction from the database.
    /// </summary>
    /// <param name="guildId">The ID of the Guild for the Role Reaction.</param>
    /// <param name="channelId">The ID of the Channel for the Role Reaction.</param>
    /// <param name="messageLink">The Link of the Message for the Role Reaction.</param>
    /// <param name="roleId">The ID of the Role for the Role Reaction.</param>
    /// <param name="reactionCode">The Unicode of the Reaction Emoji for the Role Reaction.</param>
    /// <returns>The ID of the Role Reaction.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the Role Reaction doesn't exist.</exception>
    public static async Task<Guid> GetRoleReactionId(ulong guildId, ulong channelId, string messageLink, ulong roleId,
        string reactionCode)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query =
            "SELECT id FROM role_reactions WHERE guild_id=@guildId AND channel_id=@channelId AND " +
            "message_link=@messageLink AND role_id=@roleId AND reaction_code=@reactionCode";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("messageLink", NpgsqlDbType.Text) { Value = messageLink });
        command.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Numeric) { Value = (long)roleId });
        command.Parameters.Add(new NpgsqlParameter("reactionCode", NpgsqlDbType.Text) { Value = reactionCode });

        if (command.ExecuteScalar() is not null)
            return (Guid)command.ExecuteScalar()!;

        throw new KeyNotFoundException($"No Role Reaction exists with Message ID: {messageLink} Role ID: {roleId} " +
                                       $"Reaction ID: {reactionCode}");
    }

    /// <summary>
    /// Gets a single Role Reaction from the database.
    /// </summary>
    /// <param name="reactionId">The ID of the Role Reaction.</param>
    /// <returns>The Role Reaction.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the Role Reaction Doesn't exist.</exception>
    public static async Task<RoleReactionRow> GetRoleReaction(Guid reactionId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT * FROM role_reactions WHERE id=@reactionId LIMIT 1";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("reactionId", DbType.Guid) { Value = reactionId });
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return new RoleReactionRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id")),
                ChannelId = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id")),
                MessageLink = reader.GetString(reader.GetOrdinal("message_link")),
                RoleId = (ulong)reader.GetInt64(reader.GetOrdinal("role_id")),
                ReactionCode = reader.GetString(reader.GetOrdinal("reaction_code"))
            };

        throw new KeyNotFoundException($"No Role Reaction exists with ID: {reactionId}");
    }
    
    /// <summary>
    /// Gets a list of Role Reactions for a specified Guild from the database.
    /// </summary>
    /// <param name="guildId">The ID of the Guild for the Role Reaction.</param>
    /// <returns>The list of Role Reactions.</returns>
    public static async Task<List<RoleReactionRow>> GetRoleReactions(ulong guildId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT * FROM role_reactions WHERE guild_id=@guildId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        
        List<RoleReactionRow> reactions = [];

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reactions.Add(new RoleReactionRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id")),
                ChannelId = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id")),
                MessageLink = reader.GetString(reader.GetOrdinal("message_link")),
                RoleId = (ulong)reader.GetInt64(reader.GetOrdinal("role_id")),
                ReactionCode = reader.GetString(reader.GetOrdinal("reacton_code"))
            });
        }

        return reactions;
    }
    
    /// <summary>
    /// Gets a list of Role Reactions for a specified Guild and Channel from the database.
    /// </summary>
    /// <param name="guildId">The ID of the Guild for the Role Reaction.</param>
    /// <param name="channelId">The ID of the Channel for the Role Reaction.</param>
    /// <returns>The list of Role Reactions.</returns>
    public static async Task<List<RoleReactionRow>> GetRoleReactions(ulong guildId, ulong channelId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT * FROM role_reactions WHERE guild_id=@guildId AND channel_id=@channelId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Numeric) { Value = (long)channelId });
        
        List<RoleReactionRow> reactions = [];

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reactions.Add(new RoleReactionRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id")),
                ChannelId = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id")),
                MessageLink = reader.GetString(reader.GetOrdinal("message_id")),
                RoleId = (ulong)reader.GetInt64(reader.GetOrdinal("role_id")),
                ReactionCode = reader.GetString(reader.GetOrdinal("reacton_code"))
            });
        }

        return reactions;
    }
}