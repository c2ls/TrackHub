---
id: units-devices
title: Units and devices
description: Add, edit, retire and restore the units you track, and see the GPS devices that report for them, from the Fleet & Tracking group of Account Management.
category: administration
screens: [manageAdmin]
related: [gps-integration, management-overview, users-roles-groups, glossary]
tags: [units, devices, fleet, retire, gps]
order: 30
---

# Units and devices

The **Units** and **Devices** sections sit under the **Fleet & Tracking** group of the [Account Management](topic:management-overview) screen, which only **Managers** can open.

A **unit** is the thing you track — a vehicle, an asset, a person, or any object. A **device** is the GPS hardware that reports positions. A device is linked to a unit so the unit shows up on the map; that link is made on the **GPS Integration** page.

## Units

Columns: **Name**, **Type**, and **Action**. Use the search box to find a unit by name.

- **Add** opens the **Unit Details** dialog. Enter a **Name** and choose a **Type** (Car, Truck, Motorcycle, Asset, and so on), then press **Save**. The type decides the icon and the rules used to tell when the unit counts as stopped.
- **Edit** reopens the same dialog.
- **Retire** takes a unit out of use. It stops being tracked and leaves every list, but its history is kept.

To see retired units, turn on **Show retired units**. Each retired unit then offers **Restore**, which brings it back.

For a staff member you want to follow on the map — a sales representative, a technician — register the device they carry as a unit with a name that says who it is, for example *Sales Representative North*. See [Staff and workforce](topic:staff-workforce).

## Devices

The **Devices** section lists the GPS devices known to your account. Columns: **Name** (with the device's **ID** beneath), **Serial Number**, **Description**, **Type**, and **Action**.

Devices are not added here: they arrive on their own when your GPS provider is synchronised. The only action is **Delete**, which asks you to confirm.

## Making a unit appear on the map

A unit shows on the map when two things are true:

1. **A device is linked to it.** Link devices under **GPS Integration → Device Assignments** — see [GPS Integration](topic:gps-integration#device-assignments).
2. **It is in a group the viewer belongs to.** Plain Users see only the units in their groups — see [Users, roles, and groups](topic:users-roles-groups#groups). Managers see every unit.
