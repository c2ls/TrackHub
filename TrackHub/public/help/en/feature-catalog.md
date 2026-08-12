---
id: feature-catalog
title: Feature catalogue
description: The ten account features — what each one turns on or off, its settings, who can change it, and what happens when a feature is switched off.
category: reference
screens: []
related: [roles-and-permissions, admin-platform-setup, admin-account-setup, system-administration]
tags: [features, subscription, billing, entitlements, reference, catalogue]
order: 15
---

# Feature catalogue

Every account has a set of **features**. A feature is a billing entitlement: it decides which parts of TrackHub exist for that account. There are exactly **ten** features, and this page is the reference for all of them.

Two rules apply to every feature:

- **Only a platform administrator can change them**, in **System Admin → Account Features**. Managers see their own account's list read-only in **Account Management → Account Features**.
- **Features are off unless they are switched on.** A feature that has never been provisioned for an account behaves exactly like one that was switched off. Nothing is enabled implicitly.

## The ten features at a glance

| Feature | Key | Settings | What it gates |
|---|---|---|---|
| Geofencing | `geofencing` | — | The whole **Geofences** menu item, geofence alerts, and two reports |
| Trip Management | `trip-management` | — | Nothing yet — reserved entitlement |
| Driver Mobile | `driver-mobile` | — | Nothing in the portal — reserved entitlement |
| Public Links | `public-links` | — | The **Public Links** section and creating new links |
| Documents | `documents` | — | The **Documents** section and four document reports |
| Notifications | `notifications` | — | Notification rules, subscriptions, templates, deliveries, and all alert delivery |
| Email Notifications | `notifications.email` | — | Email as a delivery channel |
| WhatsApp Notifications | `notifications.whatsapp` | — | WhatsApp as a delivery channel |
| GPS Integration | `gps.integration` | Storing Interval (Seconds) | How positions are collected, and nine GPS reports |
| GPS Position History | `gps.positionHistory` | Retention Days | Stored history, replay, and one report |

Dependencies to keep in mind:

- **Email Notifications** and **WhatsApp Notifications** only matter while the base **Notifications** feature is on. With Notifications off, the screens where you would choose a channel are not there at all.
- **GPS Position History** is normally paired with **GPS Integration**: history is built from the positions the integration collects.

## Geofencing

**Key:** `geofencing`. **Settings:** none.

This is the only feature that removes a whole left-menu item. When it is off:

- **Geofences** disappears from the left menu, and its page cannot be opened.
- Existing geofences are not evaluated, so no geofence entry or exit alerts are raised.
- The **Transporters In Geofence** and **Geofence Events** reports disappear from the report list.
- The Geofences help topic is hidden from the help index.

When it is switched back on, the geofences you drew before are still there.

## Trip Management

**Key:** `trip-management`. **Settings:** none.

A reserved entitlement. It appears in both feature lists and can be switched on or off, but in the current release **nothing in the product changes** either way.

## Driver Mobile

**Key:** `driver-mobile`. **Settings:** none.

A reserved entitlement for the field-driver mobile app. It appears in both feature lists, but **nothing in the web portal changes** when you switch it.

## Public Links

**Key:** `public-links`. **Settings:** none.

When it is off:

- The **Public Links** section disappears from the **Documents & Sharing** group of Account Management.
- Creating a new public link is refused.

Links that were already issued are **not** deleted and remain listable and revocable through the interfaces that still reach them. Switching the feature back on restores the section as it was.

## Documents

**Key:** `documents`. **Settings:** none.

When it is off:

- The **Documents** section disappears from the **Documents & Sharing** group of Account Management.
- Document search, the expiring-documents list, document shares, document types, and unit document compliance are all refused.
- Document expiry alerts stop being raised for the account.
- Four reports disappear from the report list: documents expiring, missing required documents, share activity, and upload volume.

Stored documents are never deleted by switching the feature off.

## Notifications

**Key:** `notifications`. **Settings:** none.

The broadest of the features. When it is off:

- Four sections disappear from the **Alerts & Notifications** group: **Notification Rules**, **Alert Subscriptions**, **Notification Templates**, and **Notification Deliveries**. **Alert Events** stays visible, because other parts of the platform also raise events.
- Notification rules are not evaluated and nothing is dispatched or digested — no email, no WhatsApp, no webhook.
- Reading your own in-app notifications from the bell still works.

## Email Notifications

**Key:** `notifications.email`. **Settings:** none.

A per-channel entitlement, checked separately from the base **Notifications** feature. When it is off, **Email** is removed from the channel picker in the notification-rule and alert-subscription dialogs, and any delivery that would have gone out by email is simply not sent.

## WhatsApp Notifications

**Key:** `notifications.whatsapp`. **Settings:** none.

Behaves exactly like Email Notifications, for the WhatsApp channel. When it is off, **WhatsApp** is removed from the channel pickers and WhatsApp deliveries are not sent.

## GPS Integration

**Key:** `gps.integration`. **Setting:** **Storing Interval (Seconds)**, default **360**.

This feature does **not** hide the **GPS Integration** menu item — that page stays available to managers either way. What it changes is how the map gets its positions, and which reports exist:

- **On** — TrackHub keeps a stored, continuously refreshed picture of where each unit is, updated in the background at the storing interval you set. The map reads that stored picture, so it loads quickly and does not depend on the provider being reachable at that instant.
- **Off** — the map asks the GPS provider directly on every refresh. You still see positions, but they arrive more slowly and depend on the provider responding.

The **Storing Interval (Seconds)** value controls how often positions are written down. A shorter interval means a finer trail and more storage; a longer one means less detail and less storage.

With the feature off, nine GPS reports disappear from the report list (provider health, sync history, sync statistics, device inventory, recently added devices, unassigned devices, ignored devices, assignment history, and latest position freshness), and alerts about expiring provider credentials are not raised.

## GPS Position History

**Key:** `gps.positionHistory`. **Setting:** **Retention Days**, default **30**.

Controls whether a unit's past track is kept and can be replayed. When it is off:

- On the dashboard, the choice between stored and live positions is hidden and everything is read live from the provider.
- Requests for position history and trip replay are refused.
- The **GPS Position History** report disappears from the report list.
- The daily clean-up that deletes old positions also stops, so existing rows stay on disk — they just cannot be read.

**Retention Days** is how long positions are kept before the daily clean-up deletes them. Raising it keeps more history and uses more storage; lowering it is destructive, because positions older than the new limit are deleted on the next run.

Managers can see both of these values, read-only, under **GPS Integration → Position Retention**.

## What a switched-off feature looks like

TrackHub fails closed. If a feature is not explicitly on for your account, everything behind it is treated as off:

- Menu items, sections, buttons, delivery channels, and reports **disappear** rather than showing an error.
- If a request does reach the server anyway, it is refused and the app shows the message **"This feature is not enabled for your account."**
- **No data is ever deleted by switching a feature off.** Geofences, documents, links, notification rules, and position history all survive. Switching the feature back on restores access to exactly what was there.

Changes take up to about a minute to reach every part of the platform. Refresh the page if a newly enabled feature has not appeared yet.

## Where each side is managed

| | Platform administrator | Manager |
|---|---|---|
| Where | System Admin → Account Features | Account Management → Account Features |
| Can switch features on or off | Yes | No |
| Can set Tier and the setting values | Yes | No |
| Can view the account's features | Yes, for every account | Yes, own account only |

The administrator's row for a feature also records a **Tier** (a free-text plan label such as `default`) and a **Source** (where the entitlement came from, for example `superadmin`). Neither of these changes what the feature does — they are for record-keeping and billing. See [Initial platform setup](topic:admin-platform-setup) for the administrator's side and [Setting up your account](topic:admin-account-setup) for the manager's.
