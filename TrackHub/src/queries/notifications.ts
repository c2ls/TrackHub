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

/**
 * In-app notification hooks for the navbar bell. The feed polls every minute;
 * marking a notification read invalidates the feed so the unread badge updates.
 * Failures surface through the query client's global error toast — the bell
 * renders an empty feed on error instead of breaking.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/notificationDeliveries';

const FEED_SIZE = 10;

export const notificationKeys = {
  all: ['notifications'] as const,
  my: (unreadOnly: boolean) => [...notificationKeys.all, 'my', unreadOnly] as const,
};

export function useMyNotifications(
  unreadOnly = false,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: notificationKeys.my(unreadOnly),
    queryFn: () => api.getMyNotifications(unreadOnly, 0, FEED_SIZE),
    enabled: options.enabled ?? true,
    refetchInterval: 60000,
  });
}

export function useMarkNotificationRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (notificationDeliveryId: string) =>
      api.markNotificationRead(notificationDeliveryId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationKeys.all }),
  });
}
