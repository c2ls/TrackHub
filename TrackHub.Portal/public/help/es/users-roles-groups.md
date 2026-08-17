---
id: users-roles-groups
title: Usuarios, roles y grupos
description: Agregue las personas que inician sesión en su cuenta, asígneles roles y políticas, y organice unidades y usuarios en grupos.
category: administration
screens: [manageAdmin]
related: [roles-and-permissions, management-overview, admin-account-setup]
tags: [usuarios, roles, políticas, grupos, permisos]
order: 20
---

# Usuarios, roles y grupos

Estas secciones se encuentran en la pantalla [Administración de Cuentas](topic:management-overview), que solo los **Gestores** pueden abrir. **Usuarios**, **Roles** y **Políticas** están bajo el grupo **Usuarios y Accesos**; **Grupos** está bajo el grupo **Flota y Rastreo**. Expanda una sección para trabajar con ella.

## Usuarios

La sección **Usuarios** administra las personas que pueden iniciar sesión en su cuenta.

Columnas: **Nombre de Usuario** (cada fila muestra el correo electrónico de la persona con su nombre de usuario debajo), **Primer Nombre**, **Apellido**, **Estado**, **Acción** y **Contraseña**.

Para agregar un usuario, expanda la sección y haga clic en el icono **+** (agregar). El diálogo **Detalle del Usuario** recopila:

- **Correo Electrónico** — obligatorio.
- **Contraseña** — obligatoria, y se muestra solo al crear un nuevo usuario.
- **Nombre de Usuario** — obligatorio.
- **Primer Nombre** (obligatorio), **Segundo Nombre**, **Apellido** (obligatorio), **Segundo Apellido**.
- **Fecha de Nacimiento**.
- Casillas **Activo** y **Usuario de Integración**.

Presione **Guardar**. Una nueva contraseña debe cumplir la política de contraseñas de su cuenta (el servidor rechaza las contraseñas demasiado débiles — normalmente al menos 8 caracteres con una letra mayúscula, una minúscula y un número).

Acciones de fila:

- **Editar** — reabre el diálogo **Detalle del Usuario** para cambiar los datos de la persona. El campo de contraseña se oculta al editar.
- **Eliminar** — quita al usuario (se le pide confirmar).
- **Contraseña** — el enlace en la columna **Contraseña** abre el diálogo **Actualizar Contraseña**; escriba una nueva contraseña y presione **Guardar**.
- **Estado** — un usuario que no está bloqueado muestra **Activo**. Cuando un usuario está bloqueado (por ejemplo, tras demasiados intentos fallidos de inicio de sesión) la columna muestra un botón **Desbloquear**; haga clic para desbloquear la cuenta de inmediato.

Los roles y las políticas que tiene un usuario se asignan en las secciones **Roles** y **Políticas**, no en este diálogo. **Un usuario que cree aquí no tiene ningún rol hasta que le asigne uno** — vea [Configuración de su cuenta](topic:admin-account-setup) para saber qué hacer a continuación, y en qué orden.

## Roles

Los roles agrupan permisos. Los roles integrados de su cuenta son **Gestor**, **Usuario** y **Administrador**.

La sección lista cada rol con una columna **Roles** y una columna **Acción**. Los roles no se crean ni se eliminan aquí — usted solo asigna usuarios a ellos.

Haga clic en **Asignar** en un rol para abrir el diálogo **Asignar Rol**:

- Elija un **Usuario** de la lista desplegable para agregarlo al rol.
- La tabla lista por **Nombre de Usuario** los usuarios que ya están en el rol; seleccione una o más filas para quitarlos.
- Cierre el diálogo cuando termine.

Para conocer lo que puede hacer cada rol, vea [Roles y permisos](topic:roles-and-permissions).

## Políticas

Las políticas son conjuntos de permisos más detallados: **Acceso Total**, **Administrar Usuarios**, **Solo Lectura**, **Actualización Limitada** y **Auditoría**. Esta sección controla qué usuarios tienen cada política.

La sección lista cada política con una columna **Políticas** y una columna **Acción**. Haga clic en **Asignar** para abrir el diálogo **Asignar Política**, que funciona igual que el diálogo de roles: agregue un usuario de la lista desplegable, o seleccione usuarios listados y quítelos, luego cierre.

Vea [Roles y permisos](topic:roles-and-permissions) para saber cómo se combinan los roles y las políticas.

## Grupos

Los grupos le permiten organizar unidades y usuarios juntos — por ejemplo, por sucursal o región.

Columnas: **Grupos** (nombre), **Descripción**, **Acción**, **Usuarios** y **Unidades**.

Para agregar un grupo, expanda la sección y haga clic en el icono **+** (agregar). El diálogo **Detalle del Grupo** solicita un **Nombre** (obligatorio), una **Descripción** (obligatoria) y una casilla **Activo**. Presione **Guardar**.

Acciones de fila:

- **Editar** y **Eliminar** en la columna **Acción** cambian o quitan el grupo (la eliminación pide confirmar).
- **Asignar** en la columna **Usuarios** abre el diálogo **Asignar Usuario**: agregue usuarios al grupo desde la lista desplegable, o seleccione usuarios listados y quítelos.
- **Asignar** en la columna **Unidades** abre el diálogo **Asignar Unidad**: agregue unidades al grupo desde la lista desplegable, o seleccione unidades listadas y quítelas.
