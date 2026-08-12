---
id: geofences
title: Geofences
description: Draw named zones on the map — polygons or circles — and get entry, exit, and dwell alerts for the units that cross them.
category: operation
screens: [geofenceManager]
featureKey: geofencing
related: [dashboard-live-map, alerts-notifications]
tags: [geofence, zones, polygon, circle, alerts, dwell]
order: 30
---

# Geofences

A geofence is an area you draw on the map and give a name, a type, and a colour. Once a zone exists, TrackHub can tell you which units are inside it, and — if you turn the options on — raise an alert when a unit enters, leaves, or lingers inside it. The **In Geofence** count on the [live map](topic:dashboard-live-map) is the number of units located within any of your geofences right now.

Geofences are available only when the geofencing feature is enabled for your account, and managing them requires geofence permissions. See [Roles and permissions](topic:roles-and-permissions).

## Opening the Geofence Manager

Open **Geofences** from the left-hand menu. The screen shows the map on the left and, on the right, a paged list of your geofences with **Name**, **Type**, **Color**, and an **Action** (delete) column.

## Creating a geofence

The map has a control stack on its right edge with two drawing tools:

1. Click **Draw polygon** (the pentagon icon) to draw a free-form shape, or **Draw circle** (the circle icon) to draw a round zone.
2. **Draw the shape** on the map:
   - For a polygon, click points to lay out the corners of the area. You can adjust the corners while drawing.
   - For a circle, drag out from the centre to set the radius.
3. When the shape looks right, click the **Save shape** control (the save icon) — it appears in the control stack only while you are drawing or editing.
4. The geofence details form opens. Fill it in (see below) and click **Save**.

## Geofence details

The form has these fields:

- **Name** – required; how the zone is labelled.
- **Description** – optional free text.
- **Type** – required; choose what the zone represents. The available types are: Client Location, Construction Site, Danger Zone, Fuel Station, Garage, Hospital, Hotel, Office, Park, Parking Lot, Restricted Area, Retail Store, School, Warehouse.
- **Color** – required; the colour the zone is drawn in (Red, Blue, Green, Yellow, Orange, Purple, Pink, Brown, Black, White). Use colour to tell zone types apart at a glance.
- **Radius (m)** and **Center** – shown for circle geofences only. The radius must be between 10 and 100000 m; you can type an exact radius here after drawing. The centre is read-only and comes from where you drew the circle.
- **Dwell threshold (min)** – optional; between 1 and 10080 minutes. If set, a unit must stay inside the zone for at least this long before a dwell alert is raised. Leave it blank for no dwell alert.
- **Active** – tick to keep the geofence in use.
- **Alert on entry** – tick to be alerted when a unit enters the zone.
- **Alert on exit** – tick to be alerted when a unit leaves the zone.

Entry, exit, and dwell alerts are delivered through the alerts and notifications system; see [Alerts and notifications](topic:alerts-notifications) for how you receive them.

## Editing a geofence

Select the geofence (from the list or on the map), then use the map's edit handles to reshape it: drag its corners (polygon) or its radius (circle) to the new outline, and click the **Save shape** control to commit the geometry. The details form reopens so you can also change the name, type, colour, description, dwell threshold, or alert options; click **Save** to finish.

## Deleting a geofence

In the list on the right, click the delete action on the geofence's row. A **Delete Geofence** confirmation appears ("Are you sure you want to delete this geofence?") — confirm to remove it. This cannot be undone.

## Finding a geofence in the list

The list is filtered and paged on the server, so use the toolbar above the map to narrow it:

- **Type** – restrict the list to one geofence type (or "All types").
- **Status** – show **Active**, **Inactive**, or **All** zones. It defaults to **Active**, so retired zones are one click away under **Inactive** or **All**.
- The navbar search box filters the list by name as you type.

The footer under the list shows "Showing *from*–*to* of *total*" with arrows to page through the results. Selecting an entry highlights the matching shape on the map. The map always shows every geofence, even while the list is filtered or paged.

## How geofences appear on the live map

On the live map, geofences are drawn only when you turn the geofence overlay on there (via the **In Geofence** card). With the overlay on, each geofence is drawn in the colour you chose, and the **In Geofence** card tells you how many units are inside a zone at that moment. See [Live map](topic:dashboard-live-map).
