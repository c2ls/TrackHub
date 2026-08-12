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

using TrackHub.Security.Domain.Models;

namespace TrackHub.Security.Domain.Interfaces;

public interface IServiceClientPermissionReader
{
    Task<bool> HasPermissionAsync(string clientId, string resource, string action, CancellationToken cancellationToken);
    Task<bool> HasPermissionAsync(string clientId, string resource, string action, Guid? accountId, IReadOnlyCollection<string> scopes, IReadOnlyCollection<string> audiences, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ServiceClientPermissionVm>> GetServiceClientPermissionsAsync(string? clientId, Guid? accountId, int skip, int take, CancellationToken cancellationToken);
}
