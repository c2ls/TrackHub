---
id: admin-platform-setup
title: Initial platform setup
description: For the platform administrator — secure the installed administrator account, tour System Admin, create customer accounts, manage their lifecycle, and switch features on.
category: getting-started
screens: []
related: [admin-account-setup, feature-catalog, system-administration, roles-and-permissions]
tags: [setup, onboarding, administrator, accounts, features, lifecycle, platform]
order: 30
---

# Initial platform setup

This guide is for the **platform administrator** — the person who looks after TrackHub itself, across every customer account. It picks up where installation leaves off: the platform is running, and you have been given the administrator account created during installation.

Follow the steps in order. Each one leaves the platform in a state the next step can build on.

Setting up a *single* account, once it exists, is a different job with its own guide: [Setting up your account](topic:admin-account-setup).

## 1. Sign in and change your password

The installation creates one administrator user. It holds both the **Administrator** and **Manager** roles, so it can reach every screen in the product.

1. Sign in with it as described in [Getting started](topic:getting-started).
2. Go to **Profile** in the left menu.
3. In the **Profile Information** card, click the **padlock** icon in the top-right corner of the card.
4. In the **Update Password** dialog, enter a new password twice and press **Save**.

Do this before anything else. The password an installation ships with is a setup convenience, not a secret — treat the account as unsecured until you have changed it.

Your new password must be at least **8 characters** and contain **an uppercase letter, a lowercase letter, and a number**.

## 2. Get to know System Admin

**System Admin** is the platform-wide screen and only Administrators can open it. It is one long page of collapsible sections; click a heading to expand it. In order, the sections are:

| Section | What it is for | How often you touch it |
|---|---|---|
| **Account** | The master list of every customer account, and where you create new ones | Every new customer |
| **API Clients** | Integration clients that let external systems connect | Rarely |
| **Service Client Permissions** | Which client may perform which action on which resource | Rarely |
| **Unit Types** | Movement thresholds used to decide when a unit counts as stopped | Rarely — leave as installed |
| **Geocoding Providers** | The service that turns coordinates into street addresses | Once at setup |
| **Roles** | The permission matrix behind Administrator, Manager, and User | Rarely — leave as installed |
| **Policies** | Reusable permission sets | Rarely — leave as installed |
| **Account Features** | Which features each account has | Every new customer |
| **Support Grants** | Time-boxed support access into a customer account | When troubleshooting |

Every section is described field by field in [System administration](topic:system-administration). The rest of this guide covers the four you will actually use during onboarding.

## 3. Create a customer account

An **account** is a customer — its own users, units, devices, and data, sealed off from every other account.

1. Open the **Account** section and click the **+** icon.
2. Fill in the **Account Details** dialog:
   - **Name** (required) and **Description**.
   - **Email Address**, **Password**, **First Name**, **Last Name** — these are the account's **first user**, and they only appear when you are creating a new account.
   - **Type** (required) — **Personal**, **Business**, or **Associate**.
   - **Active** — leave it ticked.
3. Press **Save**.

**What happens when you save.** TrackHub creates the account and then creates that first user inside it **with the Manager role**. That person is the account's administrator: they can run everything inside their own account but nothing across accounts. They are *not* a platform Administrator, and they will not see System Admin.

Only **Name** and **Type** are enforced before the dialog will save, but if you leave the user fields blank the account is created with nobody who can sign in to it. Always fill them in.

**The starting status** comes from the **Active** checkbox: ticked gives you an **Active** account, unticked gives you a **Suspended** one. The **Trial** status is not something you can pick here or move an account into afterwards.

To add more users to an account without leaving this screen, use the **Add User** action on the account's row. This creates the user only — see [Setting up your account](topic:admin-account-setup) for why that is not the whole job.

## 4. Manage an account's lifecycle

Every account carries a **Status**, shown as a coloured badge on its row. Use the **Change Status** action to move it.

| Status | Can its people use TrackHub? |
|---|---|
| **Trial** | Yes |
| **Active** | Yes |
| **Suspended** | No |
| **Cancelled** | No |
| **Archived** | No |

The **New Status** dropdown only offers moves that are allowed from where the account is now:

| From | You can move it to |
|---|---|
| **Trial** | Active, Suspended, Cancelled |
| **Active** | Suspended, Cancelled |
| **Suspended** | Active, Cancelled, Archived |
| **Cancelled** | Active, Archived |
| **Archived** | *Nothing — Archived is final* |

Two consequences worth knowing:

- **Nothing returns to Trial.** Once an account leaves Trial it cannot go back.
- **You cannot archive directly.** An Active account has to be suspended or cancelled first. And because Archived is a dead end, the **Change Status** action is not even shown on an archived account's row.

A **Reason** is **required** when you move an account to **Suspended** or **Cancelled**. For the other moves it is optional, but it is worth filling in anyway — it is what you will read when someone asks why.

**What suspending does to the customer.** It takes effect immediately. Everyone in that account — Managers included — is stopped at a single **Account Not Available** screen reading *"This account is not currently operational. Please contact support for assistance."*, with the current status underneath. No map, no reports, no management screens. Nothing is deleted; returning the account to **Active** restores everything.

## 5. Switch on the account's features

A new account starts with **no features**. Until you provision them, its Geofences menu is missing, its notification screens are absent, and its GPS reports do not exist. This step is not optional.

1. Open the **Account Features** section.
2. Click the **+** icon (**Add Feature to Account**).
3. Choose the **Account** and the **Feature**.
4. Set **Enabled**, a **Tier** (a plan label, `default` is fine), and — for the two features that have one — the setting value:
   - **Storing Interval (Seconds)** for GPS Integration, default **360**.
   - **Retention Days** for GPS Position History, default **30**.
5. Press **Save**, and repeat for each feature the customer is entitled to.

Editing an existing row keeps the account and feature fixed and lets you change the rest.

For a typical tracking customer, start with **GPS Integration** and **GPS Position History**, then add whatever the subscription includes. What every feature does, and exactly what disappears when one is off, is set out in the [Feature catalogue](topic:feature-catalog).

## 6. Platform reference data

These sections are shared by every account. In most installations they are correct as installed and you should leave them alone.

- **Unit Types** — the categories of tracked unit (car, boat, drone, and so on) together with the thresholds that decide when a unit counts as stopped: **Stopped Gap (Minutes)**, **Max Time Gap (Minutes)**, **Max Distance (Km)**, and **ACC Based**. You can **Edit** these thresholds; you cannot add or remove unit types, because the catalogue of types is fixed.
- **Geocoding Providers** — the service that turns coordinates into street addresses. Only one is active at a time. If addresses are not appearing on the map, this is where to look.
- **API Clients** and **Service Client Permissions** — how other systems connect to TrackHub, and what they are allowed to do. Only touch these when you are wiring up an integration; they are not part of onboarding a normal customer.
- **Roles** and **Policies** — the permission matrix behind Administrator, Manager, and User. The shipped matrix is what the rest of this documentation assumes. Changing it can hide screens from people in ways that are hard to trace back, so leave it as installed unless you have a specific reason.

## 7. Support Grants

A platform Administrator cannot simply browse into a customer's data. To work inside one account for troubleshooting, raise a **Support Grant**: choose the **Account**, the **Support User Id**, a **Reason**, a **Ticket Reference**, an **Access Level** (`read` by default), and a start and end time. The grant then has to be **approved**, and it can be **revoked** at any time.

Grants are time-boxed and logged on purpose. Use them instead of borrowing a customer's credentials, and revoke them when the ticket closes.

## 8. Hand over to the account's manager

The account exists, it is Active, and its features are on. What is left belongs to the person you created as its Manager. Tell them:

- Which features their account has, and that changing them is a billing decision that comes back to you.
- That they should change their password on first sign-in, exactly as you did in step 1.
- That their setup order matters, and it is written down for them in [Setting up your account](topic:admin-account-setup).
- That **creating a user does not give that user a role**, and that **a plain User with no group sees an empty map**. These are the two things new managers get wrong.

## Checklist

- [ ] Administrator password changed.
- [ ] Geocoding provider configured and active.
- [ ] Account created, with its first user (the account Manager).
- [ ] Account status is **Active**.
- [ ] Every entitled feature switched on, with its setting values.
- [ ] Manager handed the account and pointed at their setup guide.

For the health of the platform itself — which services are up, and any announcements — see [Platform status](topic:platform-status).
