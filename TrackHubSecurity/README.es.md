# TrackHub Security API

[← Volver a la página principal](README.md) · [English](README.en.md)

La Security API es la propietaria del **modelo de autorización** de TrackHub y responde a las verificaciones de identidad y permisos que hace cada uno de los demás servicios en cada solicitud.

Construida sobre .NET 10 con un endpoint GraphQL de HotChocolate, siguiendo las convenciones de Clean Architecture y CQRS de la plataforma.

---

## Qué gestiona

| Área | Tablas |
|---|---|
| **Identidad** | `security.users`, `security.driver_credentials`, `security.driver_device_registrations` |
| **RBAC** | `security.roles`, `security.user_role`, `security.resource_action_role` |
| **Políticas** | `security.policies`, `security.user_policy`, `security.resource_action_policy` |
| **Catálogo** | `security.resources`, `security.actions`, `security.resource_action` |
| **Clientes de servicio** | `security.clients`, `security.service_client_permissions` |

La autorización es de **doble vía**: el acceso se concede si *cualquiera* de las dos vías, la de roles o la de políticas, resuelve el par `Resource + Action` solicitado.

Roles predeterminados: **Administrator** (acceso total), **Manager** (acceso operativo y administrativo a nivel de cuenta), **User** (limitado al grupo), **Audit** (acceso de auditoría de solo lectura).

Detalle completo: **[Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity)** y **[User Permissions Overview](https://github.com/shernandezp/TrackHub/wiki/User-Permissions-Overview)** en la wiki.

---

## Inicio rápido

### Requisitos previos

- .NET 10 SDK
- PostgreSQL 14+
- Una instancia de TrackHub AuthorityServer en ejecución, para autenticación
- Los paquetes `TrackHubCommon.*` disponibles desde un feed local de NuGet

### Pasos

1. **Clonar**

   ```bash
   git clone https://github.com/shernandezp/TrackHubSecurity.git
   cd TrackHubSecurity
   ```

2. **Configurar la conexión a la base de datos** en `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHubSecurity;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Aplicar las migraciones**

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

4. **Sembrar roles, recursos, acciones y permisos de clientes de servicio**

   ```bash
   dotnet run --project src/DBInitializer
   ```

5. **Ejecutar**

   ```bash
   dotnet run --project src/Web
   ```

6. **Abrir el endpoint de GraphQL** en `https://localhost:<port>/graphql`.

---

## Notas específicas del proyecto

- **Agregar una fila resource-action y otorgarla a un rol son dos pasos separados.** Un nuevo par `Actions.Custom` necesita tanto el seed de `ResourceAction` (la lista `foreach (var resourceName …)`) *como* una entrada en la matriz de cada rol. `ResourceActionRoleReader` compara los nombres de rol de forma **exacta**, sin recorrer `ParentRoleId`; nada se hereda. Omitir el segundo paso devuelve `FORBIDDEN` para cualquier usuario que no sea Administrator, aunque el despliegue esté sembrado correctamente en todo lo demás.
- **`SeedRoleResourceActionsAsync` únicamente AGREGA filas.** Quitar un permiso de la matriz no lo elimina de una base de datos ya sembrada; los permisos retirados necesitan SQL de limpieza explícito.
- **Una identidad de servicio necesita dos registros, no uno.** El nombre del cliente debe existir en `security.clients` *y* sus permisos en `security.service_client_permissions` (client, resource, action, scope, audience). Si falta el segundo, todas las llamadas devuelven un `FORBIDDEN` limpio mientras el servicio que llama se reporta como saludable; es la causa más común de "todo se deniega después de una actualización".
- **Los nuevos recursos de autorización deben agregarse a `DefaultResources`** en `ApplicationDbContextInitializer`. El rol Administrator recibe automáticamente todos los resource-action.
- **`allowcrossaccount = true` marca una identidad interna de la plataforma.** El token de ese cliente **no** lleva el claim `account_id`, que es contra lo que se compara un permiso a nivel de plataforma. Un cliente partner se siembra de manera opuesta: con un `accountid` y **sin** `allowcrossaccount`.
- **Las consultas de identidad fijan el sujeto al llamador** (`IdentityCallerGuard`): un token de usuario solo puede preguntar por sí mismo, un token de servicio solo por su propio cliente.
- Las decisiones de autorización se almacenan en caché durante 30 s en el `Common.Infrastructure.IdentityService` de cada llamador, por lo que un cambio de permisos puede tardar hasta ese tiempo en aplicarse.
- Política de contraseñas: longitud mínima 8, con al menos una letra mayúscula, una minúscula y un dígito.
- Después de cambiar cualquier superficie de GraphQL, ejecutar las pruebas de contrato:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentación

- **Técnica** — la [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity), [User Permissions Overview](https://github.com/shernandezp/TrackHub/wiki/User-Permissions-Overview), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture)
- **Usuario** — en la aplicación: el botón de ayuda o **F1** en cualquier pantalla
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
