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

using TrackHub.Security.Application.DriverIdentity.Commands;
using TrackHub.Security.Domain.Models;

namespace TrackHub.Security.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<DriverCredentialVm> CreateDriverCredential([Service] ISender sender, CreateDriverCredentialCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> ActivateDriverCredential([Service] ISender sender, ActivateDriverCredentialCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> LockDriverCredential([Service] ISender sender, LockDriverCredentialCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> ResetDriverCredential([Service] ISender sender, ResetDriverCredentialCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> RevokeDriverCredential([Service] ISender sender, RevokeDriverCredentialCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }

    public async Task<DriverDeviceRegistrationVm> RegisterDriverDevice([Service] ISender sender, RegisterDriverDeviceCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdateDriverDevicePushToken([Service] ISender sender, UpdateDriverDevicePushTokenCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> RevokeDriverDevice([Service] ISender sender, RevokeDriverDeviceCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
}
