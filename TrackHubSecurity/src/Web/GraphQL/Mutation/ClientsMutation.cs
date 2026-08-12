// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using TrackHub.Security.Application.Clients.Commands.Create;
using TrackHub.Security.Application.Clients.Commands.Delete;
using TrackHub.Security.Application.Clients.Commands.Update;
using TrackHub.Security.Domain.Models;

namespace TrackHub.Security.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<ClientVm> CreateClient([Service] ISender sender, CreateClientCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdateClient([Service] ISender sender, Guid id, UpdateClientCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Client.ClientId) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<Guid> DeleteClient([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteClientCommand(id), cancellationToken);
        return id;
    }
}
