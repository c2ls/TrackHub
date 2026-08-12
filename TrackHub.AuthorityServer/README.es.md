# Servicio de Autorización de TrackHub

[← Volver a la página principal](README.md) · [English](README.en.md)

El Servicio de Autorización es el **servidor de tokens de identidad y OAuth 2.0 / OpenID Connect basado en OpenIddict** de TrackHub. Autentica usuarios, conductores y clientes de servicio, y emite los tokens de acceso (audiencia `trackhub_api`) que validan todos los demás servicios de TrackHub.

También aloja la UI de login de ASP.NET y es propietario de las tablas de OpenIddict en la base de datos `TrackHubSecurity`.

---

## Qué proporciona

- **Flujo de Código de Autorización con PKCE** para clientes públicos — web, móvil y móvil de conductor
- **Flujo de Credenciales de Cliente** para servicios backend e integraciones con socios, con derivación automática del tenant
- **Gestión de tokens** — emisión, renovación y revocación, con delimitación basada en audiencia
- **Una UI de login personalizable** con soporte de marca
- **Validación de credenciales con hash BCrypt** contra la tabla `security.users`, propiedad de Security
- **El claim de rol**, resuelto en el login y vuelto a resolver en cada renovación

Detalle completo: **[Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity)** y **[Technology](https://github.com/shernandezp/TrackHub/wiki/Technology)** en la wiki.

---

## Inicio rápido

### Requisitos previos

- .NET 10 SDK
- PostgreSQL 14+
- Un certificado SSL — autofirmado para desarrollo, emitido por una CA para producción
- Los paquetes `TrackHubCommon.*` disponibles desde un feed local de NuGet

### Pasos

1. **Clonar**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.AuthorityServer.git
   cd TrackHub.AuthorityServer
   ```

2. **Configurar la conexión a la base de datos** en `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHubSecurity;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Generar un certificado autofirmado** (solo desarrollo):

   ```powershell
   $cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\CurrentUser\My"
   $password = ConvertTo-SecureString -String "openiddict" -Force -AsPlainText
   Export-PfxCertificate -Cert $cert -FilePath "certificate.pfx" -Password $password
   ```

4. **Aplicar las migraciones** — esto crea las tablas de OpenIddict:

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

5. **Registrar los clientes OAuth**

   ```bash
   dotnet run --project src/ClientSeeder
   ```

6. **Ejecutar**

   ```bash
   dotnet run --project src/Web
   ```

7. **Abrir la página de login** en `https://localhost:<port>`.

---

## Notas específicas del proyecto

- **Este servicio es propietario únicamente de las tablas `OpenIddict*`.** La migración de su `AuthorityDbContext` apunta a la base de datos `TrackHubSecurity` y forma parte de la secuencia de instalación documentada. Su `SecurityDbContext` es una **proyección de solo lectura** acotada de tablas propiedad de TrackHubSecurity (`users`, `clients`, `driver_credentials`, `roles`, `user_role`, `service_client_permissions`), y su migración está deliberadamente **vacía**: toda la configuración usa `ExcludeFromMigrations()`. No agregar DDL ahí.
- **El tenant de un cliente de servicio se deriva, nunca lo suministra el llamador.** El claim `account_id` proviene de las filas activas del cliente en `security.service_client_permissions`, de modo que una credencial de socio no puede ampliar su propio alcance:

  | Permisos vigentes del cliente | Claim `account_id` |
  |---|---|
  | Al menos un permiso con `allowcrossaccount` (identidades internas de la plataforma) | **No se emite** — el token queda sin delimitar |
  | Permisos que nombran exactamente una cuenta | Se emite automáticamente |
  | Permisos que nombran varias cuentas | **Requiere el parámetro de solicitud `account_id`** |
  | Ningún permiso asociado a una cuenta | No se emite |

- **El parámetro opcional de solicitud `account_id`** permite a un cliente multicuenta nombrar el tenant que desea. Solo puede *restringir*, nunca ampliar: la cuenta solicitada debe corresponder a uno de los propios permisos activos del cliente:

  ```bash
  curl -X POST https://<authority>/connect/token \
    -d grant_type=client_credentials \
    -d client_id=<partner_client> \
    -d client_secret=<secret> \
    -d scope=service_scope \
    -d account_id=<account-guid>
  ```

  El endpoint responde `invalid_request` cuando el valor no es un identificador válido, cuando el cliente no tiene ningún permiso activo para esa cuenta, o cuando un cliente multicuenta lo omite. Ese último caso es un rechazo deliberado: emitir un tenant arbitrario apuntaría en silencio a un partner hacia el cliente equivocado.

- **`UseCors` debe ejecutarse antes que `UseHealthChecks`.** Este servicio originalmente lo tenía invertido, lo cual hacía que el mosaico de inicio de sesión en la página pública de estado quedara permanentemente ilegible: la página consulta `/health` de forma cross-origin sin token.
- **Detrás de un proxy inverso**, el middleware `ForwardedHeaders` confía en `X-Forwarded-Proto` y `X-Forwarded-For`, de modo que OpenIddict identifica correctamente las solicitudes HTTPS aunque el tráfico interno entre contenedores sea HTTP.
- **El claim de rol es solo para fines de visibilidad.** La aplicación de permisos siempre pasa por la Security API; este claim nunca otorga nada. Los usuarios deben volver a iniciar sesión para que se refleje un rol recién otorgado.
- Los mensajes de login y validación renderizados en el servidor provienen de recursos `.resx` a través de `ResourceLocalizer`, resueltos según la cultura de la solicitud ambiente. Nunca deben codificarse de forma fija.
- Para producción, usar un certificado emitido por una Autoridad de Certificación.

---

## Documentación

- **Técnica** — la [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity), [Technology](https://github.com/shernandezp/TrackHub/wiki/Technology), [Database](https://github.com/shernandezp/TrackHub/wiki/Database)
- **Usuario** — en la aplicación: el botón de ayuda o **F1** en cualquier pantalla
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
