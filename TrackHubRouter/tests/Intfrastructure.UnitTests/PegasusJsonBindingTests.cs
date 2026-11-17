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

using Common.Domain.Extensions;
using TrackHub.Router.Infrastructure.Pegasus.Mappers;
using TrackHub.Router.Infrastructure.Pegasus.Models;

namespace TrackHub.Router.Infrastructure.Pegasus.Tests;

[TestFixture]
public class JsonBindingTests
{
    private static DeviceTransporterVm Device(string serial)
        => new() { TransporterId = Guid.NewGuid(), Serial = serial, Name = "Unit", TransporterType = "Truck" };

    [Test]
    public void LoginResponse_BindsTheDocumentedSample()
    {
        const string json = """
            {
              "message": "User successfully authenticated",
              "app": null,
              "auth": "208b....a0ce"
            }
            """;

        var response = json.Deserialize<LoginResponse>()!;

        Assert.That(response.Auth, Is.EqualTo("208b....a0ce"));
        Assert.That(response.Message, Is.EqualTo("User successfully authenticated"));
    }

    [Test]
    public void PagedVehicles_BindsTheDocumentedSample()
    {
        const string json = """
            {
                "search": { "groups": [ 306 ] },
                "set": 6,
                "total": 17,
                "data": [
                    {
                        "info": {
                            "range_unit": "km", "description": null, "tank_volume": null, "color": null, "make": null,
                            "vin": null, "license_plate": null, "alias": null, "range": null, "year": null, "model": null,
                            "tank_unit": "liter"
                        },
                        "name": "Real Kamaleon 1",
                        "primary": 28,
                        "id": 2839,
                        "device": 867162029806270,
                        "token": null,
                        "groups": [ 28, 207, 306 ],
                        "images": { "photo": false, "on_icon": false, "idle_icon": false, "icon": false, "off_icon": false },
                        "configuration": "0000",
                        "type": "vehicle",
                        "properties": {}
                    }
                ],
                "pages": 3,
                "page": 1
            }
            """;

        var response = json.Deserialize<PagedResponse<Vehicle>>()!;
        var device = DeviceMapper.Instance.Map(response.Data!).Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Page, Is.EqualTo(1));
            Assert.That(response.Pages, Is.EqualTo(3));
            Assert.That(device.Identifier, Is.EqualTo(2839));
            Assert.That(device.Serial, Is.EqualTo("867162029806270"));
            Assert.That(device.Name, Is.EqualTo("Real Kamaleon 1"));
        }
    }

    [Test]
    public void PagedVehiclesWithLatestSelect_BindsTheDocumentedSample()
    {
        const string json = """
            {
                "search": { "groups": [ 306 ] },
                "set": 6,
                "total": 17,
                "data": [
                    {
                        "device": {
                            "imei": 867162029806270,
                            "latest": {
                                "vcounters": {
                                    "vehicle_ecu_eusage": 7849080.000000001,
                                    "vehicle_dev_dist": 198141682.4082619,
                                    "vehicle_ecu_tfuel": 36689.83,
                                    "vehicle_ecu_eidle": 282814022,
                                    "vehicle_dev_ign": 7977182,
                                    "vehicle_dev_idle": 430389.6900012493,
                                    "evtime": 1659975549,
                                    "systime": 1659976062.090836,
                                    "vehicle_ecu_ifuel": 104.44000000000001,
                                    "vehicle_ecu_dist": 106525000
                                },
                                "loc": {
                                    "head": 25,
                                    "code": 0,
                                    "age": 2,
                                    "lon": -72.39416,
                                    "mph": 0,
                                    "label": "kamaldb",
                                    "evtime": 1659975548,
                                    "source": 1,
                                    "valid": true,
                                    "moving": true,
                                    "systime": 1659977251.342421,
                                    "lat": -37.04291,
                                    "trip_id": 283907696662909
                                },
                                "counters": {
                                    "dev_dist": 39741620,
                                    "ecu_ifuel": 104.4,
                                    "ecu_dist": 106607000,
                                    "dev_ign": 7991180,
                                    "dev_idle": 430394.6900012493,
                                    "ecu_eidle": 283285626,
                                    "evtime": 1659975549,
                                    "systime": 1659976062.090836,
                                    "ecu_eusage": 7849080.000000001,
                                    "ecu_tfuel": 53554.8
                                }
                            }
                        },
                        "id": 2839,
                        "name": "Real Kamaleon 1"
                    }
                ],
                "pages": 3,
                "page": 1
            }
            """;
        var device = Device("867162029806270");

        var response = json.Deserialize<PagedResponse<VehicleLatest>>()!;
        var position = response.Data!.MapToPositionVm(new Dictionary<string, DeviceTransporterVm> { [device.Serial] = device }).Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.TransporterId, Is.EqualTo(device.TransporterId));
            Assert.That(position.Latitude, Is.EqualTo(-37.04291));
            Assert.That(position.Longitude, Is.EqualTo(-72.39416));
            Assert.That(position.Course, Is.EqualTo(25));
            Assert.That(position.Speed, Is.Zero);
            Assert.That(position.DeviceDateTime, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1659975548)));
            Assert.That(position.ServerDateTime, Is.EqualTo(DateTimeOffset.FromUnixTimeMilliseconds(1659977251342)));
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(39741620));
            Assert.That(position.Attributes!.Value.Hourmeter, Is.EqualTo(7991180 / 3600d).Within(1e-9));
        }
    }

    [Test]
    public void LatestIos_BindsTheDocumentedDeviceSample()
    {
        const string json = """
            {
              "imei": 814401083083440,
              "latest": {
                "prefix": "API-2::device.device.814401083083440",
                "ios": {
                  "io_pwr": true,
                  "io_out2_short": false,
                  "io_ign": false,
                  "evtime": 1685350875.0,
                  "io_exp_state": false,
                  "io_out1_short": false,
                  "io_out1": false,
                  "io_out2": false,
                  "systime": 1685350912.500692,
                  "io_in1": false,
                  "io_in2": false,
                  "io_in3": false
                }
              }
            }
            """;

        var device = json.Deserialize<LatestDevice>()!;

        Assert.That(device.Imei, Is.EqualTo(814401083083440));
        Assert.That(device.Latest!.Ios!.Io_ign, Is.False);
    }

    [Test]
    public void Rawdata_BindsTheDocumentedSample()
    {
        const string json = """
            {
              "events": [
                {
                  "ecu_battery": null,
                  "head": 7,
                  "code": 47,
                  "hdop": 1.16,
                  "event_time": "2018-09-30T14:14:46",
                  "ecu_distance": null,
                  "vid": 1674,
                  "lon": -80.16431,
                  "sv": 7,
                  "mph": 16,
                  "label": "agglnchng",
                  "valid_position": true,
                  "type": 10,
                  "lat": 25.69373,
                  "system_time": "2018-09-30T14:14:49.760305",
                  "speed": 16,
                  "id": 5014039405,
                  "device_id": 357042060068252
                }
              ]
            }
            """;

        var response = json.Deserialize<RawdataResponse>()!;
        var position = response.Events!.MapToPositionVm(Device("357042060068252")).Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.Latitude, Is.EqualTo(25.69373));
            Assert.That(position.Longitude, Is.EqualTo(-80.16431));
            Assert.That(position.Course, Is.EqualTo(7));
            Assert.That(position.Speed, Is.EqualTo(16 * 1.609344).Within(1e-9));
            Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2018, 9, 30, 14, 14, 46, TimeSpan.Zero)));
            Assert.That(position.ServerDateTime, Is.EqualTo(new DateTimeOffset(2018, 9, 30, 14, 14, 49, TimeSpan.Zero).AddTicks(7603050)));
            Assert.That(position.Attributes!.Value.Satellites, Is.EqualTo(7));
            Assert.That(position.Attributes!.Value.Ignition, Is.Null);
        }
    }
}
