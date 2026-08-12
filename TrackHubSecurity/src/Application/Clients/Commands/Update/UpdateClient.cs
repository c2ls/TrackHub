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

using Common.Application.Interfaces;
using TrackHub.Security.Application.Audit.Events;

namespace TrackHub.Security.Application.Clients.Commands.Update;

[Authorize(Resource = Resources.Administrative, Action = Actions.Edit)]
public readonly record struct UpdateClientCommand(ClientUserDto Client) : IRequest;
public class UpdateClientCommandHandler(IClientWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<UpdateClientCommand>
{

    public async Task Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        await writer.UpdateClientAsync(request.Client, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "UpdateClient", "Client", request.Client.ClientId.ToString(), null), cancellationToken);
    }

}
