using System.Text.Json;
using Nyx.FlowEngine.Domain.Entities;

namespace Nyx.FlowEngine.Application;

public class PolicyRuleEvaluator
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Parsea la política configurada a nivel de Checkpoint de forma segura y con valores por defecto intuitivos.
    /// </summary>
    public static CheckpointPoliciesDto ParseCheckpointPolicies(string? policiesJson)
    {
        if (string.IsNullOrWhiteSpace(policiesJson) || policiesJson == "{}")
        {
            return new CheckpointPoliciesDto();
        }

        try
        {
            return JsonSerializer.Deserialize<CheckpointPoliciesDto>(policiesJson, _jsonOptions) ?? new CheckpointPoliciesDto();
        }
        catch
        {
            return new CheckpointPoliciesDto();
        }
    }

    /// <summary>
    /// Evalúa la regla de Handshake de Telefonía para determinar si una acción es permitida según el actor, estado y política del checkpoint.
    /// </summary>
    public static (bool Allowed, string Reason) ValidateHandshakeAction(
        string action, 
        CycleInstance instance, 
        long actorId, 
        CheckpointPoliciesDto? checkpointPolicy = null,
        bool isSupervisorOrAdmin = false)
    {
        var policy = checkpointPolicy ?? new CheckpointPoliciesDto();

        if (isSupervisorOrAdmin)
        {
            return (true, "Acción autorizada por Supervisor / Administrador.");
        }

        switch (action.ToUpperInvariant())
        {
            case "REQUEST_HANDSHAKE": // Iniciar derivación
                if (!policy.EnableHandshake)
                {
                    return (false, "La política de este checkpoint no tiene habilitada la derivación / Handshake telefónico.");
                }
                if (instance.OwnerActorId.HasValue && instance.OwnerActorId.Value != actorId)
                {
                    return (false, "Solo el titular (Owner) actual de la gestión puede solicitar la transferencia.");
                }
                if (instance.HandshakeStatus == "PENDING_ACCEPTANCE")
                {
                    return (false, "Ya existe una transferencia pendiente de aceptación.");
                }
                return (true, "Transferencia solicitada correctamente.");

            case "CANCEL_HANDSHAKE": // Cancelar antes de que el receptor acepte
                if (instance.HandshakeStatus != "PENDING_ACCEPTANCE")
                {
                    return (false, "No hay ninguna transferencia pendiente para cancelar.");
                }
                if (!policy.AllowOwnerCancelBeforeAccept)
                {
                    return (false, "La política de este checkpoint no permite cancelar derivaciones ya emitidas.");
                }
                if (instance.OwnerActorId.HasValue && instance.OwnerActorId.Value != actorId)
                {
                    return (false, "Solo el titular original (Owner) puede cancelar la transferencia pendiente.");
                }
                return (true, "Transferencia cancelada por el dueño original.");

            case "ACCEPT_HANDSHAKE": // Receptor acepta la llamada
                if (instance.HandshakeStatus != "PENDING_ACCEPTANCE")
                {
                    return (false, "La transferencia ya no está pendiente.");
                }
                if (instance.HandshakeTargetActorId.HasValue && instance.HandshakeTargetActorId.Value != actorId)
                {
                    return (false, "Solo el asesor receptor destinatario puede aceptar la llamada.");
                }
                return (true, "Llamada aceptada por el receptor.");

            case "REJECT_HANDSHAKE": // Receptor rechaza la llamada
                if (instance.HandshakeStatus != "PENDING_ACCEPTANCE")
                {
                    return (false, "La transferencia ya no está pendiente.");
                }
                if (instance.HandshakeTargetActorId.HasValue && instance.HandshakeTargetActorId.Value != actorId)
                {
                    return (false, "Solo el asesor receptor destinatario puede rechazar la llamada.");
                }
                return (true, "Llamada rechazada. La gestión retorna al titular.");

            case "REVERT_HANDSHAKE": // Receptor devuelve la llamada al dueño tras haberla aceptado (por error o finalizada)
                if (instance.HandshakeStatus != "ACCEPTED")
                {
                    return (false, "Solo se pueden revertir gestiones que hayan sido previamente aceptadas.");
                }
                if (instance.CurrentActorId.HasValue && instance.CurrentActorId.Value != actorId)
                {
                    return (false, "Política de Seguridad CTI: Únicamente el asesor receptor que aceptó la gestión tiene permiso para revertirla al titular.");
                }
                return (true, "Gestión devuelta al titular original.");

            default:
                return (true, "Acción sin política restrictiva.");
        }
    }

    /// <summary>
    /// Evalúa si los facts de la instancia satisfacen las precondiciones de activación de un checkpoint.
    /// </summary>
    public static bool EvaluatePreconditions(string? conditionJson, string factsJson)
    {
        if (string.IsNullOrWhiteSpace(conditionJson) || conditionJson == "{}")
        {
            return true;
        }

        try
        {
            using var condDoc = JsonDocument.Parse(conditionJson);
            using var factDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(factsJson) ? "{}" : factsJson);

            foreach (var prop in condDoc.RootElement.EnumerateObject())
            {
                if (!factDoc.RootElement.TryGetProperty(prop.Name, out var factValue))
                {
                    return false;
                }

                if (factValue.ToString() != prop.Value.ToString())
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
