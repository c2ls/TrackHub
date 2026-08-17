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

using Common.Domain.Constants;

namespace TrackHub.Manager.Application.GpsIntegration.Commands;

public sealed class RegisterManualDeviceValidator : AbstractValidator<RegisterManualDeviceCommand>
{
    public RegisterManualDeviceValidator()
    {
        RuleFor(v => v.Device)
            .NotEmpty();

        RuleFor(v => v.Device.AccountId)
            .NotEmpty();

        RuleFor(v => v.Device.OperatorId)
            .NotEmpty();

        RuleFor(v => v.Device.Name)
            .NotEmpty()
            .MaximumLength(ColumnMetadata.DefaultNameLength);

        RuleFor(v => v.Device.Serial)
            .NotEmpty()
            .MaximumLength(ColumnMetadata.DefaultFieldLength);

        // 0 (or negative) means "allocate one for me" — catalog-less providers have no id.
        RuleFor(v => v.Device.Identifier)
            .GreaterThanOrEqualTo(0);

        RuleFor(v => v.Device.DeviceTypeId)
            .Must(id => Enum.IsDefined(typeof(Common.Domain.Enums.DeviceType), (int)id))
            .WithMessage("DeviceTypeId must be a known device type.");

        RuleFor(v => v.Device.Description)
            .MaximumLength(ColumnMetadata.DefaultDescriptionLength);
    }
}
