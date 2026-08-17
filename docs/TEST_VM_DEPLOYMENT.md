# 🌐 Guía de Despliegue del CRM Nyx en VM de Pruebas (Debian)

Esta guía explica cómo configurar el firewall en la VM Debian, solucionar problemas de espacio de disco y desplegar el CRM con el script de PowerShell `deploy_to_server.ps1`.

---

## 🛡️ Desbloqueo de Puertos en el Firewall de Debian (UFW)

Para permitir el tráfico externo al servidor a través del Reverse Proxy Nginx en el puerto 80/443:

```bash
# 1. Instalar UFW si no está instalado
sudo apt-get install -y ufw

# 2. Permitir SSH (imprescindible para no perder acceso)
sudo ufw allow 22/tcp

# 3. Abrir puertos HTTP (80) y HTTPS (443) para el CRM
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# 4. (Opcional) Abrir puertos directos de aplicaciones si se requiere acceso directo
sudo ufw allow 5068/tcp
sudo ufw allow 5261/tcp
sudo ufw allow 5070/tcp

# 5. Habilitar el Firewall
sudo ufw enable

# 6. Verificar regla de puertos activos
sudo ufw status verbose
```

---

## 🧹 Solución a Error "No space left on device" (Espacio en Disco)

Si el despliegue falla por falta de espacio en disco durante la construcción de Docker:

```bash
# 1. Verificar espacio libre en particiones
df -h

# 2. Limpiar paquetes de apt almacenados en caché
sudo apt-get clean
sudo apt-get autoremove -y

# 3. Limpiar imágenes huérfanas y caché de compilación de Docker
docker system prune -a --volumes -f
```

---

## 🚀 Despliegue desde PowerShell (Windows)

Ejecuta el script `deploy_to_server.ps1` desde la carpeta `CRM_API`:

```powershell
.\deploy_to_server.ps1 -ServerIP "10.10.40.12" -User "root" -RemotePath "/opt/crm_nyx"
```

---

## 🔍 Comandos de Verificación en el Servidor Debian

```bash
cd /opt/crm_nyx

# Ver estado de contenedores
docker compose -f docker-compose.prod.yml ps

# Ver logs de Nginx o la API
docker compose -f docker-compose.prod.yml logs -f nginx
docker compose -f docker-compose.prod.yml logs -f crm_apihub
```
