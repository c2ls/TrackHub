---
id: dashboard-trips-replay
title: Viajes y reproducción
description: Consulte los viajes de una unidad por rango de fechas, lea las estadísticas, reproduzca el recorrido en el mapa y expórtelo a CSV.
category: operation
screens: [dashboard]
related: [dashboard-live-map, reports, trip-management]
tags: [viajes, reproducción, historial, exportar]
order: 20
---

# Viajes y reproducción

Revise dónde ha estado una unidad: consulte sus viajes para cualquier rango de fechas, lea las estadísticas de distancia, tiempo y velocidad, reproduzca el recorrido en el mapa y exporte la trayectoria a una hoja de cálculo.

Abra **Tablero** desde el menú de la izquierda y haga clic en la pestaña **Posiciones** (junto a **Unidades**). Esta pestaña es para recorridos históricos; la vista en vivo se cubre en [Mapa en vivo](topic:dashboard-live-map).

Aquí un *viaje* es cualquier tramo que la unidad recorrió entre detenciones, reconstruido a partir de su historial de posiciones: no es un trabajo planificado ni despachado. Para recorridos planificados con varias paradas, ETA, peajes y enlace de seguimiento para el cliente, vea [Viajes y planificación de rutas](topic:trip-management), que tiene su propia reproducción del recorrido registrado contra la ruta planeada.

## Consultar viajes

En la parte superior de la pestaña hay una fila de filtros con un campo **Fecha Inicio**, un campo **Fecha Fin**, un selector de **Unidad** y un botón **Buscar**:

1. Elija la **Unidad** cuyo historial desea. La primera unidad queda seleccionada por usted.
2. Elija la **Fecha Inicio** y la **Fecha Fin** — un rango de fecha y hora.
3. Haga clic en **Buscar**.

Una unidad, la fecha de inicio y la fecha de fin son todos obligatorios. TrackHub carga entonces los viajes de esa unidad para el rango y los dibuja en el mapa. Si no se encuentra nada, el mapa y la lista simplemente quedan vacíos.

## Fuente del historial

Si su cuenta tiene habilitada la funcionalidad de historial de posiciones, aparece un selector **Fuente del historial** con dos opciones:

- **Proveedor GPS** — historial leído directamente del operador/proveedor GPS (la opción predeterminada).
- **TrackHub** — historial almacenado por el propio TrackHub.

Elija la fuente antes de hacer clic en **Buscar**. Si su cuenta no tiene esta funcionalidad, el selector queda oculto y TrackHub siempre usa el historial del proveedor. El historial de posiciones es una funcionalidad de cuenta que administra su administrador — consulte [Roles y permisos](topic:roles-and-permissions).

## La lista de viajes

A la derecha está la lista de viajes de su consulta, con el rango de fechas elegido mostrado en la parte superior. Cada entrada es una de:

- **En Tránsito** — la unidad estaba desplazándose. Muestra la distancia recorrida.
- **Detenido** — la unidad estaba estacionada o inactiva.

Cada entrada también muestra sus horas **Desde** y **Hasta** y su duración. Los tramos en tránsito se marcan en verde con una flecha hacia adelante; las paradas se marcan en rojo con un ícono de detención. Haga clic en cualquier entrada para seleccionar ese viaje: se resalta en el mapa y habilita la [reproducción](#reproducir-un-viaje).

## Estadísticas del viaje

Un panel **Resumen** flota sobre el mapa. Cuando no hay ningún viaje seleccionado, agrega toda la consulta; cuando selecciona un viaje, muestra solo ese viaje. Lista:

- **Distancia Total** (km)
- **Duración**
- **Vel. Máx** (km/h)
- **Vel. Prom** (km/h)
- **Paradas**
- **Alarmas**

Puede ocultar o mostrar este panel con el botón de estadísticas en el mapa.

## Reproducir un viaje

Seleccione un viaje en la lista. Si el viaje tiene una línea de tiempo, aparecen controles de reproducción debajo del mapa:

- **Reproducir / Pausar** — inicia o detiene la animación; un marcador se desplaza a lo largo de la ruta.
- **Velocidad** — elija velocidad de reproducción **1x**, **2x**, **4x** u **8x**.
- **Control deslizante de la línea de tiempo** — arrastre para saltar a cualquier punto del viaje.

## Exportar a CSV

Haga clic en el botón **Exportar** (con el ícono de descarga) en la parte superior derecha de la pestaña para guardar los viajes cargados como una hoja de cálculo CSV. El botón permanece atenuado hasta que una consulta haya devuelto viajes.

El archivo contiene una fila por cada posición registrada, con estas columnas:

| Columna | Significado |
|---|---|
| **Unidad** | El nombre de la unidad. |
| **Viaje** | El viaje al que pertenece el punto. |
| **Fecha y hora** | Cuándo se registró la posición. |
| **Latitud** | |
| **Longitud** | |
| **Velocidad (km/h)** | |

No hay columna de dirección. Los puntos históricos de viaje llevan solo coordenadas y velocidad, no direcciones de calle, por lo que la exportación omite intencionalmente una columna de dirección. Para ver la dirección de una posición en vivo, use el botón **Obtener dirección** en la ventana emergente de una unidad en el [mapa en vivo](topic:dashboard-live-map).

El archivo se nombra automáticamente a partir de la unidad, la fuente del historial y el rango de fechas, por ejemplo `replay_TRUCK-01_PROVIDER_2026-07-01_2026-07-05.csv`.

Para un conjunto más amplio de resúmenes basados en el historial y salidas programadas, consulte [Reportes](topic:reports).

Consultar viajes requiere acceso a posiciones e historial de posiciones. Consulte [Roles y permisos](topic:roles-and-permissions).
