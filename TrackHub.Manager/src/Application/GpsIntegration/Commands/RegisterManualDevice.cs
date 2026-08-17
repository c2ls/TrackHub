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

using Common.Application.Paging;

namespace TrackHub.Manager.Application.GpsIntegration.Commands;

// Manual registration exists for providers without a device-catalog API (e.g. Prosegur):
// device sync can never discover their devices, so operators enter them by hand and the
// position loop picks them up once assigned. Identifier <= 0 lets the writer allocate the
// next free one — catalog-less providers have no numeric id of their own.
[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Write)]
public readonly record struct RegisterManualDeviceCommand(DeviceDto Device, bool AutoAssign = true) : IRequest<DeviceVm>;

public class RegisterManualDeviceCommandHandler(
    IDeviceWriter deviceWriter,
    ITransporterReader transporterReader,
    ITransporterWriter transporterWriter,
    IGroupReader groupReader,
    IGroupWriter groupWriter,
    ITransporterGroupWriter transporterGroupWriter,
    ITransporterDeviceAssignmentWriter assignmentWriter)
    : IRequestHandler<RegisterManualDeviceCommand, DeviceVm>
{
    public async Task<DeviceVm> Handle(RegisterManualDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await deviceWriter.CreateManualDeviceAsync(request.Device, cancellationToken);

        if (request.AutoAssign)
        {
            await AssignToTransporterAsync(device, cancellationToken);
        }

        return device;
    }

    // Mirrors the sync path's adopt-or-create shape (SynchronizeOperatorDevicesCommandHandler),
    // with one deliberate difference: the transporter is named after the DEVICE name first, not
    // ProviderDisplayName — for SOAP providers queried by plate (Prosegur/Rastrack) the entered
    // device name IS the plate, and the Router sends the TRANSPORTER name to the provider.
    private async Task AssignToTransporterAsync(DeviceVm device, CancellationToken cancellationToken)
    {
        var name = FirstNonEmpty(device.Name, device.Serial, $"Device {device.Identifier}");

        var adoptedId = await transporterReader.FindAdoptableTransporterAsync(device.AccountId, name, cancellationToken);
        var transporterId = adoptedId ?? Guid.Empty;
        if (adoptedId is null)
        {
            var transporter = await transporterWriter.CreateTransporterAsync(
                new TransporterDto(
                    name,
                    ResolveTransporterTypeId(device.DeviceTypeId),
                    device.AccountId),
                cancellationToken);
            transporterId = transporter.TransporterId;

            var defaultGroupId = await ResolveDefaultGroupIdAsync(device.AccountId, cancellationToken);
            await transporterGroupWriter.CreateTransporterGroupAsync(
                new TransporterGroupDto(transporterId, defaultGroupId),
                cancellationToken);
        }

        await assignmentWriter.AssignAsync(
            new TransporterDeviceAssignmentDto(
                device.AccountId,
                transporterId,
                device.DeviceId,
                Priority: 0,
                IsPrimary: true,
                AssignmentReason: adoptedId is null
                    ? "Manual registration"
                    : "Manual registration (adopted existing transporter)"),
            cancellationToken);
    }

    // Resolves the account's default group by name, creating it (Active) on first use — same
    // rationale as the sync path: the account read is paged, so search by name.
    private async Task<long> ResolveDefaultGroupIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var groups = await groupReader.GetGroupsByAccountAsync(
            accountId, 0, PageRequest.MaxPageSize, GroupMetadata.DefaultGroupName, cancellationToken);
        var existing = groups.Items.FirstOrDefault(g =>
            string.Equals(g.Name, GroupMetadata.DefaultGroupName, StringComparison.OrdinalIgnoreCase));
        if (existing.GroupId != 0)
        {
            return existing.GroupId;
        }

        var created = await groupWriter.CreateGroupAsync(
            new GroupDto(GroupMetadata.DefaultGroupName, GroupMetadata.DefaultGroupDescription, Active: true),
            accountId,
            cancellationToken);
        return created.GroupId;
    }

    private static string FirstNonEmpty(params string?[] values)
        => values.Select(v => v?.Trim()).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))!;

    private static short ResolveTransporterTypeId(short deviceTypeId)
        => (short)(Enum.IsDefined(typeof(Common.Domain.Enums.DeviceType), (int)deviceTypeId)
            ? (Common.Domain.Enums.DeviceType)deviceTypeId switch
            {
                Common.Domain.Enums.DeviceType.Aviation => Common.Domain.Enums.TransporterType.Aircraft,
                Common.Domain.Enums.DeviceType.Cycling => Common.Domain.Enums.TransporterType.Bicycle,
                Common.Domain.Enums.DeviceType.Drones => Common.Domain.Enums.TransporterType.Drone,
                Common.Domain.Enums.DeviceType.Marine => Common.Domain.Enums.TransporterType.Boat,
                Common.Domain.Enums.DeviceType.PetTracking => Common.Domain.Enums.TransporterType.Pet,
                Common.Domain.Enums.DeviceType.Phone or Common.Domain.Enums.DeviceType.Fitness or Common.Domain.Enums.DeviceType.Smartwatch or Common.Domain.Enums.DeviceType.Wearable => Common.Domain.Enums.TransporterType.Person,
                Common.Domain.Enums.DeviceType.OBDScanner => Common.Domain.Enums.TransporterType.FleetVehicle,
                _ => Common.Domain.Enums.TransporterType.Asset
            }
            : Common.Domain.Enums.TransporterType.Asset);
}
