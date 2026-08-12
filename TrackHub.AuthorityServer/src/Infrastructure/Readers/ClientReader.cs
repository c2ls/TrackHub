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

using TrackHub.AuthorityServer.Domain.Interfaces;
using TrackHub.AuthorityServer.Domain.Models;

namespace TrackHub.AuthorityServer.Infrastructure.Readers;

public sealed class ClientReader(SecurityDbContext context) : IClientReader
{
    public async Task<ClientVm> GetClientAsync(string name, CancellationToken cancellationToken)
    {
        return await context.Clients
            .AsNoTracking()
            .Where(c => c.Name.Equals(name))
            .Select(c => new ClientVm(
                c.ClientId,
                c.UserId,
                c.Name))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
