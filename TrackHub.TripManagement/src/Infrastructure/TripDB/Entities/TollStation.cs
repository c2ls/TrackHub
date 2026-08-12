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

using Common.Infrastructure;
using NetTopologySuite.Geometries;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Entities;

/// <summary>
/// Platform toll reference data - no AccountId (spec 11 section 5). <see cref="Point"/> carries a
/// GiST index: route matching is an ST_DWithin against it (spec 11 section 6.2).
/// </summary>
public sealed class TollStation : BaseAuditableEntity
{
    public Guid TollStationId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Point Point { get; set; } = default!;

    /// <summary>ISO 3166-1 alpha-2.</summary>
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? RoadName { get; set; }

    /// <summary>Free text (e.g. North / Both); null means bidirectional.</summary>
    public string? Direction { get; set; }
    public string? Operator { get; set; }
    public string? Notes { get; set; }
    public bool Active { get; set; } = true;
}
