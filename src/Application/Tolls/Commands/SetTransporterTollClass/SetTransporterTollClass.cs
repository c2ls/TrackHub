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
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.Tolls.Commands.SetTransporterTollClass;

/// <summary>
/// Maps a transporter type — or one specific transporter, as a row-level override — to a toll
/// vehicle class.
/// <para>
/// Unlike the catalog itself, this IS account-scoped and IS feature-gated: fleet composition is
/// tenant business data, so it lives under <see cref="Resources.Trips"/>/<c>Edit</c> rather than
/// under the platform's <see cref="Resources.TollCatalog"/> (spec 11 §5, §7.6).
/// </para>
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct SetTransporterTollClassCommand(
    short? TransporterTypeId,
    Guid? TransporterId,
    string TollVehicleClassCode) : IRequest<TransporterTollClassVm>;

public sealed class SetTransporterTollClassCommandHandler(
    ITransporterTollClassStore store,
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<SetTransporterTollClassCommand, TransporterTollClassVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TransporterTollClassVm> Handle(SetTransporterTollClassCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);

        // A row-level override names a transporter, which is a cross-account reference and must be
        // validated against the trip's account like every other parent (spec 11 §5, acceptance 2).
        // Unvalidated, the mapping was written happily and then simply never applied — the estimate
        // silently fell back to the type-level default with nothing to show the operator why.
        if (request.TransporterId is { } transporterId)
        {
            await TripVisibility.EnsureTransporterVisibleAsync(
                reader, user, caller.AccountId, UserId, transporterId, cancellationToken);
        }

        return await store.SetMappingAsync(
            caller.AccountId, request.TransporterTypeId, request.TransporterId, request.TollVehicleClassCode, cancellationToken);
    }
}

public sealed class SetTransporterTollClassValidator : AbstractValidator<SetTransporterTollClassCommand>
{
    public SetTransporterTollClassValidator()
    {
        RuleFor(v => v.TollVehicleClassCode).NotEmpty().MaximumLength(20);

        // EXACTLY one, mirroring the table's CHECK ((transportertypeid IS NULL) <> (transporterid
        // IS NULL)). The old "at least one" rule let a request carrying both through to Postgres,
        // where the CHECK raised 23514 as an unhandled 500 instead of the 400 a caller can act on.
        // A validator that is weaker than its constraint just relocates the error.
        RuleFor(v => v)
            .Must(v => v.TransporterTypeId.HasValue ^ v.TransporterId.HasValue)
            .WithMessage("Exactly one of a transporter type or a transporter must be supplied, not both.");
    }
}
