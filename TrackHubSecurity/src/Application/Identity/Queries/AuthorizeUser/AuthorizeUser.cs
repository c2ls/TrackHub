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

namespace TrackHub.Security.Application.Identity.Queries.AuthorizeUser;

/// <summary>
/// Combined role + policy authorization decision, evaluated in one call. This is the query
/// every service's authorization pipeline uses; <c>isInRole</c>/<c>authorize</c> remain as
/// the underlying primitives.
/// </summary>
public readonly record struct AuthorizeUserQuery(Guid UserId, string Resource, string Action) : IRequest<bool>;

public class AuthorizeUserQueryHandler(IIdentityService service, IUser user) : IRequestHandler<AuthorizeUserQuery, bool>
{
    public async Task<bool> Handle(AuthorizeUserQuery request, CancellationToken cancellationToken)
    {
        IdentityCallerGuard.EnsureCallerIsSubjectUserOrService(user, request.UserId, "AuthorizeUser");
        return await service.AuthorizeUserAsync(request.UserId, request.Resource, request.Action, cancellationToken);
    }
}

public sealed class AuthorizeUserQueryValidator : AbstractValidator<AuthorizeUserQuery>
{
    public AuthorizeUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
        RuleFor(x => x.Resource)
            .NotEmpty();
        RuleFor(x => x.Action)
            .NotEmpty();
    }
}
