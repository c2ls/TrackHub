# API de Gestión de Viajes de TrackHub

## Características Principales

- **Gestión del Ciclo de Vida del Viaje**: Máquina de estados gobernada (`Created → InProgress → Paused → Completed / Cancelled / Aborted`) con una única matriz de transiciones como fuente de verdad
- **Planificación de Paradas y Entregas**: Paradas ordenadas con operaciones de alta/actualización/eliminación/reordenamiento, progreso de llegada y salida, omisión de paradas y resultados de entrega por parada
- **Planificación de Rutas**: Geometría de ruta y corredores de tolerancia calculados mediante OpenRouteService y almacenados como geometrías PostGIS
- **Estimación de Peajes**: Catálogo de estaciones de peaje, tarifas y clases de vehículo con estimación de costo basada en la ruta e informe explícito de cobertura parcial
- **Prueba de Entrega**: Captura de firmas, fotografías y documentos vinculados al servicio de documentos con validación de escaneo limpio
- **Enlaces Públicos de Seguimiento**: Endpoint REST anónimo, revocable y con limitación de tasa para compartir el avance de un viaje con un destinatario
- **Interfaz GraphQL**: Consultas eficientes y flexibles con servidor GraphQL Hot Chocolate
- **Arquitectura Limpia**: Arquitectura en capas que asegura mantenibilidad y capacidad de prueba
- **PostgreSQL + PostGIS**: Capacidades de base de datos espacial de nivel empresarial usando geometría NetTopologySuite (SRID 4326)

---

## Inicio Rápido

### Requisitos Previos

- .NET 10.0 SDK
- PostgreSQL 14+ con extensión PostGIS habilitada
- TrackHub Authority Server ejecutándose (para autenticación)
- APIs de Manager y Telemetry accesibles (datos maestros, posiciones, alertas, concesiones de enlaces públicos)
- Una clave de API de OpenRouteService (para planificación de rutas)

### Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/shernandezp/TrackHub.TripManagement.git
   cd TrackHub.TripManagement
   ```

2. **Habilitar extensión PostGIS** en PostgreSQL:
   ```sql
   CREATE EXTENSION postgis;
   ```

3. **Configurar la conexión a la base de datos** en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;user id=postgres;password=yourpassword;database=TrackHub;port=5432"
     }
   }
   ```

4. **Configurar el proveedor de rutas** en `appsettings.json`:
   ```json
   {
     "AppSettings": {
       "Routing": {
         "Provider": "OpenRouteService",
         "BaseUrl": "https://api.openrouteservice.org",
         "ApiKey": "your-api-key",
         "Profile": "driving-hgv"
       }
     }
   }
   ```

5. **Ejecutar las migraciones de la base de datos**:
   ```bash
   dotnet ef database update
   ```

6. **Iniciar la aplicación**:
   ```bash
   dotnet run --project src/Web
   ```

7. **Acceder al Playground GraphQL** en `https://localhost:5006/graphql`

---

## Componentes y Recursos Utilizados

| Componente                | Descripción                                             | Documentación                                                                 |
|---------------------------|---------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate             | Servidor GraphQL para .Net        | [Documentación Hot Chocolate](https://chillicream.com/docs/hotchocolate/v13)                           |
| .NET Core                 | Development platform for modern applications          | [.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |
| Postgres                  | Sistema de gestión de bases de datos relacional         | [Documentación Postgres](https://www.postgresql.org/)                         |
| OpenRouteService          | Proveedor de rutas usado para geometría de ruta y ETAs   | [Documentación OpenRouteService](https://openrouteservice.org/dev/#/api-docs) |

---

## Descripción General

La **API de Gestión de Viajes de TrackHub** proporciona servicios para planificar, despachar y hacer seguimiento de viajes. Sigue los principios de la **Arquitectura Limpia** del proyecto, aprovechando **GraphQL** para las interacciones de la API y **Postgres** para la gestión de la base de datos. Sus tablas residen en el esquema `trip` de la base de datos compartida `TrackHub` y sus capacidades están habilitadas mediante la característica de cuenta `trip-management`.

### Características Principales

La API ofrece las siguientes funcionalidades:
- Planificación de viajes con paradas ordenadas, entregas y asignaciones de conductor/transportador.
- Planificación de rutas y generación de corredores mediante un proveedor de rutas externo.
- Administración del catálogo de peajes y estimación de costos de peaje basada en la ruta.
- Detección de llegadas, salidas, retrasos y desviaciones de ruta a partir de posiciones de telemetría.
- Captura de pruebas de entrega y publicación de enlaces públicos de seguimiento de solo lectura.
- Provisión de los conjuntos de datos que respaldan los informes de viajes servidos por la API de Reporting.

---

## Entidades

### Gestión de Viajes

- **Trip**: La unidad de despacho, con su código, referencia externa, programación, estado y la cuenta a la que pertenece.
- **TripStop**: Una parada ordenada del viaje con su ubicación, ventana planificada, ETA y estado de progresión (`Pending`, `Arrived`, `Departed`, `Skipped`).
- **Delivery**: Una entrega asociada a una parada, con su resultado (`Pending`, `Delivered`, `PartiallyDelivered`, `Rejected`).
- **ProofOfDelivery**: La evidencia de firma, fotografía o documento registrada al cerrar una entrega.
- **TripAssignment**: Vincula un viaje con el conductor y el transportador que lo ejecutan, con estado `Active`, `Ended` o `Cancelled`.
- **RoutePlan**: La geometría de ruta planificada y su corredor para un viaje, producida por un proveedor (`OpenRouteService` o `Manual`) y almacenada como geometría PostGIS.
- **TripEvent**: El registro incremental de todo lo ocurrido en un viaje, indicando el origen (`Portal`, `Driver`, `Detection`, `Job`, `ServiceClient`).
- **TripShare**: Un enlace público de seguimiento revocable para un viaje, respaldado por una concesión de enlace público en la API de Manager.
- **TripDocument**: Un documento adjunto a un viaje o entrega (firma, fotografía, manifiesto, carta de porte, recibo).
- **TollStation**, **TollTariff**, **TollVehicleClass**, **TransporterTollClass**: El catálogo de peajes y la asignación de clase por transportador utilizada para la estimación de costos.
- **VwUser**, **VwVisibleTransporter**: Vistas utilizadas para limitar los datos de viajes a la cuenta y los grupos del usuario que llama.

---

## Operaciones GraphQL

### Mutaciones

- **createTrip**, **updateTrip**, **deleteTrip**: CRUD de viajes.
- **assignTrip**: Asigna un conductor y un transportador a un viaje.
- **planTripRoute**: Solicita un plan de ruta al proveedor de rutas y almacena su geometría y corredor.
- **startTrip**, **pauseTrip**, **resumeTrip**, **completeTrip**, **cancelTrip**, **abortTrip**: Transiciones de ciclo de vida, validadas contra la matriz de transiciones.
- **addTripStop**, **updateTripStop**, **removeTripStop**, **reorderTripStops**: Planificación de paradas.
- **recordStopArrival**, **recordStopDeparture**, **skipStop**: Progresión de paradas.
- **createDelivery**, **updateDelivery**, **updateDeliveryOutcome**, **deleteDelivery**: Gestión de entregas.
- **recordProofOfDelivery**: Registra la evidencia de prueba de entrega de una entrega.
- **shareTrip**, **revokeTripShare**: Emite y revoca enlaces públicos de seguimiento.
- **processTripPositions**: Procesa posiciones de transportadores para detectar llegadas, salidas, retrasos y desviaciones del corredor.
- **importTrips**, **updateTripStatus**: Puntos de entrada de integración para sistemas de despacho externos.
- **createTollVehicleClass**, **updateTollVehicleClass**, **deactivateTollVehicleClass**, **createTollStation**, **updateTollStation**, **deactivateTollStation**, **createTollTariff**, **updateTollTariff**, **deleteTollTariff**, **importTollCatalog**, **setTransporterTollClass**: Administración del catálogo de peajes.

### Consultas

- **trips**: Listado paginado de viajes de la cuenta del usuario que llama, con filtros.
- **tripDetail**: Un viaje individual con sus paradas, entregas, asignación y plan de ruta.
- **activeTrips**: Viajes actualmente en curso, para el mapa en vivo.
- **tripTimeline**: Registro de eventos paginado de un viaje.
- **tripRouteReplay**: Ruta planificada y posiciones registradas para reproducción.
- **tripReportData**, **tripStopReportData**, **tripTollReportData**, **tripPodReportData**: Conjuntos de datos paginados consumidos por la API de Reporting.
- **tollStations**, **tollStationDetail**, **tollVehicleClasses**, **transporterTollClasses**: Lecturas del catálogo de peajes.
- **estimateTolls**: Estima el costo de peajes sobre una ruta para una clase de vehículo dada.

### Endpoints REST

- **GET `~/public/trips/{publicLinkGrantId}`**: Endpoint público de seguimiento anónimo. Limitado por IP del cliente y deliberadamente sin caché para que las revocaciones surtan efecto de inmediato y cada resolución quede auditada.
- **GET `/health`**: Sonda de salud, incluyendo verificación del contexto de base de datos.

---

## Servicios en Segundo Plano

| Servicio                      | Clave de Trabajo         | Intervalo   | Propósito                                                                 |
|-------------------------------|--------------------------|-------------|---------------------------------------------------------------------------|
| `TripEtaRefreshService`       | `trip-eta-refresh`       | 5 minutos   | Recalcula los ETA de las paradas de viajes en curso y genera eventos de retraso |
| `TripScheduleReminderService` | `trip-schedule-reminder` | 15 minutos  | Señala los viajes cuyo inicio programado ya venció pero que aún no han comenzado |

Ambos trabajos registran una ejecución solo cuando realizaron trabajo, por lo que un registro antiguo para estas claves es el estado saludable esperado y no un trabajo atascado.

---

## Configuración

| Clave                                       | Propósito                                                    |
|---------------------------------------------|--------------------------------------------------------------|
| `ConnectionStrings:DefaultConnection`       | Conexión PostgreSQL para el esquema `trip`                   |
| `AuthorityServer:ClientId` (`trip_client`)  | Cliente de servicio usado para llamadas entre servicios      |
| `AppSettings:GraphQLIdentityService`        | Endpoint GraphQL de la API de Security                       |
| `AppSettings:GraphQLManagerService`         | Endpoint GraphQL de la API de Manager                        |
| `AppSettings:GraphQLTelemetryService`       | Endpoint GraphQL de la API de Telemetry                      |
| `AppSettings:Routing`                       | Configuración del proveedor de rutas (proveedor, URL base, clave de API, perfil, límite de tasa, tiempo de espera, máximo de waypoints) |
| `AllowedCorsOrigins`                        | Orígenes autorizados para llamar la API desde el navegador   |

El portal accede a este servicio mediante `REACT_APP_TRIPMANAGEMENT_ENDPOINT`; los demás servicios backend lo hacen mediante `AppSettings:GraphQLTripManagementService`. En desarrollo el servicio escucha en `https://localhost:5006` y `http://localhost:5007`.

### ¿Por qué GraphQL?

El uso de **GraphQL** permite consultas eficientes y personalizables, permitiendo a los clientes solicitar solo los datos que necesitan para minimizar el ancho de banda y mejorar el rendimiento de la aplicación. Con GraphQL, las aplicaciones pueden recuperar detalles específicos sobre viajes, paradas, entregas o peajes, optimizando tanto la eficiencia operativa como la experiencia del usuario.

## Licencia

Este proyecto está bajo la Licencia Apache 2.0. Consulta el archivo [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
