/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License").
*  You may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

/**
 * Account features API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer. The `*Master`
 * functions are the SuperAdministrator cross-account variants.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  AccountFeatureItemFragment as AccountFeatureItemType,
  AccountFeatureDtoInput,
} from './generated/graphql';
import {
  GetAccountFeaturesDocument,
  SetAccountFeatureDocument,
  GetAccountFeaturesMasterDocument,
  SetAccountFeatureMasterDocument,
} from './accountFeaturesOperations';

export type AccountFeature = AccountFeatureItemType;
export type { AccountFeatureDtoInput };

function toFeatureInput(feature: AccountFeatureDtoInput): AccountFeatureDtoInput {
  return {
    accountId: feature.accountId,
    featureKey: feature.featureKey,
    enabled: feature.enabled,
    tier: feature.tier,
    source: feature.source,
    effectiveFrom: feature.effectiveFrom,
    effectiveTo: feature.effectiveTo,
    configurationJson: feature.configurationJson,
  };
}

export async function getAccountFeatures(accountId: string): Promise<AccountFeature[]> {
  const data = await executeGraphQL('manager', GetAccountFeaturesDocument, { accountId });
  return data.accountFeatures;
}

export async function setAccountFeature(feature: AccountFeatureDtoInput): Promise<AccountFeature> {
  const data = await executeGraphQL('manager', SetAccountFeatureDocument, {
    feature: toFeatureInput(feature),
  });
  return data.setAccountFeature;
}

export async function getAccountFeaturesMaster(accountId: string): Promise<AccountFeature[]> {
  const data = await executeGraphQL('manager', GetAccountFeaturesMasterDocument, { accountId });
  return data.accountFeaturesMaster;
}

export async function setAccountFeatureMaster(
  feature: AccountFeatureDtoInput
): Promise<AccountFeature> {
  const data = await executeGraphQL('manager', SetAccountFeatureMasterDocument, {
    feature: toFeatureInput(feature),
  });
  return data.setAccountFeatureMaster;
}
