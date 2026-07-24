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

namespace TrackHub.Telemetry.Application.TransporterPosition.Queries.GetByOperator;

/// <summary>
/// Bounds the batched live-map read by its INPUT, not its output. This projection is one row per
/// transporter and the map must plot every vehicle it is given — paging the row set would show 50 of
/// 200 vehicles and look like a fleet outage — so the cap is on how many operators a single call may
/// ask about. A caller past the cap is rejected and splits the request; nothing is ever dropped.
/// </summary>
public sealed class GetTransporterPositionsByOperatorsQueryValidator : AbstractValidator<GetTransporterPositionsByOperatorsQuery>
{
    public const int MaxOperators = 200;

    public GetTransporterPositionsByOperatorsQueryValidator()
    {
        RuleFor(x => x.OperatorIds)
            .NotEmpty()
            .WithMessage("At least one operator id is required.");

        RuleFor(x => x.OperatorIds)
            .Must(ids => ids is null || ids.Count <= MaxOperators)
            .WithMessage($"A single request cannot cover more than {MaxOperators} operators; split the request.");
    }
}
