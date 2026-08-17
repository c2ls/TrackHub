---
id: drivers-workforce
title: Conductores y personal
description: Registre conductores, otórgueles credenciales y dispositivos móviles, controle licencias y otras habilitaciones, y mantenga un registro acotado en el tiempo de quién condujo cada unidad.
category: administration
screens: [manageAdmin]
related: [management-overview, units-devices, documents, alerts-notifications, feature-catalog]
tags: [conductores, personal, credenciales, dispositivos, habilitaciones, licencias, asignaciones, vencimientos]
order: 35
---

# Conductores y personal

Un **conductor** es una persona operativa — alguien que conduce sus unidades. Los conductores no son usuarios del portal: nunca inician sesión en el portal web, tienen sus propias credenciales para la aplicación móvil del conductor y se administran íntegramente desde el grupo **Flota y Rastreo** de la pantalla **Administración de Cuentas**.

El área tiene cinco secciones plegables, en este orden:

| Sección | Para qué sirve |
|---|---|
| **Conductores** | El registro — quién existe, su documento, código de empleado y licencia principal. |
| **Credenciales y Dispositivos del Conductor** | La credencial con la que cada conductor inicia sesión en la aplicación móvil, y los dispositivos en los que está registrada. |
| **Habilitaciones del Conductor** | Licencias, exámenes médicos, capacitaciones, verificaciones de antecedentes y permisos de materiales peligrosos, con sus fechas de vencimiento. |
| **Asignaciones del Conductor** | Registros acotados en el tiempo de qué conductor operó qué unidad, y cuándo. |
| **Vencimientos de Habilitaciones (30 días)** | Una lista, para toda la cuenta, de todo lo que está por vencer. |

## Qué requiere la funcionalidad Personal

Las dos primeras secciones son **plataforma base**. Siempre están disponibles para un administrador con los permisos correspondientes, en cualquier cuenta, sin importar lo que diga la suscripción. Siempre puede registrar conductores, y siempre puede emitir, bloquear, restablecer y revocar sus credenciales móviles.

Las tres últimas secciones — **Habilitaciones del Conductor**, **Asignaciones del Conductor** y **Vencimientos de Habilitaciones** — pertenecen a la funcionalidad facturable **Personal**. Sin ella simplemente no aparecen en la pantalla, no se generan alertas de vencimiento y los tres reportes de personal no están en el catálogo de reportes. Vea el [Catálogo de funcionalidades](topic:feature-catalog).

## Conductores

El registro propiamente dicho. Las columnas son **Nombre**, **Teléfono**, **Documento**, **Activo** y una columna de acción.

Presione el icono **+** (agregar) para abrir el diálogo **Crear Conductor**:

- **Nombre** (obligatorio) — el nombre completo del conductor.
- **Teléfono** — dígitos, espacios, paréntesis, puntos, guiones y un **+** inicial opcional, con al menos cinco caracteres. Cualquier otra cosa se rechaza con **"Número de teléfono inválido."**
- **Tipo de Documento** y **Número de Documento** — el documento de identidad. El número es único dentro de su cuenta y nunca se coteja entre cuentas.
- **Código de Empleado** — su propia referencia de nómina o de recursos humanos.
- **Número de Licencia** y **Vencimiento de Licencia** — la licencia principal, que se muestra como resumen en el registro del conductor. En un conductor *nuevo* la fecha de vencimiento no puede estar ya en el pasado (**"La fecha de vencimiento de la licencia ya pasó."**); al editar un conductor existente sí puede, de modo que una licencia vencida pueda registrarse y corregirse.
- **Transportador Predeterminado** — la unidad que este conductor opera habitualmente, elegida entre las unidades de su cuenta. Es un *valor predeterminado*, no una asignación; vea más abajo.
- **Activo** — desmarque para dejar al conductor fuera de uso.

Las acciones de fila son **Editar** y **Desactivar Conductor** (se le pide confirmación). Desactivar no elimina nada, y a un conductor inactivo no se le puede dar una nueva asignación.

## Credenciales y Dispositivos del Conductor

Esta sección administra cómo un conductor inicia sesión en la **aplicación móvil del conductor**. Elija un conductor en el selector **Conductor** de la parte superior; la sección muestra entonces la credencial de ese conductor y los dispositivos registrados con ella.

Un conductor tiene como máximo una credencial. Presione **Crear Credencial** y defina un **Usuario** y una **Contraseña**. La credencial nace **Pendiente de Activación**, y el conductor no puede iniciar sesión hasta que se active.

| Columna | Significado |
|---|---|
| **Usuario** | El nombre que el conductor escribe en la aplicación. |
| **Estado** | Pendiente de Activación, Activa, Bloqueada o Revocada. |
| **Intentos Fallidos** | Inicios de sesión fallidos consecutivos. El contador se reinicia con un ingreso exitoso. |
| **Bloqueada Hasta** | Cuándo expira un bloqueo automático o manual. |
| **Último Ingreso** | El último inicio de sesión exitoso. |
| **Requiere Cambio de Contraseña** | El conductor debe definir una contraseña nueva en el próximo ingreso. |

Acciones de fila:

- **Activar** — abre **Activación de Conductor** y habilita una credencial pendiente.
- **Bloquear** — impide el inicio de sesión hasta el momento **Bloqueada Hasta** que usted elija (**"El conductor no podrá iniciar sesión hasta ese momento."**). TrackHub también bloquea una credencial por sí solo tras **cinco** intentos fallidos consecutivos, durante **15 minutos**.
- **Restablecer Contraseña** — define una contraseña nueva y, opcionalmente, obliga al conductor a cambiarla de nuevo en el próximo ingreso.
- **Revocar** — es permanente: **"el conductor no podrá iniciar sesión y las sesiones existentes dejarán de renovarse."** Revocar también corta las sesiones de la aplicación que ya estaban en curso, porque la renovación del token vuelve a verificar la credencial cada vez.

### Dispositivos Registrados

Debajo de la credencial está la lista de dispositivos que el conductor registró desde la aplicación: **Nombre del Dispositivo**, **Plataforma**, **Versión de la App**, **Token Push (enmascarado)**, **Última Actividad** y **Estado**. **Revocar Registro de Dispositivo** elimina un dispositivo — el conductor deberá registrarlo nuevamente desde la aplicación; su credencial no se toca.

Los tokens push se muestran enmascarados, y los identificadores internos de sesión nunca se envían al portal. No hay forma de revelar ninguno de los dos, por diseño.

## Habilitaciones del Conductor

*Requiere la funcionalidad Personal.*

Un registro de habilitación cubre todo lo acreditado de un conductor, no solo la licencia de conducción. Elija un conductor y use **+** para abrir **Crear Habilitación**:

- **Tipo** (obligatorio) — **Licencia**, **Examen Médico**, **Capacitación**, **Verificación de Antecedentes**, **Permiso de Materiales Peligrosos** u **Otra**.
- **Categoría** — la subclase, cuando el tipo la tiene, por ejemplo la categoría de una licencia.
- **Número** — el número del documento o del certificado.
- **Fecha de Emisión** y **Fecha de Vencimiento** — una habilitación no puede vencer antes de haberse emitido.
- **Autoridad Emisora** — quién la otorgó.
- **Estado** (obligatorio) — **Vigente**, **Vencida** o **Revocada**. El vencimiento se deduce por sí solo de la **Fecha de Vencimiento**, así que el estado que normalmente fija a mano es **Revocada**, para una habilitación retirada antes de su fecha.
- **Documento Vinculado** — el identificador del documento de respaldo (el escaneo de la licencia, el certificado médico). Guarde el archivo en [Documentos](topic:documents) y refiéralo aquí.
- **Notas** — texto libre.

La columna **Fecha de Vencimiento** cambia de color a medida que la fecha se acerca. Las acciones de fila son **Editar** y **Eliminar**; la eliminación es permanente y solo la auditoría conserva el registro después.

Los campos **Número de Licencia** / **Vencimiento de Licencia** del registro del conductor permanecen como resumen de un vistazo. El conjunto completo de licencias y certificados vive aquí.

## Asignaciones del Conductor

*Requiere la funcionalidad Personal.*

Una asignación es un **registro acotado en el tiempo de que un conductor operó una unidad**. A diferencia del **Transportador Predeterminado**, que es una única preselección que cambia cada vez que la edita, las asignaciones se acumulan en un historial que puede consultar hacia atrás.

Para crear una, elija el **Conductor**, la **Unidad** y un **Tipo de Asignación** (**Regular** o **Temporal**), y presione **Asignar Conductor**.

Siguen dos tablas. **Asignaciones Activas** muestra lo que está abierto ahora, con una acción **Finalizar Asignación**. **Historial de Asignaciones** muestra todo, filtrado por conductor, unidad, **Desde** y **Hasta** — presione **Buscar** para aplicar los filtros. Las columnas en ambas son **Conductor**, **Unidad**, **Inicia El**, **Finaliza El**, **Tipo de Asignación**, **Estado** (Activa, Finalizada o Cancelada) y **Creada Por**.

Las reglas que conviene conocer:

- **Una sola asignación abierta por pareja de conductor y unidad.** Asignar el mismo conductor a la misma unidad de nuevo mientras la primera sigue abierta se rechaza como conflicto. Finalice la primera y luego cree la nueva.
- **Un conductor puede tener varias asignaciones abiertas a la vez**, siempre que sean a unidades *distintas*.
- **Las asignaciones finalizadas son inmutables.** **Finalizar Asignación** se lo advierte: *"Las asignaciones finalizadas no se pueden modificar."* Si finaliza una por error, cree una asignación nueva — no puede reabrirla.
- A un **conductor inactivo** no se le puede dar una nueva asignación.
- Si su cuenta tiene activado **Bloquear la asignación cuando la licencia del conductor esté vencida** (vea el [Catálogo de funcionalidades](topic:feature-catalog)), asignar a un conductor cuya habilitación de tipo **Licencia** esté vencida o revocada se rechaza con un mensaje de validación. Las cuentas sin ese ajuste no se ven afectadas.

## Vencimientos de Habilitaciones (30 días)

*Requiere la funcionalidad Personal.*

Una vista, para toda la cuenta, de cada habilitación que vence dentro de los próximos 30 días, sea de quien sea. Columnas: **Conductor**, **Tipo**, **Categoría**, **Número**, **Fecha de Vencimiento**, **Días Restantes** y **Estado**. **Días Restantes** está coloreado y muestra **Vencida** una vez que la fecha pasó.

Esta es la pantalla que conviene revisar cada semana. Su equivalente en reportes, que sí puede exportar, es el reporte **Vencimientos de Habilitaciones**.

## Alertas de vencimiento

*Requiere la funcionalidad Personal.*

Una vez al día, TrackHub revisa las habilitaciones de cada cuenta con Personal habilitado y genera un evento de alerta cuando cada una cruza los **30**, **15**, **7** y **0** días para el vencimiento. Los tres primeros generan **Habilitación del Conductor por Vencer**; el día en que caduca se genera **Habilitación del Conductor Vencida**, con mayor severidad.

Cada umbral se dispara **exactamente una vez** por habilitación, para siempre — de modo que recibe cuatro recordatorios repartidos a lo largo del último mes de vigencia de una licencia, no cuatro cada día. Extender la fecha de vencimiento crea un nuevo juego de umbrales por cruzar.

Los eventos llegan a **Eventos de Alerta** como cualquier otro, y una regla de notificación puede enrutarlos a correo, WhatsApp o un webhook. Vea [Alertas y notificaciones](topic:alerts-notifications).

## Reportes de personal

*Requiere la funcionalidad Personal.*

Tres reportes están en la categoría **Personal** del catálogo de reportes:

- **Registro de Conductores** — la lista completa de conductores con licencia, código de empleado y unidad predeterminada. Sin filtros; solo Excel.
- **Vencimientos de Habilitaciones** — habilitaciones que vencen dentro de una ventana **Dentro de (días)** (30 por defecto). Excel o PDF.
- **Historial de Asignaciones de Conductores** — asignaciones dentro de un rango **Desde** / **Hasta**. Solo Excel.

Los datos de los conductores son datos personales, por lo que estas exportaciones se registran en la auditoría con severidad alta. Vea [Reportes](topic:reports).

## Quién puede usar esto

Las cinco secciones residen dentro de **Administración de Cuentas**, que está disponible para los Gestores. Una sección, un botón **+** o una acción de fila también puede faltar porque su rol no tiene el permiso de conductores correspondiente — leer, escribir, editar y eliminar se otorgan por separado. Vea [Roles y permisos](topic:roles-and-permissions) y [Administración de Cuentas](topic:management-overview).

Si faltan las secciones de habilitaciones, asignaciones o vencimientos pero el registro de conductores sí está, se trata de la funcionalidad Personal, no de sus permisos.
