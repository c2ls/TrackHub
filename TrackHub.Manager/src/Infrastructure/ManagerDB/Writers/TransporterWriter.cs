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

using Common.Application.Interfaces;
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;
using TransporterType = Common.Domain.Enums.TransporterType;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;
// This class represents a writer for the Transporter entity
public sealed class TransporterWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), ITransporterWriter
{
    // Creates a new transporter asynchronously
    // Parameters:
    // - transporterDto: The transporter data transfer object
    // - cancellationToken: The cancellation token
    // Returns:
    // - The created transporter view model
    public async Task<TransporterVm> CreateTransporterAsync(TransporterDto transporterDto, CancellationToken cancellationToken)
    {
        var transporter = new Transporter(
            transporterDto.Name,
            transporterDto.TransporterTypeId,
            transporterDto.AccountId);

        await Context.Transporters.AddAsync(transporter, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return new TransporterVm(
            transporter.TransporterId,
            transporter.Name,
            (TransporterType)transporter.TransporterTypeId,
            transporter.TransporterTypeId);
    }

    // Updates an existing transporter asynchronously
    // Parameters:
    // - transporterDto: The updated transporter data transfer object
    // - cancellationToken: The cancellation token
    public async Task UpdateTransporterAsync(UpdateTransporterDto transporterDto, CancellationToken cancellationToken)
    {
        var transporter = await Context.Transporters.FindAsync(transporterDto.TransporterId, cancellationToken)
            ?? throw new NotFoundException(nameof(Transporter), $"{transporterDto.TransporterId}");
        RequireAccountWriteAccess(transporter.AccountId);

        Context.Transporters.Attach(transporter);

        transporter.Name = transporterDto.Name;
        transporter.TransporterTypeId = transporterDto.TransporterTypeId;

        await Context.SaveChangesAsync(cancellationToken);
    }

    // Deletes a transporter asynchronously
    // Parameters:
    // - transporterId: The ID of the transporter to delete
    // - cancellationToken: The cancellation token
    public async Task DeleteTransporterAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        var transporter = await Context.Transporters.FindAsync(transporterId, cancellationToken)
            ?? throw new NotFoundException(nameof(Transporter), $"{transporterId}");
        RequireAccountWriteAccess(transporter.AccountId);

        Context.Transporters.Attach(transporter);

        Context.Transporters.Remove(transporter);
        await Context.SaveChangesAsync(cancellationToken);
    }

}
