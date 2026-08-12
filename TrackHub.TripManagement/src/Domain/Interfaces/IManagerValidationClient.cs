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

namespace TrackHub.TripManagement.Domain.Interfaces;

/// <summary>
/// Cross-account reference validation against Manager. Every referenced parent — transporter,
/// driver, document — is checked against the trip's account at write time; a mismatch is a
/// 403/404 per the disclosure rules, never a silent accept (spec 11 §5, acceptance 2).
/// </summary>
public interface IManagerValidationClient
{
    /// <summary>
    /// Manager's <c>validateDriverAssignment</c>. Only the <c>"Transporter"</c> resource type is
    /// accepted today (spec 09); a driver qualifies through an active
    /// <c>DriverTransporterAssignment</c> or their <c>DefaultTransporterId</c>.
    /// </summary>
    Task<bool> ValidateDriverAssignmentAsync(Guid driverId, string resourceType, Guid resourceId, CancellationToken cancellationToken);

    Task<bool> ValidateGroupVisibilityAsync(Guid accountId, Guid userId, string resourceType, Guid resourceId, CancellationToken cancellationToken);

    Task<bool> ValidateFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken);
}
