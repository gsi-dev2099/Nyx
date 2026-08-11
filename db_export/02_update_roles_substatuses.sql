-- Actualizaciones y migraciones incrementales de base de datos NYX CRM
BEGIN;

-- 1. Insertar subestado 23 (POR_CORREGIR_ASESOR) si no existe
INSERT INTO sales_service.order_substatus (id_substatus, id_status, code, name, description) 
OVERRIDING SYSTEM VALUE
VALUES (23, 11, 'POR_CORREGIR_ASESOR', 'Por corregir por Asesor', 'Incidencia observada devuelta para corrección por el asesor') 
ON CONFLICT (id_substatus) DO UPDATE SET name = EXCLUDED.name;

-- 2. Asegurar mapeos de roles para usuarios de pruebas
INSERT INTO access_control.user_role (id_user, id_role)
SELECT 101, 1 WHERE NOT EXISTS (SELECT 1 FROM access_control.user_role WHERE id_user = 101 AND id_role = 1);

INSERT INTO access_control.user_role (id_user, id_role)
SELECT 9, 1 WHERE NOT EXISTS (SELECT 1 FROM access_control.user_role WHERE id_user = 9 AND id_role = 1);

INSERT INTO access_control.user_role (id_user, id_role)
SELECT 16, 1 WHERE NOT EXISTS (SELECT 1 FROM access_control.user_role WHERE id_user = 16 AND id_role = 1);

INSERT INTO access_control.user_role (id_user, id_role)
SELECT 251, 2 WHERE NOT EXISTS (SELECT 1 FROM access_control.user_role WHERE id_user = 251 AND id_role = 2);

INSERT INTO access_control.user_role (id_user, id_role)
SELECT 12, 4 WHERE NOT EXISTS (SELECT 1 FROM access_control.user_role WHERE id_user = 12 AND id_role = 4);

INSERT INTO access_control.user_role (id_user, id_role)
SELECT 237, 3 WHERE NOT EXISTS (SELECT 1 FROM access_control.user_role WHERE id_user = 237 AND id_role = 3);

COMMIT;
