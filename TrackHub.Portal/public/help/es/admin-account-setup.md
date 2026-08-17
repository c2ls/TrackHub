---
id: admin-account-setup
title: Configuración de su cuenta
description: Para un Gestor que configura una cuenta nueva — el orden en que hacer las cosas, qué saber antes de crear usuarios y operadores GPS, y los dos errores que todos cometen.
category: getting-started
screens: []
related: [admin-platform-setup, feature-catalog, users-roles-groups, gps-integration, management-overview]
tags: [configuración, puesta en marcha, gestor, usuarios, grupos, operadores, orden]
order: 40
---

# Configuración de su cuenta

Esta guía es para el **Gestor** de una cuenta de TrackHub — la persona que administra una cuenta, su gente y su flota. Si su cuenta acaba de crearse para usted, empiece aquí y recorra los pasos en orden.

**Por qué importa el orden:** varios pasos dependen de otros anteriores. Las unidades no pueden asignarse a dispositivos que aún no se han sincronizado, y un usuario normal no puede ver una unidad que no esté en alguno de sus grupos. Hacerlo fuera de orden es lo que produce el reclamo clásico: *"Inicié sesión y el mapa está vacío."*

Si lo que está configurando es la plataforma y no una cuenta, vea [Configuración inicial de la plataforma](topic:admin-platform-setup).

## 1. Inicie sesión y asegure su cuenta

Usted recibió el primer usuario de la cuenta, y tiene el rol **Gestor**.

1. Inicie sesión como se describe en [Primeros pasos](topic:getting-started).
2. Abra **Perfil** y, en la tarjeta **Información del Perfil**, haga clic en el ícono de **candado** en la esquina superior derecha de la tarjeta. Ingrese una contraseña nueva dos veces en el diálogo **Actualizar Contraseña** y presione **Guardar**.
3. Ya que está ahí, use la tarjeta **Perfil de Usuario** para fijar su **Idioma** y sus preferencias de **Modo Claro/Oscuro** y **Contraer Barra Lateral**, y presione **Guardar**.

Las contraseñas deben tener al menos **8 caracteres** e incluir **una letra mayúscula, una letra minúscula y un número**.

## 2. Configure su personalización

Abra **Administración de Cuentas → Cuenta y Suscripción → Personalización**.

- **Nombre para Mostrar** — el nombre que representa a su cuenta. Es el único campo de personalización con un efecto visible en otro lugar: aparece en la parte superior de las **exportaciones de reportes en PDF**, y en la pantalla de estado si su cuenta llegara a suspenderse. Póngale el nombre de su organización.
- **Documento del Logo**, **Color Primario** y **Encabezado de Reporte / Exportación** — estos se guardan para uso futuro. En la versión actual todavía no cambian la apariencia del portal ni el contenido de sus exportaciones, así que no prometa a sus colegas un tema con la marca basándose en ellos.

Presione **Guardar**.

## 3. Revise qué funcionalidades tiene

Abra **Administración de Cuentas → Cuenta y Suscripción → Funcionalidades de Cuenta**. Verá las diez funcionalidades listadas con **Habilitada** en Sí o No.

Esta lista es **de solo lectura para usted**. Qué funcionalidades tiene su cuenta es una decisión de facturación que toma el administrador de la plataforma, así que si falta algo que esperaba —geocercado, documentos, notificaciones— es a él a quien debe consultar.

Revíselo ahora y no después. Una funcionalidad faltante es la otra causa habitual de que una pantalla o sección "no esté", y se ve idéntica a un problema de permisos. Qué activa y desactiva cada funcionalidad está en el [Catálogo de funcionalidades](topic:feature-catalog).

## 4. Cree sus usuarios

Abra **Administración de Cuentas → Usuarios y Accesos → Usuarios** y haga clic en el ícono **+**. El diálogo **Detalle del Usuario** recopila **Correo Electrónico**, **Contraseña**, **Nombre de Usuario**, **Nombre** y **Apellido** (además de segundos nombres y fecha de nacimiento opcionales), y las casillas **Activo** y **Usuario de Integración**.

Antes de empezar, tres cosas que debe saber.

**Crear un usuario no le da un rol.** Este es el error más común de todos. El diálogo **Detalle del Usuario** no tiene campo de rol. Un usuario recién creado puede iniciar sesión y aterrizará en un portal desnudo. Después debe ir a la sección **Roles** —una sección *distinta* en la misma pantalla—, hacer clic en **Asignar** en el rol que quiera y elegir al usuario del menú desplegable. Hasta que lo haga, no tiene ningún rol.

**Elija el rol deliberadamente.** Hay tres, y la diferencia entre ellos es grande:

| Rol | Qué obtiene | Désela a |
|---|---|---|
| **Gestor** | Toda la cuenta: unidades, dispositivos, usuarios, grupos, operadores GPS, documentos, alertas, personalización. Ve **todas** las unidades de la cuenta sin importar los grupos. | Las personas que administran la cuenta |
| **Usuario** | Las pantallas del día a día — tablero, reportes, geocercas, su propio perfil. **Ve solo las unidades de los grupos a los que pertenece.** | Operadores cotidianos |
| **Administrador** | Toda la plataforma, en todas las cuentas. | Nadie dentro de una cuenta de cliente |

Hacer Gestor a alguien no es un favor menor: puede ver y cambiar todo lo que usted puede, para todos en la cuenta. Si alguien solo necesita vigilar vehículos, es un **Usuario**.

**Otros detalles.** Las contraseñas deben tener al menos 8 caracteres, con una letra mayúscula, una minúscula y un número. Marque **Usuario de Integración** solo para una cuenta de máquina usada por una integración, nunca para una persona. Si un usuario queda bloqueado tras demasiados intentos fallidos de inicio de sesión, la columna **Estado** de su fila se convierte en un botón **Desbloquear**.

## 5. Cree sus grupos — el paso que decide quién ve qué

Abra **Administración de Cuentas → Flota y Rastreo → Grupos**.

> **Este es el paso que la gente se salta, y es el que rompe todo.** Un **Usuario** sencillo ve únicamente las unidades que pertenecen a un grupo del que es miembro. La pertenencia a un grupo es la *única* forma en que un usuario normal obtiene visibilidad de una unidad — no existe ninguna asignación de unidades por usuario en ningún otro lugar. **Un Usuario que no está en ningún grupo ve un mapa vacío, sin contadores de unidades, y reportes que no devuelven filas.** Supondrá que TrackHub está roto.
>
> Los Gestores y Administradores no se ven afectados: siempre ven todas las unidades de la cuenta.

Para cada grupo:

1. Haga clic en el ícono **+** y complete el diálogo **Detalle del Grupo**: **Nombre** (obligatorio), **Descripción** (obligatoria) y la casilla **Activo**. Presione **Guardar**.
2. En la fila del grupo, haga clic en **Asignar** en la columna **Usuarios** y agregue los usuarios que deben ver las unidades de este grupo.
3. Haga clic en **Asignar** en la columna **Unidades** y agregue las unidades.

El paso 3 debe esperar a que existan sus unidades (paso 7), así que cuente con volver aquí. Lo que sí puede hacer ahora es decidir la forma —por sucursal, por región, por cliente— y crear los grupos.

**Los Puntos de Interés se comportan distinto** y conviene saberlo: un punto de interés sin grupo es visible para *todos* en la cuenta, mientras que uno asignado a un grupo queda limitado a los miembros de ese grupo.

## 6. Conecte sus operadores GPS

Abra **Integración GPS** desde el menú lateral izquierdo. Un **operador** es una conexión configurada a un proveedor GPS — el servidor, la cuenta y las credenciales que TrackHub usa para traer sus dispositivos y sus posiciones.

**Tenga a mano las credenciales de su proveedor antes de empezar.** TrackHub no las obtiene por usted; provienen de su proveedor GPS.

1. Expanda **Operadores GPS** y haga clic en el ícono **+**. Complete **Nombre** (obligatorio), los datos de contacto opcionales, **Intervalo de Sincronización (min)** (predeterminado **30**) y **Protocolo** (obligatorio).
2. Elija el **Protocolo** con cuidado — debe coincidir con el que su proveedor realmente habla. Las opciones son `COMMAND_TRACK`, `TRACCAR`, `FLESPI`, `GEOTAB`, `GPS_GATE`, `NAVIXY`, `SAMSARA`, `WIALON` y `PROTRACK`. Un protocolo equivocado produce errores de autenticación o de conectividad que parecen credenciales incorrectas.
3. Presione **Guardar** y luego, en la fila del operador, haga clic en la acción **Credencial** (la llave). Ingrese la **URL** (obligatoria) y aquellos de **Nombre de Usuario**, **Contraseña**, **Clave Secreta** y **Clave Secreta 2** que use su proveedor. Presione **Guardar**. Las credenciales se almacenan cifradas.
4. **Pruebe la conexión antes de habilitar nada.** Haga clic en la acción de verificación en la columna **Probar Credencial**. Busca obtener *"La credencial fue probada con éxito"*. TrackHub no le impide habilitar ni sincronizar un operador sin probar —los botones siguen todos disponibles—, así que esta disciplina depende de usted. Un operador sin probar que falla a las 3 de la madrugada es una forma mucho peor de enterarse.
5. Solo cuando la prueba pase, use **Habilitar** en la fila del operador. Un operador deshabilitado nunca sincroniza automáticamente; su estado se muestra como **Deshabilitado**.
6. Haga clic en **Sincronizar** para traer la lista de dispositivos de inmediato, en lugar de esperar al intervalo. Los dispositivos aparecen entonces en **Dispositivos Sincronizados**.

**Intervalo de Sincronización (min)** es cada cuánto TrackHub consulta al proveedor automáticamente. Más corto es más fresco y más pesado para el proveedor; 30 minutos es un punto de partida sensato para los cambios en la lista de dispositivos.

**Vigile el estado después.** El panel en la parte superior de la página resume operadores, dispositivos y sincronización, y cada operador lleva un distintivo de estado: **Saludable**, **Degradado**, **Fuera de línea**, **Deshabilitado** o **Desconocido**. **Sincronizaciones Recientes** muestra qué hizo realmente cada sincronización. El detalle completo está en [Integración GPS](topic:gps-integration).

**Una acción con la que hay que tener cuidado: Reiniciar Sincronización.** Elimina los dispositivos sincronizados del operador *y las unidades que solo existían para esos dispositivos*, y luego empieza de cero. Le pide confirmación. Úsela para reconstruir una lista de dispositivos rota, nunca como una actualización de rutina.

## 7. Cree sus unidades y luego asigne los dispositivos

Un **dispositivo** es el equipo rastreador; una **unidad** es el vehículo o activo que realmente le importa. Los dispositivos llegan automáticamente desde la sincronización del operador —usted no los crea a mano— y luego se vinculan a las unidades.

1. **Unidades** (**Administración de Cuentas → Flota y Rastreo → Unidades**) — haga clic en **+**, dele a la unidad un **Nombre** y un **Tipo**, y **Guarde**. El **Tipo** determina los umbrales de movimiento usados para decidir cuándo la unidad cuenta como detenida.
2. **Asigne el dispositivo** — en la pantalla **Integración GPS**, bajo **Asignaciones de Dispositivos**, elija la unidad y un dispositivo sin asignar y presione **Asignar Dispositivo**. Solo se ofrecen los dispositivos que no están asignados actualmente a una unidad.
3. **Conductores** (**Flota y Rastreo → Conductores**) — si registra quién conduce, agréguelos aquí. Un conductor puede llevar un **Transportador Predeterminado**, así que cree primero las unidades.
4. **Puntos de Interés** (**Flota y Rastreo → Puntos de Interés**) — ubicaciones con nombre, como sitios de cliente, bodegas, estaciones de combustible y talleres, con un nombre, coordenadas, un tipo, un color y opcionalmente un grupo.

Ahora vuelva a **Grupos** y complete el paso 5.3 — asigne las unidades a sus grupos.

## 8. Configure las funcionalidades opcionales que tenga

Solo estarán las funcionalidades habilitadas para su cuenta. Una línea para cada una:

- **Geocercas** — dibuje las zonas para las que quiere alertas de entrada y salida. Vea [Geocercas](topic:geofences).
- **Reglas de notificación** — decida qué genera una alerta y a dónde va. El correo y WhatsApp son derechos separados de la funcionalidad base de notificaciones, así que un canal que no puede seleccionar es una cuestión de facturación, no un fallo. Vea [Alertas y notificaciones](topic:alerts-notifications).
- **Documentos** — guarde la documentación de vehículos y conductores, con seguimiento de vencimientos. Vea [Documentos](topic:documents).
- **Enlaces Públicos** — comparta una vista limitada y con tiempo acotado con alguien que no tiene sesión. Vea [Enlaces públicos](topic:public-links).

## 9. Verifique la configuración

No lo dé por terminado hasta haberlo comprobado desde el otro lado:

1. **Como usted mismo**, abra el **Tablero**. Sus unidades deberían aparecer en el mapa y estar moviéndose. Si no lo hacen, revise **Integración GPS → Sincronizaciones Recientes** y el distintivo de estado del operador.
2. **Inicie sesión como un Usuario de prueba** — un Usuario sencillo que pertenezca a un grupo. Debería ver exactamente las unidades de ese grupo y nada más. Si su mapa está vacío, no está en ningún grupo, o el grupo no tiene unidades: vuelva al paso 5.
3. **Ejecute un reporte** como ese usuario y confirme que devuelve filas. Un usuario sin grupos sigue viendo la *lista* de reportes; simplemente los reportes no devuelven nada. Por eso un reporte vacío es un síntoma de visibilidad, no una falla de reportes.

## Lista de verificación

- [ ] Contraseña cambiada, idioma configurado.
- [ ] **Nombre para Mostrar** de la personalización configurado.
- [ ] Funcionalidades de la cuenta revisadas, y las faltantes consultadas con el administrador de la plataforma.
- [ ] Usuarios creados **y** roles asignados en la sección Roles.
- [ ] Grupos creados, con usuarios y unidades asignados.
- [ ] Operador GPS creado, credenciales probadas, habilitado, primera sincronización ejecutada.
- [ ] Unidades creadas y dispositivos asignados.
- [ ] Verificado, como Usuario sencillo, que el mapa no está vacío.
