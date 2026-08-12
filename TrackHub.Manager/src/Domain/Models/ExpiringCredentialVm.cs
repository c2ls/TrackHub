// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

namespace TrackHub.Manager.Domain.Models;

public readonly record struct ExpiringCredentialVm(
    Guid CredentialId,
    Guid OperatorId,
    Guid AccountId,
    DateTimeOffset? TokenExpiration,
    DateTimeOffset? RefreshTokenExpiration,
    DateTimeOffset? EarliestExpirationAt);
