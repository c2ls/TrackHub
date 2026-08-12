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

namespace TrackHub.AuthorityServer.Domain.Models;

/// <summary>
/// The account reach a service client's currently effective permission grants declare.
/// <paramref name="AllowsCrossAccount"/> means at least one grant is a declared platform-wide grant,
/// so the token must stay unscoped (no account claim) for those grants to match.
/// <paramref name="AccountIds"/> lists the distinct accounts the client's account-bound grants name.
/// </summary>
public record struct ServiceClientAccountScopeVm(
    bool AllowsCrossAccount,
    IReadOnlyCollection<Guid> AccountIds);
