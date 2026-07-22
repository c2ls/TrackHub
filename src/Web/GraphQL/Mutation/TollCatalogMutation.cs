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

using TrackHub.TripManagement.Application.TollCatalog.Commands.Import;
using TrackHub.TripManagement.Application.TollCatalog.Commands.Stations;
using TrackHub.TripManagement.Application.TollCatalog.Commands.Tariffs;
using TrackHub.TripManagement.Application.TollCatalog.Commands.VehicleClasses;
using TrackHub.TripManagement.Application.Tolls.Commands.SetTransporterTollClass;

namespace TrackHub.TripManagement.Web.GraphQL.Mutation;

/// <summary>
/// Administrator toll reference-data administration, plus the one account-scoped mapping
/// (transporter → toll class) that belongs to the tenant rather than to the platform.
/// </summary>
public partial class Mutation
{
    public async Task<TollVehicleClassVm> CreateTollVehicleClass([Service] ISender sender, CreateTollVehicleClassCommand command)
        => await sender.Send(command);

    public async Task<bool> UpdateTollVehicleClass([Service] ISender sender, UpdateTollVehicleClassCommand command)
    {
        await sender.Send(command);
        return true;
    }

    public async Task<Guid> DeactivateTollVehicleClass([Service] ISender sender, Guid id)
        => await sender.Send(new DeactivateTollVehicleClassCommand(id));

    public async Task<TollStationVm> CreateTollStation([Service] ISender sender, CreateTollStationCommand command)
        => await sender.Send(command);

    public async Task<bool> UpdateTollStation([Service] ISender sender, UpdateTollStationCommand command)
    {
        await sender.Send(command);
        return true;
    }

    public async Task<Guid> DeactivateTollStation([Service] ISender sender, Guid id)
        => await sender.Send(new DeactivateTollStationCommand(id));

    public async Task<TollTariffVm> CreateTollTariff([Service] ISender sender, CreateTollTariffCommand command)
        => await sender.Send(command);

    public async Task<bool> UpdateTollTariff([Service] ISender sender, UpdateTollTariffCommand command)
    {
        await sender.Send(command);
        return true;
    }

    public async Task<Guid> DeleteTollTariff([Service] ISender sender, Guid id)
        => await sender.Send(new DeleteTollTariffCommand(id));

    public async Task<TollCatalogImportResultVm> ImportTollCatalog([Service] ISender sender, ImportTollCatalogCommand command)
        => await sender.Send(command);

    public async Task<TransporterTollClassVm> SetTransporterTollClass([Service] ISender sender, SetTransporterTollClassCommand command)
        => await sender.Send(command);
}
