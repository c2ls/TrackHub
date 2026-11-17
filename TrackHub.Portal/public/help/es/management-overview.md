---
id: management-overview
title: Administración de Cuentas
description: Donde un Gestor configura su propia cuenta — los seis grupos de secciones, para qué sirve cada sección y quién puede abrir la pantalla.
category: administration
screens: [manageAdmin]
related: [users-roles-groups, units-devices, staff-workforce, documents, public-links, alerts-notifications, admin-account-setup]
tags: [administración, cuenta, secciones, festivos, puntos de interés]
order: 10
---

# Administración de Cuentas

**Administración de Cuentas** es donde usted configura todo lo de su propia cuenta — usuarios, accesos, flota, personal, alertas, documentos, personalización y monitoreo — desde una sola pantalla. Ábrala desde **Administración de Cuentas** en el menú lateral.

¿Cuenta nueva? Siga mejor [Configuración de su cuenta](topic:admin-account-setup): presenta estas secciones en el orden en que deben hacerse.

## Quién puede abrirla

Solo los **Gestores** ven Administración de Cuentas. Si va a su dirección sin el rol de Gestor, vuelve al Tablero. Vea [Roles y permisos](topic:roles-and-permissions).

## Cómo funciona la pantalla

Una **lista de secciones** a la izquierda nombra cada sección bajo su grupo; la sección que elija se abre a la derecha, una a la vez.

![Administración de Cuentas con la lista de secciones y la sección Unidades abierta](assets/es/units-list.png)

- Cuando una sección permite crear registros, aparece un botón **Agregar** arriba a la derecha de la sección.
- Las filas tienen sus propias acciones — normalmente **Editar** y **Eliminar**, más **Asignar** cuando una sección relaciona registros entre sí. Las eliminaciones piden confirmación.

## Los seis grupos

### Cuenta y Suscripción

- **Cuenta** — el **Nombre**, la **Descripción**, el **Tipo** y la fecha **Modificado** de su cuenta. **Editar** abre **Detalle de la Cuenta**, donde cambia el nombre, la descripción y la **Zona horaria**. Toda fecha que TrackHub calcula para su cuenta — "hoy", totales diarios, rangos de reportes — se interpreta en esa zona horaria.
- **Personalización** — **Nombre para Mostrar**, **Documento del Logo**, **Color Primario** (con **Vista Previa**) y **Encabezado de Reporte / Exportación**.
- **Funcionalidades de Cuenta** — una lista de solo lectura de las funcionalidades activadas para su cuenta. Vea el [Catálogo de funcionalidades](topic:feature-catalog).
- **Calendario de Festivos** — los festivos de su empresa, año por año. Elija el **Año**, agregue cada festivo con su **Fecha** y su **Nombre**, y presione **Guardar año**. Al guardar se reemplaza el año completo, así que agregue también los domingos si su empresa los trata como festivos.

### Flota y Rastreo

- **Dispositivos** y **Unidades** — el hardware GPS y lo que usted rastrea. Vea [Unidades y dispositivos](topic:units-devices).
- **Personal** — las personas que trabajan para su cuenta, con sus credenciales de la aplicación móvil. Con la funcionalidad de personal, también **Habilitaciones del Personal**, **Asignaciones del Personal** y **Vencimientos de Habilitaciones (30 días)**. Vea [Personal y fuerza de trabajo](topic:staff-workforce).
- **Grupos** — ponga unidades y personal en grupos, y entregue los grupos a los usuarios. Vea [Usuarios, roles y grupos](topic:users-roles-groups#grupos).
- **Puntos de Interés** — lugares con nombre, como sitios de clientes, bodegas, estaciones de combustible y talleres, con **Tipo**, **Coordenadas**, **Color** y **Grupo** opcional. Un punto de interés sin grupo es visible para todos en la cuenta; uno con grupo, solo para los miembros de ese grupo. Pueden mostrarse en el mapa en vivo y usarse como paradas de viajes.
- **Clases de peaje por vehículo** — aparece con la gestión de viajes. Asocie sus tipos de vehículo, o un vehículo, a una clase de vehículo para peajes, para que los viajes nuevos tengan estimación de peajes. Vea [Viajes y planificación de rutas](topic:trip-management#clases-de-peaje-para-su-flota).

### Usuarios y Accesos

- **Usuarios**, **Roles** y **Políticas** — quién puede iniciar sesión y qué puede hacer. Vea [Usuarios, roles y grupos](topic:users-roles-groups).

### Alertas y Notificaciones

- **Reglas de Notificación**, **Suscripciones de Alertas**, **Plantillas de Notificación**, **Eventos de Alerta** y **Entregas de Notificaciones**. Vea [Alertas y notificaciones](topic:alerts-notifications).

### Documentos y Compartidos

- **Biblioteca de Documentos**, **Documentos por Vencer** y **Tipos de Documento**. Vea [Documentos](topic:documents).
- **Enlaces Públicos** — acceso con plazo a un recurso sin iniciar sesión. Vea [Enlaces públicos](topic:public-links).

### Operación y Monitoreo

- **Auditoría** — un registro de solo lectura de quién hizo qué en su cuenta (**Actor**, **Recurso**, **Resultado**, **Ocurrió En**). Presione **Cargar más** para ver entradas anteriores.
- **Procesos en Segundo Plano** — una vista de solo lectura de las tareas automáticas que se ejecutan para su cuenta, con su **Estado**, **Intentos** e **Inicio**.

## Si falta una sección o un botón

Una sección, un botón **Agregar** o una acción de fila puede faltar porque su rol no incluye ese permiso, o porque la funcionalidad relacionada no está activada para su cuenta. Documentos, Enlaces Públicos, la mayoría de las secciones de notificaciones, las secciones de personal y Clases de peaje por vehículo solo aparecen con su funcionalidad activada. Vea [Solución de problemas](topic:troubleshooting#falta-una-funcionalidad-en-mi-menú).
