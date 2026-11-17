---
id: troubleshooting
title: Troubleshooting
description: Quick, symptom-first fixes for sign-in trouble, missing features, blocked accounts, error messages, and units that won't move.
category: troubleshooting
screens: []
related: [getting-started, gps-integration, reports]
tags: [troubleshooting, errors, sign in, account, gps]
order: 10
---

# Troubleshooting

Quick, symptom-first fixes for the problems administrators and operators run into most. Find your symptom below and follow the steps. If a fix points at a setting you cannot change, note the exact message and contact your administrator (or, for platform-wide issues, the platform support team).

## I can't sign in

Sign-in problems have a few distinct causes. Work through them in order:

- **Wrong username or password.** Re-enter your credentials carefully (check Caps Lock). Passwords always contain at least **8 characters**, including **an uppercase letter, a lowercase letter and a number**.
- **Your account is locked.** After repeated failed sign-ins a user can be locked out. A Manager can clear this from **Account Management → Users**: a locked user shows an **Unlock** button in the **Status** column (see [Users, roles, and groups](topic:users-roles-groups#users)). Once unlocked, try again.
- **Too many attempts in a row (cooldown).** After an interrupted sign-in the **Authentication Failed** page makes you wait for a short countdown (about 30 seconds) before it will start another attempt. If nothing happens when you retry, wait half a minute and try once more.
- **Your session expired.** If you were signed in and suddenly get sent back to sign in, your session simply timed out. Sign in again. If it keeps happening immediately after signing in, your session could not be refreshed — wait a moment and retry.
- **"Authentication Failed" error page.** If sign-in is interrupted you may land on a full-screen **Authentication Failed** page. It lists the likely causes (the authentication service being temporarily unavailable, network connectivity issues, or an invalid or expired authentication request). Wait for the **Retry in Ns** countdown to finish, then press **Retry Authentication**, or press **Go Back**. Specific variants:
  - A message about failing to exchange the authorization code for an access token means the authentication service was unreachable — retry shortly.
  - A message about no authorization code being received means the sign-in was interrupted or you used an old link — start a fresh sign-in rather than reusing a bookmarked callback URL. Retrying from this page starts a clean attempt.
  - A message that the sign-in attempt could not be verified usually means you signed in from a bookmarked or old sign-in page, or your browser blocks site storage (for example in private mode). Open the portal home page directly and try again.

If retries keep failing across several minutes, the authentication service may be down — contact your administrator.

## My account is not operational

If you see a full-screen **Account Not Available** message — "This account is not currently operational. Please contact support for assistance." — with a **Status** shown at the bottom, your whole account has been put into a non-operational state (Suspended, Cancelled, or Archived).

Only **Trial** and **Active** accounts can use the app. This is not something you can fix from inside the portal: **contact your administrator** (or the platform team) to have the account reinstated. Platform super administrators change account status from **System Admin → Account → Change Status** (see [System administration](topic:system-administration)). For the full picture, see [Roles and permissions](topic:roles-and-permissions#account-status-what-a-suspended-account-looks-like).

## A feature is missing from my menu

A section or menu item you expected is not there. Two common reasons:

- **Your role or policies don't include it.** Access to screens and actions is controlled by roles and policies. Ask an administrator to review your assignments — see [Roles and permissions](topic:roles-and-permissions).
- **The feature isn't enabled for your account.** Optional features are switched on per account. When **Geofencing** or **Trip Management** is off, the whole **Geofences** or **Trips** menu item disappears. Other features (Documents, Public Links, Notifications, and so on) hide their **section or buttons** inside the relevant screen rather than a menu item. You can see which features are on under **Account Management → Account Features** (view only). Only the platform administrator can switch one on.

## "This feature is not enabled for your account"

If an action shows the message **"This feature is not enabled for your account."**, the underlying feature is switched off for your account. Nothing is broken. Ask your administrator to enable the feature (platform administrators do this in **System Admin → Account Features**). See [System administration](topic:system-administration).

## Red error messages (toasts)

A red banner that appears briefly at the top-centre of the screen and fades after a few seconds is an error notification. Most are **transient server errors** — the action didn't go through this time.

- **Retry the action.** Many of these clear on a second attempt.
- If the message is **"This feature is not enabled for your account"** or **"This account is not currently operational"**, see the dedicated sections above — retrying won't help.
- If an operation keeps failing after several retries, note the exact wording and roughly when it happened, then **escalate to your administrator**. Repeated failures across different actions usually point to a service or connectivity problem rather than something you did.

## My vehicles don't move on the map

If units show old positions or don't update, the problem is almost always in the GPS integration chain. Check, in order, on the **GPS Integration** page (see [GPS integration](topic:gps-integration)):

- **Is the operator enabled?** A disabled operator stops pulling data and shows health **Disabled**. Re-enable it.
- **Are the credentials working?** Use **Test Credential** on the operator. A failed test means the connection to the provider is broken (see below).
- **Is the operator healthy?** On the dashboard, check whether operators are **Degraded** or **Offline**, and look at **Recent Sync Runs** for **Failed** results and error codes, and at **Open GPS Alerts**.
- **Is the device still assigned to the unit?** If a device's assignment was ended, its unit stops receiving positions. Check **Device Assignments** — the link should be **Active**, not **Ended**. Re-assign the device if needed.
- **Trigger a sync** to force an update, and read the message: *"the provider returned no devices"* usually means the credential reaches the wrong provider account.
- **Can you see the unit at all?** A plain User only sees units in their groups. If a Manager sees it and you do not, ask to have the unit added to one of your groups.

## A connectivity test fails

When **Test Credential** reports **"An error occurred while testing the credential"**, TrackHub could not reach the provider with the stored details. Check:

- The **URL** in the operator's **Credential** dialog is correct.
- The other fields (username, password, API key, and so on — the dialog names them the way your provider does) match what the provider expects and haven't expired or been rotated.
- The operator's **Protocol** is really your provider. The wrong protocol looks exactly like bad credentials.
- The provider itself is online.

Correct the details in the **Credential** dialog, save, and test again. See [GPS integration](topic:gps-integration).

## My report is empty or too large

- **Empty results.** The most common cause is the **filters** — the date range, unit selection, or other criteria matched nothing. Widen the **From**/**To** window, pick the correct unit(s), and generate again. Also confirm the units actually reported data in that period.
- **Too much data.** A very wide date range or "all units" can produce a very large result. Some reports offer a **Max Rows** filter that caps how many rows are returned — if a report looks truncated, raise **Max Rows**; if it is unwieldy, lower it or narrow the **From**/**To** window and select fewer units.

For how to run reports, see [Reports](topic:reports).

## I saved something but the dialog stayed open

When a create/edit dialog stays open after you press **Save**, the save did **not** succeed — an error was shown (usually a red toast at the top of the screen). The dialog is deliberately kept open with your entries intact so you don't have to retype them.

- Read the error message, fix the cause (for example, a missing required field, a validation error shown under a field, or a transient server error), and press **Save** again.
- Required fields are marked and will show a message if left blank; correct those first.
