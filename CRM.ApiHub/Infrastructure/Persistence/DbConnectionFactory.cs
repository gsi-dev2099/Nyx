using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks; // Necesario para Task
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CRM.ApiHub.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(); 
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
                    decrypted = (endIndex != -1) 
                        ? decrypted.Substring(0, startIndex) + replacement + decrypted.Substring(endIndex)
                        : decrypted.Substring(0, startIndex) + replacement;
                }
                else
                {
                    if (!decrypted.EndsWith(";")) decrypted += ";";
                    decrypted += $"Search Path={combinedSearchPath};";
                }
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error al intentar obtener esquemas de DB. Se continuará con la cadena original.");
        }
        
        // Asegurar configuración de Pool de conexiones Npgsql para producción
        if (!decrypted.Contains("Pooling=", StringComparison.OrdinalIgnoreCase))
        {
            if (!decrypted.EndsWith(";")) decrypted += ";";
            decrypted += "Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100;Connection Lifetime=300;Timeout=15;";
        }
        
        _connectionString = decrypted;
    }

    // Implementación síncrona
    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);

    // Implementación asíncrona solicitada por la interfaz
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}