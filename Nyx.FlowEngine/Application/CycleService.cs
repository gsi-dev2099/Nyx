using Nyx.FlowEngine.Domain.Entities;
using Nyx.FlowEngine.Infrastructure;
using System.Text.Json;

namespace Nyx.FlowEngine.Application;

public class CycleService : ICycleService
{
    private readonly ICycleRepository _repo;
    private readonly ILogger<CycleService> _logger;

    public CycleService(ICycleRepository repo, ILogger<CycleService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // ==========================================
    // CICLOS
    // ==========================================
    public async Task<IEnumerable<CycleDefinition>> GetCyclesAsync(bool includeInactive = false) => await _repo.GetCyclesAsync(includeInactive);

    public async Task<CycleDefinitionDetailDto?> GetCycleDetailAsync(long cycleId, bool includeInactive = false)
    {
        var cycle = await _repo.GetCycleByIdAsync(cycleId);
        if (cycle == null) return null;

        var stages = (await _repo.GetStagesByCycleAsync(cycleId)).ToList();
        var allCheckpoints = (await _repo.GetFullCheckpointsByCycleAsync(cycleId, includeInactive)).ToList();

        var stageDtos = stages.Select(s => new CycleStageDetailDto
        {
            IdStage = s.IdStage,
            IdCycle = s.IdCycle,
            StageCode = s.StageCode,
            Name = s.Name,
            Description = s.Description,
            OrderIndex = s.OrderIndex,
            IsTerminal = s.IsTerminal,
            SlaHours = s.SlaHours,
            PoliciesJson = s.PoliciesJson,
            Checkpoints = allCheckpoints.Where(c => c.TriggerStageId == s.IdStage).ToList()
        }).ToList();

        return new CycleDefinitionDetailDto
        {
            IdCycle = cycle.IdCycle,
            CycleCode = cycle.CycleCode,
            Name = cycle.Name,
            Description = cycle.Description,
            ScopeType = cycle.ScopeType,
            IsActive = cycle.IsActive,
            CurrentVersion = cycle.CurrentVersion,
            EntryPolicyJson = cycle.EntryPolicyJson,
            ExitPolicyJson = cycle.ExitPolicyJson,
            CreatedBy = cycle.CreatedBy,
            CreatedAt = cycle.CreatedAt,
            Stages = stageDtos,
            Checkpoints = allCheckpoints
        };
    }

    public async Task<CycleDefinition> CreateCycleAsync(CycleDefinition cycle)
    {
        var id = await _repo.CreateCycleAsync(cycle);
        cycle.IdCycle = id;
        await _repo.LogAuditAsync(cycle.CreatedBy, "CYCLE_CREATED", null, null, $"{{\"cycleCode\":\"{cycle.CycleCode}\",\"name\":\"{cycle.Name}\"}}");
        return cycle;
    }

    public async Task<bool> UpdateCycleAsync(long id, CycleDefinition cycle)
    {
        cycle.IdCycle = id;
        var ok = await _repo.UpdateCycleAsync(cycle);
        if (ok) await _repo.LogAuditAsync(1, "CYCLE_UPDATED", null, null, $"{{\"id\":{id},\"name\":\"{cycle.Name}\"}}");
        return ok;
    }

    public async Task<bool> SoftDeleteCycleAsync(long cycleId)
    {
        var ok = await _repo.SoftDeleteCycleAsync(cycleId);
        if (ok) await _repo.LogAuditAsync(1, "CYCLE_SOFT_DELETED", null, null, $"{{\"id\":{cycleId}}}");
        return ok;
    }

    // ==========================================
    // ETAPAS
    // ==========================================
    public async Task<IEnumerable<CycleStage>> GetStagesByCycleAsync(long cycleId) => await _repo.GetStagesByCycleAsync(cycleId);

    public async Task<CycleStage> CreateStageAsync(CycleStage stage)
    {
        var id = await _repo.CreateStageAsync(stage);
        stage.IdStage = id;
        await _repo.LogAuditAsync(1, "STAGE_CREATED", null, null, $"{{\"stageCode\":\"{stage.StageCode}\",\"cycleId\":{stage.IdCycle}}}");
        return stage;
    }

    public async Task<bool> ReorderStagesAsync(long cycleId, List<long> stageIdsInOrder)
    {
        short order = 1;
        foreach (var id in stageIdsInOrder)
        {
            await _repo.UpdateStageOrderAsync(id, order++);
        }
        await _repo.LogAuditAsync(1, "STAGES_REORDERED", null, null, $"{{\"cycleId\":{cycleId},\"count\":{stageIdsInOrder.Count}}}");
        return true;
    }

    public async Task<bool> UpdateStageAsync(long id, CycleStage stage)
    {
        stage.IdStage = id;
        var ok = await _repo.UpdateStageAsync(stage);
        if (ok) await _repo.LogAuditAsync(1, "STAGE_UPDATED", null, null, $"{{\"id\":{id},\"name\":\"{stage.Name}\"}}");
        return ok;
    }

    public async Task<bool> DeleteStageAsync(long stageId)
    {
        var ok = await _repo.DeleteStageAsync(stageId);
        if (ok) await _repo.LogAuditAsync(1, "STAGE_DELETED", null, null, $"{{\"id\":{stageId}}}");
        return ok;
    }

    // ==========================================
    // CHECKPOINTS
    // ==========================================
    public async Task<IEnumerable<CheckpointCatalog>> GetCheckpointsByCycleAsync(long cycleId, bool includeInactive = false) => await _repo.GetCheckpointsByCycleAsync(cycleId, includeInactive);
    public async Task<IEnumerable<CheckpointCatalogDetailDto>> GetFullCheckpointsByCycleAsync(long cycleId, bool includeInactive = false) => await _repo.GetFullCheckpointsByCycleAsync(cycleId, includeInactive);
    public async Task<CheckpointCatalog?> GetCheckpointByIdAsync(long id) => await _repo.GetCheckpointByIdAsync(id);
    public async Task<CheckpointCatalogDetailDto?> GetFullCheckpointByIdAsync(long id) => await _repo.GetFullCheckpointByIdAsync(id);

    public async Task<CheckpointCatalog> CreateCheckpointAsync(SaveCheckpointDto cp)
    {
        var id = await _repo.CreateCheckpointAsync(cp);
        cp.IdCheckpoint = id;

        if (cp.Steps != null && cp.Steps.Any())
        {
            short order = 1;
            foreach (var s in cp.Steps)
            {
                s.StepOrder = order++;
                s.IdCheckpoint = id;
            }
            await _repo.SaveCheckpointStepsAsync(id, cp.Steps);
        }

        await _repo.LogAuditAsync(1, "CHECKPOINT_CREATED", null, id, $"{{\"code\":\"{cp.Code}\",\"cycleId\":{cp.IdCycle},\"stepsCount\":{cp.Steps?.Count ?? 0}}}");
        return cp;
    }

    public async Task<bool> UpdateCheckpointAsync(long id, SaveCheckpointDto cp)
    {
        cp.IdCheckpoint = id;
        var ok = await _repo.UpdateCheckpointAsync(cp);
        if (ok)
        {
            if (cp.Steps != null)
            {
                short order = 1;
                foreach (var s in cp.Steps)
                {
                    s.StepOrder = order++;
                    s.IdCheckpoint = id;
                }
                await _repo.SaveCheckpointStepsAsync(id, cp.Steps);
            }
            await _repo.LogAuditAsync(1, "CHECKPOINT_UPDATED", null, id, $"{{\"id\":{id},\"name\":\"{cp.Name}\",\"stepsCount\":{cp.Steps?.Count ?? 0}}}");
        }
        return ok;
    }

    public async Task<bool> SoftDeleteCheckpointAsync(long cpId)
    {
        var ok = await _repo.SoftDeleteCheckpointAsync(cpId);
        if (ok) await _repo.LogAuditAsync(1, "CHECKPOINT_SOFT_DELETED", null, cpId, "{}");
        return ok;
    }

    public async Task<bool> ToggleCheckpointActiveAsync(long cpId)
    {
        var active = await _repo.ToggleCheckpointActiveAsync(cpId);
        await _repo.LogAuditAsync(1, active ? "CHECKPOINT_ACTIVATED" : "CHECKPOINT_DEACTIVATED", null, cpId, "{}");
        return active;
    }

    public async Task SaveCheckpointStepsAsync(long checkpointId, IEnumerable<CheckpointStep> steps)
    {
        await _repo.SaveCheckpointStepsAsync(checkpointId, steps);
        await _repo.LogAuditAsync(1, "CHECKPOINT_STEPS_SAVED", null, checkpointId, $"{{\"count\":{steps.Count()}}}");
    }

    public async Task SaveCheckpointCanvasSchemaAsync(long checkpointId, string canvasSchemaJson)
    {
        await _repo.UpdateCheckpointCanvasSchemaAsync(checkpointId, canvasSchemaJson);
        await _repo.LogAuditAsync(1, "CANVAS_SCHEMA_UPDATED", null, checkpointId, "{}");
    }

    // ==========================================
    // METADATOS Y CONCILIACIÓN (ROLES Y CARTERAS)
    // ==========================================
    public async Task<IEnumerable<MetaRole>> GetMetaRolesAsync() => await _repo.GetMetaRolesAsync();

    public async Task<MetaRole> CreateMetaRoleAsync(MetaRole role)
    {
        var id = await _repo.CreateMetaRoleAsync(role);
        role.IdRole = id;
        await _repo.LogAuditAsync(1, "META_ROLE_SAVED", null, null, $"{{\"roleCode\":\"{role.RoleCode}\",\"name\":\"{role.Name}\"}}");
        return role;
    }

    public async Task<IEnumerable<MetaPortfolio>> GetMetaPortfoliosAsync() => await _repo.GetMetaPortfoliosAsync();

    public async Task<MetaPortfolio> CreateMetaPortfolioAsync(MetaPortfolio portfolio)
    {
        var id = await _repo.CreateMetaPortfolioAsync(portfolio);
        portfolio.IdPortfolio = id;
        await _repo.LogAuditAsync(1, "META_PORTFOLIO_SAVED", null, null, $"{{\"portfolioCode\":\"{portfolio.PortfolioCode}\",\"name\":\"{portfolio.Name}\"}}");
        return portfolio;
    }

    public async Task<IEnumerable<MetaCampaign>> GetMetaCampaignsAsync() => await _repo.GetMetaCampaignsAsync();

    public async Task<MetaCampaign> CreateMetaCampaignAsync(MetaCampaign campaign)
    {
        var id = await _repo.CreateMetaCampaignAsync(campaign);
        campaign.IdCampaign = id;
        await _repo.LogAuditAsync(1, "META_CAMPAIGN_SAVED", null, null, $"{{\"campaignCode\":\"{campaign.CampaignCode}\",\"name\":\"{campaign.Name}\"}}");
        return campaign;
    }

    // ==========================================
    // POLÍTICAS
    // ==========================================
    public async Task<IEnumerable<CyclePolicyRule>> GetPoliciesAsync(long? cycleId) => await _repo.GetPoliciesAsync(cycleId);

    public async Task<CyclePolicyRule> SavePolicyRuleAsync(CyclePolicyRule rule)
    {
        var id = await _repo.SavePolicyRuleAsync(rule);
        rule.IdRule = id;
        await _repo.LogAuditAsync(1, "POLICY_SAVED", null, null, $"{{\"ruleCode\":\"{rule.RuleCode}\"}}");
        return rule;
    }

    // ==========================================
    // INSTANCIAS DE CICLO
    // ==========================================
    public async Task<CycleInstanceDetailDto> StartCycleInstanceAsync(string cycleCode, string entityType, long entityId, long actorId)
    {
        var cycle = await _repo.GetCycleByCodeAsync(cycleCode) 
            ?? throw new InvalidOperationException($"El ciclo '{cycleCode}' no existe o no está activo.");

        var stages = (await _repo.GetStagesByCycleAsync(cycle.IdCycle)).OrderBy(s => s.OrderIndex).ToList();
        if (!stages.Any()) throw new InvalidOperationException($"El ciclo '{cycleCode}' no tiene etapas configuradas.");

        var initialStage = stages.First();

        var instance = new CycleInstance
        {
            IdCycle = cycle.IdCycle,
            EntityType = entityType,
            EntityId = entityId,
            CurrentStageId = initialStage.IdStage,
            OwnerActorId = actorId,
            CurrentActorId = actorId,
            HandshakeStatus = "NONE",
            DayCounter = 1,
            Metadata = "{}",
            Facts = "{}",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };

        var instanceId = await _repo.CreateInstanceAsync(instance);
        instance.IdInstance = instanceId;

        await _repo.LogAuditAsync(actorId, "CYCLE_INSTANCE_STARTED", instanceId, null, $"{{\"cycleCode\":\"{cycleCode}\",\"initialStage\":\"{initialStage.StageCode}\"}}");

        // Instanciar Checkpoints de la etapa inicial (excluyendo los que esperan disparo por KO)
        var checkpoints = await _repo.GetCheckpointsByCycleAsync(cycle.IdCycle);
        foreach (var cp in checkpoints.Where(c => c.TriggerStageId == initialStage.IdStage && c.TriggeredByKo == null))
        {
            var scheduledDate = (cp.ActivationTrigger == "DELAYED_DAYS" && cp.DelayDays > 0)
                ? DateTime.UtcNow.AddDays(cp.DelayDays.Value)
                : (DateTime?)null;

            var cpInst = new CheckpointInstance
            {
                IdInstance = instanceId,
                IdCheckpoint = cp.IdCheckpoint,
                Status = scheduledDate.HasValue ? "SCHEDULED" : "PENDING",
                OpenedAtStage = initialStage.IdStage,
                ScheduledFor = scheduledDate,
                AnswersJson = "{}",
                CreatedAt = DateTime.UtcNow
            };
            await _repo.CreateCheckpointInstanceAsync(cpInst);
        }

        return (await GetInstanceDetailByIdAsync(instanceId))!;
    }

    public async Task<CycleInstanceDetailDto?> GetInstanceDetailByEntityAsync(string entityType, long entityId)
    {
        var inst = await _repo.GetInstanceByEntityAsync(entityType, entityId);
        return inst == null ? null : await GetInstanceDetailByIdAsync(inst.IdInstance);
    }

    public async Task<CycleInstanceDetailDto?> GetInstanceDetailByIdAsync(long instanceId)
    {
        var inst = await _repo.GetInstanceByIdAsync(instanceId);
        if (inst == null) return null;

        var cycle = await _repo.GetCycleByIdAsync(inst.IdCycle);
        var stages = (await _repo.GetStagesByCycleAsync(inst.IdCycle)).OrderBy(s => s.OrderIndex).ToList();
        var currentStage = stages.FirstOrDefault(s => s.IdStage == inst.CurrentStageId);
        var cpInstances = (await _repo.GetCheckpointInstancesForInstanceAsync(instanceId)).ToList();
        var transitions = (await _repo.GetTransitionsForInstanceAsync(instanceId)).ToList();

        var stageDtos = stages.Select(s => new CycleStageDetailDto
        {
            IdStage = s.IdStage,
            IdCycle = s.IdCycle,
            StageCode = s.StageCode,
            Name = s.Name,
            OrderIndex = s.OrderIndex,
            IsTerminal = s.IsTerminal,
            SlaHours = s.SlaHours,
            PoliciesJson = s.PoliciesJson
        }).ToList();

        return new CycleInstanceDetailDto
        {
            IdInstance = inst.IdInstance,
            IdCycle = inst.IdCycle,
            CycleCode = cycle?.CycleCode ?? "",
            CycleName = cycle?.Name ?? "",
            EntityType = inst.EntityType,
            EntityId = inst.EntityId,
            CurrentStageId = inst.CurrentStageId,
            CurrentStageName = currentStage?.Name ?? "",
            OwnerActorId = inst.OwnerActorId,
            CurrentActorId = inst.CurrentActorId,
            HandshakeStatus = inst.HandshakeStatus,
            HandshakeTargetActorId = inst.HandshakeTargetActorId,
            Status = inst.Status,
            Facts = inst.Facts,
            CreatedAt = inst.CreatedAt,
            Stages = stageDtos,
            Checkpoints = cpInstances,
            Transitions = transitions
        };
    }

    public async Task<CycleValidationResultDto> ValidateStageAdvanceAsync(long instanceId)
    {
        var inst = await _repo.GetInstanceByIdAsync(instanceId)
            ?? throw new InvalidOperationException("Instancia no encontrada.");

        var cpInstances = (await _repo.GetCheckpointInstancesForInstanceAsync(instanceId)).ToList();
        var blocking = cpInstances
            .Where(c => c.OpenedAtStage == inst.CurrentStageId && c.BlocksAdvance && c.Status != "APPROVED")
            .ToList();

        return new CycleValidationResultDto
        {
            CanAdvance = !blocking.Any(),
            BlockingReasons = blocking.Select(b => $"El checkpoint '{b.Name}' ({b.Code}) está en estado {b.Status} y es bloqueante.").ToList(),
            BlockingCheckpoints = blocking
        };
    }

    public async Task<CycleInstanceDetailDto> AdvanceStageAsync(long instanceId, long actorId)
    {
        var validation = await ValidateStageAdvanceAsync(instanceId);
        if (!validation.CanAdvance)
        {
            throw new InvalidOperationException($"No se puede avanzar la etapa: {string.Join(" | ", validation.BlockingReasons)}");
        }

        var inst = await _repo.GetInstanceByIdAsync(instanceId)!;
        var stages = (await _repo.GetStagesByCycleAsync(inst!.IdCycle)).OrderBy(s => s.OrderIndex).ToList();
        var currentStageIndex = stages.FindIndex(s => s.IdStage == inst.CurrentStageId);

        if (currentStageIndex < 0 || currentStageIndex >= stages.Count - 1)
        {
            inst.Status = "COMPLETED";
            inst.CompletedAt = DateTime.UtcNow;
            await _repo.UpdateInstanceAsync(inst);
            await _repo.LogAuditAsync(actorId, "CYCLE_COMPLETED", instanceId, null, "{}");
            return (await GetInstanceDetailByIdAsync(instanceId))!;
        }

        var nextStage = stages[currentStageIndex + 1];
        var fromStageId = inst.CurrentStageId;
        inst.CurrentStageId = nextStage.IdStage;
        await _repo.UpdateInstanceAsync(inst);

        await _repo.CreateTransitionAsync(new StageTransition
        {
            IdInstance = instanceId,
            FromStageId = fromStageId,
            ToStageId = nextStage.IdStage,
            Direction = "FORWARD",
            TriggeredBy = "MANUAL_OR_AUTO_ADVANCE",
            ActorId = actorId,
            TransitionedAt = DateTime.UtcNow
        });

        await _repo.LogAuditAsync(actorId, "STAGE_TRANSITION", instanceId, null, $"{{\"fromStage\":{fromStageId},\"toStage\":{nextStage.IdStage}}}");

        // Instanciar Checkpoints de la nueva etapa (excluyendo KOs encadenados)
        var checkpoints = await _repo.GetCheckpointsByCycleAsync(inst.IdCycle);
        foreach (var cp in checkpoints.Where(c => c.TriggerStageId == nextStage.IdStage && c.TriggeredByKo == null))
        {
            var scheduledDate = (cp.ActivationTrigger == "DELAYED_DAYS" && cp.DelayDays > 0)
                ? DateTime.UtcNow.AddDays(cp.DelayDays.Value)
                : (DateTime?)null;

            var cpInst = new CheckpointInstance
            {
                IdInstance = instanceId,
                IdCheckpoint = cp.IdCheckpoint,
                Status = scheduledDate.HasValue ? "SCHEDULED" : "PENDING",
                OpenedAtStage = nextStage.IdStage,
                ScheduledFor = scheduledDate,
                AnswersJson = "{}",
                CreatedAt = DateTime.UtcNow
            };
            await _repo.CreateCheckpointInstanceAsync(cpInst);
        }

        return (await GetInstanceDetailByIdAsync(instanceId))!;
    }

    public async Task<ResolveCheckpointResultDto> ResolveCheckpointAsync(long cpInstanceId, string status, string answersJson, long actorId)
    {
        var cpInst = await _repo.GetCheckpointInstanceByIdAsync(cpInstanceId)
            ?? throw new InvalidOperationException("Instancia de checkpoint no encontrada.");

        cpInst.Status = status.ToUpperInvariant();
        cpInst.ResolvedBy = actorId;
        cpInst.ResolvedAt = DateTime.UtcNow;
        cpInst.AnswersJson = answersJson ?? "{}";

        await _repo.UpdateCheckpointInstanceAsync(cpInst);
        await _repo.LogAuditAsync(actorId, "CHECKPOINT_RESOLVED", cpInst.IdInstance, cpInst.IdCheckpoint, $"{{\"status\":\"{status}\"}}");

        var validation = await ValidateStageAdvanceAsync(cpInst.IdInstance);
        return new ResolveCheckpointResultDto
        {
            Success = true,
            Message = $"Checkpoint resuelto con estado {status}.",
            NewStatus = status,
            StageCompleted = validation.CanAdvance,
            AutoAdvancedToNextStage = false
        };
    }

    // ==============================================================================
    // CONTRATO ZERO-LOGIC UI: /ui-context & /execute-action (GOBERNADO POR CHECKPOINT)
    // ==============================================================================
    public async Task<UiContextDto> GetUiContextAsync(long instanceId, long actorId)
    {
        var inst = await GetInstanceDetailByIdAsync(instanceId)
            ?? throw new InvalidOperationException($"Instancia {instanceId} no encontrada.");

        var currentStageDto = inst.Stages.FirstOrDefault(s => s.IdStage == inst.CurrentStageId);
        var activeCps = inst.Checkpoints.Where(c => c.OpenedAtStage == inst.CurrentStageId).ToList();

        var isMyTurn = (inst.CurrentActorId == null || inst.CurrentActorId == actorId || inst.OwnerActorId == actorId);
        var isTerminal = (inst.Status == "COMPLETED" || inst.Status == "CLOSED_KO" || inst.Status == "CANCELLED");

        var blockingCps = activeCps.Where(c => c.BlocksAdvance && c.Status != "APPROVED").ToList();
        var canAdvance = !blockingCps.Any() && !isTerminal;

        var uiHints = new UiHintsDto
        {
            IsReadOnly = isTerminal || !isMyTurn,
            CanAdvanceStage = canAdvance,
            BlockingReasons = blockingCps.Select(b => $"Checkpoint '{b.Name}' ({b.Code}) en estado {b.Status}.").ToList(),
            BadgeStatus = isTerminal ? inst.Status : (isMyTurn ? "EN_GESTION" : "CUSTODIA_OTRO_ASESOR"),
            BadgeColor = isTerminal ? (inst.Status == "COMPLETED" ? "success" : "danger") : (canAdvance ? "success" : "warning"),
            WarningMessage = !isMyTurn ? $"La ficha está asignada actualmente al Asesor #{inst.CurrentActorId}." : null
        };

        var allowedActions = new List<AllowedActionDto>();
        bool allowHandshake = false;

        if (!isTerminal && isMyTurn)
        {
            // Acciones directas evaluando la política individual de cada Checkpoint pendiente
            foreach (var cp in activeCps.Where(c => c.Status == "PENDING"))
            {
                var catalogCp = await _repo.GetCheckpointByIdAsync(cp.IdCheckpoint);
                var pol = PolicyRuleEvaluator.ParseCheckpointPolicies(catalogCp?.PoliciesJson);

                if (pol.EnableHandshake) allowHandshake = true;

                // Si requiere aprobación de supervisor y el actor es asesor
                string label = $"✅ Completar: {cp.Name}";
                string style = "btn-success";
                if (pol.RequiresSupervisorApproval && actorId < 200)
                {
                    label = $"📤 Enviar a Supervisión: {cp.Name}";
                    style = "btn-secondary";
                }

                // Acción 1: Aprobar / Completar
                allowedActions.Add(new AllowedActionDto
                {
                    ActionCode = $"APPROVE_{cp.Code}",
                    Label = label,
                    ButtonStyle = style,
                    RequiresConfirmation = pol.RequiresSupervisorApproval,
                    Effect = "RESOLVE_APPROVED",
                    CheckpointInstanceId = cp.IdCpInstance
                });

                // Acción 2: Rechazar / Disparo KO
                var rejectLabel = cp.FinalizesCycle ? $"⛔ KO Definitivo (Cierre): {cp.Name}" : $"❌ KO / Reintentar: {cp.Name}";
                allowedActions.Add(new AllowedActionDto
                {
                    ActionCode = $"REJECT_{cp.Code}",
                    Label = rejectLabel,
                    ButtonStyle = cp.FinalizesCycle ? "btn-danger" : "btn-warning",
                    RequiresReason = true,
                    ReasonOptions = new List<string> { "Cliente no interesado", "Precio elevado", "No contactado / Buzón", "No titular", "Sin cobertura fibra", "Rechazo Scoring", "Fraude detectado" },
                    Effect = cp.FinalizesCycle ? "FINALIZE_CYCLE_KO" : "TRIGGER_KO_CHAIN",
                    CheckpointInstanceId = cp.IdCpInstance
                });
            }

            // Si todos los checkpoints bloqueantes están listos -> botón de avanzar etapa
            if (canAdvance)
            {
                allowedActions.Add(new AllowedActionDto
                {
                    ActionCode = "ADVANCE_STAGE",
                    Label = "🚀 Avanzar a Siguiente Etapa",
                    ButtonStyle = "btn-primary",
                    RequiresConfirmation = true,
                    Effect = "ADVANCE_STAGE"
                });
            }

            // Botón de Handshake / Transferencia telefónica (solo si la política del checkpoint lo permite)
            if (allowHandshake && (inst.HandshakeStatus == "NONE" || inst.HandshakeStatus == "REVERTED"))
            {
                allowedActions.Add(new AllowedActionDto
                {
                    ActionCode = "REQUEST_HANDSHAKE",
                    Label = "📞 Transferir Llamada (Handshake)",
                    ButtonStyle = "btn-secondary",
                    RequiresActorSelection = true,
                    Effect = "HANDSHAKE_TRANSFER"
                });
            }
            else if (inst.HandshakeStatus == "PENDING_ACCEPTANCE" && inst.OwnerActorId == actorId)
            {
                allowedActions.Add(new AllowedActionDto
                {
                    ActionCode = "CANCEL_HANDSHAKE",
                    Label = "🚫 Cancelar Solicitud de Derivación",
                    ButtonStyle = "btn-danger",
                    Effect = "HANDSHAKE_CANCEL"
                });
            }
        }

        // Si el actor es el receptor que tiene la custodia aceptada -> puede devolverla si la aceptó por error o terminó
        if (inst.HandshakeStatus == "ACCEPTED" && inst.CurrentActorId == actorId)
        {
            allowedActions.Add(new AllowedActionDto
            {
                ActionCode = "REVERT_HANDSHAKE",
                Label = "↩️ Devolver Gestión al Titular (Aceptado por error o finalizado)",
                ButtonStyle = "btn-outline",
                RequiresReason = true,
                ReasonOptions = new List<string> { "Gestión finalizada con éxito", "Aceptada por error", "Cliente solicita hablar con titular", "Transferencia incorrecta" },
                Effect = "HANDSHAKE_REVERT"
            });
        }

        // Si el actor es el receptor del handshake y está pendiente
        if (inst.HandshakeStatus == "PENDING_ACCEPTANCE" && inst.HandshakeTargetActorId == actorId)
        {
            allowedActions.Add(new AllowedActionDto
            {
                ActionCode = "ACCEPT_HANDSHAKE",
                Label = "📥 Aceptar Transferencia Telefónica",
                ButtonStyle = "btn-success",
                Effect = "HANDSHAKE_ACCEPT"
            });
            allowedActions.Add(new AllowedActionDto
            {
                ActionCode = "REJECT_HANDSHAKE",
                Label = "❌ Rechazar Transferencia",
                ButtonStyle = "btn-danger",
                RequiresReason = true,
                ReasonOptions = new List<string> { "Ocupado en otra llamada", "Fuera de turno", "Perfil no apto" },
                Effect = "HANDSHAKE_REJECT"
            });
        }

        // Target actors (Directorio disponible)
        var targetActors = new UiTargetActorsDto
        {
            Supervisors = new List<UiTargetActorDto>
            {
                new() { ActorId = 201, Name = "Carlos Gómez (Supervisor TM)", Role = "Supervisor", Department = "Supervisión", Status = "AVAILABLE" },
                new() { ActorId = 202, Name = "Elena Morales (Supervisor TT)", Role = "Supervisor", Department = "Supervisión", Status = "AVAILABLE" }
            },
            PeerAdvisors = new List<UiTargetActorDto>
            {
                new() { ActorId = 101, Name = "Ronald Asesor 1", Role = "Asesor Ventas", Department = "Asesor", Status = "AVAILABLE" },
                new() { ActorId = 102, Name = "Laura Martínez (Asesor)", Role = "Asesor Ventas", Department = "Asesor", Status = "AVAILABLE" },
                new() { ActorId = 103, Name = "Andrés Vega (Retenciones)", Role = "Especialista Retención", Department = "Asesor", Status = "AVAILABLE" }
            },
            Backoffice = new List<UiTargetActorDto>
            {
                new() { ActorId = 301, Name = "Mesa Backoffice Solivesa", Role = "BO Tramitación", Department = "Backoffice", Status = "AVAILABLE" },
                new() { ActorId = 302, Name = "Mesa Backoffice Leyash", Role = "BO Verificación", Department = "Backoffice", Status = "AVAILABLE" }
            },
            QualityAuditors = new List<UiTargetActorDto>
            {
                new() { ActorId = 401, Name = "Auditoría Calidad RGPD", Role = "Auditor", Department = "Calidad", Status = "AVAILABLE" }
            }
        };

        return new UiContextDto
        {
            InstanceId = inst.IdInstance,
            CycleCode = inst.CycleCode,
            CycleName = inst.CycleName,
            CurrentStage = new UiStageDto
            {
                StageId = currentStageDto?.IdStage ?? inst.CurrentStageId,
                StageCode = currentStageDto?.StageCode ?? "",
                Name = currentStageDto?.Name ?? inst.CurrentStageName,
                OrderIndex = currentStageDto?.OrderIndex ?? 1,
                SlaHours = currentStageDto?.SlaHours,
                IsTerminal = currentStageDto?.IsTerminal ?? false
            },
            Ownership = new UiOwnershipDto
            {
                OwnerActorId = inst.OwnerActorId,
                CurrentActorId = inst.CurrentActorId,
                IsMyTurn = isMyTurn,
                HandshakeStatus = inst.HandshakeStatus,
                HandshakeTargetActorId = inst.HandshakeTargetActorId
            },
            UiHints = uiHints,
            ActiveCheckpoints = activeCps,
            AllowedActions = allowedActions,
            TargetActors = targetActors
        };
    }

    public async Task<ExecuteActionResultDto> ExecuteActionAsync(long instanceId, ExecuteActionRequest req)
    {
        var inst = await _repo.GetInstanceByIdAsync(instanceId)
            ?? throw new InvalidOperationException($"Instancia {instanceId} no encontrada.");

        var action = req.ActionCode.ToUpperInvariant();

        if (action == "ADVANCE_STAGE")
        {
            await AdvanceStageAsync(instanceId, req.ActorId);
            return new ExecuteActionResultDto
            {
                Success = true,
                Message = "Etapa avanzada con éxito.",
                ResultingState = "STAGE_ADVANCED",
                UpdatedUiContext = await GetUiContextAsync(instanceId, req.ActorId),
                InstanceDetail = await GetInstanceDetailByIdAsync(instanceId)
            };
        }

        if (action == "REQUEST_HANDSHAKE")
        {
            var res = await RequestHandshakeAsync(instanceId, req.TargetActorId ?? 0, req.ActorId, req.Reason);
            return new ExecuteActionResultDto
            {
                Success = res.Success,
                Message = res.Message,
                ResultingState = "HANDSHAKE_REQUESTED",
                UpdatedUiContext = await GetUiContextAsync(instanceId, req.ActorId),
                InstanceDetail = await GetInstanceDetailByIdAsync(instanceId)
            };
        }

        if (action == "ACCEPT_HANDSHAKE")
        {
            var res = await AcceptHandshakeAsync(instanceId, req.ActorId);
            return new ExecuteActionResultDto
            {
                Success = res.Success,
                Message = res.Message,
                ResultingState = "HANDSHAKE_ACCEPTED",
                UpdatedUiContext = await GetUiContextAsync(instanceId, req.ActorId),
                InstanceDetail = await GetInstanceDetailByIdAsync(instanceId)
            };
        }

        if (action == "CANCEL_HANDSHAKE")
        {
            var res = await CancelHandshakeAsync(instanceId, req.ActorId);
            return new ExecuteActionResultDto
            {
                Success = res.Success,
                Message = res.Message,
                ResultingState = "HANDSHAKE_CANCELLED",
                UpdatedUiContext = await GetUiContextAsync(instanceId, req.ActorId),
                InstanceDetail = await GetInstanceDetailByIdAsync(instanceId)
            };
        }

        if (action == "REJECT_HANDSHAKE")
        {
            var res = await RejectHandshakeAsync(instanceId, req.ActorId, req.Reason ?? "Rechazo de derivación");
            return new ExecuteActionResultDto
            {
                Success = res.Success,
                Message = res.Message,
                ResultingState = "HANDSHAKE_REJECTED",
                UpdatedUiContext = await GetUiContextAsync(instanceId, req.ActorId),
                InstanceDetail = await GetInstanceDetailByIdAsync(instanceId)
            };
        }

        if (action == "REVERT_HANDSHAKE")
        {
            var res = await RevertHandshakeAsync(instanceId, req.ActorId, req.Reason ?? "Devuelta al titular original (aceptada por error o finalizada)");
            return new ExecuteActionResultDto
            {
                Success = res.Success,
                Message = res.Message,
                ResultingState = "HANDSHAKE_REVERTED",
                UpdatedUiContext = await GetUiContextAsync(instanceId, req.ActorId),
                InstanceDetail = await GetInstanceDetailByIdAsync(instanceId)
            };
        }

        // Resolución de Checkpoint (Aprobar / Rechazar)
        if (req.CheckpointInstanceId.HasValue)
        {
            var cpInst = await _repo.GetCheckpointInstanceByIdAsync(req.CheckpointInstanceId.Value)
                ?? throw new InvalidOperationException("Instancia de checkpoint no encontrada.");

            var catalogCp = await _repo.GetCheckpointByIdAsync(cpInst.IdCheckpoint);
            var pol = PolicyRuleEvaluator.ParseCheckpointPolicies(catalogCp?.PoliciesJson);

            if (action.StartsWith("APPROVE"))
            {
                cpInst.Status = "APPROVED";
                cpInst.ResolvedBy = req.ActorId;
                cpInst.ResolvedAt = DateTime.UtcNow;
                cpInst.AnswersJson = req.AnswersJson ?? "{}";
                await _repo.UpdateCheckpointInstanceAsync(cpInst);

                await _repo.LogAuditAsync(req.ActorId, "CHECKPOINT_APPROVED", instanceId, cpInst.IdCheckpoint, $"{{\"answers\":{cpInst.AnswersJson}}}");

                // Auto-avanzar etapa si la política de este checkpoint lo tiene configurado
                if (pol.AutoAdvanceOnApproval)
                {
                    var validation = await ValidateStageAdvanceAsync(instanceId);
                    if (validation.CanAdvance)
                    {
                        await AdvanceStageAsync(instanceId, req.ActorId);
                    }
                }
            }
            else if (action.StartsWith("REJECT"))
            {
                cpInst.Status = "KO";
                cpInst.ResolvedBy = req.ActorId;
                cpInst.ResolvedAt = DateTime.UtcNow;
                cpInst.AnswersJson = $"{{\"reason\":\"{req.Reason}\",\"answers\":{req.AnswersJson ?? "{}"}}}";
                await _repo.UpdateCheckpointInstanceAsync(cpInst);

                await _repo.LogAuditAsync(req.ActorId, "CHECKPOINT_KO", instanceId, cpInst.IdCheckpoint, $"{{\"reason\":\"{req.Reason}\"}}");

                // 1. Si finaliza ciclo -> Cierre irrevocable
                if (catalogCp != null && catalogCp.FinalizesCycle)
                {
                    inst.Status = "CLOSED_KO";
                    inst.CompletedAt = DateTime.UtcNow;
                    await _repo.UpdateInstanceAsync(inst);
                    await _repo.CreateTransitionAsync(new StageTransition
                    {
                        IdInstance = instanceId,
                        FromStageId = inst.CurrentStageId,
                        ToStageId = inst.CurrentStageId,
                        Direction = "TERMINAL_KO",
                        TriggeredBy = $"KO_CHECKPOINT_{catalogCp.Code}",
                        ActorId = req.ActorId,
                        TransitionedAt = DateTime.UtcNow
                    });
                    await _repo.LogAuditAsync(req.ActorId, "CYCLE_CLOSED_KO", instanceId, cpInst.IdCheckpoint, $"{{\"reason\":\"{req.Reason}\"}}");
                }
                else
                {
                    // 2. Disparos Encadenados por KO (disparaSiKoDe)
                    var allCycleCps = await _repo.GetCheckpointsByCycleAsync(inst.IdCycle);
                    var chainedCps = allCycleCps.Where(c => c.TriggeredByKo == catalogCp?.IdCheckpoint).ToList();

                    foreach (var chained in chainedCps)
                    {
                        var newCpInst = new CheckpointInstance
                        {
                            IdInstance = instanceId,
                            IdCheckpoint = chained.IdCheckpoint,
                            Status = "PENDING",
                            OpenedAtStage = inst.CurrentStageId,
                            ScheduledFor = null,
                            AnswersJson = "{}",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _repo.CreateCheckpointInstanceAsync(newCpInst);
                        await _repo.LogAuditAsync(req.ActorId, "KO_CHAIN_TRIGGERED", instanceId, chained.IdCheckpoint, $"{{\"triggeredByKoOf\":{catalogCp?.IdCheckpoint}}}");
                    }

                    // 3. Retroceso de etapa si está configurado
                    if (catalogCp?.RollbackToStage.HasValue == true && catalogCp.RollbackToStage.Value != inst.CurrentStageId)
                    {
                        var fromStage = inst.CurrentStageId;
                        inst.CurrentStageId = catalogCp.RollbackToStage.Value;
                        await _repo.UpdateInstanceAsync(inst);
                        await _repo.CreateTransitionAsync(new StageTransition
                        {
                            IdInstance = instanceId,
                            FromStageId = fromStage,
                            ToStageId = inst.CurrentStageId,
                            Direction = "BACKWARD",
                            TriggeredBy = $"ROLLBACK_BY_KO_{catalogCp.Code}",
                            ActorId = req.ActorId,
                            TransitionedAt = DateTime.UtcNow
                        });
                        await _repo.LogAuditAsync(req.ActorId, "STAGE_ROLLBACK", instanceId, catalogCp.IdCheckpoint, $"{{\"from\":{fromStage},\"to\":{inst.CurrentStageId}}}");
                    }
                }
            }

            return new ExecuteActionResultDto
            {
                Success = true,
                Message = $"Acción '{action}' ejecutada correctamente.",
                ResultingState = inst.Status,
                UpdatedUiContext = await GetUiContextAsync(instanceId, req.ActorId),
                InstanceDetail = await GetInstanceDetailByIdAsync(instanceId)
            };
        }

        return new ExecuteActionResultDto
        {
            Success = false,
            Message = $"Acción '{action}' no reconocida.",
            ResultingState = inst.Status
        };
    }

    // ==========================================
    // IMPORTACIÓN Y EXPORTACIÓN JSON (GSI BACKUP)
    // ==========================================
    public async Task<GsiImportResultDto> ImportGsiBackupJsonAsync(long cycleId, string jsonContent)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var checkpointsArray = root.TryGetProperty("checkpoints", out var cpProp) ? cpProp : root;
        if (checkpointsArray.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("El JSON no contiene un array de checkpoints válido.");
        }

        var stages = (await _repo.GetStagesByCycleAsync(cycleId)).ToList();
        var stageDict = stages.ToDictionary(s => s.Name.ToLowerInvariant(), s => s.IdStage);

        var listToUpsert = new List<CheckpointCatalog>();
        int order = 1;

        foreach (var item in checkpointsArray.EnumerateArray())
        {
            var id = item.GetProperty("id").GetInt64();
            var nombre = item.GetProperty("nombre").GetString() ?? $"Checkpoint #{id}";
            var etapaStr = item.TryGetProperty("etapa", out var eProp) ? eProp.GetString() ?? "" : "";
            
            // Resolver etapa ID
            long? stageId = null;
            foreach (var kvp in stageDict)
            {
                if (kvp.Key.Contains(etapaStr.ToLowerInvariant()) || etapaStr.ToLowerInvariant().Contains(kvp.Key))
                {
                    stageId = kvp.Value;
                    break;
                }
            }
            if (!stageId.HasValue && stages.Any()) stageId = stages.First().IdStage;

            var bloquea = item.TryGetProperty("bloquea", out var bProp) && (bProp.GetString()?.ToLowerInvariant() == "sí" || bProp.GetString()?.ToLowerInvariant() == "si");
            var finaliza = item.TryGetProperty("finalizaCiclo", out var fProp) && fProp.GetBoolean();
            var disparaSiKo = item.TryGetProperty("disparaSiKoDe", out var dProp) && dProp.ValueKind == JsonValueKind.Number ? (long?)dProp.GetInt64() : null;

            var dueno = item.TryGetProperty("dueno", out var duProp) ? duProp.GetString() ?? "Asesor" : "Asesor";
            var fuente = item.TryGetProperty("fuente", out var fuProp) ? fuProp.GetString() ?? "INTERNAL" : "INTERNAL";
            var cartera = item.TryGetProperty("cartera", out var caProp) ? caProp.GetString() ?? "ENTITY" : "ENTITY";
            var pasos = item.TryGetProperty("pasos", out var paProp) ? paProp.GetString() ?? "" : "";
            var providersJson = item.TryGetProperty("proveedores", out var provProp) ? provProp.GetRawText() : "[\"Genérico\"]";
            var policiesJson = item.TryGetProperty("politicas", out var polProp) ? polProp.GetRawText() : "{\"enableHandshake\":true,\"onlyReceptorCanRevert\":true,\"allowOwnerCancelBeforeAccept\":true,\"handshakeTimeoutMinutes\":15,\"requiresSupervisorApproval\":false,\"autoAdvanceOnApproval\":false}";

            listToUpsert.Add(new CheckpointCatalog
            {
                IdCheckpoint = id,
                IdCycle = cycleId,
                TriggerStageId = stageId,
                Code = $"CP_TEL_{id:03d}",
                Name = nombre,
                Description = pasos,
                Origin = fuente,
                Scope = cartera,
                BlocksAdvance = bloquea,
                FinalizesCycle = finaliza,
                TriggeredByKo = disparaSiKo,
                Category = cartera,
                OwnerDept = dueno,
                ProvidersJson = providersJson,
                PoliciesJson = policiesJson,
                ExecutionOrder = order++,
                IsActive = true,
                Version = 1,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _repo.BulkUpsertCheckpointsAsync(cycleId, listToUpsert);
        await _repo.LogAuditAsync(1, "GSI_BACKUP_IMPORTED", null, null, $"{{\"count\":{listToUpsert.Count},\"cycleId\":{cycleId}}}");

        return new GsiImportResultDto
        {
            Success = true,
            Message = $"Se importaron con éxito {listToUpsert.Count} checkpoints al ciclo #{cycleId}.",
            StagesProcessed = stages.Count,
            CheckpointsImported = listToUpsert.Count
        };
    }

    public async Task<string> ExportGsiBackupJsonAsync(long cycleId)
    {
        var cps = await _repo.GetFullCheckpointsByCycleAsync(cycleId);
        var stages = await _repo.GetStagesByCycleAsync(cycleId);
        var stageDict = stages.ToDictionary(s => s.IdStage, s => s.Name);

        var list = cps.Select(c => new
        {
            id = c.IdCheckpoint,
            nombre = c.Name,
            cartera = c.Scope,
            campana = "",
            fuente = c.Origin,
            etapa = c.TriggerStageId.HasValue && stageDict.ContainsKey(c.TriggerStageId.Value) ? stageDict[c.TriggerStageId.Value] : "Preventa",
            dueno = c.OwnerDept,
            proveedores = JsonSerializer.Deserialize<List<string>>(c.ProvidersJson) ?? new List<string> { "Genérico" },
            bloquea = c.BlocksAdvance ? "Sí" : "No",
            retrocede = c.RollbackToStage.HasValue && stageDict.ContainsKey(c.RollbackToStage.Value) ? stageDict[c.RollbackToStage.Value] : "No aplica",
            finalizaCiclo = c.FinalizesCycle,
            disparaSiKoDe = c.TriggeredByKo,
            politicas = PolicyRuleEvaluator.ParseCheckpointPolicies(c.PoliciesJson),
            condicionPrevia = (string?)null,
            estado = c.IsActive ? "ACTIVO" : "PROPUESTO",
            pasos = c.Description ?? ""
        }).ToList();

        var payload = new
        {
            checkpoints = list,
            exportedAt = DateTime.UtcNow,
            engine = "Nyx Flow Engine Standalone v2.0"
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    // ==========================================
    // SIMULACIÓN TEMPORAL (D+X)
    // ==========================================
    public async Task<int> SimulateTimeAdvanceAsync(long instanceId, int days)
    {
        var count = await _repo.FastForwardTimeAsync(instanceId, days);
        await _repo.LogAuditAsync(1, "SIMULATE_TIME_ADVANCED", instanceId, null, $"{{\"days\":{days},\"activatedCount\":{count}}}");
        return count;
    }

    // ==========================================
    // HANDSHAKE TELEFONÍA & OWNERSHIP (GOBERNADO POR POLÍTICA DE CHECKPOINT)
    // ==========================================
    private async Task<(CheckpointPoliciesDto Policy, long? CheckpointId)> GetStageActiveHandshakePolicyAsync(long instanceId, long stageId)
    {
        var activeCps = await _repo.GetCheckpointInstancesForInstanceAsync(instanceId);
        var stageCps = activeCps.Where(c => c.OpenedAtStage == stageId).ToList();

        foreach (var cpInst in stageCps)
        {
            var catalogCp = await _repo.GetCheckpointByIdAsync(cpInst.IdCheckpoint);
            var pol = PolicyRuleEvaluator.ParseCheckpointPolicies(catalogCp?.PoliciesJson);
            if (pol.EnableHandshake)
            {
                return (pol, cpInst.IdCheckpoint);
            }
        }

        return (new CheckpointPoliciesDto(), null);
    }

    public async Task<(bool Success, string Message)> RequestHandshakeAsync(long instanceId, long targetActorId, long actorId, string? context)
    {
        var inst = await _repo.GetInstanceByIdAsync(instanceId) ?? throw new InvalidOperationException("Instancia no encontrada.");
        var (policy, cpId) = await GetStageActiveHandshakePolicyAsync(instanceId, inst.CurrentStageId);

        var validation = PolicyRuleEvaluator.ValidateHandshakeAction("REQUEST_HANDSHAKE", inst, actorId, policy);
        if (!validation.Allowed) return (false, validation.Reason);

        inst.HandshakeStatus = "PENDING_ACCEPTANCE";
        inst.HandshakeTargetActorId = targetActorId;
        inst.HandshakeRequestedAt = DateTime.UtcNow;
        await _repo.UpdateInstanceAsync(inst);

        await _repo.LogAuditAsync(actorId, "HANDSHAKE_REQUESTED", instanceId, cpId, $"{{\"targetActorId\":{targetActorId},\"context\":\"{context}\"}}");
        return (true, "Derivación solicitada al receptor con confirmación.");
    }

    public async Task<(bool Success, string Message)> AcceptHandshakeAsync(long instanceId, long actorId)
    {
        var inst = await _repo.GetInstanceByIdAsync(instanceId) ?? throw new InvalidOperationException("Instancia no encontrada.");
        var (policy, cpId) = await GetStageActiveHandshakePolicyAsync(instanceId, inst.CurrentStageId);

        var validation = PolicyRuleEvaluator.ValidateHandshakeAction("ACCEPT_HANDSHAKE", inst, actorId, policy);
        if (!validation.Allowed) return (false, validation.Reason);

        inst.HandshakeStatus = "ACCEPTED";
        inst.CurrentActorId = actorId;
        await _repo.UpdateInstanceAsync(inst);

        await _repo.LogAuditAsync(actorId, "HANDSHAKE_ACCEPTED", instanceId, cpId, $"{{\"newCurrentActor\":{actorId}}}");
        return (true, "Llamada aceptada. Gestión transferida al receptor.");
    }

    public async Task<(bool Success, string Message)> CancelHandshakeAsync(long instanceId, long actorId)
    {
        var inst = await _repo.GetInstanceByIdAsync(instanceId) ?? throw new InvalidOperationException("Instancia no encontrada.");
        var (policy, cpId) = await GetStageActiveHandshakePolicyAsync(instanceId, inst.CurrentStageId);

        var validation = PolicyRuleEvaluator.ValidateHandshakeAction("CANCEL_HANDSHAKE", inst, actorId, policy);
        if (!validation.Allowed) return (false, validation.Reason);

        inst.HandshakeStatus = "NONE";
        inst.HandshakeTargetActorId = null;
        inst.HandshakeRequestedAt = null;
        await _repo.UpdateInstanceAsync(inst);

        await _repo.LogAuditAsync(actorId, "HANDSHAKE_CANCELLED", instanceId, cpId, "{}");
        return (true, "Derivación cancelada por el dueño original.");
    }

    public async Task<(bool Success, string Message)> RejectHandshakeAsync(long instanceId, long actorId, string reason)
    {
        var inst = await _repo.GetInstanceByIdAsync(instanceId) ?? throw new InvalidOperationException("Instancia no encontrada.");
        var (policy, cpId) = await GetStageActiveHandshakePolicyAsync(instanceId, inst.CurrentStageId);

        var validation = PolicyRuleEvaluator.ValidateHandshakeAction("REJECT_HANDSHAKE", inst, actorId, policy);
        if (!validation.Allowed) return (false, validation.Reason);

        inst.HandshakeStatus = "NONE";
        inst.HandshakeTargetActorId = null;
        inst.HandshakeRequestedAt = null;
        await _repo.UpdateInstanceAsync(inst);

        await _repo.LogAuditAsync(actorId, "HANDSHAKE_REJECTED", instanceId, cpId, $"{{\"reason\":\"{reason}\"}}");
        return (true, "Llamada rechazada. La titularidad regresa al dueño original.");
    }

    public async Task<(bool Success, string Message)> RevertHandshakeAsync(long instanceId, long actorId, string reason)
    {
        var inst = await _repo.GetInstanceByIdAsync(instanceId) ?? throw new InvalidOperationException("Instancia no encontrada.");
        var (policy, cpId) = await GetStageActiveHandshakePolicyAsync(instanceId, inst.CurrentStageId);

        var validation = PolicyRuleEvaluator.ValidateHandshakeAction("REVERT_HANDSHAKE", inst, actorId, policy);
        if (!validation.Allowed) return (false, validation.Reason);

        inst.CurrentActorId = inst.OwnerActorId;
        inst.HandshakeStatus = "REVERTED";
        inst.HandshakeTargetActorId = null;
        await _repo.UpdateInstanceAsync(inst);

        await _repo.LogAuditAsync(actorId, "HANDSHAKE_REVERTED", instanceId, cpId, $"{{\"reason\":\"{reason}\",\"restoredTo\":{inst.OwnerActorId}}}");
        return (true, "Gestión revertida con éxito al titular original.");
    }

    // ==========================================
    // AUDITORÍA
    // ==========================================
    public async Task<IEnumerable<CycleAuditLog>> GetAuditLogsAsync(int limit = 50) => await _repo.GetAuditLogsAsync(limit);
}
