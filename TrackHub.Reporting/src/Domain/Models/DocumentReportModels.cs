// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

namespace TrackHub.Reporting.Domain.Models;

// Intermediate projections deserialized from Manager GraphQL for the document reports.
public readonly record struct ReportDocumentVm(
    string Category, string OwnerEntityType, string OwnerEntityId, string FileName,
    string Classification, string Status, DateTimeOffset? ExpiresAt);

public readonly record struct ReportDocumentTypeVm(string Category, bool Required, bool Enabled);

// One group-visible transporter with its Active document categories (Manager's batched
// transporterDocumentCompliance projection — replaces one documentsForOwner call per transporter).
public readonly record struct TransporterDocumentComplianceVm(
    Guid TransporterId, string TransporterName, IReadOnlyCollection<string> ActiveCategories);

public readonly record struct ReportShareVm(
    string ResourceType, string ResourceId, string Scopes, string Purpose,
    DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt, int AccessCount, DateTimeOffset? LastAccessedAt);

// ---- Row VMs (property order = Excel column order; property name = resx header key) ----

public readonly record struct ExpiringDocumentRowVm(
    string Category, string OwnerType, string OwnerId, string FileName, string Classification, string Status, DateTimeOffset? ExpiresAt);

public readonly record struct MissingRequiredDocumentRowVm(
    string OwnerType, string OwnerName, string OwnerId, string Category);

public readonly record struct DocumentShareActivityRowVm(
    string SharedDocumentId, string Scopes, string Purpose, int AccessCount, DateTimeOffset? LastAccessedAt, DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt);

public readonly record struct DocumentUploadVolumeRowVm(string Category, int DocumentCount);
