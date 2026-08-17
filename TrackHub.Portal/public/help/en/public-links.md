---
id: public-links
title: Public links
description: Grant limited, time-boxed, login-free access to a resource with a share token you can revoke at any time.
category: administration
screens: [manageAdmin]
related: [management-overview, dashboard-live-map]
tags: [public links, share token, revoke, access, expiry]
order: 50
---

# Public links

Public links (share tokens) let you grant limited, time-boxed access to a specific resource **without a login**. You manage them in the **Public Links** section of the **Account Management** screen, under the **Documents & Sharing** group.

## The list

Each existing link is shown with these columns:

| Column | Meaning |
|---|---|
| **Resource** | The resource the link points at (its type and id). |
| **Scopes (comma separated)** | The scopes the token is allowed to use. |
| **Expires At** | When the link stops working. |
| **Access Count** | How many times the link has been used. |
| **Status** | **Active**, or **Revoked At** once the link has been revoked. |
| **Action** | **Revoke Public Link**, shown while the link is still active. |

## Creating a link

Press the **+** (add) icon on the section heading to open the **Create Public Link** dialog, and fill in:

- **Resource Type** (required)
- **Resource Id** (required)
- **Scopes (comma separated)** (required)
- **Purpose**
- **Expires At** (required)

Press **Save**. The portal then shows the **Public Link Token** once, with the warning **"Copy this token now. It will not be shown again."** Copy it immediately — the token cannot be retrieved later. Close the dialog when you have it.

> The **+** icon appears only when the public-links feature is enabled for your account. Even when it is not, you can still see, monitor and **revoke** existing links — only creating new ones is gated.

## Revoking a link

To stop an active link, press **Revoke Public Link** on its row. The link is disabled immediately, its **Status** shows as revoked, and it cannot be used again — there is no way to un-revoke it, so issue a fresh link if access is still needed.

## Related sharing

Document shares use the same public-link mechanism behind the scenes: sharing a file from the documents panel mints a link scoped to that document. See [Documents](topic:documents). Public links are also how you can expose a limited live view without handing out an account; for what your fleet sees on the map, see [Live map](topic:dashboard-live-map).

## Who can use this

Public Links lives inside **Account Management**, which is available to Managers with the matching permissions. See [Management overview](topic:management-overview).
