---
id: alerts-notifications
title: Alertas y notificaciones
description: Vea los eventos de alerta registrados, gestiónelos, defina reglas de notificación y lea la campana de la barra superior.
category: administration
screens: [manageAdmin]
related: [geofences, units-devices]
tags: [alertas, notificaciones, reconocer, resolver, reglas, campana]
order: 60
---

# Alertas y notificaciones

TrackHub registra las condiciones que necesitan atención como **eventos de alerta**, y puede avisar automáticamente a las personas correctas mediante **reglas de notificación**. Ambas residen en el grupo **Alertas y Notificaciones** de la pantalla **Administración de Cuentas**, y las notificaciones en la aplicación también le llegan por la campana de la barra superior.

## Eventos de alerta vs mensajes de error

Dos cosas distintas pueden parecer "una alerta":

- Los **eventos de alerta** son condiciones registradas que TrackHub conserva hasta que alguien las atiende — una conexión GPS perdida, un documento por vencer, una violación de geocerca. Permanecen en una lista con un **Estado** y usted las gestiona.
- Los **mensajes de error (toasts)** son los pequeños avisos emergentes que aparecen brevemente cuando una acción falla (por ejemplo, cuando una pantalla no se pudo cargar). Son momentáneos y **no** se almacenan — no hay nada que reconocer.

## La campana de notificaciones

El icono de campana en la barra de navegación superior es su feed personal dentro de la aplicación. Una insignia roja muestra cuántas notificaciones sin leer tiene. Haga clic en la campana para abrir la lista; cada entrada muestra el **tipo de evento** y una línea de **severidad · hora** (crítica en rojo, advertencia en ámbar, el resto en azul). Al hacer clic en una entrada sin leer, se marca como leída y desaparece del contador. Cuando no hay nada que mostrar, el menú dice **"No hay notificaciones"**.

La campana refleja las notificaciones que le fueron entregadas a usted personalmente. Para gestionar el registro de alertas de la cuenta, use **Eventos de Alerta** más abajo.

## Eventos de Alerta

Abra **Administración de Cuentas**, vaya al grupo **Alertas y Notificaciones** y expanda **Eventos de Alerta**. Esta es la lista de trabajo donde reconoce y resuelve alertas.

- Columnas: **Tipo**, **Estado**, **Modificado** y una columna de acción.
- Acciones de fila:
  - **Reconocer Alerta** — marca una alerta abierta como vista y en revisión. Se muestra mientras la alerta sigue abierta.
  - **Resolver Alerta** — la marca como atendida. Se muestra hasta que la alerta se resuelve.

Una alerta que ha reconocido aún puede resolverse después. Una vez que una alerta está **resuelta**, no se ofrecen más acciones. La lista se actualiza después de cada acción.

> Las alertas de conexión GPS también aparecen en modo solo lectura en la pantalla **Integración GPS** para que pueda revisar la salud de las conexiones allí, pero reconocer y resolver se hace aquí. Vea [Integración GPS](topic:gps-integration).

## Reglas de Notificación

Las reglas de notificación deciden **a quién se le avisa, y cómo, cuando ocurre un evento** — de modo que usted no tenga que vigilar la lista de alertas por su cuenta. Expanda **Reglas de Notificación** en el mismo grupo.

Cada fila de regla muestra su **Clave**, su **Tipo**, si está habilitada (**Estado**) y cuándo se **Modificó** por última vez, con las acciones **Editar** y **Deshabilitar**. **Deshabilitar** desactiva una regla habilitada sin eliminarla.

Para crear una regla, presione el icono **+** (agregar) en la sección y complete el diálogo **Crear Regla de Notificación**. **No** escribe JSON — todo se elige mediante campos estructurados:

- **Clave** (obligatorio) — un identificador corto para la regla.
- **Tipo** (obligatorio) — la clase de regla.
- **Evento Disparador** (obligatorio) — el evento que dispara la regla, elegido de una lista (ver abajo).
- **Canales** — marque los canales de entrega: **En la Aplicación**, **Correo**, **Webhook** y **WhatsApp**. Al seleccionar **Webhook** aparecen los campos **URL del Webhook** y **Secreto del Webhook**.
- **Destinatarios** — marque los roles a notificar (**Administrador**, **Gestor**) y/o **Notificar a los suscriptores**.
- **Contactos adicionales** — presione **Agregar contacto** para añadir destinatarios puntuales, cada uno con un canal (Correo o WhatsApp) y una **Dirección**; elimine una fila con su icono de eliminar.
- **Limitación** — límites opcionales para que las personas no se saturen: **Ventana de Deduplicación (minutos)**, **Resumen** (Ninguno, Cada hora o Diario) y **Máximo por Hora**.
- **Habilitada** — marque para activar la regla.

Presione **Guardar** para crear la regla, o **Guardar** en el diálogo **Actualizar Regla de Notificación** al editar.

### Eventos disparadores

La lista **Evento Disparador** cubre los eventos que la plataforma puede generar:

| Evento disparador | Se genera cuando |
|---|---|
| **Entrada a Geocerca** / **Salida de Geocerca** | Una unidad cruza el límite de una geocerca. |
| **Permanencia Excedida en Geocerca** | Una unidad permanece demasiado tiempo dentro de una geocerca. |
| **Pérdida de Comunicación** | Un dispositivo deja de reportar. |
| **Credencial GPS por Expirar** | Una credencial de integración GPS está por expirar. |
| **Fallo de Sincronización del Operador GPS** | Una sincronización del operador GPS no se completó. |
| **Documento por Vencer** / **Documento Vencido** | Un documento controlado se acerca o pasa su vencimiento. |
| **Fallo de Entrega de Notificación** | Una notificación no pudo entregarse. |

Los eventos de geocerca provienen de las geocercas que usted define — vea [Geocercas](topic:geofences). Los eventos de pérdida de comunicación y de documentos se relacionan con sus [Unidades y dispositivos](topic:units-devices).

> El icono **+** en **Reglas de Notificación** aparece solo cuando la funcionalidad de notificaciones está habilitada para su cuenta. Las opciones de canal **Correo** y **WhatsApp** aparecen solo cuando esos canales están provisionados; el aprovisionamiento lo gestiona el equipo de la plataforma en [Administración del sistema](topic:system-administration).

## Otras secciones de este grupo

El grupo **Alertas y Notificaciones** también contiene **Suscripciones de Alertas** (quién sigue qué tipos de evento en qué canal), **Plantillas de Notificación** (personalice el asunto y el cuerpo de un mensaje por canal e idioma) y **Entregas de Notificaciones** (un historial de solo lectura de lo que se envió, entregó o falló). Son configuración avanzada y normalmente las configura un administrador.

## Quién puede usar esto

Los Eventos de Alerta y las Reglas de Notificación residen dentro de **Administración de Cuentas** y requieren los permisos de gestión correspondientes. Vea [Vista general de administración](topic:management-overview).
