---
id: management-overview
title: Administración de Cuentas
description: Donde un Gestor configura su propia cuenta — los seis grupos de secciones, cómo funcionan las secciones plegables y quién puede abrir la pantalla.
category: administration
screens: [manageAdmin]
related: [users-roles-groups, units-devices, documents, public-links, alerts-notifications, admin-account-setup]
tags: [administración, cuenta, secciones, permisos]
order: 10
---

# Administración de Cuentas

**Administración de Cuentas** es donde configura todo lo relativo a su propia cuenta — usuarios, accesos, flota, alertas, documentos, personalización y monitoreo — desde una sola pantalla. Ábrala desde **Administración de Cuentas** en el menú lateral.

Este tema es una orientación de la pantalla. Cada área enlaza a un tema más detallado cuando existe.

Si su cuenta es nueva y está recorriéndola por primera vez, siga [Configuración de su cuenta](topic:admin-account-setup) en su lugar — pone estas secciones en el orden en que deben hacerse.

## Quién puede abrirla

Administración de Cuentas está disponible solo para los **Gestores**. Si no tiene la capacidad de Gestor, la entrada del menú queda oculta y al ir a la dirección directamente se le regresa al tablero. El botón flotante de configuración (configurador de cuenta) también es exclusivo de los Gestores. Vea [Roles y permisos](topic:roles-and-permissions).

Si su cuenta está suspendida o cancelada, todo el portal muestra una pantalla de estado en lugar de las secciones de administración.

## Cómo funcionan las secciones

La pantalla está organizada en seis **grupos** con etiqueta, siempre en el mismo orden. Bajo el encabezado de cada grupo hay una pila de **secciones plegables** (acordeones):

- Haga clic en el encabezado de una sección para expandirla. La mayoría de las secciones cargan sus datos la primera vez que las abre.
- Cuando una sección permite agregar registros, aparece un botón **+** (agregar) en su encabezado una vez que la sección está expandida.
- Dentro de una sección, las filas tienen sus propias acciones — normalmente **Editar** y **Eliminar**, además de botones **Asignar** donde una sección vincula registros entre sí. Las eliminaciones piden confirmación primero.

## Los seis grupos

### Cuenta y Suscripción

- **Cuenta** — los datos principales de su cuenta (**Nombre**, **Descripción**, **Tipo**, **Modificado**). **Editar** abre el diálogo **Detalle de la Cuenta** para cambiar el nombre o la descripción.
- **Personalización** — cómo se ve su cuenta en el portal y en las exportaciones: **Nombre para Mostrar**, **Documento del Logo**, **Color Primario** (un valor hexadecimal de 6 dígitos con vista previa en vivo) y un encabezado de reporte/exportación.
- **Funcionalidades de Cuenta** — una lista de solo lectura de las funcionalidades opcionales aprovisionadas para su cuenta. Los Gestores pueden ver esta lista; activar o desactivar funcionalidades lo gestiona el equipo de plataforma. Vea [Administración del sistema](topic:system-administration).

### Flota y Rastreo

- **Dispositivos** y **Unidades** — sus dispositivos de rastreo y las unidades para las que reportan. Vea [Unidades y dispositivos](topic:units-devices).
- **Conductores** — los conductores asociados con sus unidades.
- **Grupos** — organice unidades y usuarios juntos, por ejemplo por sucursal o región. Vea [Usuarios, roles y grupos](topic:users-roles-groups).
- **Puntos de Interés** — ubicaciones con nombre (sitios de clientes, bodegas, estaciones de combustible, talleres, etc.) que pueden mostrarse en el mapa.

### Usuarios y Accesos

- **Usuarios**, **Roles** y **Políticas** — las personas que pueden iniciar sesión, y los roles y políticas que deciden lo que pueden hacer. Vea [Usuarios, roles y grupos](topic:users-roles-groups).

### Alertas y Notificaciones

- **Reglas de Notificación**, **Suscripciones de Alerta**, **Plantillas de Notificación**, **Eventos de Alerta** y **Entregas de Notificación** — las reglas que generan alertas, quién está suscrito, las plantillas de mensaje, las alertas en sí y el registro de lo que se envió. Vea [Alertas y notificaciones](topic:alerts-notifications).

### Documentos y Compartidos

- **Documentos** — almacene, busque y administre los documentos de su cuenta. Vea [Documentos](topic:documents).
- **Enlaces Públicos** — otorgue acceso limitado y con tiempo definido a un recurso sin iniciar sesión. Vea [Enlaces públicos](topic:public-links).

### Operación y Monitoreo

- **Auditoría** — un registro de solo lectura de las acciones relevantes para la seguridad realizadas en su cuenta (quién hizo qué).
- **Procesos en Segundo Plano** — una vista de solo lectura de los procesos automatizados que se ejecutan para su cuenta, con su estado e intentos.

## Si falta una sección o un botón

Una sección, un botón **+** o una acción de fila puede faltar porque no tiene el permiso correspondiente, o porque la funcionalidad relacionada no está habilitada para su cuenta. En particular, Enlaces Públicos y Documentos solo aparecen con la funcionalidad correspondiente activada. Vea [Solución de problemas](topic:troubleshooting).
