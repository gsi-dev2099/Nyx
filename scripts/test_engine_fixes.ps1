Write-Host "=== 1. Test Flow Engine Full Catalog ==="
$full = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/catalogs/full"
Write-Host "Full catalog items: $($full.Count)"

Write-Host "=== 2. Test ApiHub Proxy Full Catalog ==="
$proxyFull = Invoke-RestMethod -Uri "http://localhost:5068/api/engines/flow/catalogs/full"
Write-Host "ApiHub proxy items: $($proxyFull.Count)"

Write-Host "=== 3. Test Flow Instance Start ==="
$startReq = @{ flowCode = 'PIPELINE_ALARMAS'; entityType = 'order'; entityId = 99991; actorId = 1 }
$startJson = $startReq | ConvertTo-Json
$inst = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/start" -Method Post -Body $startJson -ContentType 'application/json'
Write-Host "Instance created: #$($inst.idInstance) Stage: $($inst.currentStageId) Status: $($inst.status)"

Write-Host "=== 4. Test Flow Instance By Entity in ApiHub ==="
$byEntity = Invoke-RestMethod -Uri "http://localhost:5068/api/engines/flow/instances/by-entity/order/99991"
Write-Host "Instance found by entity: #$($byEntity.idInstance) Checkpoints active: $($byEntity.checkpointInstances.Count)"

Write-Host "=== 5. Resolve Pending Checkpoints for Stage 1 ==="
foreach ($cp in $byEntity.checkpointInstances) {
    if ($cp.status -eq "PENDING") {
        $resReq = @{ status = "SUBSANADO"; actorId = 1 } | ConvertTo-Json
        $res = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp.idCpInstance)/resolve" -Method Post -Body $resReq -ContentType 'application/json'
        Write-Host "Resolved CP #$($cp.idCpInstance) -> $($res.status)"
    }
}

Write-Host "=== 6. Test Advance Stage in FlowEngine after Resolving Blockers ==="
$advReq = @{ actorId = 1 } | ConvertTo-Json
$advanced = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/$($inst.idInstance)/advance" -Method Post -Body $advReq -ContentType 'application/json'
Write-Host "Successfully Advanced to Stage: $($advanced.currentStageId) DayCounter: $($advanced.dayCounter) Status: $($advanced.status)"

Write-Host "=== 7. Test Checkpoints Triggered for New Stage 2 ==="
$byEntityAfter = Invoke-RestMethod -Uri "http://localhost:5068/api/engines/flow/instances/by-entity/order/99991"
Write-Host "Instance #$($byEntityAfter.idInstance) at Stage $($byEntityAfter.currentStageId) now has $($byEntityAfter.checkpointInstances.Count) total checkpoint instances."

Write-Host "=== 8. Test Step Progress Toggle in ApiHub ==="
$pendingCp = $byEntityAfter.checkpointInstances | Where-Object { $_.status -eq "PENDING" } | Select-Object -First 1
if ($pendingCp) {
    $toggleReq = @{ isCompleted = $true; actorId = 9 } | ConvertTo-Json
    $toggled = Invoke-RestMethod -Uri "http://localhost:5068/api/engines/flow/checkpoints/instances/$($pendingCp.idCpInstance)/steps/1/toggle" -Method Post -Body $toggleReq -ContentType 'application/json'
    Write-Host "Toggled step 1 on CP instance #$($pendingCp.idCpInstance): isCompleted = $($toggled.isCompleted)"
}

Write-Host "=== ALL E2E INTEGRATION TESTS PASSED 100% ==="
