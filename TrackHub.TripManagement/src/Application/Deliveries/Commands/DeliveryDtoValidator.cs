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

namespace TrackHub.TripManagement.Application.Deliveries.Commands;

/// <summary>Shared shape validation for the delivery write contract.</summary>
public sealed class DeliveryDtoValidator : AbstractValidator<DeliveryDto>
{
    public DeliveryDtoValidator()
    {
        RuleFor(v => v.Reference).MaximumLength(80);
        RuleFor(v => v.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.BranchName).MaximumLength(200);
        RuleFor(v => v.ProductsSummary).MaximumLength(1000);
        RuleFor(v => v.Observations).MaximumLength(1000);
        RuleFor(v => v.SequenceIndex).GreaterThanOrEqualTo(0);
    }
}
