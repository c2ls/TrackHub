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

namespace TrackHub.Manager.Infrastructure.Entities;

public class TransporterPosition(
    Guid transporterId,
    Guid? geometryId,
    double latitude,
    double longitude,
    double? altitude,
    DateTimeOffset deviceDateTime,
    double speed,
    double? course,
    int? eventId,
    string? address,
    string? city,
    string? state,
    string? country,
    AttributesVm? attributes
    )
{
    private Transporter? _transporter;

    public Guid TransporterPositionId { get; set; }
    public Guid TransporterId { get; set; } = transporterId;
    public Guid? GeometryId { get; set; } = geometryId;
    public double Latitude { get; set; } = latitude;
    public double Longitude { get; set; } = longitude;
    public double? Altitude { get; set; } = altitude;
    public DateTimeOffset DeviceDateTime { get; set; } = deviceDateTime;
    public double Speed { get; set; } = speed;
    public double? Course { get; set; } = course;
    public int? EventId { get; set; } = eventId;
    public string? Address { get; set; } = address;
    public string? City { get; set; } = city;
    public string? State { get; set; } = state;
    public string? Country { get; set; } = country;
    public AttributesVm? Attributes { get; set; } = attributes;

    public Transporter Transporter
    {
        get => _transporter ?? throw new InvalidOperationException("Transporter is not loaded");
        set => _transporter = value;
    }

}
