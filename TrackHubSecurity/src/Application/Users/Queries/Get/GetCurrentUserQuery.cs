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

namespace TrackHub.Security.Application.Users.Queries.Get;

[Authorize(Resource = Resources.Profile, Action = Actions.Read)]
public readonly record struct GetCurrentUserQuery() : IRequest<UserVm>;

// The GetCurrentUserQueryHandler class is responsible for handling the GetCurrentUserQuery and returning the corresponding UserVm.
public class GetCurrentUserQueryHandler(IUserReader reader, IUser user) : IRequestHandler<GetCurrentUserQuery, UserVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    // The Handle method is called when the GetCurrentUserQuery is executed.
    // It retrieves the user from the IUserReader and returns the corresponding UserVm.
    public async Task<UserVm> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        => await reader.GetUserAsync(UserId, cancellationToken);

}
