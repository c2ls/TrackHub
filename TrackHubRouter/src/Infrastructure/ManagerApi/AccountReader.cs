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

using System.Text.Json;

namespace TrackHub.Router.Infrastructure.ManagerApi;

public class AccountReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IAccountReader
{
    // Fallback cadence (seconds) when the gps.integration feature omits a storing interval.
    private const int DefaultStoringIntervalSeconds = 360;

    // Single source of truth for the queries this reader sends; the
    // ServiceContracts tests validate these exact strings against the Manager schema.
    internal const string AccountsToSyncQuery = @"
                query($filter: FiltersInput!) {
                    accountSettingsMaster(
                        query: { filter: $filter }
                      ) {
                            accountId
                       }
                }";

    internal const string ValidateFeatureEnabledQuery = @"
                query($accountId: UUID!, $featureKey: String!) {
                    validateFeatureEnabled(query: { accountId: $accountId, featureKey: $featureKey })
                }";

    internal const string AllAccountFeaturesQuery = @"
                query {
                    allAccountFeaturesMaster {
                        accountId
                        featureKey
                        enabled
                        effectiveFrom
                        effectiveTo
                        configurationJson
                    }
                }";

    public async Task<IEnumerable<AccountSettingsVm>> GetAccountsToSyncAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = AccountsToSyncQuery,
            Variables = new
            {
                filter = new
                {
                    filters = Array.Empty<object>()
                }
            }
        };
        var accounts = await QueryAsync<IEnumerable<AccountSettingsVm>>(request, cancellationToken);

        // Two round trips regardless of account count: the account list plus every account's
        // features in one batched master read (previously one accountFeatures call per account).
        var featureRequest = new GraphQLRequest { Query = AllAccountFeaturesQuery };
        var allFeatures = await QueryAsync<IReadOnlyCollection<AccountFeatureMasterStateVm>>(featureRequest, cancellationToken);
        var featuresByAccount = allFeatures
            .GroupBy(f => f.AccountId)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<AccountFeatureStateVm>)g
                .Select(f => new AccountFeatureStateVm(f.FeatureKey, f.Enabled, f.EffectiveFrom, f.EffectiveTo, f.ConfigurationJson))
                .ToList());

        return accounts
            .Select(account => BuildAccountSettings(account, featuresByAccount.GetValueOrDefault(account.AccountId, [])))
            .ToList();
    }

    public async Task<bool> IsFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = ValidateFeatureEnabledQuery,
            Variables = new
            {
                accountId,
                featureKey
            }
        };

        return await QueryAsync<bool>(request, cancellationToken);
    }

    private static AccountSettingsVm BuildAccountSettings(AccountSettingsVm account, IReadOnlyCollection<AccountFeatureStateVm> features)
        => new(
            account.AccountId,
            ResolveStoringInterval(features),
            IsFeatureEnabled(features, FeatureKeys.Geofencing),
            IsFeatureEnabled(features, FeatureKeys.TripManagement),
            IsFeatureEnabled(features, FeatureKeys.GpsIntegration),
            IsFeatureEnabled(features, FeatureKeys.GpsPositionHistory));

    private static bool IsFeatureEnabled(IEnumerable<AccountFeatureStateVm> features, string featureKey)
    {
        var now = DateTimeOffset.UtcNow;
        return features.Any(feature =>
            feature.FeatureKey == featureKey
            && feature.Enabled
            && (!feature.EffectiveFrom.HasValue || feature.EffectiveFrom <= now)
            && (!feature.EffectiveTo.HasValue || feature.EffectiveTo >= now));
    }

    // Storing cadence is a storage/cost setting owned by the SuperAdministrator and stored in the
    // gps.integration feature configuration ("storingIntervalSeconds").
    private static int ResolveStoringInterval(IEnumerable<AccountFeatureStateVm> features)
    {
        var integration = features.FirstOrDefault(f => f.FeatureKey == FeatureKeys.GpsIntegration);
        if (!string.IsNullOrWhiteSpace(integration.ConfigurationJson))
        {
            try
            {
                var doc = JsonDocument.Parse(integration.ConfigurationJson!);
                if (doc.RootElement.TryGetProperty("storingIntervalSeconds", out var value)
                    && value.TryGetInt32(out var seconds)
                    && seconds > 0)
                {
                    return seconds;
                }
            }
            catch (JsonException)
            {
                // fall through to default
            }
        }
        return DefaultStoringIntervalSeconds;
    }

    private readonly record struct AccountFeatureStateVm(
        string FeatureKey,
        bool Enabled,
        DateTimeOffset? EffectiveFrom,
        DateTimeOffset? EffectiveTo,
        string? ConfigurationJson);

    private readonly record struct AccountFeatureMasterStateVm(
        Guid AccountId,
        string FeatureKey,
        bool Enabled,
        DateTimeOffset? EffectiveFrom,
        DateTimeOffset? EffectiveTo,
        string? ConfigurationJson);
}
