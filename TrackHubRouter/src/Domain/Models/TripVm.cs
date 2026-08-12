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

namespace TrackHub.Router.Domain.Models;

public struct TripVm
{
    public Guid TripId { get; set; }
    public List<TripPointVm> Points { get; set; }
    public double TotalDistance { get; set; }
    public TimeSpan Duration { get; set; }
    public double AverageSpeed { get; set; }
    public short Type { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
}
