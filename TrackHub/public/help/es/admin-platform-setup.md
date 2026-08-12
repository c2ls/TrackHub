---
id: admin-platform-setup
title: Configuración inicial de la plataforma
description: Para el administrador de la plataforma — asegure la cuenta administradora instalada, recorra Administrador del Sistema, cree cuentas, gestione su ciclo de vida y active funcionalidades.
category: getting-started
screens: []
related: [admin-account-setup, feature-catalog, system-administration, roles-and-permissions]
tags: [configuración, puesta en marcha, administrador, cuentas, funcionalidades, ciclo de vida, plataforma]
order: 30
---

# Configuración inicial de la plataforma

Esta guía es para el **administrador de la plataforma** — la persona que cuida de TrackHub en sí, en todas las cuentas de clientes. Comienza donde termina la instalación: la plataforma está en funcionamiento y usted recibió la cuenta administradora creada durante la instalación.

Siga los pasos en orden. Cada uno deja la plataforma en un estado sobre el que el siguiente puede construir.

Configurar una *sola* cuenta, una vez que existe, es un trabajo distinto con su propia guía: [Configuración de su cuenta](topic:admin-account-setup).

## 1. Inicie sesión y cambie su contraseña

La instalación crea un usuario administrador. Tiene los roles **Administrador** y **Gestor**, por lo que puede llegar a todas las pantallas del producto.

1. Inicie sesión con él como se describe en [Primeros pasos](topic:getting-started).
2. Vaya a **Perfil** en el menú lateral izquierdo.
3. En la tarjeta **Información del Perfil**, haga clic en el ícono de **candado** en la esquina superior derecha de la tarjeta.
4. En el diálogo **Actualizar Contraseña**, ingrese una contraseña nueva dos veces y presione **Guardar**.

Haga esto antes que nada. La contraseña con la que llega una instalación es una comodidad de puesta en marcha, no un secreto — considere la cuenta insegura hasta haberla cambiado.

Su nueva contraseña debe tener al menos **8 caracteres** e incluir **una letra mayúscula, una letra minúscula y un número**.

## 2. Conozca Administrador del Sistema

**Administrador del Sistema** es la pantalla de toda la plataforma y solo los Administradores pueden abrirla. Es una única página larga de secciones plegables; haga clic en un encabezado para expandirla. En orden, las secciones son:

| Sección | Para qué sirve | Con qué frecuencia la usa |
|---|---|---|
| **Cuenta** | La lista maestra de todas las cuentas de clientes, y donde crea las nuevas | En cada cliente nuevo |
| **Clientes API** | Clientes de integración que permiten conectarse a sistemas externos | Rara vez |
| **Permisos de Cliente de Servicio** | Qué cliente puede realizar qué acción sobre qué recurso | Rara vez |
| **Tipos de Unidades** | Umbrales de movimiento que deciden cuándo una unidad cuenta como detenida | Rara vez — déjelos como se instalaron |
| **Proveedores de Geocodificación** | El servicio que convierte coordenadas en direcciones | Una vez, al configurar |
| **Roles** | La matriz de permisos detrás de Administrador, Gestor y Usuario | Rara vez — déjela como se instaló |
| **Políticas** | Conjuntos de permisos reutilizables | Rara vez — déjelas como se instalaron |
| **Funcionalidades de Cuenta** | Qué funcionalidades tiene cada cuenta | En cada cliente nuevo |
| **Concesiones de Soporte** | Acceso de soporte con tiempo limitado a la cuenta de un cliente | Al resolver incidencias |

Cada sección se describe campo por campo en [Administración del sistema](topic:system-administration). El resto de esta guía cubre las cuatro que realmente usará al dar de alta a un cliente.

## 3. Cree una cuenta de cliente

Una **cuenta** es un cliente — sus propios usuarios, unidades, dispositivos y datos, aislados de cualquier otra cuenta.

1. Abra la sección **Cuenta** y haga clic en el ícono **+**.
2. Complete el diálogo **Detalle de la Cuenta**:
   - **Nombre** (obligatorio) y **Descripción**.
   - **Correo Electrónico**, **Contraseña**, **Nombre**, **Apellido** — estos son el **primer usuario** de la cuenta, y solo aparecen al crear una cuenta nueva.
   - **Tipo** (obligatorio) — **Personal**, **Empresarial** o **Asociado**.
   - **Activo** — déjelo marcado.
3. Presione **Guardar**.

**Qué ocurre al guardar.** TrackHub crea la cuenta y luego crea ese primer usuario dentro de ella **con el rol Gestor**. Esa persona es el administrador de la cuenta: puede administrar todo dentro de su propia cuenta, pero nada entre cuentas. **No** es un Administrador de la plataforma y no verá Administrador del Sistema.

Solo **Nombre** y **Tipo** se exigen antes de que el diálogo guarde, pero si deja en blanco los campos del usuario la cuenta se crea sin nadie que pueda iniciar sesión en ella. Complételos siempre.

**El estado inicial** proviene de la casilla **Activo**: marcada da una cuenta **Activa**, sin marcar da una **Suspendida**. El estado **Prueba** no es algo que pueda elegir aquí ni al que pueda mover una cuenta después.

Para agregar más usuarios a una cuenta sin salir de esta pantalla, use la acción **Agregar Usuario** en la fila de la cuenta. Esto crea únicamente el usuario — vea [Configuración de su cuenta](topic:admin-account-setup) para saber por qué eso no es todo el trabajo.

## 4. Gestione el ciclo de vida de una cuenta

Cada cuenta lleva un **Estado**, mostrado como un distintivo de color en su fila. Use la acción **Cambiar Estado** para moverlo.

| Estado | ¿Puede su gente usar TrackHub? |
|---|---|
| **Prueba** | Sí |
| **Activa** | Sí |
| **Suspendida** | No |
| **Cancelada** | No |
| **Archivada** | No |

El menú **Nuevo Estado** solo ofrece los movimientos permitidos desde donde está la cuenta ahora:

| Desde | Puede moverla a |
|---|---|
| **Prueba** | Activa, Suspendida, Cancelada |
| **Activa** | Suspendida, Cancelada |
| **Suspendida** | Activa, Cancelada, Archivada |
| **Cancelada** | Activa, Archivada |
| **Archivada** | *Nada — Archivada es definitiva* |

Dos consecuencias que conviene conocer:

- **Nada vuelve a Prueba.** Una vez que una cuenta sale de Prueba, no puede regresar.
- **No se puede archivar directamente.** Una cuenta Activa debe suspenderse o cancelarse primero. Y como Archivada es un callejón sin salida, la acción **Cambiar Estado** ni siquiera se muestra en la fila de una cuenta archivada.

Se **requiere** un **Motivo** al mover una cuenta a **Suspendida** o **Cancelada**. Para los demás movimientos es opcional, pero vale la pena completarlo de todos modos — es lo que leerá cuando alguien pregunte por qué.

**Qué le hace la suspensión al cliente.** Surte efecto de inmediato. Todos en esa cuenta — incluidos los Gestores — quedan detenidos en una única pantalla **Cuenta No Disponible** que dice *"Esta cuenta no está operativa actualmente. Por favor, contacte a soporte para obtener ayuda."*, con el estado actual debajo. Sin mapa, sin reportes, sin pantallas de administración. No se elimina nada; devolver la cuenta a **Activa** restaura todo.

## 5. Active las funcionalidades de la cuenta

Una cuenta nueva empieza **sin ninguna funcionalidad**. Hasta que las aprovisione, le faltará el menú Geocercas, no tendrá las pantallas de notificaciones y sus reportes GPS no existirán. Este paso no es opcional.

1. Abra la sección **Funcionalidades de Cuenta**.
2. Haga clic en el ícono **+** (**Agregar Funcionalidad a la Cuenta**).
3. Elija la **Cuenta** y la **Funcionalidad**.
4. Configure **Habilitada**, un **Nivel** (una etiqueta de plan; `default` está bien) y —para las dos funcionalidades que lo tienen— el valor de ajuste:
   - **Intervalo de Almacenamiento (Segundos)** para Integración GPS, predeterminado **360**.
   - **Días de Retención** para Historial de Posiciones GPS, predeterminado **30**.
5. Presione **Guardar** y repita para cada funcionalidad a la que el cliente tenga derecho.

Editar una fila existente mantiene fijas la cuenta y la funcionalidad y le permite cambiar el resto.

Para un cliente de rastreo típico, empiece con **Integración GPS** e **Historial de Posiciones GPS**, y luego agregue lo que incluya la suscripción. Qué hace cada funcionalidad, y exactamente qué desaparece cuando una está desactivada, se detalla en el [Catálogo de funcionalidades](topic:feature-catalog).

## 6. Datos de referencia de la plataforma

Estas secciones son compartidas por todas las cuentas. En la mayoría de las instalaciones son correctas tal como se instalaron y conviene no tocarlas.

- **Tipos de Unidades** — las categorías de unidad rastreada (auto, barco, dron, etc.) junto con los umbrales que deciden cuándo una unidad cuenta como detenida: **Detenido (Minutos)**, **Tiempo Máximo (Minutos)**, **Máxima Distancia (Km)** y **Basado en Ignición**. Puede **Editar** estos umbrales; no puede agregar ni eliminar tipos de unidad, porque el catálogo de tipos es fijo.
- **Proveedores de Geocodificación** — el servicio que convierte coordenadas en direcciones. Solo uno está activo a la vez. Si las direcciones no aparecen en el mapa, aquí es donde hay que mirar.
- **Clientes API** y **Permisos de Cliente de Servicio** — cómo se conectan otros sistemas a TrackHub y qué se les permite hacer. Tóquelos solo cuando esté configurando una integración; no forman parte del alta de un cliente normal.
- **Roles** y **Políticas** — la matriz de permisos detrás de Administrador, Gestor y Usuario. La matriz que viene instalada es la que asume el resto de esta documentación. Cambiarla puede ocultar pantallas a las personas de maneras difíciles de rastrear, así que déjela como está salvo que tenga una razón concreta.

## 7. Concesiones de Soporte

Un Administrador de la plataforma no puede simplemente entrar a navegar por los datos de un cliente. Para trabajar dentro de una cuenta al resolver una incidencia, cree una **Concesión de Soporte**: elija la **Cuenta**, el **Id de Usuario de Soporte**, una **Razón**, una **Referencia del Ticket**, un **Nivel de Acceso** (`read` de forma predeterminada) y una hora de inicio y de fin. La concesión debe luego **aprobarse**, y puede **revocarse** en cualquier momento.

Las concesiones tienen tiempo limitado y quedan registradas a propósito. Úselas en lugar de pedir prestadas las credenciales de un cliente, y revóquelas cuando el ticket se cierre.

## 8. Entregue la cuenta a su gestor

La cuenta existe, está Activa y sus funcionalidades están activadas. Lo que queda le corresponde a la persona que creó como su Gestor. Dígale:

- Qué funcionalidades tiene su cuenta, y que cambiarlas es una decisión de facturación que vuelve a usted.
- Que debe cambiar su contraseña en el primer inicio de sesión, exactamente como hizo usted en el paso 1.
- Que el orden de su configuración importa, y que está escrito para él en [Configuración de su cuenta](topic:admin-account-setup).
- Que **crear un usuario no le da un rol a ese usuario**, y que **un Usuario sencillo sin grupo ve un mapa vacío**. Estas son las dos cosas que los gestores nuevos hacen mal.

## Lista de verificación

- [ ] Contraseña del administrador cambiada.
- [ ] Proveedor de geocodificación configurado y activo.
- [ ] Cuenta creada, con su primer usuario (el Gestor de la cuenta).
- [ ] El estado de la cuenta es **Activa**.
- [ ] Todas las funcionalidades a las que tiene derecho activadas, con sus valores de ajuste.
- [ ] Cuenta entregada al Gestor, indicándole su guía de configuración.

Para la salud de la plataforma en sí —qué servicios están arriba y cualquier anuncio— vea [Estado de la Plataforma](topic:platform-status).
