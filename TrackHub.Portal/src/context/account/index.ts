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


import { createContext, useContext, useMemo } from 'react';
import { accountCalendar, type AccountCalendar } from 'utils/accountCalendar';

export interface CurrentAccountValue {
  /** IANA zone from the account-context bootstrap read; null until it resolves (UTC calendar). */
  timeZoneId: string | null;
  isManager?: boolean;
}

export const CurrentAccountContext = createContext<CurrentAccountValue>({ timeZoneId: null });

/** The account's calendar: `const calendar = useAccountCalendar(); calendar.today()`. */
export function useAccountCalendar(): AccountCalendar {
  const { timeZoneId } = useContext(CurrentAccountContext);
  return useMemo(() => accountCalendar(timeZoneId), [timeZoneId]);
}
