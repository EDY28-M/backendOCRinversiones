# 🚀 Despliegue en Render (Backend .NET)

Este proyecto está configurado para desplegarse en Render usando **Docker**.

## 1. Configuración en Render

1.  Crea un **New Web Service**.
2.  Conecta tu repositorio de GitHub.
3.  **Configuración del Servicio:**
    *   **Name:** `backend-orc` (o el nombre que prefieras)
    *   **Region:** La más cercana a tus usuarios (ej. Oregon / Ohio).
    *   **Runtime:** **Docker**.
    *   **Build Command:** (Déjalo vacío, usará el Dockerfile).
    *   **Start Command:** (Déjalo vacío, usará el ENTRYPOINT del Dockerfile).

## 2. Variables de Entorno (Environment Variables)

Para que la aplicación funcione en producción, DEBES agregar las siguientes variables en la pestaña **Environment** de Render:

| Clave | Valor Recomendado / Descripción |
| :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:8080` (Render usa el puerto 10000 interno por defecto pero 8080 es estándar para contenedores .NET) |
| `ConnectionStrings__DefaultConnection` | **Tu cadena de conexión a SQL Server**. <br>Nota: Asegúrate de que tu base de datos permita conexiones externas o esté en la misma red. |
| `Jwt__Key` | `wGToCQBmp1KVBRm59nKjn6YngYaSOOcb0/qhr6Mpi57KK2nkMl0DIopjpvO` (Generada para ti) |
| `Jwt__Issuer` | `ORCInversionesAPI` (o lo que configures) |
| `Jwt__Audience` | `ORCInversionesClient` (o lo que configures) |
| `AllowedHosts` | `*` (O el dominio de tu frontend si quieres restringirlo) |
| `FrontedUrl` | La URL de tu frontend en producción (ej. `https://mi-frontend.onrender.com`). Usada para CORS. |

### ⚠️ Importante sobre CORS en Producción

En `appsettings.json` o en las variables de entorno, asegúrate de sobreescribir la configuración de CORS para que acepte tu frontend de producción.

Puedes agregar una variable llamada `CorsOrigins` con la URL de tu frontend:
`CorsOrigins` = `https://tu-app-frontend.onrender.com`

(Nota: El código actual en `Program.cs` puede necesitar una pequeña modificación para leer esta variable si no es automática).

## 3. Base de Datos en la Nube

Como estás usando **SQL Server**, necesitas una instancia accesible desde internet.
*   **Opción A (Recomendada):** Usar un servicio gestionado como **Azure SQL Database** o **AWS RDS**.
*   **Opción B:** Si usas una base de datos local en tu PC, Render **NO** podrá acceder a ella a menos que uses un túnel (como Ngrok), pero no se recomienda para producción constante.
*   **Opción C:** Usar PostgreSQL (Supabase/Neon) si decidieras migrar (el código actual usa SQL Server).

## 4. Health Check

Render verificará que tu servicio esté corriendo.
La aplicación escucha en el puerto `8080`.

---
**Nota:** El Dockerfile utiliza una imagen "Multi-stage build" para mantener el contenedor ligero y seguro.
