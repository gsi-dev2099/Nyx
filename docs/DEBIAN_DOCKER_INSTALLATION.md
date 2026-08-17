# 🐧 Guía de Solución de Espacio de Disco (Symlink /srv) en Debian

Debian crea la partición `/var` de 2.7GB por separado, lo que provoca el error `no space left on device` cuando `containerd` e imágenes de Docker escriben en `/var/lib/containerd` y `/var/lib/docker`.

---

## 🛠️ Solución Definitiva: Redirigir /var/lib/docker y /var/lib/containerd a /srv (37GB Libres)

Ejecuta este bloque en la terminal del servidor Debian (`root@debian`):

```bash
# 1. Detener Docker y Containerd
sudo systemctl stop docker containerd

# 2. Eliminar daemon.json anterior si existe
sudo rm -f /etc/docker/daemon.json

# 3. Crear carpetas reales en la partición grande /srv
sudo mkdir -p /srv/var_docker /srv/var_containerd

# 4. Limpiar carpetas antiguas de /var/lib
sudo rm -rf /var/lib/docker /var/lib/containerd

# 5. Crear Enlaces Simbólicos (Symlinks) apuntando a /srv
sudo ln -s /srv/var_docker /var/lib/docker
sudo ln -s /srv/var_containerd /var/lib/containerd

# 6. Reiniciar Containerd y Docker
sudo systemctl start containerd
sudo systemctl start docker

# 7. Verificar que el espacio disponible en /srv sea utilizado
df -h /srv
```
