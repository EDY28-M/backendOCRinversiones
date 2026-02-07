# 🔧 Solución de Problemas JWT

## ✅ Estado Actual

**Los paquetes JWT YA ESTÁN INSTALADOS correctamente:**
- ✅ Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
- ✅ System.IdentityModel.Tokens.Jwt 8.0.0

**Las claves JWT ya fueron generadas y configuradas en appsettings.json**

---

## 🚨 Problemas Comunes y Soluciones

### Problema 1: "Cannot find package Microsoft.AspNetCore.Authentication.JwtBearer"

**Solución:**
```bash
# Limpiar y restaurar
dotnet clean
dotnet restore

# Si persiste, reinstalar:
dotnet remove package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
```

---

### Problema 2: "JWT Key is too short" o "Key must be at least 32 characters"

**Solución:**
Ya actualizado en `appsettings.json` con una clave de 64 caracteres segura.

**Verificar:**
```bash
# Ver contenido de appsettings.json
cat appsettings.json
```

Debe contener:
```json
"Jwt": {
  "Key": "B!SCbzDrUu5Ce3|PX(@mvrZg}!Q_NhkQ59HHexB?QNO|yD{}t41N@az2$ZZaHLF,",
  "Issuer": "ORCInversionesAPI",
  "Audience": "ORCInversionesClient",
  "ExpirationHours": "24"
}
```

---

### Problema 3: "Error al compilar Program.cs con JWT"

**Verificar que Program.cs tiene estas líneas:**
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// ...

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});
```

---

### Problema 4: "Could not load file or assembly 'System.IdentityModel.Tokens.Jwt'"

**Solución:**
```bash
# Reinstalar el paquete
dotnet remove package System.IdentityModel.Tokens.Jwt
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.0.0

# Restaurar
dotnet restore
dotnet build
```

---

### Problema 5: "AuthService no genera tokens"

**Verificar AuthService.cs tiene este método:**
```csharp
public string GenerateJwtToken(int userId, string username, string roleName)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, username),
        new Claim(ClaimTypes.Role, roleName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

---

### Problema 6: "401 Unauthorized" al llamar endpoints protegidos

**Causa:** No estás enviando el token o está mal formateado.

**Solución:**
1. Hacer login en `/api/auth/login`
2. Copiar el `token` de la respuesta
3. En Swagger:
   - Click en "Authorize" 🔓
   - Escribir: `Bearer {token-aqui}`
   - Click "Authorize"

**Ejemplo con curl:**
```bash
curl -X GET https://localhost:7xxx/api/users \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### Problema 7: "IDX10503: Signature validation failed. Keys tried: 'Microsoft.IdentityModel.Tokens.SymmetricSecurityKey'"

**Causa:** La clave JWT en appsettings.json no coincide con la usada para firmar el token.

**Solución:**
1. Asegúrate de usar la misma clave en appsettings.json y appsettings.Development.json
2. Reinicia la aplicación después de cambiar la clave
3. Genera un nuevo token con login

---

## 🧪 Verificación Rápida

### 1. Verificar paquetes instalados
```bash
dotnet list package
```

Debe mostrar:
```
Microsoft.AspNetCore.Authentication.JwtBearer   8.0.0
System.IdentityModel.Tokens.Jwt                 8.0.0
```

### 2. Verificar configuración
```bash
cat appsettings.json | Select-String "Jwt" -Context 0,6
```

### 3. Compilar proyecto
```bash
dotnet build
```

Debe compilar sin errores.

### 4. Ejecutar proyecto
```bash
dotnet run
```

Debe iniciar sin errores.

### 5. Probar login
```bash
curl -X POST https://localhost:7xxx/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

---

## 🔑 Generar Nueva Clave JWT (Opcional)

Si quieres cambiar la clave JWT:

**Opción 1: PowerShell**
```powershell
# Generar clave aleatoria de 64 caracteres
$chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-="
-join ((1..64) | ForEach-Object { $chars[(Get-Random -Maximum $chars.Length)] })
```

**Opción 2: Online**
- Ir a: https://generate-random.org/api-token-generator
- Generar token de 64 caracteres
- Copiar y pegar en appsettings.json

**Opción 3: Usando el generador incluido**
```bash
dotnet run JwtKeyGenerator.cs
```

---

## 📋 Checklist de Verificación

- [ ] Paquetes JWT instalados (verificar con `dotnet list package`)
- [ ] appsettings.json tiene sección "Jwt" con Key de al menos 32 caracteres
- [ ] Program.cs tiene configuración de Authentication y Authorization
- [ ] AuthService.cs tiene método GenerateJwtToken
- [ ] Proyecto compila sin errores (`dotnet build`)
- [ ] Aplicación ejecuta sin errores (`dotnet run`)
- [ ] Endpoint /api/auth/login responde correctamente

---

## 🆘 Si Nada Funciona

**Reset completo:**
```bash
# 1. Limpiar todo
dotnet clean
Remove-Item -Recurse -Force bin, obj

# 2. Restaurar paquetes
dotnet restore

# 3. Verificar appsettings.json
notepad appsettings.json
# Verificar que la clave JWT esté presente

# 4. Compilar
dotnet build

# 5. Ejecutar
dotnet run
```

---

## 📞 Información Adicional

**Archivos clave para JWT:**
- `appsettings.json` - Configuración de JWT
- `Program.cs` - Configuración de Authentication
- `Application/Services/AuthService.cs` - Generación de tokens
- `Controllers/AuthController.cs` - Endpoint de login

**Logs útiles:**
```bash
# Ejecutar con logs detallados
dotnet run --verbosity detailed
```

---

**Si el problema persiste, comparte:**
1. El error exacto completo
2. El comando que ejecutaste
3. El contenido de tu appsettings.json (sin la clave JWT completa)
