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

namespace TrackHub.Security.Infrastructure.SecurityDB.Entities;
public sealed class UserRole
{
    private Role? _role;
    private User? _user;

    public required Guid UserId { get; set; }
    public required int RoleId { get; set; }

    public Role Role
    {
        get => _role ?? throw new InvalidOperationException("Role is not loaded");
        set => _role = value;
    }
    public User User
    {
        get => _user ?? throw new InvalidOperationException("User is not loaded");
        set => _user = value;
    }
}
