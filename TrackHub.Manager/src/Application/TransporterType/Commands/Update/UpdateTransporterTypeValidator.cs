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

namespace TrackHub.Manager.Application.TransporterType.Commands.Update;

public sealed class UpdateTransporterTypeValidator : AbstractValidator<UpdateTransporterTypeCommand>
{
    public UpdateTransporterTypeValidator()
    {
        RuleFor(v => v.TransporterType)
            .NotEmpty();

        RuleFor(v => v.TransporterType.TransporterTypeId)
            .NotEmpty();

        RuleFor(v => v.TransporterType.StoppedGap)
            .GreaterThan(0);

        // No rule on AccBased: it is a bool, and NotEmpty() rejects `false`, which made it
        // impossible to switch a transporter type to non-ACC-based tracking.

        RuleFor(v => v.TransporterType.MaxDistance)
            .GreaterThan(0);

        RuleFor(v => v.TransporterType.MaxTimeGap)
            .GreaterThan(0);

    }
}
