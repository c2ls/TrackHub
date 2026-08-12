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

using System.Text.Json;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

// Reads the caller-account's branding from the Manager catalog (existing `accountBranding` query) to brand
// PDF exports. Runs under the caller's token (header propagation); the result is cached
// for 60 seconds per account so repeated exports stay cheap. Every failure — missing feature, an
// authorization error (a non-manager exporting a global PDF report may lack Accounts/Read), a transport
// fault — is swallowed and logged, returning null: a branding lookup must NEVER fail the export.
//
// Only the account display name is wired today: the logo lives as a Document (branding exposes only a
// LogoDocumentId, not bytes), so embedding the image requires the document-download flow. PdfReportBuilder
// already renders LogoImage when present, so wiring it later is additive.
public sealed class ReportBrandingReader(
    IGraphQLClientFactory graphQLClient,
    IMemoryCache cache,
    ILogger<ReportBrandingReader> logger) : IReportBrandingReader
{
    private readonly IGraphQLClient _client = graphQLClient.CreateClient(Clients.Manager);
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    // Single source of truth for the query this reader sends; the ServiceContracts tests validate this
    // exact string against the Manager schema.
    internal const string AccountBrandingQuery = @"
                query($accountId: UUID!) {
                    accountBranding(query: { accountId: $accountId }) {
                        displayName
                    }
                }";

    public async Task<ReportBrandingVm?> GetBrandingAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var cacheKey = $"report-branding::{accountId}";
        if (cache.TryGetValue(cacheKey, out ReportBrandingVm? cached))
        {
            return cached;
        }

        ReportBrandingVm? result = null;
        try
        {
            var request = new GraphQLRequest
            {
                Query = AccountBrandingQuery,
                Variables = new { accountId }
            };

            var response = await _client.SendQueryAsync<object>(request, cancellationToken);
            if (response?.Errors is { Length: > 0 } errors)
            {
                throw new InvalidOperationException(string.Join("; ", errors.Select(e => e.Message)));
            }

            var json = response?.Data?.ToString();
            if (!string.IsNullOrEmpty(json))
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.TryGetProperty("accountBranding", out var node)
                    && node.ValueKind == JsonValueKind.Object
                    && node.TryGetProperty("displayName", out var displayName)
                    && displayName.ValueKind == JsonValueKind.String)
                {
                    var name = displayName.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        result = new ReportBrandingVm(name, null);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log-and-continue: branding is decorative — never let it fail the export.
            logger.LogWarning(ex, "Branding lookup failed for account {AccountId}; exporting without branding.", accountId);
            result = null;
        }

        cache.Set(cacheKey, result, CacheDuration);
        return result;
    }
}
