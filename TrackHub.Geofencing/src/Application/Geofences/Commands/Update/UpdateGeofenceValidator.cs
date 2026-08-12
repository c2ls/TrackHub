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

using TrackHub.Geofencing.Application.Geofences.Commands;
using TrackHub.Geofencing.Application.Geofences.Commands.Update;

namespace TrackHub.Geofencing.Application.Transporters.Commands.Update;

public sealed class UpdateGeofenceValidator : AbstractValidator<UpdateGeofenceCommand>
{
    public UpdateGeofenceValidator()
    {
        RuleFor(v => v.Geofence)
            .NotEmpty()
            .SetValidator(new GeofenceDtoValidator());

        RuleFor(v => v.Geofence.GeofenceId)
            .NotEmpty();
    }
}
