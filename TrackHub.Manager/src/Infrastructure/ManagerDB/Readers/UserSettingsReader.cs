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

using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

// UserSettingsReader class for reading user settings data
public sealed class UserSettingsReader(IApplicationDbContext context) : IUserSettingsReader
{
    /// <summary>
    /// Retrieves a user settings by ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns an UserSettingsVm object</returns>
    public async Task<UserSettingsVm> GetUserSettingsAsync(Guid id, CancellationToken cancellationToken)
        => await context.UserSettings
            .Where(a => a.UserId.Equals(id))
            .Select(a => new UserSettingsVm(
                a.Language,
                a.Style,
                a.Navbar,
                a.UserId))
            .FirstAsync(cancellationToken);

}
