# Script de Reinstalación de Paquetes JWT

# Ejecuta estos comandos en orden:

# 1. Limpiar proyecto
dotnet clean

# 2. Restaurar paquetes
dotnet restore

# 3. Si persiste el error, reinstalar JWT manualmente:
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.0.0

# 4. Compilar
dotnet build
