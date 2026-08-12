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

/// <summary>
/// The single transporter-visibility primitive. Returns the set of transporter
/// ids a user may see within an account: account-wide for Administrator/Manager roles (and global
/// service clients), narrowed to the user's group membership otherwise. The live map, its stored
/// fallback, and the replay group check are all reimplemented on top of this so they can never
/// diverge (resolves K1).
/// </summary>
public interface IVisibleTransporterReader
{
    Task<IReadOnlySet<Guid>> GetVisibleTransporterIdsAsync(Guid userId, Guid accountId, CancellationToken cancellationToken);
}
