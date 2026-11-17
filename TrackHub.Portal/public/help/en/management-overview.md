---
id: management-overview
title: Account Management
description: Where a Manager configures their own account — the six section groups, what each section is for, and who can open the screen.
category: administration
screens: [manageAdmin]
related: [users-roles-groups, units-devices, staff-workforce, documents, public-links, alerts-notifications, admin-account-setup]
tags: [management, account, administration, sections, holidays, points of interest]
order: 10
---

# Account Management

**Account Management** is where you configure everything for your own account — users, access, fleet, staff, alerts, documents, branding, and monitoring — from a single screen. Open it from **Account Management** in the left menu.

New account? Follow [Setting up your account](topic:admin-account-setup) instead — it puts these sections in the order they need to be done.

## Who can open it

Only **Managers** see Account Management. Going to its address without the Manager role sends you back to the Dashboard. See [Roles and permissions](topic:roles-and-permissions).

## How the screen works

A **section list** on the left names every section under its group; the section you pick opens on the right, one at a time.

![Account Management with the section list and the Units section open](assets/en/units-list.png)

- When a section lets you create records, an **Add** button appears at the top right of the section.
- Rows carry their own actions — typically **Edit** and **Delete**, plus **Assign** where a section links records together. Deletions ask you to confirm first.

## The six groups

### Account & Subscription

- **Account** — your account's **Name**, **Description**, **Type** and **Modified** date. **Edit** opens **Account Details**, where you change the name, the description and the **Time zone**. Every date TrackHub works out for your account — "today", daily totals, report ranges — is read in that time zone.
- **Branding** — **Display Name**, **Logo Document**, **Primary Color** (with a live **Preview**), and **Report / Export Header**.
- **Account Features** — a read-only list of the features switched on for your account. See the [Feature catalogue](topic:feature-catalog).
- **Holiday Calendar** — your company's holidays, year by year. Pick the **Year**, add each holiday with its **Date** and **Name**, and press **Save year**. Saving replaces the whole year, so add Sundays too if your company treats them as holidays.

### Fleet & Tracking

- **Devices** and **Units** — the GPS hardware and the things you track. See [Units and devices](topic:units-devices).
- **Staff** — the people who work for your account, with their mobile-app credentials. With the workforce feature, also **Staff Qualifications**, **Staff Assignments**, and **Qualification Expirations (30 days)**. See [Staff and workforce](topic:staff-workforce).
- **Groups** — put units and staff into groups, and give groups to users. See [Users, roles, and groups](topic:users-roles-groups#groups).
- **Points of Interest** — named places such as client sites, warehouses, fuel stations and workshops, with a **Type**, **Coordinates**, **Color** and optional **Group**. A point of interest with no group is visible to everyone in the account; one with a group only to that group's members. They can be shown on the live map and used as trip stops.
- **Transporter toll classes** — appears with trip management. Map your vehicle types, or one vehicle, to a toll vehicle class so new trips get a toll estimate. See [Trips and route planning](topic:trip-management#toll-classes-for-your-fleet).

### Users & Access

- **Users**, **Roles**, and **Policies** — who can sign in, and what they may do. See [Users, roles, and groups](topic:users-roles-groups).

### Alerts & Notifications

- **Notification Rules**, **Alert Subscriptions**, **Notification Templates**, **Alert Events**, and **Notification Deliveries**. See [Alerts and notifications](topic:alerts-notifications).

### Documents & Sharing

- **Document Library**, **Expiring Documents** and **Document Types**. See [Documents](topic:documents).
- **Public Links** — time-boxed access to one resource without a login. See [Public links](topic:public-links).

### Operations & Monitoring

- **Audit Trail** — a read-only record of who did what in your account (**Actor**, **Resource**, **Result**, **Occurred At**). Press **Load more** for older entries.
- **Background Jobs** — a read-only view of the automatic tasks that run for your account, with their **Status**, **Attempts**, and **Started At**.

## If a section or button is missing

A section, an **Add** button, or a row action can be absent because your role does not include that permission, or because the related feature is not switched on for your account. Documents, Public Links, most of the notification sections, the workforce sections and Transporter toll classes only appear with their feature on. See [Troubleshooting](topic:troubleshooting#a-feature-is-missing-from-my-menu).
