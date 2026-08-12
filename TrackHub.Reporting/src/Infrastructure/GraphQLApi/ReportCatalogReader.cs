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
using Common.Application.Exceptions;
using Common.Domain.Extensions;
using GraphQL.Client.Abstractions;
using HotChocolate;
using Microsoft.Extensions.Caching.Memory;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

// Reads governed-catalog metadata for a single report from the Manager catalog, caching
// each result — hits and misses — for 60 seconds so the execution pipeline stays cheap under repeated
// exports. The `reportByCode` query returns null for an unknown code, which is cached as a null result
// (so the pipeline raises REPORT_NOT_FOUND). Runs under the caller's token (header propagation).
public class ReportCatalogReader(IGraphQLClientFactory graphQLClient, IMemoryCache cache) : IReportCatalogReader
{
    private readonly IGraphQLClient _client = graphQLClient.CreateClient(Clients.Manager);
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    // Single source of truth for the query this reader sends; the ServiceContracts tests validate this
    // exact string against the Manager schema.
    internal const string ReportByCodeQuery = @"
                query($code: String!) {
                    reportByCode(code: $code) {
                        code
                        description
                        category
                        requiredFeatureKey
                        managerOnly
                        supportsPdf
                        sortOrder
                        active
                    }
                }";

    public async Task<ReportMetadataVm?> GetReportByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var cacheKey = $"report-metadata::{code}";
        if (cache.TryGetValue(cacheKey, out ReportMetadataVm? cached))
        {
            return cached;
        }

        var request = new GraphQLRequest
        {
            Query = ReportByCodeQuery,
            Variables = new { code }
        };

        var response = await _client.SendQueryAsync<object>(request, cancellationToken)
            ?? throw new Exception("GraphQL query execution error.");

        if (response.Errors is { Length: > 0 })
        {
            throw new GraphQLException(response.Errors.ConvertToIError());
        }

        ReportMetadataVm? result = null;
        var json = response.Data?.ToString();
        if (!string.IsNullOrEmpty(json))
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("reportByCode", out var node)
                && node.ValueKind != JsonValueKind.Null)
            {
                result = node.GetRawText().Deserialize<ReportMetadataVm>();
            }
        }

        cache.Set(cacheKey, result, CacheDuration);
        return result;
    }
}
