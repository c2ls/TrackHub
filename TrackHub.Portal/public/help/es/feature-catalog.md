---
id: feature-catalog
title: Catálogo de funcionalidades
description: Las once funcionalidades de cuenta — qué activa o desactiva cada una, sus ajustes, quién puede cambiarlas y qué ocurre cuando una se desactiva.
category: reference
screens: []
related: [roles-and-permissions, admin-platform-setup, admin-account-setup, system-administration, staff-workforce]
tags: [funcionalidades, ajustes, referencia, catálogo]
order: 15
---

# Catálogo de funcionalidades

Cada cuenta tiene un conjunto de **funcionalidades** que deciden qué partes de TrackHub existen para esa cuenta. Son once, y esta página es la referencia de todas ellas.

Dos reglas se aplican a todas:

- **Solo un administrador de la plataforma puede cambiarlas**, en **Administrador del Sistema → Funcionalidades de Cuenta**. Los Gestores ven la lista de su propia cuenta en solo lectura en **Administración de Cuentas → Funcionalidades de Cuenta**.
- **Las funcionalidades están desactivadas salvo que se activen.** Una funcionalidad que nunca se agregó a una cuenta se comporta exactamente igual que una desactivada.

## Las once funcionalidades de un vistazo

| Funcionalidad | Ajustes | Qué controla |
|---|---|---|
| Geocercado | — | Todo el elemento de menú **Geocercas**, las alertas de geocerca y dos reportes |
| Gestión de Viajes | Ciclo automático de los viajes (vea más abajo) | Todo el elemento de menú **Viajes**, los enlaces de seguimiento para clientes, las clases de peaje y seis reportes |
| Móvil del Personal | — | Nada en el portal web |
| Enlaces Públicos | — | La sección **Enlaces Públicos** |
| Documentos | — | Las secciones de **Documentos** y cuatro reportes de documentos |
| Notificaciones | — | Las reglas, suscripciones, plantillas y entregas de notificaciones, y todo el envío de alertas |
| Notificaciones por Correo | — | El correo como canal de entrega |
| Notificaciones por WhatsApp | — | WhatsApp como canal de entrega |
| Integración GPS | Intervalo de Almacenamiento (Segundos) | Cómo se recopilan las posiciones, y los reportes GPS |
| Historial de Posiciones GPS | Días de Retención | El historial almacenado, la reproducción desde TrackHub y un reporte |
| Personal | Bloquear la asignación cuando la licencia del personal esté vencida | Las habilitaciones y asignaciones del personal, los avisos de vencimiento y tres reportes |

Dependencias que conviene tener presentes:

- **Notificaciones por Correo** y **Notificaciones por WhatsApp** solo importan mientras **Notificaciones** está activada.
- **Historial de Posiciones GPS** normalmente va junto con **Integración GPS**: el historial se construye con las posiciones que recopila la integración.
- **Personal** amplía el área del personal; el registro del personal y sus credenciales funcionan sin ella.

## Geocercado

Cuando está desactivada:

- **Geocercas** desaparece del menú lateral, y la cifra **En Geocerca** y el chip **Geocercas** desaparecen del mapa en vivo.
- No se generan alertas de entrada, salida ni permanencia en geocercas.
- Desaparecen los reportes **Unidades en Geocerca** y **Eventos de Geocerca**.

Al volver a activarla, las geocercas que dibujó siguen ahí.

## Gestión de Viajes

Cuando está desactivada:

- **Viajes** desaparece del menú lateral, y **Clases de peaje por vehículo** desaparece de Administración de Cuentas.
- Los viajes ya no se inician, avanzan ni completan automáticamente.
- Los enlaces de seguimiento para clientes dejan de funcionar: el cliente ve el mensaje de "enlace no válido".
- Desaparecen los seis reportes de viajes y la categoría de reportes **Viajes**.
- Se detienen los recordatorios de inicio y los avisos de retraso de los viajes.

Se conservan los viajes, las paradas, las entregas, las pruebas de entrega y los enlaces emitidos. Al volver a activarla, el tablero queda como estaba. El **Catálogo de peajes** de la plataforma no se ve afectado.

### Ajustes de Gestión de Viajes

Deciden cómo se comporta el ciclo automático para la cuenta:

| Ajuste | Predeterminado |
|---|---|
| **Iniciar y completar los viajes automáticamente con las zonas** | Activado |
| **Empezar a vigilar un viaje estos minutos antes de su inicio planeado** | 60 |
| **Minutos después del inicio planeado antes de marcar el viaje como sin iniciar a tiempo** | 120 |
| **Minutos estacionado en la última parada antes de que el viaje se cierre solo** | 30 |
| **Cuántas horas atrás buscar una salida registrada** | 24 |
| **Minutos después de la ventana prometida antes de contar una parada como retrasada** | 15 |
| **Minutos antes del inicio planeado para enviar el recordatorio** | 60 |

Desactive el primero para una cuenta cuyos vehículos no reportan de forma confiable: entonces todos los viajes se manejan a mano. Vea [Viajes y planificación de rutas](topic:trip-management).

## Móvil del Personal

Nada cambia en el portal web al activarla o desactivarla.

## Enlaces Públicos

Cuando está desactivada, la sección **Enlaces Públicos** desaparece de Administración de Cuentas y no se pueden crear enlaces nuevos. Los enlaces ya emitidos no se eliminan.

## Documentos

Cuando está desactivada:

- Desaparecen las secciones **Biblioteca de Documentos**, **Documentos por Vencer** y **Tipos de Documento**, y ya no se pueden subir ni abrir archivos — incluido **Documentos del Personal**.
- Se detienen los avisos de vencimiento de documentos.
- Desaparecen cuatro reportes: documentos por vencer, documentos requeridos faltantes, actividad de documentos compartidos y volumen de carga.

Los documentos almacenados nunca se eliminan.

## Notificaciones

Cuando está desactivada:

- Desaparecen **Reglas de Notificación**, **Suscripciones de Alertas**, **Plantillas de Notificación** y **Entregas de Notificaciones**. **Eventos de Alerta** permanece, porque otras partes de TrackHub también generan eventos.
- No se envía nada — ni correo, ni WhatsApp, ni webhook.
- Sus propias notificaciones en la campana siguen funcionando.

## Notificaciones por Correo

Cuando está desactivada, **Correo** se quita de los canales disponibles en las reglas de notificación y las suscripciones de alertas, y no se envían entregas por correo.

## Notificaciones por WhatsApp

Igual que Notificaciones por Correo, para **WhatsApp**.

## Integración GPS

**Ajuste:** **Intervalo de Almacenamiento (Segundos)**, predeterminado **360**.

El elemento de menú **Integración GPS** permanece en ambos casos. Lo que cambia es cómo obtiene el mapa sus posiciones:

- **Activada** — TrackHub mantiene una imagen actualizada de dónde está cada unidad, renovada en segundo plano según el intervalo de almacenamiento. El mapa carga rápido y no depende de que el proveedor responda en ese momento.
- **Desactivada** — el mapa le pregunta directamente al proveedor GPS en cada actualización. Las posiciones siguen apareciendo, pero más despacio.

Con la funcionalidad desactivada, los reportes GPS desaparecen de la lista y no se generan avisos de credenciales del proveedor por expirar.

## Historial de Posiciones GPS

**Ajuste:** **Días de Retención**, predeterminado **30**.

Cuando está desactivada:

- La opción **Fuente del historial** de la pestaña **Posiciones** del Tablero se oculta y el historial siempre se lee del proveedor GPS.
- Desaparece el reporte **Historial de Posiciones GPS**.

**Días de Retención** es cuánto tiempo se conservan las posiciones almacenadas. Reducirlo elimina las posiciones más antiguas que el nuevo límite en la siguiente limpieza diaria.

Los Gestores pueden ver ambos valores GPS, en solo lectura, en **Integración GPS → Retención de Posiciones**.

## Personal

**Ajuste:** **Bloquear la asignación cuando la licencia del personal esté vencida**, desactivado por defecto.

El registro de **Personal** y sus **Credenciales y dispositivos** funcionan sin esta funcionalidad. Personal agrega lo que se construye alrededor del personal. Cuando está desactivada:

- Desaparecen **Habilitaciones del Personal**, **Asignaciones del Personal** y **Vencimientos de Habilitaciones (30 días)**.
- No se generan alertas de **Habilitación del Personal por Vencer** ni **Habilitación del Personal Vencida**.
- Desaparecen los tres reportes y la categoría de reportes **Personal**.
- El **Transportador Predeterminado** del miembro del personal sigue funcionando.

Con el ajuste activado, asignar a un miembro del personal cuya habilitación **Licencia** está vencida o revocada se rechaza. Vea [Personal y fuerza de trabajo](topic:staff-workforce).

## Cómo se ve una funcionalidad desactivada

- Los elementos de menú, secciones, botones, canales y reportes **desaparecen** en lugar de mostrar un error.
- Si aun así una solicitud llega a TrackHub, se rechaza con **"Esta funcionalidad no está habilitada para su cuenta."**
- **Desactivar una funcionalidad no elimina datos.** Al volver a activarla, se recupera el acceso exactamente a lo que había.

Los cambios pueden tardar alrededor de un minuto en llegar a todo. Recargue la página si una funcionalidad recién activada todavía no aparece.

## Dónde se administra cada lado

| | Administrador de la plataforma | Gestor |
|---|---|---|
| Dónde | Administrador del Sistema → Funcionalidades de Cuenta | Administración de Cuentas → Funcionalidades de Cuenta |
| Activar o desactivar funcionalidades | Sí | No |
| Definir **Nivel** y ajustes | Sí | No |
| Ver las funcionalidades de la cuenta | Todas las cuentas | Solo la propia |

La lista del administrador también muestra un **Nivel** y un **Origen** para cada funcionalidad. Son etiquetas de registro y no cambian lo que hace la funcionalidad. Vea [Configuración inicial de la plataforma](topic:admin-platform-setup) y [Configuración de su cuenta](topic:admin-account-setup).
