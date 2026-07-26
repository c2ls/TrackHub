# API de Gestión de TrackHub

[← Volver a la página principal](README.md) · [English](README.en.md)

La API de Gestión es el **servicio de datos maestros** de TrackHub — el mayor servicio backend de la plataforma, y del que más leen los demás servicios.

Construida sobre .NET 10 con un endpoint GraphQL de HotChocolate, siguiendo las convenciones de Clean Architecture y CQRS de la plataforma.

---

## Qué gestiona

| Módulo | Contenido |
|---|---|
| **Cuentas** | Cuentas y estado del ciclo de vida, configuración, funcionalidades, marca, concesiones de soporte |
| **Identidad (lado del portal)** | Usuarios, configuración de usuario, grupos, membresía usuario–grupo y transportador–grupo |
| **Activos** | Transportadores, tipos de transportador, dispositivos, asignaciones transportador–dispositivo |
| **Integración GPS** | Operadores, credenciales de proveedor cifradas, la superficie de comandos de sincronización de dispositivos |
| **Referencia geoespacial** | Proveedores de geocodificación, puntos de interés |
| **Documentos** | Documentos, versiones, tipos, firmas, uso compartido, retención |
| **Fuerza laboral** | Conductores, calificaciones, historial de asignación conductor–transportador |
| **Alertas y notificaciones** | Eventos de alerta, reglas de notificación, entregas, plantillas, suscripciones |
| **Plataforma** | Anuncios, eventos de auditoría, ejecuciones de trabajos en segundo plano, concesiones de enlaces públicos, el catálogo de reportes |

Gestiona los esquemas `app` y `map`, además del **DDL del esquema `telemetry`** que sirve el servicio Telemetry. Los datos de posición de alto volumen en sí pertenecen a Telemetry.

Detalle completo: **[Manager](https://github.com/shernandezp/TrackHub/wiki/Manager)** en la wiki.

---

## Inicio rápido

### Requisitos previos

- SDK de .NET 10
- PostgreSQL 14+ (con PostGIS, ya que la base de datos `TrackHub` se comparte con los esquemas de Geofencing y TripManagement)
- Un TrackHub AuthorityServer en ejecución, para autenticación
- Los paquetes `TrackHubCommon.*` disponibles desde un feed local de NuGet — **no** están en nuget.org

### Pasos

1. **Clonar**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.Manager.git
   cd TrackHub.Manager
   ```

2. **Configurar la conexión a la base de datos** en `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Aplicar las migraciones** — esto crea los esquemas `app`, `map` **y `telemetry`**:

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

4. **Sembrar datos iniciales** (catálogo de reportes, datos de referencia):

   ```bash
   dotnet run --project src/DBInitializer
   ```

5. **Ejecutar**

   ```bash
   dotnet run --project src/Web
   ```

6. **Abrir el endpoint GraphQL** en `https://localhost:<port>/graphql`.

---

## Notas específicas del proyecto

- **Este servicio migra el esquema `telemetry`.** El servicio Telemetry no tiene migraciones propias y debe apuntar a la misma base de datos `TrackHub`. Agregar una tabla de telemetry implica agregar una migración de Manager.
- **`ApplicationDbContext` es `NoTracking` de forma predeterminada.** Una fila obtenida para mutación debe adjuntarse con `Attach`, o sus cambios se descartan silenciosamente en `SaveChangesAsync`. EF InMemory usa `TrackAll` por defecto, por lo que una prueba unitaria no detectará la omisión.
- **No agregar `Common.Domain.Enums` a los `GlobalUsings` de Infrastructure.** Su `TransporterType` colisiona con la entidad de tabla `Infrastructure.Entities` del mismo nombre — impórtelo por archivo en su lugar.
- **`transporter_position.attributes` es una columna `json` de PostgreSQL.** `json` no tiene operador de igualdad, por lo que `Distinct()`, `GroupBy()` u operaciones de conjunto sobre una entidad o proyección que la incluya fallan en tiempo de ejecución con `42883`. EF InMemory no detectará esto.
- **El catálogo de reportes se resiembra en cada arranque.** El código es la fuente de verdad para los metadatos sembrados — las ediciones de un SuperAdministrator a Description, Category, RequiredFeatureKey, ManagerOnly, SupportsPdf o SortOrder se revierten al reiniciar. Solo `Active` persiste. Esto es intencional.
- **Manager → Router usa un tiempo de espera de cliente de 120 s**, porque el despacho de sincronización manual espera la obtención del proveedor. Todo otro cliente usa 30 s.
- **El endpoint REST de anuncios es anónimo y evita el mediator** — todos los comportamientos del pipeline asumen un principal. Se ejecuta detrás de un output caching de 60 s y un límite de tasa por IP de cliente, razón por la cual el servicio también ejecuta `UseForwardedHeaders`.
- **El texto localizado nunca vive en la base de datos.** Los textos predeterminados de notificaciones provienen de recursos `.resx`; las plantillas personalizadas por la cuenta y el texto de anuncios son contenido de usuario, almacenado por idioma.
- Después de cambiar cualquier superficie GraphQL, ejecutar las pruebas de contrato:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentación

- **Técnica** — la [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Manager](https://github.com/shernandezp/TrackHub/wiki/Manager), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication), [Coding Standards](https://github.com/shernandezp/TrackHub/wiki/Coding-Standards)
- **De usuario** — en la app: el botón de Ayuda o **F1** en cualquier pantalla
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
