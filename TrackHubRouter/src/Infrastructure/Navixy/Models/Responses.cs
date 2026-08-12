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

namespace TrackHub.Router.Infrastructure.Navixy.Models;

/// <summary>
/// Navixy reports failures as HTTP 200 with <c>success: false</c> plus a status code. Every
/// response model exposes both so the reader base can distinguish "empty result" from "failed
/// call" (codes 3/4 = invalid/expired session hash → re-auth and retry once; others → throw).
/// </summary>
internal interface INavixyResponse
{
    bool Success { get; }
    NavixyStatus? Status { get; }
}

/// <summary>
/// Error detail Navixy attaches to <c>success: false</c> responses.
/// Code 3 = wrong user hash, 4 = user/session not found — both mean the session is gone.
/// </summary>
internal sealed record NavixyStatus(
    int Code,
    string? Description
);

/// <summary>
/// Response from Navixy tracker/list API
/// </summary>
internal sealed record TrackerListResponse(
    bool Success,
    IEnumerable<Tracker>? List,
    NavixyStatus? Status = null
) : INavixyResponse;

/// <summary>
/// Response from Navixy track/list API
/// </summary>
internal sealed record TrackListResponse(
    bool Success,
    IEnumerable<TrackListEntry>? List,
    NavixyStatus? Status = null
) : INavixyResponse;

/// <summary>
/// Response from Navixy track/read API
/// </summary>
internal sealed record TrackReadResponse(
    bool Success,
    bool Limit_exceeded,
    IEnumerable<TrackPoint>? List,
    NavixyStatus? Status = null
) : INavixyResponse;

/// <summary>
/// Response from Navixy user/auth API
/// </summary>
internal sealed record AuthResponse(
    bool Success,
    string? Hash,
    NavixyStatus? Status = null
) : INavixyResponse;
