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

using TrackHub.Security.Application.StaffIdentity.Commands;
using TrackHub.Security.Domain.Models;

namespace TrackHub.Security.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<StaffCredentialVm> CreateStaffCredential([Service] ISender sender, CreateStaffCredentialCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> ActivateStaffCredential([Service] ISender sender, ActivateStaffCredentialCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> LockStaffCredential([Service] ISender sender, LockStaffCredentialCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> ResetStaffCredential([Service] ISender sender, ResetStaffCredentialCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> RevokeStaffCredential([Service] ISender sender, RevokeStaffCredentialCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> RevokeStaffCredentials([Service] ISender sender, RevokeStaffCredentialsCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }

    public async Task<StaffDeviceRegistrationVm> RegisterStaffDevice([Service] ISender sender, RegisterStaffDeviceCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdateStaffDevicePushToken([Service] ISender sender, UpdateStaffDevicePushTokenCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> RevokeStaffDevice([Service] ISender sender, RevokeStaffDeviceCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
}
