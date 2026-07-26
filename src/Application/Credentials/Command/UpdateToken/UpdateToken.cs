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

namespace TrackHub.Manager.Application.Credentials.Command.UpdateToken;

// ServiceClient-only, mirroring its read twin GetTokenQuery: every Router flow that refreshes a
// provider token (interactive provider reads and the SyncWorker alike) sends this under the
// Router's own identity, which holds the seeded Credentials/Write grant.
[Authorize(Resource = Resources.Credentials, Action = Actions.Write, PrincipalTypes = "ServiceClient")]
[AllowCrossAccount("Router persists refreshed provider session tokens under its global service identity with no account claim (ServiceClient-only surface). CredentialWriter.UpdateTokenAsync still loads the credential and RequireAccountWriteAccess checks its operator's owning account, so a tenant-bound service client stays confined to its own account.")]
public readonly record struct UpdateTokenCommand(UpdateTokenDto Credential) : IRequest;

public class UpdateCommandHandler(ICredentialWriter writer, IConfiguration configuration) : IRequestHandler<UpdateTokenCommand>
{
    // This method handles the update token command.
    public async Task Handle(UpdateTokenCommand request, CancellationToken cancellationToken)
    {
        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Credential key not found.");
        await writer.UpdateTokenAsync(request.Credential, key, cancellationToken);
    }
}
