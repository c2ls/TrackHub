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

namespace TrackHub.Router.Domain.Models;

// Provider-neutral position attributes. The five promoted fields stay strongly typed (they are the
// signals every consumer already understands); everything else a provider reports — fuel level,
// RPM, battery voltage, harsh-event flags, door/PTO state, GSM signal, etc. — flows through the
// open Extra bag, so adding a new signal no longer requires a schema change across
// Router/Telemetry/Reporting/portal (router-audit A-03). Extra is a JSON object string (build it
// with AttributesExtra.From) so it is a plain GraphQL `String` on the wire and in the Router's
// output schema, and is stored verbatim in Telemetry's schemaless `attributes` json column.
public readonly record struct AttributesVm(
    bool? Ignition,
    int? Satellites,
    double? Mileage,
    double? Hourmeter,
    double? Temperature,
    string? Extra = null
    );
