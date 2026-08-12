// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using Common.Application.Extensions;
using Common.Application.GraphQL.Inputs;

namespace TrackHub.Security.Application.Users.Queries.GetAll;

[Authorize(Resource = Resources.Administrative, Action = Actions.Read)]
public readonly record struct GetUsersQuery(FiltersInput Filter, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<UserVm>>;

public class GetUsersMasterQueryHandler(IUserReader reader) : IRequestHandler<GetUsersQuery, IReadOnlyCollection<UserVm>>
{
    public async Task<IReadOnlyCollection<UserVm>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        => await reader.GetUsersAsync(request.Filter.GetFilters(), request.Skip, request.Take, cancellationToken);

}
