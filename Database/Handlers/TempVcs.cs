using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class TempVcs
{
    /// <summary>
    /// Adds a Temporary VC to the database.
    /// </summary>
    /// <param name="id">The ID of the Temporary VC.</param>
    /// <param name="createdBy">The ID Channel master.</param>
    /// <param name="guildId">The Guild ID the Temporary VC belongs to.</param>
    /// <param name="name">The name of the Temporary VC.</param>
    /// <param name="bitrate">The bitrate of the Temporary VC.</param>
    /// <param name="userLimit">The user limit of the Temporary VC.</param>
    /// <param name="userCount">The number of users in the Temporary VC.</param>
    /// <param name="userQueue">The order of users to join the Temporary VC.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddTempVc(ulong id, ulong createdBy, ulong guildId, string name, int bitrate, int userLimit,
        int userCount, List<ulong> userQueue)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        const string query = 
            "INSERT INTO temp_vcs (id, created_by, guild_id, master, name, bitrate, user_limit, user_count, " +
            " user_queue) VALUES (@id, @createdBy, @guildId, @master, @name, @bitrate, @userLimit, " +
            "@userCount, @userQueue)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric) { Value = (long)createdBy });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("master", NpgsqlDbType.Numeric) { Value = (long)createdBy });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        command.Parameters.Add(new NpgsqlParameter("bitrate", NpgsqlDbType.Integer) { Value = bitrate });
        command.Parameters.Add(new NpgsqlParameter("userLimit", NpgsqlDbType.Integer) { Value = userLimit });
        command.Parameters.Add(new NpgsqlParameter("userCount", NpgsqlDbType.Integer) { Value = userCount });

        string queue = "";
        userQueue.ForEach(item => queue += $"{item},");
        if (queue.EndsWith(','))
            queue = queue.Remove(queue.Length - 1);
        
        command.Parameters.Add(new NpgsqlParameter("userQueue", NpgsqlDbType.Text) { Value = queue });
            
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
    /// Removes a Temporary VC from the database.
    /// </summary>
    /// <param name="id">The ID of the Temporary VC.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveTempVc(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        const string query = "DELETE FROM temp_vcs WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
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
    /// Modifies a Temporary VC in the database.
    /// </summary>
    /// <param name="id">The ID of the Temporary VC.</param>
    /// <param name="master">The ID of the Temporary VC master.</param>
    /// <param name="name">The name of the Temporary VC.</param>
    /// <param name="bitrate">The bitrate of the Temporary VC.</param>
    /// <param name="userLimit">The user limit of the Temporary VC.</param>
    /// <param name="userCount">The number of users in the Temporary VC.</param>
    /// <param name="userQueue">The order of users to join the Temporary VC.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyTempVc(ulong id, ulong? master = null, string? name = null,
        int? bitrate = null, int? userLimit = null, int? userCount = null, List<ulong>? userQueue = null)
    {
        if (master is null && name is null && bitrate is null && userLimit is null && userCount is null
            && userQueue is null) return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        string query = 
            "UPDATE temp_vcs SET";

        if (master is not null)
        {
            query += " master=@master,";
            command.Parameters.Add(new NpgsqlParameter("master", NpgsqlDbType.Numeric) { Value = (long)master });
        }

        if (name is not null)
        {
            query += " name=@name,";
            command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        }

        if (bitrate is not null)
        {
            query += " bitrate=@bitrate,";
            command.Parameters.Add(new NpgsqlParameter("bitrate", NpgsqlDbType.Integer) { Value = bitrate });
        }

        if (userLimit is not null)
        {
            query += " user_limit=@userLimit,";
            command.Parameters.Add(new NpgsqlParameter("userLimit", NpgsqlDbType.Integer) { Value = userLimit });
        }

        if (userCount is not null)
        {
            query += " user_count=@userCount,";
            command.Parameters.Add(new NpgsqlParameter("userCount", NpgsqlDbType.Integer) { Value = userCount });
        }

        if (userQueue is not null)
        {
            query += " user_queue=@userQueue";
            string queue = userQueue.Aggregate("", (current, userId) => current + $"{userId},");
            if (queue.EndsWith(','))
                queue = queue.Remove(queue.Length - 1);
            command.Parameters.Add(new NpgsqlParameter("userQueue", NpgsqlDbType.Text) { Value = queue });
        }
        
        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
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
    /// Gets a Temporary VC from the database.
    /// </summary>
    /// <param name="id">The ID of theVC.</param>
    /// <returns>The Temporary VC.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Temporary VC doesn't exist.</exception>
    public static async Task<TempVcRow> GetTempVc(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        const string query = "SELECT * FROM temp_vcs WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            List<ulong> userQueue = [];
            try
            {
                string? queue = reader.GetString(reader.GetOrdinal("user_queue"));
                
                if (!queue.Contains(','))
                    userQueue = [Convert.ToUInt64(queue)];
                else
                    queue.Split(',').ToList().ForEach(item => userQueue.Add(Convert.ToUInt64(item)));
            }
            catch (Exception)
            {
                // ignore
            }
            return new TempVcRow
            {
                Id = (ulong)reader.GetInt64(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id")),
                Master = (ulong)reader.GetInt64(reader.GetOrdinal("master")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Bitrate = reader.GetInt32(reader.GetOrdinal("bitrate")),
                UserLimit = reader.GetInt32(reader.GetOrdinal("user_limit")),
                UserCount = reader.GetInt32(reader.GetOrdinal("user_count")),
                UserQueue = userQueue
            };
        }

        throw new KeyNotFoundException($"No Temp VC exists with id: {id}");
    }
    
    /// <summary>
    /// Gets all Temporary VCs from the database.
    /// </summary>
    /// <returns>The Temporary VCs.</returns>
    public static async Task<List<TempVcRow>> GetTempVcs()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        const string query = "SELECT * FROM temp_vcs";

        command.CommandText = query;
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        List<TempVcRow> tempVcs = [];
        while (await reader.ReadAsync())
        {
            List<ulong> userQueue = [];
            try
            {
                string queue = reader.GetString(reader.GetOrdinal("user_queue"));
                
                if (!queue.Contains(','))
                    userQueue = [Convert.ToUInt64(queue)];
                else
                    queue.Split(',').ToList().ForEach(item => userQueue.Add(Convert.ToUInt64(item)));
            }
            catch (Exception)
            {
                // ignore
            }
            tempVcs.Add(new TempVcRow
            {
                Id = (ulong)reader.GetInt64(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id")),
                Master = (ulong)reader.GetInt64(reader.GetOrdinal("master")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Bitrate = reader.GetInt32(reader.GetOrdinal("bitrate")),
                UserLimit = reader.GetInt32(reader.GetOrdinal("user_limit")),
                UserCount = reader.GetInt32(reader.GetOrdinal("user_count")),
                UserQueue = userQueue
            });
        }

        return tempVcs;
    }

    /// <summary>
    /// Checks if a Temporary VC exists in the database.
    /// </summary>
    /// <param name="id">The ID of the Temporary VC.</param>
    /// <returns>Whether the Temporary VC exists.</returns>
    public static async Task<bool> TempVcExists(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM temp_vcs WHERE id=@id";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

        return command.ExecuteScalar() is not null;
    }
}