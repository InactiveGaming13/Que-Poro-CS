using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class BannedPhrases
{
        public static async Task AddPhrase(ulong creatorId, int severity, string phrase, bool enabled = true)
        {
            await using NpgsqlConnection connection = await Database.GetConnection();
            await using NpgsqlCommand command = connection.CreateCommand();
            const string query = 
                "INSERT INTO banned_phrases (created_by, severity, phrase, enabled) " +
                "VALUES (@createdBy, @severity, @phrase, @enabled)";

            command.CommandText = query;
            command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric) { Value = (long)creatorId });
            command.Parameters.Add(new NpgsqlParameter("severity", NpgsqlDbType.Integer) { Value = severity });
            command.Parameters.Add(new NpgsqlParameter("phrase", NpgsqlDbType.Text) { Value = phrase });
            command.Parameters.Add(new NpgsqlParameter("enabled", NpgsqlDbType.Boolean) { Value = enabled });
            
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("banned_phrases_phrase_key", StringComparison.CurrentCultureIgnoreCase))
                    Console.WriteLine("Phrase already exists!");
                
                Console.WriteLine(e);
            }
        }
        
        public static async Task RemovePhrase(Guid id)
        {
            await using NpgsqlConnection connection = await Database.GetConnection();
            await using NpgsqlCommand command = connection.CreateCommand();
            
            const string query = "DELETE FROM banned_phrases WHERE id=@id";
            command.CommandText = query;
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });
            
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task ModifyPhrase(Guid id, ulong? guildId = null, int? severity = null,
            string? phrase = null, bool? enabled = null)
        {
            if (guildId is null && severity is null && phrase is null && enabled is null)
                return;
            
            await using NpgsqlConnection connection = await Database.GetConnection();
            await using NpgsqlCommand command = connection.CreateCommand();
            
            string query = "UPDATE banned_phrases SET";

            if (guildId is not null)
            {
                query += " guild_id=@guildId,";
                command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
            }

            if (severity is not null)
            {
                query += " severity=@severity,";
                command.Parameters.Add(new NpgsqlParameter("severity", NpgsqlDbType.Integer) { Value = severity });
            }

            if (phrase is not null)
            {
                query += " phrase=@phrase,";
                command.Parameters.Add(new NpgsqlParameter("phrase", NpgsqlDbType.Text) { Value = phrase });
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
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });
            
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task<Guid?> GetPhraseId(string phrase)
        {
            await using NpgsqlConnection connection = await Database.GetConnection();
            await using NpgsqlCommand command = connection.CreateCommand();
            
            const string query = "SELECT id FROM banned_phrases WHERE phrase LIKE @phrase";

            command.CommandText = query;
            command.Parameters.Add(new NpgsqlParameter("phrase", NpgsqlDbType.Text) { Value = phrase });

            return (Guid?)command.ExecuteScalar()!;
        }

        public static async Task<BannedPhraseRow?> GetPhrase(Guid id)
        {
            await using NpgsqlConnection connection = await Database.GetConnection();
            await using NpgsqlCommand command = connection.CreateCommand();
            
            const string query = "SELECT * FROM banned_phrases WHERE id=@id";

            command.CommandText = query;
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = id });
            
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                return new BannedPhraseRow
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                    Severity = reader.GetInt32(reader.GetOrdinal("severity")),
                    Phrase = reader.GetString(reader.GetOrdinal("phrase")),
                    Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"))
                };
            }

            return null;
        }
    }