---
id: feature-catalog
title: Feature catalogue
description: The eleven account features — what each one turns on or off, its settings, who can change it, and what happens when a feature is switched off.
category: reference
screens: []
related: [roles-and-permissions, admin-platform-setup, admin-account-setup, system-administration, staff-workforce]
tags: [features, settings, reference, catalogue]
order: 15
---

# Feature catalogue

Every account has a set of **features** that decide which parts of TrackHub exist for that account. There are eleven, and this page is the reference for all of them.

Two rules apply to every feature:

- **Only a platform administrator can change them**, in **System Admin → Account Features**. Managers see their own account's list read-only in **Account Management → Account Features**.
- **Features are off unless they are switched on.** A feature that was never added to an account behaves exactly like one that was switched off.

## The eleven features at a glance

| Feature | Settings | What it gates |
|---|---|---|
| Geofencing | — | The whole **Geofences** menu item, geofence alerts, and two reports |
| Trip Management | Automatic trip lifecycle (see below) | The whole **Trips** menu item, customer tracking links, toll classes, and six reports |
| Staff Mobile | — | Nothing in the web portal |
| Public Links | — | The **Public Links** section |
| Documents | — | The **Documents** sections and four document reports |
| Notifications | — | Notification rules, subscriptions, templates, deliveries, and all alert delivery |
| Email Notifications | — | Email as a delivery channel |
| WhatsApp Notifications | — | WhatsApp as a delivery channel |
| GPS Integration | Storing Interval (Seconds) | How positions are collected, and the GPS reports |
| GPS Position History | Retention Days | Stored history, replay from TrackHub, and one report |
| Workforce | Block assignment when the staff member's license is expired | Staff qualifications, assignments, expiry alerts, and three reports |

Dependencies to keep in mind:

- **Email Notifications** and **WhatsApp Notifications** only matter while **Notifications** is on.
- **GPS Position History** is normally paired with **GPS Integration**: history is built from the positions the integration collects.
- **Workforce** extends the staff area; the staff registry and staff credentials work without it.

## Geofencing

When it is off:

- **Geofences** disappears from the left menu, and the **In Geofence** figure and **Geofences** chip disappear from the live map.
- No geofence entry, exit or dwell alerts are raised.
- The **Units in Geofence** and **Geofence Events** reports disappear.

When it is switched back on, the geofences you drew are still there.

## Trip Management

When it is off:

- **Trips** disappears from the left menu, and **Transporter toll classes** disappears from Account Management.
- Trips are no longer started, advanced or completed automatically.
- Customer tracking links stop working: the customer sees the "link is not valid" message.
- The six trip reports and the **Trips** report category disappear.
- Trip start reminders and delay alerts stop.

Trips, stops, deliveries, proof of delivery and issued links are kept. Switching the feature back on restores the board as it was. The platform's **Toll catalog** is not affected.

### Trip Management settings

These decide how the automatic lifecycle behaves for the account:

| Setting | Default |
|---|---|
| **Start and complete trips automatically from the zones** | On |
| **Start watching a trip this many minutes before its planned start** | 60 |
| **Minutes past the planned start before a trip reads as overdue** | 120 |
| **Minutes parked at the final stop before the trip closes itself** | 30 |
| **How many hours back to search for a recorded departure** | 24 |
| **Minutes past the promised window before a stop counts as delayed** | 15 |
| **Minutes before a planned start to send the reminder** | 60 |

Turn the first one off for an account whose vehicles do not report reliably: every trip then runs by hand. See [Trips and route planning](topic:trip-management).

## Staff Mobile

Nothing in the web portal changes when you switch it.

## Public Links

When it is off, the **Public Links** section disappears from Account Management and no new links can be created. Links already issued are not deleted.

## Documents

When it is off:

- The **Document Library**, **Expiring Documents** and **Document Types** sections disappear, and files can no longer be uploaded or opened — **Staff Documents** included.
- Document expiry alerts stop.
- Four reports disappear: expiring documents, missing required documents, share activity, and upload volume.

Stored documents are never deleted.

## Notifications

When it is off:

- **Notification Rules**, **Alert Subscriptions**, **Notification Templates**, and **Notification Deliveries** disappear. **Alert Events** stays, because other parts of TrackHub also raise events.
- Nothing is sent — no email, WhatsApp or webhook.
- Your own notifications in the bell still work.

## Email Notifications

When it is off, **Email** is removed from the channel choices in notification rules and alert subscriptions, and email deliveries are not sent.

## WhatsApp Notifications

The same as Email Notifications, for **WhatsApp**.

## GPS Integration

**Setting:** **Storing Interval (Seconds)**, default **360**.

The **GPS Integration** menu item stays either way. What changes is how the map gets its positions:

- **On** — TrackHub keeps an up-to-date picture of where each unit is, refreshed in the background at the storing interval. The map loads quickly and does not depend on the provider answering at that moment.
- **Off** — the map asks the GPS provider directly on every refresh. Positions still appear, but more slowly.

With the feature off, the GPS reports disappear from the report list and expiring provider credential alerts are not raised.

## GPS Position History

**Setting:** **Retention Days**, default **30**.

When it is off:

- The **History source** choice on the Dashboard's **Positions** tab is hidden and history is always read from the GPS provider.
- The **GPS Position History** report disappears.

**Retention Days** is how long stored positions are kept. Lowering it deletes positions older than the new limit at the next daily clean-up.

Managers can see both GPS values, read-only, under **GPS Integration → Position Retention**.

## Workforce

**Setting:** **Block assignment when the staff member's license is expired**, default off.

The **Staff** registry and its **Credentials and devices** work without this feature. Workforce adds what is built around the staff. When it is off:

- **Staff Qualifications**, **Staff Assignments**, and **Qualification Expirations (30 days)** disappear.
- No **Staff Qualification Expiring** or **Staff Qualification Expired** alerts are raised.
- The three reports and the **Workforce** report category disappear.
- The staff member's **Default Transporter** still works.

With the setting on, assigning a staff member whose **License** qualification is expired or revoked is refused. See [Staff and workforce](topic:staff-workforce).

## What a switched-off feature looks like

- Menu items, sections, buttons, channels and reports **disappear** rather than showing an error.
- If a request still reaches TrackHub, it is refused with **"This feature is not enabled for your account."**
- **No data is deleted by switching a feature off.** Switching it back on restores access to exactly what was there.

Changes can take about a minute to reach everything. Refresh the page if a newly enabled feature has not appeared.

## Where each side is managed

| | Platform administrator | Manager |
|---|---|---|
| Where | System Admin → Account Features | Account Management → Account Features |
| Switch features on or off | Yes | No |
| Set **Tier** and settings | Yes | No |
| View the account's features | Every account | Own account only |

The administrator's list also shows a **Tier** and a **Source** for each feature. They are labels for record-keeping and do not change what the feature does. See [Initial platform setup](topic:admin-platform-setup) and [Setting up your account](topic:admin-account-setup).
