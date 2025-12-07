# Análisis de Deuda Técnica en Tipos - Relevo Frontend

Este documento detalla el análisis y la refactorización realizada de los tipos TypeScript en el proyecto `@relevo-frontend`.

## 1. Estado Actual (Post-Refactorización)

Se ha consolidado el sistema de tipos para eliminar la duplicidad y fragmentación.

- **Fuente de la Verdad (SSOT)**: `src/types/domain.ts` contiene ahora todas las definiciones de entidades de negocio (`Patient`, `HandoverStatus`, `IllnessSeverity`, `UserRole`, etc.).
- **API Types**: `src/api/types.ts` ahora extiende y reutiliza los tipos de dominio en lugar de redefinirlos.
- **Tipos Fuertes**: Se han reemplazado `string` genéricos por Union Types exportados.

## 2. Mejoras Realizadas

### Consolidación de Entidades

| Concepto | Antes | Ahora | Beneficio |
| :--- | :--- | :--- | :--- |
| **Paciente** | Definido en 3 lugares con propiedades dispares | `Patient` en `domain.ts` es la base. `PatientDetail` y `PatientHandoverData` extienden de él. | Mantenimiento centralizado. Garantía de consistencia. |
| **Status** | Literales duplicados en cada archivo | `HandoverStatus`, `IllnessSeverity`, `SituationAwarenessStatus` definidos en `domain.ts` y reusados. | Seguridad de tipos y autocompletado consistente. |
| **Roles** | `string[]` | `UserRole[]` (`"physician" | "nurse" | ...`) | Validación de roles en tiempo de compilación. |

### Endurecimiento de Tipos (Hardening)

- **`SituationAwarenessStatus`**: Ahora es un union type (`"Draft" | "Ready" | ...`) en lugar de `string`.
- **`PatientHandoverData`**: Se ha roto el "God Object". Ahora extiende de `Patient` y usa tipos auxiliares como `PhysicianAssignment` para agrupar datos relacionados.

### Limpieza

- Se han eliminado definiciones redundantes en `src/api/types.ts`.
- Se han alineado los mappers de API para usar los nuevos tipos consolidados.

## 3. Próximos Pasos (Recomendaciones)

1.  **Validación en Runtime**: Integrar Zod para validar que las respuestas de la API realmente cumplen con los Union Types definidos (especialmente `IllnessSeverity` y `HandoverStatus`).
2.  **Migración de Componentes**: Revisar componentes antiguos que puedan estar usando tipos ad-hoc y migrarlos a los tipos de dominio.
3.  **Strict Null Checks**: Asegurar que el manejo de `null | undefined` en propiedades opcionales (`?`) sea consistente en todo el consumo de estos tipos.
