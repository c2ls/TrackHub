---
id: gps-integration
title: GPS Integration
description: Connect your GPS providers, sync the device list, and watch integration health — operators, sync runs, alerts, devices, assignments, and retention.
category: administration
screens: [gpsIntegration]
related: [units-devices, system-administration, troubleshooting, admin-account-setup]
tags: [gps, operators, sync, devices, credentials, health]
order: 80
---

# GPS Integration

The **GPS Integration** page brings together everything about the link between TrackHub and your GPS provider(s). At the top is a health dashboard; below it are collapsible sections for operators, sync runs, alerts, devices, assignments, and retention. Click a section heading to expand it; the operators section has an **Add** (**+**) icon.

This page is for managers. For the integration to pull data, the GPS integration feature must be enabled for your account (set in [System Administration](topic:system-administration)). See also [Troubleshooting](topic:troubleshooting).

Setting up an operator for the first time? [Setting up your account](topic:admin-account-setup) walks through it in order — credentials ready, protocol chosen, connection tested **before** you enable it.

## The integration dashboard

The panel at the top summarises the state of your integration in three cards:

- **Operators** — Enabled / Total, plus how many are **Healthy**, **Degraded**, and **Offline**.
- **Devices** — the total, broken down into **Assigned**, **Unassigned**, **Ignored**, and **Added (last 24h)**.
- **Synchronization** — **OK / Failed (24h)**, plus **Average Sync Duration**, **Last Automatic Sync**, and **Last Manual Sync**.

Below the cards, **Devices by Provider/Status** gives a per-operator breakdown.

**Operator health** (shown here and on each operator row) means:

- **Healthy** — syncing normally.
- **Degraded** — working but with problems (for example slow or partly failing syncs).
- **Offline** — not reachable / syncs are failing.
- **Disabled** — the operator has been turned off.
- **Unknown** — no health information yet.

## GPS Operators

An **operator** is a configured connection to a GPS provider — the account, server, and credentials TrackHub uses to pull devices and positions. Each operator speaks one **Protocol**.

Click the **+** icon to open the **Operator Details** dialog:

- **Name** (required) and **Description**.
- **Phone Number**, **Email Address**, **Address**, **Contact Name**.
- **Sync Interval (min)** — how often the operator syncs automatically (defaults to 30).
- **Protocol** (required).

Press **Save**. The operator row shows its protocol, sync interval, enabled state, health, last successful sync, and modified time, with these actions:

- **Edit** — change the operator's details.
- **Enable** / **Disable** — pause or resume automatic syncing without deleting the configuration. A disabled operator's health shows as **Disabled**.
- **Delete** — remove the operator (you confirm first).
- **Test Credential** (the check action) — verifies the connection with the stored credentials. A dialog reports "The credential was successfully tested" or "An error occurred while testing the credential".
- **Trigger Sync** — pulls the device list from the provider immediately. If it does not complete, a dialog shows "Sync did not complete. Check provider connectivity and try again."
- **Credential** — opens the credential dialog (see below).
- **Reset Sync** — a stronger option that warns first: it deletes the synchronized devices for that operator and any units that only existed for those devices, then starts a fresh sync. Use it only to rebuild an operator's device list from scratch.

### Credentials

In the operator's row, click **Credential** (the key action) to open the dialog, which collects:

- **URL** (required) — the provider's endpoint.
- **Username** and **Password**.
- **Secret Key** and **Secret Key 2** — used by providers that authenticate with keys.

Fill in the fields your provider requires and **Save**, then use **Test Credential** to confirm the connection.

## Recent Sync Runs

A history of the most recent synchronizations across your operators. Columns include **Operator**, **Trigger**, **Result**, **Started**, **Completed**, **Devices (+/~/-)** (added / updated / removed), **Positions Accepted/Read**, and **Error Code**. **Result** values are **Succeeded**, **Partial**, **Failed**, **Running**, and **Pending**. A failed run usually carries an **Error Code** to help diagnose the cause.

## Open GPS Alerts

Lists the open alerts raised by the GPS integration. Columns include **Event Type**, **Severity**, **Status**, **Source Module**, and last-seen time. **Severity** ranges from **Critical** through **High** and **Medium** to **Low**. Use this section to spot integration problems that need attention.

## Synchronized Devices

The devices discovered from your providers. Columns are **Name**, **ID**, **Serial Number**, **Status**, **First Seen**, and **Last Seen**, with actions. Filter the list with the **Search** box, the **Status** and **Operators** dropdowns, and the **Show unassigned only** and **Show recently added only** switches.

**Device status** means:

- **Unassigned** — not linked to a unit yet.
- **Assigned** — linked to a unit.
- **Ignored** — deliberately hidden from normal views.
- **Removed** / **Missing** — no longer reported by the provider.

Row and bulk actions:

- **Ignore** / **Unignore** — hide or unhide a device. **Bulk Ignore** / **Bulk Unignore** apply to every device currently shown by your filters.
- **Delete** — remove a synchronized device (it warns first; assignment history stays in the audit logs).

## Device Assignments

Links between GPS devices and your units. In short: choose a unit and an unassigned device and press **Assign Device**; use **End Assignment** to stop an active link. This is covered in full — including what **Primary** and **Priority** mean — under [Units and devices](topic:units-devices).

## Position Retention

Shows how long position history is kept and how often positions are stored. These values come from your subscription and are **read-only** for managers. The note reads: "These storage settings depend on your subscription plan and are managed by the administrator."

Fields shown are **Position History** (enabled, Yes/No), **Retention Days**, and **Storing Interval (Seconds)**. To change these, the platform team adjusts your account's GPS features in [System Administration](topic:system-administration).
