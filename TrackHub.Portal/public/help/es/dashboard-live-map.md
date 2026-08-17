---
id: dashboard-live-map
title: Mapa en vivo
description: Vigile su flota en tiempo real — tarjetas de resumen, filtros, marcadores, capas, modo Seguir y la lista lateral de unidades.
category: operation
screens: [dashboard]
related: [dashboard-trips-replay, geofences, glossary]
tags: [mapa, unidades, seguir, filtros]
order: 10
---

# Mapa en vivo

Vigile toda su flota en tiempo real: dónde está cada unidad en este momento, cuáles se están moviendo y cuáles permanecen dentro de una geocerca.

Abra **Tablero** desde el menú de la izquierda. La pantalla tiene dos pestañas en la parte superior:

- **Unidades** — el mapa en vivo con cada unidad y su posición actual (esta página).
- **Posiciones** — viajes históricos y reproducción, que se cubren en [Viajes y reproducción](topic:dashboard-trips-replay).

Se encuentra en la pestaña **Unidades** de forma predeterminada.

El mapa que ve (Google u OpenStreetMap) depende de su cuenta. TrackHub puede configurarse para usar cualquiera de los dos proveedores; si su cuenta usa Google, se le asigna una Clave de Mapas. Este es un ajuste a nivel de cuenta que administra su administrador, por lo que el mapa puede verse distinto al de otra empresa que use TrackHub. Todo lo descrito a continuación funciona igual en ambos.

En la parte superior derecha de la pantalla se encuentra el cuadro **Buscar unidades**. Escriba el nombre o la placa de una unidad para filtrar al instante la [lista lateral de unidades](#la-lista-lateral-de-unidades). Este cuadro aparece solo en la pestaña **Unidades**.

## Las tarjetas de resumen

Cinco tarjetas se ubican en la parte superior y siempre reflejan **toda** su flota autorizada — los filtros de abajo no cambian estas cifras:

| Tarjeta | Qué cuenta |
|---|---|
| **Total de Unidades** | Cada unidad que reporta una posición. |
| **Unidades Activas** | Unidades que han reportado recientemente (dentro del intervalo en línea de su cuenta). |
| **Unidades en Movimiento** | Unidades actualmente en movimiento (velocidad mayor que cero). |
| **En Geocerca** | Unidades ubicadas actualmente dentro de una de sus geocercas. |
| **Alertas Críticas** | Alertas abiertas de severidad crítica que aún no han sido reconocidas ni resueltas. |

Las tarjetas **Unidades Activas**, **Unidades en Movimiento** y **En Geocerca** también muestran ese conteo como porcentaje de su total.

Al hacer clic en el ícono de marcador de la tarjeta **En Geocerca** se activa y desactiva la capa de geocercas en el mapa (consulte [Capas del mapa](#capas-del-mapa)).

## La barra de filtros

Debajo de las tarjetas hay una fila de filtros que acotan **lo que muestra el mapa**. No cambian las tarjetas de resumen ni la lista lateral. Cada filtro es un menú desplegable que inicia en su valor "todos":

- **Grupo** — muestra solo las unidades de un grupo elegido (*Todos los grupos* de forma predeterminada).
- **Tipo de Unidad** — muestra solo un tipo de unidad, por ejemplo Camión o Automóvil (*Todos los tipos* de forma predeterminada).
- **Operador** — muestra solo las unidades atendidas por un operador GPS elegido (*Todos los operadores* de forma predeterminada).
- **Estado** — *Todos los estados*, **En Movimiento**, **Detenida** o **Sin Conexión**.

A la derecha de esa misma fila hay tres chips de alternancia: **Puntos de Interés**, **Seguir** y **Recorrido** (explicados en [Capas del mapa](#capas-del-mapa) y [Modo Seguir](#modo-seguir)).

## El mapa en vivo

Cada unidad aparece como un marcador de color que apunta en su dirección de desplazamiento. El color del marcador refleja el estado de la unidad: verde para en movimiento, rojo para detenida y gris para sin conexión. Una unidad se considera sin conexión cuando no ha reportado dentro del intervalo en línea de su cuenta; de lo contrario está en movimiento cuando su velocidad es mayor que cero o su ignición está encendida, y detenida el resto del tiempo.

Cuando muchas unidades están cerca entre sí, se agrupan en un **clúster** que muestra un conteo; acerque el zoom o haga clic en el clúster para separarlo en marcadores individuales. El mapa se centra en su flota la primera vez que se carga.

Haga clic en cualquier marcador para seleccionar esa unidad, abrir su ventana emergente y centrar el mapa en ella. Al seleccionar una fila de la [lista lateral de unidades](#la-lista-lateral-de-unidades) también se selecciona la unidad y se centra el mapa en ella.

## Ventanas emergentes de unidad

Al hacer clic en un marcador se abre una ventana emergente con los últimos detalles de la unidad. Según lo que reporte el dispositivo, es posible que vea:

- El **Nombre** y el tipo de unidad en el encabezado de color.
- Fecha/hora del **Último Reporte**, más una nota amigable de "justo ahora / hace X min".
- La **Velocidad** en km/h con una etiqueta de **En Movimiento** o **Detenido**.
- **Kilometraje**, **Temperatura**, **Horómetro**, **Ignición** (encendido/apagado), **Satélites** y **Altitud** — solo cuando el dispositivo los envía.
- **Dirección** — la dirección de calle de la posición. Si aún no hay una dirección almacenada, la ventana emergente muestra en su lugar la **Ubicación** (ciudad, estado, país) o las **Coordenadas** en crudo con un botón **Obtener dirección**; haga clic en él para consultar la dirección de calle bajo demanda.
- Dos botones de acción rápida en la parte inferior: **Compartir por WhatsApp** (envía un enlace de Google Maps a la ubicación) y **Street View** (abre Google Street View en ese punto).

## Capas del mapa

Se pueden activar tres capas de forma independiente.

**Puntos de Interés** — haga clic en el chip **Puntos de Interés** en la barra de filtros para mostrar sus ubicaciones guardadas (bodegas, estaciones de combustible, sitios de clientes, etc.) en el mapa. Se cargan la primera vez que activa la capa.

**Geocercas** — active la capa de geocercas haciendo clic en el ícono de marcador de la tarjeta **En Geocerca**. Sus geocercas aparecen entonces como formas de color sobre el mapa. Consulte [Geocercas](topic:geofences) para saber cómo crearlas y darles color.

**Recorrido** — haga clic en el chip **Recorrido** para dibujar una breve línea de rastro con las posiciones más recientes de la unidad **seleccionada**, de modo que pueda ver por dónde acaba de pasar. Seleccione primero una unidad.

## Modo Seguir

Haga clic en el chip **Seguir** para mantener el mapa centrado en la unidad seleccionada a medida que llegan nuevas posiciones. Seguir solo está disponible una vez que ha seleccionado una unidad; el chip permanece deshabilitado hasta entonces. Si usted mismo desplaza (arrastra) el mapa, Seguir se desactiva de nuevo.

## Controles del mapa

Sobre el propio mapa se ubican pequeños controles:

- **Pantalla completa** (arriba a la izquierda) — expande el mapa para llenar la pantalla; haga clic de nuevo para salir.
- **Medir distancia** (arriba a la izquierda) — mide la distancia entre los puntos que marque en el mapa, en unidades métricas.
- **Barra de escala** (abajo a la izquierda) — muestra la escala actual del mapa en unidades métricas.

El mapa también intenta usar la ubicación de su navegador como centro inicial, y sigue el tema claro/oscuro de la aplicación, por lo que cambiar la aplicación a modo oscuro también oscurece las capas del mapa.

## Actualización automática y el contador

Cuando la actualización automática está habilitada para su cuenta, un pequeño contador en el mapa (por ejemplo `45 s.`) cuenta hacia atrás hasta la siguiente actualización. Cuando llega a cero, TrackHub recarga todas las posiciones y el conteo **En Geocerca**, y luego reinicia el contador. El intervalo de actualización es un ajuste de cuenta que administra su administrador. Si la actualización automática está desactivada, no se muestra ningún contador y las posiciones se actualizan cuando recarga la página. Una actualización fallida conserva las últimas posiciones conocidas en el mapa en lugar de dejarlo en blanco.

## La lista lateral de unidades

A la derecha hay una lista desplazable de todas sus unidades, con columnas:

- **Est.** — un punto de estado (verde cuando está en movimiento, rojo cuando está detenida).
- **Nombre** — el nombre o la placa de la unidad.
- **Fecha Hora** — hora de su último reporte.
- **Velocidad (Km/Hr)**.

Haga clic en una fila para seleccionar esa unidad, resaltarla y centrar el mapa en ella. El cuadro **Buscar unidades** de la parte superior derecha filtra esta lista. Encima de la lista, chips de color resumen cuántas unidades de cada tipo están reportando actualmente.

Para ver el mapa en vivo se requiere acceso a las posiciones. Consulte [Roles y permisos](topic:roles-and-permissions).
