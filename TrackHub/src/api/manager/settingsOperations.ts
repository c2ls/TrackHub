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
 * Account/user settings GraphQL documents (Manager backend). Codegen validates
 * these against schemas/manager.graphql and emits typed document nodes — values
 * always travel as variables, never string interpolation.
 */

import { graphql } from './generated';

export const AccountSettingsItemFragment = graphql(`
  fragment AccountSettingsItem on AccountSettingsVm {
    accountId
    maps
    mapsKey
    onlineInterval
    refreshMap
    refreshMapInterval
  }
`);

export const UserSettingsItemFragment = graphql(`
  fragment UserSettingsItem on UserSettingsVm {
    userId
    style
    language
    navbar
  }
`);

export const GetAccountSettingsByUserDocument = graphql(`
  query GetAccountSettingsByUser {
    accountSettingsByUser {
      ...AccountSettingsItem
    }
  }
`);

export const GetUserSettingsDocument = graphql(`
  query GetUserSettings {
    userSettings {
      ...UserSettingsItem
    }
  }
`);

export const UpdateAccountSettingsDocument = graphql(`
  mutation UpdateAccountSettings($id: UUID!, $accountSettings: AccountSettingsDtoInput!) {
    updateAccountSettings(id: $id, command: { accountSettings: $accountSettings })
  }
`);

export const UpdateUserSettingsDocument = graphql(`
  mutation UpdateUserSettings($id: UUID!, $userSettings: UserSettingsDtoInput!) {
    updateUserSettings(id: $id, command: { userSettings: $userSettings })
  }
`);
