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

import { useQueries } from '@tanstack/react-query';
import { getTransporter } from 'api/manager/transporters';
import { getDocument } from 'api/manager/documents';
import { getTripDetail } from 'api/tripManagement/trips';
import { transporterKeys } from 'queries/transporters';
import { tripKeys } from 'queries/trips';
import { documentKeys } from 'queries/documents';
import { useStaffNames } from 'queries/staff';
import { useUserLookup } from 'queries/users';

export interface EntityRef {
  type: string;
  id: string;
}

// A row can point at a record that no longer exists; its id stays on screen instead of a toast.
const lookupOptions = { retry: false, staleTime: 5 * 60_000, meta: { silent: true } } as const;

const idsOf = (refs: readonly EntityRef[], type: string) =>
  [...new Set(refs.filter((ref) => ref.type === type && ref.id).map((ref) => ref.id))];

/** Display names for the referenced records, keyed `${type}:${id}`; unresolved refs are absent. */
export function useEntityNames(refs: readonly EntityRef[]): Map<string, string> {
  const transporterIds = idsOf(refs, 'Transporter');
  const tripIds = idsOf(refs, 'Trip');
  const documentIds = idsOf(refs, 'Document');
  const staffIds = idsOf(refs, 'Staff');
  const hasUsers = idsOf(refs, 'User').length > 0;

  const transporters = useQueries({
    queries: transporterIds.map((id) => ({
      queryKey: transporterKeys.detail(id),
      queryFn: ({ signal }: { signal: AbortSignal }) => getTransporter(id, { signal }),
      ...lookupOptions,
    })),
  });
  const trips = useQueries({
    queries: tripIds.map((id) => ({
      queryKey: tripKeys.detail(id),
      queryFn: ({ signal }: { signal: AbortSignal }) => getTripDetail(id, { signal }),
      ...lookupOptions,
    })),
  });
  const documents = useQueries({
    queries: documentIds.map((id) => ({
      queryKey: documentKeys.detail(id),
      queryFn: () => getDocument(id),
      ...lookupOptions,
    })),
  });
  const staff = useStaffNames(staffIds);
  const users = useUserLookup({ enabled: hasUsers });

  const names = new Map<string, string>();
  transporters.forEach((result) => {
    if (result.data) names.set(`Transporter:${result.data.transporterId}`, result.data.name);
  });
  trips.forEach((result) => {
    if (result.data) names.set(`Trip:${result.data.trip.tripId}`, result.data.trip.code);
  });
  documents.forEach((result) => {
    if (result.data) names.set(`Document:${result.data.documentId}`, result.data.title || result.data.fileName);
  });
  staff.data?.forEach((member) => names.set(`Staff:${member.staffId}`, member.name));
  if (hasUsers) users.data?.forEach((user) => names.set(`User:${user.userId}`, user.username));
  return names;
}
