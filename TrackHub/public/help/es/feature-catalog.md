---
id: feature-catalog
title: Catálogo de funcionalidades
description: Las diez funcionalidades de cuenta — qué activa o desactiva cada una, sus ajustes, quién puede cambiarlas y qué ocurre cuando se desactiva una.
category: reference
screens: []
related: [roles-and-permissions, admin-platform-setup, admin-account-setup, system-administration]
tags: [funcionalidades, suscripción, facturación, referencia, catálogo]
order: 15
---

# Catálogo de funcionalidades

Cada cuenta tiene un conjunto de **funcionalidades**. Una funcionalidad es un derecho de suscripción: decide qué partes de TrackHub existen para esa cuenta. Hay exactamente **diez**, y esta página es la referencia de todas ellas.

Dos reglas se aplican a todas:

- **Solo un administrador de la plataforma puede cambiarlas**, en **Administrador del Sistema → Funcionalidades de Cuenta**. Los Gestores ven la lista de su propia cuenta, en modo de solo lectura, en **Administración de Cuentas → Funcionalidades de Cuenta**.
- **Las funcionalidades están desactivadas salvo que se activen.** Una funcionalidad que nunca se aprovisionó para una cuenta se comporta exactamente igual que una que se desactivó. Nada se habilita de forma implícita.

## Las diez funcionalidades de un vistazo

| Funcionalidad | Clave | Ajustes | Qué controla |
|---|---|---|---|
| Geocercado | `geofencing` | — | Todo el elemento de menú **Geocercas**, las alertas de geocerca y dos reportes |
| Gestión de Viajes | `trip-management` | — | Nada todavía — derecho reservado |
| Móvil del Conductor | `driver-mobile` | — | Nada en el portal — derecho reservado |
| Enlaces Públicos | `public-links` | — | La sección **Enlaces Públicos** y la creación de nuevos enlaces |
| Documentos | `documents` | — | La sección **Documentos** y cuatro reportes de documentos |
| Notificaciones | `notifications` | — | Reglas, suscripciones, plantillas, entregas y toda la entrega de alertas |
| Notificaciones por Correo | `notifications.email` | — | El correo electrónico como canal de entrega |
| Notificaciones por WhatsApp | `notifications.whatsapp` | — | WhatsApp como canal de entrega |
| Integración GPS | `gps.integration` | Intervalo de Almacenamiento (Segundos) | Cómo se recopilan las posiciones, y nueve reportes GPS |
| Historial de Posiciones GPS | `gps.positionHistory` | Días de Retención | El historial almacenado, la reproducción y un reporte |

Dependencias que conviene tener presentes:

- **Notificaciones por Correo** y **Notificaciones por WhatsApp** solo importan mientras la funcionalidad base **Notificaciones** esté activa. Con Notificaciones desactivada, las pantallas donde elegiría un canal no existen.
- **Historial de Posiciones GPS** normalmente acompaña a **Integración GPS**: el historial se construye con las posiciones que recopila la integración.

## Geocercado

**Clave:** `geofencing`. **Ajustes:** ninguno.

Es la única funcionalidad que elimina un elemento completo del menú lateral. Cuando está desactivada:

- **Geocercas** desaparece del menú lateral y su página no puede abrirse.
- Las geocercas existentes no se evalúan, por lo que no se generan alertas de entrada ni de salida.
- Los reportes **Unidades en Geocerca** y **Eventos de Geocerca** desaparecen de la lista de reportes.
- El tema de ayuda de Geocercas se oculta del índice de ayuda.

Cuando se vuelve a activar, las geocercas que dibujó antes siguen ahí.

## Gestión de Viajes

**Clave:** `trip-management`. **Ajustes:** ninguno.

Un derecho reservado. Aparece en ambas listas de funcionalidades y puede activarse o desactivarse, pero en la versión actual **nada cambia en el producto** en ninguno de los dos casos.

## Móvil del Conductor

**Clave:** `driver-mobile`. **Ajustes:** ninguno.

Un derecho reservado para la aplicación móvil de conductores de campo. Aparece en ambas listas, pero **nada cambia en el portal web** al conmutarla.

## Enlaces Públicos

**Clave:** `public-links`. **Ajustes:** ninguno.

Cuando está desactivada:

- La sección **Enlaces Públicos** desaparece del grupo **Documentos y Compartidos** de Administración de Cuentas.
- Se rechaza la creación de un enlace público nuevo.

Los enlaces ya emitidos **no** se eliminan y siguen pudiendo listarse y revocarse desde las interfaces que aún los alcanzan. Volver a activar la funcionalidad restaura la sección tal como estaba.

## Documentos

**Clave:** `documents`. **Ajustes:** ninguno.

Cuando está desactivada:

- La sección **Documentos** desaparece del grupo **Documentos y Compartidos** de Administración de Cuentas.
- Se rechazan la búsqueda de documentos, la lista de documentos por vencer, los documentos compartidos, los tipos de documento y el cumplimiento documental de las unidades.
- Dejan de generarse alertas de vencimiento de documentos para la cuenta.
- Cuatro reportes desaparecen de la lista: documentos por vencer, documentos requeridos faltantes, actividad de compartidos y volumen de cargas.

Los documentos almacenados nunca se eliminan al desactivar la funcionalidad.

## Notificaciones

**Clave:** `notifications`. **Ajustes:** ninguno.

La más amplia de las funcionalidades. Cuando está desactivada:

- Cuatro secciones desaparecen del grupo **Alertas y Notificaciones**: **Reglas de Notificación**, **Suscripciones de Alerta**, **Plantillas de Notificación** y **Entregas de Notificaciones**. **Eventos de Alerta** permanece visible, porque otras partes de la plataforma también generan eventos.
- Las reglas de notificación no se evalúan y no se envía ni se resume nada — ni correo, ni WhatsApp, ni webhook.
- Leer sus propias notificaciones en la aplicación desde la campana sigue funcionando.

## Notificaciones por Correo

**Clave:** `notifications.email`. **Ajustes:** ninguno.

Un derecho por canal, verificado por separado de la funcionalidad base **Notificaciones**. Cuando está desactivada, **Correo** se elimina del selector de canales en los diálogos de regla de notificación y de suscripción de alerta, y cualquier entrega que hubiera salido por correo simplemente no se envía.

## Notificaciones por WhatsApp

**Clave:** `notifications.whatsapp`. **Ajustes:** ninguno.

Se comporta exactamente igual que Notificaciones por Correo, para el canal de WhatsApp. Cuando está desactivada, **WhatsApp** se elimina de los selectores de canal y las entregas por WhatsApp no se envían.

## Integración GPS

**Clave:** `gps.integration`. **Ajuste:** **Intervalo de Almacenamiento (Segundos)**, predeterminado **360**.

Esta funcionalidad **no** oculta el elemento de menú **Integración GPS** — esa página sigue disponible para los gestores en cualquier caso. Lo que cambia es cómo obtiene el mapa sus posiciones y qué reportes existen:

- **Activada** — TrackHub mantiene una imagen almacenada y actualizada continuamente de dónde está cada unidad, refrescada en segundo plano según el intervalo de almacenamiento que configure. El mapa lee esa imagen almacenada, por lo que carga rápido y no depende de que el proveedor esté accesible en ese instante.
- **Desactivada** — el mapa consulta directamente al proveedor GPS en cada actualización. Sigue viendo posiciones, pero llegan más lentamente y dependen de que el proveedor responda.

El valor **Intervalo de Almacenamiento (Segundos)** controla cada cuánto se registran las posiciones. Un intervalo más corto significa un recorrido más detallado y más almacenamiento; uno más largo, menos detalle y menos almacenamiento.

Con la funcionalidad desactivada, nueve reportes GPS desaparecen de la lista (salud del proveedor, historial de sincronizaciones, estadísticas de sincronización, inventario de dispositivos, dispositivos recién agregados, dispositivos sin asignar, dispositivos ignorados, historial de asignaciones y frescura de la última posición), y no se generan alertas por credenciales de proveedor próximas a vencer.

## Historial de Posiciones GPS

**Clave:** `gps.positionHistory`. **Ajuste:** **Días de Retención**, predeterminado **30**.

Controla si se conserva el recorrido pasado de una unidad y si puede reproducirse. Cuando está desactivada:

- En el tablero, la elección entre posiciones almacenadas y en vivo se oculta y todo se lee en vivo del proveedor.
- Las solicitudes de historial de posiciones y de reproducción de viajes se rechazan.
- El reporte **Historial de Posiciones GPS** desaparece de la lista de reportes.
- La limpieza diaria que elimina las posiciones antiguas también se detiene, por lo que las filas existentes permanecen en disco — simplemente no pueden leerse.

**Días de Retención** es cuánto tiempo se conservan las posiciones antes de que la limpieza diaria las elimine. Aumentarlo conserva más historial y usa más almacenamiento; reducirlo es destructivo, porque las posiciones más antiguas que el nuevo límite se eliminan en la siguiente ejecución.

Los Gestores pueden ver ambos valores, en solo lectura, en **Integración GPS → Retención de Posiciones**.

## Cómo se ve una funcionalidad desactivada

TrackHub falla de forma cerrada. Si una funcionalidad no está explícitamente activa para su cuenta, todo lo que hay detrás se considera desactivado:

- Los elementos de menú, secciones, botones, canales de entrega y reportes **desaparecen** en lugar de mostrar un error.
- Si aun así una solicitud llega al servidor, se rechaza y la aplicación muestra el mensaje **"Esta funcionalidad no está habilitada para su cuenta."**
- **Desactivar una funcionalidad nunca elimina datos.** Las geocercas, los documentos, los enlaces, las reglas de notificación y el historial de posiciones sobreviven. Volver a activarla restaura el acceso exactamente a lo que había.

Los cambios tardan hasta alrededor de un minuto en llegar a toda la plataforma. Actualice la página si una funcionalidad recién habilitada aún no aparece.

## Dónde se administra cada lado

| | Administrador de la plataforma | Gestor |
|---|---|---|
| Dónde | Administrador del Sistema → Funcionalidades de Cuenta | Administración de Cuentas → Funcionalidades de Cuenta |
| Puede activar o desactivar funcionalidades | Sí | No |
| Puede fijar el Nivel y los valores de ajuste | Sí | No |
| Puede ver las funcionalidades de la cuenta | Sí, de todas las cuentas | Sí, solo la propia |

La fila del administrador para una funcionalidad también registra un **Nivel** (una etiqueta de plan en texto libre, como `default`) y un **Origen** (de dónde proviene el derecho, por ejemplo `superadmin`). Ninguno de los dos cambia lo que hace la funcionalidad — son para registro y facturación. Vea [Configuración inicial de la plataforma](topic:admin-platform-setup) para el lado del administrador y [Configuración de su cuenta](topic:admin-account-setup) para el del gestor.
