---
id: platform-status
title: Estado de la Plataforma
description: Verifique si TrackHub está funcionando — indicadores de servicio, qué hacer cuando algo está en rojo y anuncios de la plataforma.
category: administration
screens: [platformStatus]
related: [troubleshooting, system-administration, admin-platform-setup]
tags: [estado, salud, caída, mantenimiento, anuncios, solución de problemas]
order: 75
---

# Estado de la Plataforma

La página **Estado de la Plataforma** responde una sola pregunta en lenguaje sencillo: *¿TrackHub está funcionando, o el problema es mío?*

## Funciona incluso cuando no puede iniciar sesión

Esto es lo importante. La página de estado es **pública**: se abre sin iniciar sesión y sigue funcionando aunque el propio servicio de inicio de sesión esté caído.

**Guárdela en favoritos ahora**, mientras todo funciona. Su dirección es la dirección habitual de TrackHub seguida de `/status`, por ejemplo:

```
https://direccion-de-su-trackhub/status
```

Si nadie de su equipo puede ingresar, abra ese favorito. Si el indicador **Inicio de sesión** está en rojo, el problema es de la plataforma y no de su contraseña — comuníquese con su proveedor en lugar de restablecer credenciales.

## Los indicadores

Cada indicador corresponde a una parte de la plataforma, nombrada por lo que hace y no por su nombre técnico:

| Indicador | Qué cubre |
|---|---|
| **Inicio de sesión** | El ingreso a TrackHub. |
| **Permisos** | Qué puede ver y hacer cada persona. |
| **Gestión de flota** | Vehículos, conductores, grupos, documentos y configuración de la cuenta. |
| **Conexiones GPS** | La comunicación con sus proveedores GPS. |
| **Datos de posición** | Las posiciones de los vehículos y su historial. |
| **Geocercas** | Entradas, salidas y permanencias dentro de sus zonas. |
| **Informes** | La generación de los informes que descarga o previsualiza. |

Cada indicador muestra uno de tres estados:

- **Funcionando** (verde) — el servicio respondió con normalidad.
- **No funciona** (rojo) — el servicio no respondió o informó un problema. Debajo aparece una línea breve que indica cuál, por ejemplo *«El servicio responde, pero su base de datos no.»*
- **Desconocido** (gris) — el servicio aún no se ha verificado o no está configurado en esta instalación.

El aviso superior resume todo: **Todos los sistemas funcionan correctamente** o **Algunos sistemas presentan problemas**.

La página se verifica sola cada minuto e indica cuándo fue la última verificación. Use **Actualizar** para verificar de inmediato. La verificación se detiene mientras la pestaña está en segundo plano y se reanuda al volver a ella.

## Qué hacer cuando algo está en rojo

1. **Actualice una vez.** Una verificación fallida aislada puede ser un problema pasajero de red.
2. **Observe cuál indicador está en rojo.** Le dice qué funcionará y qué no:
   - *Inicio de sesión* — nadie puede ingresar. Las sesiones ya abiertas pueden seguir funcionando un tiempo.
   - *Datos de posición* o *Conexiones GPS* — el mapa y las posiciones quedan desactualizados; su información histórica no se pierde.
   - *Informes* — los informes fallan, pero todo lo demás funciona.
   - *Gestión de flota* — la mayoría de las pantallas de administración no cargarán.
3. **Revise si hay un anuncio** en la parte superior de la página. El mantenimiento programado suele publicarse allí, lo que significa que la interrupción es esperada y tiene una hora de finalización.
4. **Si no hay anuncio y el indicador sigue en rojo**, repórtelo a su proveedor indicando cuál indicador está en rojo y desde cuándo — eso es exactamente lo que necesitan.

Una cosa que la página no puede informarle: si el sitio completo no carga, ninguna página alojada en ese sitio puede reportarlo. En ese caso la interrupción es más amplia que cualquier servicio individual.

## Anuncios

Los administradores de la plataforma pueden publicar anuncios: *«Estamos en una ventana de mantenimiento»*, *«El próximo fin de semana habrá mantenimiento»*, *«Estamos investigando un problema»*.

Los anuncios aparecen en dos lugares:

- En esta página de estado, **incluso para quienes no han iniciado sesión**.
- Como una barra de color en la parte superior de todas las pantallas del portal, para todos los usuarios que han iniciado sesión.

El color sigue la importancia: azul para información, ámbar para advertencia y rojo para crítico. Puede cerrar la barra del portal con su **×**; permanecerá cerrada hasta que cierre el navegador, y un anuncio que siga activo volverá a aparecer la próxima vez que inicie sesión.

Si falta la barra de anuncios mientras el indicador *Gestión de flota* está en rojo, es lo esperado: los anuncios se almacenan allí, por lo que una interrupción los oculta. Los indicadores siguen funcionando de todos modos.

## Para gerentes y administradores

**Sincronización GPS** — los **gerentes y administradores de la plataforma** que han iniciado sesión ven un indicador adicional para el sincronizador en segundo plano. Es el complemento de la pantalla Integración GPS: esa pantalla muestra los operadores de su cuenta, este indicador muestra si el sincronizador está funcionando. No tiene un punto de verificación propio, por lo que su estado se deduce de qué tan recientemente trabajó: verde cuando registró actividad en los últimos cinco minutos. Si ninguna cuenta tiene la integración GPS habilitada, muestra **Desconocido** con *«no hay nada que sincronizar»* — eso es normal, no una falla.

**Tareas en segundo plano** (solo administradores de la plataforma) — el trabajo programado que se ejecuta solo: evaluación de alertas, envío y resúmenes de notificaciones, análisis y vencimiento de documentos, vencimiento de pruebas, detección de permanencia en geocercas y tareas de limpieza. De cada una verá su estado, cuándo hizo algo por última vez y el resultado. Pase el cursor sobre un resultado fallido para ver el código de error.

Lea esta tabla con atención, porque la mayoría de estas tareas **solo registran actividad cuando realmente tuvieron trabajo que hacer**. Una tarea de resumen sin nada que resumir no escribe nada. Por eso *«Inactiva»* y una marca de tiempo antigua son normales y saludables — la página deliberadamente no las considera detenidas. Solo las tareas verificadas que registran en cada ciclo pueden marcarse como **Sin actividad reciente**, y hoy la evaluación de alertas es la única.

**Gestionar anuncios** — abre el editor de anuncios. Solo los administradores de la plataforma ven este botón y solo ellos pueden crear o modificar anuncios; los gerentes de cuenta no pueden.

Para cada anuncio usted indica:

- **Mensaje (inglés)** — obligatorio, hasta 500 caracteres, texto plano. Los signos de formato se muestran literalmente, no se interpretan.
- **Mensaje (español)** — opcional. Los usuarios en español lo verán cuando exista, y el mensaje en inglés cuando no.
- **Severidad** — Información, Advertencia o Crítico, lo que define el color.
- **Inicia el** / **Termina el** — opcionales. Deje el inicio vacío para publicar de inmediato, y el fin vacío para mantenerlo visible hasta que lo desactive. Un anuncio desaparece solo al llegar la hora de fin.
- **Activo** — desmárquelo para guardar un borrador o para retirar un anuncio sin eliminarlo. Los anuncios inactivos son invisibles en todas partes.

Publique el anuncio de mantenimiento **antes** de que comience el mantenimiento. Una vez detenido el servicio de gestión de flota, los anuncios ya no pueden cargarse — por lo que un aviso programado con antelación es lo que la gente verá durante la ventana.
