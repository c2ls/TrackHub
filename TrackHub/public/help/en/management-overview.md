---
id: management-overview
title: Account Management
description: Where a Manager configures their own account — the six section groups, how the collapsible sections work, and who can open the screen.
category: administration
screens: [manageAdmin]
related: [users-roles-groups, units-devices, documents, public-links, alerts-notifications]
tags: [management, account, administration, sections, permissions]
order: 10
---

# Account Management

**Account Management** is where you configure everything for your own account — users, access, fleet, alerts, documents, branding, and monitoring — from a single screen. Open it from **Account Management** in the left-hand menu.

This topic is an orientation to the screen. Each area links to a more detailed topic where one exists.

## Who can open it

Account Management is available only to **Managers**. If you do not hold the Manager capability the menu entry is hidden, and going to the address directly sends you back to the dashboard. The floating settings button (account configurator) is likewise Manager-only. See [Roles and permissions](topic:roles-and-permissions).

If your account is suspended or cancelled, the whole portal shows a status screen instead of the management sections.

## How the sections work

The screen is organised into six labelled **groups**, always shown in the same order. Under each group heading sits a stack of **collapsible sections** (accordions):

- Click a section heading to expand it. Most sections load their data the first time you open them.
- When a section supports adding records, a **+** (add) button appears in its header once the section is expanded.
- Inside a section, rows carry their own actions — typically **Edit** and **Delete**, plus **Assign** buttons where a section links records together. Deletions ask you to confirm first.

## The six groups

### Account & Subscription

- **Account** — your account's core details (**Name**, **Description**, **Type**, **Modified**). **Edit** opens the **Account Details** dialog to change the name or description.
- **Branding** — how your account looks in the portal and on exports: **Display Name**, **Logo Document**, **Primary Color** (a 6-digit hex value with a live preview), and a report/export header.
- **Account Features** — a read-only list of the optional features provisioned for your account. Managers can view this list; turning features on or off is handled by the platform team. See [System administration](topic:system-administration).

### Fleet & Tracking

- **Devices** and **Units** — your tracking devices and the units they report for. See [Units and devices](topic:units-devices).
- **Drivers** — the drivers associated with your units.
- **Groups** — organise units and users together, for example by branch or region. See [Users, roles, and groups](topic:users-roles-groups).
- **Points of Interest** — named locations (client sites, warehouses, fuel stations, workshops, and so on) that can be shown on the map.

### Users & Access

- **Users**, **Roles**, and **Policies** — the people who can sign in, and the roles and policies that decide what they may do. See [Users, roles, and groups](topic:users-roles-groups).

### Alerts & Notifications

- **Notification Rules**, **Alert Subscriptions**, **Notification Templates**, **Alert Events**, and **Notification Deliveries** — the rules that raise alerts, who is subscribed, the message templates, the alerts themselves, and the record of what was sent. See [Alerts and notifications](topic:alerts-notifications).

### Documents & Sharing

- **Documents** — store, search, and manage your account's documents. See [Documents](topic:documents).
- **Public Links** — grant limited, time-boxed access to a resource without a login. See [Public links](topic:public-links).

### Operations & Monitoring

- **Audit Trail** — a read-only record of security-relevant actions taken in your account (who did what).
- **Background Jobs** — a read-only view of the automated jobs that run for your account, with their status and attempts.

## If a section or button is missing

A section, a **+** button, or a row action can be absent because you do not hold the matching permission, or because the related feature is not enabled for your account. Public Links and Documents in particular only appear with the relevant feature turned on. See [Troubleshooting](topic:troubleshooting).
