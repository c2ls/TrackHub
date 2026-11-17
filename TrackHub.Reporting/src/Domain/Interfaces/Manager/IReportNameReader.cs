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

namespace TrackHub.Reporting.Domain.Interfaces.Manager;

// Display names for the ids reports carry. Each lookup returns what the caller may read; a missing
// entry (deleted, retired without the right to list retired units, no permission) leaves the id shown.
public interface IReportNameReader
{
    Task<string?> GetTransporterNameAsync(Guid transporterId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, string>> GetTransporterNamesAsync(IEnumerable<Guid> transporterIds, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, string>> GetOperatorNamesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, string>> GetDeviceSerialsAsync(Guid accountId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, string>> GetStaffNamesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, string>> GetDocumentNamesAsync(IEnumerable<Guid> documentIds, CancellationToken cancellationToken);
}
