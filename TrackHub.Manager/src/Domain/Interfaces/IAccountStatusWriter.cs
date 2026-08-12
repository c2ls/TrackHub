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

namespace TrackHub.Manager.Domain.Interfaces;

public interface IAccountStatusWriter
{
    /// <summary>
    /// Applies a validated lifecycle transition: updates <c>Status</c>/<c>StatusChangedAt</c> (and the
    /// derived <c>Active</c>), writes an <c>AccountStatusChanged</c> audit event, and returns the
    /// updated account plus its previous status (for the domain event).
    /// </summary>
    Task<(AccountVm Account, AccountStatus PreviousStatus)> ChangeStatusAsync(
        Guid accountId, AccountStatus targetStatus, string? reason, CancellationToken cancellationToken);
}
