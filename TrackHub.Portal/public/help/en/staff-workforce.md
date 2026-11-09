---
id: staff-workforce
title: Staff and workforce
description: Register staff, give them mobile credentials and devices, track licences and other qualifications, and keep a time-bounded record of which staff member used which unit.
category: administration
screens: [manageAdmin]
related: [management-overview, units-devices, documents, alerts-notifications, feature-catalog]
tags: [staff, workforce, credentials, devices, qualifications, licences, assignments, expirations]
order: 35
---

# Staff and workforce

A **staff member** is anyone who works for your account — a driver, an operator, a mechanic, a salesperson, a supervisor. A staff member is tracked through a unit: register the device they carry as a unit with a name that says who it is (for example *Sales Representative North*) and link the staff member to it with **Default Transporter** or an assignment. The dashboard shows units, so that unit is what you follow on the map. Staff are not portal users: they never sign in to the web portal, they have their own credentials for the staff mobile app, and they are managed entirely from the **Fleet & Tracking** group of the **Account Management** screen.

The area has four collapsible sections, in this order:

| Section | What it is for |
|---|---|
| **Staff** | The registry — who exists, their document, employee code, and primary licence. Each row also opens the staff member's mobile-app credential and enrolled devices. |
| **Staff Qualifications** | Licences, medical exams, training, background checks, and HAZMAT permits, with expiry dates. |
| **Staff Assignments** | Time-bounded records of which staff member operated which unit, and when. |
| **Qualification Expirations (30 days)** | An account-wide list of everything about to lapse. |

## What needs the Workforce feature

The **Staff** registry and its **Credentials and devices** are **core platform**. They are always there for an administrator with the right permissions, on every account, whatever the subscription says. You can always register staff, and you can always issue, lock, reset, and revoke their mobile credentials.

The last three sections — **Staff Qualifications**, **Staff Assignments**, and **Qualification Expirations** — belong to the billable **Workforce** feature. Without it they are simply not on the screen, expiration alerts are not raised, and the three workforce reports are absent from the report catalogue. See the [Feature catalogue](topic:feature-catalog).

## Staff

The registry itself. Columns are **Name**, **Staff type**, **Document**, **Phone**, **Credential**, **Active**, and an action column. The **All types** selector beside the search box narrows the list to one staff type.

Press the **+** (add) icon to open the **Create Staff Member** dialog:

- **Name** (required) — the staff member's full name.
- **Staff type** (required) — what the person does: **Driver**, **Operator**, **Driver assistant**, **Mechanic**, **Supervisor**, **Administrative** or **Other**. Existing records are **Driver**. The type decides which parts of the staff mobile app the person sees; people without a vehicle are never asked to pick one.
- **Phone** — digits, spaces, brackets, dots, hyphens, and an optional leading `+`, at least five characters. Anything else is rejected with **"Invalid phone number."**
- **Document Type** and **Document Number** — the identity document. The number is unique within your account and is never matched across accounts.
- **Employee Code** — your own payroll or HR reference.
- **License Number** and **License Expires At** — the primary licence, shown as a summary on the staff record. On a *new* staff the expiry date cannot already be in the past (**"The license expiration date is already past."**); when editing an existing staff it can, so a lapsed licence can be recorded and corrected.
- **Default Transporter** — the unit this staff member normally operates, picked from your account's units. This is a *default*, not an assignment; see below.
- **Active** — untick to take the staff out of use.

Row actions are **Edit**, **Credentials and devices** (see below), and **Deactivate Staff Member** (you are asked to confirm). Deactivating does not delete anything, and an inactive staff cannot be given a new assignment.

## Staff Credentials & Devices

This is how a staff member signs in to the **staff mobile app**. Open it with the **Credentials and devices** action on the staff member's row; the dialog shows that staff member's credential and the devices enrolled on it.

A staff member has at most one credential. Press **Create Credential** and set a **Login** and a **Password**. The credential starts **Pending Activation**, and the staff cannot sign in until it is activated.

| Column | Meaning |
|---|---|
| **Login** | The name the staff types in the app. |
| **Status** | Pending Activation, Active, Locked, or Revoked. |
| **Failed Attempts** | Consecutive failed sign-ins. The counter resets on a successful sign-in. |
| **Locked Until** | When an automatic or manual lock expires. |
| **Last Login** | The last successful sign-in. |
| **Password Reset Required** | The staff must set a new password at next sign-in. |

Row actions:

- **Activate** — opens **Staff Activation** and makes a pending credential usable.
- **Lock** — blocks sign-in until a **Locked Until** moment you choose (**"The staff cannot sign in until this moment."**). TrackHub also locks a credential by itself after **five** consecutive failed attempts, for **15 minutes**.
- **Reset Password** — sets a new password, optionally forcing the staff to change it again at next sign-in.
- **Revoke** — permanent: **"the staff will no longer be able to sign in and existing sessions stop refreshing."** A revoked credential also kills the app sessions already running, because the app's token renewal re-checks the credential every time.

### Registered Devices

Under the credential sits the list of devices the staff has enrolled from the app: **Device Name**, **Platform**, **App Version**, **Push Token (masked)**, **Last Seen**, and **Status**. **Revoke Device Registration** removes one device — the staff must enrol it again from the app; their credential is untouched.

Push tokens are shown masked, and the internal session identifiers are never sent to the portal at all. There is no way to reveal either, by design.

## Staff Qualifications

*Requires the Workforce feature.*

One qualification record covers everything credentialed about a staff member, not just the driving licence. Pick a staff member, then use **+** to open **Create Qualification**:

- **Type** (required) — **License**, **Medical Exam**, **Training**, **Background Check**, **HAZMAT Permit**, or **Other**.
- **Category** — the sub-class where the type has one, for example a licence category.
- **Number** — the document or certificate number.
- **Issued At** and **Expires At** — a qualification cannot expire before it was issued.
- **Issuing Authority** — who granted it.
- **Status** (required) — **Valid**, **Expired**, or **Revoked**. Expiry is worked out from **Expires At** on its own, so the status you normally set by hand is **Revoked**, for a qualification that was withdrawn before its date.
- **Linked Document** — the identifier of the supporting document (the licence scan, the medical certificate). Store the file itself under [Documents](topic:documents) and reference it here.
- **Notes** — free text.

The **Expires At** column is colour-coded as the date approaches. Row actions are **Edit** and **Delete**; deletion is permanent, and only the audit trail keeps the record afterwards.

The **License Number** / **License Expires At** fields on the staff record stay as the at-a-glance summary. The full set of licences and certificates lives here.

## Staff Assignments

*Requires the Workforce feature.*

An assignment is a **time-bounded record that a staff member operated a unit**. Unlike **Default Transporter**, which is a single pre-selection that changes whenever you edit it, assignments accumulate into a history you can look back through.

To create one, pick the **Staff**, the **Unit**, and an **Assignment Type** (**Regular** or **Temporary**), then press **Assign Staff Member**.

Two tables follow. **Active Assignments** shows what is open now, with an **End Assignment** action. **Assignment History** shows everything, filtered by staff, unit, **From**, and **To** — press **Search** to run the filters. Columns in both are **Staff**, **Unit**, **Starts At**, **Ends At**, **Assignment Type**, **Status** (Active, Ended, or Cancelled), and **Created By**.

The rules worth knowing:

- **One open assignment per staff-and-unit pair.** Assigning the same staff to the same unit again while the first is still open is refused as a conflict. End the first one, then create the new one.
- **A staff member can hold several open assignments at once**, as long as they are to *different* units.
- **Ended assignments are immutable.** **End Assignment** warns you: *"Ended assignments cannot be modified."* If you end one by mistake, create a new assignment — you cannot re-open it.
- An **inactive staff** cannot be given a new assignment.
- If your account has **Block assignment when the staff member's license is expired** switched on (see the [Feature catalogue](topic:feature-catalog)), assigning a staff member whose **License** qualification is expired or revoked is rejected with a validation message. Accounts without that setting are unaffected.

## Qualification Expirations (30 days)

*Requires the Workforce feature.*

An account-wide view of every qualification expiring within the next 30 days, whichever staff it belongs to. Columns: **Staff**, **Type**, **Category**, **Number**, **Expires At**, **Days Left**, and **Status**. **Days Left** is colour-coded and reads **Expired** once the date has passed.

This is the screen to check weekly. The reporting equivalent, which you can export, is the **Qualification Expirations** report.

## Expiration alerts

*Requires the Workforce feature.*

Once a day TrackHub scans the qualifications of every Workforce-enabled account and raises an alert event as each one crosses **30**, **15**, **7**, and **0** days to expiry. The first three raise **Staff Qualification Expiring**; the day it lapses raises **Staff Qualification Expired**, at a higher severity.

Each threshold fires **exactly once** per qualification, ever — so you get four reminders spread out over the last month of a licence's life, not four every day. Extending the expiry date creates a fresh set of thresholds to cross.

The events land in **Alert Events** like any other, and a notification rule can route them to email, WhatsApp, or a webhook. See [Alerts and notifications](topic:alerts-notifications).

## Workforce reports

*Requires the Workforce feature.*

Three reports sit in the **Workforce** category of the report catalogue:

- **Staff Registry** — the complete staff list with licence, employee code, and default unit. No filters; Excel only.
- **Qualification Expirations** — qualifications expiring within a **Within Days** window (30 by default). Excel or PDF.
- **Staff Assignment History** — assignments over a **From** / **To** range. Excel only.

Staff data is personal data, so these exports are recorded in the audit trail at high severity. See [Reports](topic:reports).

## Who can use this

Every section lives inside **Account Management**, which is available to Managers. A section, a **+** button, or a row action can also be missing because your role does not carry the matching staff permission — read, write, edit, and delete are granted separately. See [Roles and permissions](topic:roles-and-permissions) and [Account Management](topic:management-overview).

If the qualification, assignment, or expiration sections are missing but the staff registry is there, that is the Workforce feature, not your permissions.

**Staff Assignments** also needs the staff operations permission (read to see it, write to assign, edit to end an assignment).
