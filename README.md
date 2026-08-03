# MesaSitec – Prueba Técnica

Sistema de mesa de servicio multi-tenant desarrollado como prueba técnica para Sitecpro.

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/es-es/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- Git

## Cómo levantar el proyecto

### Backend

- Abrir una terminal (PowerShell, CMD o Git Bash) en la raíz del proyecto.

- Abre la carpeta del backend
- Ejecutar la aplicación (restaura paquetes, aplica migraciones, crea la BD y la semilla)
```bash
   cd backend; dotnet run
```
   La API se inicia en http://localhost:5080.

### Frontend

- Abrir una terminal (PowerShell, CMD o Git Bash) en la raíz del proyecto.

- Abre la carpeta del frontend
- Instalar dependencias
- Inicia el servidor de desarrollo
```bash
   cd frontend; npm install; npm run dev
```

   La aplicación se abre en http://localhost:5173.

## Credenciales de prueba

| Email | Organización | Rol |
|---|---|---|
| admin@norte.test | Cooperativa Norte | Admin |
| agente1@norte.test | Cooperativa Norte | Agente |
| agente2@norte.test | Cooperativa Norte | Agente |
| user1@norte.test | Cooperativa Norte | Solicitante |
| user2@norte.test | Cooperativa Norte | Solicitante |
| admin@sur.test | Bufete Sur | Admin |
| user1@sur.test | Bufete Sur | Solicitante |

Contraseña para todos: `Sitec.2026`

## Endpoints implementados

- POST /api/v1/auth/login
- GET /api/v1/me
- GET /api/v1/categorias
- GET /api/v1/solicitudes
- POST /api/v1/solicitudes
- GET /api/v1/solicitudes/{id}
- PUT /api/v1/solicitudes/{id}
- POST /api/v1/solicitudes/{id}/transiciones
- GET /api/v1/health
- GET /api/v1/usuarios/agentes (extra, documentado en DECISIONES.md)

## Qué está implementado y qué no

### Backend

Implementado:
- Modelo de datos (Tenant, Usuario, Categoria, Solicitud)
- Semilla automática con 2 organizaciones y 33 solicitudes
- Autenticación JWT (login, /me, claims tenantId/rol/email)
- 9 endpoints del contrato (CRUD de solicitudes, transiciones, categorías, health)
- Reglas de negocio RN-01 a RN-07 completas
- Manejo global de errores con formato problem+json
- 12 pruebas unitarias (máquina de estados, SLA, permisos)
- Migraciones automáticas al arrancar

Adicional:
- Endpoint extra `GET /usuarios/agentes` (justificado en DECISIONES.md)

### Frontend

Implementado:
- Login con validación de credenciales
- Listado de solicitudes con filtros, paginación y ordenamiento
- Detalle de solicitud con botones condicionales según rol y estado
- Formulario de creación/edición con validación en cliente
- Modal de transiciones extraído a componente reutilizable
- Cliente HTTP centralizado con interceptor de token y redirección 401
- Todos los data-testid obligatorios implementados

Pendiente:
- `toast-mensaje` es un placeholder, sin funcionalidad real de notificaciones

### General

Implementado:
- Estructura de carpetas funcional (alternativa a la sugerida en 5.2)

No implementado:
- `docker-compose.yml`

## Pruebas unitarias

```bash
dotnet test backend/tests/Tests.csproj
```

Ejecuta 12 pruebas que cubren la máquina de estados (RN-02), el cálculo del SLA (RN-04) y las reglas de permisos (RN-03).

## Documentación adicional

DECISIONES.md – Decisiones técnicas, uso de IA y puntos de atasco.