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

namespace TrackHub.Router.Domain.Interfaces.Manager;

/// <summary>
/// Latest-position writer that authenticates with the Router's own service identity instead of the
/// calling user's token. Used by on-demand map reads (accounts without gps.integration background
/// sync) so the Router API keeps the latest-position projection current regardless of the calling
/// user's permissions. Positions still only enter the system from provider reads performed by the
/// Router itself.
/// </summary>
public interface IPositionSystemWriter : IPositionWriter;
