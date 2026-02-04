# 🔧 Guía de Configuración de Redis

## 📋 Resumen

Redis se utiliza como **capa de caché distribuida** (Nivel 2) para mejorar el rendimiento del backend. Funciona junto con MemoryCache (Nivel 1) para proporcionar:

- **Mayor velocidad**: Datos frecuentes en memoria local
- **Persistencia**: Caché compartida entre reinicios de la app
- **Escalabilidad**: Caché compartida entre múltiples instancias

---

## 🚀 Opción 1: Redis Local (Desarrollo)

### Paso 1: Iniciar Redis con Docker

```bash
# Usando docker-compose (recomendado)
docker-compose -f docker-compose.redis.yml up -d

# O usando Docker directamente
docker run -d --name orc-redis -p 6379:6379 redis:7-alpine
```

### Paso 2: Configurar appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "Redis": "localhost:6379,abortConnect=false"
  }
}
```

### Paso 3: Verificar conexión

```bash
# Acceder a Redis CLI
docker exec -it orc-redis redis-cli

# Probar conexión
127.0.0.1:6379> ping
PONG

# Ver estadísticas
127.0.0.1:6379> info stats
```

### UI Web para Redis (opcional)

Si usaste docker-compose, accede a Redis Commander en:
```
http://localhost:8081
```

---

## ☁️ Opción 2: Redis en Render (Producción)

### Opción 2A: Redis Cloud (Gratis)

1. Crear cuenta en [Redis Cloud](https://redis.com/try-free/)
2. Crear nueva suscripción (Free - 30MB)
3. Obtener el endpoint y password
4. Configurar en Render:

```bash
# Environment Variable en Render
ConnectionStrings__Redis=redis-XXXX.cXXX.us-east-1-2.ec2.cloud.redislabs.com:port,password=tu-password,abortConnect=false
```

### Opción 2B: Upstash Redis (Recomendado - Serverless)

1. Crear cuenta en [Upstash](https://upstash.com/)
2. Crear nuevo database
3. Seleccionar región más cercana (us-east-1 para Render)
4. Copiar la URL de conexión:

```bash
# Environment Variable en Render
ConnectionStrings__Redis=redis://default:password@host:port
```

### Opción 2C: Redis en Render Native (Pagado)

Render ofrece Redis como servicio nativo (desde $10/mes):

1. Dashboard → New → Redis
2. Seleccionar región y plan
3. Conectar con la variable de entorno proporcionada

---

## 🔧 Configuración de Connection String

### Formatos soportados:

```bash
# Formato básico
localhost:6379

# Con password
localhost:6379,password=tu-password

# URL completa (Upstash/Redis Cloud)
redis://username:password@host:port

# Con opciones adicionales
host:port,password=pass,ssl=true,abortConnect=false,connectTimeout=5000
```

### Opciones recomendadas:

| Opción | Descripción | Valor recomendado |
|--------|-------------|-------------------|
| `abortConnect` | No fallar si no puede conectar | `false` |
| `connectTimeout` | Timeout de conexión (ms) | `5000` |
| `syncTimeout` | Timeout de operaciones (ms) | `5000` |
| `ssl` | Usar TLS/SSL | `true` (en producción) |

---

## 📊 Monitoreo de Redis

### Comandos útiles:

```bash
# Estadísticas generales
redis-cli info

# Keys en memoria
redis-cli dbsize

# Memoria usada
redis-cli info memory | grep used_memory_human

# Limpiar toda la caché (⚠️ Precaución)
redis-cli flushall

# Ver keys por patrón
redis-cli keys "categories:*"
```

### Métricas importantes:

- **used_memory**: Memoria usada por Redis
- **keyspace_hits**: Caché hits (ideal > 80%)
- **keyspace_misses**: Caché misses
- **connected_clients**: Conexiones activas

---

## 🎯 Estrategia de Caché

### TTL (Time To Live) configurado:

| Tipo de datos | Duración | Descripción |
|---------------|----------|-------------|
| Categorías | 2 horas | Datos casi estáticos |
| Marcas | 2 horas | Datos casi estáticos |
| Productos públicos | 10 segundos | Datos semi-dinámicos |
| Metadatos públicos | 10 segundos | Marcas/categorías activas |

### Invalidación:

- **Automática**: Cuando se crea/actualiza/elimina un recurso
- **Manual**: Mediante el método `RemoveByPrefix()`
- **Por tiempo**: Expiración automática después del TTL

---

## 🐛 Troubleshooting

### Problema: "Redis no configurado"

**Causa**: Connection string vacío o mal formado

**Solución**:
```bash
# Verificar variable de entorno
echo $ConnectionStrings__Redis

# En Render, debe estar en formato:
redis://username:password@host:port
```

### Problema: "No connection could be made"

**Causa**: Redis no está corriendo o firewall bloquea el puerto

**Solución**:
```bash
# Verificar si Redis está corriendo
docker ps | grep redis

# Ver logs
docker logs orc-redis

# Probar conexión local
redis-cli -h localhost -p 6379 ping
```

### Problema: Timeout en operaciones

**Causa**: Latencia de red alta o Redis sobrecargado

**Solución**:
- Aumentar `syncTimeout` en connection string
- Verificar memoria disponible: `redis-cli info memory`
- Considerar upgrade de plan si es Redis Cloud

### Problema: Caché no se invalida

**Causa**: Prefijo de key incorrecto o Redis desconectado

**Solución**:
```bash
# Verificar keys existentes
redis-cli keys "*"

# Limpiar manualmente si es necesario
redis-cli flushall
```

---

## 🔒 Seguridad

### En producción:

1. **Siempre usar password** en Redis
2. **Habilitar SSL/TLS** para conexiones externas
3. **Restringir acceso por IP** si es posible
4. **No exponer Redis** directamente a internet
5. **Usar Redis Cloud/Upstash** para producción (gestionado)

### Variables de entorno en Render:

```bash
# ✅ Correcto - Usar URL completa con credenciales
ConnectionStrings__Redis=redis://default:password@host.redis.cloud:port

# ❌ Incorrecto - No hardcodear en appsettings.json
```

---

## 📈 Optimizaciones

### Configuración de Redis (redis.conf):

```conf
# Política de evicción cuando la memoria está llena
maxmemory-policy allkeys-lru

# Límite de memoria (ajustar según plan)
maxmemory 256mb

# Persistencia (opcional para caché)
appendonly yes
appendfsync everysec
```

### En docker-compose:

```yaml
command: redis-server --appendonly yes --maxmemory 256mb --maxmemory-policy allkeys-lru
```

---

## 🧪 Testing

### Verificar que Redis funciona:

1. Iniciar la API
2. Hacer request a `/api/categories` (primera vez - lento)
3. Hacer request nuevamente (debe ser < 10ms)
4. Verificar en logs: "✅ Redis conectado correctamente"
5. Verificar keys en Redis: `redis-cli keys "*"`

### Health Check:

```bash
curl https://tu-api.com/health
```

Debe mostrar:
```json
{
  "checks": [
    {
      "name": "cache",
      "status": "Healthy",
      "data": {
        "redisEnabled": true,
        "redisConnected": true
      }
    }
  ]
}
```

---

## 📚 Recursos

- [Redis Documentation](https://redis.io/documentation)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [Upstash Redis](https://docs.upstash.com/redis)
- [Redis Cloud](https://docs.redis.com/latest/rc/)

---

**¿Problemas?** Revisa los logs de la aplicación:
```bash
# En Render
tail -f /var/log/render/*.log

# Local
dotnet run --verbosity debug
```
