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

using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.Operators.Commands.Create;

[Authorize(Resource = Resources.Operators, Action = Actions.Write)]
public readonly record struct CreateOperatorCommand(OperatorDto Operator) : IRequest<OperatorVm>;

public class CreateOperatorCommandHandler(IOperatorWriter writer, IUserReader userReader, IUser user) : IRequestHandler<CreateOperatorCommand, OperatorVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    // This method handles the CreateOperatorCommand and creates an operator.
    public async Task<OperatorVm> Handle(CreateOperatorCommand request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        return await writer.CreateOperatorAsync(request.Operator, user.AccountId, cancellationToken);
    }
}
