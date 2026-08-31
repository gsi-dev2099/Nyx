# Estado Actual del Proyecto NYX CRM

Este documento resume de manera integral todo lo que se ha construido y consolidado en el repositorio hasta la fecha, abarcando arquitectura, lógica de negocio, diseño UI/UX, conexiones e infraestructura de pruebas.

---

## 1. Arquitectura General y Conexiones

El sistema está construido bajo **.NET 9** y se basa en una arquitectura de microservicios orquestada, dividida en los siguientes componentes principales:

### A. CRM.ApiHub (Orquestador Central)
- **Arquitectura:** Hexagonal (Puertos y Adaptadores). Separa estrictamente el dominio, los casos de uso (Application) y los detalles de infraestructura.
- **Persistencia:** Utiliza **PostgreSQL** mediante **Dapper** (micro-ORM) para ejecutar consultas SQL optimizadas, garantizando alto rendimiento bajo concurrencia.
- **Responsabilidad:** Es el único punto de entrada para el Frontend. Coordina la lectura y escritura de la base de datos, y se comunica con los motores satélites.

### B. Motores Satélites (Microservicios)
El `ApiHub` delega responsabilidades complejas a tres motores satélites que corren de forma independiente (típicamente en Docker):
1. **FlowEngine (Motor de Flujos):** Máquina de estados.
2. **SlaEngine (Motor de Acuerdos de Nivel de Servicio):** Controla los tiempos y cronómetros.
3. **ApprovalEngine (Motor de Aprobaciones):** (Integrado en el ecosistema, listo para expandirse).

### C. Patrón Híbrido de Integración (Resiliencia)
La comunicación HTTP entre el `ApiHub` y los motores está blindada con **Polly** (`Microsoft.Extensions.Http.Resilience`), utilizando Circuit Breakers y Reintentos. Sigue dos patrones:
- **Comunicación Síncrona/Bloqueante (FlowEngine):** El `ApiHub` le pregunta al `FlowEngine` si un cambio de estado es válido (ej. `validate-transition`). Si falla o el motor cae, la transacción de base de datos se **aborta** (`InvalidTransitionException`) para mantener la integridad (evitar estados fantasma).
- **Comunicación Asíncrona / Fire-and-forget (SlaEngine):** Una vez que el cambio de estado se guarda en PostgreSQL, el `ApiHub` avisa al `SlaEngine` (`TrackStateChangeAsync`). Si el motor cae, el sistema captura el error y registra un **Log Estructurado de Alta Precisión (Serilog)**, pero **no bloquea ni revierte** la transacción principal, priorizando la experiencia del usuario.

---

## 2. Lógica de Negocio: Módulo "Leads"

El núcleo desarrollado hasta el momento gira en torno a la gestión de **Leads** (Bolsa de Trabajo).

### Casos de Uso (Application)
- **UpdateLeadStatusUseCase:** 
  1. Extrae el Lead de PostgreSQL.
  2. Valida la transición consultando al `FlowEngine` de forma síncrona.
  3. Ejecuta el `UPDATE` inmutable con Dapper.
  4. Envía una notificación interna vía `INotificationService`.
  5. Invoca al `SlaEngine` en modo asíncrono para iniciar cronómetros.

### Excepciones de Dominio
- **`InvalidTransitionException`:** Garantiza que los errores lógicos del dominio no expongan información del framework, traduciéndose de manera limpia en errores HTTP 400 (Bad Request) hacia el frontend.

---

## 3. Frontend y Diseño UI/UX (Blazor)

### Arquitectura Híbrida
- **CRM.WebFrontend (Server) + CRM.WebFrontend.Client (WASM):** Una aproximación moderna que permite renderizado rápido y alta interactividad.

### Componentes Clave
- **Bolsa de Trabajo (Lead Tray):** 
  - Renderiza grandes volúmenes de datos usando `<Virtualize>`, protegiendo al DOM de cuellos de botella.
  - Implementa el botón **"Asignarme"**, que invoca a la API para cambiar el estado del Lead y transferir su custodia.
- **Esqueletos de Carga (Loading Skeletons):** Manejan el estado de espera (shimmer UI) dando un feedback visual fluido al usuario antes de inyectar los datos reales.
- **Inyección dinámica JWT:** Todo el tráfico HTTP del cliente incluye automáticamente el token de seguridad utilizando `PersistentAuthenticationStateProvider`.

### Diseño Visual
- Se implementó un sistema de tokens en Vanilla CSS (`index.css`) con variables estandarizadas para **Light Mode** y **Dark Mode**.
- Interfaz pulida, evitando colores genéricos y priorizando paletas HSL armoniosas, micro-animaciones en interacciones, y legibilidad total sin importar el tema activo.

---

## 4. Infraestructura de Pruebas (Tests)

Todo el ecosistema ha sido empaquetado en una estructura limpia bajo el directorio `/tests/`, garantizando máxima calidad en cada capa:

1. **CRM.ApiHub.Tests (xUnit + Moq):**
   - Pruebas unitarias de casos de uso (ej. `UpdateLeadStatusUseCaseTests`). Simulan caídas del FlowEngine y validan el control estricto de la transacción.
2. **CRM.WebFrontend.Client.Tests (bUnit):**
   - Pruebas de componentes de Blazor aisladas. Aseguran que los botones, esqueletos y la interfaz respondan lógicamente al estado inyectado.
3. **CRM.WebFrontend.E2ETests (Playwright + NUnit):**
   - Automatización de End-to-End contra contenedores Docker reales (Base de datos y API vivas).
   - Simula a un Asesor real haciendo login, navegando a la bolsa de trabajo y dando clic en "Asignarme", con aserciones sólidas de visibilidad de UI y notificaciones.

---

## Resumen de Integridad
Actualmente el proyecto cuenta con un entorno local **100% operativo** (respaldado por Docker Compose), compilación limpia sin errores (`dotnet build CRM.sln`), base de datos estructurada con repositorios Dapper optimizados, y una interfaz moderna escalable. Todo bajo los más altos estándares ISO y las directrices de Arquitectura Hexagonal.
