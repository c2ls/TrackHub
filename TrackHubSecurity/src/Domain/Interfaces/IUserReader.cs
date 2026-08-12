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

using Common.Domain.Helpers;

namespace TrackHub.Security.Domain.Interfaces;
public interface IUserReader
{
    Task<string> GetUserNameAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserVm>> GetUsersAsync(Filters filters, int skip, int take, CancellationToken cancellationToken);
    Task<UserVm> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserVm>> GetUsersAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserVm>> GetUsersByRoleAsync(Guid accountId, int roleId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserVm>> GetUsersByPolicyAsync(Guid accountId, int policyId, CancellationToken cancellationToken);
    Task<bool> ValidateEmailAddressAsync(string emailAddress, CancellationToken cancellationToken);
    Task<bool> ValidateEmailAddressAsync(Guid userId, string emailAddress, CancellationToken cancellationToken);
    Task<bool> ValidateUsernameAsync(Guid userId, string username, CancellationToken cancellationToken);
    Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> IsManagerAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> IsManagerAsync(Guid userId, Guid managerId, CancellationToken cancellationToken);
}
