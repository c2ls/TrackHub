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
public sealed class ResourceAction
{
    private Action? _action;
    private Resource? _resource;

    public required int ActionId { get; set; }
    public required int ResourceId { get; set; }

    public Action Action
    {
        get => _action ?? throw new InvalidOperationException("Action is not loaded");
        set => _action = value;
    }
    public Resource Resource
    {
        get => _resource ?? throw new InvalidOperationException("Resource is not loaded");
        set => _resource = value;
    }
}
