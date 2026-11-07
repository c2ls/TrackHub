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

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/accountHolidays';
import type { AccountHolidayDtoInput } from 'api/manager/accountHolidays';

export const accountHolidayKeys = {
  all: ['accountHolidays'] as const,
  year: (accountId: string, year: number) => [...accountHolidayKeys.all, accountId, year] as const,
};

export function useAccountHolidays(accountId: string | undefined, year: number, options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: accountHolidayKeys.year(accountId ?? '', year),
    queryFn: ({ signal }) => api.getAccountHolidays(accountId as string, year, { signal }),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useSetAccountHolidays() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ accountId, year, dates }: { accountId: string; year: number; dates: AccountHolidayDtoInput[] }) =>
      api.setAccountHolidays(accountId, year, dates),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: accountHolidayKeys.all }),
  });
}
