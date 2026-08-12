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

using TrackHub.TripManagement.Application.ProofsOfDelivery.Commands.Record;

namespace TrackHub.TripManagement.Web.GraphQL.Mutation;

/// <summary>
/// Proof-of-delivery capture. Documents are uploaded through Manager's existing REST surface and
/// referenced here by id — no new upload surface is introduced by this module.
/// </summary>
public partial class Mutation
{
    public async Task<ProofOfDeliveryVm> RecordProofOfDelivery([Service] ISender sender, RecordProofOfDeliveryCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);
}
