---
id: public-links
title: Enlaces públicos
description: Otorgue acceso limitado y con tiempo definido a un recurso, sin inicio de sesión, con un token que puede revocar cuando quiera.
category: administration
screens: [manageAdmin]
related: [management-overview, dashboard-live-map]
tags: [enlaces públicos, token, revocar, accesos, vencimiento]
order: 50
---

# Enlaces públicos

Los enlaces públicos (tokens de acceso compartido) le permiten otorgar acceso limitado y con tiempo definido a un recurso específico **sin necesidad de iniciar sesión**. Los administra en la sección **Enlaces Públicos** de la pantalla **Administración de Cuentas**, dentro del grupo **Documentos y Compartidos**.

## La lista

Cada enlace existente se muestra con estas columnas:

| Columna | Significado |
|---|---|
| **Recurso** | El recurso al que apunta el enlace (su tipo e id). |
| **Ámbitos (separados por coma)** | Los ámbitos que el token puede usar. |
| **Vence En** | Cuándo deja de funcionar el enlace. |
| **Accesos** | Cuántas veces se ha usado el enlace. |
| **Estado** | **Activo**, o **Revocado En** una vez que el enlace ha sido revocado. |
| **Acción** | **Revocar Enlace Público**, visible mientras el enlace sigue activo. |

## Crear un enlace

Presione el icono **+** (agregar) en el encabezado de la sección para abrir el diálogo **Crear Enlace Público**, y complete:

- **Tipo de Recurso** (obligatorio)
- **Id del Recurso** (obligatorio)
- **Ámbitos (separados por coma)** (obligatorio)
- **Propósito**
- **Vence En** (obligatorio)

Presione **Guardar**. El portal muestra entonces el **Token de Enlace Público** una sola vez, con la advertencia **"Copie este token ahora. No se mostrará de nuevo."** Cópielo de inmediato — el token no puede recuperarse más tarde. Cierre el diálogo cuando lo tenga.

> El icono **+** aparece solo cuando la funcionalidad de enlaces públicos está habilitada para su cuenta. Aun cuando no lo esté, todavía puede ver, monitorear y **revocar** los enlaces existentes — solo la creación de nuevos enlaces está restringida.

## Revocar un enlace

Para detener un enlace activo, presione **Revocar Enlace Público** en su fila. El enlace se deshabilita de inmediato, su **Estado** pasa a revocado y no puede volver a usarse — no hay forma de deshacer la revocación, así que emita un nuevo enlace si aún se necesita acceso.

## Compartir relacionado

Los documentos compartidos usan el mismo mecanismo de enlace público internamente: compartir un archivo desde el panel de documentos genera un enlace acotado a ese documento. Vea [Documentos](topic:documents). Los enlaces públicos también sirven para exponer una vista en vivo limitada sin entregar una cuenta; para lo que su flota muestra en el mapa, vea [Mapa en vivo](topic:dashboard-live-map).

## Quién puede usar esto

Enlaces Públicos reside dentro de **Administración de Cuentas**, disponible para los Gestores con los permisos correspondientes. Vea [Vista general de administración](topic:management-overview).
