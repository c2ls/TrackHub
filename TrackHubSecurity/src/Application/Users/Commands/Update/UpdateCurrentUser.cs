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

namespace TrackHub.Security.Application.Users.Commands.Update;

[Authorize(Resource = Resources.Profile, Action = Actions.Edit)]
public readonly record struct UpdateCurrentUserCommand(UpdateCurrentUserDto User) : IRequest;
public class UpdateCurrentUserCommandHandler(IUserWriter writer, IUser user) : IRequestHandler<UpdateCurrentUserCommand>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    /// <summary>
    /// Handle the UpdateUserCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The task result</returns>
    public async Task Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        // Update the user asynchronously
        await writer.UpdateUserAsync(request.User, UserId, cancellationToken);
    }
}
