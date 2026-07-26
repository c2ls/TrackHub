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

namespace TrackHub.Manager.Application.TransporterType.Queries.Get;

[Authorize(Resource = Resources.TransporterType, Action = Actions.Read)]
[PlatformScoped("Platform transporter-type catalog: one seeded set of vehicle types serves every tenant, read by Router's device adapters under global service identities. No tenant owns a row.")]
public readonly record struct GetTransporterTypeQuery(short TransporterTypeId) : IRequest<TransporterTypeVm>;

public class GetTransporterTypeQueryHandler(ITransporterTypeReader reader) : IRequestHandler<GetTransporterTypeQuery, TransporterTypeVm>
{
    public async Task<TransporterTypeVm> Handle(GetTransporterTypeQuery request, CancellationToken cancellationToken)
        => await reader.GetTransporterTypeAsync(request.TransporterTypeId, cancellationToken);

}
