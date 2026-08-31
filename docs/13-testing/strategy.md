# ISO Header
Código: TST-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Estrategia de Pruebas de Carga (ISO 25010)

## Objetivos de Rendimiento
Debido a la implementación de rotación de familias de tokens con Redis, es mandatorio asegurar que el *Rate Limiting* (Capa 1) y el generador de JWT soporten los picos de concurrencia propios de un Call Center (ej. inicio de turnos masivos).

## Herramienta Seleccionada
**k6 (Grafana Labs)**. Elegido por su capacidad de scripting en JavaScript y su eficiencia para disparar miles de Virtual Users (VUs) desde un entorno local o de CI/CD.

## Escenarios de Prueba
1. **Pico de Login (Spike Test):** Simular 500 VUs iniciando sesión simultáneamente en un lapso de 1 minuto para evaluar el cuello de botella en BCrypt y la conexión a PostgreSQL.
2. **Refresh Token (Soak Test):** Simular solicitudes continuas de `RefreshToken` para medir el impacto en la memoria de Redis y la CPU al generar nuevos JWTs constantemente.
