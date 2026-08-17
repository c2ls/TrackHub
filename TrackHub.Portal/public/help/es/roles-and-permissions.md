---
id: roles-and-permissions
title: Roles y permisos
description: Quién puede ver y hacer qué — los tres roles del portal, las funcionalidades que ocultan partes de la app y cómo se ve una cuenta suspendida.
category: getting-started
screens: []
related: [getting-started, users-roles-groups, system-administration, feature-catalog, admin-account-setup]
tags: [roles, permisos, funcionalidades, estado de cuenta, acceso]
order: 20
---

# Roles y permisos

Lo que usted puede ver y hacer en TrackHub lo deciden **dos cosas en conjunto**: su **rol** (si es Usuario, Gestor o Administrador) y las **funcionalidades** de su cuenta (qué capacidades están activadas). Una cuenta suspendida bloquea a todos sin importar el rol.

## Los tres tipos de inicio de sesión

TrackHub reconoce distintos tipos de "quién está solicitando el acceso". Solo el primero usa este portal web:

- **Usuario** — una persona que inicia sesión en el portal web de TrackHub (un administrador, un gestor o un operador cotidiano). **Esta guía trata sobre los Usuarios.**
- **Conductor** — un conductor de campo que inicia sesión a través de la **aplicación móvil**, no de este portal. Los conductores no ven las pantallas del portal descritas aquí.
- **Cliente de Servicio** — una integración de máquina a máquina (otro sistema que se conecta de forma automática). Ninguna persona inicia sesión como Cliente de Servicio, y nunca usa el portal.

Como cada pantalla del portal está reservada para los **Usuarios**, a los Conductores y a los Clientes de Servicio nunca se les muestra el portal, incluso si llegan a su dirección.

## Los tres roles del portal

A cada usuario del portal se le asignan uno o más **roles**. TrackHub incluye tres roles estándar:

| Rol | Para quién es | En términos sencillos |
|------|---------------|----------------|
| **Administrador** | Superadministrador a nivel de plataforma | Puede hacer todo, en todas las cuentas, incluyendo la administración a nivel de plataforma. |
| **Gestor** | Administrador de la cuenta | Administra su propia cuenta: unidades, usuarios, dispositivos, grupos, proveedores de GPS, documentos y más. |
| **Usuario** | Operador cotidiano | Usa las pantallas del día a día: ve el mapa en vivo, ejecuta reportes y trabaja con las zonas y los puntos que tiene permitido ver. |

Puede consultar qué roles tiene en su pantalla **Perfil**, en la tarjeta **Roles**, junto a las **Políticas** de acceso a las que pertenece. Vea [Perfil y configuración](topic:profile-and-settings#sus-roles-y-políticas).

**Qué desbloquean los roles en el menú:**

- Ser **Administrador** desbloquea el área **Administrador del Sistema**.
- Ser **Gestor** desbloquea las áreas **Administración de Cuentas** e **Integración GPS**, así como el botón de engranaje de **Configuración** en la esquina inferior derecha.
- Todas las personas con acceso al portal (incluidos los **Usuarios** sencillos) pueden llegar al **Tablero**, a **Reportes**, a **Geocercas** (cuando está habilitado) y a su propio **Perfil**.

> En una configuración típica, un Administrador también es un Gestor, por lo que un Administrador ve todo lo que ve un Gestor **más** el área Administrador del Sistema.

TrackHub decide lo que usted puede ver consultando al servicio de seguridad, en segundo plano, si usted es administrador y si es gestor. Usted no lo configura por sí mismo; proviene de los roles que le han sido asignados.

## Lo que ve cada rol en el menú lateral

| Elemento del menú lateral | **Usuario** cotidiano | **Gestor** | **Administrador** |
|----------------|:-----------------:|:-----------:|:-----------------:|
| **Tablero** (mapa en vivo) | Sí | Sí | Sí |
| **Reportes** | Sí | Sí | Sí |
| **Geocercas** *(solo si el geocercado está habilitado)* | Sí | Sí | Sí |
| **Perfil** | Sí | Sí | Sí |
| **Administración de Cuentas** | — | Sí | Sí |
| **Integración GPS** | — | Sí | Sí |
| Botón de engranaje **Configuración** (inferior derecha) | — | Sí | Sí |
| **Administrador del Sistema** | — | — | Sí |

Si intenta abrir una pantalla que su rol no permite, TrackHub simplemente lo regresa al **Tablero**.

### Qué hay dentro de las áreas de gestor y administrador

**Administración de Cuentas** (Gestores y Administradores) agrupa las herramientas para administrar su propia cuenta en secciones:

- **Cuenta y Suscripción** — Detalle de la Cuenta, Personalización y Funcionalidades de Cuenta (solo lectura).
- **Flota y Rastreo** — Dispositivos, Unidades, Conductores, Grupos y Puntos de Interés.
- **Usuarios y Accesos** — Usuarios, Roles y Políticas.
- **Alertas y Notificaciones** — Reglas de Notificación, Suscripciones de Alerta, Plantillas de Notificación, Eventos de Alerta y Entregas de Notificaciones.
- **Documentos y Compartidos** — Documentos y Enlaces Públicos.
- **Operación y Monitoreo** — Auditoría y Procesos en Segundo Plano.

Vea [Resumen de Administración de Cuentas](topic:management-overview) para el detalle de cada sección.

**Integración GPS** (Gestores y Administradores) es la sala de control de sus proveedores de GPS: un panel de resumen, **Operadores GPS**, **Dispositivos Sincronizados**, **Asignaciones de Dispositivos**, **Retención de Posiciones**, **Sincronizaciones Recientes** y **Alertas GPS Abiertas**. Vea [Integración GPS](topic:gps-integration).

**Administrador del Sistema** (solo Administradores) sirve para administrar toda la plataforma entre cuentas, incluyendo **Cuentas**, **Clientes API**, **Permisos de Cliente de Servicio**, **Tipos de Unidades**, **Proveedores de Geocodificación**, **Funcionalidades de Cuenta**, **Concesiones de Soporte**, además de **Roles** y **Políticas** a nivel de plataforma. Vea [Administración del sistema](topic:system-administration).

## Funcionalidades de cuenta que activan o desactivan partes de la aplicación

Aparte de su rol personal, **toda su cuenta** tiene un conjunto de **funcionalidades** que pueden activarse o desactivarse (normalmente como parte de su plan de suscripción). Puede ver qué funcionalidades tiene su cuenta en **Administración de Cuentas → Funcionalidades de Cuenta**. Los Gestores pueden *ver* esta lista, pero activar o desactivar funcionalidades es una decisión de facturación que gestiona el administrador de la plataforma.

Las funcionalidades de cuenta son:

| Funcionalidad | Clave | Qué controla |
|---------|-------|------------------|
| **Integración GPS** | `gps.integration` | Cómo se recopilan las posiciones, más los reportes GPS. El elemento de menú **Integración GPS** permanece en ambos casos. |
| **Historial de Posiciones GPS** | `gps.positionHistory` | El almacenamiento y la reproducción del recorrido pasado de una unidad (reproducción del historial). |
| **Geocercado** | `geofencing` | Todo el elemento de menú **Geocercas** y las zonas del mapa. |
| **Gestión de Viajes** | `trip-management` | Reservada — nada cambia en la versión actual. |
| **Móvil del Conductor** | `driver-mobile` | Reservada — nada cambia en el portal web. |
| **Enlaces Públicos** | `public-links` | La creación de enlaces públicos para compartir. |
| **Documentos** | `documents` | Las superficies de gestión de documentos (subir, compartir y hacer seguimiento de archivos). |
| **Notificaciones** | `notifications` | Las reglas de notificación y la entrega de alertas. |
| **Notificaciones por Correo** | `notifications.email` | La entrega de notificaciones por correo electrónico. |
| **Notificaciones por WhatsApp** | `notifications.whatsapp` | La entrega de notificaciones por WhatsApp. |

La funcionalidad **Geocercado** es la que oculta un elemento completo del menú lateral: cuando está desactivada, **Geocercas** desaparece del menú para todos en la cuenta. Las demás funcionalidades operan a nivel de sección: cuando una funcionalidad está desactivada, su sección, sus botones o su canal de entrega quedan ocultos dentro de la pantalla correspondiente (por ejemplo, las secciones **Documentos** o **Enlaces Públicos** de Administración de Cuentas), en lugar de eliminar un elemento del menú.

Para saber exactamente qué activa y desactiva cada funcionalidad, sus ajustes y qué ocurre con sus datos al desactivar una, vea el [Catálogo de funcionalidades](topic:feature-catalog).

## Estado de la cuenta: cómo se ve una cuenta suspendida

Cada cuenta se encuentra en uno de cinco estados. Esto determina si alguien en la cuenta puede usar TrackHub en absoluto.

| Estado | ¿Pueden las personas usar la aplicación? |
|--------|-------------------------|
| **Prueba** | Sí — acceso completo durante el período de prueba. |
| **Activa** | Sí — acceso normal y completo. |
| **Suspendida** | No — la aplicación está bloqueada (vea más abajo). |
| **Cancelada** | No — la aplicación está bloqueada. |
| **Archivada** | No — la aplicación está bloqueada. |

Cuando su cuenta está **Suspendida**, **Cancelada** o **Archivada**, iniciar sesión **no** lo lleva al Tablero. En su lugar, cada usuario de la cuenta ve una única pantalla **Cuenta No Disponible** con el mensaje:

> Esta cuenta no está operativa actualmente. Por favor, contacte a soporte para obtener ayuda.

junto con el **Estado** actual. No hay menús, mapas, reportes ni herramientas de administración disponibles hasta que la cuenta vuelva a estar **Activa**. Si ve esta pantalla, contacte al soporte de TrackHub o al administrador de su cuenta. Vea también [Solución de problemas](topic:troubleshooting#mi-cuenta-no-está-operativa).

## Matriz de referencia rápida

| Capacidad | Usuario | Gestor | Administrador | ¿También necesita una funcionalidad de cuenta? |
|------------|:----:|:-------:|:-------------:|--------------------------------|
| Ver el mapa en vivo (Tablero) | Sí | Sí | Sí | — |
| Ejecutar y exportar Reportes | Sí | Sí | Sí | — |
| Ver y administrar Geocercas | Sí | Sí | Sí | Geocercado |
| Reproducir el recorrido pasado de una unidad | Sí | Sí | Sí | Historial de Posiciones GPS |
| Editar el propio Perfil y contraseña | Sí | Sí | Sí | — |
| Administrar unidades, dispositivos, usuarios, grupos | — | Sí | Sí | — |
| Administrar Documentos | — | Sí | Sí | Documentos |
| Crear Enlaces Públicos | — | Sí | Sí | Enlaces Públicos |
| Reglas de notificación y alertas | — | Sí | Sí | Notificaciones |
| Sala de control de Integración GPS | — | Sí | Sí | — |
| Configuración (opciones del mapa) | — | Sí | Sí | — |
| Administrador del Sistema a nivel de plataforma | — | — | Sí | — |

*Un guion significa que el elemento no se muestra para ese rol. Un "Sí" aún depende de que la cuenta esté **Activa** o en **Prueba**; una cuenta Suspendida, Cancelada o Archivada bloquea todo.*
