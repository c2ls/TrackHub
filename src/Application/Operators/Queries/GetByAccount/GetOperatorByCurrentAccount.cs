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
using Common.Application.Paging;
using Common.Domain.Helpers;
using TrackHub.Manager.Application.Lookups;

namespace TrackHub.Manager.Application.Operators.Queries.GetByAccount;

[Authorize(Resource = Resources.Operators, Action = Actions.Read)]
public readonly record struct GetOperatorByCurrentAccountQuery(
    int? Skip,
    int? Take,
    string? Search) : IRequest<OperatorsPageVm>;

public class GetOperatorsCurrentAccountQueryHandler(IOperatorReader reader, IUserReader userReader, IUser user) : IRequestHandler<GetOperatorByCurrentAccountQuery, OperatorsPageVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<OperatorsPageVm> Handle(GetOperatorByCurrentAccountQuery request, CancellationToken cancellationToken)
    {
        // Get the account associated with the current user
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        // Get the operators associated with the account
        var filters = new Filters(new Dictionary<string, object> {{ "AccountId", user.AccountId }});
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        return await reader.GetOperatorsPageAsync(filters, skip, take, request.Search, cancellationToken);
    }

}

/// <summary>
/// Operator picker feed for the caller's account: id and name only, unpaged, capped by
/// <see cref="LookupLimits.Ceiling"/>.
/// </summary>
[Authorize(Resource = Resources.Operators, Action = Actions.Read)]
public readonly record struct GetOperatorLookupByAccountQuery() : IRequest<IReadOnlyCollection<OperatorLookupVm>>;

public class GetOperatorLookupByAccountQueryHandler(IOperatorReader reader, IUserReader userReader, IUser user)
    : IRequestHandler<GetOperatorLookupByAccountQuery, IReadOnlyCollection<OperatorLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<OperatorLookupVm>> Handle(GetOperatorLookupByAccountQuery request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        var rows = await reader.GetOperatorLookupByAccountAsync(user.AccountId, LookupLimits.FetchSize, cancellationToken);
        return LookupLimits.EnsureWithinCeiling(rows, "operatorLookup");
    }
}
