---
id: platform-status
title: Platform Status
description: Check whether TrackHub itself is working — service tiles, what to do when something is red, and platform announcements.
category: administration
screens: [platformStatus]
related: [troubleshooting, system-administration, admin-platform-setup]
tags: [status, health, outage, maintenance, announcements, troubleshooting]
order: 75
---

# Platform Status

The **Platform Status** page answers one question in plain language: *is TrackHub working, or is it just me?*

## It works even when you cannot sign in

This is the important part. The status page is **public** — it loads without signing in, and it keeps working when the sign-in service itself is down.

**Bookmark it now**, while everything is working. Its address is your normal TrackHub address followed by `/status`, for example:

```
https://your-trackhub-address/status
```

If nobody in your team can log in, open that bookmark. If the **Sign-in** tile is red, the problem is the platform and not your password — contact your provider rather than resetting credentials.

## The tiles

Each tile is one part of the platform, named for what it does rather than by its technical name:

| Tile | What it covers |
|---|---|
| **Sign-in** | Logging in to TrackHub. |
| **Permissions** | What each person is allowed to see and do. |
| **Fleet management** | Vehicles, drivers, groups, documents, account settings. |
| **GPS connections** | Communication with your GPS providers. |
| **Position data** | Vehicle positions and their history. |
| **Geofences** | Entries, exits and stops inside your zones. |
| **Reports** | Building the reports you download or preview. |

Each tile shows one of three states:

- **Working** (green) — the service answered normally.
- **Not working** (red) — the service did not answer, or reported a problem. A short line underneath says which, for example *"The service is reachable, but its database is not."*
- **Unknown** (grey) — the service has not been checked yet, or is not configured in this installation.

The banner at the top summarises everything: **All systems operational**, or **Some systems are having problems**.

The page re-checks every minute on its own and shows when it last checked. Use **Refresh** to check immediately. Checking pauses while the browser tab is in the background, and resumes when you return to it.

## What to do when something is red

1. **Refresh once.** A single failed check can be a brief network hiccup.
2. **Look at which tile is red.** It tells you what will and will not work:
   - *Sign-in* — nobody can log in. Existing sessions may keep working for a while.
   - *Position data* or *GPS connections* — the map and positions go stale; your historical data is not lost.
   - *Reports* — reports fail, but everything else is fine.
   - *Fleet management* — most administration screens will not load.
3. **Check for an announcement** at the top of the page. Planned maintenance is usually posted there, which means the outage is expected and has an end time.
4. **If there is no announcement and the tile stays red**, report it to your provider and say which tile is red and since when — that is exactly what they need.

One thing the page cannot tell you: if the whole site fails to load, no page hosted on that site can report it. In that case the outage is broader than any single service.

## Announcements

Platform administrators can post announcements — *"We are in a maintenance window"*, *"Next weekend there will be maintenance"*, *"We are investigating a problem"*.

Announcements appear in two places:

- On this status page, **including for people who are not signed in**.
- As a coloured bar at the top of every screen inside the portal, for everyone who is signed in.

Colour follows importance: blue for information, amber for a warning, red for critical. You can dismiss the in-portal bar with its **×**; it stays dismissed until you close the browser, and a still-active announcement comes back the next time you sign in.

If the announcement bar is missing while the *Fleet management* tile is red, that is expected — announcements are stored there, so an outage hides them. The tiles keep working regardless.

## For managers and administrators

**GPS synchronisation** — signed-in **managers and platform administrators** see an extra tile for the background synchroniser. It is the companion to the GPS Integration screen: that screen shows your account's operators, this tile shows whether the synchroniser itself is running at all. It has no health endpoint of its own, so its state comes from how recently it did work: green when it recorded activity in the last five minutes. If no account has GPS integration enabled it shows **Unknown** with *"there is nothing to synchronise"* — that is normal, not a fault.

**Background tasks** (platform administrators only) — the scheduled work that runs on its own: alert evaluation, notification delivery and digests, document scanning and expiry, trial expiry, geofence stop detection, and clean-up jobs. For each one you see its state, when it last did something, and the outcome. Hover a failed outcome to see the error code.

Read this table carefully, because most of these tasks **only record activity when they actually had work to do**. A digest job with nothing to summarise writes nothing at all. So *"Idle"* and an old timestamp are normal and healthy — the page deliberately does not call them stale. Only tasks that are verified to record on every cycle can be flagged **No recent activity**, and today alert evaluation is the only one.

**Manage announcements** — opens the announcement editor. Only platform administrators can see this button, and only they can create or change announcements; account managers cannot.

For each announcement you provide:

- **Message (English)** — required, up to 500 characters, plain text. Formatting marks are shown literally, not interpreted.
- **Message (Spanish)** — optional. Spanish-language users see it when present, and the English message when not.
- **Severity** — Information, Warning or Critical, which sets the colour.
- **Starts at** / **Ends at** — optional. Leave the start empty to publish immediately, and the end empty to keep it visible until you deactivate it. An announcement disappears on its own at the end time.
- **Active** — clear this to save a draft, or to retire an announcement without deleting it. Inactive announcements are invisible everywhere.

Publish a maintenance announcement **before** the maintenance starts. Once the fleet-management service is stopped, announcements can no longer be loaded — so a banner scheduled in advance is what people will see during the window.
