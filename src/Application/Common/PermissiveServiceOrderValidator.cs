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

namespace TrackHub.TripManagement.Application.Common;

/// <summary>
/// The default <see cref="IServiceOrderValidator"/>: accepts any reference, because spec 12 owns
/// service orders and there is no store in this module to consult. Inventing a cross-service call
/// here was rejected — spec 12 has not defined the contract, and TripManagement must not grow a
/// dependency on a module that does not exist yet.
/// <para>
/// <b>This is a deliberate, bounded permissiveness, not an oversight.</b> It is safe only because
/// the call sites are already correct: <c>CreateTripCommand</c> and <c>UpdateTripCommand</c> ask
/// this port on every write, so the day spec 12 registers a real implementation the reference is
/// enforced everywhere at once, with no handler reopened and nothing to remember. Registration is a
/// plain <c>AddScoped</c> in the Application layer; spec 12's implementation registers in the
/// Infrastructure layer, which runs afterwards and therefore wins — the same override shape
/// <c>FeatureFlagService</c> uses against Common's fail-open default.
/// </para>
/// </summary>
public sealed class PermissiveServiceOrderValidator : IServiceOrderValidator
{
    public Task<bool> ExistsInAccountAsync(Guid serviceOrderId, Guid accountId, CancellationToken cancellationToken)
        => Task.FromResult(true);
}
