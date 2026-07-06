using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CRM.ApiHub.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly ILogger<NpgsqlConnectionFactory> _logger;
    private readonly string _connectionString;
    
    public NpgsqlConnectionFactory(IConfiguration config, ILogger<NpgsqlConnectionFactory> logger)
    {
        _logger = logger;
        var rawConnectionString = config.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(config), "La cadena de conexión 'DefaultConnection' no está configurada.");
            
        // Desencripta si la cadena comienza con "Encrypted:"
        var decrypted = EncryptionHelper.Decrypt(rawConnectionString);
        
        try
        {
            var schemas = new List<string>();
            using (var connection = new NpgsqlConnection(decrypted))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT schema_name 
                    FROM information_schema.schemata 
                    WHERE schema_name NOT IN ('pg_catalog', 'information_schema', 'pg_toast') 
                      AND schema_name NOT LIKE 'pg_temp_%' 
                      AND schema_name NOT LIKE 'pg_toast_temp_%';", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            schemas.Add(reader.GetString(0));
                        }
                    }
                }
            }
            
            if (schemas.Count > 0)
            {
                var combinedSearchPath = string.Join(",", schemas);
                
                if (decrypted.Contains("Search Path="))
                {
                    var startIndex = decrypted.IndexOf("Search Path=");
                    var endIndex = decrypted.IndexOf(";", startIndex);
                    
                    var replacement = $"Search Path={combinedSearchPath}";
                    if (endIndex != -1)
                    {
                        decrypted = decrypted.Substring(0, startIndex) + replacement + decrypted.Substring(endIndex);
                    }
                    else
                    {
                        decrypted = decrypted.Substring(0, startIndex) + replacement;
                    }
                }
                else
                {
                    if (!decrypted.EndsWith(";"))
                    {
                        decrypted += ";";
                    }
                    decrypted += $"Search Path={combinedSearchPath};";
                }
            }
        }
        catch (System.Exception ex)
        {
            // En caso de error (ej. migración o db caída temporalmente en arranque),
            // se continúa con la cadena descifrada original pero registrando la excepción.
            _logger.LogError(ex, "Error al intentar obtener los esquemas de la base de datos en el arranque de NpgsqlConnectionFactory. Se continuará con la cadena descifrada original.");
        }
        
        _connectionString = decrypted;
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
}
