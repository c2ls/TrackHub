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

using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

// The latest-position projection is owned by the TrackHub.Telemetry service. Manager only
// removes a transporter's latest position when the transporter itself is deleted (lifecycle cleanup).
public sealed class TransporterPositionWriter(IApplicationDbContext context) : ITransporterPositionWriter
{
    /// <summary>
    /// Removes the latest position of a transporter, if any, when the transporter is deleted.
    /// </summary>
    public async Task DeleteTransporterPositionAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        var position = await context.TransporterPositions.FirstOrDefaultAsync(t => t.TransporterId == transporterId, cancellationToken);

        if (position is not null)
        {
            context.TransporterPositions.Attach(position);

            context.TransporterPositions.Remove(position);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
