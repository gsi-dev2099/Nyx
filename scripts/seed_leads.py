import subprocess
import json

def run_psql(db, sql):
    cmd = ["docker", "exec", "-i", "crm_postgres", "psql", "-U", "postgres", "-d", db]
    res = subprocess.run(cmd, input=sql, text=True, capture_output=True)
    if res.returncode != 0:
        print(f"Error executing on {db}: {res.stderr}")
    return res.stdout

def main():
    print("Obteniendo pre-ventas de nyx_crm...")
    out = run_psql("nyx_crm", "SELECT json_agg(id_presale ORDER BY id_presale) FROM lead_service.lead_pre_sale;")
    
    # Parse json array
    lines = [l.strip() for l in out.splitlines() if l.strip().startswith("[")]
    if not lines:
        print("No se pudo obtener la lista de leads:", out)
        return
    
    lead_ids = json.loads(lines[0])
    print(f"Total leads encontrados: {len(lead_ids)}")

    # 1. Limpiar instancias previas en nyx_flow
    lead_ids_str = ",".join(map(str, lead_ids))
    run_psql("nyx_flow", f"DELETE FROM flow_instance WHERE entity_type = 'lead_presale' AND entity_id IN ({lead_ids_str});")
    print("Instancias previas limpiadas en nyx_flow.")

    # 2. Generar SQL para nyx_crm y nyx_flow
    crm_sql_lines = []
    flow_sql_lines = ["BEGIN;"]

    # Categories:
    # 0: 1ra Llamada Sin Asignar
    # 1: 1ra Llamada Asignada (Asesor 101)
    # 2: 2da Llamada (Asesor 101)
    # 3: 3ra Llamada (Asesor 101)
    # 4: Retención / Alerta Cambio (Asesor 101)
    # 5: Venta Creada - Ficha de Venta CP#16 (Etapa 13)
    # 6: Rescate KO - Gestión Botada CP#75 (Etapa 12)
    # 7: Rescate KO - Gestión Alternas CP#76 (Etapa 12)

    categories_count = {
        "1ra_Sin_Asignar": 0,
        "1ra_Asignada": 0,
        "2da_Llamada": 0,
        "3ra_Llamada": 0,
        "Retencion": 0,
        "Venta_Creada": 0,
        "Botada_Rescate": 0,
        "Alternas_Rescate": 0
    }

    for idx, lead_id in enumerate(lead_ids):
        cat = idx % 8

        if cat == 0:
            # 1ra Llamada Sin Asignar
            crm_sql_lines.append(f"""
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = NULL, assigned_advisor_2_id = NULL, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = {lead_id};
            """)
            flow_sql_lines.append(f"""
                DO $$
                DECLARE v_id BIGINT;
                BEGIN
                    INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                    VALUES (2, 'lead_presale', {lead_id}, 12, 'ACTIVE') RETURNING id_instance INTO v_id;

                    INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                    VALUES 
                    (v_id, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 12, 'PENDING', 12, NULL, NULL),
                    (v_id, 13, 'PENDING', 12, NULL, NULL),
                    (v_id, 14, 'PENDING', 12, NULL, NULL),
                    (v_id, 15, 'PENDING', 12, NULL, NULL);
                END $$;
            """)
            categories_count["1ra_Sin_Asignar"] += 1

        elif cat == 1:
            # 1ra Llamada Asignada (Asesor 101)
            crm_sql_lines.append(f"""
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = NULL, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = {lead_id};
            """)
            flow_sql_lines.append(f"""
                DO $$
                DECLARE v_id BIGINT;
                BEGIN
                    INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                    VALUES (2, 'lead_presale', {lead_id}, 12, 'ACTIVE') RETURNING id_instance INTO v_id;

                    INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                    VALUES 
                    (v_id, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 12, 'PENDING', 12, NULL, NULL),
                    (v_id, 13, 'PENDING', 12, NULL, NULL),
                    (v_id, 14, 'PENDING', 12, NULL, NULL),
                    (v_id, 15, 'PENDING', 12, NULL, NULL);
                END $$;
            """)
            categories_count["1ra_Asignada"] += 1

        elif cat == 2:
            # 2da Llamada
            crm_sql_lines.append(f"""
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = {lead_id};
            """)
            flow_sql_lines.append(f"""
                DO $$
                DECLARE v_id BIGINT;
                BEGIN
                    INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                    VALUES (2, 'lead_presale', {lead_id}, 12, 'ACTIVE') RETURNING id_instance INTO v_id;

                    INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                    VALUES 
                    (v_id, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 13, 'PENDING', 12, NULL, NULL),
                    (v_id, 14, 'PENDING', 12, NULL, NULL),
                    (v_id, 15, 'PENDING', 12, NULL, NULL);
                END $$;
            """)
            categories_count["2da_Llamada"] += 1

        elif cat == 3:
            # 3ra Llamada
            crm_sql_lines.append(f"""
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = 101,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = {lead_id};
            """)
            flow_sql_lines.append(f"""
                DO $$
                DECLARE v_id BIGINT;
                BEGIN
                    INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                    VALUES (2, 'lead_presale', {lead_id}, 12, 'ACTIVE') RETURNING id_instance INTO v_id;

                    INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                    VALUES 
                    (v_id, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 13, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 14, 'PENDING', 12, NULL, NULL),
                    (v_id, 15, 'PENDING', 12, NULL, NULL);
                END $$;
            """)
            categories_count["3ra_Llamada"] += 1

        elif cat == 4:
            # Retención / Alerta Cambio
            crm_sql_lines.append(f"""
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = 101,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = {lead_id};
            """)
            flow_sql_lines.append(f"""
                DO $$
                DECLARE v_id BIGINT;
                BEGIN
                    INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                    VALUES (2, 'lead_presale', {lead_id}, 12, 'ACTIVE') RETURNING id_instance INTO v_id;

                    INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                    VALUES 
                    (v_id, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 13, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 14, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 15, 'PENDING', 12, NULL, NULL);
                END $$;
            """)
            categories_count["Retencion"] += 1

        elif cat == 5:
            # Venta Creada - Ficha de Venta CP#16 (Etapa 13)
            crm_sql_lines.append(f"""
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = 101,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = {lead_id};
            """)
            flow_sql_lines.append(f"""
                DO $$
                DECLARE v_id BIGINT;
                BEGIN
                    INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                    VALUES (2, 'lead_presale', {lead_id}, 13, 'ACTIVE') RETURNING id_instance INTO v_id;

                    INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                    VALUES 
                    (v_id, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 13, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 14, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 15, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 16, 'PENDING', 13, NULL, NULL),
                    (v_id, 79, 'PENDING', 13, NULL, NULL),
                    (v_id, 18, 'PENDING', 13, NULL, NULL),
                    (v_id, 77, 'PENDING', 13, NULL, NULL),
                    (v_id, 78, 'PENDING', 13, NULL, NULL),
                    (v_id, 80, 'PENDING', 13, NULL, NULL);
                END $$;
            """)
            categories_count["Venta_Creada"] += 1

        elif cat == 6:
            # Rescate KO: Gestión Botada CP#75 (Etapa 12)
            crm_sql_lines.append(f"""
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = NULL, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = {lead_id};
            """)
            flow_sql_lines.append(f"""
                DO $$
                DECLARE v_id BIGINT;
                BEGIN
                    INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                    VALUES (2, 'lead_presale', {lead_id}, 12, 'ACTIVE') RETURNING id_instance INTO v_id;

                    INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                    VALUES 
                    (v_id, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 12, 'KO', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 75, 'PENDING', 12, NULL, NULL);
                END $$;
            """)
            categories_count["Botada_Rescate"] += 1

        elif cat == 7:
            # Rescate KO: Gestión Alternas CP#76 (Etapa 12)
            crm_sql_lines.append(f"""
                UPDATE lead_service.lead_pre_sale 
                SET id_status = 1, owner_user_id = 101, current_user_id = 101, 
                    assigned_advisor_1_id = 101, assigned_advisor_2_id = 101, assigned_advisor_3_id = NULL,
                    assignment_status = 'NONE', discard_reason = NULL, discarded_at = NULL, discarded_by = NULL
                WHERE id_presale = {lead_id};
            """)
            flow_sql_lines.append(f"""
                DO $$
                DECLARE v_id BIGINT;
                BEGIN
                    INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, status)
                    VALUES (2, 'lead_presale', {lead_id}, 12, 'ACTIVE') RETURNING id_instance INTO v_id;

                    INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, resolved_by, resolved_at)
                    VALUES 
                    (v_id, 11, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 12, 'APPROVED', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 13, 'KO', 12, 101, CURRENT_TIMESTAMP),
                    (v_id, 76, 'PENDING', 12, NULL, NULL);
                END $$;
            """)
            categories_count["Alternas_Rescate"] += 1

    flow_sql_lines.append("COMMIT;")

    print("Actualizando registros en nyx_crm...")
    run_psql("nyx_crm", "\n".join(crm_sql_lines))

    print("Insertando instancias y checkpoints en nyx_flow...")
    run_psql("nyx_flow", "\n".join(flow_sql_lines))

    print("\n" + "="*50)
    print("DISTRIBUCION EXITOSA:")
    for k, v in categories_count.items():
        print(f" - {k}: {v} leads")
    print("="*50)

if __name__ == "__main__":
    main()
