// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

namespace TrackHub.Security.Application.Users.Commands.Update;

public sealed class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {

        RuleFor(v => v.User)
            .NotEmpty();

        RuleFor(v => v.User.UserId)
            .NotEmpty();

        // Same complexity policy as CreateUser/CreateManager — a password change
        // must not be allowed to weaken the account below the creation policy.
        RuleFor(v => v.User.Password)
            .MinimumLength(ColumnMetadata.MinimumPasswordLength)
            .MaximumLength(ColumnMetadata.DefaultPasswordLength)
            .NotEmpty()
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
