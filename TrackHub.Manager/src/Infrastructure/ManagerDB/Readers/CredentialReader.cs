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

using Common.Domain.Extensions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

// Represents a reader for retrieving credentials
public sealed class CredentialReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), ICredentialReader
{
    // Retrieves a credential asynchronously
    // Parameters:
    //   id: The ID of the credential
    //   key: The encryption key
    //   cancellationToken: The cancellation token
    // Returns:
    //   A CredentialVm object representing the retrieved credential
    public async Task<CredentialVm> GetCredentialAsync(Guid id, string key, CancellationToken cancellationToken)
    {
        var credential = await Context.Credentials
            .Where(c => c.CredentialId.Equals(id))
            .Select(c => new
            {
                c.CredentialId,
                c.Uri,
                c.Username,
                c.Password,
                c.Key,
                c.Key2,
                c.Salt,
                c.Operator.AccountId
            })
            .FirstAsync(cancellationToken);
        RequireAccountAccess(credential.AccountId);
        var salt = Convert.FromBase64String(credential.Salt);
        return new CredentialVm(
            credential.CredentialId,
            credential.Uri,
            credential.Username.DecryptData(key, salt),
            credential.Password.DecryptData(key, salt),
            credential.Key?.DecryptData(key, salt),
            credential.Key2?.DecryptData(key, salt));
    }


    public async Task<CredentialVm> GetCredentialByOperatorAsync(Guid operatorId, string key, CancellationToken cancellationToken)
    {
        var credential = await Context.Credentials
            .Where(c => c.OperatorId.Equals(operatorId))
            .Select(c => new
            {
                c.CredentialId,
                c.Uri,
                c.Username,
                c.Password,
                c.Key,
                c.Key2,
                c.Salt,
                c.Operator.AccountId
            })
            .FirstAsync(cancellationToken);
        RequireAccountAccess(credential.AccountId);
        var salt = Convert.FromBase64String(credential.Salt);
        return new CredentialVm(
            credential.CredentialId,
            credential.Uri,
            credential.Username.DecryptData(key, salt),
            credential.Password.DecryptData(key, salt),
            credential.Key?.DecryptData(key, salt),
            credential.Key2?.DecryptData(key, salt));
    }

    // Retrieves a token asynchronously
    // Parameters:
    //   id: The ID of the credential
    //   key: The encryption key
    //   cancellationToken: The cancellation token
    // Returns:
    //   A TokenVm object representing the retrieved token
    // Remarks:
    //   This method might not be needed
    public async Task<TokenVm> GetTokenAsync(Guid id, string key, CancellationToken cancellationToken)
    {
        var token = await Context.Credentials
            .Where(c => c.CredentialId.Equals(id))
            .Select(c => new
            {
                c.Token,
                c.TokenExpiration,
                c.RefreshToken,
                c.RefreshTokenExpiration,
                c.Salt,
                c.Operator.AccountId
            })
            .FirstAsync(cancellationToken);
        RequireAccountAccess(token.AccountId);
        var salt = Convert.FromBase64String(token.Salt);
        return new TokenVm(
            token.Token != null ? token.Token.DecryptData(key, salt) : null,
            token.TokenExpiration,
            token.RefreshToken != null ? token.RefreshToken.DecryptData(key, salt) : null,
            token.RefreshTokenExpiration);
    }

    public async Task<CredentialMetadataVm?> GetMetadataByOperatorAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        var c = await Context.Credentials
            .Where(x => x.OperatorId == operatorId)
            .Select(c => new
            {
                c.CredentialId,
                c.OperatorId,
                c.Uri,
                c.Username,
                c.Salt,
                HasPassword = c.Password != null,
                HasKey = c.Key != null,
                HasKey2 = c.Key2 != null,
                HasToken = c.Token != null,
                HasRefreshToken = c.RefreshToken != null,
                c.TokenExpiration,
                c.RefreshTokenExpiration,
                c.CredentialVersion,
                c.RotatedAt,
                c.RotatedByPrincipalType,
                c.RotatedByPrincipalId,
                c.Operator.AccountId
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (c is null)
        {
            return null;
        }
        RequireAccountAccess(c.AccountId);
        return new CredentialMetadataVm(
            c.CredentialId,
            c.OperatorId,
            c.Uri,
            MaskUsername(c.Username, key: null, salt: c.Salt),
            c.HasPassword,
            c.HasKey,
            c.HasKey2,
            c.HasToken,
            c.HasRefreshToken,
            c.TokenExpiration,
            c.RefreshTokenExpiration,
            c.CredentialVersion,
            c.RotatedAt,
            c.RotatedByPrincipalType,
            c.RotatedByPrincipalId);
    }

    public async Task<IReadOnlyCollection<ExpiringCredentialVm>> GetExpiringCredentialsAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        return await Context.Credentials
            .Where(c => (c.TokenExpiration.HasValue && c.TokenExpiration <= cutoff)
                || (c.RefreshTokenExpiration.HasValue && c.RefreshTokenExpiration <= cutoff))
            .Select(c => new ExpiringCredentialVm(
                c.CredentialId,
                c.OperatorId,
                c.Operator.AccountId,
                c.TokenExpiration,
                c.RefreshTokenExpiration,
                c.TokenExpiration.HasValue && c.RefreshTokenExpiration.HasValue
                    ? (c.TokenExpiration < c.RefreshTokenExpiration ? c.TokenExpiration : c.RefreshTokenExpiration)
                    : (c.TokenExpiration ?? c.RefreshTokenExpiration)))
            .ToListAsync(cancellationToken);
    }

    private static string MaskUsername(string encryptedUsername, string? key, string salt)
    {
        if (string.IsNullOrEmpty(encryptedUsername))
        {
            return string.Empty;
        }
        var hint = encryptedUsername.Length > 4 ? encryptedUsername[^4..] : encryptedUsername;
        return $"***{hint}";
    }
}
