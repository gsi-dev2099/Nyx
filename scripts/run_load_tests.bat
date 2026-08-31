@echo off
echo =======================================================
echo Iniciando Pruebas de Carga y Estres con k6 (ISO 25010)
echo Objetivo: CRM.ApiHub (/api/health)
echo =======================================================
echo.

docker run --rm -i --network nyx_default -v "%cd%\tests\load_tests:/tests" grafana/k6 run /tests/stress_api.js

echo.
echo =======================================================
echo Prueba Finalizada. Revisa los resultados en Grafana.
echo =======================================================
pause
