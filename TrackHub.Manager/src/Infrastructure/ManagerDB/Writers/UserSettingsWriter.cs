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

using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

// UserSettingsWriter class for handling user settings-related operations. UpdateUserSettingsAsync
// is the [Authorize(Profile/Edit)] SELF surface: the row key is a UserId off the wire, so the
// caller is bound to their own row here (the enforcement point UpdateUserSettingsCommand's
// [AccountScopeEnforcedInHandler] marker cites). Create/Delete run inside the user provisioning
// and deletion flows, which are guarded upstream by the user replica writer.
public sealed class UserSettingsWriter(IApplicationDbContext context, ICurrentPrincipal principal) : IUserSettingsWriter
{
    private void RequireSelf(Guid userId)
    {
        if ((principal.PrincipalType == PrincipalType.ServiceClient && !principal.AccountId.HasValue)
            || principal.UserId == userId)
        {
            return;
        }

        throw new ForbiddenAccessException("Insufficient permissions. Settings may only be changed for the calling user.");
    }

    /// <summary>
    /// Creates a new user setting asynchronously
    /// </summary>
    /// <param name="userSettingsDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The created user settings view model</returns>
    public async Task<UserSettingsVm> CreateUserSettingsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userSettings = new UserSettings(userId);

        await context.UserSettings.AddAsync(userSettings, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new UserSettingsVm(
            userSettings.Language,
            userSettings.Style,
            userSettings.Navbar,
            userSettings.UserId);
    }

    /// <summary>
    /// Updates an existing user setting asynchronously
    /// </summary>
    /// <param name="userSettingsDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task UpdateUserSettingsAsync(UserSettingsDto userSettingsDto, CancellationToken cancellationToken)
    {
        RequireSelf(userSettingsDto.UserId);

        var userSettings = await context.UserSettings.FindAsync([userSettingsDto.UserId], cancellationToken)
            ?? throw new NotFoundException(nameof(UserSettings), $"{userSettingsDto.UserId}");

        context.UserSettings.Attach(userSettings);

        userSettings.Language = userSettingsDto.Language;
        userSettings.Style = userSettingsDto.Style;
        userSettings.Navbar = userSettingsDto.Navbar;

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a user setting asynchronously
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task DeleteUserSettingsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userSettings = await context.UserSettings.FindAsync([userId], cancellationToken)
            ?? throw new NotFoundException(nameof(UserSettings), $"{userId}");

        context.UserSettings.Attach(userSettings);

        context.UserSettings.Remove(userSettings);
        await context.SaveChangesAsync(cancellationToken);
    }
}
