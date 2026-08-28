# ISO Header
Código: ADR-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Uso de PostgreSQL y Dapper

## Contexto
Requerimos una base de datos relacional robusta y un acceso a datos extremadamente rápido y predecible.

## Decisión
- **Base de Datos:** PostgreSQL
- **Data Access:** Dapper (Micro-ORM)

## Consecuencias
- **Positivas:** Dapper se elige específicamente para mantener el **control total de las queries SQL** y evitar los JOINs innecesarios o *N+1 queries problem* que generan frecuentemente los ORMs pesados (como Entity Framework) en tablas de alta concurrencia.
- **Negativas:** Obliga al equipo a escribir consultas SQL nativas, lo que aumenta el tiempo de desarrollo inicial en queries complejas.
