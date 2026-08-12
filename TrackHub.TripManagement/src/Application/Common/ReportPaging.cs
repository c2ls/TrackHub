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

namespace TrackHub.TripManagement.Application.Common;

/// <summary>
/// The one page-size policy every export feed shares. Reporting drains at 500 a page and the
/// governed limits assume that ceiling, so a per-query literal would drift the moment one feed was
/// edited in isolation.
/// </summary>
public static class ReportPaging
{
    public const int DefaultPageSize = 500;
    public const int MaxPageSize = 500;

    public static (int Skip, int Take) Clamp(int? skip, int? take)
        => (Math.Max(skip ?? 0, 0), Math.Clamp(take ?? DefaultPageSize, 1, MaxPageSize));
}
