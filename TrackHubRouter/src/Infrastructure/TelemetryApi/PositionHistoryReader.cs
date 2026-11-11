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
using FluentValidation.Results;

namespace TrackHub.Router.Infrastructure.TelemetryApi;

// STORED replay source read. Uses the user-token Telemetry client so the Telemetry service enforces
// the gps.positionHistory flag, PositionHistory authorization, and group visibility. The window is
// drained to the end; past MaxPoints the replay is refused rather than cut short.
public class PositionHistoryReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Telemetry)), IPositionHistoryReader
{
    private static readonly JsonSerializerOptions AttributeOptions = new() { PropertyNameCaseInsensitive = true };
    internal const int MaxPoints = 100_000;
    private const int PageSize = 500;
    public const string LimitExceededCode = "POSITION_HISTORY_LIMIT_EXCEEDED";

    internal const string PositionHistoryFeedQuery = @"
                query($accountId: UUID!, $transporterId: UUID!, $from: DateTime!, $to: DateTime!, $take: Int!, $cursor: String) {
                    positionHistoryFeed(query: { accountId: $accountId, transporterId: $transporterId, from: $from, to: $to, take: $take, cursor: $cursor })
                    {
                        items {
                            sourceTimestamp
                            latitude
                            longitude
                            altitude
                            speed
                            course
                            eventId
                            address
                            city
                            state
                            country
                            attributes
                            transporterId
                        }
                        hasMore
                        nextCursor
                    }
                }";

    public async Task<IEnumerable<PositionVm>> GetPositionHistoryRangeAsync(Guid accountId, Guid transporterId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var rows = new List<PositionHistoryRow>();
        string? cursor = null;
        while (true)
        {
            var page = await QueryAsync<PositionHistoryPage>(new GraphQLRequest
            {
                Query = PositionHistoryFeedQuery,
                Variables = new { accountId, transporterId, from, to, take = PageSize, cursor }
            }, cancellationToken);
            rows.AddRange(page.Items ?? []);

            if (rows.Count > MaxPoints)
            {
                throw new Common.Application.Exceptions.ValidationException(LimitExceededCode,
                    [new ValidationFailure("to", $"More than {MaxPoints} stored positions fall in this window; narrow it.")]);
            }

            if (!page.HasMore || string.IsNullOrEmpty(page.NextCursor) || page.NextCursor == cursor)
            {
                break;
            }

            cursor = page.NextCursor;
        }

        // The feed runs newest first.
        rows.Reverse();
        return rows.Select(row => new PositionVm(
            row.TransporterId,
            string.Empty,
            string.Empty,
            row.Latitude,
            row.Longitude,
            row.Altitude,
            row.SourceTimestamp,
            null,
            row.Speed,
            row.Course,
            row.EventId,
            row.Address,
            row.City,
            row.State,
            row.Country,
            ParseAttributes(row.Attributes)));
    }

    private static AttributesVm? ParseAttributes(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AttributesVm>(json, AttributeOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record PositionHistoryPage(IReadOnlyCollection<PositionHistoryRow>? Items, bool HasMore, string? NextCursor);

    private readonly record struct PositionHistoryRow(
        Guid TransporterId,
        DateTimeOffset SourceTimestamp,
        double Latitude,
        double Longitude,
        double? Altitude,
        double Speed,
        double? Course,
        int? EventId,
        string? Address,
        string? City,
        string? State,
        string? Country,
        string? Attributes);
}
