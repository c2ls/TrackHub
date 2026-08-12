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

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class User(Guid userId,
    string username,
    bool active,
    Guid accountId)
{
    private Account? _account;

    public Guid UserId { get; set; } = userId;
    public string Username { get; set; } = username;
    public bool Active { get; set; } = active;
    public Guid AccountId { get; set; } = accountId;
    public ICollection<Group> Groups { get; set; } = [];
    public UserSettings? UserSettings { get; set; }

    public Account Account
    {
        get => _account ?? throw new InvalidOperationException("Account is not loaded");
        set => _account = value;
    }
}
