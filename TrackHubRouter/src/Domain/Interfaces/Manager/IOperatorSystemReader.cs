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

namespace TrackHub.Router.Domain.Interfaces.Manager;

/// <summary>
/// Reads operators under the Router's OWN service identity (client credentials), so the operator's
/// credential material comes back decrypted.
/// </summary>
/// <remarks>
/// <para>
/// The Router is the only component that talks to GPS providers, so it is the only component that
/// needs decrypted provider credentials. Manager releases them to a ServiceClient principal, or to a
/// user holding <c>Credentials/Custom</c> — a permission that means "may view credential material"
/// and belongs to credential administration, not to everyday map and sync usage.
/// </para>
/// <para>
/// Separation of concerns: <see cref="IOperatorReader"/> answers <b>which</b> operators the caller may
/// act on (Manager applies that caller's account and group visibility, and redacts credentials);
/// this reader supplies the <b>credential</b> for an operator already resolved that way. Authorization
/// and tenant scope therefore always follow the caller, while secrets never leave the Router.
/// </para>
/// </remarks>
public interface IOperatorSystemReader
{
    /// <summary>Operator by id, with credentials.</summary>
    /// <remarks>
    /// Deliberately the ONLY per-operator read here: Manager admits the Router's account-less
    /// service identity solely through the [AllowCrossAccount] by-id operator query. Resolve the
    /// operator from a transporter under the CALLER's identity (IOperatorReader) first, then read
    /// the credential here by the resolved id — never add a by-transporter read to this interface.
    /// </remarks>
    Task<OperatorVm> GetOperatorAsync(Guid operatorId, CancellationToken cancellationToken);

    /// <summary>Every operator of an account, with credentials.</summary>
    Task<IEnumerable<OperatorVm>> GetOperatorsByAccountsAsync(Guid accountId, CancellationToken cancellationToken);
}
