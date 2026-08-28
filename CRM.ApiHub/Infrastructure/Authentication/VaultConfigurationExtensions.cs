using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace CRM.ApiHub.Infrastructure.Authentication;

public static class VaultConfigurationExtensions
{
    public static IConfigurationBuilder AddVaultSecrets(this IConfigurationBuilder builder)
    {
        var vaultUrl = Environment.GetEnvironmentVariable("VAULT_ADDR") ?? "http://crm_vault:8200";
        var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN") ?? "NyxVaultRootToken2026";
        
        try
        {
            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);
            var vaultClientSettings = new VaultClientSettings(vaultUrl, authMethod);
            IVaultClient vaultClient = new VaultClient(vaultClientSettings);

            // Fetching the database secrets from KV v2
            var secret = vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "nyxcrm/database", mountPoint: "secret").GetAwaiter().GetResult();
            
            var memoryData = new Dictionary<string, string?>();
            foreach (var kvp in secret.Data.Data)
            {
                // Mapear "ConnectionStrings__DefaultConnection" -> "ConnectionStrings:DefaultConnection"
                var key = kvp.Key.Replace("__", ":");
                memoryData[key] = kvp.Value?.ToString();
            }

            builder.AddInMemoryCollection(memoryData);
            Console.WriteLine("[INFO] Secretos de Base de Datos cargados desde HashiCorp Vault exitosamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL ERROR] No se pudieron leer los secretos de Vault. Fail-Safe: {ex.Message}");
            throw; // Fail-Safe aborts startup
        }

        return builder;
    }
}
