# Pruebas de Carga y Estrés (ISO 25010)

En Nyx CRM utilizamos **k6** (de Grafana Labs) para certificar la confiabilidad y el rendimiento (Performance Efficiency) del sistema bajo cargas simuladas. 

## Ejecución de Pruebas

Para mantener los entornos limpios, no instalamos `k6` nativamente en la estación de trabajo. Toda la prueba se inyecta mediante contenedores de Docker efímeros.

### Script de Estrés Base
**Ruta:** `tests/load_tests/stress_api.js`

El script ejecuta 3 _stages_ sobre el endpoint `http://crm_apihub:5068/api/health`:
1. Ramp-up a 50 usuarios concurrentes (10s).
2. Sostenimiento de la carga de 50 usuarios (30s).
3. Ramp-down de enfriamiento a 0 usuarios (10s).

**Thresholds (Límites Aceptables):**
- El 95% de las respuestas deben ser menores a 200ms (`p(95)<200`).
- La tasa de error debe ser inferior al 1% (`rate<0.01`).

### ¿Cómo iniciar la prueba?

1. Asegúrate de que tu ecosistema CRM esté levantado con `docker-compose up -d`.
2. Desde la raíz del repositorio, ejecuta:
   ```cmd
   .\scripts\run_load_tests.bat
   ```
3. El script descargará la imagen `grafana/k6` si no existe, la conectará a la red interna del Docker Compose (`nyx_default`) y comenzará a disparar tráfico hacia el `ApiHub`.

## Monitoreo en Grafana (Observabilidad)
El principal objetivo de esta prueba es visualizar el estrés en vivo a través de nuestra topología inmutable.

Mientras la consola muestra la progresión del test, abre Grafana (`http://localhost:3000`) y dirígete al Dashboard **CRM ApiHub Observability**:
1. **HTTP Request Rate**: Verás un pico masivo que refleja los requests inyectados.
2. **Memory & CPU Usage**: Deberás observar si existe un Memory Leak (si la RAM no se estabiliza después del test) o un estrangulamiento térmico de la CPU.
3. **Live Logs (Loki)**: Vigila este panel buscando la inyección de advertencias o errores (si la carga rompe conexiones a la base de datos o gatilla rate limiters).
