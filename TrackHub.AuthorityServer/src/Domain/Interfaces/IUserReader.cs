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

using TrackHub.AuthorityServer.Domain.Models;
using TrackHub.AuthorityServer.Domain.Records;

namespace TrackHub.AuthorityServer.Domain.Interfaces;

public interface IUserReader
{
    Task<UserVm> GetUserAsync(UserLoginDto user, CancellationToken cancellationToken);

    Task<UserVm> GetUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// The user's most privileged role name (Administrator &lt; Manager &lt; User by seeded id
    /// order), for the access token's role claim; null when the user has no role row.
    /// </summary>
    Task<string?> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken);
}
