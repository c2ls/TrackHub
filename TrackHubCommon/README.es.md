# TrackHub Common Library

[← Volver a la página principal](README.md) · [English](README.en.md)

TrackHubCommon es la base compartida sobre la que se construye cada servicio backend de TrackHub. Se distribuye como **cuatro paquetes locales de NuGet**, no como referencias de proyecto.

| Paquete | Contenido |
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
- Un feed local de NuGet — los paquetes **no** se publican en nuget.org

### Consumo de los paquetes

Agregar las referencias de paquete a través del `Directory.Packages.props` del repositorio, y luego referenciarlas por proyecto:

```xml
<ItemGroup>
  <PackageReference Include="TrackHubCommon.Domain" />
  <PackageReference Include="TrackHubCommon.Application" />
  <PackageReference Include="TrackHubCommon.Infrastructure" />
  <PackageReference Include="TrackHubCommon.Web" />
</ItemGroup>
```

Registrar los servicios en el `DependencyInjection.cs` de cada capa:

```csharp
// Application layer
services.AddApplicationServices(typeof(SomeHandler).Assembly);
services.AddDistributedMemoryCache();   // required — CachingBehavior resolves IDistributedCache

// Web layer
builder.Services
    .AddTrackHubGraphQLServer<Query, Mutation>(builder.Environment.IsDevelopment());
```

### Compilación de los paquetes

```bash
dotnet build
```

**Usar `dotnet build`, no `dotnet pack`.** Los proyectos configuran `GeneratePackageOnBuild=true`, así que los paquetes se producen durante la compilación. `dotnet pack` puede empaquetar un DLL **obsoleto**, o fallar con NU5026 tras un clean — no recompila de forma confiable.

---

## Reempaquetado tras un cambio

Cuando cambia cualquier proyecto `TrackHubCommon.*` — una constante nueva, un comportamiento nuevo, un cambio de contrato — los paquetes deben reconstruirse y cada consumidor debe actualizarse.

1. **Incrementar `<Version>`** en `Directory.Build.props`. Es la fuente de verdad, aplicada en conjunto a los cuatro paquetes.
2. **`dotnet build`** (ver arriba).
3. **Copiar los archivos `.nupkg`** de cada `src/Common.*/bin/Debug/` al feed local y a `NugetPackages/`.
4. **Purgar la caché global** al reempaquetar la *misma* versión:

   ```bash
   rm -rf ~/.nuget/packages/trackhubcommon.*/<version>
   ```

   De lo contrario los consumidores restauran la copia extraída previamente y se obtienen confusos errores `CS0117 'Resources' does not contain …` contra código recién escrito.
5. **Actualizar y restaurar cada consumidor.**

---

## Notas específicas del proyecto

- **Cada consumidor debe avanzar junto con los demás, y ninguno puede quedar rezagado.** Los ocho repositorios de servicio siguen la versión a través de su `Directory.Packages.props` — y **el harness de ServiceContracts la sigue a través de una `PackageReference` directa** en `TrackHub.ServiceContracts.Harness.csproj`. No tiene `Directory.Packages.props`, así que un barrido basado solo en props lo pasa por alto y la suite de contratos falla al restaurar.
- **`AccountScopeBehavior` es fail-closed; el `IFeatureFlagService` por defecto es fail-open.** Eso es deliberado: un ámbito de tenant faltante es una falla de seguridad, mientras que un registro de feature faltante es una falla de configuración del servicio que las propias pruebas del servicio deberían detectar. Un servicio que use `[RequireFeature]` **debe** registrar su propio `IFeatureFlagService`.
- **`AddDistributedMemoryCache()` no es opcional.** `CachingBehavior` resuelve `IDistributedCache` para cada tipo de solicitud; un registro faltante hace fallar **toda** solicitud con un error de DI enmascarado.
- **Agregar un recurso de autorización no es suficiente.** También debe agregarse a `DefaultResources` del `ApplicationDbContextInitializer` de `TrackHubSecurity`, y otorgarse en la matriz de cada rol — [dos pasos separados](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity#seeding-rules-that-bite).
- **Los catálogos de constantes son el contrato.** Los nombres de recurso, acción, clave de feature, esquema y tabla nunca son literales de cadena en el sitio de llamada — un error de tipeo se convierte en una falla silenciosa de autorización o de mapeo.
- Verificar que una constante llegó a un DLL compilado se hace mejor con `grep -a`; los literales de metadatos UTF-16 derrotan a un `strings` simple.
- Las compilaciones de imagen Docker empaquetan estos paquetes automáticamente en una etapa `common`, así que un despliegue no necesita un feed prepoblado. Una ejecución local de `dotnet ef` sí lo necesita.

---

## Documentación

- **Técnica** — el [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Common Library](https://github.com/shernandezp/TrackHub/wiki/Common-Library), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture), [Coding Standards](https://github.com/shernandezp/TrackHub/wiki/Coding-Standards)
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
