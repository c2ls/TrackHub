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
using TrackHub.Manager.Application.Lookups;

namespace TrackHub.Manager.Application.Transporters.Queries.GetLookup;

/// <summary>
/// Account-wide transporter picker feed. Deliberately unpaged: a picker that pages hides options
/// the user must be able to choose. The set is capped by <see cref="LookupLimits.Ceiling"/>, which
/// raises rather than truncates.
/// </summary>
[Authorize(Resource = Resources.Transporters, Action = Actions.Read)]
public readonly record struct GetTransporterLookupByAccountQuery() : IRequest<IReadOnlyCollection<TransporterLookupVm>>;

public class GetTransporterLookupByAccountQueryHandler(ITransporterReader reader, IUserReader userReader, IUser user)
    : IRequestHandler<GetTransporterLookupByAccountQuery, IReadOnlyCollection<TransporterLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<TransporterLookupVm>> Handle(GetTransporterLookupByAccountQuery request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        var rows = await reader.GetTransporterLookupByAccountAsync(user.AccountId, LookupLimits.FetchSize, cancellationToken);
        return LookupLimits.EnsureWithinCeiling(rows, "transporterLookupByAccount");
    }
}

/// <summary>
/// Transporter picker feed narrowed to the caller's group visibility — the lookup counterpart of
/// <c>transportersByUser</c>, for the screens that must not show the whole account.
/// </summary>
[Authorize(Resource = Resources.Transporters, Action = Actions.Read)]
public readonly record struct GetTransporterLookupByUserQuery() : IRequest<IReadOnlyCollection<TransporterLookupVm>>;

public class GetTransporterLookupByUserQueryHandler(ITransporterReader reader, IUser user)
    : IRequestHandler<GetTransporterLookupByUserQuery, IReadOnlyCollection<TransporterLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<TransporterLookupVm>> Handle(GetTransporterLookupByUserQuery request, CancellationToken cancellationToken)
    {
        var rows = await reader.GetTransporterLookupByUserAsync(UserId, LookupLimits.FetchSize, cancellationToken);
        return LookupLimits.EnsureWithinCeiling(rows, "transporterLookupByUser");
    }
}
