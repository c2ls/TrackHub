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

using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class AccountBrandingReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IAccountBrandingReader
{
    // Platform-default primary color used when an account has no branding row.
    public const string DefaultPrimaryColor = "#1A73E8";

    // Owner-entity type of the branding logo document.
    public const string BrandingDocumentOwnerType = "AccountBranding";

    public async Task<AccountBrandingVm> GetBrandingAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        var entity = await Context.AccountBrandings
            .FirstOrDefaultAsync(b => b.AccountId == scopedAccountId, cancellationToken);

        if (entity is not null)
        {
            return new AccountBrandingVm(
                entity.AccountId, entity.DisplayName, entity.LogoDocumentId,
                entity.PrimaryColor, entity.ReportHeader, entity.LastModified);
        }

        // Absent row ⇒ platform default branding. Default the display name to the
        // account name so portal/mobile/reports always have a sensible label.
        var name = await Context.Accounts
            .Where(a => a.AccountId == scopedAccountId)
            .Select(a => a.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return new AccountBrandingVm(scopedAccountId, name, null, DefaultPrimaryColor, null, default);
    }

    public async Task<bool> LogoDocumentBelongsToAccountAsync(Guid accountId, Guid documentId, CancellationToken cancellationToken)
        => await Context.Documents.AnyAsync(d =>
            d.DocumentId == documentId
            && d.AccountId == accountId
            && d.OwnerEntityType == BrandingDocumentOwnerType, cancellationToken);
}
