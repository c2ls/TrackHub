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

using Common.Infrastructure;

namespace TrackHub.Security.Infrastructure.Entities;

public sealed class DriverDeviceRegistration(
    Guid driverId,
    Guid accountId,
    string deviceId,
    string? deviceName,
    string platform,
    string? appVersion,
    string? pushToken,
    string refreshTokenFamilyId) : BaseAuditableEntity
{
    public Guid DriverDeviceRegistrationId { get; private set; } = Guid.NewGuid();
    public Guid DriverId { get; set; } = driverId;
    public Guid AccountId { get; set; } = accountId;
    public string DeviceId { get; set; } = deviceId;
    public string? DeviceName { get; set; } = deviceName;
    public string Platform { get; set; } = platform;
    public string? AppVersion { get; set; } = appVersion;
    public string? PushToken { get; set; } = pushToken;
    public string RefreshTokenFamilyId { get; set; } = refreshTokenFamilyId;
    public bool Active { get; set; } = true;
    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
}
