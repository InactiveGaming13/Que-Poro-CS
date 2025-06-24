using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public class UserStats
{
    public static async Task AddUser(ulong userId, int sent = 0, int deleted = 0, int edited = 0, int tempVcCreated = 0,
        int modActions = 0, int strikes = 0)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query =
            "INSERT INTO user_stats (id, sent, deleted, edited, temp_vc_created, mod_actions, strikes) " +
            "VALUES (@id, @sent, @deleted, @edited, @tempVcCreated, @modActions, @strikes)";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("sent", NpgsqlDbType.Integer) { Value = sent });
        command.Parameters.Add(new NpgsqlParameter("deleted", NpgsqlDbType.Integer) { Value = deleted });
        command.Parameters.Add(new NpgsqlParameter("edited", NpgsqlDbType.Integer) { Value = edited });
        command.Parameters.Add(new NpgsqlParameter("tempVcCreated", NpgsqlDbType.Integer) { Value = tempVcCreated });
        command.Parameters.Add(new NpgsqlParameter("modActions", NpgsqlDbType.Integer) { Value = modActions });
        command.Parameters.Add(new NpgsqlParameter("strikes", NpgsqlDbType.Integer) { Value = strikes });

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public static async Task<UserStatRow?> GetUser(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query = "SELECT * FROM user_stats WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ulong userId = (ulong)reader.GetInt64(reader.GetOrdinal("id"));
            int sent = reader.GetInt32(reader.GetOrdinal("sent"));
            int deleted = reader.GetInt32(reader.GetOrdinal("deleted"));
            int edited = reader.GetInt32(reader.GetOrdinal("edited"));
            int tempVcsCreated = reader.GetInt32(reader.GetOrdinal("temp_vc_created"));
            int modActions = reader.GetInt32(reader.GetOrdinal("mod_actions"));
            int strikes = reader.GetInt32(reader.GetOrdinal("strikes"));
                
            return new UserStatRow
            {
                UserId = userId,
                SentMessages = sent,
                DeletedMessages = deleted,
                EditedMessages = edited,
                TempVcsCreated = tempVcsCreated,
                ModeratorActions = modActions,
                ModeratorStrikes = strikes
            };
        }

        return null;
    }

    public static async Task ModifyUser(ulong userId, int? sent = null, int? deleted = null, int? edited = null,
        int? tempVcCreated = null, int? modActions = null, int? strikes = null)
    {
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

        query += " WHERE id=@id";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}