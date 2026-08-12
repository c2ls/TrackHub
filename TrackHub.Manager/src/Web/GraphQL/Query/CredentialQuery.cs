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

using TrackHub.Manager.Application.Credentials.Queries.Get;
using TrackHub.Manager.Application.Credentials.Queries.GetByOperator;
using TrackHub.Manager.Application.CredentialToken.Queries.GetToken;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<CredentialVm> GetCredential([Service] ISender sender, [AsParameters] GetCredentialQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<CredentialVm> GetCredentialByOperator([Service] ISender sender, [AsParameters] GetCredentialByOperatorQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TokenVm> GetToken([Service] ISender sender, [AsParameters] GetTokenQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
