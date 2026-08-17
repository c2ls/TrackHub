---
id: drivers-workforce
title: Drivers and workforce
description: Register drivers, give them mobile credentials and devices, track licences and other qualifications, and keep a time-bounded record of who drove which unit.
category: administration
screens: [manageAdmin]
related: [management-overview, units-devices, documents, alerts-notifications, feature-catalog]
tags: [drivers, workforce, credentials, devices, qualifications, licences, assignments, expirations]
order: 35
---

# Drivers and workforce

A **driver** is an operational person — someone who drives your units. Drivers are not portal users: they never sign in to the web portal, they have their own credentials for the driver mobile app, and they are managed entirely from the **Fleet & Tracking** group of the **Account Management** screen.

The area has five collapsible sections, in this order:

| Section | What it is for |
|---|---|
| **Drivers** | The registry — who exists, their document, employee code, and primary licence. |
| **Driver Credentials & Devices** | The sign-in credential each driver uses in the mobile app, and the devices it is enrolled on. |
| **Driver Qualifications** | Licences, medical exams, training, background checks, and HAZMAT permits, with expiry dates. |
| **Driver Assignments** | Time-bounded records of which driver operated which unit, and when. |
| **Qualification Expirations (30 days)** | An account-wide list of everything about to lapse. |

## What needs the Workforce feature

The first two sections are **core platform**. They are always there for an administrator with the right permissions, on every account, whatever the subscription says. You can always register drivers, and you can always issue, lock, reset, and revoke their mobile credentials.

The last three sections — **Driver Qualifications**, **Driver Assignments**, and **Qualification Expirations** — belong to the billable **Workforce** feature. Without it they are simply not on the screen, expiration alerts are not raised, and the three workforce reports are absent from the report catalogue. See the [Feature catalogue](topic:feature-catalog).

## Drivers

The registry itself. Columns are **Name**, **Phone**, **Document**, **Active**, and an action column.

Press the **+** (add) icon to open the **Create Driver** dialog:

- **Name** (required) — the driver's full name.
- **Phone** — digits, spaces, brackets, dots, hyphens, and an optional leading `+`, at least five characters. Anything else is rejected with **"Invalid phone number."**
- **Document Type** and **Document Number** — the identity document. The number is unique within your account and is never matched across accounts.
- **Employee Code** — your own payroll or HR reference.
- **License Number** and **License Expires At** — the primary licence, shown as a summary on the driver record. On a *new* driver the expiry date cannot already be in the past (**"The license expiration date is already past."**); when editing an existing driver it can, so a lapsed licence can be recorded and corrected.
- **Default Transporter** — the unit this driver normally operates, picked from your account's units. This is a *default*, not an assignment; see below.
- **Active** — untick to take the driver out of use.

Row actions are **Edit** and **Deactivate Driver** (you are asked to confirm). Deactivating does not delete anything, and an inactive driver cannot be given a new assignment.

## Driver Credentials & Devices

This section administers how a driver signs in to the **driver mobile app**. Pick a driver from the **Driver** selector at the top; the section then shows that driver's credential and the devices enrolled on it.

A driver has at most one credential. Press **Create Credential** and set a **Login** and a **Password**. The credential starts **Pending Activation**, and the driver cannot sign in until it is activated.

| Column | Meaning |
|---|---|
| **Login** | The name the driver types in the app. |
| **Status** | Pending Activation, Active, Locked, or Revoked. |
| **Failed Attempts** | Consecutive failed sign-ins. The counter resets on a successful sign-in. |
| **Locked Until** | When an automatic or manual lock expires. |
| **Last Login** | The last successful sign-in. |
| **Password Reset Required** | The driver must set a new password at next sign-in. |

Row actions:

- **Activate** — opens **Driver Activation** and makes a pending credential usable.
- **Lock** — blocks sign-in until a **Locked Until** moment you choose (**"The driver cannot sign in until this moment."**). TrackHub also locks a credential by itself after **five** consecutive failed attempts, for **15 minutes**.
- **Reset Password** — sets a new password, optionally forcing the driver to change it again at next sign-in.
- **Revoke** — permanent: **"the driver will no longer be able to sign in and existing sessions stop refreshing."** A revoked credential also kills the app sessions already running, because the app's token renewal re-checks the credential every time.

### Registered Devices

Under the credential sits the list of devices the driver has enrolled from the app: **Device Name**, **Platform**, **App Version**, **Push Token (masked)**, **Last Seen**, and **Status**. **Revoke Device Registration** removes one device — the driver must enrol it again from the app; their credential is untouched.

Push tokens are shown masked, and the internal session identifiers are never sent to the portal at all. There is no way to reveal either, by design.

## Driver Qualifications

*Requires the Workforce feature.*

One qualification record covers everything credentialed about a driver, not just the driving licence. Pick a driver, then use **+** to open **Create Qualification**:

- **Type** (required) — **License**, **Medical Exam**, **Training**, **Background Check**, **HAZMAT Permit**, or **Other**.
- **Category** — the sub-class where the type has one, for example a licence category.
- **Number** — the document or certificate number.
- **Issued At** and **Expires At** — a qualification cannot expire before it was issued.
- **Issuing Authority** — who granted it.
- **Status** (required) — **Valid**, **Expired**, or **Revoked**. Expiry is worked out from **Expires At** on its own, so the status you normally set by hand is **Revoked**, for a qualification that was withdrawn before its date.
- **Linked Document** — the identifier of the supporting document (the licence scan, the medical certificate). Store the file itself under [Documents](topic:documents) and reference it here.
- **Notes** — free text.

The **Expires At** column is colour-coded as the date approaches. Row actions are **Edit** and **Delete**; deletion is permanent, and only the audit trail keeps the record afterwards.

The **License Number** / **License Expires At** fields on the driver record stay as the at-a-glance summary. The full set of licences and certificates lives here.

## Driver Assignments

*Requires the Workforce feature.*

An assignment is a **time-bounded record that a driver operated a unit**. Unlike **Default Transporter**, which is a single pre-selection that changes whenever you edit it, assignments accumulate into a history you can look back through.

To create one, pick the **Driver**, the **Unit**, and an **Assignment Type** (**Regular** or **Temporary**), then press **Assign Driver**.

Two tables follow. **Active Assignments** shows what is open now, with an **End Assignment** action. **Assignment History** shows everything, filtered by driver, unit, **From**, and **To** — press **Search** to run the filters. Columns in both are **Driver**, **Unit**, **Starts At**, **Ends At**, **Assignment Type**, **Status** (Active, Ended, or Cancelled), and **Created By**.

The rules worth knowing:

- **One open assignment per driver-and-unit pair.** Assigning the same driver to the same unit again while the first is still open is refused as a conflict. End the first one, then create the new one.
- **A driver can hold several open assignments at once**, as long as they are to *different* units.
- **Ended assignments are immutable.** **End Assignment** warns you: *"Ended assignments cannot be modified."* If you end one by mistake, create a new assignment — you cannot re-open it.
- An **inactive driver** cannot be given a new assignment.
- If your account has **Block assignment when the driver's license is expired** switched on (see the [Feature catalogue](topic:feature-catalog)), assigning a driver whose **License** qualification is expired or revoked is rejected with a validation message. Accounts without that setting are unaffected.

## Qualification Expirations (30 days)

*Requires the Workforce feature.*

An account-wide view of every qualification expiring within the next 30 days, whichever driver it belongs to. Columns: **Driver**, **Type**, **Category**, **Number**, **Expires At**, **Days Left**, and **Status**. **Days Left** is colour-coded and reads **Expired** once the date has passed.

This is the screen to check weekly. The reporting equivalent, which you can export, is the **Qualification Expirations** report.

## Expiration alerts

*Requires the Workforce feature.*

Once a day TrackHub scans the qualifications of every Workforce-enabled account and raises an alert event as each one crosses **30**, **15**, **7**, and **0** days to expiry. The first three raise **Driver Qualification Expiring**; the day it lapses raises **Driver Qualification Expired**, at a higher severity.

Each threshold fires **exactly once** per qualification, ever — so you get four reminders spread out over the last month of a licence's life, not four every day. Extending the expiry date creates a fresh set of thresholds to cross.

The events land in **Alert Events** like any other, and a notification rule can route them to email, WhatsApp, or a webhook. See [Alerts and notifications](topic:alerts-notifications).

## Workforce reports

*Requires the Workforce feature.*

Three reports sit in the **Workforce** category of the report catalogue:

- **Driver Registry** — the complete driver list with licence, employee code, and default unit. No filters; Excel only.
- **Qualification Expirations** — qualifications expiring within a **Within Days** window (30 by default). Excel or PDF.
- **Driver Assignment History** — assignments over a **From** / **To** range. Excel only.

Driver data is personal data, so these exports are recorded in the audit trail at high severity. See [Reports](topic:reports).

## Who can use this

All five sections live inside **Account Management**, which is available to Managers. A section, a **+** button, or a row action can also be missing because your role does not carry the matching driver permission — read, write, edit, and delete are granted separately. See [Roles and permissions](topic:roles-and-permissions) and [Account Management](topic:management-overview).

If the qualification, assignment, or expiration sections are missing but the driver registry is there, that is the Workforce feature, not your permissions.
