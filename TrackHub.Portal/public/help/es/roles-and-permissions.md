---
id: roles-and-permissions
title: Roles y permisos
description: Quién puede ver y hacer qué — los tres roles del portal, cómo los grupos limitan lo que ve, las funcionalidades que ocultan partes de la app y cómo se ve una cuenta suspendida.
category: getting-started
screens: []
related: [getting-started, users-roles-groups, system-administration, feature-catalog, admin-account-setup]
tags: [roles, permisos, funcionalidades, grupos, estado de cuenta, acceso]
order: 20
---

# Roles y permisos

Lo que usted puede ver y hacer en TrackHub lo deciden tres cosas en conjunto: su **rol** (Usuario, Gestor o Administrador), los **grupos** a los que pertenece y las **funcionalidades** de su cuenta (qué capacidades están activadas). Una cuenta suspendida bloquea a todos sin importar el rol.

## Quién usa el portal

Solo los **usuarios** inician sesión en este portal web. Los miembros del **personal** (conductores, operadores, empleados de campo) usan en su lugar la aplicación móvil del personal y nunca ven el portal, y los sistemas conectados (**clientes de servicio**) nunca inician sesión.

## Los tres roles del portal

A cada usuario del portal se le asignan uno o más **roles**. TrackHub incluye tres:

| Rol | Para quién es | En términos sencillos |
|------|---------------|----------------|
| **Administrador** | Administrador de la plataforma | Puede hacer todo, en todas las cuentas, incluida la administración a nivel de plataforma. |
| **Gestor** | Administrador de la cuenta | Administra su propia cuenta: unidades, personal, usuarios, grupos, proveedores GPS, documentos, alertas y más. Ve todas las unidades de la cuenta. |
| **Usuario** | Operador cotidiano | Usa las pantallas del día a día — el mapa en vivo, los viajes, las geocercas y los reportes — para las unidades de sus grupos. |

Puede consultar sus roles en su pantalla **Perfil**, junto a las **Políticas** de acceso a las que pertenece. Vea [Perfil y configuración](topic:profile-and-settings#sus-roles-y-políticas).

En una configuración típica, un Administrador también es Gestor, por lo que un Administrador ve todo lo que ve un Gestor **más** Administrador del Sistema.

## Los grupos deciden qué unidades ve un Usuario

Un **Usuario** sencillo solo ve las unidades (y el personal) de los **grupos** a los que pertenece. Un Usuario sin grupos ve un mapa vacío y reportes vacíos. Los Gestores y Administradores siempre ven todas las unidades de su cuenta. Los grupos se administran en **Administración de Cuentas → Grupos** — vea [Usuarios, roles y grupos](topic:users-roles-groups#grupos).

## Lo que ve cada rol en el menú lateral

| Elemento del menú lateral | **Usuario** cotidiano | **Gestor** | **Administrador** |
|----------------|:-----------------:|:-----------:|:-----------------:|
| **Tablero** | Sí | Sí | Sí |
| **Viajes** *(solo si la gestión de viajes está habilitada)* | Sí | Sí | Sí |
| **Geocercas** *(solo si el geocercado está habilitado)* | Sí | Sí | Sí |
| **Reportes** | Sí | Sí | Sí |
| **Estado de la Plataforma** | Sí | Sí | Sí |
| **Perfil** | Sí | Sí | Sí |
| **Administración de Cuentas** | — | Sí | Sí |
| **Integración GPS** | — | Sí | Sí |
| Ícono **Configuración** en la barra superior (opciones del mapa) | — | Sí | Sí |
| **Administrador del Sistema** | — | — | Sí |

Si intenta abrir una pantalla que su rol no permite, TrackHub lo regresa al **Tablero**.

El despacho es deliberadamente una tarea de operador cotidiano: un **Usuario** sencillo puede crear viajes, planificar rutas, asignar un miembro del personal y compartir un enlace de seguimiento para las unidades de sus grupos.

Dentro de una pantalla, una sección, un botón **Agregar** o una acción de fila también puede faltar porque su rol o sus políticas no incluyen ese permiso en particular.

## Funcionalidades de cuenta que activan o desactivan partes de la aplicación

Aparte de su rol, **toda su cuenta** tiene un conjunto de **funcionalidades** que el administrador de la plataforma activa o desactiva. Los Gestores pueden ver cuáles están activas en **Administración de Cuentas → Funcionalidades de Cuenta** (solo lectura).

| Funcionalidad | Qué controla |
|---------|------------------|
| **Integración GPS** | Cómo se recopilan las posiciones, más los reportes GPS. El elemento de menú **Integración GPS** permanece en ambos casos. |
| **Historial de Posiciones GPS** | El almacenamiento y la reproducción del recorrido pasado de una unidad. |
| **Geocercado** | Todo el elemento de menú **Geocercas** y las zonas del mapa. |
| **Gestión de Viajes** | Todo el elemento de menú **Viajes**, los enlaces de seguimiento para clientes y los reportes de viajes. |
| **Móvil del Personal** | Nada cambia en el portal web. |
| **Enlaces Públicos** | La sección **Enlaces Públicos**. |
| **Documentos** | La sección **Documentos** y los reportes de documentos. |
| **Notificaciones** | Las reglas, suscripciones, plantillas y entregas de notificaciones. |
| **Notificaciones por Correo** | El correo como canal de entrega. |
| **Notificaciones por WhatsApp** | WhatsApp como canal de entrega. |
| **Personal** | Las habilitaciones y asignaciones del personal, los avisos de vencimiento y los reportes de personal. |

**Geocercado** y **Gestión de Viajes** son las dos funcionalidades que ocultan un elemento completo del menú lateral. Las demás ocultan una sección, un botón o un canal de entrega dentro de una pantalla. Para saber exactamente qué activa y desactiva cada funcionalidad, vea el [Catálogo de funcionalidades](topic:feature-catalog).

## Estado de la cuenta: cómo se ve una cuenta suspendida

Cada cuenta se encuentra en uno de cinco estados:

| Estado | ¿Pueden las personas usar la aplicación? |
|--------|-------------------------|
| **Prueba** | Sí |
| **Activa** | Sí |
| **Suspendida** | No |
| **Cancelada** | No |
| **Archivada** | No |

Cuando su cuenta está **Suspendida**, **Cancelada** o **Archivada**, cada usuario de la cuenta ve una única pantalla **Cuenta No Disponible** con el mensaje:

> Esta cuenta no está operativa actualmente. Por favor, contacte a soporte para obtener ayuda.

junto con el **Estado** actual. No hay nada más disponible hasta que la cuenta vuelva a estar **Activa**. Contacte al soporte de TrackHub o al administrador de su cuenta. Vea también [Solución de problemas](topic:troubleshooting#mi-cuenta-no-está-operativa).
