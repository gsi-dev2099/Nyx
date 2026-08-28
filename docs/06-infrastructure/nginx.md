# ISO Header
Código: INF-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Nginx Proxy Security (Capa 2)

El proxy Nginx es la primera línea de defensa (Capa 2). Configura las siguientes cabeceras de seguridad estrictas:
- **Strict-Transport-Security (HSTS):** `max-age=31536000; includeSubDomains; preload`
- **Content-Security-Policy (CSP):** `default-src 'self'`
- **X-Frame-Options:** `DENY`
- **X-Content-Type-Options:** `nosniff`
- **X-XSS-Protection:** `1; mode=block`
