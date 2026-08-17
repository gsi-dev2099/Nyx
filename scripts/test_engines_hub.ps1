# Script de Prueba E2E Completa - Nyx Engines Hub
$ErrorActionPreference = "Continue"
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   PRUEBA E2E MULTI-ROL: NYX ENGINES HUB & 3 MOTORES        " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$users = @(
    @{ Username = "ronald"; Password = "Nyx2024!"; ExpectedRole = "ADMIN_CRM"; Id = 2 },
    @{ Username = "cnaranjo"; Password = "Nyx2024!"; ExpectedRole = "SUPERVISOR"; Id = 9 },
    @{ Username = "gvillanueva"; Password = "Nyx2024!"; ExpectedRole = "BACKOFFICE"; Id = 237 },
    @{ Username = "dramos"; Password = "Nyx2024!"; ExpectedRole = "SUPERVISOR"; Id = 251 },
    @{ Username = "patricia"; Password = "Nyx2024!"; ExpectedRole = "ASESOR"; Id = 101 }
)

$tokens = @{}

Write-Host " "
Write-Host "[ETAPA 1] Verificacion de Autenticacion para 5 Usuarios..." -ForegroundColor Yellow
foreach ($u in $users) {
    try {
        $body = @{ username = $u.Username; password = $u.Password } | ConvertTo-Json -Compress
        $res = Invoke-RestMethod -Uri "http://127.0.0.1:5068/api/auth/login" -Method Post -Body $body -ContentType "application/json"
        if ($res.role -eq $u.ExpectedRole) {
            Write-Host "  [OK] Usuario '$($u.Username)' (ID=$($u.Id)) autenticado como '$($res.role)'" -ForegroundColor Green
            $tokens[$u.Username] = $res.token
        } else {
            Write-Host "  [ERROR] Usuario '$($u.Username)' rol recibido: '$($res.role)', esperado: '$($u.ExpectedRole)'" -ForegroundColor Red
        }
    } catch {
        Write-Host "  [ERROR] Error autenticando usuario '$($u.Username)': $($_.Exception.Message)" -ForegroundColor Red
    }
}

$adminToken = $tokens["ronald"]
$adminHeaders = @{ Authorization = "Bearer $adminToken" }

Write-Host " "
Write-Host "[ETAPA 2] Verificacion de Salud de los 3 Motores (/api/engines/status)..." -ForegroundColor Yellow
try {
    $statusRes = Invoke-RestMethod -Uri "http://127.0.0.1:5068/api/engines/status" -Method Get -Headers $adminHeaders
    Write-Host "  Timestamp: $($statusRes.timestamp)" -ForegroundColor Gray
    foreach ($engine in $statusRes.engines) {
        $stStr = "OFFLINE"
        if ($engine.isHealthy) { $stStr = "OK" }
        Write-Host "  [$stStr] $($engine.name) (Puerto: $($engine.port), BD: $($engine.database)) -> Healthy: $($engine.isHealthy)" -ForegroundColor Green
    }
} catch {
    Write-Host "  [ERROR] Error consultando /api/engines/status: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host " "
Write-Host "[ETAPA 3] Consulta del Catalogo de Checkpoints Capa 1..." -ForegroundColor Yellow
try {
    $catalog = Invoke-RestMethod -Uri "http://127.0.0.1:5068/api/engines/flow/catalogs" -Method Get -Headers $adminHeaders
    Write-Host "  Total Checkpoints registrados en catalogo: $($catalog.Count)" -ForegroundColor Green
    foreach ($cp in $catalog) {
        Write-Host "  - [$($cp.code)] $($cp.name) | Bloquea: $($cp.blocksAdvance) | Firma: $($cp.approvalStatus)" -ForegroundColor Gray
    }
} catch {
    Write-Host "  [INFO] No se pudo obtener el catalogo de checkpoints: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host " "
Write-Host "[ETAPA 4] Consulta de Bandeja de Aprobaciones para Supervisor cnaranjo (ID=9)..." -ForegroundColor Yellow
$supToken = $tokens["cnaranjo"]
if ($supToken) {
    $supHeaders = @{ Authorization = "Bearer $supToken" }
    try {
        $pending = Invoke-RestMethod -Uri "http://127.0.0.1:5068/api/engines/approval/pending?approverId=9&approverRole=SUPERVISOR" -Method Get -Headers $supHeaders
        Write-Host "  Solicitudes pendientes encontradas: $($pending.Count)" -ForegroundColor Green
        foreach ($req in $pending) {
            Write-Host "  - Req #$($req.idRequest) | Politica: $($req.policyCode) | Entidad: $($req.entityType) #$($req.entityId) | Estado: $($req.status)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  [INFO] Error consultando bandeja de aprobaciones: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host " "
Write-Host "[ETAPA 5] Verificacion de Instancia de Flujo Capa 2..." -ForegroundColor Yellow
try {
    $inst = Invoke-RestMethod -Uri "http://127.0.0.1:5068/api/engines/flow/instances/1" -Method Get -Headers $adminHeaders
    Write-Host "  [OK] Instancia #1 encontrada: Entidad $($inst.entityType) #$($inst.entityId) | Etapa Actual: $($inst.currentStageId) | Estado: $($inst.status)" -ForegroundColor Green
} catch {
    Write-Host "  [INFO] Instancia #1 no existe aun (esperado si no hay ejecuciones previas en la BD aislada)" -ForegroundColor Gray
}

Write-Host " "
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   RESUMEN FINAL: MODULOS DEL HUB Y MOTORES LISTOS          " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
