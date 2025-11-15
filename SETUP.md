# 🚀 Guía de Configuración - RELEVO

Esta guía te ayudará a configurar todo el proyecto RELEVO desde cero.

## ⚡ Inicio Rápido

Si ya tienes todo instalado, aquí están los comandos esenciales:

```bash
# 1. Base de datos (Terminal 1)
cd relevo-api
docker compose up -d

# 2. Backend API (Terminal 2)
cd relevo-api/src/Relevo.Web
dotnet run --launch-profile https

# 3. Frontend (Terminal 3)
cd relevo-frontend
pnpm install
pnpm run setup  # Solo la primera vez
pnpm run dev

# 4. Realtime Hub (Terminal 4, opcional)
cd relevo-realtimehub
pnpm install
pnpm run start:dev
```

**Importante**: No olvides configurar las variables de entorno (ver secciones detalladas abajo).

---

## 📋 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

- [Node.js 18+](https://nodejs.org/en)
- [pnpm](https://pnpm.io) (o npm/yarn)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com) (para la base de datos Oracle)
- [Git](https://git-scm.com)

### 🔧 Instalación de Requisitos

#### Node.js y pnpm (macOS)

```bash
# Instalar Node.js usando Homebrew
brew install node

# Verificar instalación
node --version  # Debería mostrar v18+ o superior
npm --version

# Instalar pnpm globalmente
npm install -g pnpm

# Verificar pnpm
pnpm --version
```

#### .NET 8 SDK (macOS)

```bash
# Opción 1: Usando Homebrew (Recomendado)
brew install --cask dotnet-sdk

# Opción 2: Descargar desde el sitio oficial
# Visita: https://dotnet.microsoft.com/download

# Verificar instalación
dotnet --version  # Debería mostrar 8.0.x o superior
```

**Nota**: Si `.NET` está instalado pero no está en el PATH, agrega esto a tu `~/.zshrc`:
```bash
export PATH="/usr/local/share/dotnet:$PATH"
# O si está en Homebrew:
export PATH="/opt/homebrew/share/dotnet:$PATH"
```

Luego ejecuta: `source ~/.zshrc`

#### Docker (macOS)

```bash
# Opción 1: Docker Desktop (Recomendado para principiantes)
# Descarga desde: https://www.docker.com/products/docker-desktop

# Opción 2: Colima + Docker CLI (Para usuarios avanzados)
brew install colima docker docker-compose
colima start
```

## 🗄️ Paso 1: Configurar la Base de Datos Oracle

### Opción A: Usando Docker Compose (Recomendado) ⭐

Los scripts SQL se ejecutan automáticamente cuando el contenedor se inicia por primera vez:

```bash
# Navegar al directorio de la API
cd relevo-api

# Iniciar el contenedor de Oracle (los scripts SQL se ejecutarán automáticamente)
docker compose up -d

# Esperar ~60-90 segundos para que Oracle se inicialice completamente
# Puedes verificar los logs:
docker logs -f xe11
```

**Nota**: 
- La contraseña por defecto es `TuPass123` según el `compose.yml`
- Los scripts SQL en `src/Relevo.Infrastructure/Sql/` se montan automáticamente y se ejecutan en orden
- Si el contenedor ya existe, elimínalo primero: `docker compose down -v` y luego `docker compose up -d`

### Opción B: Verificar la inicialización manualmente

Si necesitas verificar o ejecutar los scripts manualmente:

```bash
# Esperar a que Oracle esté listo (puede tardar 1-2 minutos)
docker exec xe11 bash -c "echo 'SELECT 1 FROM DUAL;' | sqlplus -s system/TuPass123"

# Verificar que las tablas se crearon
docker exec -i xe11 bash -c "
sqlplus -s system/TuPass123 << EOF
SELECT 'UNITS: ' || COUNT(*) FROM UNITS
UNION ALL
SELECT 'PATIENTS: ' || COUNT(*) FROM PATIENTS
UNION ALL
SELECT 'SHIFTS: ' || COUNT(*) FROM SHIFTS;
EXIT;
EOF
"
```

## 🔧 Paso 2: Configurar el Backend API (.NET)

### 2.1. Verificar configuración

El archivo `appsettings.Development.json` ya está configurado para conectarse a Oracle:

```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15"
  }
}
```

### 2.2. Configurar Clerk (Autenticación)

Edita `relevo-api/src/Relevo.Web/appsettings.json` o `appsettings.Development.json` con tus credenciales de Clerk:

```json
{
  "Clerk": {
    "PublishableKey": "pk_test_tu_clave_aqui",
    "SecretKey": "sk_test_tu_clave_secreta_aqui"
  }
}
```

**Obtén tus claves de Clerk:**
1. Ve a [Clerk Dashboard](https://dashboard.clerk.dev/)
2. Crea una aplicación o usa una existente
3. Copia las claves desde la sección "API Keys"

### 2.3. Ejecutar el Backend

```bash
cd relevo-api/src/Relevo.Web
dotnet restore
dotnet run --launch-profile https
```

El API estará disponible en `https://localhost:57679` (o el puerto configurado en `launchSettings.json`).

## 🎨 Paso 3: Configurar el Frontend (React/Vite)

### 3.1. Instalar dependencias

```bash
cd relevo-frontend
pnpm install
```

### 3.2. Configurar variables de entorno

Crea un archivo `.env` en la raíz de `relevo-frontend`:

```bash
# Clerk Authentication - Obtén esto desde tu dashboard de Clerk
VITE_CLERK_PUBLISHABLE_KEY=pk_test_tu_clave_publica_aqui

# API Configuration - URL del backend
VITE_API_URL=https://localhost:57679
```

**Importante**: Usa la misma `PublishableKey` que configuraste en el backend.

### 3.3. Ejecutar el setup inicial

```bash
pnpm run setup
```

Este comando:
- Inicializa el repositorio Git
- Configura Husky (git hooks)
- Instala Playwright para tests E2E

### 3.4. Ejecutar el Frontend

```bash
pnpm run dev
```

El frontend estará disponible en `http://localhost:5173` (o el puerto que Vite asigne).

## 🔌 Paso 4: Configurar el Realtime Hub (NestJS)

### 4.1. Instalar dependencias

```bash
cd relevo-realtimehub
pnpm install
```

### 4.2. Ejecutar el Realtime Hub

```bash
# Modo desarrollo (con watch)
pnpm run start:dev

# O modo producción
pnpm run start:prod
```

El hub estará disponible en el puerto configurado (por defecto `3000`).

## ✅ Verificación

Una vez que todo esté ejecutándose:

1. **Base de datos**: Verifica que el contenedor de Oracle esté corriendo:
   ```bash
   docker ps | grep xe11
   ```

2. **Backend API**: Abre `https://localhost:57679` en tu navegador (deberías ver una respuesta del API)

3. **Frontend**: Abre `http://localhost:5173` y verifica que cargue correctamente

4. **Realtime Hub**: Verifica que el servicio esté escuchando en su puerto

## 🧪 Testing

### Frontend

```bash
cd relevo-frontend

# Tests unitarios
pnpm run test:unit

# Tests E2E
pnpm run test:e2e

# Todos los tests
pnpm run test
```

### Backend

```bash
cd relevo-api

# Tests unitarios
dotnet test tests/Relevo.UnitTests

# Tests funcionales
dotnet test tests/Relevo.FunctionalTests

# Tests de integración
dotnet test tests/Relevo.IntegrationTests
```

### Realtime Hub

```bash
cd relevo-realtimehub

# Tests unitarios
pnpm run test

# Tests E2E
pnpm run test:e2e
```

## 🐛 Solución de Problemas

### Oracle no se conecta

1. Verifica que el contenedor esté corriendo:
   ```bash
   docker ps
   ```

2. Verifica los logs:
   ```bash
   docker logs xe11
   ```

3. Espera más tiempo (Oracle puede tardar 1-2 minutos en inicializarse completamente)

### Error de autenticación con Clerk

1. Verifica que las claves de Clerk sean correctas en ambos archivos:
   - `relevo-api/src/Relevo.Web/appsettings.json`
   - `relevo-frontend/.env`

2. Asegúrate de usar la misma aplicación de Clerk en ambos lugares

### Puerto ya en uso

Si algún puerto está ocupado:

- **Backend**: Edita `relevo-api/src/Relevo.Web/Properties/launchSettings.json`
- **Frontend**: Vite automáticamente usará el siguiente puerto disponible
- **Realtime Hub**: Edita `relevo-realtimehub/src/main.ts`

## 📚 Documentación Adicional

- [Database Setup Guide](relevo-api/src/Relevo.Web/README-DATABASE.md)
- [Docker Setup Guide](relevo-api/src/Relevo.Web/README-DOCKER.md)
- [Clerk Setup Guide](relevo-frontend/CLERK_SETUP.md)
- [Frontend README](relevo-frontend/README.md)

## 🎯 Orden Recomendado de Ejecución

Para desarrollo, ejecuta en este orden:

1. **Base de datos** (Docker):
   ```bash
   cd relevo-api && docker compose up -d
   # Esperar ~60-90 segundos para que Oracle se inicialice
   ```

2. **Backend API** (terminal 1):
   ```bash
   cd relevo-api/src/Relevo.Web && dotnet run --launch-profile https
   ```

3. **Realtime Hub** (terminal 2, opcional):
   ```bash
   cd relevo-realtimehub && pnpm run start:dev
   ```

4. **Frontend** (terminal 3):
   ```bash
   cd relevo-frontend && pnpm run dev
   ```

¡Listo! Ahora deberías tener todo el proyecto RELEVO funcionando. 🎉

