# Comandos Rápidos - Backend ORC Inversiones

## 🚀 Setup Inicial (Solo una vez)

```bash
# 1. Restaurar paquetes
dotnet restore

# 2. Instalar EF Core Tools (si no las tienes)
dotnet tool install --global dotnet-ef

# 3. Crear migración inicial
dotnet ef migrations add InitialCreate

# 4. Aplicar migración (crear BD)
dotnet ef database update
```

---

## ▶️ Ejecutar Aplicación

```bash
# Modo normal
dotnet run

# Con auto-reload (desarrollo)
dotnet watch run

# Modo producción
dotnet run --configuration Release
```

**URLs:**
- Swagger: `https://localhost:7xxx/swagger`
- HTTP: `http://localhost:5xxx`

---

## 🗄️ Comandos de Base de Datos

```bash
# Ver migraciones
dotnet ef migrations list

# Crear nueva migración
dotnet ef migrations add <NombreMigracion>

# Aplicar migraciones pendientes
dotnet ef database update

# Revertir a migración específica
dotnet ef database update <NombreMigracion>

# Eliminar última migración (sin aplicar)
dotnet ef migrations remove

# Eliminar base de datos completa (CUIDADO)
dotnet ef database drop

# Recrear BD desde cero
dotnet ef database drop --force
dotnet ef database update

# Generar script SQL
dotnet ef migrations script
dotnet ef migrations script --output migration.sql
```

---

## 🔨 Compilación y Limpieza

```bash
# Compilar
dotnet build

# Compilar en Release
dotnet build -c Release

# Limpiar artefactos
dotnet clean

# Restaurar + Limpiar + Compilar
dotnet clean && dotnet restore && dotnet build

# Publicar para producción
dotnet publish -c Release -o ./publish
```

---

## 🔍 Diagnóstico

```bash
# Ver información del proyecto
dotnet --info

# Ver versión de .NET
dotnet --version

# Ver SDK instalados
dotnet --list-sdks

# Verificar configuración del proyecto
dotnet build --verbosity detailed

# Ver paquetes instalados
dotnet list package

# Verificar actualizaciones de paquetes
dotnet list package --outdated
```

---

## 📦 Gestión de Paquetes

```bash
# Agregar paquete
dotnet add package <PackageName>

# Agregar versión específica
dotnet add package <PackageName> --version 8.0.0

# Remover paquete
dotnet remove package <PackageName>

# Actualizar paquete
dotnet add package <PackageName> --version <NewVersion>

# Restaurar paquetes
dotnet restore
```

---

## 🧪 Testing (Cuando se implemente)

```bash
# Ejecutar tests
dotnet test

# Con cobertura
dotnet test /p:CollectCoverage=true

# Filtrar por nombre
dotnet test --filter "FullyQualifiedName~ProductsController"

# Con output detallado
dotnet test --verbosity normal
```

---

## 🔐 Usuario Admin Rápido (SQL)

```sql
USE ORCInversiones;

-- Verificar roles
SELECT * FROM Roles;

-- Insertar admin
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, CreatedAt)
VALUES (
    'admin',
    'admin@orcinversiones.com',
    '$2a$11$xJKVqYXN5YHJfGJdKk5h5.N1xKzO9QqQX8Z3rQK5sX6Z8K9vQr5YW',
    1,
    1,
    GETDATE()
);

-- Verificar
SELECT u.Id, u.Username, u.Email, r.Name as Role
FROM Users u
JOIN Roles r ON u.RoleId = r.Id;
```

**Credenciales del hash:**
- Username: `admin`
- Password: `Admin123`

---

## 📡 Testing con cURL

### Login
```bash
curl -X POST https://localhost:7xxx/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

### Crear Categoría
```bash
curl -X POST https://localhost:7xxx/api/categories \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{"name":"Electrónica","description":"Productos electrónicos"}'
```

### Listar Productos
```bash
curl -X GET https://localhost:7xxx/api/products \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## 🐛 Debugging

```bash
# Ejecutar con logs detallados
dotnet run --verbosity detailed

# Variables de entorno para logging
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# Ver logs de EF Core
$env:Logging__LogLevel__Microsoft.EntityFrameworkCore="Information"
dotnet run
```

---

## 🔄 Git (Opcional)

```bash
# Inicializar repo
git init

# Agregar .gitignore
dotnet new gitignore

# Primer commit
git add .
git commit -m "Initial commit - Clean Architecture Backend"

# Agregar remoto
git remote add origin <URL>
git push -u origin main
```

---

## 🚨 Solución Rápida de Problemas

### Error: "dotnet ef not found"
```bash
dotnet tool install --global dotnet-ef
```

### Error: "Cannot connect to SQL Server"
```bash
# Verificar servicio
net start MSSQLSERVER

# O usar SQL Server Management Studio
```

### Error: "Port already in use"
```bash
# Cambiar puerto en launchSettings.json
# O matar proceso:
netstat -ano | findstr :7000
taskkill /PID <process_id> /F
```

### Limpiar todo y empezar de nuevo
```bash
# 1. Limpiar proyecto
dotnet clean
Remove-Item -Recurse -Force bin, obj

# 2. Eliminar BD
dotnet ef database drop --force

# 3. Eliminar migraciones
Remove-Item -Recurse -Force Infrastructure/Data/Migrations

# 4. Empezar de nuevo
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

---

## ⚙️ Variables de Entorno

```powershell
# Configurar entorno de desarrollo
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Configurar producción
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Ver configuración actual
$env:ASPNETCORE_ENVIRONMENT
```

---

## 📊 Conexión a SQL Server

### Windows Authentication (Por defecto)
```json
"Server=localhost;Database=ORCInversiones;Integrated Security=true;TrustServerCertificate=true;"
```

### SQL Authentication
```json
"Server=localhost;Database=ORCInversiones;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
```

### SQL Express
```json
"Server=localhost\\SQLEXPRESS;Database=ORCInversiones;Integrated Security=true;TrustServerCertificate=true;"
```

---

## 🎯 Comandos del Día a Día

```bash
# Iniciar trabajo
git pull
dotnet restore
dotnet build
dotnet run

# Durante desarrollo
# (Terminal 1) - Auto-reload
dotnet watch run

# (Terminal 2) - Migraciones si cambias entidades
dotnet ef migrations add <NombreCambio>
dotnet ef database update

# Finalizar día
git add .
git commit -m "feat: descripción de cambios"
git push
```

---

## 🔑 Shortcuts Útiles

**En Swagger:**
- `Ctrl + /` → Buscar endpoint
- Click "Authorize" → Agregar token JWT
- "Try it out" → Ejecutar endpoint

**En VS Code:**
- `Ctrl + Shift + B` → Build
- `F5` → Debug
- `Ctrl + C` → Stop server

**En Visual Studio:**
- `Ctrl + F5` → Run without debugging
- `F5` → Run with debugging
- `Ctrl + Shift + B` → Build

---

<div align="center">

**💡 Tip:** Guarda este archivo para referencia rápida

</div>
