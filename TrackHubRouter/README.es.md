# API de Router de TrackHub

[← Volver a la página principal](README.md) · [English](README.en.md)

El Router es la **capa de integración multiprotocolo** de TrackHub. Se conecta a proveedores externos de rastreo GPS — cada uno con su propia tecnología de API — y estandariza sus datos en un formato unificado que el resto de la plataforma consume.

Este repositorio también contiene el **SyncWorker**, el host en segundo plano que impulsa la sincronización.

Construido sobre .NET 10 con un endpoint GraphQL de HotChocolate más una superficie REST para integración de terceros.

---

## Qué hace

- **Integración multiproveedor** — nueve proveedores GPS implementados (CommandTrack, Traccar, Flespi, GeoTab, GpsGate, Navixy, Samsara, Wialon, Protrack)
- **Normalización de datos** — las cargas útiles de los proveedores se convierten en una única forma `PositionVm`: velocidad en km/h, grados decimales, marcas de tiempo UTC
- **Obtención de posiciones en tiempo real e histórica** en todos los operadores conectados
- **Sincronización en segundo plano** — los ciclos de posición, dispositivo y salud del SyncWorker
- **Sincronización manual y ping de conectividad** como funcionalidades de usuario autorizadas y con límite de tasa
- **Declaración de capacidades del proveedor** — una solicitud de algo que un proveedor no puede hacer falla como una limitación del *proveedor*, no como un error de TrackHub

El Router **no tiene base de datos propia**. Los datos maestros pasan por la [API de Gestión](https://github.com/shernandezp/TrackHub.Manager); las posiciones, el historial, las verificaciones de salud y las ejecuciones de sincronización van a la [API de Telemetry](https://github.com/shernandezp/TrackHub.Telemetry); los flujos de detección van a [Geofencing](https://github.com/shernandezp/TrackHub.Geofencing) y [Trip Management](https://github.com/shernandezp/TrackHub.TripManagement).

Detalle completo: **[Router](https://github.com/shernandezp/TrackHub/wiki/Router)** en la wiki.

---

## Inicio rápido

### Requisitos previos

- SDK de .NET 10
- Un TrackHub AuthorityServer, API de Gestión y API de Telemetry en ejecución
- Al menos una cuenta de proveedor GPS (Traccar y CommandTrack son los probados)
- Los paquetes `TrackHubCommon.*` disponibles desde un feed local de NuGet

### Pasos

1. **Clonar**

   ```bash
   git clone https://github.com/shernandezp/TrackHubRouter.git
   cd TrackHubRouter
   ```

2. **Configurar los servicios subyacentes y los protocolos habilitados** en `src/Web/appsettings.json` (y `src/SyncWorker/appsettings.json`):

   ```json
   {
     "AppSettings": {
       "GraphQLManagerService": "https://localhost:5001/graphql",
       "GraphQLTelemetryService": "https://localhost:5011/graphql",
       "GraphQLGeofenceService": "https://localhost:5004/graphql",
       "GraphQLTripManagementService": "https://localhost:5006/graphql",
       "Protocols": [ "CommandTrack", "Traccar" ],
       "MaxConcurrentOperatorSyncs": 10,
       "DeviceCatalogCacheSeconds": 60
     }
   }
   ```

3. **Configurar las credenciales del operador** a través de la API de Gestión o el portal web — el Router las lee, no las almacena.

4. **Ejecutar la API**

   ```bash
   dotnet run --project src/Web
   ```

5. **Ejecutar el worker en segundo plano** (opcional, en una segunda terminal)

   ```bash
   dotnet run --project src/SyncWorker
   ```

6. **Abrir** el endpoint GraphQL en `https://localhost:<port>/graphql` y la referencia REST en `https://localhost:<port>/scalar`.

---

## Proveedores compatibles

| Proveedor | Documentación | Estado | Probado |
|---|---|---|---|
| CommandTrack | [Docs](https://www.c2ls.co/home/documentacion-de-la-api/) | ✅ Implementado | ✅ |
| Traccar | [Docs](https://www.traccar.org/api-reference/) | ✅ Implementado | ✅ |
| Flespi | [Docs](https://flespi.io/docs/) | ✅ Implementado | ❌ |
| GeoTab | [Docs](https://developers.geotab.com/myGeotab/guides/codeBase/usingInDotnet) | ✅ Implementado | ❌ |
| GpsGate | [Docs](https://support.gpsgate.com/hc/en-us/articles/360016602140-REST-API-Documentation) | ⚠️ Sin API de historial de posición | ❌ |
| Navixy | [Docs](https://www.navixy.com/docs/navixy-api/user-api/getting-started) | ✅ Implementado | ❌ |
| Samsara | [Docs](https://developers.samsara.com/docs/tms-integration) | ✅ Implementado | ❌ |
| Wialon | [Docs](https://help.wialon.com/en/api/user-guide) | ✅ Implementado | ❌ |
| Protrack | — | ✅ Implementado | ❌ |

`Mettax` está reservado en el enum `ProtocolType` sin un ensamblado de proveedor — configurarlo lanza una excepción al iniciar.

**Agregar un proveedor**: el recorrido completo está en [Adding a Provider](https://github.com/shernandezp/TrackHub/wiki/Adding-a-Provider) en la wiki.

---

## Notas específicas del proyecto

- **Un proveedor mal configurado falla al iniciar, a propósito.** Un protocolo configurado que no resuelve ningún ensamblado, ningún tipo de lector, o cuya declaración de capacidades no coincide con sus lectores, lanza una excepción durante el arranque en lugar de omitirse silenciosamente.
- **Tres cosas deben alinearse al agregar un proveedor**: el valor del enum `ProtocolType` (en TrackHubCommon), el ensamblado del proveedor (espacio de nombres `TrackHub.Router.Infrastructure.{Protocol}`, lectores con propiedades `Protocol` coincidentes y un `ProviderDescriptor` que declara nombre visible y capacidades), y la entrada `AppSettings:Protocols` (**tanto en Web como en SyncWorker**). La matriz de capacidades y la lista de proveedores expuesta a los clientes se construyen solas a partir de los descriptores descubiertos.
- **`Ping` siempre debe realizar un round-trip real al proveedor.** Las sesiones se almacenan en caché en `IProviderSessionStore`, por lo que un ping que solo hiciera `Init` sería un simple acierto en memoria — convirtiendo el monitor de salud en un no-operativo que reporta HEALTHY mientras el proveedor está caído.
- **Los errores del proveedor deben lanzar una excepción, nunca degradar a resultados vacíos.** Un error silenciado se convierte en una sincronización "exitosa" de 0 posiciones, lo que detiene silenciosamente el flujo de posiciones mientras Health permanece en verde. Wialon y Navixy reportan fallas como HTTP 200 con una carga útil de error — ambos casos se detectan y lanzan la excepción.
- **El SyncWorker está diseñado para una sola instancia.** `IOperatorSyncLock`, `ExecutionIntervalManager` e `IProviderSessionStore` son todos en proceso. Escalarlo horizontalmente requiere una reivindicación entre instancias (un bloqueo consultivo de PostgreSQL o `SKIP LOCKED`).
- **El alcance del tenant sigue al llamador; las credenciales se leen con la identidad del servicio.** Las superficies interactivas resuelven el operador a través de `IOperatorReader` (propagando el token del llamador, de modo que Manager aplica su visibilidad), y luego leen la credencial descifrada a través de `IOperatorSystemReader`. Nunca ampliar los permisos de un usuario para obtener material de credenciales.
- **Los `HttpClient` de proveedor deshabilitan la redirección automática** — una URL base configurada por el operador no debe poder redirigir con 302 al Router hacia adentro.
- `AttributesVm.Hourmeter` está en **horas**; `Mileage` **no** tiene conversión en el límite del mapper, así que no asumir metros o kilómetros sin verificar el proveedor.
- Después de cambiar cualquier superficie GraphQL, ejecutar las pruebas de contrato:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentación

- **Técnica** — la [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Router](https://github.com/shernandezp/TrackHub/wiki/Router), [Adding a Provider](https://github.com/shernandezp/TrackHub/wiki/Adding-a-Provider), [Telemetry](https://github.com/shernandezp/TrackHub/wiki/Telemetry), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication)
- **De usuario** — en la app: el botón de Ayuda o **F1** en cualquier pantalla
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
