# TrackHub Trip Management API

[← Volver a la página principal](README.md) · [English](README.en.md)

La Trip Management API planifica, despacha y hace seguimiento de viajes: paradas ordenadas, entregas, geometría de ruta con un corredor de desviación, estimación de peajes, prueba de entrega y enlaces públicos de seguimiento compartibles.

Construida sobre .NET 10 con un endpoint GraphQL de HotChocolate y geometría NetTopologySuite en SRID 4326. Es dueña del esquema **`trip`** en la base de datos compartida `TrackHub`, y toda su superficie multi-tenant está condicionada por la característica de cuenta `trip-management`.

---

## Qué hace

- **Ciclo de vida del viaje** — una máquina de estados gobernada (`Created → InProgress → Paused → Completed / Cancelled / Aborted`) con una única matriz de transiciones como fuente de verdad
- **Planificación de paradas y entregas** — paradas ordenadas con alta/actualización/eliminación/reordenamiento, progreso de llegada y salida, manejo de omisiones y resultados de entrega por parada
- **Planificación de rutas** — geometría de ruta y corredores de tolerancia a partir de OpenRouteService, almacenados como geometría PostGIS
- **Estimación de peajes** — un catálogo de estaciones de peaje, tarifas y clases de vehículo con estimación de costo basada en la ruta e informe explícito de cobertura parcial
- **Detección** — llegadas a paradas, salidas, retrasos y desviaciones del corredor, calculados a partir de posiciones de telemetría
- **Prueba de entrega** — captura de firma, fotografía y documento vinculada al servicio de documentos, con validación de escaneo limpio
- **Enlaces públicos de seguimiento** — anónimos, revocables, con limitación de tasa y control de divulgación por enlace compartido

Detalle completo: **[Trip Management](https://github.com/shernandezp/TrackHub/wiki/Trip-Management)** en el wiki.

---

## Inicio rápido

### Requisitos previos

- .NET 10 SDK
- PostgreSQL 14+ **con PostGIS habilitado**
- Un TrackHub AuthorityServer, Management API y Telemetry API en ejecución
- Una clave de API de OpenRouteService, para la planificación de rutas

### Pasos

1. **Clonar**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.git
   cd TrackHub/TrackHub.TripManagement
   ```

2. **Habilitar PostGIS** en la base de datos `TrackHub` (un solo `CREATE EXTENSION` cubre también el esquema `geofencing`):

   ```sql
   CREATE EXTENSION IF NOT EXISTS postgis;
   ```

3. **Configurar la conexión a la base de datos y el proveedor de rutas** en `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     },
     "AppSettings": {
       "GraphQLManagerService": "https://localhost:5001/graphql",
       "GraphQLTelemetryService": "https://localhost:5011/graphql",
       "Routing": {
         "Provider": "OpenRouteService",
         "BaseUrl": "https://api.openrouteservice.org",
         "ApiKey": "your-api-key",
         "Profile": "driving-hgv"
       }
     }
   }
   ```

4. **Aplicar las migraciones** — esto crea el esquema `trip`:

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

5. **Ejecutar**

   ```bash
   dotnet run --project src/Web
   ```

6. **Abrir el endpoint GraphQL** en `https://localhost:5006/graphql`.

En desarrollo el servicio escucha en `https://localhost:5006` y `http://localhost:5007`; detrás de nginx se ubica en `/Trip/`.

---

## Notas específicas del proyecto

- **Volver a ejecutar `db-init` tras desplegar este servicio en una instalación existente.** La migración crea el esquema pero no siembra nada. Sin el registro de `trip_client` ni el sembrado de recursos y roles de `Trips` / `TripTracking` / `TollCatalog`, **toda llamada de viaje devuelve `FORBIDDEN` mientras el servicio se reporta saludable.**
- **Este servicio registra su propio `IFeatureFlagService`** (`Infrastructure/TripDB/DependencyInjection.cs`). El valor por defecto de Common es fail-**open** (`AlwaysEnabledFeatureFlagService`, `TryAddScoped`), así que eliminar ese registro deshabilitaría silenciosamente cada verificación `[RequireFeature]`.
- **`Resources.TollCatalog` deliberadamente no está condicionado por feature.** El catálogo de peajes es información de referencia de la plataforma mantenida por el Administrador, sin `AccountId`; condicionarlo a una feature de tenant lo clasificaría mal.
- **El estado de detección se persiste, nunca se mantiene en memoria.** El Router envía **una posición por transportador por llamada**, así que cualquier debounce o duración de ejecución mantenida en memoria nunca podría transcurrir. Una prueba que alimenta un escenario completo en una sola llamada no prueba nada sobre el comportamiento en producción — refleje la realidad de una posición por llamada.
- **La geometría de llegada se toma como instantánea en `StartTrip`.** El polígono de la geocerca vinculada se lee una sola vez, en `TripStop.ArrivalGeom`, así que editar esa geocerca en medio del viaje no puede mover el área de llegada de un viaje en curso. Esta lectura va directo a `geofencing.geofences` como una proyección de solo lectura — **no existe llamada de servicio de TripManagement hacia Geofencing.**
- **El endpoint público de seguimiento deliberadamente no tiene caché de salida.** Un acierto de caché nunca llegaría al resolver de Manager, por lo que no contaría el acceso ni escribiría el evento de auditoría `PublicLinkAccessed` — y seguiría sirviendo un enlace revocado. Además devuelve **404, no `FEATURE_DISABLED`**, cuando la feature está apagada: la página no debe revelar que el viaje existe.
- **Las banderas de divulgación fallan cerradas.** Lo que ve un destinatario público está determinado por las banderas de campo de `trip_shares`, que por defecto son todas `false` — nunca por filtrado del lado del cliente.
- **Las estimaciones de peaje son explicables, nunca subestimadas en silencio.** Una estación coincidente sin tarifa para la clase del viaje aporta 0 y establece `TollStatus = PartialNoTariff`; un catálogo vacío produce `NoStations` y una estimación **nula** en lugar de un número fabricado. Las tarifas son temporales, así que la estimación de un viaje histórico se mantiene explicable.
- **Un fallo de ORS degrada a `RoutePlan.Status = Failed` — nunca bloquea un comando de viaje.**
- Ambos trabajos en segundo plano (`trip-eta-refresh`, `trip-schedule-reminder`) son **registradores de solo cuando hay trabajo**: una marca de tiempo antigua en `BackgroundJobRun` es el estado saludable normal, no un trabajo atascado.
- Nota de rendimiento: el predicado de geografía `ST_DWithin(..., TRUE)` usado para la coincidencia de estaciones de peaje no puede usar el índice GiST de geometría.
- Después de cambiar cualquier superficie GraphQL, ejecutar las pruebas de contrato:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentación

- **Técnica** — el [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Trip Management](https://github.com/shernandezp/TrackHub/wiki/Trip-Management), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication), [Reporting](https://github.com/shernandezp/TrackHub/wiki/Reporting)
- **De usuario** — en la app: el botón de Ayuda o **F1** en cualquier pantalla
- **Despliegue** — [TrackHub.Deployment](../TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
