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
using Common.Domain.Extensions;
using Microsoft.Extensions.Configuration;

namespace TrackHub.Manager.Application.Credentials.Command.Update;

[Authorize(Resource = Resources.Credentials, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct UpdateCredentialCommand(UpdateCredentialDto Credential) : IRequest;

public class UpdateCredentialCommandHandler(ICredentialWriter writer, IConfiguration configuration) : IRequestHandler<UpdateCredentialCommand>
{
    public async Task Handle(UpdateCredentialCommand request, CancellationToken cancellationToken)
    {
        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Credential key not found.");

        // Generate a salt for encryption
        var salt = CryptographyExtensions.GenerateAesKey(256);

        // Update the credential using the writer
        await writer.UpdateCredentialAsync(request.Credential, salt, key, cancellationToken);
    }
}
