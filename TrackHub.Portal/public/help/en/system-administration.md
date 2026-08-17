---
id: system-administration
title: System Administration
description: Platform-wide controls for super administrators — accounts, API clients, permissions, roles, unit types, geocoding, account features, and support access.
category: administration
screens: [systemAdmin]
related: [roles-and-permissions, gps-integration, platform-status, admin-platform-setup, feature-catalog]
tags: [admin, accounts, clients, features, roles, policies, support]
order: 70
---

# System Administration

The **System Admin** page is a single scrolling page of collapsible sections that operate across the whole platform rather than one account. Click a section heading to expand it; sections that create new records show an **Add** (**+**) icon in the heading. This page is restricted to platform super administrators — see [Roles and permissions](topic:roles-and-permissions). Sections are described in the order they appear.

## Accounts

The master list of every account (tenant) on the platform. Each row shows the account **Name**, **Status** (a coloured badge), and its modified time, with an **Edit**, a **Change Status**, and an **Add User** action.

Click the **+** icon to open the **Account Details** dialog and create an account. It collects:

- **Name** (required) and **Description**.
- The first user for the account: **Email Address** (required), **Password** (required), **First Name** (required), **Last Name** (required). These fields appear only when creating a new account.
- **Type** (required) and an **Active** checkbox.

Press **Save**. Creating an account also creates its first user from those fields, **with the Manager role** — that person administers their own account but has no platform-wide access. Use **Edit** on a row to change an existing account's name, description, type, or active flag (the user fields are hidden when editing).

The **Active** checkbox decides the new account's starting status: ticked creates an **Active** account, unticked creates a **Suspended** one. **Trial** cannot be chosen here.

For the whole onboarding sequence — creating an account, setting its status, and switching on its features — see [Initial platform setup](topic:admin-platform-setup).

### Changing an account's status

Accounts move through a lifecycle shown as a coloured badge in the **Status** column: **Trial**, **Active**, **Suspended**, **Cancelled**, and **Archived**.

Click **Change Status** on a row to open the dialog. The **New Status** dropdown offers only the transitions allowed from the current state:

- From **Trial** to Active, Suspended, or Cancelled
- From **Active** to Suspended or Cancelled
- From **Suspended** to Active, Cancelled, or Archived
- From **Cancelled** to Active or Archived
- **Archived** is final — no transitions are offered.

Enter a **Reason** and press **Save**. A reason is **required** when moving an account to **Suspended** or **Cancelled**; for other transitions it is optional. Suspending or cancelling an account immediately stops its users from using the app (they see an "Account Not Available" screen — see [Troubleshooting](topic:troubleshooting)).

### Adding a user to an account

Each account row has an **Add User** action. Click it to open the user dialog pre-linked to that account, fill in the user's details, and **Save** — without leaving System Administration.

## API Clients

Integration clients that let external systems connect to the platform, optionally acting as a linked user. Click the **+** icon to open the **API Client Details** dialog. For a new client it collects:

- **Name** (required), **Description**, and **Secret** (required) — the client's credential.
- **Linked User** — optionally tie the client to an existing user (an integration user).

Press **Save**. When editing an existing client, only the linked-user selection can be changed; name, description, and secret are set at creation. **Delete** removes a client after you confirm.

## Service Client Permissions

Fine-grained rules that say which client may perform which action on which resource, optionally scoped to one account. Click the **+** icon to open the **Service Client Permission** dialog. It collects **Client**, **Resource**, **Action**, **Scope**, and **Audience** (all required, entered as identifiers/text), an optional **Account** (leave blank for all accounts), an **Active** flag, and an optional **Effective From** / **Effective To** window. Use **Edit** to change a rule, or the delete action to remove it (you are asked to confirm).

## Unit Types

The categories of tracked units and the movement thresholds used to interpret their data. The catalogue of unit types is fixed — you cannot add or delete types here, only **Edit** the thresholds of an existing one. Click **Edit** on a row to open the **Unit Type Details** dialog, which collects:

- **Stopped Gap (Minutes)** (required) — how long without movement counts as stopped.
- **Max Time Gap (Minutes)** (required).
- **Max Distance (Km)** (required).
- **ACC Based** — whether stop/move is decided from the ignition (ACC) signal.

## Geocoding Providers

The services used to turn coordinates into street addresses; only one provider is active at a time. Click the **+** icon to open the **Geocoding Provider Details** dialog:

- **Name** (required).
- **Type** (required) — **Nominatim**, **OpenRouteService**, or **Google**.
- **Endpoint URI** (required) and an optional **API Key**.
- **Requests per Second** (default 1) and **Timeout (Seconds)** (default 5).
- **Configuration (JSON)** — optional advanced settings.

Row actions let you **Edit** a provider, **Delete** it, or **Activate** one that is not currently active (which deactivates the previous one).

## Roles

Roles bundle permissions that are then assigned to users. This section opens a permission matrix: pick a **Role** from the dropdown, then a grid of **Resources** (rows) and **Actions** (columns — Read, Edit, Export, Execute, Write, Delete, Custom). Tick a checkbox to grant that action on that resource for the role, or untick it to revoke. Each change is saved as you make it.

## Policies

Policies work the same way as roles but define reusable permission sets that can be attached elsewhere. Pick a **Policy** from the dropdown, then tick or untick the **Resource** by **Action** checkboxes to grant or revoke each permission. Changes save per checkbox.

## Account Features

Turn optional features on or off per account and set their tier and storage settings. This is the master control; managers can only view their own account's features in Account Management.

The list is grouped by account, showing each feature's **Account**, **Feature**, **Enabled** (Yes/No badge), **Tier**, and **Source**, with an **Edit** action. Click the **+** icon (**Add Feature to Account**) to choose an **Account** and a **Feature**, then set the options. Editing an existing row fixes the account and feature and lets you change:

- **Enabled** — the on/off switch for that feature on that account.
- **Tier** — the subscription tier (defaults to `default`).
- A storage value for certain features: **Storing Interval (Seconds)** for GPS integration (default 360), or **Retention Days** for GPS position history (default 30).

There are exactly ten manageable features: GPS integration, GPS position history, geofencing, trip management, driver mobile, public links, documents, notifications, email notifications, and WhatsApp notifications. When a feature is disabled for an account, the related menu items, sections, buttons, delivery channels, and reports are hidden from its users, and anything that still reaches the server is refused with a "feature not enabled" message (see [Troubleshooting](topic:troubleshooting)). Disabling a feature never deletes data.

What each feature actually turns on and off — key by key, including which reports and sections disappear — is set out in the [Feature catalogue](topic:feature-catalog).

## Support Grants

Time-boxed grants that let a support user access a specific account for troubleshooting, each with a reason and a ticket reference for accountability. The list shows the **Account**, **Support User Id**, **Reason**, **Ticket Reference**, **Access Level**, the start/end window, **Status**, and actions.

Click the **+** icon (**Support Grant Request**) to open the dialog, which collects **Account** (required), **Support User Id** (required), **Reason** (required), **Ticket Reference** (required), **Access Level** (defaults to `read`), and **Starts At** / **Ends At** (both required). Press **Save**.

Row actions are **Approve Support Grant** (shown while the request is neither approved nor revoked) and **Revoke Support Grant** (shown until the grant is revoked). The grant status reads as requested, approved, or revoked accordingly.
