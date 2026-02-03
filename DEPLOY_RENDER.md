# 🚀 Despliegue en Render (Backend .NET)

Este proyecto está configurado para desplegarse en Render usando **Docker**.

## 1. Configuración en Render

1.  Crea un **New Web Service**.
2.  Conecta tu repositorio de GitHub.
3.  **Configuración del Servicio:**
    *   **Name:** `backendocrinversiones` (debe coincidir con render.yaml)
    *   **Region:** Oregon (o la más cercana a tus usuarios).
    *   **Runtime:** **Docker**.
    *   **Dockerfile Path:** `./backend/Dockerfile`
    *   **Docker Context:** `./backend`

## 2. ⚠️ IMPORTANTE: Variables de Entorno

**EL ERROR 500 OCURRE PORQUE LA BASE DE DATOS NO ES ACCESIBLE.**

Debes configurar estas variables en el **Dashboard de Render > Environment**:

| Variable | Valor | Descripción |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Ambiente de producción |
| `ASPNETCORE_URLS` | `http://+:8080` | Puerto del contenedor |
| `ConnectionStrings__DefaultConnection` | `Server=TU_SERVER;Database=TU_DB;User Id=TU_USER;Password=TU_PASSWORD;TrustServerCertificate=true;` | **⚠️ CRÍTICO: Debe ser un SQL Server accesible desde internet** |
| `Jwt__Key` | `B!SCbzDrUu5Ce3|PX(@mvrZg}!Q_NhkQ59HHexB?QNO|yD{}t41N@az2$ZZaHLF,` | Clave JWT (mínimo 32 caracteres) |
| `Jwt__Issuer` | `ORCInversionesAPI` | Emisor del token |
| `Jwt__Audience` | `ORCInversionesClient` | Audiencia del token |
| `CorsOrigins` | `https://frontedocrinversiones.onrender.com` | URL del frontend permitida |

## 3. 🗄️ Base de Datos - SOLUCIONES

El servidor `sql.bsite.net` es un hosting GRATUITO y puede tener:
- Restricciones de firewall (no permite conexiones desde Render)
- Límites de conexiones simultáneas
- Tiempos de respuesta lentos

### Opciones recomendadas:

#### Opción A: Azure SQL Database (Recomendada)
1. Crea una base de datos en [Azure Portal](https://portal.azure.com)
2. Habilita "Allow Azure services" en el firewall
3. Usa la cadena de conexión proporcionada por Azure

#### Opción B: Usar Supabase (PostgreSQL) - Requiere migración
1. Crea proyecto en [Supabase](https://supabase.com)
2. Migra el esquema de SQL Server a PostgreSQL
3. Cambia `UseSqlServer` por `UseNpgsql` en Program.cs

#### Opción C: ElephantSQL u otro PostgreSQL gratuito
Similar a Opción B

#### Opción D: SQL Server en la nube
- [db4free.net](https://www.db4free.net/) (MySQL gratuito)
- [Aiven](https://aiven.io/) (trial gratuito)

## 4. Health Check

El endpoint `/api/setup/health` verifica:
- Estado del servidor
- Conexión a la base de datos

Visita: `https://backendocrinversiones.onrender.com/api/setup/health`

## 5. Verificar Despliegue

1. Revisa los logs en Render Dashboard
2. Visita `/swagger` para ver la documentación de la API
3. Visita `/api/setup/health` para verificar el estado

## 6. Troubleshooting

### Error 500 en login:
```
"Error de conexión a la base de datos"
```
**Causa:** El servidor SQL no es accesible desde Render.
**Solución:** Configura `ConnectionStrings__DefaultConnection` con un servidor SQL accesible.

### Error de CORS:
**Causa:** El frontend no está en la lista de orígenes permitidos.
**Solución:** Agrega la URL del frontend en `CorsOrigins`.

---
**Nota:** El Dockerfile utiliza una imagen "Multi-stage build" para mantener el contenedor ligero y seguro.
