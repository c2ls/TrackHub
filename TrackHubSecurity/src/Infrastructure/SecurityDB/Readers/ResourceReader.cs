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

using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Readers;

public sealed class ResourceReader(IApplicationDbContext context) : IResourceReader
{

    // Get all resources
    public async Task<IReadOnlyCollection<ResourceVm>> GetResourcesAsync(CancellationToken cancellationToken)
        => await context.Resources
            .Select(r => new ResourceVm(r.ResourceId, r.ResourceName, null))
            .ToListAsync(cancellationToken);

}
