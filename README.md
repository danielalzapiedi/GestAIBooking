# GestAI Booking MVP3 · Sincronización iCal / ICS

## Qué se agregó
- Sincronización de calendarios externos por unidad vía iCal / ICS.
- Conexiones externas para Booking, Airbnb u otros canales.
- Importación manual de eventos externos y persistencia en base de datos.
- Bloqueo real de disponibilidad usando reservas externas importadas.
- Exportación de calendario ICS por unidad mediante URL con token.
- Logs de sincronización y trazabilidad base.
- Visualización de eventos externos dentro del calendario operativo.
- Tests base para parser ICS y prevención de overbooking con eventos externos.

## Flujo funcional de esta versión
1. Ir a **Gestión > Sync canales**.
2. Seleccionar una unidad activa.
3. Cargar el tipo de canal y la URL ICS de Booking/Airbnb.
4. Guardar la conexión.
5. Ejecutar **Sincronizar** para importar eventos externos.
6. Copiar la **URL exportable** y pegarla en Booking/Airbnb como calendario del PMS.

## Cómo configurar una conexión Booking / Airbnb por URL ICS
- Desde Booking/Airbnb obtener la URL de exportación del calendario de la publicación.
- En GestAI ir a la pantalla **Sync canales**.
- Crear una conexión indicando:
  - unidad interna
  - canal
  - nombre visible
  - URL ICS a importar
- Guardar y luego ejecutar sync manual.

## Cómo ejecutar sync manual
- Desde la grilla de conexiones usar el botón **Sincronizar**.
- El sistema descarga el ICS, procesa los `VEVENT`, actualiza eventos importados y registra el resultado en logs.
- El último estado queda visible en la conexión y en la tabla de logs.

## Cómo usar la URL exportable del sistema
- En la misma pantalla usar **Copiar URL**.
- Esa URL expone el calendario ICS de la unidad usando un token no trivial.
- Se puede pegar en Booking o Airbnb como calendario importado desde el PMS.

## Consideraciones de esta primera versión
- No es un channel manager enterprise.
- No modifica precios ni disponibilidad por APIs oficiales de OTA.
- No sincroniza mensajes, huéspedes ni contenido comercial del anuncio.
- La sincronización implementada en esta etapa es **manual**.
- Los eventos importados impactan disponibilidad como bloqueos externos, no como reservas internas editables.
- La detección de eventos se basa en `UID` + hash de contenido.
- El parser implementa soporte sólido para `VEVENT`, `UID`, `DTSTART`, `DTEND`, `SUMMARY` y `STATUS` para escenarios habituales de Booking/Airbnb.

## Requisitos
- .NET 9 SDK
- SQL Server
- `dotnet-ef` si querés administrar migraciones manualmente

## Ejecución sugerida
1. `dotnet restore GestAI.sln`
2. `dotnet build GestAI.sln`
3. `dotnet test GestAI.sln`
4. `dotnet ef database update --project GestAI.Infrastructure.Persistence --startup-project GestAI.Api`
5. `dotnet run --project GestAI.Api`
6. `dotnet run --project GestAI.Web`

## Nota de entrega
En este entorno no tuve disponible el SDK de .NET para ejecutar `dotnet restore / build / test`, así que dejé todo integrado y preparado, pero la verificación final de compilación y ejecución queda pendiente de correrse en una máquina con .NET 9 instalado.
