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

namespace TrackHub.Security.Application.Clients.Commands.Delete;

[Authorize(Resource = Resources.Administrative, Action = Actions.Delete)]
[PlatformScoped("OAuth client registry: service/integration client credentials are platform-owned infrastructure with no tenant dimension, administered from the Administrator-only systemadmin console.")]
public readonly record struct DeleteClientCommand(Guid Id) : IRequest;

public class DeleteClientCommandHandler(IClientWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<DeleteClientCommand>
{

    public async Task Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        await writer.DeleteClientAsync(request.Id, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "DeleteClient", "Client", request.Id.ToString(), null), cancellationToken);
    }

}
