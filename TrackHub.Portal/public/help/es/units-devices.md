---
id: units-devices
title: Unidades y dispositivos
description: Agregue, edite, retire y restaure las unidades que rastrea, y vea los dispositivos GPS que reportan por ellas, desde el grupo Flota y Rastreo de Administración de Cuentas.
category: administration
screens: [manageAdmin]
related: [gps-integration, management-overview, users-roles-groups, glossary]
tags: [unidades, dispositivos, flota, retirar, gps]
order: 30
---

# Unidades y dispositivos

Las secciones **Unidades** y **Dispositivos** se encuentran bajo el grupo **Flota y Rastreo** de la pantalla [Administración de Cuentas](topic:management-overview), que solo los **Gestores** pueden abrir.

Una **unidad** es lo que usted rastrea — un vehículo, un bien, una persona o cualquier objeto. Un **dispositivo** es el hardware GPS que reporta posiciones. Un dispositivo se vincula a una unidad para que la unidad aparezca en el mapa; ese vínculo se hace en la página **Integración GPS**.

## Unidades

Columnas: **Nombre**, **Tipo** y **Acción**. Use el cuadro de búsqueda para encontrar una unidad por su nombre.

- **Agregar** abre el cuadro **Detalle de la Unidad**. Ingrese un **Nombre** y elija un **Tipo** (Automóvil, Camión, Motocicleta, Bien, etc.), y presione **Guardar**. El tipo decide el ícono y las reglas con que se determina cuándo la unidad cuenta como detenida.
- **Editar** vuelve a abrir el mismo cuadro.
- **Retirar** saca una unidad de uso. Deja de rastrearse y sale de todas las listas, pero su historial se conserva.

Para ver las unidades retiradas, active **Mostrar unidades retiradas**. Cada unidad retirada ofrece entonces **Restaurar**, que la devuelve al uso.

Para un miembro del personal que quiera seguir en el mapa — un representante de ventas, un técnico — registre el dispositivo que lleva como una unidad con un nombre que diga quién es, por ejemplo *Representante de Ventas Norte*. Vea [Personal y fuerza de trabajo](topic:staff-workforce).

## Dispositivos

La sección **Dispositivos** lista los dispositivos GPS conocidos por su cuenta. Columnas: **Nombre** (con el **ID** del dispositivo debajo), **Número de Serie**, **Descripción**, **Tipo** y **Acción**.

Los dispositivos no se agregan aquí: llegan solos cuando se sincroniza su proveedor GPS. La única acción es **Eliminar**, que pide confirmación.

## Hacer que una unidad aparezca en el mapa

Una unidad aparece en el mapa cuando se cumplen dos condiciones:

1. **Tiene un dispositivo vinculado.** Vincule dispositivos en **Integración GPS → Asignaciones de Dispositivos** — vea [Integración GPS](topic:gps-integration#asignaciones-de-dispositivos).
2. **Está en un grupo al que pertenece quien la mira.** Los Usuarios sencillos solo ven las unidades de sus grupos — vea [Usuarios, roles y grupos](topic:users-roles-groups#grupos). Los Gestores ven todas las unidades.
