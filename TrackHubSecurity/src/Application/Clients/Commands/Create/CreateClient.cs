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

using Ardalis.GuardClauses;
using Common.Application.Interfaces;
using Common.Domain.Extensions;
using Microsoft.Extensions.Configuration;
using TrackHub.Security.Application.Audit.Events;

namespace TrackHub.Security.Application.Clients.Commands.Create;

[Authorize(Resource = Resources.Administrative, Action = Actions.Write)]
public readonly record struct CreateClientCommand(ClientDto Client) : IRequest<ClientVm>;

public class CreateClientCommandHandler(IClientWriter writer, IConfiguration configuration, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<CreateClientCommand, ClientVm>
{
    public async Task<ClientVm> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Secrets key not found.");
        var salt = CryptographyExtensions.GenerateAesKey(256);

        var vm = await writer.CreateClientAsync(request.Client, salt, key, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "CreateClient", "Client", vm.ClientId.ToString(), null, null, vm.Name), cancellationToken);
        return vm;
    }
}
