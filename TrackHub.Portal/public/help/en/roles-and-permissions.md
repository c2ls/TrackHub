---
id: roles-and-permissions
title: Roles and permissions
description: Who can see and do what — the three portal roles, how groups limit what you see, account features that hide parts of the app, and what a suspended account looks like.
category: getting-started
screens: []
related: [getting-started, users-roles-groups, system-administration, feature-catalog, admin-account-setup]
tags: [roles, permissions, features, groups, account status, access]
order: 20
---

# Roles and permissions

What you can see and do in TrackHub is decided by three things together: your **role** (User, Manager or Administrator), the **groups** you belong to, and your account's **features** (which capabilities are switched on). A suspended account blocks everyone regardless of role.

## Who uses the portal

Only **users** sign in to this web portal. **Staff** members (drivers, operators, field employees) use the staff mobile app instead and never see the portal, and connected systems (**service clients**) never sign in at all.

## The three portal roles

Every portal user is given one or more **roles**. TrackHub ships with three:

| Role | Who it is for | In plain terms |
|------|---------------|----------------|
| **Administrator** | Platform administrator | Can do everything, across every account, including platform-wide administration. |
| **Manager** | Account administrator | Runs their own account: units, staff, users, groups, GPS providers, documents, alerts, and more. Sees every unit in the account. |
| **User** | Everyday operator | Uses the day-to-day screens — the live map, trips, geofences and reports — for the units in their groups. |

You can check your roles on your **Profile** screen, alongside the access **Policies** you belong to. See [Profile and settings](topic:profile-and-settings#your-roles-and-policies).

In a typical setup an Administrator is also a Manager, so an Administrator sees everything a Manager sees **plus** System Admin.

## Groups decide which units a User sees

A plain **User** sees only the units (and staff) in the **groups** they belong to. A User in no group sees an empty map and empty reports. Managers and Administrators always see every unit in their account. Groups are managed in **Account Management → Groups** — see [Users, roles, and groups](topic:users-roles-groups#groups).

## What each role sees in the left menu

| Left-menu item | Everyday **User** | **Manager** | **Administrator** |
|----------------|:-----------------:|:-----------:|:-----------------:|
| **Dashboard** | Yes | Yes | Yes |
| **Trips** *(only if trip management is enabled)* | Yes | Yes | Yes |
| **Geofences** *(only if geofencing is enabled)* | Yes | Yes | Yes |
| **Reports** | Yes | Yes | Yes |
| **Platform Status** | Yes | Yes | Yes |
| **Profile** | Yes | Yes | Yes |
| **Account Management** | — | Yes | Yes |
| **GPS Integration** | — | Yes | Yes |
| **Settings** icon in the top bar (map options) | — | Yes | Yes |
| **System Admin** | — | — | Yes |

If you try to open a screen your role does not allow, TrackHub returns you to the **Dashboard**.

Dispatch is deliberately an everyday-operator job: a plain **User** can create trips, plan routes, assign a staff member and share a tracking link for the units in their groups.

Inside a screen, a section, an **Add** button or a row action can also be missing because your role or policies do not include that specific permission.

## Account features that switch parts of the app on or off

Separate from your role, **your whole account** has a set of **features** that the platform administrator switches on or off. Managers can see which ones are on under **Account Management → Account Features** (view only).

| Feature | What it controls |
|---------|------------------|
| **GPS Integration** | How positions are collected, plus the GPS reports. The **GPS Integration** menu item stays either way. |
| **GPS Position History** | Storing and replaying a unit's past track. |
| **Geofencing** | The whole **Geofences** menu item and map zones. |
| **Trip Management** | The whole **Trips** menu item, customer tracking links, and the trip reports. |
| **Staff Mobile** | Nothing changes in the web portal. |
| **Public Links** | The **Public Links** section. |
| **Documents** | The **Documents** section and document reports. |
| **Notifications** | Notification rules, subscriptions, templates and deliveries. |
| **Email Notifications** | Email as a delivery channel. |
| **WhatsApp Notifications** | WhatsApp as a delivery channel. |
| **Workforce** | Staff qualifications, assignments, expiry alerts, and the workforce reports. |

**Geofencing** and **Trip Management** are the two features that hide a whole left-menu item. The others hide a section, a button or a delivery channel inside a screen. For exactly what each feature turns on and off, see the [Feature catalogue](topic:feature-catalog).

## Account status: what a suspended account looks like

Every account is in one of five states:

| Status | Can people use the app? |
|--------|-------------------------|
| **Trial** | Yes |
| **Active** | Yes |
| **Suspended** | No |
| **Cancelled** | No |
| **Archived** | No |

When your account is **Suspended**, **Cancelled**, or **Archived**, every user in the account sees a single **Account Not Available** screen with the message:

> This account is not currently operational. Please contact support for assistance.

along with the current **Status**. Nothing else is available until the account is returned to **Active**. Contact your TrackHub support or account administrator. See also [Troubleshooting](topic:troubleshooting#my-account-is-not-operational).
