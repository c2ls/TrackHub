---
id: units-devices
title: Unidades y dispositivos
description: Administre las unidades que rastrea y los dispositivos GPS que reportan por ellas, ambos desde el grupo Flota y Rastreo de Administración de Cuentas.
category: administration
screens: [manageAdmin]
related: [gps-integration, management-overview, glossary]
tags: [unidades, transportadores, dispositivos, flota, gps]
order: 30
---

# Unidades y dispositivos

Las secciones **Unidades** y **Dispositivos** se encuentran bajo el grupo **Flota y Rastreo** de la pantalla [Administración de Cuentas](topic:management-overview), que solo los **Gestores** pueden abrir. Expanda una sección para trabajar con ella.

Una **unidad** es lo que usted rastrea — un vehículo, un activo, una persona o cualquier objeto. Un **dispositivo** es el hardware GPS físico que reporta posiciones. Un dispositivo se vincula a una unidad para que la unidad aparezca en el mapa; esa vinculación se realiza en la página de Integración GPS (vea más abajo).

## Unidades

La aplicación llama **Unidades** a los elementos rastreados (algunas pantallas las llaman transportadores).

Columnas: **Unidades** (nombre), **Tipo** (mostrado como una etiqueta de color) y **Acción**.

Acciones de fila:

- **Editar** abre el diálogo **Detalle de la Unidad**, donde puede cambiar el **Nombre** (obligatorio) y el **Tipo** (obligatorio — elegido de la lista de tipos de unidad: Automóvil, Camión, Motocicleta, Activo, etc.). Presione **Guardar**.
- **Eliminar** quita la unidad. Se le pide confirmar ("¿Está seguro de que desea eliminar esta unidad?").

Esta sección no tiene un botón **+** (agregar) — aquí edita o quita unidades existentes. Para conectar una unidad a un dispositivo de rastreo, use la página de Integración GPS; vea [Integración GPS](topic:gps-integration).

## Dispositivos

La sección **Dispositivos** lista los dispositivos de rastreo sin procesar conocidos por su cuenta.

Columnas: **Nombre** (con el ID del dispositivo mostrado debajo), **Número de Serie**, **Descripción**, **Tipo** (mostrado como una etiqueta de color) y **Acción**.

- La única acción de fila es **Eliminar**. Al eliminar se le pide confirmar ("¿Está seguro de que desea eliminar este dispositivo?").
- Aquí no hay un botón de "agregar dispositivo". Los dispositivos normalmente aparecen por sí solos cuando se sincronizan desde su proveedor GPS. Vea [Integración GPS](topic:gps-integration).

## Conectar un dispositivo a una unidad

La vinculación de un dispositivo GPS a una unidad se realiza en la página **Integración GPS**, no aquí — es el paso que hace que las posiciones de un dispositivo aparezcan bajo una unidad en el mapa. Vea [Integración GPS](topic:gps-integration) para saber cómo asignar un dispositivo, marcarlo como principal, definir su prioridad y finalizar una asignación.
