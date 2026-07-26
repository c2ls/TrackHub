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

namespace TrackHub.Manager.Domain.Constants;

/// <summary>Document lifecycle status values. Stored as strings.</summary>
public static class DocumentStatuses
{
    public const string Pending = nameof(Pending);
    public const string Uploaded = nameof(Uploaded);
    public const string Active = nameof(Active);
    public const string Expired = nameof(Expired);
    public const string Replaced = nameof(Replaced);
    public const string Voided = nameof(Voided);
    public const string Deleted = nameof(Deleted);

    public static readonly IReadOnlyCollection<string> All =
        [Pending, Uploaded, Active, Expired, Replaced, Voided, Deleted];

    public static bool IsValid(string? value) => value != null && All.Contains(value);
}

/// <summary>Virus/malware scan status values. Stored as strings.</summary>
public static class DocumentScanStatuses
{
    public const string Pending = nameof(Pending);
    public const string Quarantined = nameof(Quarantined);
    public const string Clean = nameof(Clean);
    public const string Infected = nameof(Infected);
    public const string Failed = nameof(Failed);

    public static readonly IReadOnlyCollection<string> All =
        [Pending, Quarantined, Clean, Infected, Failed];

    public static bool IsValid(string? value) => value != null && All.Contains(value);
}

/// <summary>Sensitivity classification values. Stored as strings.</summary>
public static class DocumentClassifications
{
    public const string Public = nameof(Public);
    public const string Internal = nameof(Internal);
    public const string Confidential = nameof(Confidential);
    public const string Legal = nameof(Legal);

    public static readonly IReadOnlyCollection<string> All =
        [Public, Internal, Confidential, Legal];

    public static bool IsValid(string? value) => value != null && All.Contains(value);

    /// <summary>
    /// Confidential/Legal require an explicit clearance beyond plain Documents/Read.
    /// </summary>
    public static bool IsSensitive(string? classification)
        => string.Equals(classification, Confidential, StringComparison.OrdinalIgnoreCase)
           || string.Equals(classification, Legal, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Owner entity types with a registered visibility resolver. Owner types not listed
/// here are deny-by-default until their module ships a resolver.
/// <para>
/// DESIGN NOTE — there is deliberately NO <c>"Trip"</c> owner type (spec 11 §11/§18.6). Trip
/// documents (manifests, bills of lading, POD signatures and photos) are created with
/// <c>OwnerEntityType = Transporter</c> and <c>OwnerEntityId = trip.TransporterId</c>, and
/// TripManagement keeps its own <c>TripDocument</c> link row. Registering a <c>"Trip"</c> owner type
/// would force <c>DocumentAccessPolicy</c> to call TripManagement to resolve visibility, inverting
/// the dependency direction (Manager must not depend on the services that depend on it). The
/// transporter resolver already yields exactly the right audience — group visibility for portal
/// users and <c>validateDriverAssignment</c> for drivers. Accepted consequence: trip documents also
/// appear in the transporter's document list, filterable by <c>Category</c>. Do not "complete" this
/// list with a Trip entry.
/// </para>
/// </summary>
public static class DocumentOwnerTypes
{
    public const string Transporter = nameof(Transporter);
    public const string Driver = nameof(Driver);

    /// <summary>Owner types with a registered visibility resolver (MVP: Transporter, Driver).</summary>
    public static bool IsRegistered(string? ownerEntityType)
        => string.Equals(ownerEntityType, Transporter, StringComparison.OrdinalIgnoreCase)
           || string.Equals(ownerEntityType, Driver, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Document sharing constants — reuses PublicLinkGrant.</summary>
public static class DocumentSharing
{
    public const string ResourceType = "Document";
    public const string ReadScope = "document.read";
}

/// <summary>Platform file-size ceilings. Modules may lower, never exceed.</summary>
public static class DocumentLimits
{
    public const long DefaultMaxBytes = 25L * 1024 * 1024;          // 25 MB per photo/document
    public const long ImportExportMaxBytes = 100L * 1024 * 1024;    // 100 MB import/export/generated

    /// <summary>Expiration alert thresholds in days.</summary>
    public static readonly IReadOnlyCollection<int> ExpirationThresholdsDays = [30, 15, 7];
}
