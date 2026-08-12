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

using TrackHub.Security.Application.Users.Commands.Create;
using TrackHub.Security.Application.Users.Commands.CreateManager;
using TrackHub.Security.Application.Users.Commands.Delete;
using TrackHub.Security.Application.Users.Commands.Unlock;
using TrackHub.Security.Application.Users.Commands.Update;
using TrackHub.Security.Domain.Models;

namespace TrackHub.Security.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<UserVm> CreateUser([Service] ISender sender, CreateUserCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<UserVm> CreateManager([Service] ISender sender, CreateManagerCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdateUser([Service] ISender sender, Guid id, UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (id != command.User.UserId) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<bool> UpdateCurrentUser([Service] ISender sender, UpdateCurrentUserCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<bool> UpdatePassword([Service] ISender sender, Guid id, UpdatePasswordCommand command, CancellationToken cancellationToken)
    {
        if (id != command.User.UserId) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<Guid> DeleteUser([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return id;
    }

    public async Task<bool> UnlockUser([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new UnlockUserCommand(id), cancellationToken);
        return true;
    }
}
