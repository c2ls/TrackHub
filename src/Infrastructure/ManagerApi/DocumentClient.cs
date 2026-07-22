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

using HotChocolate;

namespace TrackHub.TripManagement.Infrastructure.ManagerApi;

/// <summary>
/// Reads document metadata from Manager's <c>document</c> query under <c>trip_client</c> to
/// validate POD attachments. This module owns no storage surface: PODs are ordinary Manager
/// <c>Document</c> records linked by id (spec 11 §11). Manager answers a missing or
/// non-readable document with a GraphQL error, which maps to a null state here — the caller
/// then rejects the attachment rather than trusting it.
/// </summary>
public class DocumentClient(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IDocumentClient
{
    internal const string DocumentQuery = @"
                query($documentId: UUID!) {
                    document(query: { documentId: $documentId }) {
                        documentId
                        accountId
                        scanStatus
                        status
                    }
                }";

    public async Task<DocumentStateVm?> GetDocumentStateAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = DocumentQuery,
            Variables = new { documentId }
        };

        try
        {
            var document = await QueryAsync<DocumentResponse>(request, cancellationToken);
            return new DocumentStateVm(document.DocumentId, document.AccountId, document.ScanStatus, document.Status);
        }
        catch (GraphQLException)
        {
            return null;
        }
    }
}

internal sealed record DocumentResponse(Guid DocumentId, Guid AccountId, string ScanStatus, string Status);
