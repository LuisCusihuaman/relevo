# Arquitectura de Tipos y Datos

Este proyecto sigue una arquitectura estricta de tipos para mantener la consistencia entre el Backend (API) y el Frontend (UI), minimizando la duplicación y los errores.

## Flujo de Datos (Single Source of Truth)

El flujo de tipos es unidireccional y tiene una única fuente de verdad:

```mermaid
graph TD
    A[Backend (C# / FastEndpoints)] -->|OpenAPI JSON| B(OpenAPI Schema)
    B -->|openapi-typescript| C[Tipos Generados (@/api/generated)]
    C -->|Mappers (@/api/mappers)| D[Tipos de Dominio (@/types/domain)]
    D -->|Components| E[Vistas / UI]
```

### 1. Backend (SSOT)
El backend define los contratos de datos. El archivo `swagger.json` es la fuente de verdad.

### 2. Tipos Generados (`src/api/generated`)
Usamos `openapi-typescript` para generar tipos TypeScript automáticamente desde el esquema OpenAPI.
**NUNCA** edites manualmente los archivos en esta carpeta.

**Comandos:**
- `pnpm generate:types:remote`: Descarga el esquema del backend local y regenera los tipos.
- `pnpm generate:types`: Regenera los tipos usando el archivo `openapi.json` existente.

### 3. Mappers (`src/api/mappers`)
Los tipos generados por la API a menudo contienen `null | undefined` o estructuras anidadas que son difíciles de usar en la UI.
Los mappers son funciones puras que:
- Transforman tipos de API (`ApiPatientRecord`) a tipos de Dominio (`Patient`).
- Manejan valores nulos/undefined con valores por defecto seguros.
- Normalizan enums y strings.

**Ejemplo:**
```typescript
// src/api/mappers/patient.mapper.ts
export function mapApiPatientToPatient(api: ApiPatientRecord): Patient {
    return {
        id: api.id ?? "",
        name: api.name ?? "Desconocido",
        // ...
    };
}
```

### 4. Tipos de Dominio (`src/types/domain.ts`)
Estos son los tipos que usa la aplicación React. Son limpios, seguros y específicos para el frontend.
**Regla:** Todos los componentes y hooks deben usar estos tipos, NO los tipos generados de la API.

---

## Guía para Desarrolladores

### ¿Cómo agrego un nuevo campo?

1. **Backend:** Agrega el campo en el DTO de C#.
2. **Generar:** Ejecuta `pnpm generate:types:remote` (asegúrate que el backend esté corriendo).
3. **Dominio:** Agrega el campo en `src/types/domain.ts`.
4. **Mapper:** Actualiza el mapper correspondiente en `src/api/mappers/` para mapear del tipo generado al tipo de dominio.
5. **UI:** Usa el nuevo campo en tus componentes.

### ¿Por qué usar Mappers?

- **Desacoplamiento:** Si la API cambia de nombre un campo (`first_name` -> `firstName`), solo actualizas el mapper. El resto de la app no se entera.
- **Seguridad:** La API puede devolver `null`. El dominio garantiza que ciertos campos siempre tengan valor (ej. string vacío en lugar de null).
- **Transformación:** Puedes convertir fechas (strings ISO) a objetos Date, o calcular campos derivados (ej. `fullName`) en el mapper.

### Estructura de Carpetas

```
src/
├── api/
│   ├── generated/       # Tipos autogenerados (NO EDITAR)
│   ├── mappers/         # Funciones de transformación
│   └── endpoints/       # Llamadas a la API (React Query)
├── types/
│   └── domain.ts        # Tipos globales de la aplicación
```
