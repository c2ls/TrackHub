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

namespace TrackHub.TripManagement.Domain.Exceptions;

/// <summary>
/// Thrown by a routing adapter when the provider is unreachable, rate-limited past its retry
/// budget, or returns an unusable response. It never escapes to a trip command's caller: the
/// planning handler catches it and records a <c>Failed</c> route plan carrying
/// <paramref name="errorCode"/>, leaving the trip fully usable (spec 11 §7.3, acceptance 18).
/// </summary>
public sealed class RoutingUnavailableException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
