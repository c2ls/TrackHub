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

using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.Users.Queries.GetSettings;

[Authorize(Resource = Resources.Profile, Action = Actions.Read)]
public readonly record struct GetUserSettingsQuery() : IRequest<UserSettingsVm>;

public class GetUserSettingsQueryHandler(IUserSettingsReader reader, IUser user) : IRequestHandler<GetUserSettingsQuery, UserSettingsVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();
    // This method handles the GetUserSettingsQuery and returns an UserSettingsVm
    public async Task<UserSettingsVm> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
        => await reader.GetUserSettingsAsync(UserId, cancellationToken);

}
