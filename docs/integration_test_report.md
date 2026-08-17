# Reporte de Pruebas de Integración: Flujo Core de Ventas y Custodia

Este documento presenta los resultados de la suite de pruebas de integración ejecutada localmente para validar el ciclo de vida completo de las órdenes de venta, las transiciones de estado de base de datos, el historial de seguimiento y el bloqueo de custodia de roles.

---

## 🧪 Casos de Prueba Validados

Se ejecutó un escenario automatizado recreando el ciclo de pre-venta a venta, simulando las acciones de los distintos roles (`ASESOR` y `BACKOFFICE`) sobre los registros de órdenes de venta:

### 1. Transición de Estado Inicial (Borrador ➡️ En Revisión Supervisor)
* **Objetivo:** Verificar que un Asesor de ventas (`patricia`, ID `101`) pueda enviar una orden recién creada en estado `Borrador` (1) a revisión por su supervisor.
* **Resultado:** **PASADO (Succeeded)**. La base de datos permitió la transición a `En revisión supervisor` (2) de manera correcta.

### 2. Validación de Custodia (ASESOR no edita en `EN_BACKOFFICE`)
* **Objetivo:** Garantizar que si una orden ha sido trasladada al equipo de BackOffice (estado `En BackOffice`, ID `3`), un Asesor (`ASESOR`) no pueda modificar su estado o editarla.
* **Resultado:** **PASADO (Bloqueo Exitoso)**. La transición fue rechazada por la base de datos con la excepción controlada:
  `Transición de estado de 3 a 2 no permitida para el rol 'ASESOR'.`

### 3. Transición de BackOffice (BackOffice ➡️ En Revisión Supervisor)
* **Objetivo:** Confirmar que un usuario del equipo de BackOffice (`gvillanueva`, ID `237`) tenga custodia sobre la orden y sea capaz de devolverla al estado `En revisión supervisor` (2) con justificación.
* **Resultado:** **PASADO (Succeeded)**. La base de datos aceptó la transición correctamente.

### 4. Historial de Seguimiento (Auditoría de Estados)
* **Objetivo:** Validar que los triggers de base de datos registren correctamente cada cambio de estado, el usuario actor y el comentario de justificación en la tabla de historial `sales_order_status_history`.
* **Resultado:** **PASADO (Historial Completo)**. El log del historial contiene la secuencia completa de transiciones:
  1. `Borrador (1)` ➡️ `En revisión supervisor (2)` | Actor: `101` | Comentario: `"Enviando a revisión"`
  2. `En revisión supervisor (2)` ➡️ `En BackOffice (3)` | Actor: `101`
  3. `En BackOffice (3)` ➡️ `En revisión supervisor (2)` | Actor: `237` | Comentario: `"Devolviendo a revisión para corregir"`

---

## 🖥️ Evidencia de Ejecución de la Suite de Pruebas

```text
=============================================================
INTEGRATION TEST: SALES ORDER TRANSITIONS & CUSTODY CHECK
=============================================================
[SETUP] Created test order #16 in status 'Borrador' (1)

--- TEST CASE 1: Transition Borrador (1) -> En revisión supervisor (2) as ASESOR ---
✅ [TEST PASSED] Transition successfully executed under ASESOR role.

[SETUP] Manually set order #16 to status 'En BackOffice' (3) and custody to BackOffice ID 237

--- TEST CASE 2: Transition En BackOffice (3) -> En revisión supervisor (2) as ASESOR ---
✅ [TEST PASSED] Custody Enforcement Active: Transition successfully rejected as expected: Transición de estado de 3 a 2 no permitida para el rol 'ASESOR'.

--- TEST CASE 3: Transition En BackOffice (3) -> En revisión supervisor (2) as BACKOFFICE ---
✅ [TEST PASSED] BACKOFFICE was successfully allowed to change status of order.

--- TEST CASE 4: Verify Status History Log ---
[HISTORY LOG] From Status: 3 -> To Status: 2 | Changed By (Actor): 237 | Comment: Devolviendo a revisión para corregir
[HISTORY LOG] From Status: 2 -> To Status: 3 | Changed By (Actor): 101 | Comment: NULL
[HISTORY LOG] From Status: 1 -> To Status: 2 | Changed By (Actor): 101 | Comment: Enviando a revisión
✅ [TEST PASSED] Status changes were successfully logged in sales_order_status_history.

[CLEANUP] Transaction rolled back. Test data removed from database.
```

---

## 🔒 Mejoras Técnicas Incorporadas

Para lograr esta protección a nivel de API e Infraestructura, implementamos en la capa de persistencia (`SalesOrderRepository.cs`) una validación idéntica a la que realiza Leads:
* Antes de actualizar el estado de una orden, el repositorio ejecuta la consulta para determinar el rol del usuario actor a partir de su ID.
* Llama a la función PostgreSQL `sales_service.validate_status_transition` para validar la transición del estado actual al nuevo estado de destino bajo el rol determinado.
* De esta manera, el bloqueo de custodia se realiza con la máxima seguridad, tanto si la actualización es llamada por los controladores de API como por procesos en lote directamente.
