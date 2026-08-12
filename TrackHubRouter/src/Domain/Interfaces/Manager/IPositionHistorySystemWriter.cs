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

// Appends stored history rows for the storing pipeline with the Router's own service
// identity. Manager enforces the gps.positionHistory flag and per-row idempotency.
public interface IPositionHistorySystemWriter
{
    Task<int> AppendRangeAsync(Guid accountId, Guid operatorId, IEnumerable<PositionVm> positions, CancellationToken cancellationToken);
}
