---
id: dashboard-trips-replay
title: Trips and replay
description: Query a unit's trips for any date range, read the summary stats, replay the journey on the map, and export the track to CSV.
category: operation
screens: [dashboard]
related: [dashboard-live-map, reports]
tags: [trips, replay, history, export]
order: 20
---

# Trips and replay

Look back at where a unit has been: query its trips for any date range, read the distance, time, and speed stats, replay the journey on the map, and export the track to a spreadsheet.

Open **Dashboard** from the left-hand menu and click the **Positions** tab (next to **Units**). This tab is for historical journeys; the live view is covered in [Live map](topic:dashboard-live-map).

## Querying trips

At the top of the tab is a filter row with a **Start Date** field, an **End Date** field, a **Unit** selector, and a **Search** button:

1. Pick the **Unit** whose history you want. The first unit is selected for you.
2. Choose the **Start Date** and **End Date** — a date-and-time range.
3. Click **Search**.

A unit, a start date, and an end date are all required. TrackHub then loads that unit's trips for the range and draws them on the map. If nothing is found, the map and list simply stay empty.

## History source

If your account has the position-history feature enabled, a **History source** toggle appears with two options:

- **GPS provider** — history read directly from the GPS operator/provider (the default).
- **TrackHub** — history stored by TrackHub itself.

Pick the source before you click **Search**. If your account does not have this feature, the toggle is hidden and TrackHub always uses the provider's history. Position history is an account feature managed by your administrator — see [Roles and permissions](topic:roles-and-permissions).

## The trip list

On the right is the list of trips for your query, with the chosen date range shown at the top. Each entry is one of:

- **Transit** — the unit was travelling. Shows the distance covered.
- **Stopped** — the unit was parked or idle.

Each entry also shows its **From** and **To** times and its duration. Transit legs are marked in green with a forward arrow; stops are marked in red with a stop icon. Click any entry to select that trip — it highlights on the map and enables [replay](#replaying-a-trip).

## Trip statistics

A **Summary** panel floats over the map. When no single trip is selected it aggregates the whole query; when you select a trip it shows just that trip. It lists:

- **Total Distance** (km)
- **Duration**
- **Max Speed** (km/h)
- **Avg Speed** (km/h)
- **Stops**
- **Alarms**

You can hide or show this panel with the statistics toggle on the map.

## Replaying a trip

Select a trip in the list. If the trip has a timeline, playback controls appear beneath the map:

- **Play / Pause** — start or stop the animation; a marker moves along the route.
- **Speed** — choose **1x**, **2x**, **4x**, or **8x** playback speed.
- **Timeline slider** — drag to jump to any point in the trip.

## Exporting to CSV

Click the **Export** button (with the download icon) at the top right of the tab to save the loaded trips as a CSV spreadsheet. The button is greyed out until a query has returned trips.

The file contains one row per recorded position, with these columns:

| Column | Meaning |
|---|---|
| **Transporter** | The unit name. |
| **Trip** | The trip the point belongs to. |
| **Timestamp** | When the position was recorded. |
| **Latitude** | |
| **Longitude** | |
| **Speed (km/h)** | |

There is no address column. Historical trip points carry coordinates and speed only, not street addresses, so the export intentionally omits an address column. To see an address for a live position, use the **Resolve address** button in a unit's pop-up on the [live map](topic:dashboard-live-map).

The file is named automatically from the unit, the history source, and the date range, for example `replay_TRUCK-01_PROVIDER_2026-07-01_2026-07-05.csv`.

For a broader set of history-based summaries and scheduled outputs, see [Reports](topic:reports).

Querying trips requires access to positions and position history. See [Roles and permissions](topic:roles-and-permissions).
