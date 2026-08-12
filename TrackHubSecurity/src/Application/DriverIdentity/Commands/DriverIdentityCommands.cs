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

namespace TrackHub.Security.Application.DriverIdentity.Commands;

[Authorize(Resource = Resources.Drivers, Action = Actions.Write)]
public readonly record struct CreateDriverCredentialCommand(DriverCredentialDto Credential) : IRequest<DriverCredentialVm>;
public class CreateDriverCredentialCommandHandler(IDriverIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<CreateDriverCredentialCommand, DriverCredentialVm>
{
    public async Task<DriverCredentialVm> Handle(CreateDriverCredentialCommand request, CancellationToken cancellationToken)
    {
        var vm = await writer.CreateDriverCredentialAsync(request.Credential, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "CreateDriverCredential", "DriverCredential", vm.DriverCredentialId.ToString(), vm.AccountId), cancellationToken);
        return vm;
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Edit)]
public readonly record struct ActivateDriverCredentialCommand(Guid DriverCredentialId, string Password) : IRequest;
public class ActivateDriverCredentialCommandHandler(IDriverIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<ActivateDriverCredentialCommand>
{
    public async Task Handle(ActivateDriverCredentialCommand request, CancellationToken cancellationToken)
    {
        await writer.ActivateDriverCredentialAsync(request.DriverCredentialId, request.Password, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "ActivateDriverCredential", "DriverCredential", request.DriverCredentialId.ToString(), principal.AccountId), cancellationToken);
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Edit)]
public readonly record struct LockDriverCredentialCommand(Guid DriverCredentialId, DateTimeOffset LockedUntil) : IRequest;
public class LockDriverCredentialCommandHandler(IDriverIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<LockDriverCredentialCommand>
{
    public async Task Handle(LockDriverCredentialCommand request, CancellationToken cancellationToken)
    {
        await writer.LockDriverCredentialAsync(request.DriverCredentialId, request.LockedUntil, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "LockDriverCredential", "DriverCredential", request.DriverCredentialId.ToString(), principal.AccountId, null, $"lockedUntil:{request.LockedUntil:O}"), cancellationToken);
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Edit)]
public readonly record struct ResetDriverCredentialCommand(Guid DriverCredentialId, string Password, bool ResetRequired) : IRequest;
public class ResetDriverCredentialCommandHandler(IDriverIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<ResetDriverCredentialCommand>
{
    public async Task Handle(ResetDriverCredentialCommand request, CancellationToken cancellationToken)
    {
        await writer.ResetDriverCredentialAsync(request.DriverCredentialId, request.Password, request.ResetRequired, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "ResetDriverCredential", "DriverCredential", request.DriverCredentialId.ToString(), principal.AccountId), cancellationToken);
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Delete)]
public readonly record struct RevokeDriverCredentialCommand(Guid DriverCredentialId) : IRequest;
public class RevokeDriverCredentialCommandHandler(IDriverIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<RevokeDriverCredentialCommand>
{
    public async Task Handle(RevokeDriverCredentialCommand request, CancellationToken cancellationToken)
    {
        await writer.RevokeDriverCredentialAsync(request.DriverCredentialId, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "RevokeDriverCredential", "DriverCredential", request.DriverCredentialId.ToString(), principal.AccountId), cancellationToken);
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Write)]
public readonly record struct RegisterDriverDeviceCommand(DriverDeviceRegistrationDto Device) : IRequest<DriverDeviceRegistrationVm>;
public class RegisterDriverDeviceCommandHandler(IDriverIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<RegisterDriverDeviceCommand, DriverDeviceRegistrationVm>
{
    public async Task<DriverDeviceRegistrationVm> Handle(RegisterDriverDeviceCommand request, CancellationToken cancellationToken)
    {
        var vm = await writer.RegisterDriverDeviceAsync(request.Device, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "RegisterDriverDevice", "DriverDeviceRegistration", vm.DriverDeviceRegistrationId.ToString(), vm.AccountId), cancellationToken);
        return vm;
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Edit)]
public readonly record struct UpdateDriverDevicePushTokenCommand(Guid DriverDeviceRegistrationId, string? PushToken, string? AppVersion) : IRequest;
public class UpdateDriverDevicePushTokenCommandHandler(IDriverIdentityWriter writer) : IRequestHandler<UpdateDriverDevicePushTokenCommand>
{
    public async Task Handle(UpdateDriverDevicePushTokenCommand request, CancellationToken cancellationToken)
        => await writer.UpdateDriverDevicePushTokenAsync(request.DriverDeviceRegistrationId, request.PushToken, request.AppVersion, cancellationToken);
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Delete)]
public readonly record struct RevokeDriverDeviceCommand(Guid DriverDeviceRegistrationId, string RevokedBy) : IRequest;
public class RevokeDriverDeviceCommandHandler(IDriverIdentityWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<RevokeDriverDeviceCommand>
{
    public async Task Handle(RevokeDriverDeviceCommand request, CancellationToken cancellationToken)
    {
        await writer.RevokeDriverDeviceAsync(request.DriverDeviceRegistrationId, request.RevokedBy, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "RevokeDriverDevice", "DriverDeviceRegistration", request.DriverDeviceRegistrationId.ToString(), principal.AccountId), cancellationToken);
    }
}
