/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

import { executeGraphQL } from 'api/core/graphqlClient';
import type { RequestOptions } from 'api/core/errors';
import type { AccountHolidayItemFragment as AccountHolidayItemType, AccountHolidayDtoInput } from './generated/graphql';
import { GetAccountHolidaysDocument, SetAccountHolidaysDocument } from './accountHolidaysOperations';

export type AccountHoliday = AccountHolidayItemType;
export type { AccountHolidayDtoInput };

export async function getAccountHolidays(accountId: string, year: number, options?: RequestOptions): Promise<AccountHoliday[]> {
  const data = await executeGraphQL('manager', GetAccountHolidaysDocument, { accountId, year }, options);
  return data.accountHolidays;
}

export async function setAccountHolidays(accountId: string, year: number, dates: AccountHolidayDtoInput[]): Promise<AccountHoliday[]> {
  const data = await executeGraphQL('manager', SetAccountHolidaysDocument, { accountId, year, dates });
  return data.setAccountHolidays;
}
