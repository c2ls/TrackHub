# TrackHub Geofencing API

[← Volver a la página principal](README.md) · [English](README.en.md)

La Geofencing API es la propietaria de las zonas geográficas y de los eventos de visita que los transportadores generan sobre ellas. Evalúa las posiciones enviadas por el [Router](https://github.com/shernandezp/TrackHubRouter) mediante consultas espaciales de PostGIS y emite eventos de alerta a través de la [Management API](https://github.com/shernandezp/TrackHub.Manager).

Construida sobre .NET 10 con un endpoint GraphQL de HotChocolate y geometría de NetTopologySuite. Habilitada mediante la característica de cuenta `geofencing`.

---

## Qué gestiona

| Tabla | Propósito |
|---|---|
| `geofencing.geofences` | Definiciones de zonas — geometría de polígono, metadatos de círculo, opciones de alerta habilitadas, umbral de permanencia |
| `geofencing.geofenceevents` | Una fila por **visita**: una marca de tiempo de entrada más una salida anulable |

Vale la pena conocer de antemano dos comportamientos:

- **Un círculo se almacena como metadatos más un polígono con buffer.** `CircleCenter` y `CircleRadiusMeters` coexisten junto a `Geom`, que siempre contiene un polígono (un buffer de 64 segmentos calculado en el momento de la escritura). Por lo tanto, la detección, la indexación, los reportes y las superposiciones son agnósticas a la forma; los editores renderizan el círculo real a partir de los metadatos.
- **Una fila es una visita**, no un evento. La entrada y la salida comparten fila, con un antirrebote de salida de 30 s y una protección de idempotencia.

Detalle completo: **[Geofencing](https://github.com/shernandezp/TrackHub/wiki/Geofencing)** en la wiki.

---

## Inicio rápido

### Requisitos previos

- .NET 10 SDK
- PostgreSQL 14+ **con PostGIS habilitado**
- Una instancia de TrackHub AuthorityServer y Management API en ejecución
- Los paquetes `TrackHubCommon.*` disponibles desde un feed local de NuGet

### Pasos

1. **Clonar**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.Geofencing.git
   cd TrackHub.Geofencing
   ```

2. **Habilitar PostGIS** en la base de datos `TrackHub` (un solo `CREATE EXTENSION` cubre también el esquema `trip`, ya que comparten la base de datos):

   ```sql
   CREATE EXTENSION IF NOT EXISTS postgis;
   ```

3. **Configurar la conexión a la base de datos** en `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     }
   }
   ```

4. **Aplicar las migraciones** — esto crea el esquema `geofencing`:

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

5. **Ejecutar**

   ```bash
   dotnet run --project src/Web
   ```

6. **Abrir el endpoint de GraphQL** en `https://localhost:<port>/graphql`.

---

## Notas específicas del proyecto

- **Nunca agregar `[Caching]` a `geofencesByAccount`.** La clave de caché se construye únicamente a partir de propiedades de la solicitud y no puede delimitarse a la cuenta del llamador, por lo que una respuesta en caché filtra geocercas entre tenants. El input `enableCaching` de la consulta está deliberadamente inerte. Este es el caso que estableció la regla a nivel de toda la plataforma.
- **Una geocerca con visitas registradas no puede eliminarse** — el intento devuelve `ConflictException` → `CONFLICT`. El historial de visitas es permanente; la desactivación es la forma soportada de retirar una zona, y una geocerca desactivada conserva su historial legible.
- **La emisión de alertas usa la identidad propia de este servicio, `geofence_client`** (`CreateClient(Clients.Manager, asService: true)`), nunca el token propagado del llamador. Esa identidad necesita `Alerts/Write` y `BackgroundJobs/Write` sembrados en `security.service_client_permissions`, o cada emisión devolverá `FORBIDDEN`.
- **La emisión es best-effort.** Un fallo se registra en el log y nunca detiene el procesamiento de posiciones: una interrupción de Manager no debe detener la detección ni perder posiciones.
- **El evaluador de permanencia marca `DwellAlertedAt` solo después de una emisión exitosa**, lo cual es seguro ante reintentos gracias a la deduplicación de Manager. Es un registrador de "solo cuando hay trabajo": una marca de tiempo antigua en `BackgroundJobRun` es el estado estable saludable.
- **La geometría se valida, no se degrada silenciosamente.** Los centros de círculo más allá de ±85° de latitud, los anillos de círculo que cruzan el meridiano de ±180°, los polígonos autointersecantes y las coordenadas fuera de rango devuelven todos un 400 desde `GeofenceDtoValidator`. PostGIS plano más posiciones normalizadas detectarían mal el caso del meridiano, por lo que se rechaza en lugar de aproximarse.
- **El historial de eventos de geocerca no es una consulta del portal.** Se sirve a través del reporte *Geofence Events* de la pantalla de Reportes; el portal no incluye un documento `geofenceEvents`, porque Reporting es el consumidor de esa consulta.
- Las tablas `app.accounts`, `app.account_features` y `app.audit_events` se mapean como **solo lectura** y quedan excluidas de las migraciones de este servicio.
- Después de cambiar cualquier superficie de GraphQL, ejecutar las pruebas de contrato:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentación

- **Técnica** — la [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Geofencing](https://github.com/shernandezp/TrackHub/wiki/Geofencing), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication)
- **Usuario** — en la aplicación: el botón de ayuda o **F1** en cualquier pantalla
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
