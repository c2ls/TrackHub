---
id: glossary
title: Glossary
description: Plain-language meaning of the terms and on-screen information you meet across TrackHub.
category: reference
screens: []
related: [getting-started, dashboard-live-map, gps-integration, drivers-workforce]
tags: [glossary, terms, definitions, reference]
order: 10
---

# Glossary

Plain-language meaning of the terms and on-screen information you will meet across TrackHub.

## Units and what they are

**Unit** — Anything you track on the map: a vehicle, a piece of equipment, an asset, or even a person. Throughout the screens this is labelled a **Unit**. Each unit has a name and a **type**.

**Unit Type** — What kind of thing the unit is, shown with its own icon. Available types include: **Unknown, Aircraft, Asset, Bicycle, Boat, Car, Cargo Container, Construction Vehicle, Child, Delivery Van, Drone, Elderly Person, Fleet Vehicle, Heavy Equipment, Live Stock, Motorcycle, Package, Person, Pet, School Bus, Scooter, Taxi, Tools, Truck,** and **Tractor**. The type also affects how trips are calculated (for example, some types decide a stop using the ignition/ACC signal).

**Group** — A named collection of units and/or users. Groups control who can see which units: you generally see the units in the groups you belong to.

## Devices and GPS providers

**Device** — The physical GPS tracker (the hardware) that sits in or on a unit and reports its location. A device has a name, an identifier/serial number, and a device type (for example, **Cellular, Satellite, OBD Scanner, Phone, Smartwatch, Marine, Aviation, Pet Tracking**, and others).

**Operator** — A connection to a GPS provider — the external service that collects data from your devices and passes it to TrackHub. Each operator uses a **protocol** (the "language" it speaks), such as **Command Track, Traccar, Geotab,** or **GPS Gate**.

**Credential** — The username, password, or keys TrackHub uses to connect to an operator's service. You normally test a credential with **Test Credential**; a successful test confirms TrackHub can reach the provider.

**Geocoding Provider** — A service that turns map coordinates (latitude and longitude) into a readable street address. Supported providers include **Nominatim, OpenRouteService,** and **Google**.

## Position information (what the map shows about a unit)

When you open a unit on the map, you see its latest **position** and details:

- **Speed** — How fast the unit is moving, shown in kilometres per hour (Km/Hr).
- **Course** — The compass direction the unit is heading.
- **ACC Status (ignition)** — Whether the vehicle's ignition is **ACC On** or **ACC Off**. This is how TrackHub tells whether the engine is running.
- **Altitude** — The unit's height above sea level.
- **Mileage** — The distance the unit has travelled (its odometer reading).
- **Hourmeter** — How many hours the unit (or its engine) has been running — useful for equipment maintained by run-time rather than distance.
- **Satellites** — How many GPS satellites the device was using for this fix; more usually means a more accurate location.
- **Temperature** — A temperature reading, when the device provides one.
- **Address** — The readable street address for the position. If it is not shown yet, use **Resolve address** to look it up.
- **Coordinates** — The exact latitude and longitude.
- **Status** — Whether the unit is **Moving**, **Stopped**, or **Offline**. A unit becomes **Offline** when it has not reported for longer than the account's online interval.
- **Last Report** — When the position was received (shown as "just now", "min ago", "h ago", or "d ago").
- **Event / alarm** — Some positions carry an event or alarm code sent by the device (for example, a panic button or a specific sensor trigger); these can appear as alarms on a trip.

**Device time vs. server time** — A position has two timestamps. The **device time** is the moment the GPS device recorded the location. The **server time** is the moment TrackHub received it. They are usually close, but a gap can appear if a device was out of signal and sent its data later.

## Trips and replay

**Trip** — A single journey by a unit, from setting off to stopping. A trip summary can show its **From** and **To** points, **Total Distance**, **Duration**, **Max Speed**, **Average Speed**, number of **Stops**, and any **Alarms**. Time spent is split into **Transit** (moving) and **Stopped**.

**Replay (Playback)** — Playing back a unit's past movements on the map like a video, using **Play**, **Pause**, a **Speed** control, and a **Timeline**. You can choose the **History source**: the **GPS provider** (the raw data from the provider) or **TrackHub** (the history TrackHub has stored itself). You can also **Export** the replayed track. See [Trips and replay](topic:dashboard-trips-replay).

## Map zones and places

**Geofence** — A zone you draw on the map to watch a specific area — for example, to know when a unit enters or leaves it. Each geofence has a name, a **type**, a **colour**, and can be active or inactive.

- **Geofence types** describe what the zone represents: **Client Location, Construction Site, Danger Zone, Fuel Station, Garage, Hospital, Hotel, Office, Park, Parking Lot, Restricted Area, Retail Store, School,** and **Warehouse**.
- **Geofence colours** are simply how the zone is drawn on the map so you can tell zones apart. The colour names are shown in English throughout the app: **Red, Blue, Green, Yellow, Orange, Purple, Pink, Brown, Black,** and **White**.

**Point of Interest (POI)** — A single marked place on the map (a pin), rather than an area. Each POI has a name, a **type**, a location (latitude/longitude or address), and a colour. POI types include **Client Site, Warehouse, Fuel Station, Toll Booth, Rest Area, Workshop, Port,** and **Other**. Everyday users can view points of interest; managers can create and edit them.

## Connecting devices to units (assignments)

A **device assignment** links a GPS device to the unit it is currently fitted to, so the unit shows that device's positions. Because a unit's tracker can be swapped over time, assignments carry extra detail:

- **Primary** — Marks the main device for a unit when more than one device could report for it.
- **Priority** — When several devices could feed a unit, priority decides which one is preferred.
- **Effective From / Effective To** — The dates a particular assignment applies. This keeps an accurate history of which device was on which unit and when.
- **Assignment Status** — **Active** (in use now), **Ended** (finished), or **Superseded** (replaced by a newer assignment).

A synchronised device's own state is shown as **Unassigned** (not yet linked to a unit), **Assigned**, **Ignored** (deliberately skipped), **Removed**, or **Missing**.

## GPS synchronisation and health

**Sync Run** — One round of TrackHub contacting a GPS provider to pull the latest devices and positions. A sync run records when it **Started** and **Completed**, what **Triggered** it (automatic schedule or a manual request), the **Result**, how many devices were added or changed, how many positions were accepted, and any error code. You can also **Trigger Sync** manually.

**Operator Health** — A simple indicator of how well a GPS provider connection is working, based on its recent sync runs. It reads as **Healthy**, **Degraded**, **Offline**, **Disabled**, or **Unknown**, and is accompanied by the last successful sync, the last failure, and the latency (response time).

**Position Retention** — The rules for how long stored position history is kept, together with how often positions are stored (the **Storing Interval**) and the number of **Retention Days**. These settings depend on your subscription plan and are managed by the administrator. See [GPS integration](topic:gps-integration).

## Alerts and notifications

**Alert Event** — Something noteworthy that TrackHub detected and recorded, such as a unit entering a geofence, a communication gap, or a document about to expire. Each alert has a **Type**, a **Severity**, a **Source Module** (which part of TrackHub raised it), and a **Status**.

**Severity** — How important or urgent an alert is, from routine/informational up to **critical**. Critical alerts are the ones that need attention first; they can notify recipients immediately and be escalated if nobody responds in time.

**Alert Status** — Where the alert is in its lifecycle. You **Acknowledge** an alert to show you have seen it, and **Resolve** it once it has been dealt with.

**Notification Rule** — A rule that decides when an alert should be raised and who should be told. A rule has a trigger event, a severity, the recipients, and delivery settings, and can be **enabled** or **disabled**.

## Drivers and workforce

**Driver** — A person who operates your units. A driver is *not* a portal user: they never sign in to the web portal. They have a name, an identity document, an optional employee code, and a primary licence.

**Driver Credential** — The login and password a driver uses in the driver mobile app. One per driver, with its own lifecycle: **Pending Activation, Active, Locked,** or **Revoked**. Revoking it also stops the sessions already running on the driver’s phone.

**Device Registration** — A phone or tablet a driver has enrolled from the mobile app, with its platform, app version, and a masked push token. Revoking one registration forces that device to enrol again; the credential is untouched.

**Qualification** — A credentialed fact about a driver with an expiry date: a **License, Medical Exam, Training, Background Check, HAZMAT Permit,** or **Other**. Each carries a category, a number, issue and expiry dates, an issuing authority, a status (**Valid, Expired, Revoked**), and optionally a linked document.

**Assignment** — A time-bounded record that a driver operated a unit, with a start, an optional end, a type (**Regular** or **Temporary**), and a status (**Active, Ended, Cancelled**). Only one can be open per driver-and-unit pair, and an ended assignment cannot be changed.

**Default Transporter** — The unit a driver normally operates, set on the driver record. It is a single pre-selection, not a history — assignments are the historical record.

## Documents

**Document** — A file stored against something in your account (for example, a vehicle's insurance or a driver's licence). Documents support upload, **New Version** (replacing with an updated file while keeping history), **Download**, **Preview**, **Share**, **Sign**, and tracking of expiry.

**Document Type** — What kind of document it is, chosen from your account's configurable list (examples include **SOAT, RTM, License, Medical, POD, Receipt, Invoice, Manifest, Certificate, Photo, Signature,** and **Other**). The field is labelled **Type**.

**Classification** — How sensitive a document is, which controls who may open it: **Public, Internal, Confidential,** or **Legal**. More sensitive classifications (Confidential, Legal) are restricted to users whose access policy allows them.

**Scan Status** — The result of the safety (virus) scan a document goes through before it can be used:

- **Pending** — Not yet scanned.
- **Quarantined** — Held while it is being scanned; shown as "Scanning — available once clean".
- **Clean** — Passed the scan and safe to use.
- **Infected** — Failed the scan and blocked ("Blocked: failed virus scan").
- **Failed** — The scan could not be completed.

**Document Status** — Where the document is in its life: **Pending, Uploaded, Active, Expired, Replaced, Voided,** or **Deleted**.

## Sharing and support access

**Public Link** — A secure, time-limited web link that lets someone **outside** TrackHub view one specific thing (such as a document or a shared view) without signing in. A public link records its purpose, scope, expiry date, and how many times it has been used, and can be **revoked** at any time. The link's secret token is shown **only once** when it is created — copy it there and then, because it is not shown again.

**Support Grant** — Temporary, audited permission for a TrackHub **support** person to access your account so they can help you — for example, while investigating a ticket. A support grant records a reason, a ticket reference, an access level, and start/end times, must be approved, and can be revoked. It gives access only for the agreed window.

**API Client / Service Client** — A registered external system that connects to TrackHub automatically (machine-to-machine), rather than a person signing in. Its permissions are limited to specific resources and actions.

## Account and platform terms

**Account** — Your organisation's space in TrackHub, holding all its units, users, devices, and data. An account has a **type** (Personal, Business, or Associate) and a **status**.

**Account Status** — The overall state of the account: **Trial, Active, Suspended, Cancelled,** or **Archived**. Trial and Active accounts work normally; Suspended, Cancelled, and Archived accounts are blocked with an "Account Not Available" message. See [Roles and permissions](topic:roles-and-permissions#account-status-what-a-suspended-account-looks-like).

**Branding** — Your account's display name, logo, primary colour, and report/export header, so the app and your exported documents carry your own identity.

**Account Feature** — A capability that can be switched on or off for your whole account (for example, Geofencing, Documents, or GPS Integration). When a feature is off, its screens or sections are hidden for everyone in the account.

**Audit Trail** — A running record of important actions taken in the account — who did what, to which resource, with what result, and when. It is used to review history and support compliance.

**Background Job** — A task TrackHub runs automatically behind the scenes (for example, scanning for expiring documents or cleaning up old data). The Background Jobs list shows each job, its status, how many attempts it has made, and when it started.

**Role** — A named set of permissions given to a portal user (Administrator, Manager, or User). See [Roles and permissions](topic:roles-and-permissions).

**Policy** — A named grouping of access rights (such as Full Access, Manage Users, Read Only, Limited Update, or Audit) that can be assigned to users for finer-grained control.
