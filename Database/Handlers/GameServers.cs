using System.Data;
using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public class GameServers
{
    /// <summary>
    /// Adds a Game Server to the database.
    /// </summary>
    /// <param name="procId">The Unix Process ID.</param>
    /// <param name="serverName">The friendly identifier of the Game Server.</param>
    /// <param name="serverDescription">A brief description to help identify the Game Server.</param>
    /// <param name="restartable">Whether the Game Server can be restarted/shutdown using the bot.</param>
    /// <param name="screenName">The name of the Unix screen the Game Server is running within.</param>
    /// <param name="restartMethod">The keybind(s) used to restart the Game Server.</param>
    /// <param name="shutdownMethod">The keybind(s) used to shut down the Game Server.</param>
    /// <returns>Whether the operation was succeeded.</returns>
    public static async Task<bool> AddGameServer(string procId, string serverName, string serverDescription,
        bool restartable, string screenName, string restartMethod, string shutdownMethod)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query =
            "INSERT INTO game_servers (id, server_name, server_description, restartable, screen_name, restart_method, " +
            "shutdown_method) VALUES (@procId, @serverName, @serverDescription, @restartable, @screenName, " +
            "@restartMethod, @shutdownMethod)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("procId", NpgsqlDbType.Text) { Value = procId });
        command.Parameters.Add(new NpgsqlParameter("serverName", NpgsqlDbType.Text) { Value = serverName });
        command.Parameters.Add(
            new NpgsqlParameter("serverDescription", NpgsqlDbType.Text) { Value = serverDescription });
        command.Parameters.Add(new NpgsqlParameter("restartable", DbType.Boolean) { Value = restartable });
        command.Parameters.Add(new NpgsqlParameter("screenName", NpgsqlDbType.Text) { Value = screenName });
        command.Parameters.Add(new NpgsqlParameter("restartMethod", NpgsqlDbType.Text) { Value = restartMethod });
        command.Parameters.Add(new NpgsqlParameter("shutdownMethod", NpgsqlDbType.Text) { Value = shutdownMethod });

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
    /// Removes a Game Server from the database.
    /// </summary>
    /// <param name="serverId">The Unix Process ID.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public static async Task<bool> RemoveGameServer(string serverId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "DELETE FROM game_servers WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Text) { Value = serverId });

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
    /// Modifies a Game Server in the database.
    /// </summary>
    /// <param name="serverId">The Unix Process ID.</param>
    /// <param name="serverName">The friendly identifier of the Game Server.</param>
    /// <param name="serverDescription">A brief description to help identify the Game Server.</param>
    /// <param name="restartable">Whether the Game Server can be restarted/shutdown using the bot.</param>
    /// <param name="screenName">The name of the Unix screen the Game Server is running within.</param>
    /// <param name="restartMethod">The keybind(s) used to restart the Game Server.</param>
    /// <param name="shutdownMethod">The keybind(s) used to shut down the Game Server.</param>
    /// <returns>Whether the operation succeeded.</returns>
    public static async Task<bool> ModifyGameServer(string serverId, string? serverName = null,
        string? serverDescription = null, bool? restartable = null, string? screenName = null,
        string? restartMethod = null, string? shutdownMethod = null)
    {
        if (serverName is null && serverDescription is null && restartable is null && screenName is null &&
            restartMethod is null && shutdownMethod is null)
            return false;

        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        string query = "UPDATE users SET";

        if (serverName is not null)
        {
            query += " server_name=@serverName,";
            command.Parameters.Add(new NpgsqlParameter("serverName", NpgsqlDbType.Text) { Value = serverName });
        }

        if (serverDescription is not null)
        {
            query += " server_description=@serverDescription,";
            command.Parameters.Add(new NpgsqlParameter("serverDescription", NpgsqlDbType.Text)
                { Value = serverDescription });
        }

        if (restartable is not null)
        {
            query += " restartable=@restartable,";
            command.Parameters.Add(new NpgsqlParameter("restartable", NpgsqlDbType.Boolean) { Value = restartable });
        }

        if (screenName is not null)
        {
            query += " screen_name=@screenName,";
            command.Parameters.Add(new NpgsqlParameter("screenName", NpgsqlDbType.Text) { Value = screenName });
        }

        if (restartMethod is not null)
        {
            query += " restart_method=@restartMethod,";
            command.Parameters.Add(new NpgsqlParameter("restartMethod", NpgsqlDbType.Text) { Value = restartMethod });
        }

        if (shutdownMethod is not null)
        {
            query += " shutdown_method=@shutdownMethod";
            command.Parameters.Add(new NpgsqlParameter("shutdownMethod", NpgsqlDbType.Text) { Value = shutdownMethod });
        }

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=@serverId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("serverId", NpgsqlDbType.Text) { Value = serverId });

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
    /// Gets a single Game Server from the database.
    /// </summary>
    /// <param name="serverId">The Unix Process ID.</param>
    /// <returns>The Game Server.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Game Server doesn't exist.</exception>
    public static async Task<GameServerRow> GetGameServer(string serverId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT * FROM game_servers WHERE id=@serverId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("serverId", NpgsqlDbType.Text) { Value = serverId });

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return new GameServerRow
            {
                Id = reader.GetString(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                ServerName = reader.GetString(reader.GetOrdinal("server_name")),
                ServerDescription = reader.GetString(reader.GetOrdinal("server_description")),
                Restartable = reader.GetBoolean(reader.GetOrdinal("restartable")),
                ScreenName = reader.GetString(reader.GetOrdinal("screen_name")),
                RestartMethod = reader.GetString(reader.GetOrdinal("restart_method")),
                ShutdownMethod = reader.GetString(reader.GetOrdinal("shutdown_method"))
            };
        }

        throw new KeyNotFoundException($"No Game Server exists with ID: {serverId}");
    }
    
    /// <summary>
    /// Gets a single Game Server from the database.
    /// </summary>
    /// <param name="allServers">Whether to only get restartable Game Servers.</param>
    /// <returns>The List of Game Servers.</returns>
    public static async Task<List<GameServerRow>> GetGameServers(bool allServers = false)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        string query = "SELECT * FROM game_servers WHERE id=@serverId";

        if (!allServers)
            query += " AND restartable=TRUE";

        command.CommandText = query;

        List<GameServerRow> gameServerRows = [];

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            gameServerRows.Add(new GameServerRow
            {
                Id = reader.GetString(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                ServerName = reader.GetString(reader.GetOrdinal("server_name")),
                ServerDescription = reader.GetString(reader.GetOrdinal("server_description")),
                Restartable = reader.GetBoolean(reader.GetOrdinal("restartable")),
                ScreenName = reader.GetString(reader.GetOrdinal("screen_name")),
                RestartMethod = reader.GetString(reader.GetOrdinal("restart_method")),
                ShutdownMethod = reader.GetString(reader.GetOrdinal("shutdown_method"))
            });
        }

        return gameServerRows;
    }
    
    /// <summary>
    /// Checks if a Game Server exists in the database.
    /// </summary>
    /// <param name="serverId">The Unix Process ID.</param>
    /// <returns>Whether the Game Server exists.</returns>
    public static async Task<bool> GameServerExists(string serverId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "SELECT created_at FROM game_servers WHERE id=@serverId";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("serverId", NpgsqlDbType.Text) { Value = serverId });

        return command.ExecuteScalar() is not null;
    }
}