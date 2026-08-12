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
/// SMTP settings — AppSettings:Smtp, env-overridable in production.
/// Dev points at a local capture container (e.g. smtp4dev); production needs a real relay.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "AppSettings:Smtp";

    public string? Host { get; set; }
    public int Port { get; set; } = 25;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseStartTls { get; set; }
    public string FromAddress { get; set; } = "alerts@trackhub.local";
    public string FromName { get; set; } = "TrackHub Alerts";
}
