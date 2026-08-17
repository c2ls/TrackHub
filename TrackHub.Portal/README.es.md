# TrackHub Web

[← Volver a la página principal](README.md) · [English](README.en.md)

TrackHub Web es el **portal en React** — la interfaz de usuario orientada al operador para toda la plataforma. Es una aplicación de página única (single-page application) en React 19 + TypeScript 7, construida con Vite 8 y probada con Vitest 4, que se sirve en producción como archivos estáticos mediante nginx.

Es también donde vive la **documentación de usuario**: los temas de ayuda contextual en `public/help/` se incluyen en cada build del portal.

---

## Qué proporciona

- **Seguimiento GPS en tiempo real** — un mapa en vivo de transportistas y dispositivos, con actualizaciones automáticas de posición, reproducción (replay) y segmentación de viajes
- **Integración multioperador** — una sola interfaz para cada proveedor GPS conectado, con gestión de dispositivos, sincronización manual, ping de conectividad y estado de salud
- **Geofencing** — un editor de polígonos y círculos en ambos proveedores de mapas, con listas paginadas del lado del servidor y superposiciones (overlays) en el panel
- **Gestión de viajes** — un tablero de despacho para viajes, paradas, entregas, planes de ruta, peajes, prueba de entrega y enlaces públicos de seguimiento
- **Documentos y fuerza laboral (workforce)** — cargas versionadas con firmas y compartición; el registro de conductores, calificaciones y asignaciones
- **Alertas y notificaciones** — el feed dentro de la aplicación, reglas de notificación, suscripciones y plantillas
- **Reportes** — el catálogo de reportes gobernado, con vista previa dentro de la aplicación y exportación a Excel/PDF
- **Administración** — cuentas, usuarios, grupos, permisos, features y clientes de servicio (service clients)
- **Página pública de estado** — `/status` se renderiza sin necesidad de iniciar sesión, e informa el estado de salud por servicio y los anuncios de la plataforma
- **Ayuda contextual integrada** — botón de Ayuda o **F1** en cada pantalla, con un índice navegable y búsqueda del lado del cliente, en inglés y español
- **Interfaz bilingüe** (EN/ES) y un tema oscuro/claro

Detalle completo: **[Frontend](https://github.com/shernandezp/TrackHub/wiki/Frontend)** en la wiki.

---

## Inicio rápido

### Requisitos previos

- Node.js 20+
- Acceso a los servicios backend de TrackHub — AuthorityServer, Security, Manager, Router, Telemetry, Geofencing, TripManagement y Reporting

### Pasos

1. **Clonar e instalar**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.git
   cd TrackHub
   npm install
   ```

2. **Configurar el entorno.** Editar `.env` (los valores predeterminados de desarrollo apuntan a `https://localhost`); `.env.production.template` es la referencia para el despliegue.

3. **Configurar certificados HTTPS.** OAuth requiere HTTPS, y el callback está registrado en `https://localhost:3000/...`:

   ```bash
   npx mkcert create-ca
   npx mkcert create-cert
   ```

   Esto genera `ca.key` / `ca.crt` y `cert.key` / `cert.crt` en la raíz del proyecto. Vite los detecta automáticamente cuando existen tanto `cert.crt` como `cert.key`. Están en el gitignore — nunca deben confirmarse (commit). El navegador advertirá sobre el certificado autofirmado; eso es esperado en desarrollo.

4. **Ejecutar**

   ```bash
   npm run dev
   ```

   Abrir `https://localhost:3000`.

### Scripts

| Comando | Propósito |
|---|---|
| `npm run dev` (o `npm start`) | Servidor de desarrollo — también ejecuta primero el validador de ayuda |
| `npm run build` | `tsc --noEmit` y luego un build de producción — también ejecuta primero el validador de ayuda |
| `npm run typecheck` | Solo TypeScript |
| `npm test` / `npm run test:watch` | Vitest |
| `npm run codegen` | Regenera los documentos GraphQL tipados a partir de `schemas/*.graphql` |
| `npm run help:check` | Valida el contrato de autoría de ayuda sin compilar |
| `npm run lint` | ESLint |

**El gate es `npm run typecheck && npm test && npm run build`.**

---

## Notas específicas del proyecto

- **Los componentes nunca tocan la red.** Cada llamada pasa por tres capas: `src/api/<backend>/<domain>Operations.ts` (documentos GraphQL vía el tag generado `graphql()`) → `src/api/<backend>/<domain>.ts` (funciones tipadas que lanzan `ApiError`) → `src/queries/<domain>.ts` (hooks de TanStack Query que gestionan las claves de caché e invalidación). Las URLs de endpoints viven **únicamente** en `src/api/core/endpoints.ts`.
- **Los valores viajan solo como variables GraphQL.** No hay interpolación de cadenas de entrada de usuario en los documentos — el antiguo helper de escapado `formatValue` ya no existe.
- **La deriva (drift) del backend es un error de compilación.** El conjunto de pruebas de contrato exporta el SDL de cada productor a `schemas/<service>.graphql`; `npm run codegen` valida cada operación del portal contra ellos. Después de cualquier cambio de GraphQL en el backend: ejecutar las pruebas de contrato y luego `npm run codegen`.
- **`src/` es 100% TypeScript.** `allowJs` está desactivado y un guard de ESLint marca error ante cualquier archivo nuevo `.js` o `.jsx` bajo `src/`.
- **Los componentes y controles de Argon exportan tipos de props reales** — importarlos y usarlos directamente. Nunca reintroducir interfaces locales de recorte de props (prop-slice) ni casts de frontera `as unknown as` en los sitios de llamada; si un control carece de una prop que se debe pasar, ampliar el tipo de prop **exportado del control**. Las extensiones de tema viven en `src/types/mui-theme.d.ts`.
- **El shim de define `process.env.REACT_APP_*` en `vite.config.ts` es permanente por decisión** — todas las lecturas se centralizan en `api/core/endpoints.ts`, y mantener la convención de CRA hace que los archivos `.env` existentes y la documentación de despliegue sigan siendo válidos. Está deshabilitado en modo de prueba, porque los conjuntos de pruebas asignan las variables de entorno en tiempo de ejecución.
- **Las claves de i18n se verifican en tiempo de compilación.** Agregar cada clave tanto a `locales/en.json` como a `locales/es.json`; las claves dinámicas se castean solo en la expresión de la clave.
- **Escapar todo lo que se interpole en un popup de mapa.** `bindPopup`/`bindTooltip` de Leaflet y el InfoWindow de Google asignan su argumento vía `innerHTML`, por lo que el escapado de React no aplica — usar `escapeHtml` (`src/utils/htmlUtils.ts`). Los nombres de transportistas son texto libre editable por la cuenta, y las direcciones provienen de un geocodificador de terceros.
- **La conversión `datetime-local` ⇄ UTC pasa por `toDateTimeLocalInput` / `fromDateTimeLocalInput`** (`src/utils/dateUtils.ts`). El control conserva la hora *local* de pared (wall time), por lo que el instante se desplaza en ambas direcciones. Un helper que se salte ese desplazamiento solo produce un round-trip correcto bajo `TZ=UTC` — que es exactamente lo que corren las máquinas de desarrollo y CI. Verificar la propiedad de round-trip, nunca un valor literal.
- **El contenido de ayuda se valida en tiempo de build.** `scripts/build-help.mjs` se ejecuta en `predev` y `prebuild` y verifica la paridad entre idiomas, que el id sea igual al nombre de archivo, `screens:` ↔ `routes.tsx` **en ambas direcciones**, los destinos de enlace `topic:`, la ausencia de HTML crudo y la existencia de assets. Agregar o renombrar una pantalla sin actualizar el frontmatter del tema **hace fallar el build**.
- **`/status` debe seguir funcionando sin token.** Sus dos fetches anónimos (`api/core/healthProbe.ts` y `getVisibleAnnouncements`) son las únicas excepciones sancionadas a la regla de capas de la API; ambas están documentadas en los encabezados de sus archivos y se alcanzan únicamente a través de `src/queries/platformStatus.ts`.
- **`RouteDefinition.principalTypes` tiene por defecto `[User]`.** Una ruta alcanzable por principals de tipo driver o de enlace público debe establecer `public: true` explícitamente, o esos principals rebotarán entre esa ruta y `/dashboard` indefinidamente.
- **`typescript-eslint` está deshabilitado** hasta que soporte TypeScript 7. `tsc` es el gate de lint de TypeScript.
- Reporting es solo REST (`api/BasicReports`), y la base REST de documentos de Manager es `~/documents` — **sin prefijo `api/`**.

---

## Una nota sobre la configuración

El objetivo de TrackHub es estandarizar y simplificar la integración de diferentes proveedores de monitoreo, pero su configuración, despliegue y mantenimiento requieren conocimientos intermedios a avanzados de .NET y React.

A lo largo de esta aplicación y de los servicios backend, el repositorio contiene contraseñas, certificados, variables de entorno y otros secretos de desarrollo. **Esto es intencional** — permite levantar un nuevo entorno de desarrollo sin necesidad de configurar secretos manualmente. Los despliegues de producción deben sobrescribirlos todos.

---

## Documentación

- **Técnica** — la [wiki de TrackHub](https://github.com/shernandezp/TrackHub/wiki): [Frontend](https://github.com/shernandezp/TrackHub/wiki/Frontend), [Technology](https://github.com/shernandezp/TrackHub/wiki/Technology), [User Permissions Overview](https://github.com/shernandezp/TrackHub/wiki/User-Permissions-Overview), [Coding Standards](https://github.com/shernandezp/TrackHub/wiki/Coding-Standards)
- **Usuario** — en la aplicación: el botón de Ayuda o **F1** en cualquier pantalla. Los temas fuente viven en `public/help/{en,es}/`.
- **Despliegue** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## Licencia

Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.
