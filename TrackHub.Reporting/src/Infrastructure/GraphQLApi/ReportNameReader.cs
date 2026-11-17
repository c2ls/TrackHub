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

using System.Collections.Concurrent;
using TrackHub.Reporting.Domain.Interfaces.Manager;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class ReportNameReader(IGraphQLClientFactory graphQLClient, IGpsManagerReader gps, IWorkforceReportReader workforce)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IReportNameReader
{
    internal const string TransporterNameQuery = @"
                query($id: UUID!) {
                    transporter(query: { id: $id }) {
                        transporterId
                        name
                    }
                }";

    internal const string DocumentNameQuery = @"
                query($id: UUID!) {
                    document(query: { documentId: $id }) {
                        title
                        fileName
                    }
                }";

    internal const string TransportersByAccountQuery = @"
                query($skip: Int!, $take: Int!) {
                    transportersByAccount(query: { skip: $skip, take: $take }) {
                        items { transporterId name }
                        totalCount
                    }
                }";

    internal const string RetiredTransportersQuery = @"
                query($skip: Int!, $take: Int!) {
                    retiredTransporters(query: { skip: $skip, take: $take }) {
                        items { transporterId name }
                        totalCount
                    }
                }";

    public async Task<string?> GetTransporterNameAsync(Guid transporterId, CancellationToken cancellationToken)
        => (await TryQueryAsync<TransporterName>(TransporterNameQuery, transporterId, cancellationToken))?.Name;

    public async Task<IReadOnlyDictionary<Guid, string>> GetTransporterNamesAsync(IEnumerable<Guid> transporterIds, CancellationToken cancellationToken)
    {
        var names = (await OrEmpty(() => DrainAsync(TransportersByAccountQuery, cancellationToken)))
            .ToDictionary(t => t.TransporterId, t => t.Name);
        if (transporterIds.All(names.ContainsKey))
        {
            return names;
        }

        foreach (var retired in await OrEmpty(() => DrainAsync(RetiredTransportersQuery, cancellationToken)))
        {
            names.TryAdd(retired.TransporterId, retired.Name);
        }

        return names;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetOperatorNamesAsync(CancellationToken cancellationToken)
        => (await OrEmpty(() => gps.GetOperatorsAsync(cancellationToken))).ToDictionary(o => o.OperatorId, o => o.Name);

    public async Task<IReadOnlyDictionary<Guid, string>> GetDeviceSerialsAsync(Guid accountId, CancellationToken cancellationToken)
        => (await OrEmpty(() => gps.GetSynchronizedDevicesAsync(accountId, null, null, cancellationToken))).ToDictionary(d => d.DeviceId, d => d.Serial);

    public async Task<IReadOnlyDictionary<Guid, string>> GetStaffNamesAsync(CancellationToken cancellationToken)
        => (await OrEmpty(() => workforce.GetStaffMembersAsync(cancellationToken))).ToDictionary(s => s.StaffId, s => s.Name);

    public async Task<IReadOnlyDictionary<Guid, string>> GetDocumentNamesAsync(IEnumerable<Guid> documentIds, CancellationToken cancellationToken)
    {
        var names = new ConcurrentDictionary<Guid, string>();
        await Parallel.ForEachAsync(
            documentIds.Distinct(),
            new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = cancellationToken },
            async (id, token) =>
            {
                if (await TryQueryAsync<DocumentName>(DocumentNameQuery, id, token) is { } document)
                {
                    names[id] = string.IsNullOrWhiteSpace(document.Title) ? document.FileName : document.Title;
                }
            });

        return names;
    }

    private Task<IReadOnlyCollection<TransporterName>> DrainAsync(string query, CancellationToken cancellationToken)
        => ManagerPageDrain.FetchAllAsync<TransporterName>(
            (skip, take) => new GraphQLRequest { Query = query, Variables = new { skip, take } },
            (request, token) => QueryAsync<ManagerPage<TransporterName>>(request, token),
            cancellationToken);

    private async Task<T?> TryQueryAsync<T>(string query, Guid id, CancellationToken cancellationToken) where T : class
    {
        try
        {
            return await QueryAsync<T>(new GraphQLRequest { Query = query, Variables = new { id } }, cancellationToken);
        }
        catch (HotChocolate.GraphQLException)
        {
            return null;
        }
    }

    // A name is a convenience: a lookup the caller may not make must not fail the report.
    private static async Task<IReadOnlyCollection<T>> OrEmpty<T>(Func<Task<IReadOnlyCollection<T>>> read)
    {
        try
        {
            return await read();
        }
        catch (HotChocolate.GraphQLException)
        {
            return [];
        }
    }

    private sealed record TransporterName(Guid TransporterId, string Name);

    private sealed record DocumentName(string? Title, string FileName);
}
