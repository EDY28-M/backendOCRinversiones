# ============================================================
# Dockerfile - Backend ORC Inversiones (.NET 10)
# Compatible con Render.com (Linux/amd64)
# ============================================================

# ── STAGE 1: BUILD ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo el csproj primero para aprovechar cache de layers
COPY ["backendORCinverisones.csproj", "./"]
RUN dotnet restore "backendORCinverisones.csproj" --runtime linux-x64

# Copiar el resto del código
COPY . .

# Publicar en modo Release optimizado
RUN dotnet publish "backendORCinverisones.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true

# ── STAGE 2: RUNTIME ────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Crear directorio de logs con permisos correctos
# (sin USER app para evitar problemas en Render)
RUN mkdir -p /app/logs && chmod 755 /app/logs

# Copiar artefactos publicados
COPY --from=build /app/publish .

# Variables de entorno para producción en Render
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Render usa el puerto 10000 por defecto
EXPOSE 10000

ENTRYPOINT ["dotnet", "backendORCinverisones.dll"]
