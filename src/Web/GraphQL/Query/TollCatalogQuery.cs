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

using TrackHub.TripManagement.Application.TollCatalog.Queries;
using TrackHub.TripManagement.Application.Tolls.Queries.EstimateTolls;
using TrackHub.TripManagement.Application.Tolls.Queries.GetTransporterTollClasses;

namespace TrackHub.TripManagement.Web.GraphQL.Query;

/// <summary>Toll reference-data reads plus the planner's what-if estimate.</summary>
public partial class Query
{
    public async Task<TollStationsPageVm> GetTollStations([Service] ISender sender, [AsParameters] GetTollStationsQuery query)
        => await sender.Send(query);

    public async Task<TollStationDetailVm> GetTollStationDetail([Service] ISender sender, [AsParameters] GetTollStationDetailQuery query)
        => await sender.Send(query);

    public async Task<IReadOnlyCollection<TollVehicleClassVm>> GetTollVehicleClasses([Service] ISender sender)
        => await sender.Send(new GetTollVehicleClassesQuery());

    /// <summary>The account's transporter -> toll-class mappings (account-scoped, unlike the catalog).</summary>
    public async Task<IReadOnlyCollection<TransporterTollClassVm>> GetTransporterTollClasses([Service] ISender sender)
        => await sender.Send(new GetTransporterTollClassesQuery());

    public async Task<TollEstimateVm> EstimateTolls([Service] ISender sender, [AsParameters] EstimateTollsQuery query)
        => await sender.Send(query);
}
