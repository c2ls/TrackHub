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

using TrackHub.Security.Domain.Records;

namespace TrackHub.Security.Application.ServiceClientPermissions.Commands;

public sealed class CreateServiceClientPermissionCommandValidator : AbstractValidator<CreateServiceClientPermissionCommand>
{
    public CreateServiceClientPermissionCommandValidator()
        => RuleFor(v => v.Permission).SetValidator(new ServiceClientPermissionDtoValidator());
}

public sealed class UpdateServiceClientPermissionCommandValidator : AbstractValidator<UpdateServiceClientPermissionCommand>
{
    public UpdateServiceClientPermissionCommandValidator()
    {
        RuleFor(v => v.ServiceClientPermissionId).NotEmpty();
        RuleFor(v => v.Permission).SetValidator(new ServiceClientPermissionDtoValidator());
    }
}

public sealed class ServiceClientPermissionDtoValidator : AbstractValidator<ServiceClientPermissionDto>
{
    public ServiceClientPermissionDtoValidator()
    {
        RuleFor(v => v.ClientId).NotEmpty().MaximumLength(150);
        RuleFor(v => v.Resource).NotEmpty().MaximumLength(150);
        RuleFor(v => v.Action).NotEmpty().MaximumLength(150);
        RuleFor(v => v.Scope).NotEmpty().MaximumLength(150);
        RuleFor(v => v.Audience).NotEmpty().MaximumLength(150);
        RuleFor(v => v.EffectiveTo)
            .GreaterThan(v => v.EffectiveFrom)
            .When(v => v.EffectiveFrom.HasValue && v.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be later than EffectiveFrom.");
    }
}
