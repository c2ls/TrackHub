---
id: users-roles-groups
title: Users, roles, and groups
description: Add the people who sign in to your account, assign roles and policies, and organise units and users into groups.
category: administration
screens: [manageAdmin]
related: [roles-and-permissions, management-overview]
tags: [users, roles, policies, groups, permissions]
order: 20
---

# Users, roles, and groups

These sections live on the [Account Management](topic:management-overview) screen, which only **Managers** can open. **Users**, **Roles**, and **Policies** sit under the **Users & Access** group; **Groups** sits under the **Fleet & Tracking** group. Expand a section to work with it.

## Users

The **Users** section manages the people who can sign in to your account.

Columns: **Username** (each row shows the person's email with their username beneath), **First Name**, **Last Name**, **Status**, **Action**, and **Password**.

To add a user, expand the section and click the **+** (add) icon. The **User Details** dialog collects:

- **Email Address** — required.
- **Password** — required, and shown only when you are creating a new user.
- **Username** — required.
- **First Name** (required), **Second Name**, **Last Name** (required), **Second Surname**.
- **Date of Birth**.
- **Active** and **Integration User** checkboxes.

Press **Save**. A new password must meet your account's password policy (the server rejects passwords that are too weak — typically at least 8 characters with an uppercase letter, a lowercase letter, and a number).

Row actions:

- **Edit** — reopens the **User Details** dialog to change the person's details. The password field is hidden when editing.
- **Delete** — removes the user (you are asked to confirm).
- **Password** — the link in the **Password** column opens the **Update Password** dialog; type a new password and **Save**.
- **Status** — a user who is not locked shows **Active**. When a user is locked out (for example after too many failed sign-ins) the column shows an **Unlock** button; click it to unlock the account immediately.

Which roles and policies a user holds are assigned in the **Roles** and **Policies** sections, not in this dialog.

## Roles

Roles group permissions together. Your account's built-in roles are **Manager**, **User**, and **Administrator**.

The section lists each role with a **Roles** column and an **Action** column. Roles are not created or deleted here — you only assign users to them.

Click **Assign** on a role to open the **Assign Role** dialog:

- Pick a **User** from the dropdown to add them to the role.
- The table lists the users already in the role by **Username**; select one or more rows to remove them.
- Close the dialog when you are finished.

For what each role can do, see [Roles and permissions](topic:roles-and-permissions).

## Policies

Policies are finer-grained permission sets: **Full Access**, **Manage Users**, **Read Only**, **Limited Update**, and **Audit**. This section controls which users hold each policy.

The section lists each policy with a **Policies** column and an **Action** column. Click **Assign** to open the **Assign Policy** dialog, which works exactly like the role dialog: add a user from the dropdown, or select listed users and remove them, then close.

See [Roles and permissions](topic:roles-and-permissions) for how roles and policies combine.

## Groups

Groups let you organise units and users together — for example by branch or region.

Columns: **Groups** (name), **Description**, **Action**, **Users**, and **Units**.

To add a group, expand the section and click the **+** (add) icon. The **Group Details** dialog asks for a **Name** (required), a **Description** (required), and an **Active** checkbox. Press **Save**.

Row actions:

- **Edit** and **Delete** in the **Action** column change or remove the group (deletion asks you to confirm).
- **Assign** in the **Users** column opens the **Assign User** dialog: add users to the group from the dropdown, or select listed users and remove them.
- **Assign** in the **Units** column opens the **Assign Unit** dialog: add units to the group from the dropdown, or select listed units and remove them.
