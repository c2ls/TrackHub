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

namespace TrackHub.TripManagement.Domain.Interfaces;

/// <summary>
/// The account-scoped half of toll configuration: which vehicle class a transporter is priced as.
/// Account-scoped precisely because fleet composition is tenant data, unlike the catalog itself.
/// </summary>
public interface ITransporterTollClassStore
{
    /// <summary>
    /// Resolves a trip's default toll class: a row-level <c>TransporterId</c> override wins over
    /// the <c>TransporterTypeId</c> mapping; null when neither is configured.
    /// </summary>
    Task<string?> ResolveClassAsync(Guid accountId, Guid transporterId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TransporterTollClassVm>> GetMappingsAsync(Guid accountId, CancellationToken cancellationToken);

    Task<TransporterTollClassVm> SetMappingAsync(Guid accountId, short? transporterTypeId, Guid? transporterId, string vehicleClassCode, CancellationToken cancellationToken);
}
