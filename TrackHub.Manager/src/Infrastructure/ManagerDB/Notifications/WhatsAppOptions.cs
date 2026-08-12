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

namespace TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

/// <summary>
/// Meta WhatsApp Business Platform Cloud API settings — AppSettings:WhatsApp.
/// Outbound utility-template sends only; the per-language pre-approved template owns the
/// localization framing and receives the rendered text as its single body parameter.
/// </summary>
public sealed class WhatsAppOptions
{
    public const string SectionName = "AppSettings:WhatsApp";

    public string ApiBaseUrl { get; set; } = "https://graph.facebook.com/v21.0";
    public string? PhoneNumberId { get; set; }
    public string? AccessToken { get; set; }
    /// <summary>Pre-approved generic utility template name receiving the rendered body as {{1}}.</summary>
    public string TemplateName { get; set; } = "trackhub_alert";
}
