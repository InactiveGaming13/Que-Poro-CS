using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Media
{
    /// <summary>
    /// Adds Media to the database.
    /// </summary>
    /// <param name="creatorId">The User who added the Media.</param>
    /// <param name="url">The address of the Media.</param>
    /// <param name="alias">The Alias of the Media.</param>
    /// <param name="category">The Category of the Media.</param>
    /// <returns></returns>
    public static async Task<bool> AddMedia(ulong creatorId, string url, string? alias = null, string? category = null)
    {
        if (alias is null && category is null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query =
            "INSERT INTO media (created_by, alias, category, url) VALUES (@createdBy, " +
            "@alias, @category, @url)";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric) { Value = (long)creatorId });
        command.Parameters.Add(new NpgsqlParameter("url", NpgsqlDbType.Text) { Value = url });
        command.Parameters.Add(alias is null
            ? new NpgsqlParameter("alias", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("alias", NpgsqlDbType.Text) { Value = alias });
        command.Parameters.Add(category is null
            ? new NpgsqlParameter("category", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("category", NpgsqlDbType.Text) { Value = category });
        
        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception e)
        {
            if (e.Message.Contains("media_alias_key", StringComparison.CurrentCultureIgnoreCase))
                Console.WriteLine("Alias already exists!");
            
            if (e.Message.Contains("media_url_key", StringComparison.CurrentCultureIgnoreCase))
                Console.WriteLine("URL already exists!");
            
            Console.WriteLine(e);
            return false;
        }
    }
    
    /// <summary>
    /// Removes Media from the database.
    /// </summary>
    /// <param name="id">The ID of the Media.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveMedia(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "DELETE FROM media WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = id });
        
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
    /// Modifies specific Media in the database.
    /// </summary>
    /// <param name="id">The ID of the Media.</param>
    /// <param name="mediaAlias">The Alias of the Media.</param>
    /// <param name="mediaCategory">The Category of the Media.</param>
    /// <param name="url">The address of the Media.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyMedia(Guid id, string? mediaAlias = null, string? mediaCategory = null,
        string? url = null)
    {
        if (mediaAlias is null && mediaCategory is null && url is null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query = "MODIFY media SET";
        
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
        
        if (url is not null)
        {
            query += " url=@url";
            command.Parameters.Add(new NpgsqlParameter("mediaAlias", NpgsqlDbType.Text) { Value = url });
        }

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=@id";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });
        

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
    /// Gets the ID for specific Media.
    /// </summary>
    /// <param name="mediaAlias">The Alias of the Media.</param>
    /// <param name="mediaCategory">The Category of the Media.</param>
    /// <param name="url">The address of the Media.</param>
    /// <returns>The ID of the Media (if it exists).</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Media doesn't exist.</exception>
    public static async Task<Guid> GetMediaId(string url, string? mediaAlias = null, string? mediaCategory = null)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query = "SELECT id FROM media WHERE url=@url";
        
        if (mediaAlias is not null)
        {
            query += " AND media_alias=@mediaAlias";
            command.Parameters.Add(new NpgsqlParameter("mediaAlias", NpgsqlDbType.Text) { Value = mediaAlias });
        }
        
        if (mediaCategory is not null)
        {
            query += " media_category LIKE @mediaCategory";
            command.Parameters.Add(new NpgsqlParameter("mediaCategory", NpgsqlDbType.Text) { Value = mediaCategory });
        }

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("url", NpgsqlDbType.Text) { Value = url });

        if (command.ExecuteScalar() is not null)
            return (Guid)command.ExecuteScalar()!;

        throw new KeyNotFoundException(
            $"No Media exists with Alias: {mediaAlias} Category: {mediaCategory} URL: {url}");
    }

    /// <summary>
    /// Gets Media with its ID from the database.
    /// </summary>
    /// <param name="id">The ID of the Media.</param>
    /// <returns>The Media.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Media doesn't exist.</exception>
    public static async Task<MediaRow> GetMedia(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM media WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string? mediaAlias = null;
            try
            {
                mediaAlias = reader.GetString(reader.GetOrdinal("alias"));
            }
            catch (Exception)
            {
                // ignore
            }
            string? mediaCategory = null;
            try
            {
                mediaCategory = reader.GetString(reader.GetOrdinal("category"));
            }
            catch (Exception)
            {
                // ignore
            }
            return new MediaRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                Alias = mediaAlias,
                Category = mediaCategory,
                Url = reader.GetString(reader.GetOrdinal("url"))
            };
        }

        throw new KeyNotFoundException($"No Media exists with ID: {id}");
    }
    
    /// <summary>
    /// Gets Media with its Alias from the database.
    /// </summary>
    /// <param name="alias">The Alias of the Media.</param>
    /// <returns>The Media.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Media doesn't exist.</exception>
    public static async Task<MediaRow> GetMedia(string alias)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM media WHERE alias=@alias";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("alias", NpgsqlDbType.Text) { Value = alias });
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string? mediaCategory = null;
            try
            {
                mediaCategory = reader.GetString(reader.GetOrdinal("category"));
            }
            catch (Exception)
            {
                // ignore
            }
            return new MediaRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                Alias = reader.GetString(reader.GetOrdinal("alias")),
                Category = mediaCategory,
                Url = reader.GetString(reader.GetOrdinal("url"))
            };
        }

        throw new KeyNotFoundException($"No Media exists with Alias: {alias}");
    }
    
    /// <summary>
    /// Gets a List of Media from a Category from the database.
    /// </summary>
    /// <param name="category">The Category of the Media.</param>
    /// <returns>The List of Media.</returns>
    public static async Task<List<MediaRow>> GetMediaCategory(string category)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM media WHERE category=@category";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("category", NpgsqlDbType.Text) { Value = category });

        List<MediaRow> media = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string? mediaAlias = null;
            try
            {
                mediaAlias = reader.GetString(reader.GetOrdinal("alias"));
            }
            catch (Exception)
            {
                // ignore
            }

            media.Add(new MediaRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                Alias = mediaAlias,
                Category = reader.GetString(reader.GetOrdinal("category")),
                Url = reader.GetString(reader.GetOrdinal("url"))
            });
        }

        return media;
    }
    
    /// <summary>
    /// Checks if a Media exists in the database with its ID.
    /// </summary>
    /// <param name="id">The ID of the Media.</param>
    /// <returns>Whether the Media exists.</returns>
    public static async Task<bool> MediaExists(Guid id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM media WHERE id=@id";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });

        return command.ExecuteScalar() is not null;
    }
    
    /// <summary>
    /// Checks whether Media exists in the database with its Alias.
    /// </summary>
    /// <param name="url">The address of the Media.</param>
    /// <param name="alias">The Alias of the Media.</param>
    /// <returns>Whether the Media exists.</returns>
    public static async Task<bool> MediaExists(string? url = null, string? alias = null)
    {
        if (url is null && alias is null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM media WHERE url=@url OR alias=@alias";
        
        command.CommandText = query;
        command.Parameters.Add(url is null
        ? new NpgsqlParameter("url", NpgsqlDbType.Text) { Value = DBNull.Value }
        : new NpgsqlParameter("url", NpgsqlDbType.Text) { Value = url });
        command.Parameters.Add(alias is null
            ? new NpgsqlParameter("alias", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("alias", NpgsqlDbType.Text) { Value = alias });

        return command.ExecuteScalar() is not null;
    }
    
    /// <summary>
    /// Checks if a Media Category exists in the database.
    /// </summary>
    /// <param name="category">The Category of the Media.</param>
    /// <returns>Whether the Category exists.</returns>
    public static async Task<bool> MediaCategoryExists(string category)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM media WHERE category=@category";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("category", NpgsqlDbType.Text) { Value = category });

        return command.ExecuteScalar() is not null;
    }
}