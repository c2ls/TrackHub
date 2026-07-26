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

namespace TrackHub.Manager.Application.Operators.Queries.GetByUser;

// Deliberately NOT paged: the Router iterates every operator this user can reach to build the live
// map, so a window would silently drop whole providers from the map. Bounded instead.
[Authorize(Resource = Resources.Operators, Action = Actions.Read)]
public readonly record struct GetOperatorByUserQuery() : IRequest<IReadOnlyCollection<OperatorVm>>;

public class GetOperatorsByUserQueryHandler(IOperatorReader reader, IUser user) : IRequestHandler<GetOperatorByUserQuery, IReadOnlyCollection<OperatorVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<OperatorVm>> Handle(GetOperatorByUserQuery request, CancellationToken cancellationToken)
        => UnpagedReadLimits.EnsureWithinCeiling(
            await reader.GetOperatorsByUserAsync(UserId, cancellationToken),
            "operatorsByUser");

}
