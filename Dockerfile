# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backendORCinverisones.csproj", "./"]
RUN dotnet restore "backendORCinverisones.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "backendORCinverisones.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "backendORCinverisones.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Cambiar a usuario no root para seguridad (recomendado en producción)
USER app

ENTRYPOINT ["dotnet", "backendORCinverisones.dll"]
