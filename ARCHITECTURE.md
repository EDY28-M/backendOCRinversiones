# Arquitectura del Backend - ORC Inversiones

Backend desarrollado con **ASP.NET Core 8.0** con **Clean Architecture pragmática**, JWT y SQL Server.

## 📁 Estructura Implementada

```
backend/
├── Domain/                  # Entidades del negocio
│   ├── Entities/           (User, Role, Product, Category)
│   └── Enums/              (RoleType)
├── Application/            # Lógica de aplicación
│   ├── DTOs/               (Request/Response separados)
│   ├── Interfaces/         (Repositorios y Servicios)
│   └── Services/           (AuthService, PasswordService)
├── Infrastructure/         # Acceso a datos
│   ├── Data/               (DbContext + Fluent API)
│   └── Repositories/       (Implementaciones)
├── API/Middleware/         (ErrorHandlingMiddleware)
├── Controllers/            (Auth, Users, Roles, Products, Categories)
└── Program.cs              (Configuración DI + JWT)
```

## 🔐 Seguridad

- **JWT Bearer Authentication**
- **BCrypt** para hash de contraseñas
- **Roles**: Administrador (completo), Vendedor (limitado)

## 📡 Endpoints

- `/api/auth/login` - Autenticación
- `/api/users` - CRUD Usuarios (Admin)
- `/api/roles` - CRUD Roles (Admin)
- `/api/categories` - CRUD Categorías (Admin write, All read)
- `/api/products` - CRUD Productos (Admin/Vendedor write, All read)

## 🗄️ Base de Datos

- **SQL Server** con Windows Authentication
- **EF Core 8.0** con migraciones
- **Fluent API** para mapeo explícito
- Seed: Roles predeterminados (Administrador, Vendedor)

## 🚀 Pasos Siguientes

```bash
# 1. Restaurar paquetes
dotnet restore

# 2. Crear migración
dotnet ef migrations add InitialCreate

# 3. Aplicar migración
dotnet ef database update

# 4. Ejecutar
dotnet run

# 5. Swagger: https://localhost:7xxx/swagger
```

## ✅ Características

✔ Clean Architecture sin sobre-ingeniería  
✔ DTOs con validaciones DataAnnotations  
✔ Repository Pattern  
✔ Middleware de manejo de errores  
✔ Swagger con soporte JWT  
✔ Logging estructurado
