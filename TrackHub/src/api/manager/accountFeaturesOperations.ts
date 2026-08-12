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
 * Account features GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql and emits typed document nodes — values always
 * travel as variables, never string interpolation. The `*Master` operations are
 * the SuperAdministrator cross-account variants (AccountFeaturesMaster resource).
 */

import { graphql } from './generated';

export const AccountFeatureItemFragment = graphql(`
  fragment AccountFeatureItem on AccountFeatureVm {
    accountFeatureId
    accountId
    featureKey
    enabled
    tier
    source
    effectiveFrom
    effectiveTo
    configurationJson
    lastModified
  }
`);

export const GetAccountFeaturesDocument = graphql(`
  query GetAccountFeatures($accountId: UUID!) {
    accountFeatures(query: { accountId: $accountId }) {
      ...AccountFeatureItem
    }
  }
`);

export const SetAccountFeatureDocument = graphql(`
  mutation SetAccountFeature($feature: AccountFeatureDtoInput!) {
    setAccountFeature(command: { feature: $feature }) {
      ...AccountFeatureItem
    }
  }
`);

export const GetAccountFeaturesMasterDocument = graphql(`
  query GetAccountFeaturesMaster($accountId: UUID!) {
    accountFeaturesMaster(query: { accountId: $accountId }) {
      ...AccountFeatureItem
    }
  }
`);

export const SetAccountFeatureMasterDocument = graphql(`
  mutation SetAccountFeatureMaster($feature: AccountFeatureDtoInput!) {
    setAccountFeatureMaster(command: { feature: $feature }) {
      ...AccountFeatureItem
    }
  }
`);
