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

using TrackHub.Router.Domain.Records;

namespace ManagerApi;

// This class represents a CredentialWriter that implements the ICredentialWriter interface.
// It is responsible for updating a token asynchronously.
//
// asService: token refresh is CREDENTIAL MATERIAL handling, so it always runs under the Router's
// own identity regardless of which flow triggered it (interactive provider reads propagate the
// user's token; the SyncWorker host is already service-only). This mirrors the read twin
// (getToken via OperatorSystemReader) and lets Manager gate updateToken ServiceClient-only —
// never widen a user's permissions to write credential material.
public class CredentialWriter(IGraphQLClientFactory graphQLClient) : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), ICredentialWriter
{
    internal const string UpdateTokenMutation = @"
                    mutation($id:UUID!, $credentialId: UUID!, $refreshToken: String, $refreshTokenExpiration: DateTime, $token: String, $tokenExpiration: DateTime) {
                      updateToken(id: $id,
                            command: { credential: { credentialId: $credentialId, refreshToken: $refreshToken, refreshTokenExpiration: $refreshTokenExpiration, token: $token, tokenExpiration: $tokenExpiration } })
                    }";

    /// <summary>
    /// It constructs a GraphQL request with the necessary variables and sends a mutation to update the token.
    /// </summary>
    /// <param name="id">The ID of the token to update</param>
    /// <param name="credential">The credential information to update</param>
    /// <param name="token">The cancellation token</param>
    /// <returns>A boolean indicating the success of the update operation</returns>
    public async Task<bool> UpdateTokenAsync(Guid id, UpdateTokenDto credential, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = UpdateTokenMutation,
            Variables = new
            {
                id,
                credentialId = credential.CredentialId,
                refreshToken = credential.RefreshToken,
                refreshTokenExpiration = credential.RefreshTokenExpiration,
                token = credential.Token,
                tokenExpiration = credential.TokenExpiration
            }
        };
        return await MutationAsync<bool>(request, token);
    }
}
