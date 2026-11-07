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

namespace TrackHub.Security.Application.StaffIdentity.Commands;

[Authorize(Resource = Resources.Staff, Action = Actions.Write)]
// Enforcement: StaffIdentityWriter requires account access and that the staff belongs to it.
[AccountScopeEnforcedInHandler]
public readonly record struct CreateStaffCredentialCommand(StaffCredentialDto Credential) : IRequest<StaffCredentialVm>;
public class CreateStaffCredentialCommandHandler(IStaffIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<CreateStaffCredentialCommand, StaffCredentialVm>
{
    public async Task<StaffCredentialVm> Handle(CreateStaffCredentialCommand request, CancellationToken cancellationToken)
    {
        var vm = await writer.CreateStaffCredentialAsync(request.Credential, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "CreateStaffCredential", "StaffCredential", vm.StaffCredentialId.ToString(), vm.AccountId), cancellationToken);
        return vm;
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Edit)]
// Enforcement: StaffIdentityWriter loads the row and calls RequireAccountAccess on its owning
// account before mutating it.
[AccountScopeEnforcedInHandler]public readonly record struct ActivateStaffCredentialCommand(Guid StaffCredentialId, string Password) : IRequest;
public class ActivateStaffCredentialCommandHandler(IStaffIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<ActivateStaffCredentialCommand>
{
    public async Task Handle(ActivateStaffCredentialCommand request, CancellationToken cancellationToken)
    {
        await writer.ActivateStaffCredentialAsync(request.StaffCredentialId, request.Password, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "ActivateStaffCredential", "StaffCredential", request.StaffCredentialId.ToString(), principal.AccountId), cancellationToken);
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Edit)]
// Enforcement: StaffIdentityWriter loads the row and calls RequireAccountAccess on its owning
// account before mutating it.
[AccountScopeEnforcedInHandler]public readonly record struct LockStaffCredentialCommand(Guid StaffCredentialId, DateTimeOffset LockedUntil) : IRequest;
public class LockStaffCredentialCommandHandler(IStaffIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<LockStaffCredentialCommand>
{
    public async Task Handle(LockStaffCredentialCommand request, CancellationToken cancellationToken)
    {
        await writer.LockStaffCredentialAsync(request.StaffCredentialId, request.LockedUntil, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "LockStaffCredential", "StaffCredential", request.StaffCredentialId.ToString(), principal.AccountId, null, $"lockedUntil:{request.LockedUntil:O}"), cancellationToken);
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Edit)]
// Enforcement: StaffIdentityWriter loads the row and calls RequireAccountAccess on its owning
// account before mutating it.
[AccountScopeEnforcedInHandler]public readonly record struct ResetStaffCredentialCommand(Guid StaffCredentialId, string Password, bool ResetRequired) : IRequest;
public class ResetStaffCredentialCommandHandler(IStaffIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<ResetStaffCredentialCommand>
{
    public async Task Handle(ResetStaffCredentialCommand request, CancellationToken cancellationToken)
    {
        await writer.ResetStaffCredentialAsync(request.StaffCredentialId, request.Password, request.ResetRequired, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "ResetStaffCredential", "StaffCredential", request.StaffCredentialId.ToString(), principal.AccountId), cancellationToken);
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Delete)]
// Enforcement: StaffIdentityWriter loads the row and calls RequireAccountAccess on its owning
// account before mutating it.
[AccountScopeEnforcedInHandler]public readonly record struct RevokeStaffCredentialCommand(Guid StaffCredentialId) : IRequest;
public class RevokeStaffCredentialCommandHandler(IStaffIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<RevokeStaffCredentialCommand>
{
    public async Task Handle(RevokeStaffCredentialCommand request, CancellationToken cancellationToken)
    {
        await writer.RevokeStaffCredentialAsync(request.StaffCredentialId, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "RevokeStaffCredential", "StaffCredential", request.StaffCredentialId.ToString(), principal.AccountId), cancellationToken);
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Write)]
// Enforcement: StaffIdentityWriter requires account access and that the staff belongs to it.
[AccountScopeEnforcedInHandler]
public readonly record struct RegisterStaffDeviceCommand(StaffDeviceRegistrationDto Device) : IRequest<StaffDeviceRegistrationVm>;
public class RegisterStaffDeviceCommandHandler(IStaffIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<RegisterStaffDeviceCommand, StaffDeviceRegistrationVm>
{
    public async Task<StaffDeviceRegistrationVm> Handle(RegisterStaffDeviceCommand request, CancellationToken cancellationToken)
    {
        var vm = await writer.RegisterStaffDeviceAsync(request.Device, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "RegisterStaffDevice", "StaffDeviceRegistration", vm.StaffDeviceRegistrationId.ToString(), vm.AccountId), cancellationToken);
        return vm;
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Edit)]
// Enforcement: StaffIdentityWriter loads the row and calls RequireAccountAccess on its owning
// account before mutating it.
[AccountScopeEnforcedInHandler]public readonly record struct UpdateStaffDevicePushTokenCommand(Guid StaffDeviceRegistrationId, string? PushToken, string? AppVersion) : IRequest;
public class UpdateStaffDevicePushTokenCommandHandler(IStaffIdentityWriter writer) : IRequestHandler<UpdateStaffDevicePushTokenCommand>
{
    public async Task Handle(UpdateStaffDevicePushTokenCommand request, CancellationToken cancellationToken)
        => await writer.UpdateStaffDevicePushTokenAsync(request.StaffDeviceRegistrationId, request.PushToken, request.AppVersion, cancellationToken);
}

[Authorize(Resource = Resources.Staff, Action = Actions.Delete)]
// Enforcement: StaffIdentityWriter loads the row and calls RequireAccountAccess on its owning
// account before mutating it.
[AccountScopeEnforcedInHandler]public readonly record struct RevokeStaffDeviceCommand(Guid StaffDeviceRegistrationId, string RevokedBy) : IRequest;
public class RevokeStaffDeviceCommandHandler(IStaffIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<RevokeStaffDeviceCommand>
{
    public async Task Handle(RevokeStaffDeviceCommand request, CancellationToken cancellationToken)
    {
        await writer.RevokeStaffDeviceAsync(request.StaffDeviceRegistrationId, request.RevokedBy, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "RevokeStaffDevice", "StaffDeviceRegistration", request.StaffDeviceRegistrationId.ToString(), principal.AccountId), cancellationToken);
    }
}

/// <summary>
/// Ends every live credential of a staff Manager just deactivated. ServiceClient-only: Manager's
/// global manager_client identity carries no account claim, so the command names the account and
/// the writer scopes on both ids.
/// </summary>
[Authorize(Resource = Resources.Staff, Action = Actions.Custom, PrincipalTypes = "ServiceClient")]
[AllowCrossAccount("Staff offboarding: Manager's global manager_client identity carries no account claim; the command names the staff and its account and only ever deactivates that staff's credentials.")]
public readonly record struct RevokeStaffCredentialsCommand(Guid StaffId, Guid AccountId) : IRequest;
public class RevokeStaffCredentialsCommandHandler(IStaffIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<RevokeStaffCredentialsCommand>
{
    public async Task Handle(RevokeStaffCredentialsCommand request, CancellationToken cancellationToken)
    {
        await writer.RevokeStaffCredentialsAsync(request.StaffId, request.AccountId, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "RevokeStaffCredentials", "Staff", request.StaffId.ToString(), request.AccountId), cancellationToken);
    }
}
public class RevokeStaffCredentialsCommandValidator : AbstractValidator<RevokeStaffCredentialsCommand>
{
    public RevokeStaffCredentialsCommandValidator()
    {
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
    }
}
