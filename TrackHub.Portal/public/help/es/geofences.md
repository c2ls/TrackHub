---
id: geofences
title: Geocercas
description: Dibuje zonas con nombre en el mapa —polígonos o círculos— y reciba alertas de entrada, salida y permanencia de las unidades.
category: operation
screens: [geofenceManager]
featureKey: geofencing
related: [dashboard-live-map, alerts-notifications, trip-management]
tags: [geocerca, zonas, polígono, círculo, alertas, permanencia]
order: 30
---

# Geocercas

Una geocerca es un área que usted dibuja en el mapa y a la que le asigna un nombre, un tipo y un color. Una vez que existe una zona, TrackHub puede indicarle qué unidades están dentro de ella y —si activa las opciones— generar una alerta cuando una unidad entra, sale o permanece dentro de ella. El conteo **En Geocerca** del [mapa en vivo](topic:dashboard-live-map) es el número de unidades ubicadas dentro de cualquiera de sus geocercas en este momento.

Las geocercas solo están disponibles cuando la función de geocercas está habilitada para su cuenta, y administrarlas requiere permisos de geocercas. Consulte [Roles y permisos](topic:roles-and-permissions).

Las geocercas también se reutilizan como paradas de viaje: al planificar un viaje puede ubicar una parada seleccionando una zona, y la llegada a esa parada se evalúa entonces contra la forma de la zona y no contra un círculo alrededor de un punto. Vea [Viajes y planificación de rutas](topic:trip-management).

## Abrir el administrador de Geocercas

Abra **Geocercas** desde el menú de la izquierda. La pantalla muestra el mapa a la izquierda y, a la derecha, una lista paginada de sus geocercas con las columnas **Nombre**, **Tipo**, **Color** y una columna de **Acción** (eliminar).

## Crear una geocerca

El mapa tiene una barra de controles en su borde derecho con dos herramientas de dibujo:

1. Haga clic en **Dibujar polígono** (el ícono de pentágono) para dibujar una forma libre, o en **Dibujar círculo** (el ícono de círculo) para dibujar una zona redonda.
2. **Dibuje la forma** en el mapa:
   - Para un polígono, haga clic en puntos para trazar las esquinas del área. Puede ajustar las esquinas mientras dibuja.
   - Para un círculo, arrastre desde el centro para fijar el radio.
3. Cuando la forma se vea bien, haga clic en el control **Guardar forma** (el ícono de guardar) — aparece en la barra de controles solo mientras dibuja o edita.
4. Se abre el formulario de detalle de la geocerca. Complételo (vea abajo) y haga clic en **Guardar**.

## Detalle de la geocerca

El formulario tiene estos campos:

- **Nombre** – obligatorio; cómo se etiqueta la zona.
- **Descripción** – texto libre opcional.
- **Tipo** – obligatorio; elija qué representa la zona. Los tipos disponibles son: Cliente, Construcción, Zona de Peligro, Estación de Combustible, Taller, Hospital, Hotel, Oficina, Parque, Parqueadero, Área Restringida, Tienda, Escuela, Bodega.
- **Color** – obligatorio; el color con el que se dibuja la zona (Rojo, Azul, Verde, Amarillo, Naranja, Púrpura, Rosado, Marrón, Negro, Blanco). Use el color para distinguir los tipos de zona de un vistazo.
- **Radio (m)** y **Centro** – se muestran solo para geocercas de círculo. El radio debe estar entre 10 y 100000 m; puede escribir un radio exacto aquí después de dibujar. El centro es de solo lectura y proviene de donde dibujó el círculo.
- **Umbral de permanencia (min)** – opcional; entre 1 y 10080 minutos. Si se establece, una unidad debe permanecer dentro de la zona al menos este tiempo antes de que se genere una alerta de permanencia. Déjelo en blanco para no tener alerta de permanencia.
- **Activa** – marque para mantener la geocerca en uso.
- **Alerta al entrar** – marque para recibir alerta cuando una unidad entra a la zona.
- **Alerta al salir** – marque para recibir alerta cuando una unidad sale de la zona.

Las alertas de entrada, salida y permanencia se entregan a través del sistema de alertas y notificaciones; consulte [Alertas y notificaciones](topic:alerts-notifications) para saber cómo las recibe.

## Editar una geocerca

Seleccione la geocerca (desde la lista o en el mapa), luego use los controladores de edición del mapa para remodelarla: arrastre sus esquinas (polígono) o su radio (círculo) al nuevo contorno y haga clic en el control **Guardar forma** para confirmar la geometría. El formulario de detalle se reabre para que también pueda cambiar el nombre, el tipo, el color, la descripción, el umbral de permanencia o las opciones de alerta; haga clic en **Guardar** para finalizar.

## Eliminar una geocerca

En la lista de la derecha, haga clic en la acción de eliminar de la fila de la geocerca. Aparece una confirmación **Eliminar Geocerca** ("¿Está seguro de que desea eliminar esta geocerca?") — confirme para quitarla. Esto no se puede deshacer.

## Encontrar una geocerca en la lista

La lista se filtra y pagina en el servidor, así que use la barra de herramientas sobre el mapa para acotarla:

- **Tipo** – restringe la lista a un tipo de geocerca (o "Todos los tipos").
- **Estado** – muestra zonas **Activas**, **Inactivas** o **Todas**. Su valor predeterminado es **Activas**, así que las zonas retiradas quedan a un clic bajo **Inactivas** o **Todas**.
- El cuadro de búsqueda de la barra superior filtra la lista por nombre mientras escribe.

El pie bajo la lista muestra "Mostrando *desde*–*hasta* de *total*" con flechas para pasar entre los resultados. Al seleccionar una entrada se resalta la forma correspondiente en el mapa. El mapa siempre muestra todas las geocercas, incluso mientras la lista está filtrada o paginada.

## Cómo aparecen las geocercas en el mapa en vivo

En el mapa en vivo, las geocercas se dibujan solo cuando activa allí la capa de geocercas (mediante la tarjeta **En Geocerca**). Con la capa activada, cada geocerca se dibuja en el color que eligió, y la tarjeta **En Geocerca** le indica cuántas unidades están dentro de una zona en ese momento. Consulte [Mapa en vivo](topic:dashboard-live-map).
