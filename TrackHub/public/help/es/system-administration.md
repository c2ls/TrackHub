---
id: system-administration
title: Administración del Sistema
description: Controles de toda la plataforma para superadministradores — cuentas, clientes API, permisos, roles, tipos de unidad, geocodificación, funciones y soporte.
category: administration
screens: [systemAdmin]
related: [roles-and-permissions, gps-integration]
tags: [admin, cuentas, clientes, funciones, roles, políticas, soporte]
order: 70
---

# Administración del Sistema

La página **Administrador del Sistema** es una sola página desplazable de secciones plegables que operan sobre toda la plataforma en lugar de una sola cuenta. Haga clic en el encabezado de una sección para expandirla; las secciones que crean registros nuevos muestran un ícono **Agregar** (**+**) en el encabezado. Esta página está restringida a los superadministradores de la plataforma — consulte [Roles y permisos](topic:roles-and-permissions). Las secciones se describen en el orden en que aparecen.

## Cuentas

La lista maestra de todas las cuentas (inquilinos) de la plataforma. Cada fila muestra el **Nombre** de la cuenta, el **Estado** (un distintivo de color) y su fecha de modificación, con una acción **Editar**, una acción **Cambiar Estado** y una acción **Agregar Usuario**.

Haga clic en el ícono **+** para abrir el diálogo **Detalle de la Cuenta** y crear una cuenta. Recopila:

- **Nombre** (obligatorio) y **Descripción**.
- El primer usuario de la cuenta: **Nombre de Usuario** (correo, obligatorio), **Contraseña** (obligatoria), **Nombre** (obligatorio), **Apellido** (obligatorio). Estos campos aparecen solo al crear una cuenta nueva.
- **Tipo** (obligatorio) y una casilla **Activo**.

Presione **Guardar**. Crear una cuenta también crea su usuario administrador inicial a partir de esos campos. Use **Editar** en una fila para cambiar el nombre, la descripción, el tipo o el estado activo de una cuenta existente (los campos de usuario se ocultan al editar).

### Cambiar el estado de una cuenta

Las cuentas atraviesan un ciclo de vida mostrado como un distintivo de color en la columna **Estado**: **Prueba**, **Activa**, **Suspendida**, **Cancelada** y **Archivada**.

Haga clic en **Cambiar Estado** en una fila para abrir el diálogo. El menú **Nuevo Estado** ofrece solo las transiciones permitidas desde el estado actual:

- Desde **Prueba** a Activa, Suspendida o Cancelada
- Desde **Activa** a Suspendida o Cancelada
- Desde **Suspendida** a Activa, Cancelada o Archivada
- Desde **Cancelada** a Activa o Archivada
- **Archivada** es final — no se ofrecen transiciones.

Ingrese un **Motivo** y presione **Guardar**. Se **requiere** un motivo al mover una cuenta a **Suspendida** o **Cancelada**; para otras transiciones es opcional. Suspender o cancelar una cuenta detiene de inmediato el uso de la aplicación por parte de sus usuarios (ven una pantalla "Cuenta No Disponible" — consulte [Solución de problemas](topic:troubleshooting)).

### Agregar un usuario a una cuenta

Cada fila de cuenta tiene una acción **Agregar Usuario**. Haga clic para abrir el diálogo de usuario ya vinculado a esa cuenta, complete los datos del usuario y **Guarde** — sin salir de Administración del Sistema.

## Clientes API

Clientes de integración que permiten a sistemas externos conectarse a la plataforma, actuando opcionalmente como un usuario vinculado. Haga clic en el ícono **+** para abrir el diálogo **Detalle del Cliente API**. Para un cliente nuevo recopila:

- **Nombre** (obligatorio), **Descripción** y **Clave Secreta** (obligatoria) — la credencial del cliente.
- **Usuario Vinculado** — opcionalmente asocie el cliente a un usuario existente (un usuario de integración).

Presione **Guardar**. Al editar un cliente existente, solo se puede cambiar el usuario vinculado; el nombre, la descripción y la clave secreta se definen al crearlo. **Eliminar** quita un cliente tras confirmar.

## Permisos de Cliente de Servicio

Reglas detalladas que indican qué cliente puede realizar qué acción sobre qué recurso, opcionalmente limitadas a una cuenta. Haga clic en el ícono **+** para abrir el diálogo **Permiso de Cliente de Servicio**. Recopila **Cliente**, **Recurso**, **Acción**, **Ámbito** y **Audiencia** (todos obligatorios, ingresados como identificadores/texto), una **Cuenta** opcional (déjela en blanco para todas las cuentas), un indicador **Activo** y una ventana opcional **Vigente Desde** / **Vigente Hasta**. Use **Editar** para cambiar una regla, o la acción de eliminar para quitarla (se le pide confirmar).

## Tipos de Unidades

Las categorías de unidades rastreadas y los umbrales de movimiento usados para interpretar sus datos. Haga clic en el ícono **+** para abrir el diálogo **Detalle del Tipo de Unidad**, que recopila:

- **Detenido (Minutos)** (obligatorio) — cuánto tiempo sin movimiento cuenta como detenido.
- **Tiempo Máximo (Minutos)** (obligatorio).
- **Máxima Distancia (Km)** (obligatorio).
- **Basado en Ignición** — si el estado detenido/en movimiento se decide a partir de la señal de ignición (ACC).

## Proveedores de Geocodificación

Los servicios usados para convertir coordenadas en direcciones; solo un proveedor está activo a la vez. Haga clic en el ícono **+** para abrir el diálogo **Detalle del Proveedor de Geocodificación**:

- **Nombre** (obligatorio).
- **Tipo** (obligatorio) — **Nominatim**, **OpenRouteService** o **Google**.
- **URI del Endpoint** (obligatorio) y una **Clave API** opcional.
- **Solicitudes por Segundo** (predeterminado 1) y **Tiempo de Espera (Segundos)** (predeterminado 5).
- **Configuración (JSON)** — ajustes avanzados opcionales.

Las acciones de fila le permiten **Editar** un proveedor, **Eliminarlo** o **Activar** uno que no esté activo actualmente (lo que desactiva el anterior).

## Roles

Los roles agrupan permisos que luego se asignan a los usuarios. Esta sección abre una matriz de permisos: elija un **Rol** del menú desplegable y luego una cuadrícula de **Recursos** (filas) y **Acciones** (columnas — Lectura, Edición, Exportar, Ejecutar, Escribir, Eliminar, Personalizado). Marque una casilla para otorgar esa acción sobre ese recurso al rol, o desmárquela para revocarla. Cada cambio se guarda a medida que lo hace.

## Políticas

Las políticas funcionan igual que los roles pero definen conjuntos de permisos reutilizables que pueden adjuntarse en otro lugar. Elija una **Política** del menú desplegable y luego marque o desmarque las casillas de **Recurso** por **Acción** para otorgar o revocar cada permiso. Los cambios se guardan por casilla.

## Funcionalidades de Cuenta

Active o desactive funciones opcionales por cuenta y configure su nivel y ajustes de almacenamiento. Este es el control maestro; los gestores solo pueden ver las funciones de su propia cuenta en Administración de Cuentas.

La lista se agrupa por cuenta y muestra, por cada función, la **Cuenta**, la **Funcionalidad**, **Habilitada** (distintivo Sí/No), el **Nivel** y el **Origen**, con una acción **Editar**. Haga clic en el ícono **+** (**Agregar Funcionalidad a la Cuenta**) para elegir una **Cuenta** y una **Funcionalidad**, y luego configure las opciones. Editar una fila existente fija la cuenta y la función y le permite cambiar:

- **Habilitada** — el interruptor de encendido/apagado de esa función en esa cuenta.
- **Nivel** — el nivel de suscripción (predeterminado `default`).
- Un valor de almacenamiento para ciertas funciones: **Intervalo de Almacenamiento (Segundos)** para la integración GPS (predeterminado 360), o **Días de Retención** para el historial de posiciones GPS (predeterminado 30).

Las funciones administrables incluyen integración GPS, historial de posiciones GPS, geocercas, gestión de viajes, móvil del conductor, enlaces públicos, documentos, notificaciones, notificaciones por correo y notificaciones por WhatsApp. Cuando una función está deshabilitada para una cuenta, sus usuarios no ven los elementos de menú relacionados y reciben un mensaje de "función no habilitada" si intentan usarla (consulte [Solución de problemas](topic:troubleshooting)).

## Concesiones de Soporte

Concesiones con tiempo limitado que permiten a un usuario de soporte acceder a una cuenta específica para resolver problemas, cada una con una razón y una referencia de ticket para rendición de cuentas. La lista muestra la **Cuenta**, el **Id de Usuario de Soporte**, la **Razón**, la **Referencia del Ticket**, el **Nivel de Acceso**, la ventana de inicio/fin, el **Estado** y las acciones.

Haga clic en el ícono **+** (**Solicitud de Acceso de Soporte**) para abrir el diálogo, que recopila **Cuenta** (obligatoria), **Id de Usuario de Soporte** (obligatorio), **Razón** (obligatoria), **Referencia del Ticket** (obligatoria), **Nivel de Acceso** (predeterminado `read`) e **Inicia** / **Finaliza** (ambos obligatorios). Presione **Guardar**.

Las acciones de fila son **Aprobar Acceso de Soporte** (visible mientras la solicitud no esté aprobada ni revocada) y **Revocar Acceso de Soporte** (visible hasta que la concesión sea revocada). El estado de la concesión se muestra como solicitada, aprobada o revocada según corresponda.
