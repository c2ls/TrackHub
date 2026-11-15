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

using System.Text.Json;
using System.Text.Json.Serialization;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.RedGps.Models;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
internal sealed record RedGpsResponse(
    int Status,
    JsonElement Data
);

internal sealed record Vehicle(
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Id,
    string? Nombre,
    string? Patente,
    string? Idgps
);

internal sealed record UnitData(
    string? GpsIdentif,
    string? ReportDate,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Latitude,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Longitude,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Altitude,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? GpsSpeed,
    string? Direction,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Satellites,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Ignition,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Odometer,
    string? Domicilio
);

internal sealed record HistoryPoint(
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Latitud,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Longitud,
    string? Gmt,
    string? Fecha_reporte,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Velocidad,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Orientacion,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Satelites,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Odometro,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Ignicion,
    string? Domicilio
);
