Write-Host "=== TEST 1: SIMULATOR HTML STATUS ==="
$htmlStatus = (Invoke-WebRequest -Uri "http://localhost:5072/test_flow_simulator.html" -UseBasicParsing).StatusCode
Write-Host "HTTP Status: $htmlStatus"

Write-Host "`n=== TEST 2: RESET TEST FLOW INSTANCE ==="
$body = '{"flowCode":"PIPELINE_TELECOM","entityType":"lead_presale","entityId":999,"actorId":101}'
$res = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/test-reset" -Method POST -Body $body -ContentType "application/json"
Write-Host "InstanceId: $($res.idInstance) | Stage: $($res.currentStage.name) | Status: $($res.status)"
Write-Host "Initial Checkpoints:"
foreach ($cp in $res.checkpoints) {
    Write-Host "  - [$($cp.status)] $($cp.code): $($cp.name) (Finaliza: $($cp.finalizesCycle), Bloquea: $($cp.blocksAdvance))"
}

Write-Host "`n=== TEST 3: RESOLVE CP#11 (Aceptación de Fichero) -> APPROVED ==="
$cp11 = ($res.checkpoints | Where-Object { $_.idCheckpoint -eq 11 })[0]
$resolveBody = '{"status":"APPROVED","actorId":101}'
$resResolve = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp11.idCpInstance)/resolve" -Method POST -Body $resolveBody -ContentType "application/json"
Write-Host "NextAction: $($resResolve.nextAction)"
Write-Host "Message: $($resResolve.message)"
Write-Host "Triggered Checkpoints:"
foreach ($tcp in $resResolve.triggeredCheckpoints) {
    Write-Host "  + [$($tcp.status)] $($tcp.code): $($tcp.name)"
}

Write-Host "`n=== TEST 4: RESOLVE CP#12 (1ª Llamada) -> APPROVED (Avanza a Venta Creada) ==="
$detail = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/by-entity/lead_presale/999/detail" -Method GET
$cp12 = ($detail.checkpoints | Where-Object { $_.idCheckpoint -eq 12 -and $_.status -eq "PENDING" })[0]
$resResolve12 = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp12.idCpInstance)/resolve" -Method POST -Body $resolveBody -ContentType "application/json"
Write-Host "NextAction: $($resResolve12.nextAction)"
Write-Host "Message: $($resResolve12.message)"
Write-Host "New Stage: $($resResolve12.currentStageName)"

Write-Host "`n=== TEST 5: VALIDATE ADVANCE CHECK ==="
$val = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/$($res.idInstance)/validate-advance" -Method GET
Write-Host "CanAdvance: $($val.canAdvance) | PendingBlocking: $($val.pendingBlockingCount)"
foreach ($r in $val.blockingReasons) {
    Write-Host "  * $r"
}

Write-Host "`n=== TEST 6: KO CHAINING (CP#15 KO -> Triggers CP#75 -> CP#75 KO -> Triggers CP#76) ==="
# Reset instance to test KO chain
$resChain = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/test-reset" -Method POST -Body $body -ContentType "application/json"
# Manually open CP#15 for test
$cp15Open = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/99999/resolve" -Method POST -Body '{"status":"KO","actorId":101}' -ContentType "application/json" -SkipHttpErrorCheck

# Test resolving CP#12 KO -> Terminal Finalizes Cycle
$cp11Inst = ($resChain.checkpoints | Where-Object { $_.idCheckpoint -eq 11 })[0]
Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp11Inst.idCpInstance)/resolve" -Method POST -Body '{"status":"APPROVED","actorId":101}' -ContentType "application/json" | Out-Null
$detailChain = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/by-entity/lead_presale/999/detail" -Method GET
$cp12Inst = ($detailChain.checkpoints | Where-Object { $_.idCheckpoint -eq 12 })[0]
$resKo = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp12Inst.idCpInstance)/resolve" -Method POST -Body '{"status":"KO","actorId":101}' -ContentType "application/json"
Write-Host "CP#12 KO NextAction: $($resKo.nextAction)"
Write-Host "Message: $($resKo.message)"
Write-Host "FlowStatus: $($resKo.flowStatus) | IsCycleClosed: $($resKo.isCycleClosed)"

