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
    "\n  fragment GeofenceFields on GeofenceVm {\n    geofenceId\n    accountId\n    name\n    description\n    type\n    color\n    active\n    circleCenter {\n      latitude\n      longitude\n    }\n    circleRadiusMeters\n    alertOnEntry\n    alertOnExit\n    dwellThresholdMinutes\n    geom {\n      srid\n      coordinates {\n        latitude\n        longitude\n      }\n    }\n  }\n": typeof types.GeofenceFieldsFragmentDoc,
    "\n  query GetGeofence($id: UUID!) {\n    geofence(query: { id: $id }) {\n      ...GeofenceFields\n    }\n  }\n": typeof types.GetGeofenceDocument,
    "\n  query GetGeofencesByAccount(\n    $enableCaching: Boolean!\n    $skip: Int\n    $take: Int\n    $type: Short\n    $active: Boolean\n    $search: String\n  ) {\n    geofencesByAccount(\n      query: {\n        enableCaching: $enableCaching\n        skip: $skip\n        take: $take\n        type: $type\n        active: $active\n        search: $search\n      }\n    ) {\n      items {\n        ...GeofenceFields\n      }\n      totalCount\n    }\n  }\n": typeof types.GetGeofencesByAccountDocument,
    "\n  query GetTransportersInGeofence($geofenceId: UUID, $type: Short) {\n    transportersInGeofence(geofenceId: $geofenceId, type: $type) {\n      transporterId\n      transporterName\n      geofenceId\n      geofenceName\n    }\n  }\n": typeof types.GetTransportersInGeofenceDocument,
    "\n  mutation CreateGeofence($geofence: GeofenceDtoInput!) {\n    createGeofence(command: { geofence: $geofence }) {\n      ...GeofenceFields\n    }\n  }\n": typeof types.CreateGeofenceDocument,
    "\n  mutation UpdateGeofence($id: UUID!, $geofence: GeofenceDtoInput!) {\n    updateGeofence(id: $id, command: { geofence: $geofence })\n  }\n": typeof types.UpdateGeofenceDocument,
    "\n  mutation DeleteGeofence($id: UUID!) {\n    deleteGeofence(id: $id)\n  }\n": typeof types.DeleteGeofenceDocument,
};
const documents: Documents = {
    "\n  fragment GeofenceFields on GeofenceVm {\n    geofenceId\n    accountId\n    name\n    description\n    type\n    color\n    active\n    circleCenter {\n      latitude\n      longitude\n    }\n    circleRadiusMeters\n    alertOnEntry\n    alertOnExit\n    dwellThresholdMinutes\n    geom {\n      srid\n      coordinates {\n        latitude\n        longitude\n      }\n    }\n  }\n": types.GeofenceFieldsFragmentDoc,
    "\n  query GetGeofence($id: UUID!) {\n    geofence(query: { id: $id }) {\n      ...GeofenceFields\n    }\n  }\n": types.GetGeofenceDocument,
    "\n  query GetGeofencesByAccount(\n    $enableCaching: Boolean!\n    $skip: Int\n    $take: Int\n    $type: Short\n    $active: Boolean\n    $search: String\n  ) {\n    geofencesByAccount(\n      query: {\n        enableCaching: $enableCaching\n        skip: $skip\n        take: $take\n        type: $type\n        active: $active\n        search: $search\n      }\n    ) {\n      items {\n        ...GeofenceFields\n      }\n      totalCount\n    }\n  }\n": types.GetGeofencesByAccountDocument,
    "\n  query GetTransportersInGeofence($geofenceId: UUID, $type: Short) {\n    transportersInGeofence(geofenceId: $geofenceId, type: $type) {\n      transporterId\n      transporterName\n      geofenceId\n      geofenceName\n    }\n  }\n": types.GetTransportersInGeofenceDocument,
    "\n  mutation CreateGeofence($geofence: GeofenceDtoInput!) {\n    createGeofence(command: { geofence: $geofence }) {\n      ...GeofenceFields\n    }\n  }\n": types.CreateGeofenceDocument,
    "\n  mutation UpdateGeofence($id: UUID!, $geofence: GeofenceDtoInput!) {\n    updateGeofence(id: $id, command: { geofence: $geofence })\n  }\n": types.UpdateGeofenceDocument,
    "\n  mutation DeleteGeofence($id: UUID!) {\n    deleteGeofence(id: $id)\n  }\n": types.DeleteGeofenceDocument,
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
export function graphql(source: "\n  fragment GeofenceFields on GeofenceVm {\n    geofenceId\n    accountId\n    name\n    description\n    type\n    color\n    active\n    circleCenter {\n      latitude\n      longitude\n    }\n    circleRadiusMeters\n    alertOnEntry\n    alertOnExit\n    dwellThresholdMinutes\n    geom {\n      srid\n      coordinates {\n        latitude\n        longitude\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment GeofenceFields on GeofenceVm {\n    geofenceId\n    accountId\n    name\n    description\n    type\n    color\n    active\n    circleCenter {\n      latitude\n      longitude\n    }\n    circleRadiusMeters\n    alertOnEntry\n    alertOnExit\n    dwellThresholdMinutes\n    geom {\n      srid\n      coordinates {\n        latitude\n        longitude\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetGeofence($id: UUID!) {\n    geofence(query: { id: $id }) {\n      ...GeofenceFields\n    }\n  }\n"): (typeof documents)["\n  query GetGeofence($id: UUID!) {\n    geofence(query: { id: $id }) {\n      ...GeofenceFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetGeofencesByAccount(\n    $enableCaching: Boolean!\n    $skip: Int\n    $take: Int\n    $type: Short\n    $active: Boolean\n    $search: String\n  ) {\n    geofencesByAccount(\n      query: {\n        enableCaching: $enableCaching\n        skip: $skip\n        take: $take\n        type: $type\n        active: $active\n        search: $search\n      }\n    ) {\n      items {\n        ...GeofenceFields\n      }\n      totalCount\n    }\n  }\n"): (typeof documents)["\n  query GetGeofencesByAccount(\n    $enableCaching: Boolean!\n    $skip: Int\n    $take: Int\n    $type: Short\n    $active: Boolean\n    $search: String\n  ) {\n    geofencesByAccount(\n      query: {\n        enableCaching: $enableCaching\n        skip: $skip\n        take: $take\n        type: $type\n        active: $active\n        search: $search\n      }\n    ) {\n      items {\n        ...GeofenceFields\n      }\n      totalCount\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTransportersInGeofence($geofenceId: UUID, $type: Short) {\n    transportersInGeofence(geofenceId: $geofenceId, type: $type) {\n      transporterId\n      transporterName\n      geofenceId\n      geofenceName\n    }\n  }\n"): (typeof documents)["\n  query GetTransportersInGeofence($geofenceId: UUID, $type: Short) {\n    transportersInGeofence(geofenceId: $geofenceId, type: $type) {\n      transporterId\n      transporterName\n      geofenceId\n      geofenceName\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateGeofence($geofence: GeofenceDtoInput!) {\n    createGeofence(command: { geofence: $geofence }) {\n      ...GeofenceFields\n    }\n  }\n"): (typeof documents)["\n  mutation CreateGeofence($geofence: GeofenceDtoInput!) {\n    createGeofence(command: { geofence: $geofence }) {\n      ...GeofenceFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateGeofence($id: UUID!, $geofence: GeofenceDtoInput!) {\n    updateGeofence(id: $id, command: { geofence: $geofence })\n  }\n"): (typeof documents)["\n  mutation UpdateGeofence($id: UUID!, $geofence: GeofenceDtoInput!) {\n    updateGeofence(id: $id, command: { geofence: $geofence })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteGeofence($id: UUID!) {\n    deleteGeofence(id: $id)\n  }\n"): (typeof documents)["\n  mutation DeleteGeofence($id: UUID!) {\n    deleteGeofence(id: $id)\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;