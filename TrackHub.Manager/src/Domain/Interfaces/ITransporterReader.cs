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

public interface ITransporterReader
{
    Task<TransporterVm> GetTransporterAsync(Guid id, CancellationToken cancellationToken);
    Task<TransporterVm> GetTransporterAsync(string name, CancellationToken cancellationToken);
    Task<TransportersPageVm> GetTransportersByAccountAsync(Guid accountId, int skip, int take, string? search, CancellationToken cancellationToken);
    Task<TransportersPageVm> GetTransportersByGroupAsync(long groupId, int skip, int take, string? search, CancellationToken cancellationToken);
    Task<TransportersPageVm> GetTransportersByUserAsync(Guid userId, int skip, int take, string? search, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransporterLookupVm>> GetTransporterLookupByAccountAsync(Guid accountId, int fetchSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransporterLookupVm>> GetTransporterLookupByUserAsync(Guid userId, int fetchSize, CancellationToken cancellationToken);
    Task<Guid?> GetAccountIdAsync(Guid transporterId, CancellationToken cancellationToken);
}
