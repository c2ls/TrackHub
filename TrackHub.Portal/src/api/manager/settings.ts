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
 * Account/user settings API (Manager backend): plain typed async functions.
 * Failures THROW ApiError — fallbacks and toasts belong to the caller layer.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  AccountSettingsItemFragment as AccountSettingsItemType,
  UserSettingsItemFragment as UserSettingsItemType,
  AccountSettingsDtoInput,
  UserSettingsDtoInput,
} from './generated/graphql';
import {
  GetAccountSettingsByUserDocument,
  GetUserSettingsDocument,
  UpdateAccountSettingsDocument,
  UpdateUserSettingsDocument,
} from './settingsOperations';

export type AccountSettings = AccountSettingsItemType;
export type UserSettings = UserSettingsItemType;
export type { AccountSettingsDtoInput, UserSettingsDtoInput };

export async function getAccountSettings(): Promise<AccountSettings> {
  const data = await executeGraphQL('manager', GetAccountSettingsByUserDocument);
  return data.accountSettingsByUser;
}

export async function getUserSettings(): Promise<UserSettings> {
  const data = await executeGraphQL('manager', GetUserSettingsDocument);
  return data.userSettings;
}

export async function updateAccountSettings(
  accountId: string,
  accountSettings: Omit<AccountSettingsDtoInput, 'accountId'>
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateAccountSettingsDocument, {
    id: accountId,
    accountSettings: {
      accountId,
      maps: accountSettings.maps,
      mapsKey: accountSettings.mapsKey,
      onlineInterval: accountSettings.onlineInterval,
      refreshMap: accountSettings.refreshMap,
      refreshMapInterval: accountSettings.refreshMapInterval,
    },
  });
  return data.updateAccountSettings;
}

export async function updateUserSettings(
  userId: string,
  userSettings: Omit<UserSettingsDtoInput, 'userId'>
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateUserSettingsDocument, {
    id: userId,
    userSettings: {
      userId,
      style: userSettings.style,
      language: userSettings.language,
      navbar: userSettings.navbar,
    },
  });
  return data.updateUserSettings;
}
