# Guía de Inicio - Backend ORC Inversiones

## ✅ Estado del Proyecto

El backend está **completamente implementado** y compilando correctamente. Todas las capas de Clean Architecture están en su lugar.

## 📋 Checklist de Implementación

### Arquitectura ✅
- [x] Domain Layer (Entidades + Enums)
- [x] Application Layer (DTOs + Interfaces + Services)
- [x] Infrastructure Layer (DbContext + Repositories + Configurations)
- [x] API Layer (Controllers + Middleware)

### Funcionalidades ✅
- [x] Autenticación JWT
- [x] Hash de contraseñas con BCrypt
- [x] CRUD Usuarios con roles
- [x] CRUD Roles
- [x] CRUD Productos
- [x] CRUD Categorías
- [x] Validaciones DataAnnotations
- [x] Middleware de manejo de errores
- [x] Swagger con JWT

## 🚀 Pasos para Ejecutar

### 1. Verificar Prerequisitos
```bash
# Verificar .NET instalado
dotnet --version
# Debe ser 8.0 o superior

# Verificar SQL Server corriendo
# Servicios de Windows → SQL Server (MSSQLSERVER) → Estado: En ejecución
```

### 2. Configurar Base de Datos

**Opción A: Usar la configuración por defecto**
```bash
# La aplicación usa Windows Authentication por defecto
# Server=localhost;Database=ORCInversiones;Integrated Security=true;TrustServerCertificate=true;
```

**Opción B: Personalizar connection string**

Editar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "TU_CONNECTION_STRING_AQUI"
  }
}
```

### 3. Crear Base de Datos

```bash
# Instalar herramientas EF Core (si no las tienes)
dotnet tool install --global dotnet-ef

# Crear migración inicial
dotnet ef migrations add InitialCreate

# Aplicar migración a la base de datos
dotnet ef database update
```

**Nota**: La migración creará automáticamente:
- Tablas: Users, Roles, Products, Categories
- Roles predeterminados: Administrador (ID=1), Vendedor (ID=2)
- Índices únicos en Username, Email, Category.Name

### 4. Ejecutar Aplicación

```bash
# Compilar (ya está hecho)
dotnet build

# Ejecutar
dotnet run
```

**Salida esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7xxx
      Now listening on: http://localhost:5xxx
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 5. Acceder a Swagger

Abrir navegador en: `https://localhost:7xxx/swagger`

## 🔑 Crear Usuario Administrador

### Opción 1: Desde Swagger (Recomendado)

1. Ir a `https://localhost:7xxx/swagger`
2. Expandir `POST /api/users`
3. Click en "Try it out"
4. Usar este JSON:

```json
{
  "username": "admin",
  "email": "admin@orcinversiones.com",
  "password": "Admin123!",
  "roleId": 1
}
```

5. Ejecutar
6. **Nota**: Este endpoint fallará porque requiere autenticación. Ver Opción 2.

### Opción 2: Modificar DatabaseSeeder (Método temporal)

Como no tenemos un usuario inicial, podemos crear uno temporalmente modificando Program.cs:

**Agregar al final de Program.cs (antes de `app.Run()`):**

```csharp
// Seed temporal - Crear usuario admin si no existe
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    
    if (!context.Users.Any())
    {
        context.Users.Add(new User
        {
            Username = "admin",
            Email = "admin@orcinversiones.com",
            PasswordHash = passwordService.HashPassword("Admin123!"),
            RoleId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }
}
```

### Opción 3: Insertar Directamente en SQL

```sql
USE ORCInversiones;

-- Verificar que los roles existen
SELECT * FROM Roles;

-- Insertar usuario admin
-- Hash de 'Admin123!' usando BCrypt
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, CreatedAt)
VALUES (
    'admin',
    'admin@orcinversiones.com',
    '$2a$11$YourBCryptHashHere',  -- Necesitas generar el hash
    1,  -- Administrador
    1,  -- IsActive = true
    GETDATE()
);
```

## 🔐 Probar Autenticación

### 1. Login

**Endpoint:** `POST /api/auth/login`

**Body:**
```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

**Respuesta esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "email": "admin@orcinversiones.com",
  "role": "Administrador",
  "expiresAt": "2026-01-23T21:00:00Z"
}
```

### 2. Usar Token en Swagger

1. Copiar el valor de `token`
2. Click en botón "Authorize" 🔓 (arriba a la derecha)
3. Ingresar: `Bearer {tu-token-aqui}`
4. Click "Authorize"
5. Ahora puedes usar todos los endpoints protegidos

## 📡 Probar Endpoints

### Crear Categoría
```
POST /api/categories
Authorization: Bearer {token}

{
  "name": "Electrónica",
  "description": "Productos electrónicos"
}
```

### Crear Producto
```
POST /api/products
Authorization: Bearer {token}

{
  "name": "Laptop Dell XPS 15",
  "description": "Laptop de alto rendimiento",
  "price": 1299.99,
  "stock": 10,
  "categoryId": 1
}
```

### Crear Usuario Vendedor
```
POST /api/users
Authorization: Bearer {token}

{
  "username": "vendedor1",
  "email": "vendedor@orcinversiones.com",
  "password": "Vendedor123!",
  "roleId": 2
}
```

## 📊 Estructura de Base de Datos Creada

```sql
-- Tabla Roles
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Tabla Users
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    RoleId INT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Tabla Categories
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Tabla Products
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CategoryId INT NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

## 🛠️ Comandos Útiles

```bash
# Ver migraciones
dotnet ef migrations list

# Revertir última migración
dotnet ef migrations remove

# Regenerar base de datos (CUIDADO: Elimina datos)
dotnet ef database drop
dotnet ef database update

# Ver logs en tiempo real
dotnet run --verbosity detailed

# Compilar en modo Release
dotnet build -c Release

# Publicar para producción
dotnet publish -c Release -o ./publish
```

## ❗ Solución de Problemas

### Error: "Cannot connect to SQL Server"
**Solución:**
1. Verificar que SQL Server esté corriendo
2. Revisar connection string en `appsettings.json`
3. Si usas instancia nombrada: `Server=localhost\\SQLEXPRESS;...`

### Error: "The server was not found or was not accessible"
**Solución:**
1. Abrir SQL Server Configuration Manager
2. Habilitar TCP/IP en protocolos
3. Reiniciar servicio SQL Server

### Error: "401 Unauthorized" en Swagger
**Solución:**
1. Hacer login en `/api/auth/login`
2. Copiar token de la respuesta
3. Click en "Authorize" y pegar: `Bearer {token}`

### Error: "The type initializer for 'BCrypt.Net.BCrypt' threw an exception"
**Solución:** Ya está incluido BCrypt.Net-Next 4.0.3 en el proyecto.

### Error al crear migración: "No DbContext was found"
**Solución:**
```bash
dotnet ef migrations add InitialCreate --project . --startup-project .
```

## 📚 Próximos Pasos Recomendados

1. **Crear usuario admin** (Opción 2 o 3)
2. **Probar login** y obtener token
3. **Crear categorías** de productos
4. **Crear productos** asociados a categorías
5. **Crear usuarios** vendedores
6. **Probar autorización** (vendedor no puede eliminar productos)

## 🔗 Enlaces Útiles

- **Swagger UI**: `https://localhost:7xxx/swagger`
- **Health Check**: `https://localhost:7xxx/api/health` (si se implementa)
- **Documentación**: Ver `ARCHITECTURE.md` y `README.md`

## 💡 Consejos

- Usa **Postman** o **Thunder Client** (VS Code) como alternativa a Swagger
- Los tokens JWT expiran en **24 horas**
- Las contraseñas deben tener al menos **6 caracteres**
- Los **usernames y emails** deben ser únicos
- Los **roles predeterminados** (1=Admin, 2=Vendedor) no se pueden eliminar

---

## ✅ Verificación Final

```bash
# 1. ¿El proyecto compila?
dotnet build
# ✅ Debe mostrar: "Compilación realizado correctamente"

# 2. ¿Se pueden crear migraciones?
dotnet ef migrations add Test
dotnet ef migrations remove
# ✅ No debe mostrar errores

# 3. ¿El proyecto arranca?
dotnet run
# ✅ Debe mostrar "Now listening on: https://localhost:xxxx"
```

---

**¡El backend está listo para usarse!** 🎉
