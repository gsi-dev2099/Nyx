using Microsoft.AspNetCore.DataProtection.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace CRM.ApiHub.Infrastructure.Authentication;

public class VaultXmlRepository : IXmlRepository
{
    private readonly IVaultClient _vaultClient;
    private readonly string _secretPath;
    private readonly string _mountPoint;

    public VaultXmlRepository(string vaultUrl, string vaultToken, string secretPath, string mountPoint = "secret")
    {
        IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);
        var vaultClientSettings = new VaultClientSettings(vaultUrl, authMethod);
        _vaultClient = new VaultClient(vaultClientSettings);
        _secretPath = secretPath;
        _mountPoint = mountPoint;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        try
        {
            var secret = _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: _secretPath, mountPoint: _mountPoint).GetAwaiter().GetResult();
            if (secret?.Data?.Data == null) return Array.Empty<XElement>();

            var elements = new List<XElement>();
            foreach (var kvp in secret.Data.Data)
            {
                if (kvp.Value is string xmlString)
                {
                    elements.Add(XElement.Parse(xmlString));
                }
            }
            return elements.AsReadOnly();
        }
        catch (VaultApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // El secreto no existe todavía, retornar lista vacía
            return Array.Empty<XElement>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VaultXmlRepository] Error reading keys from Vault: {ex.Message}");
            throw;
        }
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        try
        {
            Dictionary<string, object> existingData = new Dictionary<string, object>();
            
            // Intentar leer los datos existentes para no sobrescribir otras llaves
            try
            {
                var secret = _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: _secretPath, mountPoint: _mountPoint).GetAwaiter().GetResult();
                if (secret?.Data?.Data != null)
                {
                    existingData = secret.Data.Data.ToDictionary(k => k.Key, v => v.Value);
                }
            }
            catch (VaultApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Ignorar si no existe
            }

            // Agregar o actualizar la nueva llave
            existingData[friendlyName] = element.ToString(SaveOptions.DisableFormatting);

            _vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(path: _secretPath, data: existingData, mountPoint: _mountPoint).GetAwaiter().GetResult();
            Console.WriteLine($"[VaultXmlRepository] Key '{friendlyName}' stored in Vault.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VaultXmlRepository] Error writing key to Vault: {ex.Message}");
            throw;
        }
    }
}
