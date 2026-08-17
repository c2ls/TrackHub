---
id: public-trip-tracking
title: Seguimiento de viajes para clientes
description: La página de solo lectura que su cliente abre con un enlace de viaje compartido — qué muestra, qué nunca muestra y qué significan los mensajes de enlace "no válido" y "vencido".
category: operation
screens: [tripTracking]
featureKey: trip-management
related: [trip-management, public-links]
tags: [seguimiento, cliente, enlace público, eta, compartir]
order: 26
---

# Seguimiento de viajes para clientes

Cuando comparte un viaje desde la pantalla de [Viajes](topic:trip-management), TrackHub le entrega un enlace que abre esta página. La persona a quien se lo envíe **no** necesita una cuenta de TrackHub, ni contraseña, ni invitación: el enlace mismo es la credencial.

Este tema describe lo que ve su cliente, para que usted pueda responderle sin tener que abrir el enlace.

## Qué muestra la página

En la parte superior: el **código del viaje**, su **estado** actual y la salida planeada. El cliente puede presionar **Actualizar** en cualquier momento para traer el estado más reciente.

Debajo, la lista de paradas en orden de visita. Por cada parada:

- su **número** y **nombre**, y la **ciudad** si se conoce;
- la **ventana planeada** que usted prometió;
- la hora de **llegada real**, si el vehículo ya estuvo allí, o la **llegada estimada** si aún no;
- una marca de **Entrega confirmada** si en esa parada se capturó prueba de entrega;
- una etiqueta con el estado de la parada: pendiente, con llegada, con salida u omitida.

Si compartió **la ruta planeada en el mapa**, aparece un mapa con la ruta dibujada. Esa casilla empieza desmarcada, así que un enlace creado sin ella muestra la línea de tiempo de paradas sin ninguna línea de ruta: ese es el resultado buscado, no una falla. Si compartió la posición en vivo **y** el viaje está en curso, la posición actual del vehículo aparece en el mapa junto con la hora del último reporte; un enlace con posición en vivo pero sin ruta igual muestra un mapa, solo con el vehículo.

La **ciudad** es el único dato de ubicación que una parada revela aquí. La dirección completa de entrega nunca se envía, marque lo que marque, de modo que una parada sin ciudad diligenciada solo muestra su nombre.

## Qué no muestra nunca la página

La página solo puede mostrar lo que el servidor le envió, y el servidor arma cada instantánea a partir de las casillas que usted marcó al crear el enlace. Lo que no marcó sencillamente no está en los datos: no hay campos ocultos ni nada que se pueda descubrir inspeccionando la página.

Hay cosas que nunca se comparten, **marque lo que marque**:

- **Cifras de peajes y costos de cualquier tipo.** Nunca salen de su cuenta.
- **Notas internas** del viaje o de sus paradas.
- **Datos de contacto del conductor.** Si compartió al conductor, solo aparece su **nombre de pila**.
- **Archivos de documentos y fotos**, incluidos los adjuntos de las pruebas de entrega. El cliente solo ve *si* una parada tiene prueba de entrega, nunca la firma ni la foto.
- **Historial de posiciones sin procesar.** El cliente ve, como máximo, la posición actual; nunca el recorrido completo.
- **Identificadores internos** de sus unidades, conductores, geocercas o documentos.

La posición en vivo además está limitada por el propio estado del viaje: aparece solo mientras el viaje está **en curso** y desaparece cuando se completa, se cancela o se aborta, aunque el enlace siga vigente.

## Los mensajes que puede ver un cliente

| Lo que ve | Qué pasó | Qué hacer |
|---|---|---|
| **Este enlace de seguimiento no es válido** | El enlace fue revocado, el viaje ya no existe, o el enlace corresponde a algo distinto del seguimiento de viajes. | Cree un enlace nuevo desde el viaje y envíelo. |
| **Este enlace de seguimiento venció** | Pasó la fecha y hora de vencimiento que usted fijó. | Cree un enlace nuevo con un vencimiento posterior. |
| **El enlace está incompleto** | Se perdió una parte de la dirección, normalmente porque se cortó al pegarla o porque el correo la partió en dos líneas. | Pídale que abra el enlace completo, o reenvíelo en una sola línea. |
| **El seguimiento no está disponible por ahora** | No se pudo contactar a TrackHub. | Pídale que reintente en unos minutos y revise el [Estado de la plataforma](topic:platform-status). |

Un enlace de una cuenta a la que se le apagó la funcionalidad de viajes muestra el mensaje de "no válido". Esto es deliberado: TrackHub no le revela nada sobre cuentas ni funcionalidades a un visitante anónimo.

## Administrar los enlaces que compartió

Cada enlace de viaje es también un enlace público común y aparece en [Enlaces públicos](topic:public-links) con su propósito, su vencimiento y cuántas veces se abrió. Cada apertura exitosa se contabiliza y se audita.

Para dejar un enlace sin efecto, abra el diálogo **Compartir** del viaje y presione **Revocar** en ese enlace. La revocación es inmediata: la siguiente actualización mostrará el mensaje de "no válido".

El token del enlace se muestra **una sola vez**, en el momento de crearlo. TrackHub no puede volver a mostrárselo; si un cliente pierde su enlace, revoque el anterior y emita uno nuevo.
