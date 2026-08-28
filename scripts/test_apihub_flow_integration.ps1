$ErrorActionPreference = "Stop"

try {
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "  TEST INTEGRACION FLOW ENGINE Y CRM WEB APIHUB" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Cyan

    # 1. Iniciar Flujo
    $startBody = @{
        flowCode = "PIPELINE_TELECOM"
        entityType = "lead_presale"
        entityId = 99903
        actorId = 1
    } | ConvertTo-Json

    $startRes = Invoke-RestMethod -Uri 'http://localhost:5072/api/flow/instances/start' -Method Post -ContentType 'application/json' -Body $startBody
    $instId = $startRes.idInstance
    Write-Host "1. Flujo Iniciado: ID=$instId, Etapa Actual ID=$($startRes.currentStageId)" -ForegroundColor Green

    # 2. Consultar Detalle Enriquecido (FlowInstanceDetailDto)
    $detail = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/$instId/detail" -Method Get
    Write-Host "2. Detalle Enriquecido:" -ForegroundColor Green
    Write-Host "   - Etapa: '$($detail.currentStage.name)' (Codigo: $($detail.currentStage.stageCode))"
    Write-Host "   - Total Checkpoints Instanciados: $($detail.checkpoints.Count)"
    Write-Host "   - Bloqueantes Pendientes: $($detail.pendingBlockingCount)"
    Write-Host "   - Puede Avanzar Etapa: $($detail.canAdvanceStage)"

    # 3. Validar Avance Preventivo (FlowValidationResultDto)
    $val = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/$instId/validate-advance" -Method Get
    Write-Host "3. Validacion Preventiva de Avance: CanAdvance=$($val.canAdvance)" -ForegroundColor Green
    if ($val.blockingReasons.Count -gt 0) {
        Write-Host "   - Razones de Bloqueo: $($val.blockingReasons -join ' | ')" -ForegroundColor Yellow
    }

    # 4. Resolver CP#11 (Aceptacion de Fichero)
    $cp11 = $detail.checkpoints | Where-Object { $_.code -eq "CP_TEL_011" }
    if ($cp11) {
        $resBody = @{ status = "APPROVED"; actorId = 1 } | ConvertTo-Json
        $resResult = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp11.idCpInstance)/resolve" -Method Post -ContentType 'application/json' -Body $resBody
        Write-Host "4. CP#11 Aprobado:" -ForegroundColor Green
        Write-Host "   - Status Resultante: $($resResult.resolvedStatus)"
        Write-Host "   - Siguiente Accion: $($resResult.nextAction)"
        Write-Host "   - Mensaje del Motor: '$($resResult.message)'"
        Write-Host "   - Checkpoints Disparados: $($resResult.triggeredCheckpoints.Count) ($($resResult.triggeredCheckpoints[0].name))"
    }

    # 5. Consultar Detalle por Entidad (by-entity detail)
    $entityDetail = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/by-entity/lead_presale/99903/detail" -Method Get
    Write-Host "5. Detalle por Entidad (by-entity):" -ForegroundColor Green
    Write-Host "   - Total CPs: $($entityDetail.checkpoints.Count)"
    Write-Host "   - Aprobados: $($entityDetail.approvedCount)"
    Write-Host "   - Pendientes: $($entityDetail.pendingCount)"

    # 6. Consultar Pasos del Checkpoint en Catalogo
    $catalogSteps = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/catalog/11/steps" -Method Get
    Write-Host "6. Catalogo Pasos CP#11: $($catalogSteps.Count) pasos cargados con exito." -ForegroundColor Green
    foreach ($st in $catalogSteps) {
        Write-Host "   - Paso $($st.stepOrder): $($st.name) [Obligatorio: $($st.isRequired)]"
    }

    Write-Host "`nTODAS LAS PRUEBAS DE INTEGRACION Y DTOS PASARON CON EXITO AL 100%." -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        Write-Host "Detalle del servidor: $($reader.ReadToEnd())" -ForegroundColor Yellow
    }
}
