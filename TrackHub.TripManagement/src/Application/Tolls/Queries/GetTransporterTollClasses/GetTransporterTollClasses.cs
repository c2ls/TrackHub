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

namespace TrackHub.TripManagement.Application.Tolls.Queries.GetTransporterTollClasses;

/// <summary>
/// The account's transporter-type → toll-class mappings.
/// <para>
/// Account-scoped and feature-gated, unlike the platform toll catalog (§5): the catalog describes
/// public road infrastructure and carries no <c>AccountId</c>, but fleet composition IS tenant
/// data, which is why this pair lives under <c>Resources.Trips</c> rather than
/// <c>Resources.TollCatalog</c>.
/// </para>
/// <para>
/// Read counterpart of <c>SetTransporterTollClassCommand</c>. The store has always implemented
/// <c>GetMappingsAsync</c>, but nothing exposed it — so an operator could write a mapping and
/// never read back what was already configured, leaving the portal unable to show more than what
/// the current session happened to set.
/// </para>
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct GetTransporterTollClassesQuery() : IRequest<IReadOnlyCollection<TransporterTollClassVm>>;

public sealed class GetTransporterTollClassesQueryHandler(
    ITransporterTollClassStore store,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetTransporterTollClassesQuery, IReadOnlyCollection<TransporterTollClassVm>>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<IReadOnlyCollection<TransporterTollClassVm>> Handle(
        GetTransporterTollClassesQuery request,
        CancellationToken cancellationToken)
    {
        // The account comes from the caller, never the wire (acceptance 1).
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        return await store.GetMappingsAsync(caller.AccountId, cancellationToken);
    }
}
