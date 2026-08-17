---
id: documents
title: Documents
description: Store, classify, virus-scan, version and share your account's documents from the Account Management screen.
category: administration
screens: [manageAdmin]
related: [management-overview, units-devices, drivers-workforce]
tags: [documents, files, upload, versions, expiry, virus scan]
order: 40
---

# Documents

The **Documents** area of the **Account Management** screen lets you store files, keep them classified and versioned, watch for expiry, and share them safely. You find it under the **Documents & Sharing** group of the page.

The area has three collapsible sections — **Document Library**, **Expiring Documents** and **Document Types** — plus an embedded documents panel that appears on the detail of the thing a document belongs to (for example a unit or a driver).

## Document Library

An account-wide search across your documents.

- Enter a **Type** (category) and/or a **Status** in the two filter fields, then press the search (magnifier) button.
- Results table:

| Column | Meaning |
|---|---|
| **File Name** | The document title, or the original file name. |
| **Owner** | The record the document belongs to (entity type and id). |
| **Type** | The document category. |
| **Classification** | Public, Internal, Confidential or Legal. |
| **Status** | Active, Pending, Uploaded, Expired, Replaced, Voided or Deleted. |
| **Scan Status** | The virus-scan result (see below). |
| **Action** | A **Download** button, shown only when the file is available. |

## Expiring Documents

A dashboard of documents nearing expiry. Columns: **Type**, **Owner**, **Expires At** and **Status**. Use it to catch licences, permits and certificates before they lapse.

> Driver licences and certificates are tracked separately, on the driver’s qualification records, which have their own expiry list and their own alerts. Upload the scan here and reference it from the qualification’s **Linked Document** field. See [Drivers and workforce](topic:drivers-workforce).

## Document Types

Configure the categories your account uses.

- Columns: **Type**, **Required**, **Tracks Expiration**, **Default Validity (days)**, **Status** and an action.
- The **+** (add) icon opens the **Document Types** dialog, where you set:
  - **Type** (the category identifier, required)
  - **Title** (the display name)
  - **Default Validity (days)**
  - **Required** — tick if every relevant record must hold this type
  - **Tracks Expiration** — tick if documents of this type have an expiry date
- Press **Save**.
- The block action on a row disables that type (you are asked to confirm first).

> The **+** icon on **Document Types** appears only when the documents feature is enabled for your account. If it is missing, the feature is not provisioned — the platform team manages that in [System administration](topic:system-administration).

## Uploading, versions and sharing

Documents are normally added from the detail panel of the record they belong to, using an embedded documents panel. That panel lists the record's documents with **File Name**, **Type**, **Classification**, **Status**, **Scan Status**, **Expires At** and **Version** columns, and offers these actions:

- **Upload Document** — press the button, then drag a file onto the drop area (**"Drag and drop a file here, or click to select"**) or click to browse. Choose a **Type** (category, required), a **Classification** (Public, Internal, Confidential or Legal — Internal by default), an optional **Title** and an optional **Expires At** date. Press **Save**.
- **New Version** — upload a replacement file for an existing document. You can add a **Reason**; the **Version** column then tracks the current version number.
- **Download** — download the current file (only when the scan passed).
- **Share** — create a public share link for the document. Set a **Purpose** and an **Expires At**, then **Save**. The share URL is shown **once**, with the warning **"Copy this link now. The token will not be shown again."** — copy it immediately. Shared links are the same mechanism described in [Public links](topic:public-links).
- **Void** — mark the document void.
- **Remove Reference** — remove the document from that record.

## What Scan Status means

Every uploaded document is virus-scanned. The status tells you whether it is safe to use:

- **Clean** — the file passed and can be downloaded.
- **Pending** or **Quarantined** — still being checked ("Scanning — available once clean"); not yet downloadable.
- **Infected** or **Failed** — the file did not pass ("Blocked: failed virus scan") and is blocked from download.

## Who can use this

Documents live inside **Account Management**, which is available to Managers. The upload/version/share/void/remove actions on the embedded panel appear only when you have management rights over the owning record, so a read-only viewer sees the list and downloads but not the editing actions. See [Management overview](topic:management-overview).
