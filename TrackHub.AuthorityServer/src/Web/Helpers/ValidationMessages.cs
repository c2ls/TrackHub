// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using Common.Domain.Localization;

namespace TrackHub.AuthorityServer.Web.Helpers;

// Request-culture lookups through the shared Common ResourceLocalizer (UseRequestLocalization
// sets the ambient culture); the resx files stay the single source of the texts.
public static class ValidationMessages
{
    private static readonly ResourceLocalizer Localizer = new("TrackHub.AuthorityServer.Web.Resources.ValidationMessages", typeof(ValidationMessages).Assembly);

    public static string EmailRequired => Localizer.GetString("EmailRequired");
    public static string InvalidEmailAddress => Localizer.GetString("InvalidEmail");
    public static string PasswordRequired => Localizer.GetString("PasswordRequired");
    public static string PasswordLength => Localizer.GetString("InvalidPassword");
}
