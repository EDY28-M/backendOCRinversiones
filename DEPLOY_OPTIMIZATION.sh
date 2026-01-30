#!/bin/bash

# ============================================
# Script de Despliegue - Optimizaciones Backend
# Backend OCR Inversiones
# ============================================

set -e

echo "=========================================="
echo "🚀 DESPLIEGUE DE OPTIMIZACIONES"
echo "=========================================="
echo ""

# Colores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 1. Restaurar paquetes NuGet
echo -e "${YELLOW}Paso 1: Restaurando paquetes NuGet...${NC}"
dotnet restore
echo -e "${GREEN}✓ Paquetes restaurados${NC}"
echo ""

# 2. Compilar proyecto
echo -e "${YELLOW}Paso 2: Compilando proyecto...${NC}"
dotnet build --configuration Release
echo -e "${GREEN}✓ Compilación exitosa${NC}"
echo ""

# 3. Crear migración EF Core
echo -e "${YELLOW}Paso 3: Creando migración de EF Core...${NC}"
dotnet ef migrations add OptimizationIndices --project . --startup-project . --context ApplicationDbContext
echo -e "${GREEN}✓ Migración creada${NC}"
echo ""

# 4. Aplicar migración
echo -e "${YELLOW}Paso 4: Aplicando migración a BD...${NC}"
dotnet ef database update
echo -e "${GREEN}✓ Migración aplicada${NC}"
echo ""

# 5. Recordatorio de scripts SQL
echo -e "${YELLOW}Paso 5: RECORDATORIO - Ejecutar scripts SQL manualmente:${NC}"
echo "  1. Infrastructure/Data/Migrations/ManualMigration_OptimizationIndices.sql"
echo "  2. Infrastructure/Data/StoredProcedures/SP_OptimizedQueries.sql"
echo ""
echo "  Ejecutar con:"
echo "  sqlcmd -S localhost -d ORCInversiones_Dev -i <archivo.sql>"
echo ""

# 6. Crear carpeta de logs
echo -e "${YELLOW}Paso 6: Creando carpeta de logs...${NC}"
mkdir -p logs
echo -e "${GREEN}✓ Carpeta logs creada${NC}"
echo ""

# 7. Ejecutar tests (si existen)
if [ -d "Tests" ]; then
    echo -e "${YELLOW}Paso 7: Ejecutando tests...${NC}"
    dotnet test
    echo -e "${GREEN}✓ Tests ejecutados${NC}"
    echo ""
else
    echo -e "${YELLOW}Paso 7: No se encontraron tests${NC}"
    echo ""
fi

# 8. Mostrar resumen
echo "=========================================="
echo -e "${GREEN}✅ DESPLIEGUE COMPLETADO EXITOSAMENTE${NC}"
echo "=========================================="
echo ""
echo "Próximos pasos:"
echo "  1. Ejecutar los scripts SQL manualmente (paso 5)"
echo "  2. Iniciar aplicación: dotnet run"
echo "  3. Verificar logs en: ./logs/backend-*.log"
echo "  4. Monitorear performance de endpoints críticos"
echo ""
echo "Documentación: OPTIMIZACIONES_IMPLEMENTADAS.md"
echo ""
