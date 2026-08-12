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
/// Wialon unit (device) model
/// Id = unit id, Nm = name, Cls = class (2 for avl_unit), Uid = unique id, Pos = last position
/// </summary>
internal readonly record struct Unit(
    long Id,         // unit id
    string Nm,       // name
    int Cls,         // class (should be 2 for units)
    string? Uid,     // unique id
    Position? Pos    // last position
);
