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

using TrackHub.Manager.Application.Credentials.Command.Create;
using TrackHub.Manager.Application.Credentials.Command.Delete;
using TrackHub.Manager.Application.Credentials.Command.Update;
using TrackHub.Manager.Application.Credentials.Command.UpdateToken;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<CredentialVm> CreateCredential([Service] ISender sender, CreateCredentialCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdateCredential([Service] ISender sender, Guid id, UpdateCredentialCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Credential.CredentialId) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<bool> UpdateOperatorCredential([Service] ISender sender, Guid id, UpdateOperatorCredentialCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Credential.OperatorId) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<bool> UpdateToken([Service] ISender sender, Guid id, UpdateTokenCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Credential.CredentialId) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<Guid> DeleteCredential([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCredentialCommand(id), cancellationToken);
        return id;
    }
}
