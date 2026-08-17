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
    "\n  fragment PositionFields on PositionVm {\n    transporterId\n    deviceName\n    transporterType\n    latitude\n    longitude\n    altitude\n    speed\n    course\n    deviceDateTime\n    serverDateTime\n    eventId\n    address\n    city\n    state\n    country\n    attributes {\n      ignition\n      satellites\n      mileage\n      hourmeter\n      temperature\n    }\n  }\n": typeof types.PositionFieldsFragmentDoc,
    "\n  fragment TripFields on TripVm {\n    tripId\n    totalDistance\n    duration\n    averageSpeed\n    type\n    from\n    to\n    points {\n      latitude\n      longitude\n      deviceDateTime\n      speed\n      course\n      eventId\n    }\n  }\n": typeof types.TripFieldsFragmentDoc,
    "\n  fragment AddressFields on AddressVm {\n    address\n    city\n    state\n    country\n  }\n": typeof types.AddressFieldsFragmentDoc,
    "\n  query PingOperator($operatorId: UUID!) {\n    pingOperator(query: { operatorId: $operatorId })\n  }\n": typeof types.PingOperatorDocument,
    "\n  query GetDevicePositionsByUser {\n    devicePositionsByUser {\n      ...PositionFields\n    }\n  }\n": typeof types.GetDevicePositionsByUserDocument,
    "\n  query GetTripsByTransporter(\n    $transporterId: UUID!\n    $from: DateTime!\n    $to: DateTime!\n    $source: PositionSourceType!\n  ) {\n    tripsByTransporter(\n      query: { transporterId: $transporterId, from: $from, to: $to, source: $source }\n    ) {\n      ...TripFields\n    }\n  }\n": typeof types.GetTripsByTransporterDocument,
    "\n  query ReverseGeocode($latitude: Float!, $longitude: Float!, $transporterId: UUID) {\n    reverseGeocode(\n      query: { latitude: $latitude, longitude: $longitude, transporterId: $transporterId }\n    ) {\n      ...AddressFields\n    }\n  }\n": typeof types.ReverseGeocodeDocument,
    "\n  query GetProviderCapabilities {\n    providerCapabilities {\n      protocolTypeId\n      protocol\n      displayName\n      realTimePositions\n      positionHistory\n      deviceCatalog\n      connectivityPing\n    }\n  }\n": typeof types.GetProviderCapabilitiesDocument,
};
const documents: Documents = {
    "\n  fragment PositionFields on PositionVm {\n    transporterId\n    deviceName\n    transporterType\n    latitude\n    longitude\n    altitude\n    speed\n    course\n    deviceDateTime\n    serverDateTime\n    eventId\n    address\n    city\n    state\n    country\n    attributes {\n      ignition\n      satellites\n      mileage\n      hourmeter\n      temperature\n    }\n  }\n": types.PositionFieldsFragmentDoc,
    "\n  fragment TripFields on TripVm {\n    tripId\n    totalDistance\n    duration\n    averageSpeed\n    type\n    from\n    to\n    points {\n      latitude\n      longitude\n      deviceDateTime\n      speed\n      course\n      eventId\n    }\n  }\n": types.TripFieldsFragmentDoc,
    "\n  fragment AddressFields on AddressVm {\n    address\n    city\n    state\n    country\n  }\n": types.AddressFieldsFragmentDoc,
    "\n  query PingOperator($operatorId: UUID!) {\n    pingOperator(query: { operatorId: $operatorId })\n  }\n": types.PingOperatorDocument,
    "\n  query GetDevicePositionsByUser {\n    devicePositionsByUser {\n      ...PositionFields\n    }\n  }\n": types.GetDevicePositionsByUserDocument,
    "\n  query GetTripsByTransporter(\n    $transporterId: UUID!\n    $from: DateTime!\n    $to: DateTime!\n    $source: PositionSourceType!\n  ) {\n    tripsByTransporter(\n      query: { transporterId: $transporterId, from: $from, to: $to, source: $source }\n    ) {\n      ...TripFields\n    }\n  }\n": types.GetTripsByTransporterDocument,
    "\n  query ReverseGeocode($latitude: Float!, $longitude: Float!, $transporterId: UUID) {\n    reverseGeocode(\n      query: { latitude: $latitude, longitude: $longitude, transporterId: $transporterId }\n    ) {\n      ...AddressFields\n    }\n  }\n": types.ReverseGeocodeDocument,
    "\n  query GetProviderCapabilities {\n    providerCapabilities {\n      protocolTypeId\n      protocol\n      displayName\n      realTimePositions\n      positionHistory\n      deviceCatalog\n      connectivityPing\n    }\n  }\n": types.GetProviderCapabilitiesDocument,
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
export function graphql(source: "\n  fragment PositionFields on PositionVm {\n    transporterId\n    deviceName\n    transporterType\n    latitude\n    longitude\n    altitude\n    speed\n    course\n    deviceDateTime\n    serverDateTime\n    eventId\n    address\n    city\n    state\n    country\n    attributes {\n      ignition\n      satellites\n      mileage\n      hourmeter\n      temperature\n    }\n  }\n"): (typeof documents)["\n  fragment PositionFields on PositionVm {\n    transporterId\n    deviceName\n    transporterType\n    latitude\n    longitude\n    altitude\n    speed\n    course\n    deviceDateTime\n    serverDateTime\n    eventId\n    address\n    city\n    state\n    country\n    attributes {\n      ignition\n      satellites\n      mileage\n      hourmeter\n      temperature\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment TripFields on TripVm {\n    tripId\n    totalDistance\n    duration\n    averageSpeed\n    type\n    from\n    to\n    points {\n      latitude\n      longitude\n      deviceDateTime\n      speed\n      course\n      eventId\n    }\n  }\n"): (typeof documents)["\n  fragment TripFields on TripVm {\n    tripId\n    totalDistance\n    duration\n    averageSpeed\n    type\n    from\n    to\n    points {\n      latitude\n      longitude\n      deviceDateTime\n      speed\n      course\n      eventId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AddressFields on AddressVm {\n    address\n    city\n    state\n    country\n  }\n"): (typeof documents)["\n  fragment AddressFields on AddressVm {\n    address\n    city\n    state\n    country\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query PingOperator($operatorId: UUID!) {\n    pingOperator(query: { operatorId: $operatorId })\n  }\n"): (typeof documents)["\n  query PingOperator($operatorId: UUID!) {\n    pingOperator(query: { operatorId: $operatorId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDevicePositionsByUser {\n    devicePositionsByUser {\n      ...PositionFields\n    }\n  }\n"): (typeof documents)["\n  query GetDevicePositionsByUser {\n    devicePositionsByUser {\n      ...PositionFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTripsByTransporter(\n    $transporterId: UUID!\n    $from: DateTime!\n    $to: DateTime!\n    $source: PositionSourceType!\n  ) {\n    tripsByTransporter(\n      query: { transporterId: $transporterId, from: $from, to: $to, source: $source }\n    ) {\n      ...TripFields\n    }\n  }\n"): (typeof documents)["\n  query GetTripsByTransporter(\n    $transporterId: UUID!\n    $from: DateTime!\n    $to: DateTime!\n    $source: PositionSourceType!\n  ) {\n    tripsByTransporter(\n      query: { transporterId: $transporterId, from: $from, to: $to, source: $source }\n    ) {\n      ...TripFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ReverseGeocode($latitude: Float!, $longitude: Float!, $transporterId: UUID) {\n    reverseGeocode(\n      query: { latitude: $latitude, longitude: $longitude, transporterId: $transporterId }\n    ) {\n      ...AddressFields\n    }\n  }\n"): (typeof documents)["\n  query ReverseGeocode($latitude: Float!, $longitude: Float!, $transporterId: UUID) {\n    reverseGeocode(\n      query: { latitude: $latitude, longitude: $longitude, transporterId: $transporterId }\n    ) {\n      ...AddressFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetProviderCapabilities {\n    providerCapabilities {\n      protocolTypeId\n      protocol\n      displayName\n      realTimePositions\n      positionHistory\n      deviceCatalog\n      connectivityPing\n    }\n  }\n"): (typeof documents)["\n  query GetProviderCapabilities {\n    providerCapabilities {\n      protocolTypeId\n      protocol\n      displayName\n      realTimePositions\n      positionHistory\n      deviceCatalog\n      connectivityPing\n    }\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;