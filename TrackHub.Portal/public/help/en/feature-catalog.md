---
id: feature-catalog
title: Feature catalogue
description: The eleven account features — what each one turns on or off, its settings, who can change it, and what happens when a feature is switched off.
category: reference
screens: []
related: [roles-and-permissions, admin-platform-setup, admin-account-setup, system-administration, drivers-workforce]
tags: [features, subscription, billing, entitlements, reference, catalogue]
order: 15
---

# Feature catalogue

Every account has a set of **features**. A feature is a billing entitlement: it decides which parts of TrackHub exist for that account. There are exactly **eleven** features, and this page is the reference for all of them.

Two rules apply to every feature:

- **Only a platform administrator can change them**, in **System Admin → Account Features**. Managers see their own account's list read-only in **Account Management → Account Features**.
- **Features are off unless they are switched on.** A feature that has never been provisioned for an account behaves exactly like one that was switched off. Nothing is enabled implicitly.

## The eleven features at a glance

| Feature | Key | Settings | What it gates |
|---|---|---|---|
| Geofencing | `geofencing` | — | The whole **Geofences** menu item, geofence alerts, and two reports |
| Trip Management | `trip-management` | — | The whole **Trips** menu item, customer tracking links, six reports, and two background jobs |
| Driver Mobile | `driver-mobile` | — | Nothing in the portal — reserved entitlement |
| Public Links | `public-links` | — | The **Public Links** section and creating new links |
| Documents | `documents` | — | The **Documents** section and four document reports |
| Notifications | `notifications` | — | Notification rules, subscriptions, templates, deliveries, and all alert delivery |
| Email Notifications | `notifications.email` | — | Email as a delivery channel |
| WhatsApp Notifications | `notifications.whatsapp` | — | WhatsApp as a delivery channel |
| GPS Integration | `gps.integration` | Storing Interval (Seconds) | How positions are collected, and nine GPS reports |
| GPS Position History | `gps.positionHistory` | Retention Days | Stored history, replay, and one report |
| Workforce | `workforce` | Block assignment when the driver's license is expired | Driver qualifications, assignment history, expiration alerts, and three reports |

Dependencies to keep in mind:

- **Email Notifications** and **WhatsApp Notifications** only matter while the base **Notifications** feature is on. With Notifications off, the screens where you would choose a channel are not there at all.
- **GPS Position History** is normally paired with **GPS Integration**: history is built from the positions the integration collects.
- **Workforce** extends a driver area that is otherwise core: the driver registry and driver credentials work without it.

## Geofencing

**Key:** `geofencing`. **Settings:** none.

This is one of the two features that remove a whole left-menu item — the other is Trip Management. When it is off:

- **Geofences** disappears from the left menu, and its page cannot be opened.
- Existing geofences are not evaluated, so no geofence entry or exit alerts are raised.
- The **Transporters In Geofence** and **Geofence Events** reports disappear from the report list.
- The Geofences help topic is hidden from the help index.

When it is switched back on, the geofences you drew before are still there.

## Trip Management

**Key:** `trip-management`. **Settings:** none.

The other feature that removes a whole left-menu item. When it is off:

- **Trips** disappears from the left menu, and the dispatch board cannot be opened.
- Everything behind the **Trips** resource is refused: trips and their stops, deliveries, proof of delivery, route planning, toll estimates, and the mapping of your own vehicles to toll classes.
- Positions stop being matched against running trips — the **TripTracking** resource, which is how the tracking pipeline advances a trip, is refused too — so arrivals and departures are no longer detected automatically.
- Customer tracking links stop resolving. The public tracking page is open to anyone with a link, so it does not disappear, but a link for the account answers as though it did not exist and the customer sees the "link is not valid" message. Nothing about the account is disclosed.
- Six reports disappear from the report list — trip summary, trip stop detail, on-time performance, stop dwell, toll cost, and the proof-of-delivery register — along with the whole **Trips** report category, which has nothing else in it.
- The two background jobs skip the account: **ETA refresh** (every 5 minutes) stops updating stop ETAs and stops raising trip-delay alerts, and **schedule reminder** (every 15 minutes) stops raising trip-start-due alerts.
- The Trips and Customer trip tracking help topics are hidden from the help index.

Trips, stops, deliveries, proof of delivery and issued tracking links are not deleted. Switching the feature back on restores the board exactly as it was.

The platform's **toll catalog** — the stations, tariffs and vehicle classes behind the estimate — is **not** part of this feature. It is platform reference data held under the **TollCatalog** resource, maintained by a platform administrator in System Admin, and it stays exactly as it is whether or not any account has trip management. What the feature gates is the account's own use of it: the per-vehicle toll-class mapping and the estimate on a trip.

Trip management also depends on **OpenRouteService**, an external routing service that supplies the driving route, the deviation corridor and the live ETAs. It is configured once for the deployment, not per account. When it is unreachable, route planning comes back as failed and ETAs fall back to the planned schedule — trips stay fully usable, just without live routing. Full detail is in [Trips and route planning](topic:trip-management).

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

## Workforce

**Key:** `workforce`. **Setting:** **Block assignment when the driver's license is expired**, default **off**.

This feature does **not** control drivers as such. The **Drivers** registry and the **Driver Credentials & Devices** section are core platform: an authorised administrator can register drivers, and issue, activate, lock, reset, and revoke their mobile credentials, on every account, whether or not this feature is on. What Workforce adds is everything built *around* the driver.

When it is off:

- Three sections disappear from the **Fleet & Tracking** group: **Driver Qualifications**, **Driver Assignments**, and **Qualification Expirations (30 days)**.
- Qualifications cannot be created, edited, or read, and assignment history cannot be recorded or queried.
- The daily scan stops, so no **Driver Qualification Expiring** or **Driver Qualification Expired** alerts are raised for the account.
- Three reports disappear from the report list: driver registry, qualification expirations, and driver assignment history — along with the whole **Workforce** report category, which has nothing else in it.
- The driver's **Default Transporter** still works. It is a field on the driver record, not an assignment.

Existing qualifications and assignments are not deleted; they simply cannot be reached until the feature is switched back on.

The **Block assignment when the driver's license is expired** setting is the only per-account choice this feature carries, and it is off unless you turn it on. With it on, assigning a driver whose **License** qualification is expired or revoked is rejected with a validation error. Accounts differ on how strict they need to be, which is why this is a setting rather than a rule. Full detail is in [Drivers and workforce](topic:drivers-workforce).

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
