---
id: alerts-notifications
title: Alerts and notifications
description: See recorded alert events, acknowledge and resolve them, set notification rules, and read the navbar notification bell.
category: administration
screens: [manageAdmin]
related: [geofences, units-devices, staff-workforce]
tags: [alerts, notifications, acknowledge, resolve, rules, bell]
order: 60
---

# Alerts and notifications

TrackHub records conditions that need attention as **alert events**, and can tell the right people automatically through **notification rules**. Both live in the **Alerts & Notifications** group of the **Account Management** screen, and in-app notifications also reach you through the bell in the top bar.

## Alert events vs error toasts

Two different things can look like "an alert":

- **Alert events** are recorded conditions TrackHub keeps until someone deals with them — a lost GPS connection, an expiring document, a geofence breach. They stay in a list with a **Status** and you work through them.
- **Error toasts** are the small pop-up notices that briefly appear when an action fails (for example a screen could not load). They are momentary and are **not** stored — there is nothing to acknowledge.

## The notification bell

The bell icon in the top navigation bar is your personal in-app feed. A red badge shows how many unread notifications you have. Click the bell to open the list; each entry shows the **event type** and a **severity · time** line (critical shows red, warning shows amber, everything else blue). Clicking an unread entry marks it read and clears it from the count. When there is nothing to show, the menu reads **"No notifications"**.

The bell reflects notifications delivered to you personally. To work through the account's alert record, use **Alert Events** below.

On the Dashboard, the **Critical Alerts** figure counts open critical alerts that nobody has acknowledged or resolved yet.

## Alert Events

Open **Account Management** and pick **Alert Events** in the **Alerts & Notifications** group. This is the working list where you acknowledge and resolve alerts.

- Columns: **Type**, **Severity** (**Info**, **Warning**, **High** or **Critical**), **Unit**, **Status** (**Open**, **Acknowledged** or **Resolved**), **First seen**, **Last seen**, and **Action**.
- Narrow the list with the **Status** and **Severity** filters (**All statuses** / **All severities** by default).
- Row actions:
  - **Acknowledge Alert** — marks an open alert as seen and being looked into. Shown while the alert is still open.
  - **Resolve Alert** — marks the alert as dealt with. Shown until the alert is resolved.

An alert you have acknowledged can still be resolved afterwards. Once an alert is **resolved**, no further actions are offered. The list refreshes after each action.

> GPS connection alerts also appear read-only on the **GPS Integration** screen so you can check connection health there, but acknowledging and resolving is done here. See [GPS integration](topic:gps-integration).

## Notification Rules

Notification rules decide **who gets told, and how, when an event happens** — so you do not have to watch the alert list yourself. Pick **Notification Rules** in the same group.

![Create Notification Rule dialog](assets/en/notification-rule-create.png)

Each rule row shows its **Key**, **Type**, whether it is enabled (**Status**), and when it was last **Modified**, with **Edit** and **Disable** actions. **Disable** switches an enabled rule off without deleting it.

To create a rule, press **Add** and fill in the **Create Notification Rule** dialog. You do **not** type JSON — everything is chosen from structured fields:

- **Key** (required) — a short identifier for the rule.
- **Type** (required) — the kind of rule.
- **Trigger Event** (required) — the event that fires the rule, picked from a list (see below).
- **Channels** — tick the delivery channels: **In-App**, **Email**, **Webhook** and **WhatsApp**. Selecting **Webhook** reveals **Webhook URL** and **Webhook Secret** fields.
- **Recipients** — tick the roles to notify (**Administrator**, **Manager**) and/or **Notify subscribers**.
- **Additional contacts** — press **Add contact** to add one-off recipients, each with a channel (Email or WhatsApp) and an **Address**; remove a row with its delete icon.
- **Throttling** — optional limits so people are not flooded: **Dedupe Window (minutes)**, **Digest** (None, Hourly or Daily) and **Max per Hour**.
- **Enabled** — tick to switch the rule on.

Press **Save** to create the rule, or **Save** on the **Update Notification Rule** dialog when editing.

### Trigger events

The **Trigger Event** list covers the events the platform can raise:

| Trigger event | Raised when |
|---|---|
| **Geofence Entered** / **Geofence Exited** | A unit crosses a geofence boundary. |
| **Geofence Dwell Exceeded** | A unit stays inside a geofence too long. |
| **Communication Loss** | A device stops reporting. |
| **GPS Credential Expiring** | A GPS integration credential is about to expire. |
| **GPS Operator Sync Failed** | A GPS operator sync did not complete. |
| **Document Expiring** / **Document Expired** | A tracked document nears or passes its expiry. |
| **Staff Qualification Expiring** / **Staff Qualification Expired** | A staff member licence, medical exam, training, background check or HAZMAT permit nears or passes its expiry. |
| **Notification Delivery Failed** | A notification could not be delivered. |
| **Trip Assigned** | A staff member is assigned to a trip. |
| **Trip Start Due** | A trip's planned start is approaching. |
| **Trip Started** / **Trip Completed** / **Trip Cancelled** | A trip starts, finishes, or is called off. |
| **Trip Stop Arrived** / **Trip Stop Departed** | The unit arrives at or leaves a stop. |
| **Trip Delayed** | A stop is running later than its promised window. |
| **Trip Route Deviation** | The unit leaves the planned route corridor. |
| **Trip POD Submitted** | Proof of delivery is captured at a stop. |

Geofence events come from the geofences you define — see [Geofences](topic:geofences). Communication-loss and document events tie back to your [Units and devices](topic:units-devices). Staff qualification events come from the daily expiration scan and need the workforce feature — see [Staff and workforce](topic:staff-workforce). Trip events need trip management — see [Trips and route planning](topic:trip-management).

The two staff-qualification events behave a little differently from the rest: they are raised at fixed distances from the expiry date — **30**, **15**, **7**, and **0** days — and each of those fires exactly once per qualification. The first three are **Staff Qualification Expiring**; the day it lapses is **Staff Qualification Expired**, at a higher severity. There is nothing to configure on the scan itself; it runs once a day for every account that has the workforce feature.

> **Notification Rules**, **Alert Subscriptions**, **Notification Templates** and **Notification Deliveries** appear only when the notifications feature is switched on for your account. The **Email** and **WhatsApp** channels appear only when those features are on too. See the [Feature catalogue](topic:feature-catalog).

## Other sections in this group

The **Alerts & Notifications** group also holds **Alert Subscriptions** (who follows which event types on which channel), **Notification Templates** (customise the subject and body of a message per channel and language) and **Notification Deliveries** (a read-only history of what was sent, delivered or failed). They are advanced setup and are usually configured by an administrator.

## Who can use this

Alert Events and Notification Rules live under **Account Management** and require the matching permissions. See [Account Management](topic:management-overview).
