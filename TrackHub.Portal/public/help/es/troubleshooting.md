---
id: troubleshooting
title: Solución de problemas
description: Soluciones rápidas orientadas al síntoma para problemas al iniciar sesión, funcionalidades faltantes, cuentas bloqueadas, errores y unidades sin movimiento.
category: troubleshooting
screens: []
related: [getting-started, gps-integration, reports]
tags: [solución de problemas, errores, iniciar sesión, cuenta, gps]
order: 10
---

# Solución de problemas

Soluciones rápidas y orientadas al síntoma para los problemas más comunes que enfrentan administradores y operadores. Encuentre su síntoma a continuación y siga los pasos. Si una solución apunta a una configuración que usted no puede cambiar, anote el mensaje exacto y contacte a su administrador (o, para problemas de toda la plataforma, al equipo de soporte de plataforma).

## No puedo iniciar sesión

Los problemas al iniciar sesión tienen algunas causas distintas. Revíselas en orden:

- **Usuario o contraseña incorrectos.** Vuelva a ingresar sus credenciales con cuidado (verifique Bloq Mayús). Las contraseñas siempre contienen al menos **8 caracteres**, incluyendo **una letra mayúscula, una letra minúscula y un número**.
- **Su cuenta está bloqueada.** Tras varios intentos fallidos de inicio de sesión, un usuario puede quedar bloqueado. Un Gestor puede resolverlo desde **Administración de Cuentas → Usuarios**: un usuario bloqueado muestra un botón **Desbloquear** en la columna **Estado** (vea [Usuarios, roles y grupos](topic:users-roles-groups)). Una vez desbloqueado, intente de nuevo.
- **Demasiados intentos seguidos (tiempo de espera).** Tras un inicio de sesión interrumpido, la página **Error de Autenticación** le hace esperar una breve cuenta regresiva (unos 30 segundos) antes de iniciar otro intento. Si no ocurre nada al reintentar, espere medio minuto e intente una vez más.
- **Su sesión expiró.** Si había iniciado sesión y de repente lo devuelven a la pantalla de inicio de sesión, su sesión simplemente expiró. Inicie sesión de nuevo. Si sigue ocurriendo inmediatamente después de iniciar sesión, su sesión no pudo renovarse — espere un momento y reintente.
- **Página de error "Error de Autenticación".** Si el inicio de sesión se interrumpe, puede llegar a una página de pantalla completa **Error de Autenticación**. Enumera las causas probables (el servicio de autenticación temporalmente no disponible, problemas de conectividad de red, o una solicitud de autenticación inválida o expirada). Espere a que termine la cuenta regresiva **Reintentar en Ns**, luego presione **Reintentar Autenticación**, o presione **Volver**. Variantes específicas:
  - Un mensaje sobre un fallo al intercambiar el código de autorización por un token de acceso significa que el servicio de autenticación no era accesible — reintente en breve.
  - Un mensaje sobre la ausencia de un código de autorización significa que el inicio de sesión se interrumpió o que usó un enlace antiguo — inicie una nueva sesión en lugar de reutilizar una URL de retorno guardada en favoritos. Reintentar desde esta página inicia un intento limpio.

Si los reintentos siguen fallando durante varios minutos, el servicio de autenticación podría estar caído — contacte a su administrador.

## Mi cuenta no está operativa

Si ve un mensaje de pantalla completa **Cuenta No Disponible** — "Esta cuenta no está operativa actualmente. Por favor, contacte a soporte para obtener ayuda." — con un **Estado** mostrado en la parte inferior, toda su cuenta ha sido puesta en un estado no operativo (Suspendida, Cancelada o Archivada).

Solo las cuentas **Prueba** y **Activa** pueden usar la aplicación. Esto no es algo que pueda solucionar desde dentro del portal: **contacte a su administrador** (o al equipo de plataforma) para que la cuenta sea reactivada. Los superadministradores de plataforma cambian el estado de la cuenta desde **Administrador del Sistema → Cuentas → Cambiar Estado** (vea [Administración del sistema](topic:system-administration)). Para el panorama completo, vea [Roles y permisos](topic:roles-and-permissions#estado-de-la-cuenta-cómo-se-ve-una-cuenta-suspendida).

## Falta una funcionalidad en mi menú

Una sección o elemento de menú que esperaba no está. Dos razones comunes:

- **Su rol o sus políticas no lo incluyen.** El acceso a las pantallas y acciones está controlado por roles y políticas. Pida a un administrador que revise sus asignaciones — vea [Roles y permisos](topic:roles-and-permissions).
- **La funcionalidad no está habilitada para su cuenta.** Las funcionalidades opcionales se activan por cuenta. Cuando **Geocercado** está desactivado, desaparece todo el elemento de menú **Geocercas**. Las demás funcionalidades (Documentos, Enlaces Públicos, Notificaciones, etc.) ocultan su **sección o botones** dentro de la pantalla correspondiente, no un elemento de menú. Puede ver qué funcionalidades están activas en **Administración de Cuentas → Funcionalidades de Cuenta** (solo lectura). Activar una es una decisión de suscripción gestionada por el equipo de plataforma.

## "Esta funcionalidad no está habilitada para su cuenta"

Si una acción muestra el mensaje **"Esta funcionalidad no está habilitada para su cuenta."**, la funcionalidad subyacente está desactivada para su cuenta. Nada está roto — simplemente la capacidad no forma parte de su plan actual. Pida a su administrador que habilite la funcionalidad (los superadministradores de plataforma lo hacen en **Administrador del Sistema → Funcionalidades de Cuenta**). Vea [Administración del sistema](topic:system-administration).

## Mensajes de error en rojo (notificaciones)

Un banner rojo que aparece brevemente en la parte superior central de la pantalla y se desvanece tras unos segundos es una notificación de error. La mayoría son **errores transitorios del servidor** — la acción no se realizó esta vez.

- **Reintente la acción.** Muchos de estos se resuelven en un segundo intento.
- Si el mensaje es **"Esta funcionalidad no está habilitada para su cuenta"** o **"Esta cuenta no está operativa actualmente"**, vea las secciones dedicadas más arriba — reintentar no ayudará.
- Si una operación sigue fallando después de varios reintentos, anote la redacción exacta y aproximadamente cuándo ocurrió, luego **escale a su administrador**. Los fallos repetidos en distintas acciones normalmente apuntan a un problema de servicio o de conectividad más que a algo que usted haya hecho.

## Mis vehículos no se mueven en el mapa

Si las unidades muestran posiciones antiguas o no se actualizan, el problema casi siempre está en la cadena de integración GPS. Verifique, en orden, en la página **Integración GPS** (vea [Integración GPS](topic:gps-integration)):

- **¿El operador está habilitado?** Un operador deshabilitado deja de extraer datos y muestra la salud **Deshabilitado**. Vuelva a habilitarlo.
- **¿Las credenciales funcionan?** Use **Probar Credencial** en el operador. Una prueba fallida significa que la conexión con el proveedor está rota (vea más abajo).
- **¿El operador está saludable?** En el panel, verifique si los operadores están **Degradados** o **Fuera de línea**, y observe **Sincronizaciones Recientes** en busca de resultados con estado **Fallida** y códigos de error, y **Alertas GPS Abiertas**.
- **¿El dispositivo sigue asignado a la unidad?** Si la asignación de un dispositivo fue finalizada, su unidad deja de recibir posiciones. Verifique **Asignaciones de Dispositivos** — el vínculo debe estar **Activa**, no **Finalizada**. Reasigne el dispositivo si es necesario.
- **Sincronice** para forzar una actualización, y observe el resultado.

## Falla una prueba de conectividad

Cuando **Probar Credencial** informa **"Ocurrió un error al probar la credencial"**, TrackHub no pudo alcanzar al proveedor con los datos almacenados. Verifique:

- Que la **URL** en el diálogo **Credencial** del operador sea correcta.
- Que el **Nombre de Usuario**, la **Contraseña** y la **Clave Secreta** / **Clave Secreta 2** coincidan con lo que espera el proveedor y no hayan expirado ni sido rotadas.
- Que el proveedor mismo esté en línea.

Corrija los datos en el diálogo **Credencial**, guarde y pruebe de nuevo. Vea [Integración GPS](topic:gps-integration).

## Mi reporte está vacío o es demasiado grande

- **Resultados vacíos.** La causa más común son los **filtros** — el rango de fechas, la selección de unidades u otros criterios no coincidieron con nada. Amplíe la ventana **Desde**/**Hasta**, elija la(s) unidad(es) correcta(s) y genere de nuevo. Confirme también que las unidades efectivamente reportaron datos en ese periodo.
- **Demasiados datos.** Un rango de fechas muy amplio o "todas las unidades" puede producir un resultado muy grande. Algunos reportes ofrecen un filtro **Máximo de Filas** que limita cuántas filas se devuelven — si un reporte parece cortado, aumente **Máximo de Filas**; si es inmanejable, redúzcalo o acote la ventana **Desde**/**Hasta** y seleccione menos unidades.

Para saber cómo generar reportes, vea [Reportes](topic:reports).

## Guardé algo pero el diálogo permaneció abierto

Cuando un diálogo de crear/editar permanece abierto después de que usted presiona **Guardar**, el guardado **no** tuvo éxito — se mostró un error (normalmente una notificación roja en la parte superior de la pantalla). El diálogo se mantiene abierto deliberadamente con sus datos intactos para que no tenga que volver a escribirlos.

- Lea el mensaje de error, corrija la causa (por ejemplo, un campo obligatorio faltante, un error de validación mostrado debajo de un campo, o un error transitorio del servidor) y presione **Guardar** de nuevo.
- Los campos obligatorios están marcados y mostrarán un mensaje si se dejan en blanco; corríjalos primero.

## Advertencias de seguridad o certificado del navegador

En entornos de desarrollo o prueba, el portal y sus servicios pueden usar certificados autofirmados, por lo que su navegador puede mostrar una advertencia de "su conexión no es privada" o de certificado. Esto es esperado solo en esos entornos. En un entorno de producción correctamente desplegado **no** debería ver advertencias de certificado — si las ve, deténgase y repórtelo a su administrador en lugar de omitirlas.
