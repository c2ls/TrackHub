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

namespace TrackHub.Security.Application.Users.Queries.GetByAccount;

[Authorize(Resource = Resources.Users, Action = Actions.Read)]
public readonly record struct GetUsersByAccountQuery(int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<UserVm>>;

// The GetUsersByAccountQueryHandler is a class that implements the IRequestHandler interface to handle the GetUsersByAccountQuery.
// It takes an IUserReader dependency in the constructor and provides the implementation for handling the query.
public class GetUsersByAccountQueryHandler(IUserReader reader, IUser user) : IRequestHandler<GetUsersByAccountQuery, IReadOnlyCollection<UserVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    // The Handle method is responsible for handling the GetUsersByAccountQuery and returning the result.
    // It asynchronously calls the GetUsersAsync method of the IUserReader dependency to retrieve the users by account ID.
    public async Task<IReadOnlyCollection<UserVm>> Handle(GetUsersByAccountQuery request, CancellationToken cancellationToken)
    {
        var user = await reader.GetUserAsync(UserId, cancellationToken);
        return await reader.GetUsersAsync(user.AccountId, request.Skip, request.Take, cancellationToken);
    }

}
