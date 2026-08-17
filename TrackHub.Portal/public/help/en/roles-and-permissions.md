---
id: roles-and-permissions
title: Roles and permissions
description: Who can see and do what — the three portal roles, account features that hide parts of the app, and what a suspended account looks like.
category: getting-started
screens: []
related: [getting-started, users-roles-groups, system-administration, feature-catalog, admin-account-setup]
tags: [roles, permissions, features, account status, access]
order: 20
---

# Roles and permissions

What you can see and do in TrackHub is decided by **two things together**: your **role** (are you a User, Manager, or Administrator) and your account's **features** (which capabilities are switched on). A suspended account blocks everyone regardless of role.

## The three kinds of sign-in

TrackHub recognises different kinds of "who is asking". Only the first one uses this web portal:

- **User** — a person who signs in to the TrackHub web portal (an administrator, a manager, or an everyday operator). **This guide is about Users.**
- **Driver** — a field driver who signs in through the **mobile app**, not this portal. Drivers do not see the portal screens described here.
- **Service Client** — a machine-to-machine integration (another system connecting automatically). No person signs in as a Service Client, and it never uses the portal.

Because every portal screen is reserved for **Users**, Drivers and Service Clients are never shown the portal even if they reach its address.

## The three portal roles

Every portal user is given one or more **roles**. TrackHub ships with three standard roles:

| Role | Who it is for | In plain terms |
|------|---------------|----------------|
| **Administrator** | Platform-level super administrator | Can do everything, across every account, including platform-wide administration. |
| **Manager** | Account administrator | Runs their own account: units, users, devices, groups, GPS providers, documents, and more. |
| **User** | Everyday operator | Uses the day-to-day screens — sees the live map, runs reports, and works with the zones and points they are allowed to see. |

You can check which roles you have on your **Profile** screen, in the **Roles** card, alongside the access **Policies** you belong to. See [Profile and settings](topic:profile-and-settings#your-roles-and-policies).

**What the roles unlock in the menu:**

- Being an **Administrator** unlocks the **System Admin** area.
- Being a **Manager** unlocks the **Account Management** and **GPS Integration** areas, and the **Account Settings** gear button in the bottom-right corner.
- Everyone with portal access (including plain **Users**) can reach the **Dashboard**, **Reports**, **Geofences** (when enabled), **Trips** (when enabled), and their own **Profile**. Dispatch is deliberately an everyday-operator job: a plain **User** can create trips, plan routes, assign a driver, run a trip and share a tracking link for the units in their groups.

> In a typical setup an Administrator is also a Manager, so an Administrator sees everything a Manager sees **plus** the System Admin area.

TrackHub decides what you can see by asking the security service, in the background, whether you are an administrator and whether you are a manager. You do not set this yourself; it comes from the roles you have been given.

## What each role sees in the left menu

| Left-menu item | Everyday **User** | **Manager** | **Administrator** |
|----------------|:-----------------:|:-----------:|:-----------------:|
| **Dashboard** (live map) | Yes | Yes | Yes |
| **Reports** | Yes | Yes | Yes |
| **Geofences** *(only if geofencing is enabled)* | Yes | Yes | Yes |
| **Trips** *(only if trip management is enabled)* | Yes | Yes | Yes |
| **Profile** | Yes | Yes | Yes |
| **Account Management** | — | Yes | Yes |
| **GPS Integration** | — | Yes | Yes |
| **Account Settings** gear button (bottom-right) | — | Yes | Yes |
| **System Admin** | — | — | Yes |

If you try to open a screen your role does not allow, TrackHub simply returns you to the **Dashboard**.

### What lives inside the manager and admin areas

**Account Management** (Managers and Administrators) groups the tools for running your own account into sections:

- **Account & Subscription** — Account details, Branding, and Account Features (view only).
- **Fleet & Tracking** — Devices, Units, Drivers, Groups, and Points of Interest.
- **Users & Access** — Users, Roles, and Policies.
- **Alerts & Notifications** — Notification Rules, Alert Subscriptions, Notification Templates, Alert Events, and Notification Deliveries.
- **Documents & Sharing** — Documents and Public Links.
- **Operations & Monitoring** — Audit Trail and Background Jobs.

See [Account Management overview](topic:management-overview) for the details of each section.

**GPS Integration** (Managers and Administrators) is the control room for your GPS providers: an overview dashboard, **GPS Operators**, **Synchronized Devices**, **Device Assignments**, **Position Retention**, **Recent Sync Runs**, and **Open GPS Alerts**. See [GPS integration](topic:gps-integration).

**System Admin** (Administrators only) is for running the whole platform across accounts, including **Accounts**, **API Clients**, **Service Client Permissions**, **Unit Types**, **Geocoding Providers**, **Account Features**, **Support Grants**, the **Toll Catalog**, plus platform-level **Roles** and **Policies**. See [System administration](topic:system-administration).

## Account features that switch parts of the app on or off

Separate from your personal role, **your whole account** has a set of **features** that can be switched on or off (usually as part of your subscription plan). You can see which features your account has under **Account Management → Account Features**. Managers can *view* this list, but turning features on or off is a billing decision handled by the platform administrator.

The account features are:

| Feature | Key | What it controls |
|---------|-----|------------------|
| **GPS Integration** | `gps.integration` | How positions are collected, plus the GPS reports. The **GPS Integration** menu item stays either way. |
| **GPS Position History** | `gps.positionHistory` | Storing and replaying a unit's past track (history playback). |
| **Geofencing** | `geofencing` | The whole **Geofences** menu item and map zones. |
| **Trip Management** | `trip-management` | The whole **Trips** menu item, customer tracking links, and the six trip reports. |
| **Driver Mobile** | `driver-mobile` | Reserved — nothing changes in the web portal. |
| **Public Links** | `public-links` | Creating shareable public links. |
| **Documents** | `documents` | The document management surfaces (uploading, sharing, and tracking files). |
| **Notifications** | `notifications` | Notification rules and alert delivery. |
| **Email Notifications** | `notifications.email` | Delivering notifications by email. |
| **WhatsApp Notifications** | `notifications.whatsapp` | Delivering notifications over WhatsApp. |

**Geofencing** and **Trip Management** are the two features that hide a whole left-menu item: when one of them is off, **Geofences** or **Trips** disappears from the menu for everyone in the account. The other features work at the section level — when a feature is off, its section, buttons, or delivery channel are hidden inside the relevant screen (for example, the **Documents** or **Public Links** sections of Account Management), rather than removing a menu item.

For exactly what each feature turns on and off, its settings, and what happens to your data when one is switched off, see the [Feature catalogue](topic:feature-catalog).

## Account status: what a suspended account looks like

Every account is in one of five states. This affects whether anyone in the account can use TrackHub at all.

| Status | Can people use the app? |
|--------|-------------------------|
| **Trial** | Yes — full access during the trial period. |
| **Active** | Yes — normal, full access. |
| **Suspended** | No — the app is blocked (see below). |
| **Cancelled** | No — the app is blocked. |
| **Archived** | No — the app is blocked. |

When your account is **Suspended**, **Cancelled**, or **Archived**, signing in does **not** bring you to the Dashboard. Instead, every user in the account sees a single **Account Not Available** screen with the message:

> This account is not currently operational. Please contact support for assistance.

along with the current **Status**. No menus, maps, reports, or management tools are available until the account is returned to **Active**. If you see this screen, contact your TrackHub support or account administrator. See also [Troubleshooting](topic:troubleshooting#my-account-is-not-operational).

## Quick reference matrix

| Capability | User | Manager | Administrator | Also needs an account feature? |
|------------|:----:|:-------:|:-------------:|--------------------------------|
| See the live map (Dashboard) | Yes | Yes | Yes | — |
| Run and export Reports | Yes | Yes | Yes | — |
| View and manage Geofences | Yes | Yes | Yes | Geofencing |
| Replay a unit's past track | Yes | Yes | Yes | GPS Position History |
| Edit own Profile and password | Yes | Yes | Yes | — |
| Manage units, devices, users, groups | — | Yes | Yes | — |
| Manage Documents | — | Yes | Yes | Documents |
| Create Public Links | — | Yes | Yes | Public Links |
| Notification rules and alerts | — | Yes | Yes | Notifications |
| GPS Integration control room | — | Yes | Yes | — |
| Account Settings (map options) | — | Yes | Yes | — |
| Platform-wide System Admin | — | — | Yes | — |

*A dash means the item is not shown for that role. "Yes" still depends on the account being **Active** or in **Trial**; a Suspended, Cancelled, or Archived account blocks everything.*
