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


namespace TrackHub.Security.Application.Action.GetAll;

[Authorize(Resource = Resources.Permissions, Action = Actions.Read)]
public readonly record struct GetActionsQuery() : IRequest<IReadOnlyCollection<ActionVm>>;

// The GetActionsQueryHandler is a class that implements the IRequestHandler interface to handle the GetActionsQuery.
public class GetActionsQueryHandler(IActionReader reader) : IRequestHandler<GetActionsQuery, IReadOnlyCollection<ActionVm>>
{
    // The Handle method is responsible for handling the GetActionsQuery and returning the result.
    // It asynchronously calls the GetActionsAsync method of the IActionReader dependency to retrieve all actions.
    public async Task<IReadOnlyCollection<ActionVm>> Handle(GetActionsQuery request, CancellationToken cancellationToken)
        => await reader.GetActionsAsync(cancellationToken);

}
