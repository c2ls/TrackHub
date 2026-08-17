---
id: documents
title: Documentos
description: Almacene, clasifique, analice, versione y comparta los documentos de su cuenta desde la pantalla de Administración de Cuentas.
category: administration
screens: [manageAdmin]
related: [management-overview, units-devices]
tags: [documentos, archivos, subir, versiones, vencimiento, análisis de virus]
order: 40
---

# Documentos

El área de **Documentos** de la pantalla **Administración de Cuentas** le permite almacenar archivos, mantenerlos clasificados y versionados, vigilar los vencimientos y compartirlos de forma segura. La encuentra en el grupo **Documentos y Compartidos** de la página.

El área tiene tres secciones plegables — **Biblioteca de Documentos**, **Documentos por Vencer** y **Tipos de Documento** — además de un panel de documentos incorporado que aparece en el detalle del elemento al que pertenece un documento (por ejemplo, una unidad o un conductor).

## Biblioteca de Documentos

Una búsqueda de los documentos de toda la cuenta.

- Ingrese un **Tipo** (categoría) y/o un **Estado** en los dos campos de filtro, luego presione el botón de búsqueda (lupa).
- Tabla de resultados:

| Columna | Significado |
|---|---|
| **Nombre de Archivo** | El título del documento, o el nombre de archivo original. |
| **Propietario** | El registro al que pertenece el documento (tipo de entidad e id). |
| **Tipo** | La categoría del documento. |
| **Clasificación** | Público, Interno, Confidencial o Legal. |
| **Estado** | Activo, Pendiente, Cargado, Expirado, Reemplazado, Anulado o Eliminado. |
| **Estado de Análisis** | El resultado del análisis de virus (ver abajo). |
| **Acción** | Un botón **Descargar**, visible solo cuando el archivo está disponible. |

## Documentos por Vencer

Un panel de documentos próximos a vencer. Columnas: **Tipo**, **Propietario**, **Vence En** y **Estado**. Úselo para detectar licencias, permisos y certificados antes de que caduquen.

## Tipos de Documento

Configure las categorías que usa su cuenta.

- Columnas: **Tipo**, **Obligatorio**, **Controla Vencimiento**, **Validez Predeterminada (días)**, **Estado** y una acción.
- El icono **+** (agregar) abre el diálogo **Tipos de Documento**, donde define:
  - **Tipo** (el identificador de categoría, obligatorio)
  - **Título** (el nombre para mostrar)
  - **Validez Predeterminada (días)**
  - **Obligatorio** — marque si todo registro relevante debe tener este tipo
  - **Controla Vencimiento** — marque si los documentos de este tipo tienen fecha de vencimiento
- Presione **Guardar**.
- La acción de bloqueo en una fila deshabilita ese tipo (se le pide confirmar primero).

> El icono **+** en **Tipos de Documento** aparece solo cuando la funcionalidad de documentos está habilitada para su cuenta. Si falta, la funcionalidad no está provisionada — el equipo de la plataforma la gestiona en [Administración del sistema](topic:system-administration).

## Subir, versiones y compartir

Los documentos normalmente se agregan desde el panel de detalle del registro al que pertenecen, usando un panel de documentos incorporado. Ese panel lista los documentos del registro con las columnas **Nombre de Archivo**, **Tipo**, **Clasificación**, **Estado**, **Estado de Análisis**, **Vence En** y **Versión**, y ofrece estas acciones:

- **Subir Documento** — presione el botón, luego arrastre un archivo al área de soltar (**"Arrastre y suelte un archivo aquí, o haga clic para seleccionar"**) o haga clic para explorar. Elija un **Tipo** (categoría, obligatorio), una **Clasificación** (Público, Interno, Confidencial o Legal — Interno por defecto), un **Título** opcional y una fecha **Vence En** opcional. Presione **Guardar**.
- **Nueva Versión** — suba un archivo de reemplazo para un documento existente. Puede agregar un **Motivo**; la columna **Versión** lleva entonces el número de versión actual.
- **Descargar** — descarga el archivo actual (solo cuando el análisis pasó).
- **Compartir** — cree un enlace público para compartir el documento. Defina un **Propósito** y un **Vence En**, luego presione **Guardar**. La URL de compartir se muestra **una sola vez**, con la advertencia **"Copie este enlace ahora. El token no se mostrará de nuevo."** — cópiela de inmediato. Los enlaces compartidos usan el mismo mecanismo descrito en [Enlaces públicos](topic:public-links).
- **Anular** — marca el documento como anulado.
- **Quitar Referencia** — quita el documento de ese registro.

## Qué significa el Estado de Análisis

Cada documento subido se analiza en busca de virus. El estado le indica si es seguro usarlo:

- **Limpio** — el archivo pasó y puede descargarse.
- **Pendiente** o **En Cuarentena** — aún se está verificando ("Analizando — disponible cuando esté limpio"); todavía no se puede descargar.
- **Infectado** o **Fallido** — el archivo no pasó ("Bloqueado: análisis de virus fallido") y está bloqueado para descarga.

## Quién puede usar esto

Los documentos residen dentro de **Administración de Cuentas**, disponible para los Gestores. Las acciones de subir/versionar/compartir/anular/quitar del panel incorporado aparecen solo cuando usted tiene permisos de gestión sobre el registro propietario, de modo que un observador de solo lectura ve la lista y descarga, pero no las acciones de edición. Vea [Vista general de administración](topic:management-overview).
