---
id: getting-started
title: Primeros pasos
description: Inicie sesión en TrackHub, oriéntese en la pantalla, use la campana de notificaciones y cierre sesión de forma segura.
category: getting-started
screens: []
related: [roles-and-permissions, profile-and-settings, dashboard-live-map]
tags: [iniciar sesión, disposición, navegación, cerrar sesión, sesión]
order: 10
---

# Primeros pasos

TrackHub lo dirige a una página segura de inicio de sesión y lo trae de regreso automáticamente. Una vez dentro, todas las pantallas comparten el mismo marco: un menú lateral izquierdo, una barra superior y un área principal en el centro.

## Iniciar sesión por primera vez

TrackHub no tiene su propio recuadro de inicio de sesión en la página principal. En su lugar, lo envía a una página de inicio de sesión independiente y lo regresa cuando termina.

1. Abra TrackHub en su navegador web.
2. Como aún no ha iniciado sesión, la aplicación lo envía a una **página de inicio de sesión** independiente. Este es el servicio de identidad que protege su cuenta.
3. Ingrese el **nombre de usuario** y la **contraseña** que le fueron asignados y confirme.
4. Cuando sus datos son aceptados, se le regresa **automáticamente a TrackHub** y llega al **Tablero**.

No necesita escribir una dirección web para la página de inicio de sesión ni copiar nada entre páginas. El recorrido de ida y vuelta (TrackHub, luego la página de inicio de sesión, luego de regreso a TrackHub) ocurre por sí solo.

**Si el inicio de sesión falla**, TrackHub muestra una página de pantalla completa **Error de Autenticación** que explica las causas probables: que el servicio de autenticación esté temporalmente no disponible, problemas de conectividad de red, o una solicitud de autenticación inválida o expirada. La página ofrece un botón **Reintentar Autenticación**. Por seguridad, queda deshabilitado durante una breve cuenta regresiva —muestra **Reintentar en Ns** y cuenta unos 30 segundos— antes de que pueda intentarlo de nuevo, y también hay un botón **Volver**. Los detalles de solución de problemas de esta página están en [Solución de problemas](topic:troubleshooting#no-puedo-iniciar-sesión).

## La disposición principal

Una vez que ha iniciado sesión, todas las pantallas comparten el mismo marco:

- Un **menú lateral izquierdo** (el sidenav) a lo largo del costado izquierdo, para moverse entre pantallas.
- Una **barra superior** en la parte de arriba, que indica dónde se encuentra, con una campana de notificaciones y un enlace **Cerrar Sesión**.
- El **área principal** en el centro, que cambia según la pantalla que elija.

## El menú lateral izquierdo

El menú lateral izquierdo enumera las pantallas que tiene permitido abrir. Según su rol y las funcionalidades que tenga su cuenta, es posible que vea algunas o todas las siguientes opciones:

- **Tablero** — el mapa en vivo y el resumen de unidades.
- **Administrador del Sistema** — administración a nivel de plataforma (solo la ven los administradores).
- **Administración de Cuentas** — administre las unidades, usuarios, dispositivos y más de su cuenta (solo la ven los gestores y administradores).
- **Geocercas** — dibuje y administre zonas en el mapa (solo aparece cuando el geocercado está activado para su cuenta).
- **Reportes** — genere y exporte reportes.
- **Integración GPS** — supervise los proveedores de GPS y la sincronización de dispositivos (solo la ven los gestores y administradores).
- **Cuenta** — un encabezado de sección, seguido de:
  - **Perfil** — su configuración personal.

Para abrir una pantalla, **haga clic en su nombre en el menú lateral izquierdo**. El elemento que está viendo actualmente queda resaltado. Qué elementos aparecen para cada rol se trata en [Roles y permisos](topic:roles-and-permissions).

**Menú compacto vs. completo.** En ventanas angostas el menú se contrae a una franja delgada de íconos para ahorrar espacio. Cuando está contraído, **pase el mouse sobre él** para expandirlo temporalmente, y aléjelo para contraerlo de nuevo. También puede fijar su preferencia de forma permanente con el interruptor **Contraer Barra Lateral** — vea [Perfil y configuración](topic:profile-and-settings#apariencia-idioma-y-el-menú-compacto).

## La barra superior

La barra superior muestra, de izquierda a derecha:

- La **ruta de navegación**, que le indica dónde se encuentra (vea más abajo).
- Un **ícono de menú** en el que puede hacer clic para contraer o expandir el menú lateral izquierdo.
- Una **campana de notificaciones** con un contador rojo de notificaciones sin leer.
- Un enlace **Cerrar Sesión** (con un pequeño ícono redondo de persona) para salir de la aplicación.

### La campana de notificaciones

La campana (rotulada **Notificaciones**) está junto a **Cerrar Sesión**. Una insignia roja indica cuántas notificaciones no ha leído todavía. Haga clic en la campana para abrir la lista de sus notificaciones recientes; cada una muestra su tipo de evento, su severidad y cuándo llegó. Al hacer clic en una notificación se marca como leída. Cuando no hay nada que mostrar, la lista dice **No hay notificaciones**.

## Ruta de navegación

En la parte superior izquierda, junto a un pequeño ícono de **inicio**, TrackHub muestra el rastro de dónde se encuentra (la pantalla actual). Haga clic en el **ícono de inicio** para volver al comienzo, o haga clic en un paso anterior del rastro para regresar a él. El nombre de la pantalla actual también se muestra en negrita justo debajo del rastro.

## Cerrar sesión

1. Haga clic en **Cerrar Sesión** en la esquina superior derecha de la barra superior (junto al ícono redondo de persona).
2. TrackHub finaliza su sesión de forma segura y lo regresa a la **página de inicio de sesión**.

Cierre siempre la sesión cuando termine, especialmente en una computadora compartida o pública.

## Permanecer con la sesión iniciada y expiración de sesión

Normalmente no tiene que iniciar sesión de nuevo mientras usa TrackHub de forma activa:

- Su sesión utiliza un pase de acceso temporal que **vence después de un tiempo por seguridad**.
- Antes de que se convierta en un problema, TrackHub **lo renueva silenciosamente en segundo plano**. No verá ninguna interrupción y no necesita hacer nada.
- Si la renovación **no** puede completarse —por ejemplo, si ha estado ausente durante mucho tiempo o el servicio de inicio de sesión no está accesible—, TrackHub no puede mantener su sesión iniciada. En ese caso, **lo regresa a la página de inicio de sesión** para que inicie sesión de nuevo. Vuelva a ingresar cualquier trabajo sin guardar en la pantalla actual después de regresar.

Si alguna vez es devuelto de forma inesperada a la página de inicio de sesión, simplemente inicie sesión de nuevo para continuar.
