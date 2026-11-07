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

using TrackHub.AuthorityServer.Domain.Models;

namespace TrackHub.AuthorityServer.Domain.Interfaces;

public interface IStaffCredentialReader
{
    Task<StaffCredentialAuthenticationVm?> GetStaffCredentialByLoginAsync(string normalizedLogin, CancellationToken cancellationToken);

    /// <summary>The credential a session was issued from; a staff may hold several, and revoking one must end the sessions issued from it.</summary>
    Task<StaffCredentialSessionVm?> GetCredentialSessionAsync(Guid staffCredentialId, CancellationToken cancellationToken);
}
