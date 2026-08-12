/* eslint-disable */
import * as types from './graphql';
import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';

/**
 * Map of all GraphQL operations in the project.
 *
 * This map has several performance disadvantages:
 * 1. It is not tree-shakeable, so it will include all operations in the project.
 * 2. It is not minifiable, so the string of a GraphQL query will be multiple times inside the bundle.
 * 3. It does not support dead code elimination, so it will add unused operations.
 *
 * Therefore it is highly recommended to use the babel or swc plugin for production.
 * Learn more about it here: https://the-guild.dev/graphql/codegen/plugins/presets/preset-client#reducing-bundle-size
 */
type Documents = {
    "\n  query GetOperatorSyncRuns($accountId: UUID, $operatorId: UUID, $take: Int!) {\n    operatorSyncRuns(query: { accountId: $accountId, operatorId: $operatorId, take: $take }) {\n      operatorSyncRunId\n      accountId\n      operatorId\n      triggerType\n      result\n      startedAt\n      completedAt\n      devicesSeen\n      devicesAdded\n      devicesUpdated\n      devicesRemoved\n      devicesIgnored\n      positionsRead\n      positionsAccepted\n      positionsRejected\n      errorCode\n      errorMessage\n      correlationId\n    }\n  }\n": typeof types.GetOperatorSyncRunsDocument,
    "\n  query GetOperatorHealth($operatorId: UUID!) {\n    operatorHealth(query: { operatorId: $operatorId }) {\n      operatorId\n      healthStatus\n      lastSuccessfulSyncAt\n      lastFailedSyncAt\n      lastDeviceSyncAt\n      lastPositionSyncAt\n      lastFailureCode\n      lastFailureMessage\n      lastLatencyMs\n    }\n  }\n": typeof types.GetOperatorHealthDocument,
    "\n  query GetOperatorHealthHistory($operatorId: UUID!, $take: Int!) {\n    operatorHealthHistory(query: { operatorId: $operatorId, take: $take }) {\n      operatorHealthCheckId\n      operatorId\n      checkType\n      status\n      latencyMs\n      startedAt\n      completedAt\n      errorCode\n      errorMessage\n      retryCount\n      correlationId\n    }\n  }\n": typeof types.GetOperatorHealthHistoryDocument,
};
const documents: Documents = {
    "\n  query GetOperatorSyncRuns($accountId: UUID, $operatorId: UUID, $take: Int!) {\n    operatorSyncRuns(query: { accountId: $accountId, operatorId: $operatorId, take: $take }) {\n      operatorSyncRunId\n      accountId\n      operatorId\n      triggerType\n      result\n      startedAt\n      completedAt\n      devicesSeen\n      devicesAdded\n      devicesUpdated\n      devicesRemoved\n      devicesIgnored\n      positionsRead\n      positionsAccepted\n      positionsRejected\n      errorCode\n      errorMessage\n      correlationId\n    }\n  }\n": types.GetOperatorSyncRunsDocument,
    "\n  query GetOperatorHealth($operatorId: UUID!) {\n    operatorHealth(query: { operatorId: $operatorId }) {\n      operatorId\n      healthStatus\n      lastSuccessfulSyncAt\n      lastFailedSyncAt\n      lastDeviceSyncAt\n      lastPositionSyncAt\n      lastFailureCode\n      lastFailureMessage\n      lastLatencyMs\n    }\n  }\n": types.GetOperatorHealthDocument,
    "\n  query GetOperatorHealthHistory($operatorId: UUID!, $take: Int!) {\n    operatorHealthHistory(query: { operatorId: $operatorId, take: $take }) {\n      operatorHealthCheckId\n      operatorId\n      checkType\n      status\n      latencyMs\n      startedAt\n      completedAt\n      errorCode\n      errorMessage\n      retryCount\n      correlationId\n    }\n  }\n": types.GetOperatorHealthHistoryDocument,
};

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 *
 *
 * @example
 * ```ts
 * const query = graphql(`query GetUser($id: ID!) { user(id: $id) { name } }`);
 * ```
 *
 * The query argument is unknown!
 * Please regenerate the types.
 */
export function graphql(source: string): unknown;

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetOperatorSyncRuns($accountId: UUID, $operatorId: UUID, $take: Int!) {\n    operatorSyncRuns(query: { accountId: $accountId, operatorId: $operatorId, take: $take }) {\n      operatorSyncRunId\n      accountId\n      operatorId\n      triggerType\n      result\n      startedAt\n      completedAt\n      devicesSeen\n      devicesAdded\n      devicesUpdated\n      devicesRemoved\n      devicesIgnored\n      positionsRead\n      positionsAccepted\n      positionsRejected\n      errorCode\n      errorMessage\n      correlationId\n    }\n  }\n"): (typeof documents)["\n  query GetOperatorSyncRuns($accountId: UUID, $operatorId: UUID, $take: Int!) {\n    operatorSyncRuns(query: { accountId: $accountId, operatorId: $operatorId, take: $take }) {\n      operatorSyncRunId\n      accountId\n      operatorId\n      triggerType\n      result\n      startedAt\n      completedAt\n      devicesSeen\n      devicesAdded\n      devicesUpdated\n      devicesRemoved\n      devicesIgnored\n      positionsRead\n      positionsAccepted\n      positionsRejected\n      errorCode\n      errorMessage\n      correlationId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetOperatorHealth($operatorId: UUID!) {\n    operatorHealth(query: { operatorId: $operatorId }) {\n      operatorId\n      healthStatus\n      lastSuccessfulSyncAt\n      lastFailedSyncAt\n      lastDeviceSyncAt\n      lastPositionSyncAt\n      lastFailureCode\n      lastFailureMessage\n      lastLatencyMs\n    }\n  }\n"): (typeof documents)["\n  query GetOperatorHealth($operatorId: UUID!) {\n    operatorHealth(query: { operatorId: $operatorId }) {\n      operatorId\n      healthStatus\n      lastSuccessfulSyncAt\n      lastFailedSyncAt\n      lastDeviceSyncAt\n      lastPositionSyncAt\n      lastFailureCode\n      lastFailureMessage\n      lastLatencyMs\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetOperatorHealthHistory($operatorId: UUID!, $take: Int!) {\n    operatorHealthHistory(query: { operatorId: $operatorId, take: $take }) {\n      operatorHealthCheckId\n      operatorId\n      checkType\n      status\n      latencyMs\n      startedAt\n      completedAt\n      errorCode\n      errorMessage\n      retryCount\n      correlationId\n    }\n  }\n"): (typeof documents)["\n  query GetOperatorHealthHistory($operatorId: UUID!, $take: Int!) {\n    operatorHealthHistory(query: { operatorId: $operatorId, take: $take }) {\n      operatorHealthCheckId\n      operatorId\n      checkType\n      status\n      latencyMs\n      startedAt\n      completedAt\n      errorCode\n      errorMessage\n      retryCount\n      correlationId\n    }\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;