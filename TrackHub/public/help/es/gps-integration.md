---
id: gps-integration
title: Integración GPS
description: Conecte sus proveedores GPS, sincronice la lista de dispositivos y vigile la salud — operadores, sincronizaciones, alertas, dispositivos y retención.
category: administration
screens: [gpsIntegration]
related: [units-devices, system-administration, troubleshooting]
tags: [gps, operadores, sincronización, dispositivos, credenciales, salud]
order: 80
---

# Integración GPS

La página **Integración GPS** reúne todo lo relacionado con el vínculo entre TrackHub y sus proveedores GPS. En la parte superior hay un panel de salud; debajo hay secciones plegables para operadores, sincronizaciones, alertas, dispositivos, asignaciones y retención. Haga clic en el encabezado de una sección para expandirla; la sección de operadores tiene un ícono **Agregar** (**+**).

Esta página es para gestores. Para que la integración obtenga datos, la función de integración GPS debe estar habilitada para su cuenta (se configura en [Administración del Sistema](topic:system-administration)). Consulte también [Solución de problemas](topic:troubleshooting).

## El panel de integración

El panel superior resume el estado de su integración en tres tarjetas:

- **Operadores** — Habilitados / Totales, más cuántos están **Saludables**, **Degradados** y **Desconectados**.
- **Dispositivos** — el total, desglosado en **Asignados**, **Sin asignar**, **Ignorados** y **Nuevos (24h)**.
- **Sincronización** — **OK / Fallidas (24h)**, más **Duración Promedio de Sincronización**, **Última Sincronización Automática** y **Última Sincronización Manual**.

Debajo de las tarjetas, **Dispositivos por Proveedor/Estado** ofrece un desglose por operador.

**La salud del operador** (mostrada aquí y en cada fila de operador) significa:

- **Saludable** — sincronizando con normalidad.
- **Degradado** — funcionando pero con problemas (por ejemplo, sincronizaciones lentas o que fallan parcialmente).
- **Fuera de línea** — no alcanzable / las sincronizaciones fallan.
- **Deshabilitado** — el operador fue apagado.
- **Desconocido** — aún no hay información de salud.

## Operadores GPS

Un **operador** es una conexión configurada a un proveedor GPS — la cuenta, el servidor y las credenciales que TrackHub usa para extraer dispositivos y posiciones. Cada operador habla un **Protocolo**.

Haga clic en el ícono **+** para abrir el diálogo **Detalle del Operador**:

- **Nombre** (obligatorio) y **Descripción**.
- **Teléfono**, **Correo Electrónico**, **Dirección**, **Nombre de Contacto**.
- **Intervalo de Sincronización (min)** — con qué frecuencia se sincroniza automáticamente (predeterminado 30).
- **Protocolo** (obligatorio).

Presione **Guardar**. La fila del operador muestra su protocolo, intervalo de sincronización, estado habilitado, salud, última sincronización exitosa y fecha de modificación, con estas acciones:

- **Editar** — cambiar los datos del operador.
- **Habilitar** / **Deshabilitar** — pausar o reanudar la sincronización automática sin eliminar la configuración. La salud de un operador deshabilitado se muestra como **Deshabilitado**.
- **Eliminar** — quitar el operador (confirma primero).
- **Probar Credencial** (la acción de verificación) — comprueba la conexión con las credenciales almacenadas. Un diálogo informa "La credencial fue probada con éxito" o "Ocurrió un error al probar la credencial".
- **Sincronizar** — extrae la lista de dispositivos del proveedor de inmediato. Si no se completa, un diálogo muestra "La sincronización no se completó. Verifique la conectividad del proveedor e intente nuevamente."
- **Credencial** — abre el diálogo de credenciales (vea abajo).
- **Reiniciar Sincronización** — una opción más fuerte que advierte primero: elimina los dispositivos sincronizados de ese operador y las unidades que solo existían para esos dispositivos, luego inicia una sincronización nueva. Úsela solo para reconstruir desde cero la lista de dispositivos de un operador.

### Credenciales

En la fila del operador, haga clic en **Credencial** (la acción de la llave) para abrir el diálogo, que recopila:

- **URL** (obligatoria) — el endpoint del proveedor.
- **Nombre de Usuario** y **Contraseña**.
- **Clave Secreta** y **Clave Secreta 2** — usadas por proveedores que autentican con claves.

Complete los campos que su proveedor requiere y **Guarde**, luego use **Probar Credencial** para confirmar la conexión.

## Sincronizaciones Recientes

Un historial de las sincronizaciones más recientes de sus operadores. Las columnas incluyen **Operador**, **Origen**, **Resultado**, **Inicio**, **Fin**, **Dispositivos (+/~/-)** (agregados / actualizados / eliminados), **Posiciones Aceptadas/Leídas** y **Código de Error**. Los valores de **Resultado** son **Exitosa**, **Parcial**, **Fallida**, **En Ejecución** y **Pendiente**. Una ejecución fallida suele llevar un **Código de Error** para ayudar a diagnosticar la causa.

## Alertas GPS Abiertas

Lista las alertas abiertas generadas por la integración GPS. Las columnas incluyen **Tipo de Evento**, **Severidad**, **Estado**, **Módulo de Origen** y la fecha de última aparición. La **Severidad** va desde **Crítica** pasando por **Alta** y **Media** hasta **Baja**. Use esta sección para detectar problemas de integración que requieren atención.

## Dispositivos Sincronizados

Los dispositivos descubiertos desde sus proveedores. Las columnas son **Nombre**, **ID**, **Número de Serie**, **Estado**, **Visto por Primera Vez** y **Visto por Última Vez**, con acciones. Filtre la lista con el cuadro **Buscar**, los menús **Estado** y **Operadores**, y los interruptores **Mostrar solo sin asignar** y **Mostrar solo recién agregados**.

**El estado del dispositivo** significa:

- **Sin asignar** — aún no vinculado a una unidad.
- **Asignado** — vinculado a una unidad.
- **Ignorado** — oculto deliberadamente de las vistas normales.
- **Eliminado** / **Faltante** — ya no reportado por el proveedor.

Acciones de fila y en lote:

- **Ignorar** / **No Ignorar** — oculta o muestra un dispositivo. **Ignorar en Lote** / **No Ignorar en Lote** se aplican a todos los dispositivos mostrados actualmente por sus filtros.
- **Eliminar** — quita un dispositivo sincronizado (advierte primero; el historial de asignación queda en los registros de auditoría).

## Asignaciones de Dispositivos

Vínculos entre dispositivos GPS y sus unidades. En resumen: elija una unidad y un dispositivo sin asignar y presione **Asignar Dispositivo**; use **Finalizar Asignación** para terminar un vínculo activo. Esto se cubre por completo — incluyendo qué significan **Principal** y **Prioridad** — en [Unidades y dispositivos](topic:units-devices).

## Retención de Posiciones

Muestra cuánto tiempo se conserva el historial de posiciones y con qué frecuencia se almacenan. Estos valores provienen de su suscripción y son de **solo lectura** para los gestores. La nota dice: "Estas configuraciones de almacenamiento dependen de su plan de suscripción y son gestionadas por el administrador."

Los campos mostrados son **Historial de Posiciones** (habilitado, Sí/No), **Días de Retención** e **Intervalo de Almacenamiento (Segundos)**. Para cambiarlos, el equipo de la plataforma ajusta las funciones GPS de su cuenta en [Administración del Sistema](topic:system-administration).
