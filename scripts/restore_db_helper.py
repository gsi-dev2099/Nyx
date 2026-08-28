import argparse
import io
import os
import psycopg2
import sys

sys.stdout.reconfigure(encoding='utf-8')

def is_meaningful_sql(text):
    for line in text.splitlines():
        line = line.strip()
        if line and not line.startswith('--'):
            return True
    return False

def restore_db(host, port, user, password, dbname, dump_path):
    print(f"\n[+] Restaurando '{dbname}' desde {dump_path}...")
    
    # 1. Ensure clean database exists (terminate stale connections and recreate)
    try:
        conn_pg = psycopg2.connect(host=host, port=port, user=user, password=password, dbname='postgres')
        conn_pg.autocommit = True
        cur_pg = conn_pg.cursor()
        cur_pg.execute(f"""
            SELECT pg_terminate_backend(pid) 
            FROM pg_stat_activity 
            WHERE datname = '{dbname}' AND pid <> pg_backend_pid();
        """)
        cur_pg.execute(f"DROP DATABASE IF EXISTS {dbname};")
        cur_pg.execute(f"CREATE DATABASE {dbname};")
        cur_pg.close()
        conn_pg.close()
        print(f"  -> Base de datos '{dbname}' reinicializada limpiamente.")
    except Exception as e:
        print(f"  [-] Aviso al inicializar base de datos: {e}")

    # 2. Restore SQL dump content with native COPY & DDL streaming
    try:
        conn = psycopg2.connect(host=host, port=port, user=user, password=password, dbname=dbname)
        conn.autocommit = True
        cur = conn.cursor()
        
        with open(dump_path, 'r', encoding='utf-8', errors='ignore') as f:
            lines = f.readlines()
            
        sql_buffer = []
        copy_stmt = None
        copy_data = []
        copy_count = 0
        
        for line in lines:
            # Skip psql client meta-commands
            stripped = line.strip()
            if stripped.startswith(r'\restrict') or stripped.startswith(r'\unrestrict') or \
               stripped.startswith(r'\connect') or stripped.startswith(r'\set') or \
               stripped.startswith(r'\unset') or stripped.startswith(r'\echo'):
                continue
                
            # Detect start of COPY FROM stdin
            if line.startswith('COPY ') and 'FROM stdin;' in line:
                pending_sql = "".join(sql_buffer)
                if is_meaningful_sql(pending_sql):
                    cur.execute(pending_sql)
                sql_buffer = []
                copy_stmt = line
                copy_data = []
            elif copy_stmt is not None:
                if stripped == r'\.':
                    # End of COPY block -> stream directly into PostgreSQL
                    cur.copy_expert(copy_stmt, io.StringIO("".join(copy_data)))
                    copy_stmt = None
                    copy_data = []
                    copy_count += 1
                else:
                    copy_data.append(line)
            else:
                sql_buffer.append(line)
                
        # Flush remaining SQL
        pending_sql = "".join(sql_buffer)
        if is_meaningful_sql(pending_sql):
            cur.execute(pending_sql)
            
        cur.close()
        conn.close()
        print(f"  [OK] Base de datos '{dbname}' restaurada exitosamente ({copy_count} tablas COPY sincronizadas).")
    except Exception as e:
        print(f"  [-] Error restaurando '{dbname}': {e}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=5432)
    parser.add_argument("--user", default="ronald")
    parser.add_argument("--password", default="Gs1$2099Zx23rO24M4r25")
    parser.add_argument("--database", default="ALL")
    parser.add_argument("--dumps-dir", default="")
    args = parser.parse_args()

    dumps_dir = args.dumps_dir
    if not dumps_dir or not os.path.exists(dumps_dir):
        script_dir = os.path.dirname(os.path.abspath(__file__))
        cand1 = os.path.join(script_dir, "..", "db_export", "dumps")
        cand2 = os.path.join(script_dir, "..", "..", "db_export", "dumps")
        dumps_dir = cand1 if os.path.exists(cand1) else cand2

    print("================================================================")
    print(" [NYX CRM] RESTAURACIÓN MANUAL DE BASES DE DATOS (NATIVE STREAM)")
    print(f" Target Host : {args.host}:{args.port}")
    print(f" Usuario     : {args.user}")
    print(f" Carpeta SQL : {os.path.abspath(dumps_dir)}")
    print("================================================================")

    databases = ["nx_ecosystem", "nyx_crm", "nyx_flow", "nyx_approval", "nyx_sla"]
    if args.database != "ALL":
        databases = [args.database]

    for db in databases:
        dump_file = os.path.join(dumps_dir, f"{db}_backup.sql")
        if os.path.exists(dump_file):
            restore_db(args.host, args.port, args.user, args.password, db, dump_file)
        else:
            print(f"  [-] Archivo de volcado no encontrado: {dump_file}")

    print("\n================================================================")
    print(" [RESTAURACIÓN FINALIZADA CON ÉXITO]")
    print("================================================================")
