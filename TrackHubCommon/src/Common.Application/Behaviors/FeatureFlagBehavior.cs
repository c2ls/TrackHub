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

using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that enforces <see cref="RequireFeatureAttribute"/> declarations on
/// commands and queries. The account whose flag is evaluated is, in order:
///   1. The <c>AccountId</c> property on the request (if non-empty Guid).
///   2. The current principal's <see cref="ICurrentPrincipal.AccountId"/>.
/// If neither is available and the principal is a service client without a scoped
/// account, the check passes when <see cref="RequireFeatureAttribute.AllowGlobalServiceClient"/>
/// is true (the default) and fails closed otherwise.
/// </summary>
public sealed class FeatureFlagBehavior<TRequest, TResponse>(
    IFeatureFlagService featureFlagService,
    ICurrentPrincipal principal,
    ILogger<FeatureFlagBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var attributes = typeof(TRequest).GetCustomAttributes<RequireFeatureAttribute>(inherit: true).ToArray();
        if (attributes.Length == 0)
        {
            return await next();
        }

        var accountId = ResolveAccountId(request);
        foreach (var attribute in attributes)
        {
            if (accountId is null)
            {
                if (principal.PrincipalType == PrincipalType.ServiceClient
                    && !principal.AccountId.HasValue
                    && attribute.AllowGlobalServiceClient)
                {
                    continue;
                }

                logger.LogWarning(
                    "Feature flag '{FeatureKey}' check failed: no account could be resolved for {RequestType}.",
                    attribute.FeatureKey,
                    typeof(TRequest).FullName);
                throw new FeatureDisabledException(attribute.FeatureKey, null);
            }

            var enabled = await featureFlagService.IsEnabledAsync(accountId.Value, attribute.FeatureKey, cancellationToken);
            if (!enabled)
            {
                throw new FeatureDisabledException(attribute.FeatureKey, accountId.Value);
            }
        }

        return await next();
    }

    private Guid? ResolveAccountId(TRequest request)
        => RequestAccountResolver.GetRequestAccountId(request) ?? principal.AccountId;
}
