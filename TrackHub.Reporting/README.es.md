# TrackHub Reporting API

[← Volver a la página principal](README.md) · [English](README.en.md)

La Reporting API convierte los datos de la plataforma en archivos. Es **solo REST** (.NET 10 Minimal APIs) y **no tiene base de datos propia**: cada dataset se compone a partir de los servicios que son dueños de los datos.

---

## Qué hace

- Ofrece un **catálogo gobernado de 30 reportes** en seis categorías: Operations, GPS, Documents, Workforce, Trips y Administration
- Renderiza cada reporte como una **vista previa** dentro del portal, una exportación a **Excel** o — cuando el catálogo lo permite — un **PDF**
- Filtra cada reporte según las features, el rol y la visibilidad de grupo del llamador
- Compone sus datasets a partir de las APIs de Management, Router, Telemetry, Geofencing y Trip Management

Detalle completo, incluyendo el catálogo completo: **[Reporting](https://github.com/shernandezp/TrackHub/wiki/Reporting)** en el wiki.

---

## Inicio rápido

### Requisitos previos

- .NET 10 SDK
- Un TrackHub AuthorityServer y una Management API en ejecución (allí vive el catálogo)
- Los demás servicios productores accesibles para los reportes que se planee ejecutar
- Los paquetes `TrackHubCommon.*` disponibles desde un feed local de NuGet

### Pasos

1. **Clonar**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.Reporting.git
   cd TrackHub.Reporting
   ```

2. **Configurar los endpoints de los productores y los límites** en `src/Web/appsettings.json`:

   ```json
   {
     "AppSettings": {
       "GraphQLManagerService": "https://localhost:5001/graphql",
       "GraphQLRouterService": "https://localhost:5003/graphql",
       "GraphQLTelemetryService": "https://localhost:5011/graphql",
       "GraphQLGeofenceService": "https://localhost:5004/graphql",
       "GraphQLTripManagementService": "https://localhost:5006/graphql",
       "Reporting": {
         "MaxExportRows": 100000,
         "MaxPdfRows": 500,
         "PreviewRows": 100
       }
     }
   }
   ```

3. **Ejecutar**

   ```bash
   dotnet run --project src/Web
   ```

4. **Invocar un reporte** con un token bearer:

   ```bash
   curl -X GET "https://localhost:<port>/api/BasicReports/live-report" \
     -H "Authorization: Bearer {your_token}" \
     -o live_report.xlsx
   ```

---

## Notas específicas del proyecto

- **El catálogo vive en la Management API, no aquí.** Cada reporte tiene una fila en `app.reports` que lleva `Category`, `RequiredFeatureKey`, `ManagerOnly`, `SupportsPdf`, `SortOrder` y `Active`. Agregar un reporte implica agregar **tanto** la implementación de `IReport` aquí **como** la fila de catálogo sembrada allá.
- **La gobernanza se aplica dos veces.** El `getReports` de Manager filtra la lista que muestra el portal; este servicio vuelve a aplicar los mismos metadatos en tiempo de ejecución vía `reportByCode` (cacheado 60 s). Un reporte oculto invocado directamente por código recibe 403 o 404 — el filtro del portal es una comodidad, no el control.
- **El catálogo se resiembra en cada arranque de Manager**, por lo que las ediciones de metadatos sembrados hechas desde la UI de administración se revierten. Solo `Active` persiste. Eso es intencional.
- **El PDF rechaza un dataset que exceda el límite; nunca trunca.** `MaxPdfRows` es 500 por defecto, y solo las filas de catálogo marcadas `SupportsPdf` ofrecen PDF. El techo de Excel es `MaxExportRows` (100 000).
- **Los encabezados de columna se resuelven por el nombre de propiedad del VM** a través del ResourceManager de `Resources` — **renombrar una propiedad del VM requiere renombrar las claves resx correspondientes**, o el encabezado recae en el nombre crudo de la propiedad.
- **Vista previa, Excel y PDF se renderizan todos a partir de un mismo `ReportDataset`** (cada `IReport` provee `GetDatasetAsync`), por lo que no hay una ruta de datos distinta por formato que pueda desalinearse.
- **Los clientes de Router, Geofence y Telemetry de este servicio están registrados `WithRetry`** — son de solo consulta, así que un reintento es seguro. Todos los demás clientes de la plataforma usan `NoRetry` por defecto, porque GraphQL siempre es POST y una mutación reintentada es una mutación duplicada.
- Los feeds paginados de origen se drenan a 500 filas por página con un tope defensivo de 100 000 filas.
- **Este servicio no aloja ningún servidor GraphQL**, por lo que el endurecimiento GraphQL de la plataforma (profundidad máxima de ejecución, detalle de error solo en desarrollo) no le aplica.
- Las exportaciones a PDF obtienen el branding de la cuenta desde Manager (caché de 60 s, tolerante a fallos — un error de branding nunca hace fallar una exportación). El nombre de la cuenta se renderiza; los bytes del logo aún no se incrustan.

---

## Documentación

- **Técnica** — el [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Reporting](https://github.com/shernandezp/TrackHub/wiki/Reporting), [Manager](https://github.com/shernandezp/TrackHub/wiki/Manager#report-catalog), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication)
- **De usuario** — en la app: el botón de Ayuda o **F1** en cualquier pantalla
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
