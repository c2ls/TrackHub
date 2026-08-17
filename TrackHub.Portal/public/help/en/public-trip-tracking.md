---
id: public-trip-tracking
title: Customer trip tracking
description: The read-only page a customer opens from a shared trip link — what it shows, what it never shows, and what the "not valid" and "expired" messages mean.
category: operation
screens: [tripTracking]
featureKey: trip-management
related: [trip-management, public-links]
tags: [tracking, customer, public link, eta, sharing]
order: 26
---

# Customer trip tracking

When you share a trip from the [Trips](topic:trip-management) screen, TrackHub gives you a link that opens this page. The person you send it to does **not** need a TrackHub account, a password, or an invitation — the link itself is the credential.

This topic describes what your customer sees, so you can answer their questions without opening the link yourself.

## What the page shows

At the top: the **trip code**, its current **status**, and the planned start. The customer can press **Refresh** at any time to pull the latest state.

Below that, the stop list in visit order. For each stop:

- its **number** and **name**, and the **city** if one is known;
- the **planned window** you promised;
- either the **actual arrival** time, if the vehicle has already been there, or the **estimated arrival** if it has not;
- a **Delivery confirmed** mark if proof of delivery was captured at that stop;
- a badge with the stop's state — pending, arrived, departed or skipped.

If you shared **the planned route on the map**, a map appears with the route drawn on it. That box starts unticked, so a link created without it shows the stop timeline with no route line — which is the intended result, not a fault. If you shared the live position **and** the trip is in progress, the vehicle's current position appears on the map with the time it was last reported; a link with the live position but no route still gets a map, showing just the vehicle.

The **city** is the only location detail a stop ever discloses here. The full delivery address is never sent, whatever you tick — so a stop with no city filled in simply shows its name and nothing more.

## What the page never shows

The page can only display what the server sent it, and the server builds each snapshot from the boxes you ticked when you created the link. Anything you did not tick is simply not in the data — there is no hidden field and nothing to reveal by inspecting the page.

Some things are never shared **regardless of what you tick**:

- **Toll and cost figures of any kind.** These never leave your account.
- **Internal notes** on the trip or its stops.
- **Driver contact details.** If you shared the driver at all, only their **first name** appears.
- **Document and photo files**, including proof-of-delivery attachments. The customer sees only *whether* a stop has proof of delivery, never the signature or photo itself.
- **Raw position history.** The customer sees the current position at most, never the full track.
- **Internal identifiers** for your units, drivers, geofences or documents.

The live position is also time-limited by the trip's own state: it appears only while the trip is **in progress**, and disappears once the trip is completed, cancelled or aborted, even if the link is still valid.

## The messages a customer may see

| What they see | What happened | What to do |
|---|---|---|
| **This tracking link is not valid** | The link was revoked, the trip no longer exists, or the link is for something other than trip tracking. | Create a new link from the trip and send it. |
| **This tracking link has expired** | The expiry date and time you set has passed. | Create a new link with a later expiry. |
| **This link is incomplete** | Part of the address was lost — usually because it was truncated when pasted, or an email client wrapped it across two lines. | Ask them to open the full link, or send it again as a single line. |
| **Tracking is temporarily unavailable** | TrackHub could not be reached. | Ask them to try again in a few minutes; check [Platform status](topic:platform-status). |

A link for an account whose trip management feature has been switched off shows the "not valid" message. This is deliberate: TrackHub does not tell an anonymous visitor anything about accounts or features.

## Managing links you have shared

Every trip link is also an ordinary public link and appears in [Public links](topic:public-links) with its purpose, its expiry and how many times it has been opened. Each successful open is counted and audited.

To stop a link working, open the trip's **Share** dialog and press **Revoke** on that link. Revocation takes effect immediately — the next refresh shows the "not valid" message.

The link's token is shown **once**, at the moment you create it. TrackHub cannot show it to you again; if a customer loses their link, revoke the old one and issue a new one.
