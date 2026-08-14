@echo off
echo ========================================================
echo Subiendo cambios a GitHub en la rama brbrpatapin...
echo ========================================================

git checkout -b brbrpatapin
git add .
git commit -m "Mejoras UI: Integracion de Linea de Tiempo en Bandeja y Ficha Asesor"
git push -u origin brbrpatapin

echo.
echo ========================================================
echo ¡Proceso completado! Presiona cualquier tecla para salir.
echo ========================================================
pause
