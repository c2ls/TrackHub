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

namespace TrackHub.Router.Domain.Exceptions;

// A manual sync targeted an operator that does not exist or does not belong to the requesting
// account. Mapped to the OPERATOR_NOT_FOUND GraphQL error code: the Router
// returns a typed error instead of a silent false so the caller learns why the trigger was rejected.
public sealed class OperatorNotFoundException(Guid operatorId)
    : Exception($"Operator {operatorId} was not found for the requesting account.")
{
    public Guid OperatorId { get; } = operatorId;
}

// A manual sync targeted a disabled operator. Mapped to the OPERATOR_DISABLED GraphQL error code
//.
public sealed class OperatorDisabledException(Guid operatorId)
    : Exception($"Operator {operatorId} is disabled.")
{
    public Guid OperatorId { get; } = operatorId;
}
