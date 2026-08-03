# DECISIONES.md

## 1. Decisiones técnicas

### Estructura de carpetas plana en backend
- **Descartada:** Separar el código en `src/{Api,Aplicacion,Dominio,Infraestructura}`.
- **Por qué mi decisión:** Para mantener el código agrupado por funcionalidad (`Controllers`, `Auth`, `Seed`, `Middlewares`) me permitió avanzar más rápido y reducir la fricción entre las capas.


### Recálculo del SLA al reabrir
- **Consultado al reclutador:** RN-04 no especifica qué ocurre con el SLA cuando una solicitud se reabre. Pregunté si debía recalcularse o mantenerse.
- **Por qué mi decisión:** Implementé el recálculo en `SolicitudesService.EjecutarTransicion`, justo después de que la máquina de estados ejecuta la acción `reabrir`.

### Endpoint extra `GET /api/v1/usuarios/agentes`
- **Motivo:** La prueba solo define 9 endpoints, pero el frontend necesitaba listar los agentes disponibles para "asignar", para no sobrecargar otro o dejar el selector inservible, añadí este endpoint filtrado por `tenantId`.


## 2. Uso de IA - ChatGPT - Claude

- **Estructura inicial del proyecto, seed data y configuración de EF Core:** generados con IA a partir de la especificación. Luego revisé y adapté cada archivo.
- **Máquina de estados (`MaquinaEstados.cs`), cálculo de SLA (`SlaCalculator.cs`) y pruebas unitarias:** Validados por mi persona.
- **Frontend:** la estructura base (router, stores de Pinia, cliente HTTP) fue generada con IA. Las vistas, las reglas de visibilidad de botones y los `data-testid` los reprogramé.
- **Ajustes finales (filtros, formato de errores, extracción del modal):** Validados y reprogramados para no pasar por alto algún requerimiento según lo descrito en la prueba.

## 3. Dónde me atasqué

- Mi primer contacto real con .NET y su ORM. No tenía experiencia previa con .NET, Entity Framework Core ni SQLite, así que dediqué tiempo a entender sus funciones, cómo se configuran las dependencias, cómo funciona el `DbContext`, qué son las migraciones automáticas y cómo instalar los paquetes NuGet necesarios. Lo resolví yendo paso a paso: crear el proyecto inicial, añadir un paquete y validar que se instalaran las versiones correctas.
- Errores con Git: commits a destiempo, reset --hard accidental, recuperación con reflog, force push.

## 4. Qué haría distinto con una semana más

- **Componentizar más el frontend:** extraería también la tabla y los filtros en componentes reutilizables, además del modal.
- **Historial de resoluciones:** en lugar de sobrescribir `motivoResolucion` y `fechaResolucion`, crearía una tabla `HistorialResolucion` que conserve el registro completo de cada cierre y reapertura.
- **Sistema de notificaciones real para `toast-mensaje`:** hoy es un placeholder estático. Lo conectaría a un store de notificaciones que muestre mensajes de éxito/error tras cada operación.
- **`docker-compose.yml`:** para levantar el proyecto con un solo comando (`docker compose up -d --build`), como sugiere la especificación.