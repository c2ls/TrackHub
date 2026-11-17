---
id: gps-integration
title: Integración GPS
description: Conecte sus proveedores GPS, sincronice la lista de dispositivos, vincule dispositivos a unidades y vigile la salud de la integración — operadores, sincronizaciones, alertas, dispositivos, asignaciones y retención.
category: administration
screens: [gpsIntegration]
related: [units-devices, admin-account-setup, troubleshooting, system-administration]
tags: [gps, operadores, proveedores, sincronización, dispositivos, asignaciones, credenciales, salud]
order: 80
---

# Integración GPS

La página **Integración GPS** reúne todo lo relacionado con el vínculo entre TrackHub y sus proveedores GPS. Arriba hay un panel de salud; debajo, secciones desplegables: **Operadores GPS**, **Sincronizaciones Recientes**, **Alertas GPS Abiertas**, **Dispositivos Sincronizados**, **Asignaciones de Dispositivos** y **Retención de Posiciones**. Haga clic en el encabezado de una sección para desplegarla.

Esta página es para los Gestores. ¿Configura un operador por primera vez? [Configuración de su cuenta](topic:admin-account-setup) lo guía paso a paso.

## El panel de la integración

Tres tarjetas resumen el estado de su integración:

- **Operadores** — **Habilitados / Totales**, más cuántos están **Saludables**, **Degradados** y **Desconectados**.
- **Dispositivos** — el **Total**, desglosado en **Asignado**, **Sin asignar**, **Ignorado** y **Nuevos (24h)**.
- **Sincronización** — **OK / Fallidas (24h)**, **Duración Promedio de Sincronización**, **Última Sincronización Automática** y **Última Sincronización Manual**.

![Tarjetas del panel de Integración GPS y Dispositivos por Proveedor/Estado](assets/es/gps-integration.png)

Debajo de las tarjetas, **Dispositivos por Proveedor/Estado** desglosa los conteos de dispositivos por operador.

**La salud del operador** significa:

- **Saludable** — sincroniza con normalidad.
- **Degradado** — funciona, pero con problemas como sincronizaciones lentas o con fallas parciales.
- **Fuera de línea** — no se puede contactar al proveedor, o las sincronizaciones fallan.
- **Deshabilitado** — el operador fue desactivado.
- **Desconocido** — todavía no hay información de salud.

## Operadores GPS

Un **operador** es su conexión con una cuenta de un proveedor GPS — el proveedor, su dirección y las credenciales que TrackHub usa para traer sus dispositivos y sus posiciones.

### Agregar un operador

Despliegue **Operadores GPS** y haga clic en el ícono **+** para abrir **Detalle del Operador**:

- **Nombre** (obligatorio) y **Descripción**.
- **Número de Teléfono**, **Correo Electrónico**, **Dirección** y **Nombre de Contacto** — su contacto en el proveedor.
- **Intervalo de Sincronización (min)** — cada cuánto se actualiza automáticamente la lista de dispositivos (30 por defecto).
- **Protocolo** (obligatorio) — su proveedor GPS.

Los proveedores que puede elegir son: Cartrack, CommandTrack, Flespi, Geotab, GPS Trackit (Zonar), GpsGate, Mapon, MettaX IoT, Motive, Navixy, Pegasus Gateway, Plaspy, Protrack, RedGPS, Ruptela TrustTrack, Samsara, Traccar, Tracksolid Pro, Webfleet y Wialon.

Algunos proveedores solo permiten una actualización de posición cada cierta cantidad de segundos. Al elegir uno de ellos, el cuadro le indica su límite; si su cuenta almacena posiciones con más frecuencia, se aplica el límite del proveedor.

Presione **Guardar**.

### Ingresar las credenciales

En la fila del operador, haga clic en **Credencial** para abrir el cuadro de credenciales. Solo pide lo que su proveedor necesita, con los nombres propios del proveedor — por ejemplo **Clave de API**, **Base de datos**, **ID de aplicación** o **Token de autorización** en lugar de una clave genérica. La **URL** se completa con la dirección habitual del proveedor cuando existe una; para proveedores con servidores regionales, la ayuda bajo el campo muestra un ejemplo. Complete los campos y presione **Guardar**.

![Diálogo de credenciales de un proveedor GPS con valores de ejemplo](assets/es/gps-credentials.png)

Luego haga clic en el botón de verificación de la columna **Probar Credencial**. Un mensaje de **Prueba de Conectividad** indica **"La credencial fue probada con éxito"** u **"Ocurrió un error al probar la credencial"**. Pruebe antes de habilitar el operador.

### Acciones del operador

La fila del operador muestra su **Protocolo**, **Intervalo de Sincronización (min)**, **Habilitada**, **Estado** (salud), **Última Sincronización Exitosa** y **Modificado**, con estas acciones:

- **Editar** — cambia los datos del operador.
- **Habilitar** / **Deshabilitar** — reanuda o pausa la sincronización automática sin borrar nada. Un operador deshabilitado muestra la salud **Deshabilitado**.
- **Eliminar** — elimina el operador (primero se le pide confirmar).
- **Sincronizar** — actualiza ya la lista de dispositivos desde el proveedor. Al terminar, verá cuántos dispositivos se detectaron, agregaron, actualizaron y eliminaron. Si todavía está en curso, se le indica que consulte el historial de sincronización en unos momentos. **"La sincronización se completó, pero el proveedor no devolvió dispositivos"** suele significar que la credencial llega a una cuenta del proveedor equivocada.
- **Credencial** — abre el cuadro de credenciales.
- **Reiniciar Sincronización** — vuelve a ejecutar desde cero la detección de dispositivos de este operador. Los dispositivos que ya no están en la lista del proveedor se retiran y sus vínculos con unidades terminan, los ignorados se detectan de nuevo, y sus unidades conservan su identidad e historial. Pide confirmación. Úselo para reparar una lista de dispositivos, no como una actualización de rutina.

## Sincronizaciones Recientes

Las sincronizaciones más recientes de sus operadores: **Operador**, **Origen**, **Resultado**, **Inicio**, **Fin**, **Dispositivos (+/~/-)** (agregados / actualizados / eliminados), **Posiciones Aceptadas/Leídas** y **Código de Error**. El **Resultado** es **Exitosa**, **Parcial**, **Fallida**, **En Ejecución** o **Pendiente**. Una ejecución fallida suele traer un **Código de Error**.

## Alertas GPS Abiertas

Las alertas abiertas generadas por la integración GPS, como sincronizaciones fallidas o credenciales del proveedor por expirar: **Tipo de Evento**, **Severidad**, **Estado**, **Módulo de Origen** y **Visto por Última Vez**. Reconózcalas y resuélvalas en **Administración de Cuentas → Eventos de Alerta** — vea [Alertas y notificaciones](topic:alerts-notifications).

## Dispositivos Sincronizados

Los dispositivos encontrados en sus proveedores: **Nombre**, **ID**, **Número de Serie**, **Estado**, **Visto por Primera Vez** y **Visto por Última Vez**. Acote la lista con **Buscar**, los filtros **Estado** y **Operadores**, y los interruptores **Mostrar solo sin asignar** y **Mostrar solo recién agregados**.

**El estado del dispositivo** significa:

- **Sin asignar** — todavía no está vinculado a una unidad.
- **Asignado** — vinculado a una unidad.
- **Ignorado** — apartado deliberadamente.
- **Eliminado** / **Faltante** — el proveedor ya no lo reporta.

Acciones:

- **Ignorar** / **No Ignorar** — aparta un dispositivo, o lo recupera. **Ignorar en Lote (esta página)** y **No Ignorar en Lote (esta página)** se aplican a los dispositivos de la página actual.
- **Eliminar dispositivo** — elimina un dispositivo sincronizado (pregunta antes; su historial de asignación queda en la auditoría).
- **Agregar Dispositivo** — solo aparece cuando tiene un operador cuyo proveedor no puede listar sus dispositivos. Abre **Registrar Dispositivo Manualmente**. Para proveedores que buscan los vehículos por placa, ingrese la placa exacta como nombre. Marque **Crear y asignar una unidad automáticamente** para que se cree y vincule una unidad en el mismo paso.

## Asignaciones de Dispositivos

Un dispositivo solo aparece en el mapa cuando está vinculado a una **unidad**. En esta sección se hace ese vínculo.

![Asignaciones de Dispositivos con los selectores de unidad y dispositivo y la tabla de asignaciones](assets/es/gps-device-assignments.png)

1. En **Unidad que recibirá el dispositivo**, elija la unidad.
2. En **Dispositivo GPS sin asignar**, elija el dispositivo. Solo se ofrecen los dispositivos que no están vinculados actualmente a una unidad.
3. Presione **Asignar Dispositivo**.

La tabla muestra **Unidades**, **Dispositivos**, **Principal**, **Prioridad**, **Estado** (**Activa**, **Finalizada** o **Reemplazada**), **Vigente Desde** y **Vigente Hasta**. Muestra los vínculos activos; cambie a **Mostrar Todo** para incluir los anteriores. **Finalizar Asignación** detiene un vínculo — se le pide un motivo — y la unidad deja de recibir las posiciones de ese dispositivo.

Una unidad vinculada sigue siendo invisible para un Usuario sencillo hasta que esté en uno de sus grupos. Vea [Usuarios, roles y grupos](topic:users-roles-groups#grupos).

## Retención de Posiciones

Cuánto tiempo se conserva el historial de posiciones y cada cuánto se almacenan: **Historial de Posiciones** (Sí/No), **Días de Retención** e **Intervalo de Almacenamiento (Segundos)**. Aquí son de solo lectura; los define el administrador de su plataforma (vea el [Catálogo de funcionalidades](topic:feature-catalog)).
