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

namespace TrackHub.Security.Application.Identity.Queries.IsValidServiceForResource;

public readonly record struct IsValidServiceForResourceQuery(string? Client, string Resource, string Action, Guid? AccountId = null, IReadOnlyCollection<string>? Scopes = null, IReadOnlyCollection<string>? Audiences = null) : IRequest<bool>;

public class IsValidServiceForResourceQueryHandler(IIdentityService service, IUser user) : IRequestHandler<IsValidServiceForResourceQuery, bool>
{
    public async Task<bool> Handle(IsValidServiceForResourceQuery request, CancellationToken cancellationToken)
    {
        IdentityCallerGuard.EnsureCallerIsSubjectService(user, request.Client, "IsValidServiceForResource");
        return request.AccountId.HasValue || request.Scopes is { Count: > 0 } || request.Audiences is { Count: > 0 }
            ? await service.IsValidServiceAsync(request.Client, request.Resource, request.Action, request.AccountId, request.Scopes ?? [], request.Audiences ?? [], cancellationToken)
            : await service.IsValidServiceAsync(request.Client, request.Resource, request.Action, cancellationToken);
    }
}
