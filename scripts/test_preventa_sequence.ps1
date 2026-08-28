Write-Host "=== TEST 1: RESET TEST FLOW INSTANCE ==="
$body = '{"flowCode":"PIPELINE_TELECOM","entityType":"lead_presale","entityId":999,"actorId":101}'
$res = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/test-reset" -Method POST -Body $body -ContentType "application/json"
Write-Host "Stage: $($res.currentStage.name) | Status: $($res.status)"
Write-Host "Checkpoints:"
foreach ($cp in $res.checkpoints) { Write-Host "  - [$($cp.status)] $($cp.code): $($cp.name)" }

Write-Host "`n=== TEST 2: RESOLVE CP#11 -> APPROVED (Debe abrir CP#12 1ª Llamada) ==="
$cp11 = ($res.checkpoints | Where-Object { $_.idCheckpoint -eq 11 })[0]
$res11 = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp11.idCpInstance)/resolve" -Method POST -Body '{"status":"APPROVED","actorId":101}' -ContentType "application/json"
Write-Host "NextAction: $($res11.nextAction) | Message: $($res11.message)"
foreach ($tcp in $res11.triggeredCheckpoints) { Write-Host "  + [$($tcp.status)] $($tcp.code): $($tcp.name)" }

Write-Host "`n=== TEST 3: RESOLVE CP#12 -> APPROVED (Debe abrir CP#13 2ª Llamada) ==="
$detail = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/by-entity/lead_presale/999/detail" -Method GET
$cp12 = ($detail.checkpoints | Where-Object { $_.idCheckpoint -eq 12 -and $_.status -eq "PENDING" })[0]
$res12 = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp12.idCpInstance)/resolve" -Method POST -Body '{"status":"APPROVED","actorId":101}' -ContentType "application/json"
Write-Host "NextAction: $($res12.nextAction) | Message: $($res12.message)"
foreach ($tcp in $res12.triggeredCheckpoints) { Write-Host "  + [$($tcp.status)] $($tcp.code): $($tcp.name)" }

Write-Host "`n=== TEST 4: RESOLVE CP#13 -> APPROVED (Debe abrir CP#14 3ª Llamada) ==="
$detail = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/by-entity/lead_presale/999/detail" -Method GET
$cp13 = ($detail.checkpoints | Where-Object { $_.idCheckpoint -eq 13 -and $_.status -eq "PENDING" })[0]
$res13 = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp13.idCpInstance)/resolve" -Method POST -Body '{"status":"APPROVED","actorId":101}' -ContentType "application/json"
Write-Host "NextAction: $($res13.nextAction) | Message: $($res13.message)"
foreach ($tcp in $res13.triggeredCheckpoints) { Write-Host "  + [$($tcp.status)] $($tcp.code): $($tcp.name)" }

Write-Host "`n=== TEST 5: RESOLVE CP#14 -> APPROVED (Debe abrir CP#15 Alerta Cambio) ==="
$detail = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/by-entity/lead_presale/999/detail" -Method GET
$cp14 = ($detail.checkpoints | Where-Object { $_.idCheckpoint -eq 14 -and $_.status -eq "PENDING" })[0]
$res14 = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp14.idCpInstance)/resolve" -Method POST -Body '{"status":"APPROVED","actorId":101}' -ContentType "application/json"
Write-Host "NextAction: $($res14.nextAction) | Message: $($res14.message)"
foreach ($tcp in $res14.triggeredCheckpoints) { Write-Host "  + [$($tcp.status)] $($tcp.code): $($tcp.name)" }

Write-Host "`n=== TEST 6: RESOLVE CP#15 -> APPROVED (Fin de Preventa -> Avanza a Venta Creada) ==="
$detail = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/instances/by-entity/lead_presale/999/detail" -Method GET
$cp15 = ($detail.checkpoints | Where-Object { $_.idCheckpoint -eq 15 -and $_.status -eq "PENDING" })[0]
$res15 = Invoke-RestMethod -Uri "http://localhost:5072/api/flow/checkpoints/instances/$($cp15.idCpInstance)/resolve" -Method POST -Body '{"status":"APPROVED","actorId":101}' -ContentType "application/json"
Write-Host "NextAction: $($res15.nextAction) | Message: $($res15.message)"
Write-Host "New Stage: $($res15.currentStageName)"
