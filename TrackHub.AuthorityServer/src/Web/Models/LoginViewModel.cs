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

using System.ComponentModel.DataAnnotations;
using TrackHub.AuthorityServer.Web.Helpers;

namespace TrackHub.AuthorityServer.Web.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessageResourceName = "EmailRequired", ErrorMessageResourceType = typeof(ValidationMessages))]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessageResourceName = "PasswordRequired", ErrorMessageResourceType = typeof(ValidationMessages))]
    [StringLength(100, ErrorMessageResourceName = "PasswordLength", ErrorMessageResourceType = typeof(ValidationMessages), MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = "?";
    public bool AuthenticationFailed { get; set; }
    public string AuthenticationFailedMessage { get; set; } = string.Empty;

    /// <summary>
    /// Absolute URL of the portal's public status page, or null when no portal origin is
    /// configured (the view then omits the link). Never hardcoded — see LoginController.
    /// </summary>
    public string? PlatformStatusUrl { get; set; }
}
