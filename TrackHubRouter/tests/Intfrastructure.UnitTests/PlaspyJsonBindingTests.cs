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
using TrackHub.Router.Infrastructure.Plaspy.Mappers;
using TrackHub.Router.Infrastructure.Plaspy.Models;

namespace TrackHub.Router.Infrastructure.Plaspy.Tests;

[TestFixture]
public class JsonBindingTests
{
    private static readonly DeviceTransporterVm Device = new() { TransporterId = Guid.NewGuid(), Serial = "device123", Name = "Device 1", TransporterType = "Truck" };

    [Test]
    public void AuthResult_BindsTheDocumentedSuccessSample()
    {
        const string json = """{ "success": true, "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXV...", "expires": "2025-12-31T23:59:59Z"}""";

        var result = json.Deserialize<AuthResult>()!;

        Assert.That(result.Success, Is.True);
        Assert.That(result.Token, Is.EqualTo("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXV..."));
        Assert.That(PositionMapper.ParseUtc(result.Expires!), Is.EqualTo(new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero)));
    }

    [Test]
    public void AuthResult_BindsTheDocumentedErrorSample()
    {
        const string json = """{ "success": false, "error": "Invalid API key or username."}""";

        var result = json.Deserialize<AuthResult>()!;

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid API key or username."));
    }

    [Test]
    public void DevicesResponse_BindsTheDocumentedSample()
    {
        const string json = """
            { "success": true, "apiUsage": 150, "apiDailyUsage": 3000, "devices": [
            { "id": "device123", "name": "Device 1", "description": "Main tracking device", "min": "12345",
            "tags": { "tag1": "value1", "tag2": "value2" }, "expires": "2024-12-31T23:59:59Z" },
            { "id": "device456", "name": "Device 2", "description": "Backup tracking device", "min": "67890",
            "tags": { "tagA": "valueA", "tagB": "valueB" }, "expires": "2025-12-31T23:59:59Z" } ]}
            """;

        var devices = DeviceMapper.Instance.Map(json.Deserialize<DevicesResponse>()!.Devices!).ToList();

        Assert.That(devices.Select(d => (d.Serial, d.Name)), Is.EqualTo(new[] { ("device123", "Device 1"), ("device456", "Device 2") }));
    }

    [Test]
    public void ErrorBody_BindsTheDocumentedSample()
    {
        const string json = """
            {
             "success": false,
             "error": "Bad Request",
             "apiUsage": 150,
             "apiDailyUsage": 3000
            }
            """;

        var response = json.Deserialize<LastLocationResponse>()!;

        Assert.That(response.Success, Is.False);
        Assert.That(response.Error, Is.EqualTo("Bad Request"));
        Assert.That(response.LastLocation, Is.Null);
    }

    [Test]
    public void LastLocationResponse_BindsTheDocumentedSample()
    {
        const string json = """
            {
             "success": true,
             "apiUsage": 150,
             "apiDailyUsage": 3000,
             "lastLocation": {
             "dateTime": "2024-06-02T08:48:25.009Z",
             "latitude": 0,
             "longitude": 0,
             "speed": 0,
             "course": 0,
             "battery": 0,
             "fuel": 0,
             "fuel2": 0,
             "temperature": 0,
             "temperature2": 0,
             "sensorTime1": 0,
             "sensorTime2": 0,
             "sensorTime3": 0,
             "sensorTime4": 0,
             "inactiveSeconds": 0,
             "rpm": 0,
             "milleage": 0,
             "warningTexts": [ "string" ],
             "alertTexts": [ "string" ],
             "alerts": [ "string" ],
             "warnings": [ "string" ],
             "hasCriticalAlert": true,
             "attributes": { "additionalProp1": "string", "additionalProp2": "string", "additionalProp3": "string" },
             "accessoriesAttributes": [ { "additionalProp1": "string", "additionalProp2": "string", "additionalProp3": "string" } ],
             "inputs": [ true ],
             "outputs": [ true ]
             }
            }
            """;

        var response = json.Deserialize<LastLocationResponse>()!;
        var position = response.LastLocation!.MapToPositionVm(Device);

        Assert.That(response.Success, Is.True);
        Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2024, 6, 2, 8, 48, 25, 9, TimeSpan.Zero)));
        Assert.That(position.Attributes!.Value.Mileage, Is.Zero);
    }

    [Test]
    public void LocationsResponse_BindsTheDocumentedSample()
    {
        const string json = """
            {
             "success": true,
             "apiUsage": 150,
             "apiDailyUsage": 3000,
             "locations": [
             {
             "dateTime": "2024-06-01T12:00:00Z",
             "latitude": 40.712776,
             "longitude": -74.005974,
             "speed": 60.5,
             "course": 180.0,
             "battery": 85,
             "fuel": 50,
             "fuel2": 50,
             "temperature": 22.5,
             "temperature2": 22.0,
             "sensorTime1": 1234567890,
             "sensorTime2": 1234567891,
             "sensorTime3": 1234567892,
             "sensorTime4": 1234567893,
             "inactiveSeconds": 120,
             "rpm": 3000,
             "milleage": 15000,
             "warningTexts": ["Engine Check"],
             "alertTexts": ["Speeding"],
             "alerts": ["High Speed"],
             "warnings": ["Low Fuel"],
             "hasCriticalAlert": true,
             "attributes": { "customAttribute1": "value1", "customAttribute2": "value2" },
             "accessoriesAttributes": [ { "accessoryType": "sensor", "accessoryStatus": "active" } ],
             "inputs": [true, false, true],
             "outputs": [false, true]
             }
             ]
            }
            """;

        var position = json.Deserialize<LocationsResponse>()!.Locations!.Single().MapToPositionVm(Device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.Latitude, Is.EqualTo(40.712776));
            Assert.That(position.Longitude, Is.EqualTo(-74.005974));
            Assert.That(position.Speed, Is.EqualTo(60.5));
            Assert.That(position.Course, Is.EqualTo(180));
            Assert.That(position.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero)));
            Assert.That(position.Attributes!.Value.Temperature, Is.EqualTo(22.5));
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(15_000_000));
        }
    }
}
