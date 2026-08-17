# TrackHub Common Library

[← Volver a la página principal](README.md) · [English](README.en.md)

TrackHubCommon es la base compartida sobre la que se construye cada servicio backend de TrackHub. **No se empaqueta** — cada servicio backend referencia sus cuatro proyectos directamente.

| Proyecto | Contenido |
|---|---|
| `TrackHubCommon.Domain` | Constantes, enumeraciones, extensiones de criptografía, localización, primitivas de eventos de dominio |
| `TrackHubCommon.Application` | El mediador CQRS personalizado, el pipeline de comportamientos, atributos, ayudantes de pruebas |
| `TrackHubCommon.Infrastructure` | Convenciones e interceptores de EF, la fábrica de clientes GraphQL, `IdentityService` |
| `TrackHubCommon.Web` | Registro del servidor GraphQL, filtros de error, transformadores del esquema de seguridad |

La estructura sigue la [plantilla de Arquitectura Limpia de Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture), adaptada a las necesidades de TrackHub.

Detalle completo: **[Common Library](https://github.com/shernandezp/TrackHub/wiki/Common-Library)** en el wiki.

---

## Qué proporciona

- **El mediador CQRS personalizado** (`Common.Mediator`) — `IRequest<TResult>`, `IRequestHandler<,>`, `MediatorDispatcher : ISender, IPublisher`. **MediatR no se usa y está prohibido.**
- **El pipeline de comportamientos** — registro, validación, autorización, ámbito de tenant fail-closed, caché, limitación de tasa, manejo de excepciones no controladas
- **Los catálogos de constantes entre servicios** — `Resources`, `Actions`, `Roles`, `Policies`, `Clients`, `FeatureKeys`, `BackgroundJobKeys`, `Reports`, y los metadatos de esquema/tabla/columna/vista
- **Criptografía** — BCrypt para contraseñas de usuario, encriptación por certificado de servidor para secretos de terceros como las credenciales de proveedores GPS
- **Localización** — `ResourceLocalizer`, la única primitiva para texto renderizado en el servidor a partir de `.resx`
- **Interceptores y convenciones de EF** — columnas de auditoría, despacho de eventos de dominio tras el commit, `UseUtcTimestamps()`
- **La fábrica de clientes GraphQL** — la única forma sancionada de registrar un cliente entre servicios, aplicando tiempos de espera, propagación de encabezados y una política de resiliencia explícita
- **`AddTrackHubGraphQLServer<TQuery, TMutation>`** — la única definición de la configuración del servidor GraphQL de la plataforma

---

## Inicio rápido

### Requisitos previos

- .NET 10 SDK

### Consumo de Common

Common **no** se empaqueta. Cada servicio del monorepo referencia los proyectos directamente, así
que no hay versión que fijar ni feed que configurar:

```xml
<ItemGroup>
  <ProjectReference Include="$(RepoRoot)TrackHubCommon/src/Common.Domain/Common.Domain.csproj" />
  <ProjectReference Include="$(RepoRoot)TrackHubCommon/src/Common.Application/Common.Application.csproj" />
  <ProjectReference Include="$(RepoRoot)TrackHubCommon/src/Common.Infrastructure/Common.Infrastructure.csproj" />
  <ProjectReference Include="$(RepoRoot)TrackHubCommon/src/Common.Web/Common.Web.csproj" />
</ItemGroup>
```

`$(RepoRoot)` se define en el `Directory.Build.props` de cada servicio como la raíz del monorepo.

Registrar los servicios en el `DependencyInjection.cs` de cada capa:

```csharp
// Application layer
services.AddApplicationServices(typeof(SomeHandler).Assembly);
services.AddDistributedMemoryCache();   // required — CachingBehavior resolves IDistributedCache

// Web layer
builder.Services
    .AddTrackHubGraphQLServer<Query, Mutation>(builder.Environment.IsDevelopment());
```

### Compilación

```bash
dotnet build TrackHub.slnx     # desde la raíz del monorepo — compila Common y todos sus consumidores
```

---

## Cambiar Common

Editar el código y compilar. Ese es todo el procedimiento.

Como los consumidores referencian los proyectos en lugar de paquetes, un cambio en Common recompila
en la misma build cada servicio que lo usa, y el compilador reporta las roturas de inmediato. No hay
versión que incrementar, nada que empaquetar, ningún feed al que copiar ni caché de NuGet que purgar
— la clase de error `CS0117 'Resources' does not contain …` causada por un paquete extraído obsoleto
ya no puede ocurrir.

Cambiar Common y sus consumidores en **un solo commit**.

---

## Notas específicas del proyecto

- **Cada consumidor avanza junto con los demás, automáticamente.** Todos los servicios y el harness
  de ServiceContracts referencian los proyectos, así que siempre compilan contra el código actual —
  ninguno puede quedar rezagado en un Common anterior, y un cambio incompatible aparece al compilar
  en lugar de después de publicar.
- **`AccountScopeBehavior` es fail-closed; el `IFeatureFlagService` por defecto es fail-open.** Eso es deliberado: un ámbito de tenant faltante es una falla de seguridad, mientras que un registro de feature faltante es una falla de configuración del servicio que las propias pruebas del servicio deberían detectar. Un servicio que use `[RequireFeature]` **debe** registrar su propio `IFeatureFlagService`.
- **`AddDistributedMemoryCache()` no es opcional.** `CachingBehavior` resuelve `IDistributedCache` para cada tipo de solicitud; un registro faltante hace fallar **toda** solicitud con un error de DI enmascarado.
- **Agregar un recurso de autorización no es suficiente.** También debe agregarse a `DefaultResources` del `ApplicationDbContextInitializer` de `TrackHubSecurity`, y otorgarse en la matriz de cada rol — [dos pasos separados](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity#seeding-rules-that-bite).
- **Los catálogos de constantes son el contrato.** Los nombres de recurso, acción, clave de feature, esquema y tabla nunca son literales de cadena en el sitio de llamada — un error de tipeo se convierte en una falla silenciosa de autorización o de mapeo.
- Verificar que una constante llegó a un DLL compilado se hace mejor con `grep -a`; los literales de metadatos UTF-16 derrotan a un `strings` simple.
- Las compilaciones de imagen Docker copian el código de TrackHubCommon al contexto de build y lo restauran como referencia de proyecto — no hay etapa de empaquetado ni feed que prepoblar, ni en un despliegue ni localmente.

---

## Documentación

- **Técnica** — el [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Common Library](https://github.com/shernandezp/TrackHub/wiki/Common-Library), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture), [Coding Standards](https://github.com/shernandezp/TrackHub/wiki/Coding-Standards)
- **Despliegue** — [TrackHub.Deployment](../TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
