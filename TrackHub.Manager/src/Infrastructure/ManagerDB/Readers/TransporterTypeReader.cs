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

using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.Readers;

public sealed class TransporterTypeReader(IApplicationDbContext context) : ITransporterTypeReader
{

    /// <summary>
    /// Retrieve a collection of transporter types asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<TransporterTypeVm>> GetTransporterTypesAsync(CancellationToken cancellationToken)
        => await context.TransporterTypes
            .OrderBy(tt => tt.TransporterTypeId)
            .Select(tt => new TransporterTypeVm(
                tt.TransporterTypeId,
                tt.AccBased,
                tt.StoppedGap,
                tt.MaxDistance,
                tt.MaxTimeGap,
                (TransporterType)tt.TransporterTypeId))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Retrieve a transporter type by ID asynchronously.
    /// </summary>
    /// <param name="transporterTypeId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TransporterTypeVm> GetTransporterTypeAsync(short transporterTypeId, CancellationToken cancellationToken)
        => await context.TransporterTypes
            .Where(d => d.TransporterTypeId == transporterTypeId)
            .Select(tt => new TransporterTypeVm(
                tt.TransporterTypeId,
                tt.AccBased,
                tt.StoppedGap,
                tt.MaxDistance,
                tt.MaxTimeGap,
                (TransporterType)tt.TransporterTypeId))
            .FirstOrDefaultAsync(cancellationToken);

}
