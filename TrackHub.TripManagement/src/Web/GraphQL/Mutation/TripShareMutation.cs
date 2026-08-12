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

using TrackHub.TripManagement.Application.TripShares.Commands.Revoke;
using TrackHub.TripManagement.Application.TripShares.Commands.Share;

namespace TrackHub.TripManagement.Web.GraphQL.Mutation;

/// <summary>Public tracking links. The plaintext token is returned only by <c>shareTrip</c>.</summary>
public partial class Mutation
{
    public async Task<TripShareVm> ShareTrip([Service] ISender sender, ShareTripCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<Guid> RevokeTripShare([Service] ISender sender, RevokeTripShareCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);
}
