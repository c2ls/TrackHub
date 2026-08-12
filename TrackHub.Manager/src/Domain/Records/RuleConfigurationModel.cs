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

namespace TrackHub.Manager.Domain.Records;

/// <summary>NotificationRule.ConfigurationJson knobs consumed by this module.</summary>
public sealed record RuleConfigurationModel
{
    /// <summary>CommunicationLoss rules: minutes without a position before an alert (default 60).</summary>
    public int? ThresholdMinutes { get; init; }
    /// <summary>Critical alerts unacknowledged this long escalate once to account administrators.</summary>
    public int? EscalateAfterMinutes { get; init; }
    /// <summary>Webhook channel: target URL.</summary>
    public string? WebhookUrl { get; init; }
    /// <summary>Webhook channel: shared secret for the HMAC-SHA256 signature header.</summary>
    public string? WebhookSecret { get; init; }
    /// <summary>Template locale for rendered messages when the recipient has no user language (en/es).</summary>
    public string? Locale { get; init; }
}
