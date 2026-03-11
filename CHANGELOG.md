# CHANGELOG

## MVP3 · Calendar Sync ICS

### Dominio y persistencia
- Nuevas entidades:
  - `ExternalChannelConnection`
  - `ExternalCalendarEvent`
  - `ExternalSyncLog`
- Nuevos enums:
  - `ExternalChannelType`
  - `ExternalSyncStatus`
- Nuevas configuraciones EF Core para conexiones, eventos y logs.
- Nueva migración `AddExternalCalendarSync`.

### Application / CQRS
- Alta, edición, listado y desactivación lógica de conexiones externas.
- Consulta de eventos externos por rango.
- Consulta de logs de sincronización.
- Acción de sincronización manual por conexión.
- Exportación de calendario ICS por unidad con token.
- Disponibilidad actualizada para contemplar eventos externos importados.
- Validación anti-overbooking al crear/editar reservas.
- Cotizador actualizado para no ofrecer unidades ocupadas por eventos externos.

### API
- Nuevo controller `ExternalCalendarsController`.
- Nuevo controller público `PublicCalendarsController` para exportación ICS por token.

### Infraestructura
- Servicio `IcsCalendarService` para descarga, parseo y generación de ICS.
- Servicio `ExternalCalendarSyncService` para importar, actualizar y cancelar eventos externos.

### Web / Blazor
- Nueva pantalla `/channel-sync/{propertyId}`.
- Acciones de guardar conexión, sincronizar, desactivar y copiar URL exportable.
- Nuevo item de menú: `Sync canales`.
- Calendario operativo con visualización diferenciada de:
  - reservas internas
  - bloqueos manuales
  - Booking externo
  - Airbnb externo

### Tests
- Parser ICS básico.
- Cotización sin disponibilidad por evento externo.
- Bloqueo de alta de reserva cuando hay evento externo solapado.
