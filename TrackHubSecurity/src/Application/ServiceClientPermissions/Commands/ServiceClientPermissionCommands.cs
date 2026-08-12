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
using TrackHub.Security.Application.Audit.Events;

namespace TrackHub.Security.Application.ServiceClientPermissions.Commands;

// The account is nested in ServiceClientPermissionDto, so this went unpoliced until TrackHubCommon
// 1.0.7. Resources.ServiceClients is Administrator-only (DBInitializer role matrix), and the only
// caller is the portal's systemadmin console.
[Authorize(Resource = Resources.ServiceClients, Action = Actions.Write)]
[AllowCrossAccount("Platform administration of service-client grants (portal systemadmin console). Binding a PARTNER client to a tenant means writing that tenant's accountid on the grant, so the target account is by definition not the platform operator's own — this is precisely the account-binding that findings.md TS-01 requires before a partner integration can ship. Internal service grants carry a null account and are unaffected.")]
public readonly record struct CreateServiceClientPermissionCommand(ServiceClientPermissionDto Permission) : IRequest<ServiceClientPermissionVm>;
public class CreateServiceClientPermissionCommandHandler(IServiceClientPermissionWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<CreateServiceClientPermissionCommand, ServiceClientPermissionVm>
{
    public async Task<ServiceClientPermissionVm> Handle(CreateServiceClientPermissionCommand request, CancellationToken cancellationToken)
    {
        var vm = await writer.CreateServiceClientPermissionAsync(request.Permission, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "CreateServiceClientPermission", "ServiceClientPermission", vm.ServiceClientPermissionId.ToString(), vm.AccountId, null, $"{vm.ClientId}:{vm.Resource}:{vm.Action}"), cancellationToken);
        return vm;
    }
}

[Authorize(Resource = Resources.ServiceClients, Action = Actions.Edit)]
[AllowCrossAccount("Edit twin of CreateServiceClientPermissionCommand above, same Administrator-only systemadmin console and same reasoning: re-binding a partner client's grant names the PARTNER's account, never the platform operator's own.")]
public readonly record struct UpdateServiceClientPermissionCommand(Guid ServiceClientPermissionId, ServiceClientPermissionDto Permission) : IRequest;
public class UpdateServiceClientPermissionCommandHandler(IServiceClientPermissionWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<UpdateServiceClientPermissionCommand>
{
    public async Task Handle(UpdateServiceClientPermissionCommand request, CancellationToken cancellationToken)
    {
        await writer.UpdateServiceClientPermissionAsync(request.ServiceClientPermissionId, request.Permission, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "UpdateServiceClientPermission", "ServiceClientPermission", request.ServiceClientPermissionId.ToString(), request.Permission.AccountId, null, $"{request.Permission.ClientId}:{request.Permission.Resource}:{request.Permission.Action}"), cancellationToken);
    }
}

[Authorize(Resource = Resources.ServiceClients, Action = Actions.Delete)]
[AllowCrossAccount("Delete twin of CreateServiceClientPermissionCommand above, same Administrator-only systemadmin console and same reasoning: the grant being removed may be bound to a PARTNER's account, never the platform operator's own.")]
public readonly record struct DeleteServiceClientPermissionCommand(Guid ServiceClientPermissionId) : IRequest<Guid>;
public class DeleteServiceClientPermissionCommandHandler(IServiceClientPermissionWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<DeleteServiceClientPermissionCommand, Guid>
{
    public async Task<Guid> Handle(DeleteServiceClientPermissionCommand request, CancellationToken cancellationToken)
    {
        var id = await writer.DeleteServiceClientPermissionAsync(request.ServiceClientPermissionId, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "DeleteServiceClientPermission", "ServiceClientPermission", id.ToString(), null), cancellationToken);
        return id;
    }
}
