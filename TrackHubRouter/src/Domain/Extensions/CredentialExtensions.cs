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

namespace TrackHub.Router.Domain.Extensions;
public static class CredentialExtensions
{
    public static CredentialTokenDto Decrypt(this CredentialTokenVm credential, string key)
    {
        var salt = Convert.FromBase64String(credential.Salt);
        return new CredentialTokenDto
        (
            credential.CredentialId,
            credential.Uri,
            credential.Username.DecryptData(key, salt),
            credential.Password.DecryptData(key, salt),
            credential.Key?.DecryptData(key, salt),
            credential.Key2?.DecryptData(key, salt),
            credential.Token?.DecryptData(key, salt),
            credential.TokenExpiration,
            credential.RefreshToken?.DecryptData(key, salt),
            credential.RefreshTokenExpiration
        );
    }
}
