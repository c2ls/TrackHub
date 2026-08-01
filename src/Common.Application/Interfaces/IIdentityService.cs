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

namespace Common.Application.Interfaces;

public interface IIdentityService
{
    Task<string> GetUserNameAsync(Guid userId, CancellationToken token);

    /// <summary>
    /// Role path: the user holds a role granted this resource/action.
    /// </summary>
    Task<bool> IsInRoleAsync(Guid userId, string resource, string action, CancellationToken token);

    /// <summary>
    /// Policy path: the user holds at least one POLICY granting this resource/action. Policies are
    /// additive grants used to raise a single user above their role; a resource/action with no
    /// policy attached returns false and leaves the role as the only path.
    /// </summary>
    Task<bool> AuthorizeAsync(Guid userId, string resource, string action, CancellationToken token);

    /// <summary>
    /// Combined role + policy authorization decision for a user, evaluated by Security in a
    /// single call: <see cref="IsInRoleAsync"/> OR <see cref="AuthorizeAsync"/>, either path alone
    /// sufficient. This is the method the authorization pipeline uses; the two remain as the
    /// underlying primitives.
    /// </summary>
    Task<bool> AuthorizeUserAsync(Guid userId, string resource, string action, CancellationToken token);

    Task<bool> IsValidServiceAsync(string? client, CancellationToken token);

    Task<bool> IsValidServiceAsync(string? client, string resource, string action, CancellationToken token);

    Task<bool> IsValidServiceAsync(string? client, string resource, string action, Guid? accountId, IReadOnlyCollection<string> scopes, IReadOnlyCollection<string> audiences, CancellationToken token);
}
