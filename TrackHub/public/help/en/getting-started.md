---
id: getting-started
title: Getting started
description: Sign in to TrackHub, find your way around the screen, use the notification bell, and sign out safely.
category: getting-started
screens: []
related: [roles-and-permissions, profile-and-settings, dashboard-live-map, admin-platform-setup, admin-account-setup]
tags: [sign in, layout, navigation, sign off, session]
order: 10
---

# Getting started

TrackHub hands you over to a secure sign-in page and brings you back automatically. Once you are in, every screen shares the same frame: a left menu, a top bar, and a main area in the centre.

## Signing in for the first time

TrackHub does not have its own login box on the main page. Instead, it sends you to a separate sign-in page and returns you when you are done.

1. Open TrackHub in your web browser.
2. Because you are not signed in yet, the app sends you to a separate **sign-in page**. This is the identity service that protects your account.
3. Enter the **username** and **password** you were given, then confirm.
4. When your details are accepted, you are sent **back to TrackHub automatically** and land on the **Dashboard**.

You do not need to type a web address for the sign-in page or copy anything between pages. The round trip (TrackHub, then the sign-in page, then back to TrackHub) happens on its own.

**If sign-in fails**, TrackHub shows a full-screen **Authentication Failed** page that explains the likely causes: the authentication service being temporarily unavailable, network connectivity issues, or an invalid or expired authentication request. The page offers a **Retry Authentication** button. For safety, it is disabled for a short countdown — it reads **Retry in Ns** and counts down about 30 seconds — before you can try again, and there is also a **Go Back** button. Troubleshooting details for this page are in [Troubleshooting](topic:troubleshooting#i-cant-sign-in).

## The main layout

Once you are signed in, every screen shares the same frame:

- A **left menu** (the sidenav) down the left side, for moving between screens.
- A **top bar** across the top, showing where you are, a notification bell, and a **Sign off** link.
- The **main area** in the centre, which changes depending on the screen you pick.

## The left menu

The left menu lists the screens you are allowed to open. Depending on your role and which features your account has, you may see some or all of the following:

- **Dashboard** — the live map and unit overview.
- **System Admin** — platform-wide administration (only administrators see this).
- **Account Management** — manage your account's units, users, devices, and more (only managers and administrators see this).
- **Geofences** — draw and manage map zones (only appears when geofencing is switched on for your account).
- **Reports** — generate and export reports.
- **GPS Integration** — monitor GPS providers and device synchronisation (only managers and administrators see this).
- **Account** — a section heading, followed by:
  - **Profile** — your personal settings.

To open a screen, **click its name in the left menu**. The item you are currently viewing is highlighted. Which items appear for which role is covered in [Roles and permissions](topic:roles-and-permissions).

**Compact vs. full menu.** On narrow windows the menu collapses to a slim strip of icons to save space. When it is collapsed, **move your mouse over it** to expand it temporarily, and move away to collapse it again. You can also set your preference permanently with the **Sidenav Mini** switch — see [Profile and settings](topic:profile-and-settings#appearance-language-and-the-compact-menu).

## The top bar

The top bar shows, from left to right:

- **Breadcrumbs** telling you where you are (see below).
- A **menu icon** you can click to collapse or expand the left menu.
- A **notification bell** with a red count of unread notifications.
- A **Sign off** link (with a small round person icon) for leaving the app.

### The notification bell

The bell (labelled **Notifications**) sits next to **Sign off**. A red badge shows how many notifications you have not read yet. Click the bell to open the list of your recent notifications; each one shows its event type, its severity, and when it arrived. Clicking a notification marks it as read. When there is nothing to show, the list reads **No notifications**.

## Breadcrumbs

At the top left, next to a small **home** icon, TrackHub shows a trail of where you are (the current screen). Click the **home icon** to return to the start, or click an earlier step in the trail to go back to it. The current screen's name is also shown in bold just beneath the trail.

## Signing out

1. Click **Sign off** in the top-right corner of the top bar (next to the round person icon).
2. TrackHub securely ends your session and returns you to the **sign-in page**.

Always sign off when you finish, especially on a shared or public computer.

## Staying signed in and session time-outs

You do not normally have to sign in again while you are actively using TrackHub:

- Your session uses a temporary access pass that **expires after a while for security**.
- Before it becomes a problem, TrackHub **renews it quietly in the background**. You will not see any interruption, and you do not need to do anything.
- If the renewal **cannot** be completed — for example, you have been away for a long time, or the sign-in service is unreachable — TrackHub cannot keep you signed in. It then **sends you back to the sign-in page** so you can sign in again. Re-enter any unsaved work on the current screen after you return.

If you are ever unexpectedly returned to the sign-in page, simply sign in again to continue.
