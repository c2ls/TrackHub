/* eslint-disable */
/** Internal type. DO NOT USE DIRECTLY. */
type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
/** Internal type. DO NOT USE DIRECTLY. */
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type OperatorHealthCheckType =
  | 'DEVICE_SYNC'
  | 'PING'
  | 'POSITION_SYNC'
  | 'TOKEN_REFRESH';

export type OperatorHealthStatus =
  | 'DEGRADED'
  | 'DISABLED'
  | 'HEALTHY'
  | 'OFFLINE'
  | 'UNKNOWN';

export type OperatorSyncResult =
  | 'FAILED'
  | 'PARTIALLY_SUCCEEDED'
  | 'PENDING'
  | 'SUCCEEDED'
  | 'THROTTLED';

export type SyncTriggerType =
  | 'AUTOMATIC'
  | 'MANUAL';

export type GetOperatorSyncRunsQueryVariables = Exact<{
  accountId?: string | null | undefined;
  operatorId?: string | null | undefined;
  take: number;
}>;


export type GetOperatorSyncRunsQuery = { operatorSyncRuns: Array<{ operatorSyncRunId: string, accountId: string, operatorId: string, triggerType: SyncTriggerType, result: OperatorSyncResult, startedAt: string, completedAt: string | null, devicesSeen: number, devicesAdded: number, devicesUpdated: number, devicesRemoved: number, devicesIgnored: number, positionsRead: number, positionsAccepted: number, positionsRejected: number, errorCode: string | null, errorMessage: string | null, correlationId: string | null }> };

export type GetOperatorHealthQueryVariables = Exact<{
  operatorId: string;
}>;


export type GetOperatorHealthQuery = { operatorHealth: { operatorId: string, healthStatus: OperatorHealthStatus, lastSuccessfulSyncAt: string | null, lastFailedSyncAt: string | null, lastDeviceSyncAt: string | null, lastPositionSyncAt: string | null, lastFailureCode: string | null, lastFailureMessage: string | null, lastLatencyMs: number | null } };

export type GetOperatorHealthHistoryQueryVariables = Exact<{
  operatorId: string;
  take: number;
}>;


export type GetOperatorHealthHistoryQuery = { operatorHealthHistory: Array<{ operatorHealthCheckId: string, operatorId: string, checkType: OperatorHealthCheckType, status: OperatorHealthStatus, latencyMs: number | null, startedAt: string, completedAt: string | null, errorCode: string | null, errorMessage: string | null, retryCount: number, correlationId: string | null }> };

export type GetPlatformSyncActivityQueryVariables = Exact<{
  lookbackMinutes: number;
}>;


export type GetPlatformSyncActivityQuery = { platformSyncActivity: { lastSyncRunAt: string | null, lastHealthCheckAt: string | null, syncRunsLastHour: number, healthChecksLastHour: number, hasEnabledGpsIntegration: boolean } };


export const GetOperatorSyncRunsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetOperatorSyncRuns"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorSyncRuns"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"operatorId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorSyncRunId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"triggerType"}},{"kind":"Field","name":{"kind":"Name","value":"result"}},{"kind":"Field","name":{"kind":"Name","value":"startedAt"}},{"kind":"Field","name":{"kind":"Name","value":"completedAt"}},{"kind":"Field","name":{"kind":"Name","value":"devicesSeen"}},{"kind":"Field","name":{"kind":"Name","value":"devicesAdded"}},{"kind":"Field","name":{"kind":"Name","value":"devicesUpdated"}},{"kind":"Field","name":{"kind":"Name","value":"devicesRemoved"}},{"kind":"Field","name":{"kind":"Name","value":"devicesIgnored"}},{"kind":"Field","name":{"kind":"Name","value":"positionsRead"}},{"kind":"Field","name":{"kind":"Name","value":"positionsAccepted"}},{"kind":"Field","name":{"kind":"Name","value":"positionsRejected"}},{"kind":"Field","name":{"kind":"Name","value":"errorCode"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}},{"kind":"Field","name":{"kind":"Name","value":"correlationId"}}]}}]}}]} as unknown as DocumentNode<GetOperatorSyncRunsQuery, GetOperatorSyncRunsQueryVariables>;
export const GetOperatorHealthDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetOperatorHealth"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorHealth"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"operatorId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"healthStatus"}},{"kind":"Field","name":{"kind":"Name","value":"lastSuccessfulSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailedSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastDeviceSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastPositionSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailureCode"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailureMessage"}},{"kind":"Field","name":{"kind":"Name","value":"lastLatencyMs"}}]}}]}}]} as unknown as DocumentNode<GetOperatorHealthQuery, GetOperatorHealthQueryVariables>;
export const GetOperatorHealthHistoryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetOperatorHealthHistory"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorHealthHistory"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"operatorId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorHealthCheckId"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"checkType"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"latencyMs"}},{"kind":"Field","name":{"kind":"Name","value":"startedAt"}},{"kind":"Field","name":{"kind":"Name","value":"completedAt"}},{"kind":"Field","name":{"kind":"Name","value":"errorCode"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}},{"kind":"Field","name":{"kind":"Name","value":"retryCount"}},{"kind":"Field","name":{"kind":"Name","value":"correlationId"}}]}}]}}]} as unknown as DocumentNode<GetOperatorHealthHistoryQuery, GetOperatorHealthHistoryQueryVariables>;
export const GetPlatformSyncActivityDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetPlatformSyncActivity"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"lookbackMinutes"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"platformSyncActivity"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"lookbackMinutes"},"value":{"kind":"Variable","name":{"kind":"Name","value":"lookbackMinutes"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"lastSyncRunAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastHealthCheckAt"}},{"kind":"Field","name":{"kind":"Name","value":"syncRunsLastHour"}},{"kind":"Field","name":{"kind":"Name","value":"healthChecksLastHour"}},{"kind":"Field","name":{"kind":"Name","value":"hasEnabledGpsIntegration"}}]}}]}}]} as unknown as DocumentNode<GetPlatformSyncActivityQuery, GetPlatformSyncActivityQueryVariables>;