---
id: trip-management
title: Trips and route planning
description: Plan multi-stop trips, place stops from the map, a point of interest or a geofence, get an ORS route with a deviation corridor and a toll estimate, assign a driver, follow progress, and share a tracking link with your customer.
category: operation
screens: [tripManager]
featureKey: trip-management
related: [public-trip-tracking, geofences, dashboard-trips-replay, public-links, reports]
tags: [trips, dispatch, stops, route, corridor, tolls, proof of delivery, tracking]
order: 25
---

# Trips and route planning

A **trip** is one vehicle's journey through an ordered list of **stops**, from a planned start time to a finish. TrackHub plans the driving route, watches the vehicle against that route while the trip runs, records arrivals and departures, and can give your customer a read-only link to follow along.

Trips are available only when the trip management feature is enabled for your account. If you cannot see **Trips** in the left-hand menu, the feature is off — ask your account administrator, and see [Feature catalog](topic:feature-catalog).

## You plan; the system measures

**You do not start or finish trips by hand.** You say what is supposed to happen — which unit, from where, to where, when — and TrackHub works out what actually happened from the vehicle's positions:

- A trip **starts itself** when its unit reaches the **origin zone**, in the order you planned them for that vehicle.
- **Loading time** is measured from the moment it arrives at the origin to the moment it leaves.
- **Arrivals and departures** at each stop are measured the same way.
- The trip **completes itself** when its route is done.

Nobody has to watch a screen for this to work, and nobody has to click **Start** three hundred times a week. Your attention goes to the **exceptions** instead — the trips that are not going to plan.

The manual buttons are still there, grouped under **Override**. They are for the cases measurement cannot cover: a tracker with no signal, an indoor dock, a correction after the fact. See *Overriding the automatic lifecycle* below.

## The dispatch board

Open **Trips** from the left-hand menu. The board is one row per trip, showing its **code**, **customer**, **unit**, **planned start**, **stop count**, **phase** and **status**.

The **phase** is the column to read. Status tells you a trip is "in progress"; phase tells you what it is *doing* — *Loading at Plant 3*, *In transit to Client X (ETA 11:05)*, *At stop · Client Y*, *Overdue to start*. It is worked out fresh on every refresh from the times actually measured, so it is never a stale label somebody forgot to update.

The board **refreshes itself every 30 seconds**, and again the moment you come back to the tab. It has to: trips now change state without anyone here pressing anything.

Use the **Exceptions** filter to cut the board down to what needs a human:

| Exception | What it means |
|---|---|
| Overdue to start | The unit never turned up at the origin. The trip stays queued — decide whether to cancel it or re-plan it. |
| Delayed | The current estimate is already past the planned end. |
| Off corridor | The vehicle has strayed from the planned route. |
| Stalled at final stop | It arrived at the last stop and the route has nowhere left to go. |

The other filters — status, unit, driver, date from, date to — and the search box in the top bar are applied by the server, so the list is a true page of matching trips rather than a filtered view of the first few. Use the arrows below the board to move between pages; the counter tells you which rows you are looking at.

Click a row to open that trip in the workspace below.

### A trip that never left

An overdue trip is **not** skipped, and the next trip for that vehicle does **not** jump the queue. That is deliberate: running Thursday's trip before anyone noticed Monday's never left is a decision for a dispatcher, not a guess for the system. Cancel it or re-plan it and the queue moves on.

## Trip statuses

| Status | Meaning |
|---|---|
| Created | Planned but not started. Stops and the route can still be edited. |
| In progress | Running. Positions are being matched against the stops and the corridor. |
| Paused | Temporarily halted. It can be resumed, cancelled or aborted. |
| Completed | Finished normally. |
| Cancelled | Called off. Stops, history and proof of delivery are kept. |
| Aborted | Ended where it stood, with a reason. |

A trip can only move along the paths above — you will not be offered a button for a transition that is not allowed. Completed, cancelled and aborted trips are final.

## Creating a trip

Click **New trip** and fill in the dialog:

- **Trip code** (required) — your own reference. It must be unique within the account; reusing a code is rejected.
- **Customer** — shown to the customer on the tracking page.
- **Unit** (required) and **Driver** — the vehicle doing the work, and who is driving it.
- **Origin** (required) — an existing **place**: one of your geofences, or a point of interest. There is no coordinate box, on purpose. The origin is what the automatic start is measured against, so a geofence is better than a point: the trip is judged against the plant's real shape instead of a 150 m circle around a pin.
- **Destinations** (at least one) and the **activity** at them — see below.
- **Planned start** (required) and **planned end**.
- **External reference** — the id this trip carries in your own TMS or ERP.
- **Toll vehicle class** — which tariff band this vehicle falls into, used for the toll estimate.
- **Notes** — internal only. Notes are never shown to the customer.

### What the vehicle does at each stop

Every destination carries an **activity**: **loading**, **unloading** or **other**. Set it before you add the destinations — a delivery run is normally *unloading* for all of them.

This is what gives a dwell figure a meaning. Ninety minutes at a stop is an anonymous number; ninety minutes at an *unloading* stop is ninety minutes of unloading, and that is what the reports add up. The return leg of a round trip is filed as **other**, because parking back at the depot is neither.

Time at the **origin** is always loading time — that is what the origin is.

## Planning a week at a time

**Bulk upload** takes a spreadsheet and creates the trips in it. Press it, download the **template** to see the column order, fill it in and upload the file (or paste the CSV straight into the box).

Places are **named**, never typed as coordinates: write `Plant 3` and `Client X;Client Y`, and TrackHub matches those names against your account's geofences first and then its points of interest. A name it cannot find is that row's error.

One bad row never fails the batch. Every good row lands, and the rejected ones come back with their line number and the reason, so you fix those lines and upload again.

`tripType=round` appends the return leg to the origin for you. Leave `startedAt` empty unless the trip has already begun — see below.

## A trip that has already begun

Real planning happens after the fact: a bulk upload lands on Monday morning for trucks that rolled on Sunday night. Open the trip, press **Override → Already in transit**.

TrackHub looks for the vehicle's recorded departure from the origin zone. If it finds one, **those measured times are used** — arrival, departure, and any stops already visited are filled in from what actually happened, and the time you typed is ignored. Only when there is no such record does your **start time** stand in, and then the loading time is left blank rather than invented.

In a bulk upload, the `startedAt` column does the same thing for each row.

## Adding stops

With a trip selected, use **Add stop** in the stop panel. A stop's position comes from exactly three places:

1. **Click on the map.** Press **Click on the map**, then click the point. The dialog reopens with the coordinates filled in.
2. **Use a point of interest.** Pick one of your saved points of interest.
3. **Use a geofence.** Pick one of your account's geofences. The stop keeps a link to the zone, so arrival is judged against the real shape of the zone rather than a circle around a point — see [Geofences](topic:geofences).

There is deliberately **no address search box**. All three sources already give exact coordinates, and TrackHub fills the **Address** and **City** fields for you by looking up the point you chose. You can edit both freely.

**Address and City are two different disclosure levels, not a duplicate.** The address is the full street label and stays internal to your team. The city is the coarse locality and is the *only* location detail a customer tracking link can ever show — so a link holder learns that a stop is in Bogotá, never the exact door it is delivered to. Keep the city filled in: if it is empty, a shared link shows no locality at all for that stop. It is limited to 200 characters.

Each stop also takes:

- **Name** (required), the resulting **address**, and the **city**.
- **Arrival radius** — how close the vehicle must get for an arrival to be detected automatically. Widen it for large yards, narrow it for street-side deliveries. It is only used when the stop is a plain point; a stop linked to a geofence uses the zone's real shape.
- **Activity** — loading, unloading or other. See *What the vehicle does at each stop* above.
- **Planned from** / **planned to** — the window you promised.
- **Proof of delivery required** — flags the stop as one that must be signed for.
- **Priority** and **observations**.

### Changing the visit order

Drag a stop up or down in the list, or use the up and down arrows on the row. The new order is saved immediately and the stop numbers on the map follow it. Re-plan the route afterwards so the driving line matches the new order.

## Planning the route

Set the **corridor width** and press **Plan route**. TrackHub asks the routing service for the driving line through your stops and gets back:

- the **planned route**, drawn as a blue line;
- the **corridor**, a band of the width you set around that line;
- the **planned distance** and **planned duration**;
- the toll stations the route passes.

The corridor is what a deviation is measured against: while the trip runs, three consecutive positions outside the band raise one **route deviation**, and the trip picks up the **Off corridor** badge until the vehicle returns.

If the routing service is unavailable, the plan comes back as **failed** with a reason. The trip stays completely usable — you can still start it, record arrivals and complete it — and stop ETAs fall back to your planned schedule instead of a live estimate.

## Toll estimate

Below the planner, the toll panel shows the estimated cost of the route and a per-station breakdown. Use the **vehicle class** selector to re-price the same route for a different tariff band without changing the trip.

Read the status badge carefully, because it changes what the number means:

- **Complete** — every station on the route has a tariff for this class. The figure is the full estimate.
- **Partial** — at least one station has **no tariff** for this class. Those stations are listed with a *No tariff* badge, and the real cost is **higher** than the figure shown. TrackHub reports the gap rather than quietly treating an unpriced station as free.
- **No stations** — no station in the platform's catalog lies on this route. There is no estimate; this is **not** a cost of zero.
- **Not calculated** — tolls have not been worked out for this route plan yet.

The station and tariff catalog is platform-wide and is maintained by a super administrator — see [System Administration](topic:system-administration).

### Toll classes for your fleet

The estimate can only price a route once TrackHub knows which tariff band the vehicle falls into. **Toll classes**, at the top of the board, is where you tell it:

1. Choose whether the rule applies to a **vehicle type** or to **one vehicle** as an exception.
2. Pick the vehicle type or the vehicle, then the **toll vehicle class**.
3. Save. You can add as many rules as you need without closing the dialog.

New trips pick up their toll class from these rules automatically. Trips that already exist keep the class they were created with, and you can always override a single trip's class in the trip dialog.

Until at least one rule exists, trips are created with no toll class and the estimate has nothing to price against, so it never engages. If the class list is empty, the platform's toll catalog has no vehicle classes yet — a super administrator has to define them first.

## Assigning a driver

The assignment panel shows who is currently assigned, on which unit, when they were assigned, and whether they have acknowledged it. Pick a driver and a unit and press **Assign driver** to change it. Only active drivers are offered.

## Running the trip

You do not have to do anything. Once the planned start is within about an hour, TrackHub begins watching that vehicle for its next planned trip; when the unit reaches the origin zone the trip starts itself, and from there positions are matched against each stop.

While a trip is in progress the phase tells you where it is, and the workspace fills in as it goes.

### Overriding the automatic lifecycle

Press **Override** in the trip header for the manual controls. They exist for the exceptions — weak GPS, an indoor dock, a device that was off, a correction after the fact — not for everyday running:

- **Start** forces a trip to begin now. It is refused if that vehicle is already running another trip: one unit runs one trip at a time.
- **Already in transit** records a trip that started before you wrote it down. See above.
- **Pause** and **Resume** cover planned stand-downs. **Pause also suspends automatic detection** — it is how you take control of a trip. Nothing is measured until you resume.
- **Complete** finishes the trip. If stops are still pending, tick **Complete even though stops are still pending** to force it. It works on a paused trip too, so you do not have to hand a finished trip back to automation just to close it.
- **Cancel** and **Abort** both need a reason and both keep everything already recorded.

You can also record stop progress by hand from the stop table with **Record arrival**, **Record departure** and **Skip** — including while the trip is paused, which is exactly when you need them. A manual entry always wins over automatic detection, and recording the same thing twice never creates a duplicate.

The trip's **History** tab shows where every event came from, so a measured start and a typed one are always told apart.

A trip that has any recorded activity cannot be deleted — cancel it instead. Only a freshly created trip with no history can be deleted, and simply being watched does not count as history.

### Editing a trip that is being watched

While a trip is still planned you can change anything, including its unit and its origin — TrackHub just re-reads the new zones on its next check.

Once it is **running**, its unit and its origin are frozen. Everything else — customer, notes, planned times, toll class — stays editable. Re-pointing a trip mid-flight would change the meaning of measurements already taken, so cancel it and plan a fresh one instead.

### Turning the automation off

An account whose vehicles do not report reliably can switch the automatic lifecycle off entirely, and every trip then runs exactly the manual way described under **Override**. It is an account setting — ask your administrator.

## Trip detail

Under the planner you get the full picture of the trip:

- **Stops** — the planned window, the ETA with its source, the actual arrival and departure, and the current status of each stop. The ETA label tells you whether it is a **live estimate** from the routing service or a fallback to your **planned schedule**, so you know how much to trust it.
- **Deliveries** — the consignments registered against each stop and how each one turned out. See *Deliveries and proof of delivery* below for how to register and close them.
- **History** — every event on the trip, with when it happened and whether it came from the portal, the driver, automatic detection or a background job.
- **Proof of delivery** — one card per capture, with who received it, when, where, any notes, and buttons to download the attachments.
- **Route replay** — press **Load replay** to draw the positions the vehicle actually recorded over the planned line. If the trip has more history than the replay limit, a warning says so and tells you how many points are shown; use the position history report for the complete track rather than assuming the drawn line is everything. See [Trips and replay](topic:dashboard-trips-replay).

## Deliveries and proof of delivery

Deliveries and proof of delivery are recorded **from this screen**. You do not need the driver app: a dispatcher taking details over the phone or radio can keep the trip's record complete on their own.

Both stay available until the trip is completed, cancelled or aborted.

### Registering what is being delivered

Press **Delivery** on a stop row to add a consignment to it. A delivery carries a **client** (required), an optional **branch**, a **reference**, a **products** summary, free **observations** and an **order** number that controls where it appears in the list.

A delivery belongs to the stop it was created on and cannot be moved to another one — create it on the right stop, or delete it and add it again. **Edit** changes its details; **Delete** removes the line entirely. Deleting a delivery does not touch proof of delivery already captured for the stop.

### Recording the outcome

Press **Outcome** on a delivery row to say how it actually went: **delivered**, **partially delivered**, **rejected**, or back to **pending**. Add observations explaining anything unusual — a rejection especially.

If you press save again after an error, TrackHub updates the same record rather than adding a second outcome, so a retry after a dropped connection is always safe.

### Capturing proof of delivery

Press **Proof of delivery** on a stop row. Record:

- **Received by** (required) and, if you have it, the receiver's **ID**.
- **Captured** — when the handover happened. It defaults to now; change it if you are entering it after the fact.
- **Delivery** — leave it empty to mark **every** delivery on the stop as delivered. Naming one delivery records evidence for that one and leaves the others exactly as they are, which is what you want when a stop is partly rejected.
- **Latitude / longitude** — optional, if you know where the handover took place.
- **Notes**.

**Attachments** — signatures, photos of the goods, stamped receipts — are uploaded with **Attach files**. They become ordinary documents in TrackHub, filed against the trip's vehicle, so they also show up under that unit's documents and can be found later — see [Documents](topic:documents).

Every attachment is virus-scanned before it can be used as evidence. A file shows **Scan pending** for a moment after upload; press **Re-check** until it turns **Clean**. TrackHub will refuse to save the capture while any attachment is not clean, and tells you how many are still waiting — remove them or wait for the scan rather than trying repeatedly.

Saving twice never creates two proofs for the same capture, so if the save fails you can simply press save again.

Captured proofs appear as cards in the trip detail, with buttons to download each attachment.

## Sharing a tracking link with the customer

Press **Share** to create a read-only link your customer can open without an account.

1. Set a **purpose** and, required, an **expiry** date and time.
2. Tick exactly what the customer may see: **stop detail**, **the planned route on the map**, **live position**, **vehicle**, **driver first name**, and **whether each stop has proof of delivery**.
3. Press save. The link appears **once**.

Every box starts **unticked unless it is deliberately safe**, and the **planned route** in particular starts off. Leaving it off is not a mistake — the customer simply gets the stop timeline and, if you shared it, the live marker, with no route line drawn. Tick it when you actually want the customer to see the intended path. The dialog tells you which of the two the link will produce before you save.

Copy it immediately — the token inside the link is shown at creation and can never be retrieved again. If you lose it, revoke the link and create a new one.

Anything you did not tick is never sent to the customer at all. Costs, tolls, internal notes, driver contact details, document files and raw position history are **never** shared, whatever you tick. Live position is only ever visible while the trip is actually in progress.

Existing links are listed in the same dialog with their state — active, expired or revoked. **Revoke** stops a link working immediately. Trip links are ordinary public links and also appear in [Public links](topic:public-links).

The customer's view is described in [Customer trip tracking](topic:public-trip-tracking).

## Reports

Six trip reports are available on the [Reports](topic:reports) screen: trip summary, trip stop detail, on-time performance, stop dwell, toll cost and the proof-of-delivery register. They respect the same group visibility as the board, so you only ever see trips for units in your groups.

**Trip summary** carries the measured durations: time at the origin, time in transit, and total trip time, next to the planned figures they should be compared with. **Trip stop detail** and **stop dwell** carry each stop's activity, so the same dwell figure reads as loading time at a plant and unloading time at a client.

A blank duration means the measurement behind it was never taken — usually a trip whose start was typed in rather than measured. That is deliberate: a blank says "not measured", where a zero would claim the truck spent no time loading.

## If something looks wrong

- **No Trips entry in the menu** — the feature is not enabled for your account.
- **A trip never started by itself** — the unit has to be reporting positions, and it has to actually reach the origin zone. Check it on the [live map](topic:dashboard-live-map), and check the origin: a point of interest is only a 150 m circle, so pick a geofence for a large yard.
- **The phase says Overdue** — the unit never arrived at the origin. Cancel the trip or re-plan it; the rest of that vehicle's queue is waiting behind it.
- **Start says the unit is busy** — that vehicle already has a trip in progress. Complete or cancel it first.
- **A trip finished but never closed itself** — its last stop was probably skipped by hand, or its tracker went quiet before it left. A background check closes trips that sit at their final stop; otherwise close it from **Override → Complete**.
- **The trip's unit or origin cannot be changed** — it is running. Cancel it and plan a new one.
- **A bulk upload rejected a row** — read the reason on that line. Almost always it is a place name that does not match any geofence or point of interest in this account.
- **Plan route does nothing** — the trip needs at least one stop.
- **"Route planning is not configured"** — the platform's routing service has no credentials. This is a platform setting, not an account one; contact your administrator.
- **The estimate says "Partial"** — that is the catalog telling you it does not have a tariff for every station on this route, not an error.
- **Arrivals are not detected** — check the stop's arrival radius, and confirm the unit is actually reporting positions on the [live map](topic:dashboard-live-map).
- **Proof of delivery will not save** — an attachment has not finished its virus scan, or failed it. Press **Re-check**, or remove the file.
- **No delivery or proof-of-delivery buttons** — the trip is already completed, cancelled or aborted; a closed trip's record is not rewritten.
- **The toll estimate never appears on new trips** — no toll class rule exists yet. Open **Toll classes** and map your vehicle types.
- **The customer sees no city on a stop** — that stop has no city filled in. Open the stop and set it; the full address is deliberately never shared.
- **The customer sees no route line** — the link was created without **the planned route on the map** ticked. Revoke it and create a new one with the box ticked.
