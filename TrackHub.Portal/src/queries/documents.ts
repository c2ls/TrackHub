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
 * Document query hooks (spec 04 documents, Manager backend). Components consume
 * these rather than the api layer directly.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/documents';
import type { DocumentUploadFields } from 'api/manager/documents';

export const documentKeys = {
  all: ['documents'] as const,
  forOwner: (accountId: string, ownerEntityType: string, ownerEntityId: string) =>
    [...documentKeys.all, 'forOwner', accountId, ownerEntityType, ownerEntityId] as const,
};

/**
 * Documents owned by one entity (spec 04 owner scoping — `Driver` is the owner
 * type spec 09 §11 uses for qualification evidence).
 */
export function useDocumentsForOwner(
  accountId: string | undefined,
  ownerEntityType: string,
  ownerEntityId: string | null | undefined,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: documentKeys.forOwner(accountId ?? '', ownerEntityType, ownerEntityId ?? ''),
    queryFn: () =>
      api.getDocumentsForOwner(accountId as string, ownerEntityType, ownerEntityId as string),
    enabled: (options.enabled ?? true) && !!accountId && !!ownerEntityId,
  });
}

/**
 * Uploads a file through the spec 04 multipart REST surface. There is no second
 * upload endpoint anywhere in the platform — POD attachments reuse this one and
 * are linked to the trip by document id (spec 11 §9, §11).
 */
export function useUploadDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ file, fields }: { file: File; fields?: DocumentUploadFields }) =>
      api.uploadDocument(file, fields ?? {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: documentKeys.all }),
  });
}

/**
 * Re-reads one document's metadata. Uploads land as `Pending` until the scanner
 * finishes, and only a `Clean` document may be attached to a POD — this is how a
 * screen refreshes that verdict without polling.
 */
export function useRefreshDocument() {
  return useMutation({
    mutationFn: (documentId: string) => api.getDocument(documentId),
  });
}

/** Invalidator for surfaces that mutate documents outside the query layer (DocumentPanel). */
export function useInvalidateDocuments() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: documentKeys.all });
}
