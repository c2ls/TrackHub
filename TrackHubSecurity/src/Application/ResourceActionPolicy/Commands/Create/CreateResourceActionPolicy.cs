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

namespace TrackHub.Security.Application.ResourceActionPolicy.Commands.Create;

[Authorize(Resource = Resources.Permissions, Action = Actions.Write)]
public readonly record struct CreateResourceActionPolicyCommand(ResourceActionPolicyDto ResourceActionPolicy) : IRequest<ResourceActionPolicyVm>;

public class CreateResourceActionPolicyCommandHandler(IResourceActionPolicyWriter writer, IUserReader userReader, IUser user, IPublisher publisher) : IRequestHandler<CreateResourceActionPolicyCommand, ResourceActionPolicyVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    // This method handles the CreateResourceActionPolicyCommand and returns a ResourceActionPolicyVm
    public async Task<ResourceActionPolicyVm> Handle(CreateResourceActionPolicyCommand request, CancellationToken cancellationToken)
    {
        var isAdmin = await userReader.IsAdminAsync(UserId, cancellationToken);
        if (!isAdmin)
        {
            throw new UnauthorizedAccessException();
        }

        var vm = await writer.CreateResourceActionPolicyAsync(request.ResourceActionPolicy, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(user, "ResourceActionPolicyChanged", "ResourceActionPolicy", $"{request.ResourceActionPolicy.ResourceId}:{request.ResourceActionPolicy.ActionId}:{request.ResourceActionPolicy.PolicyId}", null, null, "assigned"), cancellationToken);
        return vm;
    }

}
