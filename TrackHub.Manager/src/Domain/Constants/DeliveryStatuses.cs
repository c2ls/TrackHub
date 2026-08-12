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

namespace TrackHub.Manager.Domain.Constants;

/// <summary>NotificationDelivery lifecycle status values. Stored as strings.</summary>
public static class DeliveryStatuses
{
    public const string Pending = nameof(Pending);
    public const string Sending = nameof(Sending);
    public const string Sent = nameof(Sent);
    public const string Failed = nameof(Failed);
    /// <summary>Deferred into a digest window; the digest job folds these into one summary delivery.</summary>
    public const string Deferred = nameof(Deferred);
    /// <summary>A deferred delivery that has been folded into a digest summary delivery.</summary>
    public const string Digested = nameof(Digested);

    public static readonly IReadOnlyCollection<string> All = [Pending, Sending, Sent, Failed, Deferred, Digested];

    public static bool IsValid(string? value) => value != null && All.Contains(value);
}
