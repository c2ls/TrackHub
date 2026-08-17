---
id: units-devices
title: Units and devices
description: Manage the units you track and the GPS devices that report for them, both from the Fleet & Tracking group of Account Management.
category: administration
screens: [manageAdmin]
related: [gps-integration, management-overview, glossary]
tags: [units, transporters, devices, fleet, gps]
order: 30
---

# Units and devices

The **Units** and **Devices** sections both sit under the **Fleet & Tracking** group of the [Account Management](topic:management-overview) screen, which only **Managers** can open. Expand a section to work with it.

A **unit** is the thing you track — a vehicle, an asset, a person, or any object. A **device** is the physical GPS hardware that reports positions. A device is linked to a unit so the unit shows up on the map; that linking is done on the GPS Integration page (see below).

## Units

The app labels tracked things **Units** (some screens call them transporters).

Columns: **Units** (name), **Type** (shown as a coloured label), and **Action**.

Row actions:

- **Edit** opens the **Unit Details** dialog, where you can change the **Name** (required) and the **Type** (required — chosen from the unit type list: Car, Truck, Motorcycle, Asset, and so on). Press **Save**.
- **Delete** removes the unit. You are asked to confirm ("Are you sure you want to delete this unit?").

This section has no **+** (add) button — you edit or remove existing units here. To connect a unit to a tracking device, use the GPS Integration page; see [GPS integration](topic:gps-integration).

## Devices

The **Devices** section lists the raw tracking devices known to your account.

Columns: **Name** (with the device's ID shown beneath), **Serial Number**, **Description**, **Type** (shown as a coloured label), and **Action**.

- The only row action is **Delete**. Deleting asks you to confirm ("Are you sure you want to delete this device?").
- There is no "add device" button here. Devices normally appear on their own when they are synchronised from your GPS provider. See [GPS integration](topic:gps-integration).

## Connecting a device to a unit

Linking a GPS device to a unit is done on the **GPS Integration** page, not here — it is the step that makes a device's positions show up under a unit on the map. See [GPS integration](topic:gps-integration) for how to assign a device, mark it primary, set its priority, and end an assignment.
