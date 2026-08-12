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

namespace TrackHub.Security.Domain.Models;

public readonly record struct DriverCredentialVm(Guid DriverCredentialId, Guid DriverId, Guid AccountId, string NormalizedLogin, int FailedAttempts, DateTimeOffset? LockedUntil, DateTimeOffset? VerifiedAt, DateTimeOffset? LastLoginAt, bool Active, bool ResetRequired, DateTimeOffset LastModified);

// PushToken is masked (only a trailing fragment is exposed) and RefreshTokenFamilyId is dropped
// entirely: admins need presence/identification, not the sensitive token material.
public readonly record struct DriverDeviceRegistrationVm(Guid DriverDeviceRegistrationId, Guid DriverId, Guid AccountId, string DeviceId, string? DeviceName, string Platform, string? AppVersion, string? PushToken, bool Active, DateTimeOffset RegisteredAt, DateTimeOffset? LastSeenAt, DateTimeOffset? RevokedAt, string? RevokedBy, DateTimeOffset LastModified);
