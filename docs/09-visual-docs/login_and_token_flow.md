# ISO Header
Código: VUF-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Visual User Flow: Login & Token Renewal

*Nota: Este diagrama representa pantallas y estados de experiencia del usuario, no decisiones lógicas de backend.*

```mermaid
journey
    title Experiencia de Autenticación y Rotación (Usuario)
    section 1. Ingreso a la Plataforma
      Pantalla de Login: 5: Usuario
      Click en "Iniciar Sesión": 4: Usuario
    section 2. Validación de Seguridad (Transparente)
      Pantalla de Carga (Spinner): 3: Sistema
      Ingreso al Dashboard Principal: 5: Usuario
    section 3. Sesión Activa (Trabajo)
      Navegación entre Módulos: 5: Usuario
      Rotación de Token en Background (Sin Interrupción): 5: Sistema
    section 4. Detección de Brecha (Robo de Token)
      Alerta Modal: "Sesión Comprometida": 1: Sistema
      Redirección a Pantalla de Login: 1: Sistema
```
