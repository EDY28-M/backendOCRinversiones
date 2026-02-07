# Backend ORC Inversiones - Clean Architecture

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-blue)](https://docs.microsoft.com/en-us/aspnet/core/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-green)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Backend profesional desarrollado con **ASP.NET Core Web API**, implementando **Clean Architecture**, **SOLID principles** y **mejores prácticas de la industria**.

---

## 📋 Características Principales

✅ **Clean Architecture** con separación de capas (Domain, Application, Infrastructure, API)  
✅ **Autenticación JWT** con roles (Administrador, Vendedor)  
✅ **Repository Pattern** + **Unit of Work**  
✅ **Dependency Injection** nativo de .NET  
✅ **AutoMapper** para mapeo de DTOs  
✅ **FluentValidation** para validaciones robustas  
✅ **Entity Framework Core** con SQL Server  
✅ **Manejo centralizado de errores** con middleware personalizado  
✅ **Logging estructurado** con Serilog  
✅ **Swagger/OpenAPI** para documentación de API  
✅ **CORS** configurado  
✅ **Paginación** en endpoints de listado  

---

## 🏗️ Arquitectura

Este proyecto sigue **Clean Architecture** con 4 capas principales:

```
┌─────────────────────────────────────────────────────────┐
│                    API (Presentation)                    │
│              Controllers, Middleware, Filters            │
└───────────────────────────┬─────────────────────────────┘
                            │ ↓ depende de
┌───────────────────────────▼─────────────────────────────┐
│                     Application                          │
│          Services, DTOs, Interfaces, Validators          │
└───────────────────────────┬─────────────────────────────┘
                            │ ↓ depende de
┌───────────────────────────▼─────────────────────────────┐
│                       Domain (Core)                      │
│               Entities, Exceptions, Enums                │
└─────────────────────────────────────────────────────────┘
                            ↑ es usado por
┌───────────────────────────┴─────────────────────────────┐
│                    Infrastructure                        │
│         EF Core, Repositories, External Services         │
└─────────────────────────────────────────────────────────┘
```

**Más detalles**: Ver [`ARCHITECTURE.md`](ARCHITECTURE.md)

---

## 📦 Módulos del Sistema

### 1. Autenticación
- ✅ Login con email/password
- ✅ Generación de token JWT
- ✅ Autorización basada en roles

### 2. Gestión de Usuarios
- ✅ CRUD de usuarios
- ✅ Asignación de roles
- ✅ Cambio de contraseñas
- ✅ Activación/Desactivación (soft delete)

### 3. Gestión de Roles
- ✅ Listado de roles
- ✅ Roles predefinidos: Administrador, Vendedor

### 4. Gestión de Productos
- ✅ CRUD de productos
- ✅ Asignación de vendedores
- ✅ Control de stock
- ✅ Categorización

### 5. Gestión de Categorías
- ✅ CRUD de categorías
- ✅ Relación con productos

---

## 🗂️ Estructura del Proyecto

```
backendORCinverisones/
│
├── src/
│   ├── Domain/                 # Entidades, excepciones de negocio
│   ├── Application/            # Servicios, DTOs, interfaces, validadores
│   ├── Infrastructure/         # EF Core, repositorios, JWT, external services
│   └── API/                    # Controllers, middleware, configuración
│
├── docs/                       # Documentación
│   ├── ARCHITECTURE.md         # Arquitectura del sistema
│   ├── ENTITIES.md             # Diseño de entidades
│   ├── ENDPOINTS.md            # Especificación de API
│   ├── IMPLEMENTATION.md       # Guía de implementación
│   └── BEST_PRACTICES.md       # Mejores prácticas
│
├── backendORCinverisones.sln   # Solución .NET
└── README.md                   # Este archivo
```

---

## 🚀 Inicio Rápido

### Prerrequisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) o [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-editions-express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)

### Instalación

1. **Clonar el repositorio**
```bash
git clone https://github.com/tu-usuario/backendORCinverisones.git
cd backendORCinverisones
```

2. **Restaurar paquetes NuGet**
```bash
dotnet restore
```

3. **Configurar cadena de conexión**

Editar `src/API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BackendORC;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

4. **Aplicar migraciones**
```bash
cd src/API
dotnet ef database update --project ../Infrastructure
```

5. **Ejecutar la aplicación**
```bash
dotnet run
```

6. **Acceder a Swagger**
```
https://localhost:7001/swagger
```

---

## 📚 Documentación

| Documento | Descripción |
|-----------|-------------|
| [**ARCHITECTURE.md**](ARCHITECTURE.md) | Arquitectura del sistema, capas, flujo de datos, patrones de diseño |
| [**ENTITIES.md**](ENTITIES.md) | Diseño de entidades, relaciones, reglas de negocio |
| [**ENDPOINTS.md**](ENDPOINTS.md) | Especificación completa de API REST, autorización por rol |
| [**IMPLEMENTATION.md**](IMPLEMENTATION.md) | Guía paso a paso para implementar el sistema |
| [**BEST_PRACTICES.md**](BEST_PRACTICES.md) | Seguridad, clean code, performance, logging, testing |

---

## 🔐 Autenticación y Autorización

### Autenticación JWT

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@ejemplo.com",
  "password": "Admin123!"
}

# Response
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
      "id": 1,
      "nombre": "Admin",
      "email": "admin@ejemplo.com",
      "rol": "Administrador"
    },
    "expiracion": "2026-01-23T14:00:00Z"
  }
}
```

### Usar Token en Requests

```bash
GET /api/usuarios
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Roles del Sistema

| Rol | Permisos |
|-----|----------|
| **Administrador** | Acceso completo al sistema |
| **Vendedor** | Acceso limitado a productos asignados |

**Más detalles**: Ver [`ENDPOINTS.md`](ENDPOINTS.md#autorización-por-rol)

---

## 🛠️ Tecnologías Utilizadas

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Base de datos
- **AutoMapper** - Mapeo objeto-objeto
- **FluentValidation** - Validaciones
- **BCrypt.Net** - Hashing de contraseñas

### Autenticación
- **JWT (JSON Web Tokens)** - Autenticación stateless

### Documentación
- **Swagger/OpenAPI** - Documentación interactiva de API

### Logging
- **Serilog** - Logging estructurado

### Testing (Futuro)
- **xUnit** - Framework de pruebas
- **Moq** - Mocking

---

## 📊 Modelo de Datos

### Entidades Principales

```
Usuario ──┬── Rol
          │
          └── Producto ──── Categoria
```

**Diagrama ER completo**: Ver [`ENTITIES.md`](ENTITIES.md#diagrama-entidad-relación-er)

---

## 🔧 Comandos Útiles

### Migraciones de Base de Datos

```bash
# Crear migración
dotnet ef migrations add NombreMigracion --project src/Infrastructure --startup-project src/API

# Aplicar migraciones
dotnet ef database update --project src/Infrastructure --startup-project src/API

# Eliminar última migración
dotnet ef migrations remove --project src/Infrastructure --startup-project src/API

# Generar script SQL
dotnet ef migrations script --project src/Infrastructure --startup-project src/API
```

### Compilación y Ejecución

```bash
# Compilar solución
dotnet build

# Ejecutar API
cd src/API
dotnet run

# Ejecutar con watch (auto-restart)
dotnet watch run

# Publicar para producción
dotnet publish -c Release -o ./publish
```

### Testing (cuando se implementen)

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true
```

---

## 🌐 Endpoints Principales

### Autenticación
```
POST   /api/auth/login          # Login
POST   /api/auth/logout         # Logout (opcional)
```

### Usuarios
```
GET    /api/usuarios            # Listar usuarios (paginado)
GET    /api/usuarios/{id}       # Obtener por ID
POST   /api/usuarios            # Crear usuario
PUT    /api/usuarios/{id}       # Actualizar usuario
DELETE /api/usuarios/{id}       # Desactivar usuario
PUT    /api/usuarios/{id}/cambiar-password  # Cambiar contraseña
```

### Productos
```
GET    /api/productos           # Listar productos (paginado)
GET    /api/productos/{id}      # Obtener por ID
POST   /api/productos           # Crear producto
PUT    /api/productos/{id}      # Actualizar producto
DELETE /api/productos/{id}      # Desactivar producto
```

### Categorías
```
GET    /api/categorias          # Listar categorías
GET    /api/categorias/{id}     # Obtener por ID
POST   /api/categorias          # Crear categoría
PUT    /api/categorias/{id}     # Actualizar categoría
DELETE /api/categorias/{id}     # Desactivar categoría
```

**Especificación completa**: Ver [`ENDPOINTS.md`](ENDPOINTS.md)

---

## 🧪 Testing

### Pruebas Unitarias
```csharp
[Fact]
public async Task GetByIdAsync_UsuarioExiste_RetornaUsuario()
{
    // Arrange
    var mockRepo = new Mock<IUsuarioRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(new Usuario { Id = 1, Email = "test@test.com" });
    
    // Act
    var result = await _service.GetByIdAsync(1);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("test@test.com", result.Email);
}
```

**Guía completa**: Ver [`BEST_PRACTICES.md`](BEST_PRACTICES.md#testing)

---

## 📈 Roadmap

### Fase 1: Fundamentos ✅
- [x] Arquitectura de capas
- [x] Entidades de dominio
- [x] Autenticación JWT
- [x] CRUD básico

### Fase 2: Funcionalidades Avanzadas
- [ ] Refresh tokens
- [ ] Rate limiting
- [x] Caching en memoria del backend
- [ ] Notificaciones por email

### Fase 3: Escalabilidad
- [ ] Implementar CQRS
- [ ] Event Sourcing
- [ ] Microservicios
- [ ] Docker/Kubernetes

### Fase 4: Observabilidad
- [ ] Application Insights
- [ ] Health checks
- [ ] Métricas con Prometheus
- [ ] Dashboards con Grafana

---

## 🤝 Contribución

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📝 Licencia

Este proyecto está bajo la Licencia MIT. Ver archivo `LICENSE` para más detalles.

---

## 👥 Autores

- **Tu Nombre** - *Desarrollo inicial* - [GitHub](https://github.com/tu-usuario)

---

## 🙏 Agradecimientos

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft - ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

## 📞 Soporte

Para preguntas o reportar problemas, por favor abrir un [issue](https://github.com/tu-usuario/backendORCinverisones/issues).

---

<div align="center">

**Desarrollado con ❤️ usando .NET Core y Clean Architecture**

</div>
