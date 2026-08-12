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

namespace TrackHub.Router.Infrastructure.Wialon.Models;

/// <summary>
/// Response from Wialon token/login API
/// Eid = Session ID, User = User name, Uid = User ID
/// </summary>
internal sealed record LoginResponse(
    string? Eid,          // Session ID
    string? User,         // User name
    long Uid,             // User ID
    long? Error = null    // Wialon error code (e.g. 8 = invalid token)
) : IWialonResponse;
