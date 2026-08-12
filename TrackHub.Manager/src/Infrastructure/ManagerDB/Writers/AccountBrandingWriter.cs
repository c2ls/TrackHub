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
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class AccountBrandingWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IAccountBrandingWriter
{
    public async Task<AccountBrandingVm> UpsertBrandingAsync(AccountBrandingDto branding, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(branding.AccountId);

        var newValuesJson = BrandingJson(branding.DisplayName, branding.LogoDocumentId, branding.PrimaryColor, branding.ReportHeader);

        var existing = await Context.AccountBrandings
            .FirstOrDefaultAsync(b => b.AccountId == accountId, cancellationToken);

        string? oldValuesJson;
        if (existing is null)
        {
            existing = new AccountBranding(accountId, branding.DisplayName, branding.LogoDocumentId, branding.PrimaryColor, branding.ReportHeader);
            await Context.AccountBrandings.AddAsync(existing, cancellationToken);
            oldValuesJson = null;
        }
        else
        {
            Context.AccountBrandings.Attach(existing);
            oldValuesJson = BrandingJson(existing.DisplayName, existing.LogoDocumentId, existing.PrimaryColor, existing.ReportHeader);
            existing.DisplayName = branding.DisplayName;
            existing.LogoDocumentId = branding.LogoDocumentId;
            existing.PrimaryColor = branding.PrimaryColor;
            existing.ReportHeader = branding.ReportHeader;
        }

        AddAuditEvent(accountId, "BrandingChanged", "AccountBranding", accountId.ToString(), oldValuesJson, newValuesJson);

        await Context.SaveChangesAsync(cancellationToken);

        return new AccountBrandingVm(
            existing.AccountId, existing.DisplayName, existing.LogoDocumentId,
            existing.PrimaryColor, existing.ReportHeader, existing.LastModified);
    }

    private static string BrandingJson(string displayName, Guid? logoDocumentId, string primaryColor, string? reportHeader)
        => $"{{\"displayName\":{Quote(displayName)},\"logoDocumentId\":{Quote(logoDocumentId?.ToString())},"
           + $"\"primaryColor\":{Quote(primaryColor)},\"reportHeader\":{Quote(reportHeader)}}}";
}
