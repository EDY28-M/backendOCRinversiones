# Backend ORC Inversiones

API REST para el sistema de gestión de repuestos vehiculares de ORC Inversiones Perú S.A.C. Desarrollado con ASP.NET Core 8.0 y desplegado en Render con Docker.

## Requisitos previos

- .NET SDK 8.0
- SQL Server o Azure SQL Database
- Docker (solo para producción)

## Configuración de la base de datos

La base de datos se llama `ORCInversiones` y se aloja en Azure SQL Database. La cadena de conexión es:

```
Server=tcp:bdinversiones.database.windows.net,1433;Initial Catalog=ORCInversiones;Persist Security Info=False;User ID=orcinversiones;Password=Inversionesperu2026;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Para inicializar las tablas, ejecutar el script `supabase_migration.sql` (ubicado en la raíz del monorepo) desde una conexión directa a la base de datos `ORCInversiones`. No ejecutar desde `master`.

### Tablas del sistema

| Tabla | Descripción |
|-------|-------------|
| Roles | Roles del sistema (Administrador, Vendedor) |
| Users | Usuarios con autenticación por contraseña (BCrypt) |
| Categories | Categorías de productos |
| NombreMarcas | Marcas de vehículos (JAC, Foton, Hyundai, etc.) |
| Products | Catálogo de repuestos con imágenes, códigos y fichas técnicas |

### Usuario administrador por defecto

- **Usuario:** admin
- **Contraseña:** Admin123!
- **Rol:** Administrador

---

## Levantar en local

1. Clonar el repositorio:

```bash
git clone https://github.com/EDY28-M/backendOCRinversiones.git
cd backend
```

2. Restaurar dependencias:

```bash
dotnet restore
```

3. Configurar la cadena de conexión en `appsettings.json` o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:bdinversiones.database.windows.net,1433;Initial Catalog=ORCInversiones;Persist Security Info=False;User ID=orcinversiones;Password=Inversionesperu2026;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

4. Ejecutar la aplicación:

```bash
dotnet run
```

La API estará disponible en `https://localhost:5001` o `http://localhost:5000`.

5. Acceder a la documentación Swagger:

```
http://localhost:5000/swagger
```

---

## Despliegue en producción (Render)

El backend se despliega en Render como servicio Docker. El archivo `render.yaml` en la raíz del monorepo ya tiene la configuración necesaria.

### Pasos

1. Crear un servicio tipo **Web Service** en Render apuntando al repositorio de GitHub.

2. Configurar como **Docker** con las siguientes opciones:
   - Dockerfile Path: `./backend/Dockerfile`
   - Docker Context: `./backend`

3. Configurar las variables de entorno en el dashboard de Render:

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | (cadena de conexión de Azure SQL) |
| `Jwt__Key` | (clave secreta JWT de al menos 64 caracteres) |
| `Jwt__Issuer` | `ORCInversionesAPI` |
| `Jwt__Audience` | `ORCInversionesClient` |
| `CorsOrigins` | `https://orcinversionesperu.com,https://www.orcinversionesperu.com` |

4. Render detecta el Dockerfile y construye la imagen automáticamente al hacer push a `main`.

### Health check

Render usa el endpoint `/api/setup/health` para verificar que el servicio esté activo. Este endpoint también valida la conexión a la base de datos.

---

## Estructura del proyecto

```
backend/
├── Controllers/          # Controladores de la API
├── Domain/
│   ├── Entities/         # Modelos de datos (Product, User, Category, etc.)
│   └── Enums/
├── Application/
│   ├── DTOs/             # Objetos de transferencia (request/response)
│   ├── Interfaces/       # Contratos de repositorios y servicios
│   ├── Services/         # Lógica de negocio
│   ├── Validators/       # Validaciones con FluentValidation
│   └── Mappings/         # Perfiles de AutoMapper
├── Infrastructure/
│   ├── Data/             # DbContext y configuración de EF Core
│   ├── Repositories/     # Implementación de acceso a datos
│   └── HealthChecks/     # Health checks personalizados
├── Migrations/           # Migraciones de Entity Framework
├── Program.cs            # Punto de entrada y configuración de servicios
├── appsettings.json      # Configuración general
└── Dockerfile            # Imagen Docker para producción
```

---

## Referencia de la API

Todas las rutas empiezan con `/api`. Los endpoints marcados con "Auth" requieren token JWT en el header `Authorization: Bearer {token}`.

### Autenticación

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/login` | No | Iniciar sesión. Enviar `{ username, password }`. Devuelve el token JWT. |
| POST | `/api/auth/logout` | No | Cerrar sesión. |
| GET | `/api/auth/ping` | No | Ping para keep-alive del servidor. |

### Productos (admin)

Requieren autenticación. Los endpoints de escritura requieren rol Administrador.

| Método | Ruta | Rol | Descripción |
|--------|------|-----|-------------|
| GET | `/api/products` | Autenticado | Listar todos los productos |
| GET | `/api/products/{id}` | Autenticado | Obtener producto por ID |
| GET | `/api/products/recent` | Autenticado | Últimos productos creados |
| GET | `/api/products/available` | Autenticado | Productos con imágenes, paginados |
| GET | `/api/products/generate-codes` | Autenticado | Generar siguiente código disponible |
| GET | `/api/products/check-codigo/{codigo}` | Autenticado | Verificar si un código está disponible |
| POST | `/api/products` | Administrador | Crear producto |
| PUT | `/api/products/{id}` | Administrador | Actualizar producto |
| PATCH | `/api/products/{id}/status` | Administrador | Activar/desactivar producto |
| PATCH | `/api/products/{id}/featured` | Administrador | Marcar como destacado |
| DELETE | `/api/products/{id}` | Administrador | Eliminar producto |
| DELETE | `/api/products/delete-all` | Administrador | Eliminar todos los productos |
| POST | `/api/products/bulk-import` | Administrador | Importación masiva desde Excel |

### Productos (público)

No requieren autenticación. Son los que consume el frontend público.

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/products/public/active` | Productos activos con imágenes, paginados. Acepta filtros por categoría, marca y búsqueda. |
| GET | `/api/products/public/featured` | Productos destacados para mostrar en la página de inicio. |
| GET | `/api/products/public/brands` | Marcas que tienen al menos un producto activo con imagen. |
| GET | `/api/products/public/categories` | Categorías con conteo de productos activos. |

### Categorías

Requieren autenticación. Escritura solo para Administrador.

| Método | Ruta | Rol | Descripción |
|--------|------|-----|-------------|
| GET | `/api/categories` | Autenticado | Listar categorías |
| GET | `/api/categories/{id}` | Autenticado | Obtener categoría por ID |
| POST | `/api/categories` | Administrador | Crear categoría |
| PUT | `/api/categories/{id}` | Administrador | Actualizar categoría |
| DELETE | `/api/categories/{id}` | Administrador | Eliminar categoría |
| DELETE | `/api/categories/delete-all` | Administrador | Eliminar todas |

### Marcas

Requieren autenticación. Escritura solo para Administrador.

| Método | Ruta | Rol | Descripción |
|--------|------|-----|-------------|
| GET | `/api/nombremarcas` | Autenticado | Listar marcas |
| GET | `/api/nombremarcas/{id}` | Autenticado | Obtener marca por ID |
| POST | `/api/nombremarcas` | Administrador | Crear marca |
| PUT | `/api/nombremarcas/{id}` | Administrador | Actualizar marca |
| DELETE | `/api/nombremarcas/{id}` | Administrador | Eliminar marca |
| DELETE | `/api/nombremarcas/delete-all` | Administrador | Eliminar todas |

### Usuarios

Solo accesible para Administradores.

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/users` | Listar usuarios |
| GET | `/api/users/{id}` | Obtener usuario por ID |
| POST | `/api/users` | Crear usuario |
| PUT | `/api/users/{id}` | Actualizar usuario |
| DELETE | `/api/users/{id}` | Eliminar usuario |

### Roles

Solo accesible para Administradores.

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/roles` | Listar roles |
| GET | `/api/roles/{id}` | Obtener rol por ID |
| POST | `/api/roles` | Crear rol |
| PUT | `/api/roles/{id}` | Actualizar rol |
| DELETE | `/api/roles/{id}` | Eliminar rol |

### Contacto

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/contact` | No | Enviar formulario de contacto por email |

### Setup y monitoreo

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/setup/health` | No | Health check (estado del servidor y conexión a BD) |
| GET | `/api/setup/fix-admin` | No | Resetear contraseña del admin a `Admin123!` |
| GET | `/health` | No | Health check general |
| GET | `/health/ready` | No | Health check con validación de BD |
| GET | `/health/live` | No | Liveness check |

---

## Flujo de autenticación

1. El cliente envía `POST /api/auth/login` con `{ "username": "admin", "password": "Admin123!" }`.
2. Si las credenciales son correctas, el servidor responde con un token JWT.
3. Para los siguientes requests protegidos, el cliente envía el token en el header:
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
   ```
4. El token expira en 24 horas (configurable en `appsettings.json` bajo `Jwt:ExpirationHours`).

---

## Roles del sistema

| ID | Nombre | Permisos |
|----|--------|----------|
| 1 | Administrador | Acceso total: gestión de productos, categorías, marcas, usuarios y roles |
| 2 | Vendedor | Acceso de lectura a productos, categorías y marcas |

---

## Notas técnicas

- **Rate limiting:** Configurado para proteger contra abuso. 200 requests generales por ventana, 20 para login, 10 para importación masiva.
- **Caché en memoria:** Las categorías y marcas se cachean por 2 horas para reducir consultas a la base de datos.
- **Compresión:** Las respuestas se comprimen con Brotli y GZIP.
- **Logging:** Se usa Serilog con salida a consola y archivos en la carpeta `logs/`.
- **Swagger:** Disponible en `/swagger` en todos los ambientes para documentación interactiva de la API.
