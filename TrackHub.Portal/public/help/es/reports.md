---
id: reports
title: Reportes
description: Elija un reporte del catálogo, ajuste sus filtros y véalo en pantalla o expórtelo a Excel o PDF.
category: operation
screens: [reports]
related: [dashboard-trips-replay, units-devices]
tags: [reportes, exportar, excel, pdf, vista previa, filtros]
order: 40
---

# Reportes

Los reportes convierten los datos de su flota —posiciones, actividad de geocercas, salud de la integración GPS, documentos y administración de cuentas— en una tabla que puede ver en pantalla o descargar. Qué reportes ve, y los datos que contienen, dependen de las funciones de su cuenta, su rol y sus grupos, por lo que es posible que vea menos que el catálogo completo. Consulte [Roles y permisos](topic:roles-and-permissions).

## Abrir Reportes

Abra **Reportes** desde el menú de la izquierda. La pantalla tiene el catálogo de reportes a la izquierda y, una vez que elige un reporte, sus filtros (y cualquier vista previa) a la derecha.

## Elegir un reporte

El catálogo agrupa los reportes en categorías plegables — **Operaciones**, **GPS**, **Documentos** y **Administración**. La primera categoría aparece expandida; haga clic en el encabezado de una categoría para expandirla o contraerla.

Cada reporte es una tarjeta que muestra su nombre, una descripción de una línea y distintivos de formato: todos los reportes ofrecen **XLSX** (Excel), y los reportes que además admiten PDF muestran un distintivo **PDF**. Haga clic en la tarjeta de un reporte para seleccionarlo; su panel de filtros se abre a la derecha. Nada queda seleccionado hasta que hace clic en un reporte.

## El catálogo de reportes

Los reportes que TrackHub puede ofrecer incluyen los siguientes. La lista exacta depende de su cuenta y sus permisos.

### Operaciones

- **Reporte de Unidades** – última posición conocida de cada unidad de su cuenta.
- **Reporte de Posiciones** – registro histórico de posiciones de una unidad en un rango de fechas.
- **Unidades en Geocerca** – unidades ubicadas actualmente dentro de una geocerca.
- **Eventos de Geocerca** – eventos de entrada y salida de geocerca de una unidad en un rango de fechas.

### GPS

- **Resumen de Estado de Proveedores GPS** – resumen de disponibilidad, latencia y fallas por proveedor GPS.
- **Historial de Sincronización de Proveedores GPS** – historial de sincronización por ejecución de los proveedores GPS.
- **Estadísticas de Sincronización GPS** – estadísticas diarias de sincronización agrupadas por proveedor.
- **Inventario de Dispositivos GPS Sincronizados** – inventario completo de dispositivos GPS sincronizados y su asignación.
- **Dispositivos GPS Recién Agregados** – dispositivos GPS detectados por primera vez dentro del período seleccionado.
- **Dispositivos GPS sin Asignar** – dispositivos GPS sincronizados aún no asignados a una unidad.
- **Dispositivos GPS Ignorados** – dispositivos GPS sincronizados marcados como ignorados.
- **Historial de Asignaciones GPS** – historial de asignaciones de dispositivos a unidades en un rango de fechas.
- **Vigencia de la Última Posición GPS** – qué tan reciente es la última posición reportada por cada dispositivo.
- **Historial de Posiciones GPS** – historial detallado de posiciones de una unidad o dispositivo.

### Documentos

- **Documentos por Vencer** – documentos activos que vencen dentro de la cantidad de días seleccionada.
- **Documentos Requeridos Faltantes** – unidades a las que les falta uno o más tipos de documento requeridos.
- **Actividad de Documentos Compartidos** – actividad de enlaces públicos y conteo de accesos de documentos.
- **Volumen de Carga de Documentos** – conteo de carga de documentos por categoría en un rango de fechas.

### Administración

- **Cuentas por Estado** – cuentas agrupadas por estado de ciclo de vida.
- **Matriz de Habilitación de Funcionalidades** – funcionalidades habilitadas y niveles en todas las cuentas.
- **Exportación de Membresía de Grupos** – membresía de grupos de usuarios y unidades de su cuenta.

## Completar los filtros

El panel de filtros solo muestra los campos que un reporte realmente necesita; un reporte que no necesita nada muestra solo los botones de acción. Los campos posibles son:

- **Unidad** – restringe el reporte a una sola unidad (un selector).
- **Operador** – restringe el reporte a un operador GPS (un selector).
- **ID de Dispositivo** – un identificador de dispositivo, ingresado como texto libre.
- **Estado** – un estado de ciclo de vida de cuenta (usado por el reporte Cuentas por Estado).
- **Desde** / **Hasta** – la fecha y hora de inicio y de fin del período del reporte.
- **Máximo de Filas** – un tope de cuántas filas devolver.
- **Dentro de (días)** – una ventana móvil en días (para reportes de "recientes" o "por vencer").
- **Horas hacia Atrás** – cuántas horas hacia atrás resumir.

Los reportes sin un conjunto de filtros definido usan por defecto un rango de fechas **Desde** / **Hasta**.

## Vista Previa, Excel y PDF

El panel de filtros tiene hasta tres botones de acción:

- **Vista Previa** – ejecuta el reporte y muestra los resultados en una tabla a la derecha, con un conteo de **Total de filas**. Si la vista previa está limitada, un aviso le indica que está limitada a las primeras filas mostradas — refine sus filtros o exporte a Excel para obtener el conjunto completo.
- **Exportar Excel** – descarga el reporte como un archivo de Excel (`.xlsx`) nombrado según el reporte.
- **Exportar PDF** – descarga un PDF con formato. Este botón aparece solo para los reportes que admiten PDF (los que tienen un distintivo **PDF** en el catálogo).

Abra los archivos de Excel en Excel, Google Sheets o cualquier aplicación de hojas de cálculo. Si una exportación fuera muy grande, ajuste el rango de fechas o la selección de unidad y ejecútela de nuevo — las exportaciones están sujetas a un límite de filas del servidor según su plan.
