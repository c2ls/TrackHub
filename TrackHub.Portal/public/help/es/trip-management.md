---
id: trip-management
title: Viajes y planificación de rutas
description: Planifique viajes con varias paradas, ubíquelas desde el mapa, un punto de interés o una geocerca, obtenga la ruta de ORS con su corredor de desvío y la estimación de peajes, asigne conductor, siga el avance y comparta un enlace de seguimiento con su cliente.
category: operation
screens: [tripManager]
featureKey: trip-management
related: [public-trip-tracking, geofences, dashboard-trips-replay, public-links, reports]
tags: [viajes, despacho, paradas, ruta, corredor, peajes, prueba de entrega, seguimiento]
order: 25
---

# Viajes y planificación de rutas

Un **viaje** es el recorrido de un vehículo por una lista ordenada de **paradas**, desde una hora de salida planeada hasta su finalización. TrackHub planifica la ruta, vigila el vehículo contra esa ruta mientras el viaje está en curso, registra llegadas y salidas, y puede entregarle a su cliente un enlace de solo lectura para que siga el avance.

Los viajes solo están disponibles cuando la funcionalidad de gestión de viajes está habilitada en su cuenta. Si no ve **Viajes** en el menú lateral, la funcionalidad está apagada: consúltelo con el administrador de su cuenta y vea el [Catálogo de funcionalidades](topic:feature-catalog).

## Usted planifica; el sistema mide

**Usted no inicia ni finaliza viajes a mano.** Usted indica lo que debe ocurrir —qué unidad, desde dónde, hacia dónde, cuándo— y TrackHub deduce lo que realmente ocurrió a partir de las posiciones del vehículo:

- El viaje **se inicia solo** cuando su unidad llega a la **zona de origen**, en el orden que usted planificó para ese vehículo.
- El **tiempo de cargue** se mide desde que llega al origen hasta que sale.
- Las **llegadas y salidas** de cada parada se miden igual.
- El viaje **se completa solo** cuando termina su ruta.

Nadie tiene que vigilar una pantalla para que esto funcione, y nadie tiene que pulsar **Iniciar** trescientas veces por semana. Su atención va a las **excepciones**: los viajes que no van según lo planeado.

Los botones manuales siguen ahí, agrupados bajo **Intervenir**. Existen para los casos que la medición no cubre: un GPS sin señal, un muelle bajo techo, una corrección posterior. Vea *Intervenir el ciclo automático* más abajo.

## El tablero de despacho

Abra **Viajes** en el menú lateral. El tablero muestra una fila por viaje, con su **código**, **cliente**, **unidad**, **inicio planeado**, **cantidad de paradas**, **fase** y **estado**.

La columna que hay que leer es la **fase**. El estado dice que un viaje está «en curso»; la fase dice qué está *haciendo*: *Cargando en Planta 3*, *En tránsito hacia Cliente X (ETA 11:05)*, *En parada · Cliente Y*, *Sin iniciar a tiempo*. Se calcula en cada actualización a partir de los tiempos realmente medidos, así que nunca es una etiqueta vieja que alguien olvidó cambiar.

El tablero **se actualiza solo cada 30 segundos**, y de nuevo al volver a la pestaña. Tiene que hacerlo: ahora los viajes cambian de estado sin que nadie aquí pulse nada.

Use el filtro **Excepciones** para reducir el tablero a lo que necesita una persona:

| Excepción | Qué significa |
|---|---|
| Sin iniciar a tiempo | La unidad nunca llegó al origen. El viaje sigue en cola: decida si lo cancela o lo replanifica. |
| Con retraso | La estimación actual ya superó el fin planeado. |
| Fuera de corredor | El vehículo se apartó de la ruta planeada. |
| Detenido en la última parada | Llegó a la última parada y la ruta ya no tiene a dónde seguir. |

Los demás filtros —estado, unidad, conductor, desde, hasta— y el buscador de la barra superior los aplica el servidor, de modo que la lista es una página real de viajes coincidentes y no un recorte de los primeros resultados. Use las flechas debajo del tablero para pasar de página; el contador le indica qué filas está viendo.

Haga clic en una fila para abrir ese viaje en el área de trabajo de abajo.

### Un viaje que nunca salió

Un viaje sin iniciar a tiempo **no** se salta, y el siguiente viaje de ese vehículo **no** se adelanta en la cola. Es deliberado: correr el viaje del jueves antes de que alguien note que el del lunes nunca salió es una decisión de despacho, no una suposición del sistema. Cancélelo o replanifíquelo y la cola avanza.

## Estados del viaje

| Estado | Significado |
|---|---|
| Creado | Planeado pero sin iniciar. Las paradas y la ruta aún se pueden editar. |
| En curso | En marcha. Las posiciones se contrastan con las paradas y el corredor. |
| Pausado | Detenido temporalmente. Se puede reanudar, cancelar o abortar. |
| Completado | Finalizado con normalidad. |
| Cancelado | Suspendido. Se conservan paradas, historial y pruebas de entrega. |
| Abortado | Terminado donde estaba, con un motivo. |

Un viaje solo puede seguir las rutas anteriores: no se le ofrecerá un botón para una transición que no está permitida. Completado, cancelado y abortado son estados finales.

## Crear un viaje

Haga clic en **Nuevo viaje** y complete el diálogo:

- **Código del viaje** (obligatorio) — su propia referencia. Debe ser único dentro de la cuenta; repetir un código se rechaza.
- **Cliente** — se muestra al cliente en la página de seguimiento.
- **Unidad** (obligatorio) y **Conductor** — el vehículo que hace el trabajo y quién lo conduce.
- **Origen** (obligatorio) — un **lugar** existente: una de sus geocercas o un punto de interés. No hay casilla de coordenadas, y es a propósito. El origen es contra lo que se mide el inicio automático, así que una geocerca es mejor que un punto: el viaje se evalúa contra la forma real de la planta y no contra un círculo de 150 m alrededor de un pin.
- **Destinos** (al menos uno) y la **actividad** en ellos; vea más abajo.
- **Inicio planeado** (obligatorio) y **fin planeado**.
- **Referencia externa** — el identificador que este viaje tiene en su propio TMS o ERP.
- **Clase de vehículo para peajes** — a qué banda tarifaria pertenece el vehículo; se usa para estimar los peajes.
- **Notas** — de uso interno. Las notas nunca se muestran al cliente.

### Qué hace el vehículo en cada parada

Cada destino lleva una **actividad**: **cargue**, **descargue** u **otra**. Elíjala antes de agregar los destinos: un reparto normalmente es *descargue* en todos.

Esto es lo que le da sentido a una permanencia. Noventa minutos en una parada es un número anónimo; noventa minutos en una parada de *descargue* son noventa minutos de descargue, y eso es lo que suman los informes. El regreso de un viaje redondo queda como **otra**, porque estacionar de vuelta en el depósito no es ninguna de las dos.

El tiempo en el **origen** siempre es tiempo de cargue: para eso está el origen.

## Planificar una semana entera

**Carga masiva** toma una hoja de cálculo y crea los viajes que contiene. Presiónela, descargue la **plantilla** para ver el orden de las columnas, complétela y suba el archivo (o pegue el CSV directamente en el cuadro).

Los lugares se **nombran**, nunca se escriben como coordenadas: ponga `Planta 3` y `Cliente X;Cliente Y`, y TrackHub busca esos nombres primero entre las geocercas de su cuenta y luego entre sus puntos de interés. Un nombre que no encuentre es el error de esa fila.

Una fila mala nunca hace fallar todo el lote. Todas las filas buenas se crean, y las rechazadas vuelven con su número de línea y el motivo, para que corrija esas líneas y vuelva a subir el archivo.

`tripType=round` agrega por usted el regreso al origen. Deje `startedAt` vacío salvo que el viaje ya haya comenzado; vea abajo.

## Un viaje que ya comenzó

La planificación real ocurre después de los hechos: una carga masiva llega el lunes por la mañana para camiones que salieron el domingo por la noche. Abra el viaje y presione **Intervenir → Ya está en ruta**.

TrackHub busca la salida registrada del vehículo desde la zona de origen. Si la encuentra, **se usan esos tiempos medidos**: llegada, salida y las paradas ya visitadas se completan con lo que realmente ocurrió, y la hora que usted escribió se ignora. Solo cuando no hay tal registro se usa su **hora de inicio**, y entonces el tiempo de cargue queda en blanco en vez de inventarse.

En una carga masiva, la columna `startedAt` hace lo mismo para cada fila.

## Agregar paradas

Con un viaje seleccionado, use **Agregar parada** en el panel de paradas. La ubicación de una parada proviene de exactamente tres fuentes:

1. **Hacer clic en el mapa.** Presione **Hacer clic en el mapa** y luego marque el punto. El diálogo vuelve a abrirse con las coordenadas ya cargadas.
2. **Usar un punto de interés.** Elija uno de sus puntos de interés guardados.
3. **Usar una geocerca.** Elija una geocerca de su cuenta. La parada queda vinculada a la zona, de modo que la llegada se evalúa contra la forma real de la zona y no contra un círculo alrededor de un punto; vea [Geocercas](topic:geofences).

Deliberadamente **no hay un buscador de direcciones**. Las tres fuentes ya entregan coordenadas exactas, y TrackHub completa por usted los campos **Dirección** y **Ciudad** consultando el punto elegido. Ambos se pueden editar libremente.

**Dirección y Ciudad son dos niveles de divulgación distintos, no un dato repetido.** La dirección es la referencia completa de la calle y queda reservada a su equipo. La ciudad es la localidad general y es el *único* dato de ubicación que puede mostrar un enlace de seguimiento del cliente: quien tenga el enlace sabrá que una parada está en Bogotá, nunca la puerta exacta donde se entrega. Mantenga la ciudad diligenciada: si queda vacía, el enlace compartido no mostrará ninguna localidad para esa parada. Admite hasta 200 caracteres.

Cada parada además recibe:

- **Nombre** (obligatorio), la **dirección** resultante y la **ciudad**.
- **Radio de llegada** — qué tan cerca debe estar el vehículo para que la llegada se detecte automáticamente. Amplíelo en patios grandes; redúzcalo en entregas sobre la vía. Solo se usa cuando la parada es un punto simple; una parada vinculada a una geocerca usa la forma real de la zona.
- **Actividad** — cargue, descargue u otra. Vea *Qué hace el vehículo en cada parada* más arriba.
- **Planeada desde** / **planeada hasta** — la ventana que usted prometió.
- **Requiere prueba de entrega** — marca la parada como una que debe firmarse.
- **Prioridad** y **observaciones**.

### Cambiar el orden de visita

Arrastre una parada hacia arriba o hacia abajo en la lista, o use las flechas de la fila. El nuevo orden se guarda de inmediato y la numeración del mapa lo sigue. Vuelva a planificar la ruta después, para que la línea de conducción corresponda al nuevo orden.

## Planificar la ruta

Fije el **ancho del corredor** y presione **Planificar ruta**. TrackHub le pide al servicio de rutas la línea de conducción que pasa por sus paradas y recibe:

- la **ruta planeada**, dibujada como una línea azul;
- el **corredor**, una banda del ancho que usted fijó alrededor de esa línea;
- la **distancia** y la **duración planeadas**;
- las estaciones de peaje por las que pasa la ruta.

El corredor es la referencia contra la que se mide un desvío: mientras el viaje está en curso, tres posiciones consecutivas fuera de la banda generan **un** desvío de ruta, y el viaje queda marcado como **Fuera del corredor** hasta que el vehículo regresa.

Si el servicio de rutas no está disponible, el plan vuelve como **fallido** con un motivo. El viaje sigue siendo totalmente utilizable —puede iniciarlo, registrar llegadas y completarlo— y los ETA de las paradas usan su horario planeado en lugar de una estimación en vivo.

## Estimación de peajes

Debajo del planificador, el panel de peajes muestra el costo estimado de la ruta y el detalle estación por estación. Use el selector de **clase de vehículo** para volver a tarifar la misma ruta con otra banda sin modificar el viaje.

Lea con atención la etiqueta de estado, porque cambia el significado de la cifra:

- **Completa** — todas las estaciones de la ruta tienen tarifa para esta clase. La cifra es la estimación total.
- **Parcial** — al menos una estación **no tiene tarifa** para esta clase. Esas estaciones aparecen con la marca *Sin tarifa*, y el costo real es **mayor** que la cifra mostrada. TrackHub informa el faltante en lugar de tratar en silencio una estación sin tarifa como gratuita.
- **Sin estaciones** — ninguna estación del catálogo de la plataforma queda sobre esta ruta. No hay estimación; esto **no** es un costo de cero.
- **Sin calcular** — todavía no se han calculado los peajes para este plan de ruta.

El catálogo de estaciones y tarifas es de toda la plataforma y lo mantiene un superadministrador; vea [Administración del Sistema](topic:system-administration).

### Clases de peaje para su flota

La estimación solo puede calcular una ruta cuando TrackHub sabe a qué banda tarifaria pertenece el vehículo. **Clases de peaje**, en la parte superior del tablero, es donde usted se lo indica:

1. Elija si la regla aplica a un **tipo de vehículo** o a **un vehículo** en particular, como excepción.
2. Seleccione el tipo o el vehículo y luego la **clase de vehículo para peajes**.
3. Guarde. Puede agregar tantas reglas como necesite sin cerrar el diálogo.

Los viajes nuevos toman su clase de peaje de estas reglas automáticamente. Los viajes ya existentes conservan la clase con la que se crearon, y siempre puede cambiar la clase de un viaje puntual desde el diálogo del viaje.

Mientras no exista al menos una regla, los viajes se crean sin clase de peaje y la estimación no tiene con qué calcular, por lo que nunca se activa. Si la lista de clases aparece vacía, el catálogo de peajes de la plataforma todavía no tiene clases de vehículo: un superadministrador debe definirlas primero.

## Asignar un conductor

El panel de asignación muestra quién está asignado actualmente, en qué unidad, cuándo se le asignó y si ya lo confirmó. Elija conductor y unidad y presione **Asignar conductor** para cambiarlo. Solo se ofrecen conductores activos.

## Ejecutar el viaje

No tiene que hacer nada. Cuando falta alrededor de una hora para el inicio planeado, TrackHub empieza a vigilar ese vehículo para su próximo viaje planificado; cuando la unidad llega a la zona de origen el viaje se inicia solo, y desde ahí las posiciones se contrastan con cada parada.

Mientras el viaje está en curso, la fase le dice dónde va, y el área de trabajo se completa sola.

### Intervenir el ciclo automático

Presione **Intervenir** en el encabezado del viaje para acceder a los controles manuales. Existen para las excepciones —GPS débil, muelle techado, equipo apagado, una corrección posterior— y no para la operación diaria:

- **Iniciar** fuerza el arranque del viaje. Se rechaza si ese vehículo ya está corriendo otro viaje: una unidad corre un viaje a la vez.
- **Ya está en ruta** registra un viaje que comenzó antes de que usted lo escribiera. Vea arriba.
- **Pausar** y **Reanudar** cubren las detenciones previstas. **Pausar también suspende la detección automática**: así es como usted toma el control de un viaje. Nada se mide hasta que lo reanude.
- **Completar** cierra el viaje. Si quedan paradas pendientes, marque **Completar aunque queden paradas pendientes** para forzarlo. También funciona sobre un viaje pausado, para no tener que devolverlo a la automatización solo para cerrarlo.
- **Cancelar** y **Abortar** requieren un motivo y ambos conservan todo lo ya registrado.

También puede registrar el avance de las paradas a mano desde la tabla de paradas con **Registrar llegada**, **Registrar salida** y **Omitir**, incluso con el viaje pausado, que es justo cuando los necesita. El registro manual siempre prevalece sobre la detección automática, y registrar dos veces lo mismo nunca crea un duplicado.

La pestaña **Historial** del viaje muestra de dónde vino cada evento, así que un inicio medido y uno escrito a mano siempre se distinguen.

Un viaje con actividad registrada no se puede eliminar: cancélelo en su lugar. Solo se puede eliminar un viaje recién creado y sin historial, y estar siendo vigilado no cuenta como historial.

### Editar un viaje que está siendo vigilado

Mientras el viaje sigue planeado puede cambiar todo, incluidas su unidad y su origen: TrackHub simplemente vuelve a leer las nuevas zonas en su siguiente revisión.

Una vez **en curso**, su unidad y su origen quedan congelados. Todo lo demás —cliente, notas, horas planeadas, clase de peaje— sigue editable. Reapuntar un viaje en marcha cambiaría el significado de mediciones ya tomadas, así que cancélelo y planifique uno nuevo.

### Apagar la automatización

Una cuenta cuyos vehículos no reportan de forma confiable puede apagar por completo el ciclo automático, y entonces cada viaje funciona exactamente del modo manual descrito en **Intervenir**. Es una configuración de la cuenta: consúltelo con su administrador.

## Detalle del viaje

Debajo del planificador está el panorama completo del viaje:

- **Paradas** — la ventana planeada, el ETA con su origen, la llegada y la salida reales y el estado actual de cada parada. La etiqueta del ETA le indica si es una **estimación en vivo** del servicio de rutas o un respaldo tomado de su **horario planeado**, para que sepa cuánto confiar en ella.
- **Entregas** — las remesas registradas en cada parada y el resultado de cada una. Vea *Entregas y prueba de entrega* más abajo para saber cómo registrarlas y cerrarlas.
- **Historial** — todos los eventos del viaje, con la hora y si provinieron del portal, del conductor, de la detección automática o de un proceso en segundo plano.
- **Prueba de entrega** — una tarjeta por captura, con quién recibió, cuándo, dónde, las notas y botones para descargar los adjuntos.
- **Reproducción del recorrido** — presione **Cargar reproducción** para dibujar sobre la línea planeada las posiciones que el vehículo realmente registró. Si el viaje tiene más historial que el límite de reproducción, un aviso se lo indica y le dice cuántos puntos se muestran; use el reporte de historial de posiciones para el recorrido completo en vez de suponer que la línea dibujada lo es todo. Vea [Viajes y reproducción](topic:dashboard-trips-replay).

## Entregas y prueba de entrega

Las entregas y las pruebas de entrega se registran **desde esta pantalla**. No hace falta la aplicación del conductor: un despachador que reciba los datos por teléfono o radio puede mantener completo el registro del viaje por su cuenta.

Ambas cosas siguen disponibles hasta que el viaje se complete, se cancele o se aborte.

### Registrar qué se está entregando

Presione **Entrega** en la fila de una parada para agregarle una remesa. Una entrega lleva un **cliente** (obligatorio), una **sucursal** opcional, una **referencia**, un resumen de **productos**, **observaciones** libres y un número de **orden** que define su posición en la lista.

Una entrega pertenece a la parada en la que se creó y no se puede mover a otra: créela en la parada correcta, o elimínela y vuelva a agregarla. **Editar** cambia sus datos; **Eliminar** borra la línea por completo. Eliminar una entrega no afecta la prueba de entrega ya capturada para esa parada.

### Registrar el resultado

Presione **Resultado** en la fila de una entrega para indicar cómo terminó realmente: **entregada**, **entregada parcialmente**, **rechazada**, o de vuelta a **pendiente**. Agregue observaciones que expliquen cualquier situación fuera de lo común, sobre todo un rechazo.

Si vuelve a guardar después de un error, TrackHub actualiza el mismo registro en lugar de agregar un segundo resultado, así que reintentar tras una caída de conexión siempre es seguro.

### Capturar la prueba de entrega

Presione **Prueba de entrega** en la fila de una parada. Registre:

- **Recibido por** (obligatorio) y, si lo tiene, el **documento** de quien recibe.
- **Capturada** — cuándo ocurrió la entrega. Viene con la hora actual; cámbiela si la está registrando después.
- **Entrega** — déjela vacía para marcar como entregadas **todas** las entregas de la parada. Si indica una entrega, se registra la evidencia de esa y las demás quedan tal como estaban, que es justo lo que necesita cuando una parada se rechaza parcialmente.
- **Latitud / longitud** — opcionales, si sabe dónde se hizo la entrega.
- **Notas**.

Los **adjuntos** —firmas, fotos de la mercancía, recibos sellados— se cargan con **Adjuntar archivos**. Se convierten en documentos normales de TrackHub, archivados bajo el vehículo del viaje, por lo que también aparecen en los documentos de esa unidad y se pueden consultar después; vea [Documentos](topic:documents).

Todo adjunto pasa por un análisis antivirus antes de poder usarse como evidencia. Un archivo muestra **Análisis pendiente** durante un momento después de cargarlo; presione **Volver a verificar** hasta que quede en **Limpio**. TrackHub se negará a guardar la captura mientras algún adjunto no esté limpio, e indica cuántos faltan: quítelos o espere el análisis en lugar de reintentar una y otra vez.

Guardar dos veces nunca crea dos pruebas para la misma captura, así que si el guardado falla puede simplemente volver a presionar guardar.

Las pruebas capturadas aparecen como tarjetas en el detalle del viaje, con botones para descargar cada adjunto.

## Compartir un enlace de seguimiento con el cliente

Presione **Compartir** para crear un enlace de solo lectura que su cliente puede abrir sin tener una cuenta.

1. Indique un **propósito** y, obligatoriamente, una fecha y hora de **vencimiento**.
2. Marque exactamente lo que el cliente puede ver: **detalle de paradas**, **la ruta planeada en el mapa**, **posición en vivo**, **vehículo**, **nombre de pila del conductor** y **si cada parada tiene prueba de entrega**.
3. Guarde. El enlace aparece **una sola vez**.

Todas las casillas empiezan **desmarcadas salvo las que son deliberadamente seguras**, y la **ruta planeada** en particular empieza apagada. Dejarla apagada no es un olvido: el cliente recibe la línea de tiempo de paradas y, si la compartió, el marcador en vivo, sin ninguna línea de ruta dibujada. Márquela cuando realmente quiera que el cliente vea el trayecto previsto. El diálogo le indica cuál de las dos cosas producirá el enlace antes de guardar.

Cópielo de inmediato: el token que contiene se muestra al crearlo y nunca se puede volver a obtener. Si lo pierde, revoque el enlace y cree uno nuevo.

Todo lo que no marque nunca se le envía al cliente. Los costos, los peajes, las notas internas, los datos de contacto del conductor, los archivos de documentos y el historial de posiciones **nunca** se comparten, marque lo que marque. La posición en vivo solo es visible mientras el viaje está efectivamente en curso.

Los enlaces existentes se listan en el mismo diálogo con su estado —activo, vencido o revocado—. **Revocar** deja el enlace inservible de inmediato. Los enlaces de viaje son enlaces públicos comunes y también aparecen en [Enlaces públicos](topic:public-links).

Lo que ve el cliente se describe en [Seguimiento de viajes para clientes](topic:public-trip-tracking).

## Reportes

Hay seis reportes de viajes en la pantalla de [Reportes](topic:reports): resumen de viajes, detalle de paradas, cumplimiento de horarios, permanencia en paradas, costo de peajes y el registro de pruebas de entrega. Respetan la misma visibilidad por grupos que el tablero, de modo que solo verá viajes de unidades de sus grupos.

El **resumen de viajes** trae las duraciones medidas: tiempo en el origen, tiempo en tránsito y tiempo total del viaje, junto a las cifras planeadas con las que deben compararse. El **detalle de paradas** y la **permanencia en paradas** traen la actividad de cada parada, de modo que la misma permanencia se lee como tiempo de cargue en una planta y de descargue en un cliente.

Una duración en blanco significa que la medición detrás de ella nunca se tomó, normalmente un viaje cuyo inicio se escribió en vez de medirse. Es deliberado: un blanco dice «no medido», mientras que un cero afirmaría que el camión no pasó tiempo cargando.

## Si algo no cuadra

- **No aparece Viajes en el menú** — la funcionalidad no está habilitada en su cuenta.
- **Un viaje nunca se inició solo** — la unidad tiene que estar reportando posiciones y tiene que llegar realmente a la zona de origen. Revísela en el [mapa en vivo](topic:dashboard-live-map) y revise el origen: un punto de interés es solo un círculo de 150 m, así que use una geocerca para un patio grande.
- **La fase dice Sin iniciar a tiempo** — la unidad nunca llegó al origen. Cancele el viaje o replanifíquelo; el resto de la cola de ese vehículo está esperando detrás.
- **Iniciar dice que la unidad está ocupada** — ese vehículo ya tiene un viaje en curso. Complételo o cancélelo primero.
- **Un viaje terminó pero no se cerró solo** — probablemente su última parada se omitió a mano, o su GPS se quedó sin señal antes de salir. Una revisión en segundo plano cierra los viajes que quedan en su última parada; si no, ciérrelo con **Intervenir → Completar**.
- **No se puede cambiar la unidad o el origen del viaje** — está en curso. Cancélelo y planifique uno nuevo.
- **Una carga masiva rechazó una fila** — lea el motivo en esa línea. Casi siempre es un nombre de lugar que no coincide con ninguna geocerca ni punto de interés de la cuenta.
- **Planificar ruta no hace nada** — el viaje necesita al menos una parada.
- **"La planificación de rutas no está configurada"** — el servicio de rutas de la plataforma no tiene credenciales. Es una configuración de plataforma, no de la cuenta: comuníquese con su administrador.
- **La estimación dice "Parcial"** — es el catálogo avisándole que no tiene tarifa para todas las estaciones de esa ruta, no un error.
- **No se detectan las llegadas** — revise el radio de llegada de la parada y confirme que la unidad realmente esté reportando posiciones en el [mapa en vivo](topic:dashboard-live-map).
- **La prueba de entrega no se guarda** — algún adjunto no ha terminado el análisis antivirus, o no lo pasó. Presione **Volver a verificar**, o quite el archivo.
- **No aparecen los botones de entrega ni de prueba de entrega** — el viaje ya está completado, cancelado o abortado; el registro de un viaje cerrado no se reescribe.
- **La estimación de peajes nunca aparece en los viajes nuevos** — todavía no hay ninguna regla de clase de peaje. Abra **Clases de peaje** y asocie sus tipos de vehículo.
- **El cliente no ve la ciudad de una parada** — esa parada no tiene ciudad diligenciada. Abra la parada y complétela; la dirección completa deliberadamente nunca se comparte.
- **El cliente no ve la línea de la ruta** — el enlace se creó sin marcar **la ruta planeada en el mapa**. Revóquelo y cree uno nuevo con la casilla marcada.
