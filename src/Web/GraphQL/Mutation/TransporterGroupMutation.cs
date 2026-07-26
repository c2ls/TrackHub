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

using TrackHub.Manager.Application.TransporterGroup.Commands.Create;
using TrackHub.Manager.Application.TransporterGroup.Commands.Delete;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<TransporterGroupVm> CreateTransporterGroup([Service] ISender sender, CreateTransporterGroupCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<Guid> DeleteTransporterGroup([Service] ISender sender, Guid transporterId, long groupId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteTransporterGroupCommand(transporterId, groupId), cancellationToken);
        return transporterId;
    }
}
