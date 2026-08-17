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

import axios, { AxiosError } from 'axios';
import { print } from 'graphql';
import type { TypedDocumentNode } from '@graphql-typed-document-node/core';
import { GRAPHQL_ENDPOINTS, type GraphQLBackend } from './endpoints';
import { ApiError, type GraphQLErrorEntry } from './errors';
import { tokenStore } from './tokenStore';

export const REQUEST_TIMEOUT_MS = 30000;

interface GraphQLResponsePayload {
  data?: Record<string, unknown> | null;
  errors?: GraphQLErrorEntry[];
}

/**
 * A GraphQL response is a partial success when it carries errors alongside
 * usable data (e.g. the router falls back to cached positions after a
 * provider read fails). Only responses without any usable data are failures.
 */
function hasUsableData(payload: GraphQLResponsePayload): boolean {
  return (
    payload.data != null &&
    Object.values(payload.data).some((value) => value !== null && value !== undefined)
  );
}

/**
 * Executes a GraphQL operation against one of the TrackHub backends.
 * Values ALWAYS travel as GraphQL variables — never interpolated into the
 * document. Throws ApiError on failure; returns the data payload on success.
 */
export async function executeGraphQL<TData, TVariables>(
  backend: GraphQLBackend,
  document: TypedDocumentNode<TData, TVariables> | string,
  ...[variables]: TVariables extends Record<string, never> ? [] : [TVariables]
): Promise<TData> {
  const query = typeof document === 'string' ? document : print(document);
  const token = await tokenStore.acquireValidAccessToken();

  let payload: GraphQLResponsePayload;
  try {
    const response = await axios.post<GraphQLResponsePayload>(
      GRAPHQL_ENDPOINTS[backend],
      { query, variables },
      {
        headers: { Authorization: `Bearer ${token}` },
        timeout: REQUEST_TIMEOUT_MS,
      }
    );
    payload = response.data;
  } catch (error) {
    // HotChocolate returns GraphQL error payloads with non-2xx statuses too.
    const axiosError = error as AxiosError<GraphQLResponsePayload>;
    const errors = axiosError.response?.data?.errors;
    if (errors && errors.length > 0) {
      throw ApiError.fromGraphQLErrors(errors);
    }
    throw new ApiError(`Request to ${backend} failed: ${axiosError.message}`, {
      status: axiosError.response?.status,
      cause: error,
    });
  }

  if (payload.errors && payload.errors.length > 0 && !hasUsableData(payload)) {
    throw ApiError.fromGraphQLErrors(payload.errors);
  }
  return payload.data as TData;
}
