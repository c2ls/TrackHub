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

import { graphql } from './generated';

export const AccountHolidayItemFragment = graphql(`
  fragment AccountHolidayItem on AccountHolidayVm {
    accountHolidayId
    accountId
    date
    name
  }
`);

export const GetAccountHolidaysDocument = graphql(`
  query GetAccountHolidays($accountId: UUID!, $year: Int!) {
    accountHolidays(query: { accountId: $accountId, year: $year }) {
      ...AccountHolidayItem
    }
  }
`);

export const SetAccountHolidaysDocument = graphql(`
  mutation SetAccountHolidays($accountId: UUID!, $year: Int!, $dates: [AccountHolidayDtoInput!]!) {
    setAccountHolidays(command: { accountId: $accountId, year: $year, dates: $dates }) {
      ...AccountHolidayItem
    }
  }
`);
