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

namespace TrackHub.Security.Application.ResourceActionRole.Commands.Delete;

[Authorize(Resource = Resources.Permissions, Action = Actions.Delete)]
public readonly record struct DeleteResourceActionRoleCommand(int ResourceId, int ActionId, int RoleId) : IRequest;

public class DeleteResourceActionRoleCommandHandler(IResourceActionRoleWriter writer, IUserReader userReader, IUser user, IPublisher publisher) : IRequestHandler<DeleteResourceActionRoleCommand>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    // This method handles the DeleteResourceActionRoleCommand by deleting the resource action role.
    // It calls the DeleteResourceActionRoleAsync method of the writer.
    // It throws an UnauthorizedAccessException if the user is not an admin.
    public async Task Handle(DeleteResourceActionRoleCommand request, CancellationToken cancellationToken)
    {
        var isAdmin = await userReader.IsAdminAsync(UserId, cancellationToken);
        if (!isAdmin) throw new UnauthorizedAccessException();
        await writer.DeleteResourceActionRoleAsync(request.ResourceId, request.ActionId, request.RoleId, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(user, "ResourceActionRoleChanged", "ResourceActionRole", $"{request.ResourceId}:{request.ActionId}:{request.RoleId}", null, "removed", null), cancellationToken);
    }
}
