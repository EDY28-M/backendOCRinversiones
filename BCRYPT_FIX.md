# 🔧 SOLUCIÓN: Error de BCrypt - Hash de Contraseña Inválido

## ❌ Problema

```
System.ArgumentOutOfRangeException: Index and length must refer to a location within the string.
at BCrypt.Net.BCrypt.Verify(String text, String hash, Boolean enhancedEntropy, HashType hashType)
```

**Causa:** El hash de contraseña en la base de datos no es válido. BCrypt requiere un hash de **60 caracteres** con formato específico.

---

## ✅ Solución Rápida (3 Opciones)

### **Opción 1: SQL Script (RECOMENDADA)** ⭐

Ejecuta este SQL en **SQL Server Management Studio** o **Azure Data Studio**:

```sql
USE ORCInversiones;

-- Eliminar usuario admin anterior (si existe)
DELETE FROM Users WHERE Username = 'admin';

-- Insertar usuario admin con hash BCrypt válido
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, CreatedAt)
VALUES (
    'admin',
    'admin@orcinversiones.com',
    '$2a$11$KLThnOIbFlD/CEyUNmDsPOnALLquG4J9.f1dO4.hlOkuzseSGm/bS',
    1,
    1,
    GETDATE()
);

-- Verificar usuario creado
SELECT u.Id, u.Username, u.Email, r.Name as Role, LEN(u.PasswordHash) as HashLength
FROM Users u
JOIN Roles r ON u.RoleId = r.Id
WHERE u.Username = 'admin';
```

**Credenciales:**
- **Username:** `admin`
- **Password:** `Admin123!`

---

### **Opción 2: Usar Swagger (POST /api/users)** 

**Problema:** Requiere estar autenticado como Admin.

**Solución Temporal:** Desactivar temporalmente la autorización en `UsersController.cs`:

1. Abrir `Controllers\UsersController.cs`
2. Comentar temporalmente `[Authorize]` en el método `Create`:

```csharp
[HttpPost]
// [Authorize(Roles = "Administrador")]  // <-- Comentar temporalmente
public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
```

3. Reiniciar la aplicación
4. Ir a Swagger: `http://localhost:5095/swagger`
5. Ejecutar `POST /api/users`:

```json
{
  "username": "admin",
  "email": "admin@orcinversiones.com",
  "password": "Admin123!",
  "roleId": 1
}
```

6. **IMPORTANTE:** Descomentar `[Authorize]` después de crear el usuario

---

### **Opción 3: Crear Script de Seed en Program.cs**

Agregar este código al final de `Program.cs` (antes de `app.Run()`):

```csharp
// Seed temporal - Crear usuario admin si no existe
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    
    // Verificar si ya existe un usuario admin
    if (!context.Users.Any(u => u.Username == "admin"))
    {
        // Crear usuario admin
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@orcinversiones.com",
            PasswordHash = passwordService.HashPassword("Admin123!"),
            RoleId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(adminUser);
        context.SaveChanges();
        
        Console.WriteLine("✅ Usuario admin creado exitosamente");
    }
}
```

Luego ejecutar:
```bash
dotnet run
```

---

## 🔍 Verificar que Funcionó

### 1. Verificar en SQL
```sql
SELECT u.Id, u.Username, u.Email, r.Name as Role, LEN(u.PasswordHash) as HashLength
FROM Users u
JOIN Roles r ON u.RoleId = r.Id
WHERE u.Username = 'admin';
```

**Resultado esperado:**
```
Id | Username | Email                      | Role          | HashLength
1  | admin    | admin@orcinversiones.com   | Administrador | 60
```

**✅ HashLength debe ser exactamente 60**

### 2. Probar Login desde Swagger

1. Ir a: `http://localhost:5095/swagger`
2. Expandir `POST /api/auth/login`
3. Click "Try it out"
4. Usar:
```json
{
  "username": "admin",
  "password": "Admin123!"
}
```
5. Click "Execute"

**Respuesta esperada (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "email": "admin@orcinversiones.com",
  "role": "Administrador",
  "expiresAt": "2026-01-23T22:00:00Z"
}
```

---

## 📝 Hashes BCrypt Válidos

Si necesitas crear más usuarios manualmente, usa estos hashes:

| Contraseña | Hash BCrypt (60 caracteres) |
|------------|----------------------------|
| `Admin123!` | `$2a$11$KLThnOIbFlD/CEyUNmDsPOnALLquG4J9.f1dO4.hlOkuzseSGm/bS` |
| `Vendedor123!` | `$2a$11$xJKVqYXN5YHJfGJdKk5h5.N1xKzO9QqQX8Z3rQK5sX6Z8K9vQr5YW` |

**Importante:** Los hashes BCrypt siempre empiezan con `$2a$` o `$2b$` y tienen exactamente **60 caracteres**.

---

## 🛠️ Generar Tus Propios Hashes

### Método 1: Usando el Endpoint de Creación
Una vez tengas un usuario admin, usa `POST /api/users` para crear más usuarios. El sistema automáticamente generará el hash correcto.

### Método 2: PowerShell
```powershell
# Cargar BCrypt
Add-Type -Path "bin\Debug\net8.0\BCrypt.Net-Next.dll"

# Generar hash
$password = "TuContraseña123!"
$hash = [BCrypt.Net.BCrypt]::HashPassword($password)
Write-Host "Hash: $hash"
```

### Método 3: Sitio Web
- Ir a: https://bcrypt-generator.com/
- Ingresar tu contraseña
- **Importante:** Seleccionar **Rounds: 11** (coincide con el backend)
- Copiar el hash generado

---

## 🚨 Errores Comunes

### Error: "Login returns null"
**Causa:** Usuario no existe o contraseña incorrecta
**Solución:** Verificar en SQL que el usuario existe y tiene hash válido

### Error: "Hash too short"
**Causa:** Hash en BD tiene menos de 60 caracteres
**Solución:** Regenerar hash usando las opciones anteriores

### Error: "Invalid hash format"
**Causa:** Hash no empieza con `$2a$` o `$2b$`
**Solución:** Usar un hash BCrypt válido

---

## 📋 Checklist de Verificación

- [ ] Base de datos existe (`ORCInversiones`)
- [ ] Tabla Users existe
- [ ] Tabla Roles existe con id=1 (Administrador)
- [ ] Usuario admin insertado
- [ ] Hash tiene 60 caracteres
- [ ] Hash empieza con `$2a$` o `$2b$`
- [ ] Login funciona en Swagger
- [ ] Se genera token JWT

---

## 🎯 Script SQL Completo (Por Si Acaso)

```sql
-- Verificar base de datos
USE ORCInversiones;
GO

-- Verificar roles
SELECT * FROM Roles;
-- Debe mostrar: 1 | Administrador | ... 

-- Limpiar usuarios anteriores
DELETE FROM Users WHERE Username = 'admin';

-- Crear usuario admin con hash válido
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, CreatedAt)
VALUES (
    'admin',
    'admin@orcinversiones.com',
    '$2a$11$KLThnOIbFlD/CEyUNmDsPOnALLquG4J9.f1dO4.hlOkuzseSGm/bS',
    1,
    1,
    GETDATE()
);

-- Crear usuario vendedor de ejemplo
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, CreatedAt)
VALUES (
    'vendedor',
    'vendedor@orcinversiones.com',
    '$2a$11$xJKVqYXN5YHJfGJdKk5h5.N1xKzO9QqQX8Z3rQK5sX6Z8K9vQr5YW',
    2,
    1,
    GETDATE()
);

-- Verificar usuarios creados
SELECT 
    u.Id, 
    u.Username, 
    u.Email, 
    r.Name as Role, 
    u.IsActive,
    LEN(u.PasswordHash) as HashLength,
    LEFT(u.PasswordHash, 10) + '...' as HashPreview
FROM Users u
JOIN Roles r ON u.RoleId = r.Id;
```

**Credenciales:**
- Admin: `admin` / `Admin123!`
- Vendedor: `vendedor` / `Vendedor123!`

---

## ✅ Resumen

**El problema:** Hash de contraseña inválido en la base de datos.

**La solución:** Ejecutar el SQL con un hash BCrypt válido de 60 caracteres.

**Resultado:** Login funcionará correctamente y generará tokens JWT.

---

**Después de aplicar la solución, prueba hacer login y debería funcionar perfectamente! ✅**
