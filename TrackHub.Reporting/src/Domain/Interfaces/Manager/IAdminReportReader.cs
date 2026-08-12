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

using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces.Manager;

// Reads the Manager admin data the lifecycle/branding reports project. Each call runs
// under the caller's propagated token; cross-account reads require the appropriate Manager permission.
public interface IAdminReportReader
{
    Task<IReadOnlyCollection<AdminAccountVm>> GetAccountsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminAccountFeatureVm>> GetAllAccountFeaturesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminGroupVm>> GetGroupsByAccountAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminUserVm>> GetUsersByGroupAsync(long groupId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminTransporterVm>> GetTransportersByGroupAsync(long groupId, CancellationToken cancellationToken);
}
