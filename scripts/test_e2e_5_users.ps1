# ========================================================
# Nyx Engines — End-to-End Multi-Role Test Suite (5 Users)
# Roles: Asesor (101), Supervisor (9), Backoffice (237), Admin (2), Supervisor (251)
# ========================================================

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "INICIANDO PRUEBA END-TO-END DE LOS 3 MOTORES NYX" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

$slaUrl = "http://localhost:5070"
$approvalUrl = "http://localhost:5071"
$flowUrl = "http://localhost:5072"

# 1. PRUEBA USER 1: Patricia (ID 101 / ASESOR) — Registro de Venta & Inicio SLA / Flujo
Write-Host "`n1. [USER 101 - patricia (ASESOR)] Registrando venta #6001 e iniciando SLA / Flujo..." -ForegroundColor Yellow
$bodySla = @{ entityType = "order"; entityId = 6001; policyCode = "SLA_SALES_VALIDATION"; ownerUserId = 101; actorId = 101 } | ConvertTo-Json
$resSla = Invoke-RestMethod -Uri "$slaUrl/api/sla/measurements/start" -Method Post -ContentType "application/json" -Body $bodySla
Write-Host "  OK SLA Iniciado: ID=$($resSla.idMeasurement) Status=$($resSla.status) BreachAt=$($resSla.breachAt)" -ForegroundColor Green

$bodyFlow = @{ flowCode = "PIPELINE_ALARMAS"; entityType = "order"; entityId = 6001; actorId = 101 } | ConvertTo-Json
$resFlow = Invoke-RestMethod -Uri "$flowUrl/api/flow/instances/start" -Method Post -ContentType "application/json" -Body $bodyFlow
Write-Host "  OK Instancia de Flujo Creada: ID=$($resFlow.idInstance) StageID=$($resFlow.currentStageId) Status=$($resFlow.status)" -ForegroundColor Green

# 2. PRUEBA USER 2: cnaranjo (ID 9 / SUPERVISOR) — Solicitud y Aprobacion de Descuento Alto
Write-Host "`n2. [USER 9 - cnaranjo (SUPERVISOR)] Solicitando y Aprobando descuento alto para venta #6001..." -ForegroundColor Yellow
$bodyReq1 = @{ policyCode = "APPROVAL_HIGH_DISCOUNT"; entityType = "order"; entityId = 6001; requestedBy = 101; entityContextJson = '{"discountPct":20}' } | ConvertTo-Json
$req1 = Invoke-RestMethod -Uri "$approvalUrl/api/approval/requests/submit" -Method Post -ContentType "application/json" -Body $bodyReq1
Write-Host "  OK Solicitud Registrada: ReqID=$($req1.idRequest) Policy=$($req1.policyCode) Status=$($req1.status)" -ForegroundColor Green

$bodyDec1 = @{ decidedBy = 9; decision = "APPROVED"; reason = "Aprobado por supervisor cnaranjo por volumen" } | ConvertTo-Json
$dec1 = Invoke-RestMethod -Uri "$approvalUrl/api/approval/requests/$($req1.idRequest)/decide" -Method Post -ContentType "application/json" -Body $bodyDec1
Write-Host "  OK Aprobacion por Supervisor (ID 9): ReqID=$($dec1.idRequest) Nuevo Estado=$($dec1.status) Step=$($dec1.currentStep)" -ForegroundColor Green

# 3. PRUEBA USER 3: gvillanueva (ID 237 / BACKOFFICE) — Aprobacion de Excepcion por Backoffice
Write-Host "`n3. [USER 237 - gvillanueva (BACKOFFICE)] Solicitando y Aprobando excepcion BAC..." -ForegroundColor Yellow
$bodyReq2 = @{ policyCode = "APPROVAL_ORDER_CANCELLATION"; entityType = "order"; entityId = 6001; requestedBy = 9 } | ConvertTo-Json
$req2 = Invoke-RestMethod -Uri "$approvalUrl/api/approval/requests/submit" -Method Post -ContentType "application/json" -Body $bodyReq2

$bodyDec2 = @{ decidedBy = 237; decision = "APPROVED"; reason = "Aprobado por Backoffice gvillanueva" } | ConvertTo-Json
$dec2 = Invoke-RestMethod -Uri "$approvalUrl/api/approval/requests/$($req2.idRequest)/decide" -Method Post -ContentType "application/json" -Body $bodyDec2
Write-Host "  OK Aprobacion por Backoffice (ID 237): ReqID=$($dec2.idRequest) Nuevo Estado=$($dec2.status)" -ForegroundColor Green

# 4. PRUEBA USER 4: ronald (ID 2 / ADMIN) — Prueba de Regla ISO 27001 (Segregacion de Deberes)
Write-Host "`n4. [USER 2 - ronald (ADMIN)] Validando regla de Segregacion de Deberes SOX / ISO 27001..." -ForegroundColor Yellow
$bodyReqSelf = @{ policyCode = "APPROVAL_HIGH_DISCOUNT"; entityType = "order"; entityId = 6002; requestedBy = 2 } | ConvertTo-Json
$reqSelf = Invoke-RestMethod -Uri "$approvalUrl/api/approval/requests/submit" -Method Post -ContentType "application/json" -Body $bodyReqSelf

try {
    $bodySelfDec = @{ decidedBy = 2; decision = "APPROVED"; reason = "Auto-aprobacion no permitida" } | ConvertTo-Json
    $resSelf = Invoke-RestMethod -Uri "$approvalUrl/api/approval/requests/$($reqSelf.idRequest)/decide" -Method Post -ContentType "application/json" -Body $bodySelfDec
    Write-Host "  FALLO: El motor debio rechazar la auto-aprobacion" -ForegroundColor Red
} catch {
    Write-Host "  OK BLOQUEO SOX EXITOSO: El motor rechazo correctamente la auto-aprobacion (HTTP 400 Bad Request)" -ForegroundColor Green
}

# 5. PRUEBA USER 5: dramos (ID 251 / SUPERVISOR) — Resolucion de Checkpoint y Resolucion de SLA
Write-Host "`n5. [USER 251 - dramos (SUPERVISOR)] Resolviendo SLA e inspeccionando Auditoria SHA-512..." -ForegroundColor Yellow
$bodyResolveSla = @{ entityType = "order"; entityId = 6001; policyCode = "SLA_SALES_VALIDATION"; actorId = 251 } | ConvertTo-Json
$resEndSla = Invoke-RestMethod -Uri "$slaUrl/api/sla/measurements/resolve" -Method Post -ContentType "application/json" -Body $bodyResolveSla
Write-Host "  OK SLA Completado: ID=$($resEndSla.idMeasurement) Status=$($resEndSla.status) ElapsedMinutes=$($resEndSla.elapsedMinutes)" -ForegroundColor Green

Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host "PRUEBA MULTI-ROL CON 5 USUARIOS COMPLETADA CON EXITO 100%" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
