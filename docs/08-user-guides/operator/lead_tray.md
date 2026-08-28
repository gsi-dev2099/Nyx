# ISO Header
Código: USR-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Guía de Operador: Bolsa de Trabajo de Leads

## Propósito
Esta guía detalla cómo los asesores pueden utilizar la Bandeja de Leads (Bolsa de Trabajo) para gestionar y auto-asignarse prospectos recién ingresados.

## Acceso
1. Inicie sesión en el sistema con su cuenta de Asesor.
2. Navegue en el menú lateral a **Leads -> Bolsa de Trabajo**.

## Funcionamiento
La bolsa de trabajo muestra **exclusivamente** los Leads que tienen el estado de custodia vacío (`owner_user_id = null`). Estos son prospectos nuevos esperando ser atendidos.

### 1. Visualización Continua (Rendimiento)
La tabla carga de forma infinita (virtualizada). A medida que usted hace scroll hacia abajo, el sistema solicitará automáticamente el siguiente bloque de leads. Si hay muchos leads, verá un "esbozo" de carga temporal (esqueleto) mientras llegan los datos.

### 2. Auto-Asignación (Tomar Custodia)
Para comenzar a trabajar un Lead:
1. Haga clic en el botón azul **"Asignarme"** en la columna *Acción*.
2. Si el sistema responde con éxito, el Lead desaparecerá de la bolsa de trabajo y pasará a su bandeja personal ("Mis Leads"), cambiando su estado interno a **EN PROCESO**.
3. *Nota:* Por seguridad transaccional, si otro asesor hace clic al mismo tiempo en el mismo Lead, solo uno tendrá éxito (el otro recibirá un aviso de que ya no está disponible).

### 3. Resolución de Errores Comunes
- **Demasiadas peticiones (Rate Limit):** Si actualiza la página muchas veces por minuto, verá un banner rojo indicando "Demasiadas peticiones". El sistema lo bloquea temporalmente por seguridad. Espere 1 minuto y la página volverá a funcionar.
- **Error de Conexión:** Si el sistema muestra un esqueleto de carga infinito o un error rojo de red, verifique su conexión a internet y comuníquese con soporte L2 si persiste.
