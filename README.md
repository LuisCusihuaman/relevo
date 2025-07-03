# 🩺 RELEVO

RELEVO is a collaborative, real-time handoff platform for clinicians at Hospital Garrahan. It implements the I-PASS methodology to ensure safe, structured medical transitions.

---

## 📦 Repository Structure

This monorepo contains the full backend and real-time stack for RELEVO, plus a standalone frontend

```
relevo-workspace/
├── apps/
│   ├── relevo-api/           # C# Backend (main clinical API)
│   ├── nestjs-service/       # Realtime WebSocket + sync service
│   ├── hospital-mock-api/    # Mock EMR API for testing
│   └── relevo-frontend/      # Vite + React application (based on vite-react-boilerplate)
│
├── libs/
│   ├── shared-types/         # Shared TS types (frontend + NestJS)
│   ├── hospital-sdk/         # Client SDK for external hospital APIs
│   └── shared-utils/         # Common utilities
```

---

## 🌐 Frontend

### Highlights:

* TypeScript + ESLint + Prettier
* Tailwind CSS
* TanStack Router / Query / Table
* Zustand (state)
* React Hook Form + Zod (forms)
* Storybook + Testing Library + Vitest + Playwright
* Internationalization (i18n)
* Husky + Commitlint + Commitizen
* Devtools (Query, Router, Table, RHF)

To get started:

```bash
git clone https://github.com/LuisCusihuaman/relevo && \
cd relevo-workspace/apps/relevo-frontend && \
pnpm install && \
pnpm run setup
```

See [`apps/relevo-frontend/README.md`](./apps/relevo-frontend/README.md) for full instructions.

---

## 🔧 Backend Overview

| App                 | Description                                                                             |
| ------------------- | --------------------------------------------------------------------------------------- |
| `relevo-api`        | Main ASP.NET Core API with domain logic, REST endpoints, and integration with Oracle DB |
| `nestjs-service`    | Real-time WebSocket sync service using Hocuspocus and Socket.IO                         |
| `hospital-mock-api` | Mocked external EMR API for local development and testing                               |

The backend communicates with an Oracle XE database and uses Clerk for authentication. Real-time collaboration is enabled via WebSockets and Yjs (through Hocuspocus).

The `relevo-api` contains:

* Unit setup logic (unit, shifts, roles)
* Patient list and filtering
* Full I-PASS workflow endpoints (illness severity, summary, action list, synthesis)
* Audit logging and timeline API
* Role-based authorization

---

## 🧪 Testing Strategy

* Frontend: Vitest, React Testing Library, Playwright (E2E)
* Backend (C#): xUnit
* Backend (NestJS): Jest

---

## 🧭 Deployment

* All apps are dockerizable
* Environment configuration follows 12-factor principles
* Realtime and auth communication is secured via JWT and Clerk

---

## 🔐 Security & Compliance

* Role-based access control (RBAC)
* TLS 1.3 everywhere
* Oracle Transparent Data Encryption (TDE)
* Immutable audit logs with human-readable summaries
