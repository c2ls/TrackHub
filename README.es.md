# API de Telemetría de TrackHub

[← Volver a la página principal](README.md) · [English](README.en.md)

La API de Telemetry gestiona los datos de **alto volumen y de solo anexado** de TrackHub. Almacena y sirve posiciones y telemetría de operadores; nunca se comunica con proveedores GPS — ese es el rol del [Router](https://github.com/shernandezp/TrackHubRouter).

Construida sobre .NET 10 con un endpoint GraphQL de HotChocolate, siguiendo las convenciones de Clean Architecture y CQRS de la plataforma.

---

## Qué gestiona

| Tabla | Propósito |
|---|---|
| `telemetry.transporter_position` | La proyección de última posición — la fijación más reciente por transportador, que respalda el mapa en vivo |
| `telemetry.transporter_position_history` | El almacén de trazas de solo anexado, deduplicado mediante una clave de idempotencia |
| `telemetry.operator_sync_runs` | Una fila por cada intento de sincronización de dispositivo o posición: conteos, resultado, error |
| `telemetry.operator_health_checks` | Resultados de las verificaciones de conectividad y salud del operador |

La salud del operador y los resúmenes de sincronización se **derivan en tiempo de lectura** a partir de las dos últimas tablas — la fila del operador no lleva columnas de resumen agregado.

Un trabajo programado alojado en el propio host (`PositionRetentionPurgeService`) elimina el historial vencido por cuenta, respetando los días de retención de cada cuenta.

Detalle completo: **[Telemetry](https://github.com/shernandezp/TrackHub/wiki/Telemetry)** en la wiki.

---

## Inicio rápido

### Requisitos previos

- SDK de .NET 10
- PostgreSQL 14+
- La base de datos `TrackHub` con el esquema `telemetry` **ya creado por las migraciones de Manager**
- Un TrackHub AuthorityServer en ejecución, para autenticación
- Los paquetes `TrackHubCommon.*` disponibles desde un feed local de NuGet

### Pasos

1. **Clonar**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.Telemetry.git
   cd TrackHub.Telemetry
   ```

2. **Configurar la conexión a la base de datos** en `src/Web/appsettings.json` — debe apuntar a la **misma** base de datos `TrackHub` que Manager:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Ejecutar**

   ```bash
   dotnet run --project src/Web
   ```

4. **Abrir el endpoint GraphQL** en `https://localhost:<port>/graphql`.

En producción, conectarse con un rol que tenga lectura/escritura sobre `telemetry` y **solo lectura** sobre las tablas de alcance del esquema `app`.

---

## Notas específicas del proyecto

- **Este servicio no tiene migraciones propias — no agregar ninguna.** Las tablas de `telemetry` son creadas y migradas por la [API de Gestión](https://github.com/shernandezp/TrackHub.Manager). Por eso `DB_CONNECTION_TELEMETRY` debe apuntar a la misma base de datos `TrackHub`. Agregar una columna de telemetry implica agregar una migración de *Manager*.
- **Las tablas del esquema `app` se mapean como solo lectura** y se excluyen de las migraciones. Existen para que el servicio pueda aplicar el alcance por cuenta y la visibilidad de grupo sin un salto de red por solicitud: usuarios, grupos, vínculos usuario–grupo y transportador–grupo, transportadores, asignaciones de dispositivos, dispositivos, operadores y funcionalidades de cuenta.
- **`attributes` es una columna `json` de PostgreSQL.** `json` no tiene operador de igualdad, por lo que `Distinct()`, `GroupBy()` u operaciones de conjunto sobre una entidad o proyección que la incluya fallan en tiempo de ejecución con `42883`. Deduplicar con `EXISTS` o predicados basados en clave — **EF InMemory no detectará esto.**
- **`PlatformSyncActivityReader` es deliberadamente sin alcance de cuenta.** Es una lectura documentada de ámbito de plataforma, protegida por `[Authorize(Administrative, Read)]`, que devuelve solo marcas de tiempo y conteos, nunca un id de cuenta. Respalda el mosaico de SyncWorker de la página pública de estado.
- **Control de funcionalidades**: las escrituras de última posición y de salud son básicas (solo autorización); las escrituras de historial y las lecturas de repetición están controladas por `gps.positionHistory`.
- **Visibilidad**: los roles Administrator y Manager leen a nivel de toda la cuenta; los demás usuarios quedan acotados a su membresía de grupo. Los clientes de servicio leen en nombre de usuarios ya autorizados.
- La purga de retención es un **registrador que solo actúa cuando hay trabajo** — una marca de tiempo antigua de `BackgroundJobRun` para ella es el estado estable saludable, no un trabajo atascado.
- Después de cambiar cualquier superficie GraphQL, ejecutar las pruebas de contrato:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentación

- **Técnica** — la [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Telemetry](https://github.com/shernandezp/TrackHub/wiki/Telemetry), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Router](https://github.com/shernandezp/TrackHub/wiki/Router), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture)
- **De usuario** — en la app: el botón de Ayuda o **F1** en cualquier pantalla
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
