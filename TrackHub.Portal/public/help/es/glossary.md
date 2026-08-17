---
id: glossary
title: Glosario
description: Significado en lenguaje sencillo de los términos y de la información en pantalla que encontrará en TrackHub.
category: reference
screens: []
related: [getting-started, dashboard-live-map, gps-integration]
tags: [glosario, términos, definiciones, referencia]
order: 10
---

# Glosario

Significado en lenguaje sencillo de los términos y de la información en pantalla que encontrará en TrackHub.

## Las unidades y qué son

**Unidad** — Cualquier cosa que rastree en el mapa: un vehículo, un equipo, un activo o incluso una persona. En todas las pantallas se identifica como **Unidad**. Cada unidad tiene un nombre y un **tipo**.

**Tipo de Unidad** — Qué clase de cosa es la unidad, mostrada con su propio ícono. Los tipos disponibles incluyen: **Desconocido, Aeronave, Activo, Bicicleta, Barco, Automóvil, Contenedor de Carga, Vehículo de Construcción, Niño, Furgoneta de Entrega, Dron, Persona Mayor, Vehículo de Flota, Equipo Pesado, Ganado, Motocicleta, Paquete, Persona, Mascota, Autobús Escolar, Scooter, Taxi, Herramientas, Camión** y **Tractor**. El tipo también influye en cómo se calculan los viajes (por ejemplo, algunos tipos determinan una parada mediante la señal de ignición/ACC).

**Grupo** — Una colección con nombre de unidades y/o usuarios. Los grupos controlan quién puede ver cuáles unidades: por lo general usted ve las unidades de los grupos a los que pertenece.

## Dispositivos y proveedores de GPS

**Dispositivo** — El rastreador GPS físico (el hardware) que va dentro o sobre una unidad y reporta su ubicación. Un dispositivo tiene un nombre, un identificador/número de serie y un tipo de dispositivo (por ejemplo, **Celular, Satélite, Escáner OBD, Teléfono, Reloj Inteligente, Marino, Aviación, Rastreo de Mascotas**, entre otros).

**Operador** — Una conexión a un proveedor de GPS, el servicio externo que recopila los datos de sus dispositivos y los pasa a TrackHub. Cada operador usa un **Protocolo** (el "idioma" que habla), como **Command Track, Traccar, Geotab** o **GPS Gate**.

**Credencial** — El nombre de usuario, la contraseña o las claves que TrackHub usa para conectarse al servicio de un operador. Normalmente prueba una credencial con **Probar Credencial**; una prueba exitosa confirma que TrackHub puede comunicarse con el proveedor.

**Proveedor de Geocodificación** — Un servicio que convierte las coordenadas del mapa (latitud y longitud) en una dirección legible. Los proveedores admitidos incluyen **Nominatim, OpenRouteService** y **Google**.

## Información de posición (lo que el mapa muestra de una unidad)

Cuando abre una unidad en el mapa, ve su **posición** más reciente y sus detalles:

- **Velocidad (Km/Hr)** — Qué tan rápido se mueve la unidad, mostrada en kilómetros por hora.
- **Course** — La dirección de la brújula hacia la que se dirige la unidad.
- **Ignición (ACC Status)** — Si la ignición del vehículo está **ACC On** o **ACC Off**. Así es como TrackHub determina si el motor está en marcha.
- **Altitud** — La altura de la unidad sobre el nivel del mar.
- **Kilometraje** — La distancia que ha recorrido la unidad (la lectura de su odómetro).
- **Horómetro** — Cuántas horas ha estado funcionando la unidad (o su motor), útil para equipos que se mantienen por tiempo de operación en lugar de por distancia.
- **Satélites** — Cuántos satélites GPS usaba el dispositivo para esta fijación; más satélites suelen significar una ubicación más precisa.
- **Temperatura** — Una lectura de temperatura, cuando el dispositivo la proporciona.
- **Dirección** — La dirección legible de la posición. Si aún no se muestra, use **Resolve address** para consultarla.
- **Coordenadas** — La latitud y longitud exactas.
- **Estado** — Si la unidad está **Moving** (en movimiento), **Stopped** (detenida) o **Offline** (sin conexión). Una unidad pasa a **Offline** cuando no ha reportado por más tiempo que el intervalo en línea de la cuenta.
- **Último Reporte** — Cuándo se recibió la posición (mostrado como "just now", "min ago", "h ago" o "d ago").
- **Evento / alarma** — Algunas posiciones traen un código de evento o alarma enviado por el dispositivo (por ejemplo, un botón de pánico o el disparo de un sensor específico); estos pueden aparecer como alarmas en un viaje.

**Hora del dispositivo vs. hora del servidor** — Una posición tiene dos marcas de tiempo. La **hora del dispositivo** es el momento en que el dispositivo GPS registró la ubicación. La **hora del servidor** es el momento en que TrackHub la recibió. Suelen estar cercanas, pero puede aparecer una diferencia si un dispositivo estuvo sin señal y envió sus datos más tarde.

## Viajes y reproducción

**Viaje** — Un trayecto único de una unidad, desde que arranca hasta que se detiene. El resumen de un viaje puede mostrar sus puntos **Desde** y **Hasta**, la **Distancia Total**, la **Duración**, la **Vel. Máx**, la **Vel. Prom**, el número de **Paradas** y cualquier **Alarmas**. El tiempo transcurrido se divide en **En Tránsito** (en movimiento) y **Detenido**.

**Reproducción (Playback)** — Reproducir en el mapa los movimientos pasados de una unidad como un video, usando **Reproducir**, **Pausar**, un control de **Velocidad** y una **Línea de tiempo**. Puede elegir la **Fuente del historial**: el **Proveedor GPS** (los datos sin procesar del proveedor) o **TrackHub** (el historial que TrackHub ha almacenado por su cuenta). También puede **Exportar** el recorrido reproducido. Vea [Viajes y reproducción](topic:dashboard-trips-replay).

## Zonas y lugares en el mapa

**Geocerca** — Una zona que dibuja en el mapa para vigilar un área específica, por ejemplo, para saber cuándo una unidad entra o sale de ella. Cada geocerca tiene un nombre, un **Tipo**, un **Color** y puede estar activa o inactiva.

- Los **tipos de geocerca** describen qué representa la zona: **Cliente, Construcción, Zona de Peligro, Estación de Combustible, Taller, Hospital, Hotel, Oficina, Parque, Parqueadero, Área Restringida, Tienda, Escuela** y **Bodega**.
- Los **colores de geocerca** son simplemente la forma en que se dibuja la zona en el mapa para poder distinguir las zonas de un vistazo. Los nombres de los colores se muestran en inglés en toda la aplicación: **Red, Blue, Green, Yellow, Orange, Purple, Pink, Brown, Black** y **White**.

**Punto de Interés (POI)** — Un único lugar marcado en el mapa (un pin), en lugar de un área. Cada punto de interés tiene un nombre, un **Tipo**, una ubicación (latitud/longitud o dirección) y un color. Los tipos de punto de interés incluyen **Sitio de Cliente, Bodega, Estación de Combustible, Peaje, Área de Descanso, Taller, Puerto** y **Otro**. Los usuarios cotidianos pueden ver los puntos de interés; los gestores pueden crearlos y editarlos.

## Conectar dispositivos a unidades (asignaciones)

Una **asignación de dispositivo** vincula un dispositivo GPS con la unidad en la que está instalado actualmente, de modo que la unidad muestre las posiciones de ese dispositivo. Como el rastreador de una unidad puede cambiarse con el tiempo, las asignaciones llevan detalle adicional:

- **Principal** — Marca el dispositivo principal de una unidad cuando más de un dispositivo podría reportar por ella.
- **Prioridad** — Cuando varios dispositivos podrían alimentar una unidad, la prioridad decide cuál se prefiere.
- **Vigente Desde / Vigente Hasta** — Las fechas en que aplica una asignación en particular. Esto mantiene un historial preciso de qué dispositivo estuvo en qué unidad y cuándo.
- **Estado de la asignación** — **Activa** (en uso ahora), **Finalizada** (terminada) o **Reemplazada** (sustituida por una asignación más reciente).

El estado propio de un dispositivo sincronizado se muestra como **Sin asignar** (aún no vinculado a una unidad), **Asignado**, **Ignorado** (omitido deliberadamente), **Eliminado** o **Faltante**.

## Sincronización de GPS y estado de salud

**Sincronización (Sync Run)** — Una ronda en la que TrackHub contacta a un proveedor de GPS para obtener los dispositivos y las posiciones más recientes. Una sincronización registra cuándo **Inicio** y cuándo llegó a su **Fin**, qué la disparó (**Origen**: un horario automático o una solicitud manual), el **Resultado**, cuántos dispositivos se agregaron o cambiaron, cuántas posiciones se aceptaron y cualquier **Código de Error**. También puede **Sincronizar** manualmente.

**Estado del Operador** — Un indicador sencillo de qué tan bien funciona la conexión con un proveedor de GPS, basado en sus sincronizaciones recientes. Se lee como **Saludable**, **Degradado**, **Fuera de línea**, **Deshabilitado** o **Desconocido**, y va acompañado de la última sincronización exitosa, el último error y la latencia (tiempo de respuesta).

**Retención de Posiciones** — Las reglas sobre cuánto tiempo se conserva el historial de posiciones almacenado, junto con la frecuencia con que se almacenan las posiciones (el **Intervalo de Almacenamiento (Segundos)**) y el número de **Días de Retención**. Estos ajustes dependen de su plan de suscripción y son gestionados por el administrador. Vea [Integración GPS](topic:gps-integration).

## Alertas y notificaciones

**Evento de Alerta** — Algo destacable que TrackHub detectó y registró, como una unidad que entra a una geocerca, una interrupción de comunicación o un documento a punto de vencer. Cada alerta tiene un **Tipo**, una **Severidad**, un **Módulo de Origen** (qué parte de TrackHub la generó) y un **Estado**.

**Severidad** — Qué tan importante o urgente es una alerta, desde rutinaria/informativa hasta crítica. Las alertas críticas son las que necesitan atención primero; pueden notificar a los destinatarios de inmediato y escalarse si nadie responde a tiempo.

**Estado de la alerta** — En qué punto de su ciclo de vida está la alerta. Usted usa **Reconocer Alerta** para indicar que la ha visto, y **Resolver Alerta** una vez que ha sido atendida.

**Regla de Notificación** — Una regla que decide cuándo debe generarse una alerta y a quién debe avisarse. Una regla tiene un evento disparador, una severidad, los destinatarios y ajustes de entrega, y puede estar **habilitada** o deshabilitada.

## Documentos

**Documento** — Un archivo almacenado en relación con algo de su cuenta (por ejemplo, el seguro de un vehículo o la licencia de un conductor). Los documentos admiten subida, **Nueva Versión** (reemplazar con un archivo actualizado conservando el historial), **Descargar**, **Vista Previa**, **Compartir**, **Firmar** y seguimiento del vencimiento.

**Tipo de documento** — Qué clase de documento es, elegido de la lista configurable de su cuenta (los ejemplos incluyen **SOAT, RTM, License, Medical, POD, Receipt, Invoice, Manifest, Certificate, Photo, Signature** y **Other**). El campo se rotula como **Tipo**.

**Clasificación** — Qué tan sensible es un documento, lo cual controla quién puede abrirlo: **Público, Interno, Confidencial** o **Legal**. Las clasificaciones más sensibles (Confidencial, Legal) están restringidas a los usuarios cuya política de acceso lo permite.

**Estado de Análisis** — El resultado del análisis de seguridad (antivirus) por el que pasa un documento antes de poder usarse:

- **Pendiente** — Aún no analizado.
- **En Cuarentena** — Retenido mientras se analiza; se muestra como "Scanning — available once clean".
- **Limpio** — Pasó el análisis y es seguro de usar.
- **Infectado** — No pasó el análisis y quedó bloqueado ("Blocked: failed virus scan").
- **Fallido** — El análisis no pudo completarse.

**Estado del documento** — En qué punto de su vida está el documento: **Pendiente, Cargado, Activo, Expirado, Reemplazado, Anulado** o **Eliminado**.

## Compartir y acceso de soporte

**Enlace Público** — Un enlace web seguro y con tiempo limitado que permite a alguien **fuera** de TrackHub ver una cosa específica (como un documento o una vista compartida) sin iniciar sesión. Un enlace público registra su propósito, alcance, fecha de vencimiento y cuántas veces se ha usado, y puede revocarse en cualquier momento. El token secreto del enlace se muestra **solo una vez** cuando se crea —cópielo en ese momento, porque no se muestra de nuevo.

**Concesión de Soporte** — Permiso temporal y auditado para que una persona de **soporte** de TrackHub acceda a su cuenta y pueda ayudarlo, por ejemplo, mientras investiga un ticket. Una concesión de soporte registra una razón, una referencia de ticket, un nivel de acceso y horas de inicio/fin, debe ser aprobada y puede revocarse. Da acceso solo durante la ventana acordada.

**Cliente API / Cliente de Servicio** — Un sistema externo registrado que se conecta a TrackHub de forma automática (de máquina a máquina), en lugar de una persona que inicia sesión. Sus permisos se limitan a recursos y acciones específicos.

## Términos de cuenta y plataforma

**Cuenta** — El espacio de su organización en TrackHub, que contiene todas sus unidades, usuarios, dispositivos y datos. Una cuenta tiene un **Tipo** (Personal, Empresarial o Asociado) y un estado.

**Estado de la cuenta** — El estado general de la cuenta: **Prueba, Activa, Suspendida, Cancelada** o **Archivada**. Las cuentas en Prueba y Activa funcionan con normalidad; las cuentas Suspendida, Cancelada y Archivada quedan bloqueadas con un mensaje "Cuenta No Disponible". Vea [Roles y permisos](topic:roles-and-permissions#estado-de-la-cuenta-cómo-se-ve-una-cuenta-suspendida).

**Personalización** — El nombre para mostrar, el logo, el color primario y el encabezado de reporte/exportación de su cuenta, para que la aplicación y sus documentos exportados lleven su propia identidad.

**Funcionalidad de Cuenta** — Una capacidad que puede activarse o desactivarse para toda su cuenta (por ejemplo, Geocercado, Documentos o Integración GPS). Cuando una funcionalidad está desactivada, sus pantallas o secciones quedan ocultas para todos en la cuenta.

**Auditoría** — Un registro continuo de las acciones importantes realizadas en la cuenta: quién hizo qué, sobre cuál recurso, con qué resultado y cuándo. Se usa para revisar el historial y apoyar el cumplimiento.

**Proceso en Segundo Plano** — Una tarea que TrackHub ejecuta automáticamente entre bastidores (por ejemplo, buscar documentos por vencer o depurar datos antiguos). La lista de Procesos en Segundo Plano muestra cada proceso, su estado, cuántos intentos ha realizado y cuándo inició.

**Rol** — Un conjunto de permisos con nombre asignado a un usuario del portal (Administrador, Gestor o Usuario). Vea [Roles y permisos](topic:roles-and-permissions).

**Política** — Una agrupación de derechos de acceso con nombre (como Acceso Total, Administrar Usuarios, Solo Lectura, Actualización Limitada o Auditoría) que puede asignarse a los usuarios para un control más detallado.
