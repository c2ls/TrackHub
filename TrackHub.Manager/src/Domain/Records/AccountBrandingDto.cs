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

namespace TrackHub.Manager.Domain.Records;

// Account branding input DTO (upsert). PrimaryColor is validated as #RRGGBB; LogoDocumentId, if
// present, must reference a Manager Document owned by the same Account with owner type AccountBranding.
public readonly record struct AccountBrandingDto(
    Guid AccountId,
    string DisplayName,
    Guid? LogoDocumentId,
    string PrimaryColor,
    string? ReportHeader
    );
