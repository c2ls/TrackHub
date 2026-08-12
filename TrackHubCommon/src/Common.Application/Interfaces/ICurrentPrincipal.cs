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

namespace Common.Application.Interfaces;

public enum PrincipalType
{
    Unknown = 0,
    User = 1,
    Driver = 2,
    ServiceClient = 3,
    PublicLink = 4
}

public interface ICurrentPrincipal
{
    string? SubjectId { get; }
    PrincipalType PrincipalType { get; }
    Guid? UserId { get; }
    Guid? DriverId { get; }
    string? ClientId { get; }
    Guid? PublicLinkGrantId { get; }
    string? Role { get; }
    Guid? AccountId { get; }
    IReadOnlyCollection<string> Scopes { get; }
    IReadOnlyCollection<string> Audiences { get; }
    string? CorrelationId { get; }
}
