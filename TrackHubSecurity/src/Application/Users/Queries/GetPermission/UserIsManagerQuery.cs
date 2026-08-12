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

namespace TrackHub.Security.Application.Users.Queries.GetPermission;

[Authorize(Resource = Resources.Users, Action = Actions.Read)]
public readonly record struct UserIsManagerQuery() : IRequest<bool>;

public class UserIsManagerQueryHandler(IUserReader reader, IUser user) : IRequestHandler<UserIsManagerQuery, bool>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    /// <summary>
    /// Handles the UserIsManagerQuery request by checking if the user is a manager.
    /// It uses the IUserReader service to perform the check.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns a boolean value indicating whether the user is a manager or not.</returns>
    public async Task<bool> Handle(UserIsManagerQuery request, CancellationToken cancellationToken)
        => await reader.IsManagerAsync(UserId, cancellationToken);

}
