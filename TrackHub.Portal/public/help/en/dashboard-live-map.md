---
id: dashboard-live-map
title: Live map
description: Watch your fleet in real time — summary cards, filters, map markers, overlays, follow mode, and the units side list.
category: operation
screens: [dashboard]
related: [dashboard-trips-replay, geofences, glossary]
tags: [map, units, follow, filters]
order: 10
---

# Live map

Watch your whole fleet in real time: where every unit is right now, which ones are moving, and which are sitting inside a geofence.

Open **Dashboard** from the left-hand menu. The screen has two tabs at the top:

- **Units** — the live map with every unit and its current position (this page).
- **Positions** — historical trips and replay, covered in [Trips and replay](topic:dashboard-trips-replay).

You are on the **Units** tab by default.

Which map you see (Google or OpenStreetMap) depends on your account. TrackHub can be configured to use either provider; if your account uses Google, a Maps Key is set for you. This is an account-level setting managed by your administrator, so the map may look different from another company's TrackHub. Everything described below works the same on both.

At the top right of the screen is a **Search units** box. Type a unit name or plate to narrow the [units side list](#the-units-side-list) instantly. This box appears only on the **Units** tab.

## The summary cards

Five cards sit across the top and always reflect your **whole** authorised fleet — the filters below do not change these numbers:

| Card | What it counts |
|---|---|
| **Total Units** | Every unit reporting a position. |
| **Active Units** | Units that have reported recently (within your account's online interval). |
| **Moving Units** | Units currently in motion (speed greater than zero). |
| **In Geofence** | Units currently located inside one of your geofences. |
| **Critical Alerts** | Open critical-severity alerts that have not yet been acknowledged or resolved. |

The **Active Units**, **Moving Units**, and **In Geofence** cards also show that count as a percentage of your total.

Clicking the pin icon on the **In Geofence** card switches the geofence overlay on and off on the map (see [Map overlays](#map-overlays)).

## The filter bar

Below the cards is a row of filters that narrow **what the map shows**. They do not change the summary cards or the side list. Each filter is a dropdown that starts on its "all" value:

- **Group** — show only units in a chosen group (*All groups* by default).
- **Unit Type** — show only one type of unit, for example Truck or Car (*All types* by default).
- **Operator** — show only units served by a chosen GPS operator (*All operators* by default).
- **Status** — *All statuses*, **Moving**, **Stopped**, or **Offline**.

On the right of the same row are three toggle chips: **Points of Interest**, **Follow**, and **Trail** (explained in [Map overlays](#map-overlays) and [Follow mode](#follow-mode)).

## The live map

Each unit appears as a coloured marker pointing in its direction of travel. Marker colour reflects the unit's status: green for moving, red for stopped, and grey for offline. A unit counts as offline when it has not reported within your account's online interval; otherwise it is moving when its speed is above zero or its ignition is on, and stopped the rest of the time.

When many units are close together they are grouped into a **cluster** showing a count; zoom in or click the cluster to break it apart into individual markers. The map centres itself on your fleet when it first loads.

Click any marker to select that unit, open its pop-up, and centre the map on it. Selecting a row in the [units side list](#the-units-side-list) also selects the unit and centres the map on it.

## Unit pop-ups

Clicking a marker opens a pop-up with the unit's latest details. Depending on what the device reports, you may see:

- The unit **Name** and unit type in the coloured header.
- **Last Report** date/time, plus a friendly "just now / X min ago" note.
- **Speed** in km/h with a **Moving** or **Stopped** tag.
- **Mileage**, **Temperature**, **Hourmeter**, **ACC Status** (ignition On/Off), **Satellites**, and **Altitude** — only when the device sends them.
- **Address** — the street address for the position. If no address is stored yet, the pop-up instead shows the **Location** (city, state, country) or the raw **Coordinates** with a **Resolve address** button; click it to look up the street address on demand.
- Two quick-action buttons at the bottom: **Share on WhatsApp** (sends a Google Maps link to the location) and **Street View** (opens Google Street View at that spot).

## Map overlays

Three overlays can be switched on independently.

**Points of Interest** — click the **Points of Interest** chip in the filter bar to show your saved locations (warehouses, fuel stations, client sites, and so on) on the map. They load the first time you turn the overlay on.

**Geofences** — switch the geofence overlay on by clicking the pin icon on the **In Geofence** card. Your geofences then appear as coloured shapes over the map. See [Geofences](topic:geofences) for how to create and colour them.

**Trail** — click the **Trail** chip to draw a short breadcrumb line of the most recent positions for the **selected** unit, so you can see where it has just been. Select a unit first.

## Follow mode

Click the **Follow** chip to keep the map centred on the selected unit as new positions arrive. Follow is only available once you have selected a unit; the chip stays disabled until then. Panning (dragging) the map yourself turns Follow off again.

## Map controls

Small controls sit on the map itself:

- **Fullscreen** (top-left) — expand the map to fill the screen; click again to exit.
- **Measure distance** (top-left) — measure the distance between points you click on the map, in metric units.
- **Scale bar** (bottom-left) — shows the current map scale in metric units.

The map also tries to use your browser location as a starting centre, and it follows the application's light/dark theme, so switching the app to dark mode also darkens the map tiles.

## Auto-refresh and the countdown

When automatic refresh is enabled for your account, a small counter on the map (for example `45 s.`) counts down to the next refresh. When it reaches zero, TrackHub reloads all positions and the **In Geofence** count, then restarts the countdown. The refresh interval is an account setting managed by your administrator. If automatic refresh is switched off, no counter is shown and positions update when you reload the page. A failed refresh keeps the last known positions on the map rather than blanking it.

## The units side list

On the right is a scrollable list of all your units, with columns:

- **St.** — a status dot (green when moving, red when stopped).
- **Name** — the unit name or plate.
- **Date Time** — time of its last report.
- **Speed (Km/Hr)**.

Click a row to select that unit, highlight it, and centre the map on it. The **Search units** box at the top right filters this list. Above the list, coloured chips summarise how many units of each type are currently reporting.

Viewing the live map requires access to positions. See [Roles and permissions](topic:roles-and-permissions).
