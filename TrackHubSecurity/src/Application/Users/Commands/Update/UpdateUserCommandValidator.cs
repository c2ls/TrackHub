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

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    private readonly IUserReader _userReader;

    public UpdateUserCommandValidator(IUserReader userReader)
    {
        _userReader = userReader;

        RuleFor(v => v.User.FirstName)
            .NotEmpty();

        RuleFor(v => v.User.LastName)
            .NotEmpty();

        // Rule to validate the maximum length, non-empty, and uniqueness of the username
        RuleFor(v => v.User.Username)
            .MaximumLength(ColumnMetadata.DefaultUserNameLength)
            .NotEmpty()
            .MustAsync((command, username, cancellationToken) => ValidateUsername(command.User.UserId, username, cancellationToken))
            .WithMessage("Username already in use");

        // Rule to validate the maximum length, non-empty, and uniqueness of the email address
        RuleFor(v => v.User.EmailAddress)
            .MaximumLength(ColumnMetadata.DefaultEmailLength)
            .NotEmpty()
            .MustAsync((command, emailAddress, cancellationToken) => ValidateEmailAddress(command.User.UserId, emailAddress, cancellationToken))
            .WithMessage("Email address already in use");
    }

    // Method to asynchronously validate the uniqueness of the username
    private async Task<bool> ValidateUsername(Guid userId, string username, CancellationToken cancellationToken)
        => await _userReader.ValidateUsernameAsync(userId, username, cancellationToken);

    // Method to asynchronously validate the uniqueness of the email address
    private async Task<bool> ValidateEmailAddress(Guid userId, string emailAddress, CancellationToken cancellationToken)
        => await _userReader.ValidateEmailAddressAsync(userId, emailAddress, cancellationToken);
}
