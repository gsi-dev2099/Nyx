# Pruebas End-to-End (E2E) con Playwright

Este documento define las directrices y procedimientos operativos para ejecutar pruebas E2E contra el entorno local, garantizando que el desarrollo frontend Blazor WASM-Hybrid y el backend .NET 9 + PostgreSQL funcionen en perfecta sintonía antes de cada PR.

## 1. Arquitectura de Pruebas
Las pruebas E2E utilizan el framework `Microsoft.Playwright.NUnit`.
- **Proyecto:** `CRM.WebFrontend.E2ETests`
- **Framework de Testing:** NUnit 3 + Playwright
- **Enfoque:** Pruebas contra contenedores reales (No Mocks).

## 2. Requisitos Previos

Dado que las pruebas E2E interactúan con una base de datos real y motores satélite, el entorno local de Docker debe estar activo.

### Levantar el Entorno Local
Asegúrese de levantar la infraestructura base de la aplicación desde la raíz del proyecto:
```bash
docker-compose up -d
```
Verifique que los servicios esenciales están arriba:
- `PostgreSQL` (5432)
- `Redis` (6379)
- `ApiHub` (Puerto configurado)
- `CRM.WebFrontend` (Puerto 5261)

## 3. Configuración de Variables de Entorno

El framework Playwright está configurado para inyectar credenciales de un usuario de prueba dinámicamente y apuntar a un BaseUrl (por defecto `http://localhost:5261`).

**⚠️ NUNCA hardcodee credenciales en los scripts de prueba.**

### En Windows (PowerShell):
Antes de ejecutar los tests, exponga las variables en su consola local:

```powershell
$env:TEST_BASE_URL="http://localhost:5261"
$env:TEST_USER_EMAIL="asesor_test@midominio.com"
$env:TEST_USER_PASSWORD="PasswordSeguro123!"
```

## 4. Instalación de Navegadores Playwright

Si es la primera vez que ejecuta un proyecto Playwright en su máquina, debe instalar los binarios de los navegadores subyacentes (Chromium, Firefox, WebKit):

```bash
cd CRM.WebFrontend.E2ETests
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install
```
*(Nota: Si no usa PowerShell Core `pwsh`, utilice `powershell` normal en Windows).*

## 5. Ejecución de la Suite de Pruebas

Para ejecutar las pruebas E2E de validación de la interfaz:

```bash
dotnet test CRM.WebFrontend.E2ETests
```

### Ejecutar con Interfaz Gráfica (Modo Cabeza / Headed)
Si necesita observar visualmente qué está haciendo Playwright (útil para debug):

```powershell
$env:HEADED="1"
dotnet test CRM.WebFrontend.E2ETests
```

## 6. Flujos E2E Cubiertos Actualmente

1. **Bolsa de Trabajo de Leads (`LeadTrayE2ETests.cs`)**:
   - Inicia sesión.
   - Navega a `/leads/tray`.
   - Espera la desaparición del `<LoadingSkeletonTable>` y el renderizado de la tabla `<Virtualize>`.
   - Busca el primer Lead y hace clic en "Asignarme".
   - Valida la desaparición o el mensaje de notificación de éxito.
