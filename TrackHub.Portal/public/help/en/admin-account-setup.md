---
id: admin-account-setup
title: Setting up your account
description: For a Manager setting up a new account — the order to do things in, what to know before creating users and GPS operators, and the two mistakes everyone makes.
category: getting-started
screens: []
related: [admin-platform-setup, feature-catalog, users-roles-groups, gps-integration, management-overview]
tags: [setup, onboarding, manager, users, groups, operators, order]
order: 40
---

# Setting up your account

This guide is for the **Manager** of a TrackHub account — the person who administers one account, its people, and its fleet. If your account was just created for you, start here and work through the steps in order.

**Why the order matters:** several steps depend on earlier ones. Units cannot be assigned to devices that have not been synchronised yet, and a normal user cannot see a unit that is not in one of their groups. Doing it out of order is what produces the classic complaint: *"I signed in and the map is empty."*

If you are setting up the platform rather than one account, see [Initial platform setup](topic:admin-platform-setup) instead.

## 1. Sign in and secure your account

You were given the first user of the account, and it holds the **Manager** role.

1. Sign in as described in [Getting started](topic:getting-started).
2. Open **Profile** and, in the **Profile Information** card, click the **padlock** icon in the card's top-right corner. Enter a new password twice in the **Update Password** dialog and press **Save**.
3. While you are there, use the **User Profile** card to set your **Language** and your **Light/Dark Mode** and **Sidenav Mini** preferences, then press **Save**.

Passwords must be at least **8 characters** with **an uppercase letter, a lowercase letter, and a number**.

## 2. Set your branding

Open **Account Management → Account & Subscription → Branding**.

- **Display Name** — the name that represents your account. This is the one branding field with a visible effect elsewhere: it appears at the top of **PDF report exports**, and on the status screen if your account is ever suspended. Set it to your organisation's name.
- **Logo Document**, **Primary Color**, and **Report / Export Header** — these are stored for future use. In the current release they do not yet change the portal's appearance or the content of your exports, so do not promise colleagues a branded theme on the strength of them.

Press **Save**.

## 3. Check which features you have

Open **Account Management → Account & Subscription → Account Features**. You will see all ten features listed with **Enabled** Yes or No.

This list is **read-only for you**. Which features your account has is a billing decision made by the platform administrator, so if something you expected is missing — geofencing, documents, notifications — that is who to ask.

Check it now rather than later. A missing feature is the other common reason a screen or section "isn't there", and it looks identical to a permissions problem. What each feature turns on and off is in the [Feature catalogue](topic:feature-catalog).

## 4. Create your users

Open **Account Management → Users & Access → Users** and click the **+** icon. The **User Details** dialog collects **Email Address**, **Password**, **Username**, **First Name** and **Last Name** (plus optional second names and date of birth), and the **Active** and **Integration User** checkboxes.

Before you start, three things to know.

**Creating a user does not give them a role.** This is the single most common mistake. The **User Details** dialog has no role field. A newly created user can sign in and will land on a bare portal. You must then go to the **Roles** section — a *separate* section on the same screen — click **Assign** on the role you want, and pick the user from the dropdown. Until you do, they have no role.

**Choose the role deliberately.** There are three, and the gap between them is large:

| Role | What they get | Give it to |
|---|---|---|
| **Manager** | The whole account: units, devices, users, groups, GPS operators, documents, alerts, branding. Sees **every** unit in the account regardless of groups. | The people who administer the account |
| **User** | The day-to-day screens — dashboard, reports, geofences, their own profile. **Sees only the units in the groups they belong to.** | Everyday operators |
| **Administrator** | Platform-wide, across all accounts. | Nobody in a customer account |

Making someone a Manager is not a small favour: they can see and change everything you can, for everyone in the account. If someone only needs to watch vehicles, they are a **User**.

**Other details.** Passwords must be at least 8 characters with an uppercase letter, a lowercase letter, and a number. Tick **Integration User** only for a machine account used by an integration, never for a person. If a user is locked out after too many failed sign-ins, the **Status** column on their row turns into an **Unlock** button.

## 5. Create your groups — the step that decides who sees what

Open **Account Management → Fleet & Tracking → Groups**.

> **This is the step people skip, and it is the one that breaks.** A plain **User** sees only the units that belong to a group they are a member of. Group membership is the *only* way a normal user gets visibility of a unit — there is no per-user unit assignment anywhere else. **A User in no groups sees an empty map, no unit counters, and reports that return no rows.** They will assume TrackHub is broken.
>
> Managers and Administrators are not affected: they always see every unit in the account.

For each group:

1. Click the **+** icon and fill in the **Group Details** dialog: **Name** (required), **Description** (required), and the **Active** checkbox. Press **Save**.
2. On the group's row, click **Assign** in the **Users** column and add the users who should see this group's units.
3. Click **Assign** in the **Units** column and add the units.

Step 3 has to wait until your units exist (step 7), so expect to come back here. What you can do now is decide the shape — by branch, by region, by customer — and create the groups.

**Points of Interest** behave differently and are worth knowing about: a point of interest with no group is visible to *everyone* in the account, while one assigned to a group is limited to that group's members.

## 6. Connect your GPS operators

Open **GPS Integration** from the left menu. An **operator** is a configured connection to one GPS provider — the server, account, and credentials TrackHub uses to pull your devices and their positions.

**Have your provider credentials to hand before you start.** TrackHub does not obtain them for you; they come from your GPS provider.

1. Expand **GPS Operators** and click the **+** icon. Fill in **Name** (required), the optional contact details, **Sync Interval (min)** (defaults to **30**), and **Protocol** (required).
2. Pick the **Protocol** carefully — it must match what your provider actually speaks. The choices are `COMMAND_TRACK`, `TRACCAR`, `FLESPI`, `GEOTAB`, `GPS_GATE`, `NAVIXY`, `SAMSARA`, `WIALON`, and `PROTRACK`. The wrong protocol produces authentication or connectivity errors that look like bad credentials.
3. Press **Save**, then on the operator's row click the **Credential** (key) action. Enter the **URL** (required) and whichever of **Username**, **Password**, **Secret Key**, and **Secret Key 2** your provider uses. Press **Save**. Credentials are encrypted where they are stored.
4. **Test the connection before you enable anything.** Click the **check** action in the **Test Credential** column. You want *"The credential was successfully tested"*. TrackHub does not stop you enabling or syncing an untested operator — the buttons all stay clickable — so this discipline is yours to keep. An untested operator that fails at 3am is a much worse way to find out.
5. Only once the test passes, use **Enable** on the operator's row. A disabled operator never syncs automatically; its health shows as **Disabled**.
6. Click **Trigger Sync** to pull the device list immediately, rather than waiting for the interval. The devices then appear under **Synchronized Devices**.

**Sync Interval (min)** is how often TrackHub polls the provider automatically. Shorter is fresher and heavier on the provider; 30 minutes is a sensible starting point for device-list changes.

**Watch the health afterwards.** The dashboard at the top of the page summarises operators, devices, and synchronisation, and each operator carries a health badge: **Healthy**, **Degraded**, **Offline**, **Disabled**, or **Unknown**. **Recent Sync Runs** shows what each synchronisation actually did. Full detail is in [GPS integration](topic:gps-integration).

**One action to be careful with: Reset Sync.** It deletes the operator's synchronised devices *and any units that existed only for those devices*, then starts over. It asks you to confirm. Use it to rebuild a broken device list, never as a routine refresh.

## 7. Create your units, then assign devices

A **device** is the tracker hardware; a **unit** is the vehicle or asset you actually care about. Devices arrive automatically from the operator sync — you do not create them by hand — and are then linked to units.

1. **Units** (**Account Management → Fleet & Tracking → Units**) — click **+**, give the unit a **Name** and a **Type**, and **Save**. The **Type** determines the movement thresholds used to decide when the unit counts as stopped.
2. **Assign the device** — on the **GPS Integration** screen, under **Device Assignments**, choose the unit and an unassigned device and press **Assign Device**. Only devices not currently assigned to a unit are offered.
3. **Drivers** (**Fleet & Tracking → Drivers**) — if you track who is driving, add them here. A driver can carry a **Default Transporter**, so create the units first.
4. **Points of Interest** (**Fleet & Tracking → Points of Interest**) — named locations such as client sites, warehouses, fuel stations, and workshops, with a name, coordinates, a type, a colour, and optionally a group.

Now go back to **Groups** and complete step 5.3 — assign the units to their groups.

## 8. Set up the optional features you have

Only the features enabled for your account will be there. One line each:

- **Geofences** — draw the zones you want entry and exit alerts for. See [Geofences](topic:geofences).
- **Notification rules** — decide what raises an alert and where it goes. Email and WhatsApp are separate entitlements from the base notifications feature, so a channel you cannot select is a billing question, not a bug. See [Alerts and notifications](topic:alerts-notifications).
- **Documents** — store vehicle and driver paperwork, with expiry tracking. See [Documents](topic:documents).
- **Public Links** — share a limited, time-boxed view with someone who has no login. See [Public links](topic:public-links).

## 9. Verify the setup

Do not declare it done until you have checked it from the other side:

1. **As yourself**, open the **Dashboard**. Your units should appear on the map and be moving. If they do not, check **GPS Integration → Recent Sync Runs** and the operator's health badge.
2. **Sign in as a test User** — a plain User who belongs to one group. They should see exactly that group's units and nothing else. If their map is empty, they are in no group, or the group has no units: go back to step 5.
3. **Run a report** as that user and confirm rows come back. A user with no groups still sees the *list* of reports; the reports just return nothing. An empty report is therefore a visibility symptom, not a reporting fault.

## Checklist

- [ ] Password changed, language set.
- [ ] Branding **Display Name** set.
- [ ] Account features checked, gaps raised with the platform administrator.
- [ ] Users created **and** roles assigned on the Roles section.
- [ ] Groups created, with both users and units assigned.
- [ ] GPS operator created, credentials tested, enabled, first sync run.
- [ ] Units created and devices assigned.
- [ ] Verified as a plain User that the map is not empty.
