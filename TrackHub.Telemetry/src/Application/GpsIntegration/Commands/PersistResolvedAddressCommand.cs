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

namespace TrackHub.Telemetry.Application.GpsIntegration.Commands;

// ServiceClient-only write-back of reverse-geocoded addresses into the existing
// address columns of the stored history row and/or the latest-position row.
// Idempotent: rows that already carry an address are skipped.
[Authorize(Resource = Resources.Positions, Action = Actions.Custom, PrincipalTypes = "ServiceClient")]
[AllowCrossAccount("Router geocoding write-back under the global service identity: keyed by TransporterPositionHistoryId/TransporterId, it stamps reverse-geocoded addresses onto stored position rows across every account the Router syncs. It carries no account and spans tenants by design.")]
public readonly record struct PersistResolvedAddressCommand(
    Guid? TransporterPositionHistoryId,
    Guid? TransporterId,
    string? Address,
    string? City,
    string? State,
    string? Country) : IRequest<bool>;

public class PersistResolvedAddressCommandHandler(IResolvedAddressWriter writer) : IRequestHandler<PersistResolvedAddressCommand, bool>
{
    public async Task<bool> Handle(PersistResolvedAddressCommand request, CancellationToken cancellationToken)
        => await writer.PersistResolvedAddressAsync(
            request.TransporterPositionHistoryId,
            request.TransporterId,
            request.Address,
            request.City,
            request.State,
            request.Country,
            cancellationToken);
}

public sealed class PersistResolvedAddressCommandValidator : AbstractValidator<PersistResolvedAddressCommand>
{
    public PersistResolvedAddressCommandValidator()
    {
        RuleFor(v => v)
            .Must(v => v.TransporterPositionHistoryId.HasValue || v.TransporterId.HasValue)
            .WithMessage("Either TransporterPositionHistoryId or TransporterId is required.");

        RuleFor(v => v.Address)
            .NotEmpty();
    }
}
