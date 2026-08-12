// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

using TrackHub.Manager.Application.Users.Commands.Create;
using TrackHub.Manager.Application.Users.Commands.Delete;
using TrackHub.Manager.Application.Users.Commands.Update;
using TrackHub.Manager.Application.Users.Commands.UpdateSettings;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<UserVm> CreateUser([Service] ISender sender, CreateUserCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdateUser([Service] ISender sender, Guid id, UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (id != command.User.UserId) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<bool> UpdateUserSettings([Service] ISender sender, Guid id, UpdateUserSettingsCommand command, CancellationToken cancellationToken)
    {
        if (id != command.UserSettings.UserId) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<Guid> DeleteUser([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return id;
    }
}
