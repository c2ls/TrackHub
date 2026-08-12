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

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace TrackHub.Manager.Application.Credentials.Queries.Get;

[Authorize(Resource = Resources.Credentials, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetCredentialQuery(Guid Id) : IRequest<CredentialVm>;

// Handles the GetCredentialQuery and returns the corresponding CredentialVm
public class GetCredentialsQueryHandler(ICredentialReader reader, IConfiguration configuration) : IRequestHandler<GetCredentialQuery, CredentialVm>
{
    // Handles the GetCredentialQuery and returns the corresponding CredentialVm
    public async Task<CredentialVm> Handle(GetCredentialQuery request, CancellationToken cancellationToken)
    {
        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Credential key not found.");
        return await reader.GetCredentialAsync(request.Id, key, cancellationToken);
    }
}
