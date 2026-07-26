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

namespace TrackHub.Manager.Application.Operators.Commands.Delete;

[Authorize(Resource = Resources.Operators, Action = Actions.Delete)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public record DeleteOperatorCommand(Guid Id) : IRequest;

public class DeleteOperatorCommandHandler(IOperatorWriter writer, IOperatorReader reader, ICredentialWriter credentialWriter) : IRequestHandler<DeleteOperatorCommand>
{
    public async Task Handle(DeleteOperatorCommand request, CancellationToken cancellationToken) 
    {
        var @operator = await reader.GetOperatorAsync(request.Id, cancellationToken);
        if (@operator.Credential != null && @operator.Credential != default(CredentialTokenVm)) 
        {
            await credentialWriter.DeleteCredentialAsync(@operator.Credential.Value.CredentialId, cancellationToken);
        }

        await writer.DeleteOperatorAsync(request.Id, cancellationToken); 
    }
}
