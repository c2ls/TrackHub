---
id: gps-integration
title: GPS Integration
description: Connect your GPS providers, sync the device list, link devices to units, and watch integration health — operators, sync runs, alerts, devices, assignments, and retention.
category: administration
screens: [gpsIntegration]
related: [units-devices, admin-account-setup, troubleshooting, system-administration]
tags: [gps, operators, providers, sync, devices, assignments, credentials, health]
order: 80
---

# GPS Integration

The **GPS Integration** page brings together everything about the link between TrackHub and your GPS providers. At the top is a health dashboard; below it are collapsible sections: **GPS Operators**, **Recent Sync Runs**, **Open GPS Alerts**, **Synchronized Devices**, **Device Assignments**, and **Position Retention**. Click a section heading to expand it.

This page is for Managers. Setting up an operator for the first time? [Setting up your account](topic:admin-account-setup) walks through it in order.

## The integration dashboard

Three cards summarise the state of your integration:

- **Operators** — **Enabled / Total**, plus how many are **Healthy**, **Degraded**, and **Offline**.
- **Devices** — the **Total**, broken down into **Assigned**, **Unassigned**, **Ignored**, and **Added (last 24h)**.
- **Synchronization** — **OK / Failed (24h)**, **Average Sync Duration**, **Last Automatic Sync**, and **Last Manual Sync**.

![GPS Integration dashboard cards and Devices by Provider/Status](assets/en/gps-integration.png)

Below the cards, **Devices by Provider/Status** breaks the device counts down per operator.

**Operator health** means:

- **Healthy** — syncing normally.
- **Degraded** — working, but with problems such as slow or partly failing syncs.
- **Offline** — the provider cannot be reached, or syncs are failing.
- **Disabled** — the operator has been turned off.
- **Unknown** — no health information yet.

## GPS Operators

An **operator** is your connection to one GPS provider account — the provider, its address, and the credentials TrackHub uses to pull your devices and their positions.

### Adding an operator

Expand **GPS Operators** and click the **+** icon to open **Operator Details**:

- **Name** (required) and **Description**.
- **Phone Number**, **Email Address**, **Address**, and **Contact Name** — your contact at the provider.
- **Sync Interval (min)** — how often the device list is refreshed automatically (defaults to 30).
- **Protocol** (required) — your GPS provider.

The providers you can choose from are: Cartrack, CommandTrack, Flespi, Geotab, GPS Trackit (Zonar), GpsGate, Mapon, MettaX IoT, Motive, Navixy, Pegasus Gateway, Plaspy, Protrack, RedGPS, Ruptela TrustTrack, Samsara, Traccar, Tracksolid Pro, Webfleet, and Wialon.

Some providers allow only one position update every so many seconds. When you pick one of them, the dialog tells you its limit; if your account stores positions more often than that, the provider's limit applies.

Press **Save**.

### Entering the credentials

On the operator's row, click **Credential** to open the credential dialog. It asks only for what your provider needs, using the provider's own names — for example **API key**, **Database**, **Application ID** or **Authorization token** instead of a generic key. The **URL** is filled in with the provider's usual address where there is one; for providers with regional servers, the hint under the field shows an example. Fill in the fields and press **Save**.

![Credential dialog for a GPS provider with example values](assets/en/gps-credentials.png)

Then click the check button in the **Test Credential** column. A **Connectivity Test** message reports **"The credential was successfully tested"** or **"An error occurred while testing the credential"**. Test before you enable the operator.

### Operator actions

The operator row shows its **Protocol**, **Sync Interval (min)**, **Enabled**, **Health**, **Last Successful Sync**, and **Modified**, with these actions:

- **Edit** — change the operator's details.
- **Enable** / **Disable** — resume or pause automatic syncing without deleting anything. A disabled operator shows **Disabled** health.
- **Delete** — remove the operator (you confirm first).
- **Trigger Sync** — refresh the device list from the provider now. When it finishes you see how many devices were seen, added, updated and removed. If it is still running, you are told to check the sync history in a moment. **"Sync completed, but the provider returned no devices"** usually means the credential reaches the wrong provider account.
- **Credential** — open the credential dialog.
- **Reset Sync** — re-run device detection from scratch for this operator. Devices no longer in the provider's list are retired and their unit links ended, ignored devices are detected again, and your units keep their identity and history. It asks you to confirm. Use it to repair a device list, not as a routine refresh.

## Recent Sync Runs

The most recent synchronisations across your operators: **Operator**, **Trigger**, **Result**, **Started**, **Completed**, **Devices (+/~/-)** (added / updated / removed), **Positions Accepted/Read**, and **Error Code**. **Result** is **Succeeded**, **Partial**, **Failed**, **Running**, or **Pending**. A failed run usually carries an **Error Code**.

## Open GPS Alerts

Open alerts raised by the GPS integration, such as failed syncs or expiring provider credentials: **Event Type**, **Severity**, **Status**, **Source Module**, and **Last Seen**. Acknowledge and resolve them under **Account Management → Alert Events** — see [Alerts and notifications](topic:alerts-notifications).

## Synchronized Devices

The devices found at your providers: **Name**, **ID**, **Serial Number**, **Status**, **First Seen**, and **Last Seen**. Narrow the list with **Search**, the **Status** and **Operators** filters, and the **Show unassigned only** and **Show recently added only** switches.

**Device status** means:

- **Unassigned** — not linked to a unit yet.
- **Assigned** — linked to a unit.
- **Ignored** — deliberately set aside.
- **Removed** / **Missing** — no longer reported by the provider.

Actions:

- **Ignore** / **Unignore** — set a device aside, or bring it back. **Bulk Ignore (this page)** and **Bulk Unignore (this page)** apply to the devices on the current page.
- **Delete device** — remove a synchronized device (it asks first; its assignment history stays in the audit log).
- **Add Device** — appears only when you have an operator whose provider cannot list its devices. It opens **Register Device Manually**. For providers that look vehicles up by plate, enter the exact plate as the name. Tick **Create and assign a unit automatically** to have a unit created and linked in the same step.

## Device Assignments

A device only shows on the map once it is linked to a **unit**. This section is where you make that link.

![Device Assignments with the unit and device pickers and the assignment table](assets/en/gps-device-assignments.png)

1. Under **Unit to receive the device**, pick the unit.
2. Under **Unassigned GPS device**, pick the device. Only devices not currently linked to a unit are offered.
3. Press **Assign Device**.

The table lists **Units**, **Devices**, **Primary**, **Priority**, **Status** (**Active**, **Ended**, or **Superseded**), **Effective From**, and **Effective To**. It shows active links; switch to **Show All** to include past ones. **End Assignment** stops a link — you are asked for a reason — and the unit stops receiving that device's positions.

A linked unit is still invisible to a plain User until it is in one of their groups. See [Users, roles, and groups](topic:users-roles-groups#groups).

## Position Retention

How long position history is kept and how often positions are stored: **Position History** (Yes/No), **Retention Days**, and **Storing Interval (Seconds)**. These values are read-only here; your platform administrator sets them (see [Feature catalogue](topic:feature-catalog)).
