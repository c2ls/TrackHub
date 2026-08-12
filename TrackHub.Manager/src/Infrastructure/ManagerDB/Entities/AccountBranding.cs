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

using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

// 1:1 branding for an Account. AccountId is both PK and FK to accounts. No secrets.
public class AccountBranding(Guid accountId, string displayName, Guid? logoDocumentId, string primaryColor, string? reportHeader)
    : BaseAuditableEntity
{
    public Guid AccountId { get; set; } = accountId;
    public string DisplayName { get; set; } = displayName;
    public Guid? LogoDocumentId { get; set; } = logoDocumentId;
    public string PrimaryColor { get; set; } = primaryColor;
    public string? ReportHeader { get; set; } = reportHeader;

    public Account Account { get; set; } = null!;
}
