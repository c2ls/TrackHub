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

namespace TrackHub.Telemetry.Application.GpsIntegration.Queries;

public sealed class GetPositionHistoryRangeQueryValidator : AbstractValidator<GetPositionHistoryRangeQuery>
{
    private const int MaxRangeDays = 31;
    private const int MaxPointsCap = 10000;

    public GetPositionHistoryRangeQueryValidator()
    {
        RuleFor(v => v.AccountId)
            .NotEmpty();

        RuleFor(v => v.TransporterId)
            .NotEmpty();

        RuleFor(v => v)
            .Must(v => v.From < v.To)
            .WithMessage("From must be earlier than To.");

        RuleFor(v => v)
            .Must(v => (v.To - v.From) <= TimeSpan.FromDays(MaxRangeDays))
            .WithMessage($"The requested range exceeds the maximum of {MaxRangeDays} days.");

        RuleFor(v => v.MaxPoints)
            .InclusiveBetween(1, MaxPointsCap);
    }
}
