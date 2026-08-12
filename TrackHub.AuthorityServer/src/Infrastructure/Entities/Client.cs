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

using Common.Infrastructure;

namespace TrackHub.AuthorityServer.Infrastructure.Entities;

public sealed class Client(string name, Guid? userId) : BaseAuditableEntity
{
    private User? _user;

    public Guid ClientId { get; private set; } = Guid.NewGuid();
    public Guid? UserId { get; private set; } = userId;
    public string Name { get; set; } = name;

    public User User
    {
        get => _user ?? throw new InvalidOperationException("User is not loaded");
        set => _user = value;
    }
}
