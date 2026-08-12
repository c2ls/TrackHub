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

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace TrackHub.Security.Application.Clients.Queries.GetAll;

[Authorize(Resource = Resources.Administrative, Action = Actions.Read)]
public readonly record struct GetClientsQuery(int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<ClientVm>>;

public class GetClientsQueryHandler(IClientReader reader, IConfiguration configuration) : IRequestHandler<GetClientsQuery, IReadOnlyCollection<ClientVm>>
{
    public async Task<IReadOnlyCollection<ClientVm>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Secrets key not found.");

        return await reader.GetClientsAsync(key, request.Skip, request.Take, cancellationToken);
    }

}
