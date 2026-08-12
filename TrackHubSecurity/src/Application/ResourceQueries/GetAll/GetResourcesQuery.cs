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

namespace TrackHub.Security.Application.ResourceQueries.GetAll;

[Authorize(Resource = Resources.Permissions, Action = Actions.Read)]
public readonly record struct GetResourcesQuery() : IRequest<IReadOnlyCollection<ResourceVm>>;

// The GetResourcesQueryHandler is a class that implements the IRequestHandler interface to handle the GetResourcesQuery.
public class GetResourcesQueryHandler(IResourceReader reader) : IRequestHandler<GetResourcesQuery, IReadOnlyCollection<ResourceVm>>
{
    // The Handle method is responsible for handling the GetResourcesQuery and returning the result.
    // It asynchronously calls the GetResourcesAsync method of the IResourceReader dependency to retrieve all resources.
    public async Task<IReadOnlyCollection<ResourceVm>> Handle(GetResourcesQuery request, CancellationToken cancellationToken)
        => await reader.GetResourcesAsync(cancellationToken);

}
