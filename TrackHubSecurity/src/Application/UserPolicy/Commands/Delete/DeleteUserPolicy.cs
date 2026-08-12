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


using Common.Application.Interfaces;
using TrackHub.Security.Application.Audit.Events;

namespace TrackHub.Security.Application.UserPolicy.Commands.Delete;

[Authorize(Resource = Resources.Users, Action = Actions.Delete)]
public readonly record struct DeleteUserPolicyCommand(Guid UserId, int PolicyId) : IRequest;

public class DeleteUserPolicyCommandHandler(IUserPolicyWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<DeleteUserPolicyCommand>
{
    // This method handles the DeleteUserPolicyCommand by deleting the user policy using the provided writer.
    public async Task Handle(DeleteUserPolicyCommand request, CancellationToken cancellationToken)
    {
        await writer.DeleteUserPolicyAsync(request.UserId, request.PolicyId, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "UserPolicyRemoved", "UserPolicy", $"{request.UserId}:{request.PolicyId}", principal.AccountId), cancellationToken);
    }
}
