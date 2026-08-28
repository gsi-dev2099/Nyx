# ==============================================================================
# Script de Sembrado y Distribución Realista de Pre-Ventas con Nyx.FlowEngine
# ==============================================================================

Write-Host "Iniciando distribucion de leads entre Preventa (1ra llamada sin asignar) y Venta Creada..." -ForegroundColor Cyan

# 1. Obtener IDs de pre-ventas existentes
$leadIdsRaw = docker exec crm_postgres psql -U postgres -d nyx_crm -t -A -c "SELECT id_presale FROM lead_service.lead_pre_sale ORDER BY id_presale;"
$leadIds = $leadIdsRaw -split "`r?`n" | Where-Object { $_ -match '^\d+$' } | ForEach-Object { [int]$_ }

Write-Host "Total leads encontrados: $($leadIds.Count)" -ForegroundColor Green

if ($leadIds.Count -eq 0) {
    Write-Host "No hay leads para distribuir." -ForegroundColor Yellow
    exit 0
}

# 2. Limpiar instancias de flujo previas asociadas a estos leads en nyx_flow
$leadIdsJoined = $leadIds -join ","
docker exec crm_postgres psql -U postgres -d nyx_flow -c "DELETE FROM flow_instance WHERE entity_type = 'lead_presale' AND entity_id IN ($leadIdsJoined);" | Out-Null
Write-Host "Instancias de flujo previas limpiadas en nyx_flow." -ForegroundColor Yellow

# 3. Categorías de distribución
# 0: Preventa - 1ª Llamada Sin Asignar
# 1: Preventa - 1ª Llamada Asignada (Asesor 101)
# 2: Preventa - 2ª Llamada (Asesor 101)
# 3: Preventa - 3ª Llamada (Asesor 101)
# 4: Preventa - Alerta Cambio / Retención (Asesor 101)
# 5: Venta Creada - Ficha de Venta CP#16 (Etapa 13)
# 6: Rescate KO - Gestión Botada CP#75
# 7: Rescate KO - Gestión Alternas CP#76

$distCount = @{
    "1ra_Sin_Asignar" = 0
    "1ra_Asignada"    = 0
    "2da_Llamada"     = 0
    "3ra_Llamada"     = 0
    "Retencion"       = 0
    "Venta_Creada"    = 0
    "Botada_Rescate"  = 0
    "Alternas_Rescate"= 0
}

for ($i = 0; $i -lt $leadIds.Count; $i++) {
    $leadId = $leadIds[$i]
    $category = $i % 8

    # Actualizar estado base en nyx_crm
    switch ($category) {
        0 {
            # 1ª Llamada Sin Asignar
            docker exec crm_postgres psql -U postgres -d nyx_crm -c "
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = NULL, assigned_advisor_2_id = NULL, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = $leadId;" | Out-Null

            # Crear FlowInstance Etapa 12
            $instIdRaw = docker exec crm_postgres psql -U postgres -d nyx_flow -t -A -c "
                INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                VALUES (2, 'lead_presale', $leadId, 12, 'ACTIVE') RETURNING id_instance;"
            $instId = $instIdRaw.Trim()

            # Insertar checkpoints: CP#11 (APPROVED), CP#12 (PENDING), CP#13 (PENDING), CP#14 (PENDING), CP#15 (PENDING)
            docker exec crm_postgres psql -U postgres -d nyx_flow -c "
                INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                VALUES 
                ($instId, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 12, 'PENDING', 12, NULL, NULL),
                ($instId, 13, 'PENDING', 12, NULL, NULL),
                ($instId, 14, 'PENDING', 12, NULL, NULL),
                ($instId, 15, 'PENDING', 12, NULL, NULL);" | Out-Null

            $distCount["1ra_Sin_Asignar"]++
        }
        1 {
            # 1ª Llamada Asignada
            docker exec crm_postgres psql -U postgres -d nyx_crm -c "
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = NULL, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = $leadId;" | Out-Null

            $instIdRaw = docker exec crm_postgres psql -U postgres -d nyx_flow -t -A -c "
                INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                VALUES (2, 'lead_presale', $leadId, 12, 'ACTIVE') RETURNING id_instance;"
            $instId = $instIdRaw.Trim()

            docker exec crm_postgres psql -U postgres -d nyx_flow -c "
                INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                VALUES 
                ($instId, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 12, 'PENDING', 12, NULL, NULL),
                ($instId, 13, 'PENDING', 12, NULL, NULL),
                ($instId, 14, 'PENDING', 12, NULL, NULL),
                ($instId, 15, 'PENDING', 12, NULL, NULL);" | Out-Null

            $distCount["1ra_Asignada"]++
        }
        2 {
            # 2ª Llamada
            docker exec crm_postgres psql -U postgres -d nyx_crm -c "
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = $leadId;" | Out-Null

            $instIdRaw = docker exec crm_postgres psql -U postgres -d nyx_flow -t -A -c "
                INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                VALUES (2, 'lead_presale', $leadId, 12, 'ACTIVE') RETURNING id_instance;"
            $instId = $instIdRaw.Trim()

            docker exec crm_postgres psql -U postgres -d nyx_flow -c "
                INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                VALUES 
                ($instId, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 13, 'PENDING', 12, NULL, NULL),
                ($instId, 14, 'PENDING', 12, NULL, NULL),
                ($instId, 15, 'PENDING', 12, NULL, NULL);" | Out-Null

            $distCount["2da_Llamada"]++
        }
        3 {
            # 3ª Llamada
            docker exec crm_postgres psql -U postgres -d nyx_crm -c "
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = 101,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = $leadId;" | Out-Null

            $instIdRaw = docker exec crm_postgres psql -U postgres -d nyx_flow -t -A -c "
                INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                VALUES (2, 'lead_presale', $leadId, 12, 'ACTIVE') RETURNING id_instance;"
            $instId = $instIdRaw.Trim()

            docker exec crm_postgres psql -U postgres -d nyx_flow -c "
                INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                VALUES 
                ($instId, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 13, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 14, 'PENDING', 12, NULL, NULL),
                ($instId, 15, 'PENDING', 12, NULL, NULL);" | Out-Null

            $distCount["3ra_Llamada"]++
        }
        4 {
            # Retención / Alerta Cambio
            docker exec crm_postgres psql -U postgres -d nyx_crm -c "
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = 101,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = $leadId;" | Out-Null

            $instIdRaw = docker exec crm_postgres psql -U postgres -d nyx_flow -t -A -c "
                INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                VALUES (2, 'lead_presale', $leadId, 12, 'ACTIVE') RETURNING id_instance;"
            $instId = $instIdRaw.Trim()

            docker exec crm_postgres psql -U postgres -d nyx_flow -c "
                INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                VALUES 
                ($instId, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 13, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 14, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 15, 'PENDING', 12, NULL, NULL);" | Out-Null

            $distCount["Retencion"]++
        }
        5 {
            # Venta Creada (Etapa 13 / CP#16)
            docker exec crm_postgres psql -U postgres -d nyx_crm -c "
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = 101,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = $leadId;" | Out-Null

            $instIdRaw = docker exec crm_postgres psql -U postgres -d nyx_flow -t -A -c "
                INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                VALUES (2, 'lead_presale', $leadId, 13, 'ACTIVE') RETURNING id_instance;"
            $instId = $instIdRaw.Trim()

            docker exec crm_postgres psql -U postgres -d nyx_flow -c "
                INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                VALUES 
                ($instId, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 13, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 14, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 15, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 16, 'PENDING', 13, NULL, NULL),
                ($instId, 79, 'PENDING', 13, NULL, NULL),
                ($instId, 18, 'PENDING', 13, NULL, NULL),
                ($instId, 77, 'PENDING', 13, NULL, NULL),
                ($instId, 78, 'PENDING', 13, NULL, NULL),
                ($instId, 80, 'PENDING', 13, NULL, NULL);" | Out-Null

            $distCount["Venta_Creada"]++
        }
        6 {
            # Rescate KO: Gestión Botada CP#75
            docker exec crm_postgres psql -U postgres -d nyx_crm -c "
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = NULL, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = $leadId;" | Out-Null

            $instIdRaw = docker exec crm_postgres psql -U postgres -d nyx_flow -t -A -c "
                INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                VALUES (2, 'lead_presale', $leadId, 12, 'ACTIVE') RETURNING id_instance;"
            $instId = $instIdRaw.Trim()

            docker exec crm_postgres psql -U postgres -d nyx_flow -c "
                INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                VALUES 
                ($instId, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 12, 'KO', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 75, 'PENDING', 12, NULL, NULL);" | Out-Null

            $distCount["Botada_Rescate"]++
        }
        7 {
            # Rescate KO: Gestión Alternas CP#76
            docker exec crm_postgres psql -U postgres -d nyx_crm -c "
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = $leadId;" | Out-Null

            $instIdRaw = docker exec crm_postgres psql -U postgres -d nyx_flow -t -A -c "
                INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                VALUES (2, 'lead_presale', $leadId, 12, 'ACTIVE') RETURNING id_instance;"
            $instId = $instIdRaw.Trim()

            docker exec crm_postgres psql -U postgres -d nyx_flow -c "
                INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                VALUES 
                ($instId, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 13, 'KO', 12, 101, CURRENT_TIMESTAMP),
                ($instId, 76, 'PENDING', 12, NULL, NULL);" | Out-Null

            $distCount["Alternas_Rescate"]++
        }
    }
}

Write-Host "`n--- RESUMEN DE DISTRIBUCION ---" -ForegroundColor Green
$distCount.GetEnumerator() | ForEach-Object {
    Write-Host "$($_.Key): $($_.Value) leads" -ForegroundColor White
}
Write-Host "-------------------------------" -ForegroundColor Green
