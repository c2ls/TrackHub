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

using TrackHub.Router.Domain.Mappers;

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class DeviceMapperBaseTests
{
    private sealed record Item(string? Id, string? Plate, string? Name, bool Active = true);

    private sealed class KeyedByPlate : DeviceMapperBase<Item>
    {
        protected override string? Key(Item item) => item.Plate;

        protected override string Serial(Item item) => item.Id!;

        protected override string Name(Item item) => item.Name ?? item.Id!;

        protected override int Identifier(Item item, DeviceTransporterVm? assignment) => assignment?.Identifier ?? 0;

        protected override bool IsCatalogued(Item item) => item.Active;
    }

    private static readonly KeyedByPlate Mapper = new();

    [Test]
    public void Map_Catalog_SkipsUncataloguedDevices_AndUsesDefaults()
    {
        var devices = Mapper.Map([new Item("1", "AAA111", "Van"), new Item("2", null, null, Active: false)]).ToList();

        var device = devices.Single();
        Assert.That((device.DeviceId, device.Serial, device.Name, device.Identifier), Is.EqualTo(((Guid?)null, "1", "Van", 0)));
        Assert.That((device.DeviceTypeId, device.TransporterTypeId), Is.EqualTo(((short)DeviceType.Cellular, (short)TransporterType.Truck)));
    }

    [Test]
    public void Map_WithAssignments_MatchesByKey_SkipsDevicesWithoutAKey_AndCarriesTheAssignment()
    {
        var assignment = new DeviceTransporterVm(Guid.NewGuid(), 42, "1", "AAA111", "Truck", (short)TransporterType.Truck);
        var assignments = new Dictionary<string, DeviceTransporterVm> { ["AAA111"] = assignment };

        var devices = Mapper.Map([new Item("1", "AAA111", null), new Item("2", null, null), new Item("3", "BBB222", null)], assignments).ToList();

        var device = devices.Single();
        Assert.That((device.DeviceId, device.Identifier, device.Name), Is.EqualTo(((Guid?)assignment.TransporterId, 42, "1")));
    }

    [TestCase(-1d, 0)]
    [TestCase(7d, 7)]
    [TestCase(5_000_000_000d, 0)]
    public void ToIdentifier_KeepsOnlyIntRangeIds(double? id, int expected)
        => Assert.That(DeviceMapperBase<Item>.ToIdentifier(id), Is.EqualTo(expected));

    private sealed record Numbered(int Id, string Imei, string? Label);

    private sealed class KeyedById : DeviceMapperBase<Numbered, int>
    {
        protected override DeviceType DeviceType => DeviceType.Camera;

        protected override int Key(Numbered device) => device.Id;

        protected override string Serial(Numbered device) => device.Imei;

        protected override string Name(Numbered device) => device.Label ?? device.Imei;

        protected override int Identifier(Numbered device, DeviceTransporterVm? assignment) => device.Id;

        protected override string? ProviderDisplayName(Numbered device) => device.Label;
    }

    [Test]
    public void Map_IntegerKeys_MatchTheAssignment_AndCarryTheOverrides()
    {
        var assignment = new DeviceTransporterVm(Guid.NewGuid(), 7, "imei-7", "Van", "Truck", (short)TransporterType.Truck);

        var device = new KeyedById().Map([new Numbered(7, "imei-7", "Van"), new Numbered(8, "imei-8", null)],
            new Dictionary<int, DeviceTransporterVm> { [7] = assignment }).Single();

        Assert.That((device.Identifier, device.Serial, device.ProviderDisplayName, device.DeviceTypeId),
            Is.EqualTo((7, "imei-7", "Van", (short)DeviceType.Camera)));
    }

    [TestCase(null, 0)]
    [TestCase(double.NaN, 0)]
    [TestCase(2147483647d, int.MaxValue)]
    [TestCase(2147483648d, 0)]
    public void ToIdentifier_HandlesMissingAndBoundaryIds(double? id, int expected)
        => Assert.That(DeviceMapperBase<Item>.ToIdentifier(id), Is.EqualTo(expected));
}
