/* eslint-disable */
/** Internal type. DO NOT USE DIRECTLY. */
type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
/** Internal type. DO NOT USE DIRECTLY. */
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type AccountBrandingDtoInput = {
  accountId: string;
  displayName: string;
  logoDocumentId?: string | null | undefined;
  primaryColor: string;
  reportHeader?: string | null | undefined;
};

export type AccountDtoInput = {
  active: boolean;
  description?: string | null | undefined;
  emailAddress: string;
  firstName: string;
  lastName: string;
  name: string;
  password: string;
  typeId: number;
};

export type AccountFeatureDtoInput = {
  accountId: string;
  configurationJson?: string | null | undefined;
  effectiveFrom?: string | null | undefined;
  effectiveTo?: string | null | undefined;
  enabled: boolean;
  featureKey: string;
  source: string;
  tier: string;
};

export type AccountSettingsDtoInput = {
  accountId: string;
  maps: string;
  mapsKey?: string | null | undefined;
  onlineInterval: number;
  refreshMap: boolean;
  refreshMapInterval: number;
};

export type AccountStatus =
  | 'ACTIVE'
  | 'ARCHIVED'
  | 'CANCELLED'
  | 'SUSPENDED'
  | 'TRIAL';

export type AccountSupportGrantDtoInput = {
  accessLevel: string;
  accountId: string;
  endsAt: string;
  reason: string;
  startsAt: string;
  supportUserId: string;
  ticketReference: string;
};

export type AccountType =
  | 'ASSOCIATE'
  | 'BUSINESS'
  | 'PERSONAL';

export type AlertSubscriptionDtoInput = {
  accountId: string;
  channel: string;
  contact?: string | null | undefined;
  enabled: boolean;
  eventTypeFilter?: string | null | undefined;
  principalId: string;
  principalType: string;
};

export type AnnouncementSeverity =
  | 'CRITICAL'
  | 'INFO'
  | 'WARNING';

export type AssignmentStatus =
  | 'ACTIVE'
  | 'ENDED'
  | 'SUPERSEDED';

export type CredentialDtoInput = {
  key?: string | null | undefined;
  key2?: string | null | undefined;
  operatorId: string;
  password: string;
  uri: string;
  username: string;
};

export type DetectedStatus =
  | 'ASSIGNED'
  | 'AVAILABLE'
  | 'IGNORED'
  | 'NEW'
  | 'REMOVED';

export type DeviceDtoInput = {
  accountId: string;
  description?: string | null | undefined;
  deviceTypeId: number;
  identifier: number;
  name: string;
  operatorId: string;
  providerDisplayName?: string | null | undefined;
  providerMetadataHash?: string | null | undefined;
  providerStatus?: string | null | undefined;
  serial: string;
};

export type DeviceType =
  | 'AVIATION'
  | 'CAMERA'
  | 'CELLULAR'
  | 'CYCLING'
  | 'DRONES'
  | 'EMERGENCY_LOCATOR'
  | 'FITNESS'
  | 'HANDHELD'
  | 'MARINE'
  | 'OBD_SCANNER'
  | 'PET_TRACKING'
  | 'PHONE'
  | 'SATELLITE'
  | 'SMARTWATCH'
  | 'WEARABLE';

export type DocumentSearchFilterInput = {
  category?: string | null | undefined;
  classification?: string | null | undefined;
  expiringWithinDays?: number | null | undefined;
  from?: string | null | undefined;
  ownerEntityId?: string | null | undefined;
  ownerEntityType?: string | null | undefined;
  status?: string | null | undefined;
  to?: string | null | undefined;
  uploadedByPrincipalId?: string | null | undefined;
};

export type DocumentSignatureDtoInput = {
  documentId: string;
  latitude?: number | null | undefined;
  legalTextAccepted: string;
  longitude?: number | null | undefined;
  signatureImageDocumentId?: string | null | undefined;
  signerName: string;
  signerPrincipalId: string;
  signerPrincipalType: string;
};

export type DocumentTypeDtoInput = {
  accountId: string;
  category: string;
  defaultValidityDays?: number | null | undefined;
  displayName?: string | null | undefined;
  expiring: boolean;
  required: boolean;
};

export type DriverDtoInput = {
  accountId: string;
  active: boolean;
  defaultTransporterId?: string | null | undefined;
  documentNumber?: string | null | undefined;
  documentType?: string | null | undefined;
  employeeCode?: string | null | undefined;
  licenseExpiresAt?: string | null | undefined;
  licenseNumber?: string | null | undefined;
  name: string;
  phone?: string | null | undefined;
};

export type DriverQualificationDtoInput = {
  accountId: string;
  category?: string | null | undefined;
  documentId?: string | null | undefined;
  driverId: string;
  expiresAt?: string | null | undefined;
  issuedAt?: string | null | undefined;
  issuingAuthority?: string | null | undefined;
  notes?: string | null | undefined;
  number?: string | null | undefined;
  qualificationType: string;
  status: string;
};

export type GeocodingProviderDtoInput = {
  active: boolean;
  apiKey?: string | null | undefined;
  configurationJson?: string | null | undefined;
  endpointUri: string;
  name: string;
  requestsPerSecond: number;
  timeoutSeconds: number;
  type: number;
};

export type GroupDtoInput = {
  active: boolean;
  description: string;
  name: string;
};

export type NotificationRuleDtoInput = {
  accountId: string;
  channelsJson: string;
  configurationJson?: string | null | undefined;
  enabled: boolean;
  recipientSelector: string;
  ruleKey: string;
  ruleType: string;
  throttlingJson?: string | null | undefined;
  triggerEvent: string;
};

export type NotificationTemplateDtoInput = {
  accountId?: string | null | undefined;
  active: boolean;
  body: string;
  channel: string;
  locale: string;
  subject?: string | null | undefined;
  templateKey: string;
};

export type OperatorDtoInput = {
  address?: string | null | undefined;
  contactName?: string | null | undefined;
  description?: string | null | undefined;
  emailAddress?: string | null | undefined;
  name: string;
  phoneNumber?: string | null | undefined;
  protocolTypeId: number;
  syncIntervalMinutes?: number;
};

export type OperatorHealthStatus =
  | 'DEGRADED'
  | 'DISABLED'
  | 'HEALTHY'
  | 'OFFLINE'
  | 'UNKNOWN';

export type PlatformAnnouncementDtoInput = {
  active: boolean;
  endsAt?: string | null | undefined;
  messageEn: string;
  messageEs?: string | null | undefined;
  severity: AnnouncementSeverity;
  startsAt?: string | null | undefined;
};

export type PointOfInterestDtoInput = {
  accountId: string;
  active: boolean;
  address?: string | null | undefined;
  color?: number | null | undefined;
  description?: string | null | undefined;
  groupId?: number | null | undefined;
  latitude: number;
  longitude: number;
  name: string;
  type: number;
};

export type PrincipalType =
  | 'DRIVER'
  | 'PUBLIC_LINK'
  | 'SERVICE_CLIENT'
  | 'UNKNOWN'
  | 'USER';

export type ProtocolType =
  | 'COMMAND_TRACK'
  | 'FLESPI'
  | 'GEO_TAB'
  | 'GPS_GATE'
  | 'METTAX'
  | 'NAVIXY'
  | 'PROTRACK'
  | 'SAMSARA'
  | 'TRACCAR'
  | 'WIALON';

export type PublicLinkGrantDtoInput = {
  accountId: string;
  createdByPrincipalId: string;
  expiresAt: string;
  purpose: string;
  resourceId: string;
  resourceType: string;
  scopes: string;
  subjectTokenIdHash?: string | null | undefined;
};

export type ReportType =
  | 'BASIC'
  | 'CUSTOM'
  | 'EXTERNAL';

export type TransporterDeviceAssignmentDtoInput = {
  accountId: string;
  assignmentReason?: string | null | undefined;
  deviceId: string;
  isPrimary: boolean;
  priority: number;
  transporterId: string;
};

export type TransporterDtoInput = {
  accountId: string;
  name: string;
  transporterTypeId: number;
};

export type TransporterGroupDtoInput = {
  groupId: number;
  transporterId: string;
};

export type TransporterType =
  | 'AIRCRAFT'
  | 'ASSET'
  | 'BICYCLE'
  | 'BOAT'
  | 'CAR'
  | 'CARGO_CONTAINER'
  | 'CHILD'
  | 'CONSTRUCTION_VEHICLE'
  | 'DELIVERY_VAN'
  | 'DRONE'
  | 'ELDERLY_PERSON'
  | 'FLEET_VEHICLE'
  | 'HEAVY_EQUIPMENT'
  | 'LIVESTOCK'
  | 'MOTORCYCLE'
  | 'PACKAGE'
  | 'PERSON'
  | 'PET'
  | 'SCHOOL_BUS'
  | 'SCOOTER'
  | 'TAXI'
  | 'TOOL'
  | 'TRACTOR'
  | 'TRUCK';

export type TransporterTypeDtoInput = {
  accBased: boolean;
  maxDistance: number;
  maxTimeGap: number;
  stoppedGap: number;
  transporterTypeId: number;
};

export type TriggerOperatorDeviceSyncCommandInput = {
  autoAssignNewDevices?: boolean | null | undefined;
  operatorId: string;
  resetDeviceCatalog?: boolean;
};

export type UpdateAccountDtoInput = {
  accountId: string;
  active: boolean;
  description?: string | null | undefined;
  name: string;
  typeId: number;
};

export type UpdateCredentialDtoInput = {
  credentialId: string;
  key: string;
  key2: string;
  password: string;
  uri: string;
  username: string;
};

export type UpdateGeocodingProviderDtoInput = {
  apiKey?: string | null | undefined;
  configurationJson?: string | null | undefined;
  endpointUri: string;
  name: string;
  requestsPerSecond: number;
  timeoutSeconds: number;
  type: number;
};

export type UpdateGroupDtoInput = {
  active: boolean;
  description: string;
  groupId: number;
  name: string;
};

export type UpdateOperatorDtoInput = {
  address?: string | null | undefined;
  contactName?: string | null | undefined;
  description?: string | null | undefined;
  emailAddress?: string | null | undefined;
  name: string;
  operatorId: string;
  phoneNumber?: string | null | undefined;
  protocolTypeId: number;
  syncIntervalMinutes: number;
};

export type UpdatePointOfInterestDtoInput = {
  active: boolean;
  address?: string | null | undefined;
  color?: number | null | undefined;
  description?: string | null | undefined;
  groupId?: number | null | undefined;
  latitude: number;
  longitude: number;
  name: string;
  type: number;
};

export type UpdateReportDtoInput = {
  active: boolean;
  category: string;
  description?: string | null | undefined;
  managerOnly: boolean;
  reportId: string;
  requiredFeatureKey?: string | null | undefined;
  sortOrder: number;
  supportsPdf: boolean;
  typeId: number;
};

export type UpdateTransporterDtoInput = {
  name: string;
  transporterId: string;
  transporterTypeId: number;
};

export type UserGroupDtoInput = {
  groupId: number;
  userId: string;
};

export type UserSettingsDtoInput = {
  language: string;
  navbar: string;
  style: string;
  userId: string;
};

export type AccountFeatureItemFragment = { accountFeatureId: string, accountId: string, featureKey: string, enabled: boolean, tier: string, source: string, effectiveFrom: string | null, effectiveTo: string | null, configurationJson: string | null, lastModified: string };

export type GetAccountFeaturesQueryVariables = Exact<{
  accountId: string;
}>;


export type GetAccountFeaturesQuery = { accountFeatures: Array<{ accountFeatureId: string, accountId: string, featureKey: string, enabled: boolean, tier: string, source: string, effectiveFrom: string | null, effectiveTo: string | null, configurationJson: string | null, lastModified: string }> };

export type SetAccountFeatureMutationVariables = Exact<{
  feature: AccountFeatureDtoInput;
}>;


export type SetAccountFeatureMutation = { setAccountFeature: { accountFeatureId: string, accountId: string, featureKey: string, enabled: boolean, tier: string, source: string, effectiveFrom: string | null, effectiveTo: string | null, configurationJson: string | null, lastModified: string } };

export type GetAccountFeaturesMasterQueryVariables = Exact<{
  accountId: string;
}>;


export type GetAccountFeaturesMasterQuery = { accountFeaturesMaster: Array<{ accountFeatureId: string, accountId: string, featureKey: string, enabled: boolean, tier: string, source: string, effectiveFrom: string | null, effectiveTo: string | null, configurationJson: string | null, lastModified: string }> };

export type SetAccountFeatureMasterMutationVariables = Exact<{
  feature: AccountFeatureDtoInput;
}>;


export type SetAccountFeatureMasterMutation = { setAccountFeatureMaster: { accountFeatureId: string, accountId: string, featureKey: string, enabled: boolean, tier: string, source: string, effectiveFrom: string | null, effectiveTo: string | null, configurationJson: string | null, lastModified: string } };

export type AccountItemFragment = { accountId: string, name: string, description: string | null, type: AccountType, typeId: number, status: AccountStatus, statusId: number, active: boolean, lastModified: string };

export type GetAccountByUserQueryVariables = Exact<{ [key: string]: never; }>;


export type GetAccountByUserQuery = { accountByUser: { accountId: string, name: string, description: string | null, type: AccountType, typeId: number, status: AccountStatus, statusId: number, active: boolean, lastModified: string } };

export type GetAccountsQueryVariables = Exact<{
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetAccountsQuery = { accounts: { totalCount: number, items: Array<{ accountId: string, name: string, description: string | null, type: AccountType, typeId: number, status: AccountStatus, statusId: number, active: boolean, lastModified: string }> } };

export type CreateAccountMutationVariables = Exact<{
  account: AccountDtoInput;
}>;


export type CreateAccountMutation = { createAccount: { accountId: string, name: string, description: string | null, type: AccountType, typeId: number, status: AccountStatus, statusId: number, active: boolean, lastModified: string } };

export type UpdateAccountMutationVariables = Exact<{
  id: string;
  account: UpdateAccountDtoInput;
}>;


export type UpdateAccountMutation = { updateAccount: boolean };

export type UpdateAccountMasterMutationVariables = Exact<{
  id: string;
  account: UpdateAccountDtoInput;
}>;


export type UpdateAccountMasterMutation = { updateAccountMaster: boolean };

export type ChangeAccountStatusMutationVariables = Exact<{
  accountId: string;
  targetStatus: AccountStatus;
  reason?: string | null | undefined;
}>;


export type ChangeAccountStatusMutation = { changeAccountStatus: { accountId: string, name: string, description: string | null, type: AccountType, typeId: number, status: AccountStatus, statusId: number, active: boolean, lastModified: string } };

export type GetAccountContextQueryVariables = Exact<{ [key: string]: never; }>;


export type GetAccountContextQuery = { accountContext: { status: AccountStatus, statusId: number, branding: { accountId: string, displayName: string, logoDocumentId: string | null, primaryColor: string, reportHeader: string | null, lastModified: string }, features: Array<{ accountFeatureId: string, accountId: string, featureKey: string, enabled: boolean, tier: string, source: string, effectiveFrom: string | null, effectiveTo: string | null, configurationJson: string | null, lastModified: string }> } };

export type AlertEventItemFragment = { alertEventId: string, accountId: string, eventType: string, severity: string, sourceModule: string, resourceType: string, resourceId: string, status: string, firstSeenAt: string, lastSeenAt: string, deduplicationKey: string };

export type GetAlertEventsQueryVariables = Exact<{
  accountId: string;
  skip: number;
  take: number;
}>;


export type GetAlertEventsQuery = { alertEvents: Array<{ alertEventId: string, accountId: string, eventType: string, severity: string, sourceModule: string, resourceType: string, resourceId: string, status: string, firstSeenAt: string, lastSeenAt: string, deduplicationKey: string }> };

export type AcknowledgeAlertEventMutationVariables = Exact<{
  alertEventId: string;
}>;


export type AcknowledgeAlertEventMutation = { acknowledgeAlertEvent: boolean };

export type ResolveAlertEventMutationVariables = Exact<{
  alertEventId: string;
}>;


export type ResolveAlertEventMutation = { resolveAlertEvent: boolean };

export type AlertSubscriptionItemFragment = { alertSubscriptionId: string, accountId: string, principalType: string, principalId: string, eventTypeFilter: string | null, channel: string, contact: string | null, enabled: boolean, lastModified: string };

export type GetAlertSubscriptionsQueryVariables = Exact<{
  accountId: string;
  principalId?: string | null | undefined;
  skip: number;
  take: number;
}>;


export type GetAlertSubscriptionsQuery = { alertSubscriptions: Array<{ alertSubscriptionId: string, accountId: string, principalType: string, principalId: string, eventTypeFilter: string | null, channel: string, contact: string | null, enabled: boolean, lastModified: string }> };

export type CreateAlertSubscriptionMutationVariables = Exact<{
  subscription: AlertSubscriptionDtoInput;
}>;


export type CreateAlertSubscriptionMutation = { createAlertSubscription: { alertSubscriptionId: string, accountId: string, principalType: string, principalId: string, channel: string, enabled: boolean } };

export type UpdateAlertSubscriptionMutationVariables = Exact<{
  alertSubscriptionId: string;
  subscription: AlertSubscriptionDtoInput;
}>;


export type UpdateAlertSubscriptionMutation = { updateAlertSubscription: boolean };

export type DeleteAlertSubscriptionMutationVariables = Exact<{
  alertSubscriptionId: string;
}>;


export type DeleteAlertSubscriptionMutation = { deleteAlertSubscription: string };

export type AuditEventItemFragment = { auditEventId: string, accountId: string, actorType: string, actorId: string, action: string, resourceType: string, resourceId: string, result: string, reason: string | null, correlationId: string | null, occurredAt: string };

export type GetAuditTrailQueryVariables = Exact<{
  accountId: string;
  skip: number;
  take: number;
}>;


export type GetAuditTrailQuery = { auditTrail: Array<{ auditEventId: string, accountId: string, actorType: string, actorId: string, action: string, resourceType: string, resourceId: string, result: string, reason: string | null, correlationId: string | null, occurredAt: string }> };

export type BackgroundJobRunItemFragment = { backgroundJobRunId: string, jobKey: string, accountId: string | null, resourceKey: string | null, idempotencyKey: string, status: string, attempts: number, startedAt: string, completedAt: string | null, errorCode: string | null, errorMessage: string | null };

export type GetBackgroundJobRunsQueryVariables = Exact<{
  accountId: string;
  skip: number;
  take: number;
}>;


export type GetBackgroundJobRunsQuery = { backgroundJobRuns: Array<{ backgroundJobRunId: string, jobKey: string, accountId: string | null, resourceKey: string | null, idempotencyKey: string, status: string, attempts: number, startedAt: string, completedAt: string | null, errorCode: string | null, errorMessage: string | null }> };

export type AccountBrandingItemFragment = { accountId: string, displayName: string, logoDocumentId: string | null, primaryColor: string, reportHeader: string | null, lastModified: string };

export type GetAccountBrandingQueryVariables = Exact<{
  accountId: string;
}>;


export type GetAccountBrandingQuery = { accountBranding: { accountId: string, displayName: string, logoDocumentId: string | null, primaryColor: string, reportHeader: string | null, lastModified: string } };

export type UpdateAccountBrandingMutationVariables = Exact<{
  branding: AccountBrandingDtoInput;
}>;


export type UpdateAccountBrandingMutation = { updateAccountBranding: { accountId: string, displayName: string, logoDocumentId: string | null, primaryColor: string, reportHeader: string | null, lastModified: string } };

export type CredentialFieldsFragment = { credentialId: string, key: string | null, key2: string | null, password: string, uri: string, username: string };

export type GetCredentialByOperatorQueryVariables = Exact<{
  operatorId: string;
}>;


export type GetCredentialByOperatorQuery = { credentialByOperator: { credentialId: string, key: string | null, key2: string | null, password: string, uri: string, username: string } };

export type CreateCredentialMutationVariables = Exact<{
  credential: CredentialDtoInput;
}>;


export type CreateCredentialMutation = { createCredential: { credentialId: string, key: string | null, key2: string | null, password: string, uri: string, username: string } };

export type UpdateCredentialMutationVariables = Exact<{
  id: string;
  credential: UpdateCredentialDtoInput;
}>;


export type UpdateCredentialMutation = { updateCredential: boolean };

export type DeviceItemFragment = { deviceId: string, name: string, serial: string, description: string | null, deviceType: DeviceType, deviceTypeId: number, identifier: number, operatorId: string };

export type SynchronizedDeviceFragment = { deviceId: string, accountId: string, operatorId: string, serial: string, name: string, identifier: number, providerDisplayName: string | null, providerStatus: string | null, detectedStatus: DetectedStatus, firstSeenAt: string, lastSeenAt: string, lastSyncedAt: string, lastAssignedAt: string | null, ignoredAt: string | null };

export type GetDevicesByAccountQueryVariables = Exact<{
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetDevicesByAccountQuery = { devicesByAccount: { totalCount: number, items: Array<{ deviceId: string, name: string, serial: string, description: string | null, deviceType: DeviceType, deviceTypeId: number, identifier: number, operatorId: string }> } };

export type GetDeviceLookupQueryVariables = Exact<{ [key: string]: never; }>;


export type GetDeviceLookupQuery = { deviceLookup: Array<{ deviceId: string, name: string, operatorId: string }> };

export type DeleteDeviceMutationVariables = Exact<{
  deviceId: string;
}>;


export type DeleteDeviceMutation = { deleteDevice: string };

export type GetSynchronizedDevicesQueryVariables = Exact<{
  accountId: string;
  detectedStatus?: DetectedStatus | null | undefined;
  operatorId?: string | null | undefined;
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
  unassignedOnly?: boolean | null | undefined;
  recentOnly?: boolean | null | undefined;
}>;


export type GetSynchronizedDevicesQuery = { synchronizedDevices: { totalCount: number, items: Array<{ deviceId: string, accountId: string, operatorId: string, serial: string, name: string, identifier: number, providerDisplayName: string | null, providerStatus: string | null, detectedStatus: DetectedStatus, firstSeenAt: string, lastSeenAt: string, lastSyncedAt: string, lastAssignedAt: string | null, ignoredAt: string | null }> } };

export type GetUnassignedSynchronizedDevicesQueryVariables = Exact<{
  accountId: string;
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetUnassignedSynchronizedDevicesQuery = { unassignedSynchronizedDevices: { totalCount: number, items: Array<{ deviceId: string, accountId: string, operatorId: string, serial: string, name: string, identifier: number, providerDisplayName: string | null, providerStatus: string | null, detectedStatus: DetectedStatus, firstSeenAt: string, lastSeenAt: string, lastSyncedAt: string, lastAssignedAt: string | null, ignoredAt: string | null }> } };

export type SetSynchronizedDeviceIgnoredMutationVariables = Exact<{
  deviceId: string;
  ignored: boolean;
}>;


export type SetSynchronizedDeviceIgnoredMutation = { setSynchronizedDeviceIgnored: boolean };

export type RegisterManualDeviceMutationVariables = Exact<{
  device: DeviceDtoInput;
  autoAssign: boolean;
}>;


export type RegisterManualDeviceMutation = { registerManualDevice: { deviceId: string, accountId: string, operatorId: string, serial: string, name: string, identifier: number, providerDisplayName: string | null, providerStatus: string | null, detectedStatus: DetectedStatus, firstSeenAt: string, lastSeenAt: string, lastSyncedAt: string, lastAssignedAt: string | null, ignoredAt: string | null } };

export type DocumentFieldsFragment = { documentId: string, accountId: string, ownerEntityType: string, ownerEntityId: string, uploadedByPrincipalType: string, uploadedByPrincipalId: string, fileName: string, category: string, title: string | null, description: string | null, contentType: string, sizeBytes: number, sha256Hash: string, classification: string, status: string, expiresAt: string | null, visibilityScope: string, scanStatus: string, currentVersion: number, downloadUrl: string | null, lastModified: string };

export type DocumentVersionFieldsFragment = { documentVersionId: string, documentId: string, accountId: string, versionNumber: number, contentType: string, fileName: string, sizeBytes: number, sha256Hash: string, scanStatus: string, replacedByPrincipalType: string | null, replacedByPrincipalId: string | null, reason: string | null, createdAt: string };

export type DocumentTypeFieldsFragment = { documentTypeId: string, accountId: string, category: string, displayName: string | null, required: boolean, expiring: boolean, defaultValidityDays: number | null, enabled: boolean, createdAt: string };

export type DocumentSignatureFieldsFragment = { documentSignatureId: string, documentId: string, accountId: string, signerPrincipalType: string, signerPrincipalId: string, signerName: string, signatureImageDocumentId: string | null, legalTextAccepted: string, latitude: number | null, longitude: number | null, signedAt: string, createdAt: string };

export type GetDocumentsForOwnerQueryVariables = Exact<{
  accountId: string;
  ownerEntityType: string;
  ownerEntityId: string;
  skip: number;
  take: number;
}>;


export type GetDocumentsForOwnerQuery = { documentsForOwner: Array<{ documentId: string, accountId: string, ownerEntityType: string, ownerEntityId: string, uploadedByPrincipalType: string, uploadedByPrincipalId: string, fileName: string, category: string, title: string | null, description: string | null, contentType: string, sizeBytes: number, sha256Hash: string, classification: string, status: string, expiresAt: string | null, visibilityScope: string, scanStatus: string, currentVersion: number, downloadUrl: string | null, lastModified: string }> };

export type GetDocumentQueryVariables = Exact<{
  documentId: string;
}>;


export type GetDocumentQuery = { document: { documentId: string, accountId: string, ownerEntityType: string, ownerEntityId: string, uploadedByPrincipalType: string, uploadedByPrincipalId: string, fileName: string, category: string, title: string | null, description: string | null, contentType: string, sizeBytes: number, sha256Hash: string, classification: string, status: string, expiresAt: string | null, visibilityScope: string, scanStatus: string, currentVersion: number, downloadUrl: string | null, lastModified: string } };

export type SearchDocumentsQueryVariables = Exact<{
  filter: DocumentSearchFilterInput;
  skip: number;
  take: number;
}>;


export type SearchDocumentsQuery = { searchDocuments: Array<{ documentId: string, accountId: string, ownerEntityType: string, ownerEntityId: string, uploadedByPrincipalType: string, uploadedByPrincipalId: string, fileName: string, category: string, title: string | null, description: string | null, contentType: string, sizeBytes: number, sha256Hash: string, classification: string, status: string, expiresAt: string | null, visibilityScope: string, scanStatus: string, currentVersion: number, downloadUrl: string | null, lastModified: string }> };

export type GetExpiringDocumentsQueryVariables = Exact<{
  withinDays: number;
  skip: number;
  take: number;
}>;


export type GetExpiringDocumentsQuery = { expiringDocuments: Array<{ documentId: string, accountId: string, ownerEntityType: string, ownerEntityId: string, uploadedByPrincipalType: string, uploadedByPrincipalId: string, fileName: string, category: string, title: string | null, description: string | null, contentType: string, sizeBytes: number, sha256Hash: string, classification: string, status: string, expiresAt: string | null, visibilityScope: string, scanStatus: string, currentVersion: number, downloadUrl: string | null, lastModified: string }> };

export type GetDocumentTypesQueryVariables = Exact<{
  accountId: string;
  includeDisabled: boolean;
}>;


export type GetDocumentTypesQuery = { documentTypes: Array<{ documentTypeId: string, accountId: string, category: string, displayName: string | null, required: boolean, expiring: boolean, defaultValidityDays: number | null, enabled: boolean, createdAt: string }> };

export type VoidDocumentMutationVariables = Exact<{
  documentId: string;
  reason: string;
}>;


export type VoidDocumentMutation = { voidDocument: boolean };

export type ExpireDocumentMutationVariables = Exact<{
  documentId: string;
  expiresAt: string;
}>;


export type ExpireDocumentMutation = { expireDocument: boolean };

export type DeleteDocumentReferenceMutationVariables = Exact<{
  documentId: string;
}>;


export type DeleteDocumentReferenceMutation = { deleteDocumentReference: string };

export type SignDocumentMutationVariables = Exact<{
  signature: DocumentSignatureDtoInput;
}>;


export type SignDocumentMutation = { signDocument: { documentSignatureId: string, documentId: string, accountId: string, signerPrincipalType: string, signerPrincipalId: string, signerName: string, signatureImageDocumentId: string | null, legalTextAccepted: string, latitude: number | null, longitude: number | null, signedAt: string, createdAt: string } };

export type ConfigureDocumentTypeMutationVariables = Exact<{
  documentType: DocumentTypeDtoInput;
}>;


export type ConfigureDocumentTypeMutation = { configureDocumentType: { documentTypeId: string, accountId: string, category: string, displayName: string | null, required: boolean, expiring: boolean, defaultValidityDays: number | null, enabled: boolean, createdAt: string } };

export type DisableDocumentTypeMutationVariables = Exact<{
  documentTypeId: string;
}>;


export type DisableDocumentTypeMutation = { disableDocumentType: string };

export type DriverItemFragment = { driverId: string, accountId: string, name: string, phone: string | null, documentType: string | null, documentNumber: string | null, active: boolean, employeeCode: string | null, licenseNumber: string | null, licenseExpiresAt: string | null, defaultTransporterId: string | null, lastModified: string };

export type GetDriversByAccountQueryVariables = Exact<{
  accountId: string;
  skip: number;
  take: number;
}>;


export type GetDriversByAccountQuery = { driversByAccount: Array<{ driverId: string, accountId: string, name: string, phone: string | null, documentType: string | null, documentNumber: string | null, active: boolean, employeeCode: string | null, licenseNumber: string | null, licenseExpiresAt: string | null, defaultTransporterId: string | null, lastModified: string }> };

export type CreateDriverMutationVariables = Exact<{
  driver: DriverDtoInput;
}>;


export type CreateDriverMutation = { createDriver: { driverId: string, accountId: string, name: string, phone: string | null, documentType: string | null, documentNumber: string | null, active: boolean, employeeCode: string | null, licenseNumber: string | null, licenseExpiresAt: string | null, defaultTransporterId: string | null, lastModified: string } };

export type UpdateDriverMutationVariables = Exact<{
  driverId: string;
  driver: DriverDtoInput;
}>;


export type UpdateDriverMutation = { updateDriver: boolean };

export type DeactivateDriverMutationVariables = Exact<{
  driverId: string;
}>;


export type DeactivateDriverMutation = { deactivateDriver: boolean };

export type DriverQualificationItemFragment = { driverQualificationId: string, accountId: string, driverId: string, driverName: string, qualificationType: string, category: string | null, number: string | null, issuedAt: string | null, expiresAt: string | null, issuingAuthority: string | null, status: string, documentId: string | null, notes: string | null, lastModified: string };

export type DriverTransporterAssignmentItemFragment = { driverTransporterAssignmentId: string, accountId: string, driverId: string, driverName: string, transporterId: string, transporterName: string, startsAt: string, endsAt: string | null, assignmentType: string, status: string, createdByPrincipal: string, lastModified: string };

export type DriverActiveAssignmentItemFragment = { driverId: string, accountId: string, resourceType: string, resourceId: string, active: boolean, startsAt: string | null, endsAt: string | null, assignmentType: string | null };

export type GetDriverAssignmentsQueryVariables = Exact<{
  driverId: string;
}>;


export type GetDriverAssignmentsQuery = { driverAssignments: Array<{ driverId: string, accountId: string, resourceType: string, resourceId: string, active: boolean, startsAt: string | null, endsAt: string | null, assignmentType: string | null }> };

export type GetDriverQualificationsQueryVariables = Exact<{
  accountId: string;
  driverId?: string | null | undefined;
  expiringWithinDays?: number | null | undefined;
  skip: number;
  take: number;
}>;


export type GetDriverQualificationsQuery = { driverQualifications: Array<{ driverQualificationId: string, accountId: string, driverId: string, driverName: string, qualificationType: string, category: string | null, number: string | null, issuedAt: string | null, expiresAt: string | null, issuingAuthority: string | null, status: string, documentId: string | null, notes: string | null, lastModified: string }> };

export type GetDriverAssignmentHistoryQueryVariables = Exact<{
  accountId: string;
  driverId?: string | null | undefined;
  transporterId?: string | null | undefined;
  from?: string | null | undefined;
  to?: string | null | undefined;
  skip: number;
  take: number;
}>;


export type GetDriverAssignmentHistoryQuery = { driverAssignmentHistory: Array<{ driverTransporterAssignmentId: string, accountId: string, driverId: string, driverName: string, transporterId: string, transporterName: string, startsAt: string, endsAt: string | null, assignmentType: string, status: string, createdByPrincipal: string, lastModified: string }> };

export type CreateDriverQualificationMutationVariables = Exact<{
  qualification: DriverQualificationDtoInput;
}>;


export type CreateDriverQualificationMutation = { createDriverQualification: { driverQualificationId: string, accountId: string, driverId: string, driverName: string, qualificationType: string, category: string | null, number: string | null, issuedAt: string | null, expiresAt: string | null, issuingAuthority: string | null, status: string, documentId: string | null, notes: string | null, lastModified: string } };

export type UpdateDriverQualificationMutationVariables = Exact<{
  driverQualificationId: string;
  qualification: DriverQualificationDtoInput;
}>;


export type UpdateDriverQualificationMutation = { updateDriverQualification: boolean };

export type DeleteDriverQualificationMutationVariables = Exact<{
  driverQualificationId: string;
}>;


export type DeleteDriverQualificationMutation = { deleteDriverQualification: string };

export type AssignDriverToTransporterMutationVariables = Exact<{
  driverId: string;
  transporterId: string;
  startsAt: string;
  assignmentType: string;
}>;


export type AssignDriverToTransporterMutation = { assignDriverToTransporter: { driverTransporterAssignmentId: string, accountId: string, driverId: string, driverName: string, transporterId: string, transporterName: string, startsAt: string, endsAt: string | null, assignmentType: string, status: string, createdByPrincipal: string, lastModified: string } };

export type EndDriverAssignmentMutationVariables = Exact<{
  driverTransporterAssignmentId: string;
  endsAt?: string | null | undefined;
}>;


export type EndDriverAssignmentMutation = { endDriverAssignment: boolean };

export type GeocodingProviderItemFragment = { geocodingProviderId: string, name: string, type: number, endpointUri: string, apiKey: string | null, requestsPerSecond: number, timeoutSeconds: number, configurationJson: string | null, active: boolean };

export type GetGeocodingProvidersQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGeocodingProvidersQuery = { geocodingProviders: Array<{ geocodingProviderId: string, name: string, type: number, endpointUri: string, apiKey: string | null, requestsPerSecond: number, timeoutSeconds: number, configurationJson: string | null, active: boolean }> };

export type CreateGeocodingProviderMutationVariables = Exact<{
  geocodingProvider: GeocodingProviderDtoInput;
}>;


export type CreateGeocodingProviderMutation = { createGeocodingProvider: { geocodingProviderId: string, name: string, type: number, endpointUri: string, apiKey: string | null, requestsPerSecond: number, timeoutSeconds: number, configurationJson: string | null, active: boolean } };

export type UpdateGeocodingProviderMutationVariables = Exact<{
  id: string;
  geocodingProvider: UpdateGeocodingProviderDtoInput;
}>;


export type UpdateGeocodingProviderMutation = { updateGeocodingProvider: boolean };

export type DeleteGeocodingProviderMutationVariables = Exact<{
  id: string;
}>;


export type DeleteGeocodingProviderMutation = { deleteGeocodingProvider: string };

export type SetActiveGeocodingProviderMutationVariables = Exact<{
  id: string;
}>;


export type SetActiveGeocodingProviderMutation = { setActiveGeocodingProvider: boolean };

export type GetGpsIntegrationDashboardQueryVariables = Exact<{
  accountId: string;
}>;


export type GetGpsIntegrationDashboardQuery = { gpsIntegrationDashboard: { operatorsTotal: number, operatorsEnabled: number, operatorsHealthy: number, operatorsDegraded: number, operatorsOffline: number, devicesTotal: number, devicesNew: number, devicesAvailable: number, devicesAssigned: number, devicesIgnored: number, devicesRemoved: number, recentlyAddedDevicesLast24h: number, unassignedDevicesCount: number, syncRunsSucceededLast24h: number, syncRunsFailedLast24h: number, lastAutomaticSyncAt: string | null, lastManualSyncAt: string | null, averageSyncDurationSeconds: number, deviceCountsByProviderStatus: Array<{ operatorId: string, operatorName: string, detectedStatus: DetectedStatus, count: number }> } };

export type GroupItemFragment = { groupId: number, name: string, description: string, active: boolean, accountId: string };

export type GetGroupsQueryVariables = Exact<{
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetGroupsQuery = { groupsByAccount: { totalCount: number, items: Array<{ groupId: number, name: string, description: string, active: boolean, accountId: string }> } };

export type GetGroupLookupQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGroupLookupQuery = { groupLookup: Array<{ groupId: number, name: string }> };

export type CreateGroupMutationVariables = Exact<{
  group: GroupDtoInput;
}>;


export type CreateGroupMutation = { createGroup: { groupId: number, name: string, description: string, active: boolean, accountId: string } };

export type UpdateGroupMutationVariables = Exact<{
  id: number;
  group: UpdateGroupDtoInput;
}>;


export type UpdateGroupMutation = { updateGroup: boolean };

export type DeleteGroupMutationVariables = Exact<{
  id: number;
}>;


export type DeleteGroupMutation = { deleteGroup: number };

export type GetUsersByGroupQueryVariables = Exact<{
  groupId: number;
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetUsersByGroupQuery = { usersByGroup: { totalCount: number, items: Array<{ userId: string, username: string, active: boolean, accountId: string }> } };

export type CreateUserGroupMutationVariables = Exact<{
  userGroup: UserGroupDtoInput;
}>;


export type CreateUserGroupMutation = { createUserGroup: { userId: string, groupId: number } };

export type DeleteUserGroupMutationVariables = Exact<{
  userId: string;
  groupId: number;
}>;


export type DeleteUserGroupMutation = { deleteUserGroup: string };

export type CreateTransporterGroupMutationVariables = Exact<{
  transporterGroup: TransporterGroupDtoInput;
}>;


export type CreateTransporterGroupMutation = { createTransporterGroup: { transporterId: string, groupId: number } };

export type DeleteTransporterGroupMutationVariables = Exact<{
  transporterId: string;
  groupId: number;
}>;


export type DeleteTransporterGroupMutation = { deleteTransporterGroup: string };

export type NotificationDeliveryItemFragment = { notificationDeliveryId: string, accountId: string, notificationRuleId: string | null, alertEventId: string | null, channel: string, recipientPrincipalType: string, recipient: string, status: string, attempts: number, providerMessageId: string | null, error: string | null, sentAt: string | null, readAt: string | null };

export type GetNotificationDeliveriesQueryVariables = Exact<{
  accountId: string;
  status?: string | null | undefined;
  channel?: string | null | undefined;
  from?: string | null | undefined;
  to?: string | null | undefined;
  skip: number;
  take: number;
}>;


export type GetNotificationDeliveriesQuery = { notificationDeliveries: Array<{ notificationDeliveryId: string, accountId: string, notificationRuleId: string | null, alertEventId: string | null, channel: string, recipientPrincipalType: string, recipient: string, status: string, attempts: number, providerMessageId: string | null, error: string | null, sentAt: string | null, readAt: string | null }> };

export type GetDeliveryHealthQueryVariables = Exact<{
  accountId: string;
  from: string;
  to: string;
}>;


export type GetDeliveryHealthQuery = { deliveryHealth: Array<{ channel: string, status: string, count: number, averageAttempts: number }> };

export type MyNotificationItemFragment = { notificationDeliveryId: string, alertEventId: string | null, eventType: string | null, severity: string | null, sourceModule: string | null, resourceType: string | null, resourceId: string | null, payloadJson: string | null, createdAt: string, readAt: string | null };

export type GetMyNotificationsQueryVariables = Exact<{
  unreadOnly: boolean;
  skip: number;
  take: number;
}>;


export type GetMyNotificationsQuery = { myNotifications: Array<{ notificationDeliveryId: string, alertEventId: string | null, eventType: string | null, severity: string | null, sourceModule: string | null, resourceType: string | null, resourceId: string | null, payloadJson: string | null, createdAt: string, readAt: string | null }> };

export type RetryNotificationDeliveryMutationVariables = Exact<{
  notificationDeliveryId: string;
}>;


export type RetryNotificationDeliveryMutation = { retryNotificationDelivery: boolean };

export type MarkNotificationReadMutationVariables = Exact<{
  notificationDeliveryId: string;
}>;


export type MarkNotificationReadMutation = { markNotificationRead: boolean };

export type SendTestNotificationMutationVariables = Exact<{
  accountId: string;
  channel: string;
  contact?: string | null | undefined;
}>;


export type SendTestNotificationMutation = { sendTestNotification: { notificationDeliveryId: string, channel: string, status: string } };

export type NotificationRuleItemFragment = { notificationRuleId: string, accountId: string, ruleKey: string, ruleType: string, enabled: boolean, triggerEvent: string, recipientSelector: string, channelsJson: string, throttlingJson: string | null, configurationJson: string | null, lastModified: string };

export type GetNotificationRulesQueryVariables = Exact<{
  accountId: string;
  skip: number;
  take: number;
}>;


export type GetNotificationRulesQuery = { notificationRules: Array<{ notificationRuleId: string, accountId: string, ruleKey: string, ruleType: string, enabled: boolean, triggerEvent: string, recipientSelector: string, channelsJson: string, throttlingJson: string | null, configurationJson: string | null, lastModified: string }> };

export type CreateNotificationRuleMutationVariables = Exact<{
  notificationRule: NotificationRuleDtoInput;
}>;


export type CreateNotificationRuleMutation = { createNotificationRule: { notificationRuleId: string, accountId: string, ruleKey: string, enabled: boolean, lastModified: string } };

export type UpdateNotificationRuleMutationVariables = Exact<{
  notificationRuleId: string;
  notificationRule: NotificationRuleDtoInput;
}>;


export type UpdateNotificationRuleMutation = { updateNotificationRule: boolean };

export type DisableNotificationRuleMutationVariables = Exact<{
  notificationRuleId: string;
}>;


export type DisableNotificationRuleMutation = { disableNotificationRule: boolean };

export type NotificationTemplateItemFragment = { notificationTemplateId: string, accountId: string | null, templateKey: string, channel: string, locale: string, subject: string | null, body: string, active: boolean, lastModified: string };

export type GetNotificationTemplatesQueryVariables = Exact<{
  accountId: string;
}>;


export type GetNotificationTemplatesQuery = { notificationTemplates: Array<{ notificationTemplateId: string, accountId: string | null, templateKey: string, channel: string, locale: string, subject: string | null, body: string, active: boolean, lastModified: string }> };

export type CreateNotificationTemplateMutationVariables = Exact<{
  template: NotificationTemplateDtoInput;
}>;


export type CreateNotificationTemplateMutation = { createNotificationTemplate: { notificationTemplateId: string, accountId: string | null, templateKey: string, channel: string, locale: string, active: boolean } };

export type UpdateNotificationTemplateMutationVariables = Exact<{
  notificationTemplateId: string;
  template: NotificationTemplateDtoInput;
}>;


export type UpdateNotificationTemplateMutation = { updateNotificationTemplate: boolean };

export type DeleteNotificationTemplateMutationVariables = Exact<{
  notificationTemplateId: string;
}>;


export type DeleteNotificationTemplateMutation = { deleteNotificationTemplate: string };

export type OperatorDetailFragment = { operatorId: string, name: string, description: string | null, phoneNumber: string | null, emailAddress: string | null, address: string | null, contactName: string | null, protocolType: ProtocolType, protocolTypeId: number, enabled: boolean, syncIntervalMinutes: number, healthStatus: OperatorHealthStatus, lastSuccessfulSyncAt: string | null, lastFailedSyncAt: string | null, lastFailureCode: string | null, lastLatencyMs: number | null, lastDeviceSyncAt: string | null, lastPositionSyncAt: string | null, lastModified: string };

export type OperatorGpsFragment = { operatorId: string, name: string, protocolType: ProtocolType, protocolTypeId: number, enabled: boolean, lastDeviceSyncAt: string | null, lastPositionSyncAt: string | null, syncIntervalMinutes: number };

export type GetOperatorsByCurrentAccountQueryVariables = Exact<{
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetOperatorsByCurrentAccountQuery = { operatorsByCurrentAccount: { totalCount: number, items: Array<{ operatorId: string, name: string, description: string | null, phoneNumber: string | null, emailAddress: string | null, address: string | null, contactName: string | null, protocolType: ProtocolType, protocolTypeId: number, enabled: boolean, syncIntervalMinutes: number, healthStatus: OperatorHealthStatus, lastSuccessfulSyncAt: string | null, lastFailedSyncAt: string | null, lastFailureCode: string | null, lastLatencyMs: number | null, lastDeviceSyncAt: string | null, lastPositionSyncAt: string | null, lastModified: string }> } };

export type GetOperatorLookupQueryVariables = Exact<{ [key: string]: never; }>;


export type GetOperatorLookupQuery = { operatorLookup: Array<{ operatorId: string, name: string }> };

export type GetGpsOperatorsQueryVariables = Exact<{
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetGpsOperatorsQuery = { operatorsByCurrentAccount: { totalCount: number, items: Array<{ operatorId: string, name: string, protocolType: ProtocolType, protocolTypeId: number, enabled: boolean, lastDeviceSyncAt: string | null, lastPositionSyncAt: string | null, syncIntervalMinutes: number }> } };

export type CreateOperatorMutationVariables = Exact<{
  operator: OperatorDtoInput;
}>;


export type CreateOperatorMutation = { createOperator: { operatorId: string, name: string, description: string | null, phoneNumber: string | null, emailAddress: string | null, address: string | null, contactName: string | null, protocolType: ProtocolType, protocolTypeId: number, enabled: boolean, syncIntervalMinutes: number, healthStatus: OperatorHealthStatus, lastSuccessfulSyncAt: string | null, lastFailedSyncAt: string | null, lastFailureCode: string | null, lastLatencyMs: number | null, lastDeviceSyncAt: string | null, lastPositionSyncAt: string | null, lastModified: string } };

export type UpdateOperatorMutationVariables = Exact<{
  id: string;
  operator: UpdateOperatorDtoInput;
}>;


export type UpdateOperatorMutation = { updateOperator: boolean };

export type DeleteOperatorMutationVariables = Exact<{
  id: string;
}>;


export type DeleteOperatorMutation = { deleteOperator: string };

export type SetOperatorEnabledMutationVariables = Exact<{
  operatorId: string;
  enabled: boolean;
}>;


export type SetOperatorEnabledMutation = { setOperatorEnabled: boolean };

export type TriggerOperatorDeviceSyncMutationVariables = Exact<{
  command: TriggerOperatorDeviceSyncCommandInput;
}>;


export type TriggerOperatorDeviceSyncMutation = { triggerOperatorDeviceSync: boolean };

export type PlatformAnnouncementItemFragment = { platformAnnouncementId: string, messageEn: string, messageEs: string | null, severity: AnnouncementSeverity, startsAt: string | null, endsAt: string | null, active: boolean, lastModified: string };

export type GetPlatformAnnouncementsQueryVariables = Exact<{
  skip: number;
  take: number;
}>;


export type GetPlatformAnnouncementsQuery = { platformAnnouncements: Array<{ platformAnnouncementId: string, messageEn: string, messageEs: string | null, severity: AnnouncementSeverity, startsAt: string | null, endsAt: string | null, active: boolean, lastModified: string }> };

export type GetBackgroundJobStatusQueryVariables = Exact<{ [key: string]: never; }>;


export type GetBackgroundJobStatusQuery = { backgroundJobStatus: Array<{ jobKey: string, status: string, startedAt: string, completedAt: string | null, attempts: number, errorCode: string | null, recordsEveryCycle: boolean }> };

export type CreatePlatformAnnouncementMutationVariables = Exact<{
  announcement: PlatformAnnouncementDtoInput;
}>;


export type CreatePlatformAnnouncementMutation = { createPlatformAnnouncement: { platformAnnouncementId: string, messageEn: string, messageEs: string | null, severity: AnnouncementSeverity, startsAt: string | null, endsAt: string | null, active: boolean, lastModified: string } };

export type UpdatePlatformAnnouncementMutationVariables = Exact<{
  platformAnnouncementId: string;
  announcement: PlatformAnnouncementDtoInput;
}>;


export type UpdatePlatformAnnouncementMutation = { updatePlatformAnnouncement: boolean };

export type DeletePlatformAnnouncementMutationVariables = Exact<{
  platformAnnouncementId: string;
}>;


export type DeletePlatformAnnouncementMutation = { deletePlatformAnnouncement: string };

export type PointOfInterestItemFragment = { pointOfInterestId: string, accountId: string, name: string, description: string | null, type: number, latitude: number, longitude: number, address: string | null, color: number | null, groupId: number | null, active: boolean };

export type GetPointsOfInterestByAccountQueryVariables = Exact<{
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetPointsOfInterestByAccountQuery = { pointsOfInterestByAccount: { totalCount: number, items: Array<{ pointOfInterestId: string, accountId: string, name: string, description: string | null, type: number, latitude: number, longitude: number, address: string | null, color: number | null, groupId: number | null, active: boolean }> } };

export type GetPointOfInterestLookupQueryVariables = Exact<{ [key: string]: never; }>;


export type GetPointOfInterestLookupQuery = { pointOfInterestLookup: Array<{ pointOfInterestId: string, name: string, latitude: number, longitude: number, color: number | null, type: number, description: string | null, address: string | null, active: boolean }> };

export type CreatePointOfInterestMutationVariables = Exact<{
  pointOfInterest: PointOfInterestDtoInput;
}>;


export type CreatePointOfInterestMutation = { createPointOfInterest: { pointOfInterestId: string, accountId: string, name: string, description: string | null, type: number, latitude: number, longitude: number, address: string | null, color: number | null, groupId: number | null, active: boolean } };

export type UpdatePointOfInterestMutationVariables = Exact<{
  id: string;
  pointOfInterest: UpdatePointOfInterestDtoInput;
}>;


export type UpdatePointOfInterestMutation = { updatePointOfInterest: boolean };

export type DeletePointOfInterestMutationVariables = Exact<{
  id: string;
}>;


export type DeletePointOfInterestMutation = { deletePointOfInterest: string };

export type CurrentPrincipalItemFragment = { subjectId: string | null, principalType: PrincipalType, userId: string | null, driverId: string | null, clientId: string | null, publicLinkGrantId: string | null, role: string | null, accountId: string | null, scopes: Array<string>, correlationId: string | null };

export type GetCurrentPrincipalQueryVariables = Exact<{ [key: string]: never; }>;


export type GetCurrentPrincipalQuery = { currentPrincipal: { subjectId: string | null, principalType: PrincipalType, userId: string | null, driverId: string | null, clientId: string | null, publicLinkGrantId: string | null, role: string | null, accountId: string | null, scopes: Array<string>, correlationId: string | null } };

export type PublicLinkGrantFieldsFragment = { publicLinkGrantId: string, accountId: string, resourceType: string, resourceId: string, scopes: string, purpose: string, expiresAt: string, revokedAt: string | null, revokedBy: string | null, createdByPrincipalId: string, accessCount: number, lastAccessedAt: string | null, lastModified: string, token: string | null };

export type GetPublicLinkGrantsByAccountQueryVariables = Exact<{
  accountId: string;
  skip: number;
  take: number;
}>;


export type GetPublicLinkGrantsByAccountQuery = { publicLinkGrantsByAccount: Array<{ publicLinkGrantId: string, accountId: string, resourceType: string, resourceId: string, scopes: string, purpose: string, expiresAt: string, revokedAt: string | null, revokedBy: string | null, createdByPrincipalId: string, accessCount: number, lastAccessedAt: string | null, lastModified: string, token: string | null }> };

export type CreatePublicLinkGrantMutationVariables = Exact<{
  publicLinkGrant: PublicLinkGrantDtoInput;
}>;


export type CreatePublicLinkGrantMutation = { createPublicLinkGrant: { publicLinkGrantId: string, accountId: string, resourceType: string, resourceId: string, scopes: string, purpose: string, expiresAt: string, revokedAt: string | null, revokedBy: string | null, createdByPrincipalId: string, accessCount: number, lastAccessedAt: string | null, lastModified: string, token: string | null } };

export type RevokePublicLinkGrantMutationVariables = Exact<{
  publicLinkGrantId: string;
  revokedBy: string;
}>;


export type RevokePublicLinkGrantMutation = { revokePublicLinkGrant: boolean };

export type ReportFieldsFragment = { reportId: string, code: string, description: string | null, type: ReportType, typeId: number, active: boolean, category: string, requiredFeatureKey: string | null, managerOnly: boolean, supportsPdf: boolean, sortOrder: number, filters: string | null };

export type GetReportsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetReportsQuery = { reports: Array<{ reportId: string, code: string, description: string | null, type: ReportType, typeId: number, active: boolean, category: string, requiredFeatureKey: string | null, managerOnly: boolean, supportsPdf: boolean, sortOrder: number, filters: string | null }> };

export type UpdateReportMutationVariables = Exact<{
  id: string;
  report: UpdateReportDtoInput;
}>;


export type UpdateReportMutation = { updateReport: boolean };

export type AccountSettingsItemFragment = { accountId: string, maps: string, mapsKey: string | null, onlineInterval: number, refreshMap: boolean, refreshMapInterval: number };

export type UserSettingsItemFragment = { userId: string, style: string, language: string, navbar: string };

export type GetAccountSettingsByUserQueryVariables = Exact<{ [key: string]: never; }>;


export type GetAccountSettingsByUserQuery = { accountSettingsByUser: { accountId: string, maps: string, mapsKey: string | null, onlineInterval: number, refreshMap: boolean, refreshMapInterval: number } };

export type GetUserSettingsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetUserSettingsQuery = { userSettings: { userId: string, style: string, language: string, navbar: string } };

export type UpdateAccountSettingsMutationVariables = Exact<{
  id: string;
  accountSettings: AccountSettingsDtoInput;
}>;


export type UpdateAccountSettingsMutation = { updateAccountSettings: boolean };

export type UpdateUserSettingsMutationVariables = Exact<{
  id: string;
  userSettings: UserSettingsDtoInput;
}>;


export type UpdateUserSettingsMutation = { updateUserSettings: boolean };

export type AccountSupportGrantItemFragment = { accountSupportGrantId: string, accountId: string, supportUserId: string, reason: string, ticketReference: string, approvedBy: string | null, approvedAt: string | null, accessLevel: string, startsAt: string, endsAt: string, revokedAt: string | null, revokedBy: string | null, lastModified: string };

export type GetAccountSupportGrantsQueryVariables = Exact<{
  accountId?: string | null | undefined;
  skip: number;
  take: number;
}>;


export type GetAccountSupportGrantsQuery = { accountSupportGrants: Array<{ accountSupportGrantId: string, accountId: string, supportUserId: string, reason: string, ticketReference: string, approvedBy: string | null, approvedAt: string | null, accessLevel: string, startsAt: string, endsAt: string, revokedAt: string | null, revokedBy: string | null, lastModified: string }> };

export type CreateAccountSupportGrantMutationVariables = Exact<{
  accountSupportGrant: AccountSupportGrantDtoInput;
}>;


export type CreateAccountSupportGrantMutation = { createAccountSupportGrant: { accountSupportGrantId: string, accountId: string, supportUserId: string, reason: string, ticketReference: string, approvedBy: string | null, approvedAt: string | null, accessLevel: string, startsAt: string, endsAt: string, revokedAt: string | null, revokedBy: string | null, lastModified: string } };

export type ApproveAccountSupportGrantMutationVariables = Exact<{
  accountSupportGrantId: string;
  approvedBy: string;
}>;


export type ApproveAccountSupportGrantMutation = { approveAccountSupportGrant: boolean };

export type RevokeAccountSupportGrantMutationVariables = Exact<{
  accountSupportGrantId: string;
  revokedBy: string;
}>;


export type RevokeAccountSupportGrantMutation = { revokeAccountSupportGrant: boolean };

export type TransporterItemFragment = { transporterId: string, name: string, transporterType: TransporterType, transporterTypeId: number };

export type GetTransportersByAccountQueryVariables = Exact<{
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetTransportersByAccountQuery = { transportersByAccount: { totalCount: number, items: Array<{ transporterId: string, name: string, transporterType: TransporterType, transporterTypeId: number }> } };

export type GetTransportersByGroupQueryVariables = Exact<{
  groupId: number;
  skip?: number | null | undefined;
  take?: number | null | undefined;
  search?: string | null | undefined;
}>;


export type GetTransportersByGroupQuery = { transportersByGroup: { totalCount: number, items: Array<{ transporterId: string, name: string, transporterType: TransporterType, transporterTypeId: number }> } };

export type GetTransporterLookupByAccountQueryVariables = Exact<{ [key: string]: never; }>;


export type GetTransporterLookupByAccountQuery = { transporterLookupByAccount: Array<{ transporterId: string, name: string, transporterType: TransporterType, transporterTypeId: number }> };

export type GetTransporterLookupByUserQueryVariables = Exact<{ [key: string]: never; }>;


export type GetTransporterLookupByUserQuery = { transporterLookupByUser: Array<{ transporterId: string, name: string, transporterType: TransporterType, transporterTypeId: number }> };

export type CreateTransporterMutationVariables = Exact<{
  transporter: TransporterDtoInput;
}>;


export type CreateTransporterMutation = { createTransporter: { transporterId: string, name: string, transporterType: TransporterType, transporterTypeId: number } };

export type UpdateTransporterMutationVariables = Exact<{
  id: string;
  transporter: UpdateTransporterDtoInput;
}>;


export type UpdateTransporterMutation = { updateTransporter: boolean };

export type DeleteTransporterMutationVariables = Exact<{
  id: string;
}>;


export type DeleteTransporterMutation = { deleteTransporter: string };

export type AssignmentFieldsFragment = { transporterDeviceAssignmentId: string, accountId: string, transporterId: string, deviceId: string, effectiveFrom: string, effectiveTo: string | null, priority: number, isPrimary: boolean, status: AssignmentStatus, assignmentReason: string | null };

export type GetTransporterDeviceAssignmentsByAccountQueryVariables = Exact<{
  accountId: string;
  activeOnly: boolean;
  skip?: number | null | undefined;
  take?: number | null | undefined;
}>;


export type GetTransporterDeviceAssignmentsByAccountQuery = { transporterDeviceAssignmentsByAccount: { totalCount: number, items: Array<{ createdByPrincipalType: string, createdByPrincipalId: string, transporterDeviceAssignmentId: string, accountId: string, transporterId: string, deviceId: string, effectiveFrom: string, effectiveTo: string | null, priority: number, isPrimary: boolean, status: AssignmentStatus, assignmentReason: string | null }> } };

export type AssignDeviceToTransporterMutationVariables = Exact<{
  assignment: TransporterDeviceAssignmentDtoInput;
}>;


export type AssignDeviceToTransporterMutation = { assignDeviceToTransporter: { transporterDeviceAssignmentId: string, deviceId: string, transporterId: string, effectiveFrom: string, isPrimary: boolean, status: AssignmentStatus } };

export type EndDeviceTransporterAssignmentMutationVariables = Exact<{
  assignmentId: string;
  reason?: string | null | undefined;
}>;


export type EndDeviceTransporterAssignmentMutation = { endDeviceTransporterAssignment: boolean };

export type TransporterTypeItemFragment = { transporterTypeId: number, accBased: boolean, stoppedGap: number, maxDistance: number, maxTimeGap: number, type: TransporterType };

export type GetTransporterTypesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetTransporterTypesQuery = { transporterTypes: Array<{ transporterTypeId: number, accBased: boolean, stoppedGap: number, maxDistance: number, maxTimeGap: number, type: TransporterType }> };

export type UpdateTransporterTypeMutationVariables = Exact<{
  id: number;
  transporterType: TransporterTypeDtoInput;
}>;


export type UpdateTransporterTypeMutation = { updateTransporterType: boolean };

export const AccountFeatureItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountFeatureItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountFeatureVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountFeatureId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"featureKey"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"tier"}},{"kind":"Field","name":{"kind":"Name","value":"source"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<AccountFeatureItemFragment, unknown>;
export const AccountItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"typeId"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"statusId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<AccountItemFragment, unknown>;
export const AlertEventItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlertEventItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AlertEventVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"alertEventId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"eventType"}},{"kind":"Field","name":{"kind":"Name","value":"severity"}},{"kind":"Field","name":{"kind":"Name","value":"sourceModule"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"firstSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"deduplicationKey"}}]}}]} as unknown as DocumentNode<AlertEventItemFragment, unknown>;
export const AlertSubscriptionItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlertSubscriptionItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AlertSubscriptionVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"alertSubscriptionId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"principalType"}},{"kind":"Field","name":{"kind":"Name","value":"principalId"}},{"kind":"Field","name":{"kind":"Name","value":"eventTypeFilter"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"contact"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<AlertSubscriptionItemFragment, unknown>;
export const AuditEventItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AuditEventItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AuditEventVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"auditEventId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"actorType"}},{"kind":"Field","name":{"kind":"Name","value":"actorId"}},{"kind":"Field","name":{"kind":"Name","value":"action"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"result"}},{"kind":"Field","name":{"kind":"Name","value":"reason"}},{"kind":"Field","name":{"kind":"Name","value":"correlationId"}},{"kind":"Field","name":{"kind":"Name","value":"occurredAt"}}]}}]} as unknown as DocumentNode<AuditEventItemFragment, unknown>;
export const BackgroundJobRunItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"BackgroundJobRunItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"BackgroundJobRunVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgroundJobRunId"}},{"kind":"Field","name":{"kind":"Name","value":"jobKey"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceKey"}},{"kind":"Field","name":{"kind":"Name","value":"idempotencyKey"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"attempts"}},{"kind":"Field","name":{"kind":"Name","value":"startedAt"}},{"kind":"Field","name":{"kind":"Name","value":"completedAt"}},{"kind":"Field","name":{"kind":"Name","value":"errorCode"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}}]}}]} as unknown as DocumentNode<BackgroundJobRunItemFragment, unknown>;
export const AccountBrandingItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountBrandingItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountBrandingVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"displayName"}},{"kind":"Field","name":{"kind":"Name","value":"logoDocumentId"}},{"kind":"Field","name":{"kind":"Name","value":"primaryColor"}},{"kind":"Field","name":{"kind":"Name","value":"reportHeader"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<AccountBrandingItemFragment, unknown>;
export const CredentialFieldsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"CredentialFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CredentialVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"credentialId"}},{"kind":"Field","name":{"kind":"Name","value":"key"}},{"kind":"Field","name":{"kind":"Name","value":"key2"}},{"kind":"Field","name":{"kind":"Name","value":"password"}},{"kind":"Field","name":{"kind":"Name","value":"uri"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]} as unknown as DocumentNode<CredentialFieldsFragment, unknown>;
export const DeviceItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DeviceItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeviceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"serial"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"deviceType"}},{"kind":"Field","name":{"kind":"Name","value":"deviceTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"identifier"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}}]}}]} as unknown as DocumentNode<DeviceItemFragment, unknown>;
export const SynchronizedDeviceFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SynchronizedDevice"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeviceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"serial"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"identifier"}},{"kind":"Field","name":{"kind":"Name","value":"providerDisplayName"}},{"kind":"Field","name":{"kind":"Name","value":"providerStatus"}},{"kind":"Field","name":{"kind":"Name","value":"detectedStatus"}},{"kind":"Field","name":{"kind":"Name","value":"firstSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSyncedAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastAssignedAt"}},{"kind":"Field","name":{"kind":"Name","value":"ignoredAt"}}]}}]} as unknown as DocumentNode<SynchronizedDeviceFragment, unknown>;
export const DocumentFieldsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityType"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityId"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"fileName"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"contentType"}},{"kind":"Field","name":{"kind":"Name","value":"sizeBytes"}},{"kind":"Field","name":{"kind":"Name","value":"sha256Hash"}},{"kind":"Field","name":{"kind":"Name","value":"classification"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"visibilityScope"}},{"kind":"Field","name":{"kind":"Name","value":"scanStatus"}},{"kind":"Field","name":{"kind":"Name","value":"currentVersion"}},{"kind":"Field","name":{"kind":"Name","value":"downloadUrl"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<DocumentFieldsFragment, unknown>;
export const DocumentVersionFieldsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentVersionFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentVersionVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentVersionId"}},{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"versionNumber"}},{"kind":"Field","name":{"kind":"Name","value":"contentType"}},{"kind":"Field","name":{"kind":"Name","value":"fileName"}},{"kind":"Field","name":{"kind":"Name","value":"sizeBytes"}},{"kind":"Field","name":{"kind":"Name","value":"sha256Hash"}},{"kind":"Field","name":{"kind":"Name","value":"scanStatus"}},{"kind":"Field","name":{"kind":"Name","value":"replacedByPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"replacedByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"reason"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]} as unknown as DocumentNode<DocumentVersionFieldsFragment, unknown>;
export const DocumentTypeFieldsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentTypeFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentTypeVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"displayName"}},{"kind":"Field","name":{"kind":"Name","value":"required"}},{"kind":"Field","name":{"kind":"Name","value":"expiring"}},{"kind":"Field","name":{"kind":"Name","value":"defaultValidityDays"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]} as unknown as DocumentNode<DocumentTypeFieldsFragment, unknown>;
export const DocumentSignatureFieldsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentSignatureFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentSignatureVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentSignatureId"}},{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"signerPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"signerPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"signerName"}},{"kind":"Field","name":{"kind":"Name","value":"signatureImageDocumentId"}},{"kind":"Field","name":{"kind":"Name","value":"legalTextAccepted"}},{"kind":"Field","name":{"kind":"Name","value":"latitude"}},{"kind":"Field","name":{"kind":"Name","value":"longitude"}},{"kind":"Field","name":{"kind":"Name","value":"signedAt"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]} as unknown as DocumentNode<DocumentSignatureFieldsFragment, unknown>;
export const DriverItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"phone"}},{"kind":"Field","name":{"kind":"Name","value":"documentType"}},{"kind":"Field","name":{"kind":"Name","value":"documentNumber"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"employeeCode"}},{"kind":"Field","name":{"kind":"Name","value":"licenseNumber"}},{"kind":"Field","name":{"kind":"Name","value":"licenseExpiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"defaultTransporterId"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<DriverItemFragment, unknown>;
export const DriverQualificationItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverQualificationItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverQualificationVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverQualificationId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"qualificationType"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"number"}},{"kind":"Field","name":{"kind":"Name","value":"issuedAt"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"issuingAuthority"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"notes"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<DriverQualificationItemFragment, unknown>;
export const DriverTransporterAssignmentItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverTransporterAssignmentItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverTransporterAssignmentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverTransporterAssignmentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"transporterName"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"assignmentType"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"createdByPrincipal"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<DriverTransporterAssignmentItemFragment, unknown>;
export const DriverActiveAssignmentItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverActiveAssignmentItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverAssignmentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"assignmentType"}}]}}]} as unknown as DocumentNode<DriverActiveAssignmentItemFragment, unknown>;
export const GeocodingProviderItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"GeocodingProviderItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"GeocodingProviderVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"geocodingProviderId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"endpointUri"}},{"kind":"Field","name":{"kind":"Name","value":"apiKey"}},{"kind":"Field","name":{"kind":"Name","value":"requestsPerSecond"}},{"kind":"Field","name":{"kind":"Name","value":"timeoutSeconds"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"active"}}]}}]} as unknown as DocumentNode<GeocodingProviderItemFragment, unknown>;
export const GroupItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"GroupItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"GroupVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"groupId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}}]}}]} as unknown as DocumentNode<GroupItemFragment, unknown>;
export const NotificationDeliveryItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"NotificationDeliveryItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationDeliveryVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationDeliveryId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"notificationRuleId"}},{"kind":"Field","name":{"kind":"Name","value":"alertEventId"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"recipientPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"recipient"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"attempts"}},{"kind":"Field","name":{"kind":"Name","value":"providerMessageId"}},{"kind":"Field","name":{"kind":"Name","value":"error"}},{"kind":"Field","name":{"kind":"Name","value":"sentAt"}},{"kind":"Field","name":{"kind":"Name","value":"readAt"}}]}}]} as unknown as DocumentNode<NotificationDeliveryItemFragment, unknown>;
export const MyNotificationItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MyNotificationItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MyNotificationVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationDeliveryId"}},{"kind":"Field","name":{"kind":"Name","value":"alertEventId"}},{"kind":"Field","name":{"kind":"Name","value":"eventType"}},{"kind":"Field","name":{"kind":"Name","value":"severity"}},{"kind":"Field","name":{"kind":"Name","value":"sourceModule"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"payloadJson"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"readAt"}}]}}]} as unknown as DocumentNode<MyNotificationItemFragment, unknown>;
export const NotificationRuleItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"NotificationRuleItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationRuleVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationRuleId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"ruleKey"}},{"kind":"Field","name":{"kind":"Name","value":"ruleType"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"triggerEvent"}},{"kind":"Field","name":{"kind":"Name","value":"recipientSelector"}},{"kind":"Field","name":{"kind":"Name","value":"channelsJson"}},{"kind":"Field","name":{"kind":"Name","value":"throttlingJson"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<NotificationRuleItemFragment, unknown>;
export const NotificationTemplateItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"NotificationTemplateItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationTemplateVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationTemplateId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"templateKey"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"locale"}},{"kind":"Field","name":{"kind":"Name","value":"subject"}},{"kind":"Field","name":{"kind":"Name","value":"body"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<NotificationTemplateItemFragment, unknown>;
export const OperatorDetailFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"OperatorDetail"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"OperatorVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"address"}},{"kind":"Field","name":{"kind":"Name","value":"contactName"}},{"kind":"Field","name":{"kind":"Name","value":"protocolType"}},{"kind":"Field","name":{"kind":"Name","value":"protocolTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"syncIntervalMinutes"}},{"kind":"Field","name":{"kind":"Name","value":"healthStatus"}},{"kind":"Field","name":{"kind":"Name","value":"lastSuccessfulSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailedSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailureCode"}},{"kind":"Field","name":{"kind":"Name","value":"lastLatencyMs"}},{"kind":"Field","name":{"kind":"Name","value":"lastDeviceSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastPositionSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<OperatorDetailFragment, unknown>;
export const OperatorGpsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"OperatorGps"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"OperatorVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"protocolType"}},{"kind":"Field","name":{"kind":"Name","value":"protocolTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"lastDeviceSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastPositionSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"syncIntervalMinutes"}}]}}]} as unknown as DocumentNode<OperatorGpsFragment, unknown>;
export const PlatformAnnouncementItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlatformAnnouncementItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PlatformAnnouncementVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"platformAnnouncementId"}},{"kind":"Field","name":{"kind":"Name","value":"messageEn"}},{"kind":"Field","name":{"kind":"Name","value":"messageEs"}},{"kind":"Field","name":{"kind":"Name","value":"severity"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<PlatformAnnouncementItemFragment, unknown>;
export const PointOfInterestItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PointOfInterestItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PointOfInterestVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"pointOfInterestId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"latitude"}},{"kind":"Field","name":{"kind":"Name","value":"longitude"}},{"kind":"Field","name":{"kind":"Name","value":"address"}},{"kind":"Field","name":{"kind":"Name","value":"color"}},{"kind":"Field","name":{"kind":"Name","value":"groupId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}}]}}]} as unknown as DocumentNode<PointOfInterestItemFragment, unknown>;
export const CurrentPrincipalItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"CurrentPrincipalItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CurrentPrincipalVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"subjectId"}},{"kind":"Field","name":{"kind":"Name","value":"principalType"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"clientId"}},{"kind":"Field","name":{"kind":"Name","value":"publicLinkGrantId"}},{"kind":"Field","name":{"kind":"Name","value":"role"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"scopes"}},{"kind":"Field","name":{"kind":"Name","value":"correlationId"}}]}}]} as unknown as DocumentNode<CurrentPrincipalItemFragment, unknown>;
export const PublicLinkGrantFieldsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PublicLinkGrantFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PublicLinkGrantVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"publicLinkGrantId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"scopes"}},{"kind":"Field","name":{"kind":"Name","value":"purpose"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedBy"}},{"kind":"Field","name":{"kind":"Name","value":"createdByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"accessCount"}},{"kind":"Field","name":{"kind":"Name","value":"lastAccessedAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}},{"kind":"Field","name":{"kind":"Name","value":"token"}}]}}]} as unknown as DocumentNode<PublicLinkGrantFieldsFragment, unknown>;
export const ReportFieldsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReportFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReportVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"reportId"}},{"kind":"Field","name":{"kind":"Name","value":"code"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"typeId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"requiredFeatureKey"}},{"kind":"Field","name":{"kind":"Name","value":"managerOnly"}},{"kind":"Field","name":{"kind":"Name","value":"supportsPdf"}},{"kind":"Field","name":{"kind":"Name","value":"sortOrder"}},{"kind":"Field","name":{"kind":"Name","value":"filters"}}]}}]} as unknown as DocumentNode<ReportFieldsFragment, unknown>;
export const AccountSettingsItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountSettingsItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountSettingsVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"maps"}},{"kind":"Field","name":{"kind":"Name","value":"mapsKey"}},{"kind":"Field","name":{"kind":"Name","value":"onlineInterval"}},{"kind":"Field","name":{"kind":"Name","value":"refreshMap"}},{"kind":"Field","name":{"kind":"Name","value":"refreshMapInterval"}}]}}]} as unknown as DocumentNode<AccountSettingsItemFragment, unknown>;
export const UserSettingsItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserSettingsItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UserSettingsVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"style"}},{"kind":"Field","name":{"kind":"Name","value":"language"}},{"kind":"Field","name":{"kind":"Name","value":"navbar"}}]}}]} as unknown as DocumentNode<UserSettingsItemFragment, unknown>;
export const AccountSupportGrantItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountSupportGrantItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountSupportGrantVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountSupportGrantId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"supportUserId"}},{"kind":"Field","name":{"kind":"Name","value":"reason"}},{"kind":"Field","name":{"kind":"Name","value":"ticketReference"}},{"kind":"Field","name":{"kind":"Name","value":"approvedBy"}},{"kind":"Field","name":{"kind":"Name","value":"approvedAt"}},{"kind":"Field","name":{"kind":"Name","value":"accessLevel"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedBy"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<AccountSupportGrantItemFragment, unknown>;
export const TransporterItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TransporterItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"transporterType"}},{"kind":"Field","name":{"kind":"Name","value":"transporterTypeId"}}]}}]} as unknown as DocumentNode<TransporterItemFragment, unknown>;
export const AssignmentFieldsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AssignmentFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterDeviceAssignmentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterDeviceAssignmentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"priority"}},{"kind":"Field","name":{"kind":"Name","value":"isPrimary"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"assignmentReason"}}]}}]} as unknown as DocumentNode<AssignmentFieldsFragment, unknown>;
export const TransporterTypeItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TransporterTypeItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterTypeVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"accBased"}},{"kind":"Field","name":{"kind":"Name","value":"stoppedGap"}},{"kind":"Field","name":{"kind":"Name","value":"maxDistance"}},{"kind":"Field","name":{"kind":"Name","value":"maxTimeGap"}},{"kind":"Field","name":{"kind":"Name","value":"type"}}]}}]} as unknown as DocumentNode<TransporterTypeItemFragment, unknown>;
export const GetAccountFeaturesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAccountFeatures"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountFeatures"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountFeatureItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountFeatureItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountFeatureVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountFeatureId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"featureKey"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"tier"}},{"kind":"Field","name":{"kind":"Name","value":"source"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetAccountFeaturesQuery, GetAccountFeaturesQueryVariables>;
export const SetAccountFeatureDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SetAccountFeature"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"feature"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AccountFeatureDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setAccountFeature"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"feature"},"value":{"kind":"Variable","name":{"kind":"Name","value":"feature"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountFeatureItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountFeatureItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountFeatureVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountFeatureId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"featureKey"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"tier"}},{"kind":"Field","name":{"kind":"Name","value":"source"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<SetAccountFeatureMutation, SetAccountFeatureMutationVariables>;
export const GetAccountFeaturesMasterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAccountFeaturesMaster"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountFeaturesMaster"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountFeatureItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountFeatureItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountFeatureVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountFeatureId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"featureKey"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"tier"}},{"kind":"Field","name":{"kind":"Name","value":"source"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetAccountFeaturesMasterQuery, GetAccountFeaturesMasterQueryVariables>;
export const SetAccountFeatureMasterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SetAccountFeatureMaster"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"feature"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AccountFeatureDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setAccountFeatureMaster"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"feature"},"value":{"kind":"Variable","name":{"kind":"Name","value":"feature"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountFeatureItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountFeatureItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountFeatureVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountFeatureId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"featureKey"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"tier"}},{"kind":"Field","name":{"kind":"Name","value":"source"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<SetAccountFeatureMasterMutation, SetAccountFeatureMasterMutationVariables>;
export const GetAccountByUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAccountByUser"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountByUser"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"typeId"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"statusId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetAccountByUserQuery, GetAccountByUserQueryVariables>;
export const GetAccountsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAccounts"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accounts"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountItem"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"typeId"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"statusId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetAccountsQuery, GetAccountsQueryVariables>;
export const CreateAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"account"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AccountDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"account"},"value":{"kind":"Variable","name":{"kind":"Name","value":"account"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"typeId"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"statusId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<CreateAccountMutation, CreateAccountMutationVariables>;
export const UpdateAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"account"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateAccountDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"account"},"value":{"kind":"Variable","name":{"kind":"Name","value":"account"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateAccountMutation, UpdateAccountMutationVariables>;
export const UpdateAccountMasterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateAccountMaster"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"account"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateAccountDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateAccountMaster"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"account"},"value":{"kind":"Variable","name":{"kind":"Name","value":"account"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateAccountMasterMutation, UpdateAccountMasterMutationVariables>;
export const ChangeAccountStatusDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ChangeAccountStatus"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"targetStatus"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AccountStatus"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"reason"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"changeAccountStatus"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"targetStatus"},"value":{"kind":"Variable","name":{"kind":"Name","value":"targetStatus"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"reason"},"value":{"kind":"Variable","name":{"kind":"Name","value":"reason"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"typeId"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"statusId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<ChangeAccountStatusMutation, ChangeAccountStatusMutationVariables>;
export const GetAccountContextDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAccountContext"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountContext"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"statusId"}},{"kind":"Field","name":{"kind":"Name","value":"branding"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"displayName"}},{"kind":"Field","name":{"kind":"Name","value":"logoDocumentId"}},{"kind":"Field","name":{"kind":"Name","value":"primaryColor"}},{"kind":"Field","name":{"kind":"Name","value":"reportHeader"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}},{"kind":"Field","name":{"kind":"Name","value":"features"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountFeatureId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"featureKey"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"tier"}},{"kind":"Field","name":{"kind":"Name","value":"source"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]}}]}}]} as unknown as DocumentNode<GetAccountContextQuery, GetAccountContextQueryVariables>;
export const GetAlertEventsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAlertEvents"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"alertEvents"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlertEventItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlertEventItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AlertEventVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"alertEventId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"eventType"}},{"kind":"Field","name":{"kind":"Name","value":"severity"}},{"kind":"Field","name":{"kind":"Name","value":"sourceModule"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"firstSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"deduplicationKey"}}]}}]} as unknown as DocumentNode<GetAlertEventsQuery, GetAlertEventsQueryVariables>;
export const AcknowledgeAlertEventDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AcknowledgeAlertEvent"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"alertEventId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"acknowledgeAlertEvent"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"alertEventId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"alertEventId"}}}]}}]}]}}]} as unknown as DocumentNode<AcknowledgeAlertEventMutation, AcknowledgeAlertEventMutationVariables>;
export const ResolveAlertEventDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ResolveAlertEvent"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"alertEventId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resolveAlertEvent"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"alertEventId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"alertEventId"}}}]}}]}]}}]} as unknown as DocumentNode<ResolveAlertEventMutation, ResolveAlertEventMutationVariables>;
export const GetAlertSubscriptionsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAlertSubscriptions"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"principalId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"alertSubscriptions"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"principalId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"principalId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlertSubscriptionItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlertSubscriptionItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AlertSubscriptionVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"alertSubscriptionId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"principalType"}},{"kind":"Field","name":{"kind":"Name","value":"principalId"}},{"kind":"Field","name":{"kind":"Name","value":"eventTypeFilter"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"contact"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetAlertSubscriptionsQuery, GetAlertSubscriptionsQueryVariables>;
export const CreateAlertSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateAlertSubscription"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"subscription"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AlertSubscriptionDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createAlertSubscription"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"subscription"},"value":{"kind":"Variable","name":{"kind":"Name","value":"subscription"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"alertSubscriptionId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"principalType"}},{"kind":"Field","name":{"kind":"Name","value":"principalId"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}}]}}]}}]} as unknown as DocumentNode<CreateAlertSubscriptionMutation, CreateAlertSubscriptionMutationVariables>;
export const UpdateAlertSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateAlertSubscription"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"alertSubscriptionId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"subscription"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AlertSubscriptionDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateAlertSubscription"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"alertSubscriptionId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"alertSubscriptionId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"subscription"},"value":{"kind":"Variable","name":{"kind":"Name","value":"subscription"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateAlertSubscriptionMutation, UpdateAlertSubscriptionMutationVariables>;
export const DeleteAlertSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteAlertSubscription"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"alertSubscriptionId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteAlertSubscription"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"alertSubscriptionId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"alertSubscriptionId"}}}]}}]}]}}]} as unknown as DocumentNode<DeleteAlertSubscriptionMutation, DeleteAlertSubscriptionMutationVariables>;
export const GetAuditTrailDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAuditTrail"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"auditTrail"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AuditEventItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AuditEventItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AuditEventVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"auditEventId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"actorType"}},{"kind":"Field","name":{"kind":"Name","value":"actorId"}},{"kind":"Field","name":{"kind":"Name","value":"action"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"result"}},{"kind":"Field","name":{"kind":"Name","value":"reason"}},{"kind":"Field","name":{"kind":"Name","value":"correlationId"}},{"kind":"Field","name":{"kind":"Name","value":"occurredAt"}}]}}]} as unknown as DocumentNode<GetAuditTrailQuery, GetAuditTrailQueryVariables>;
export const GetBackgroundJobRunsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetBackgroundJobRuns"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgroundJobRuns"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"BackgroundJobRunItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"BackgroundJobRunItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"BackgroundJobRunVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgroundJobRunId"}},{"kind":"Field","name":{"kind":"Name","value":"jobKey"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceKey"}},{"kind":"Field","name":{"kind":"Name","value":"idempotencyKey"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"attempts"}},{"kind":"Field","name":{"kind":"Name","value":"startedAt"}},{"kind":"Field","name":{"kind":"Name","value":"completedAt"}},{"kind":"Field","name":{"kind":"Name","value":"errorCode"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}}]}}]} as unknown as DocumentNode<GetBackgroundJobRunsQuery, GetBackgroundJobRunsQueryVariables>;
export const GetAccountBrandingDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAccountBranding"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountBranding"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountBrandingItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountBrandingItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountBrandingVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"displayName"}},{"kind":"Field","name":{"kind":"Name","value":"logoDocumentId"}},{"kind":"Field","name":{"kind":"Name","value":"primaryColor"}},{"kind":"Field","name":{"kind":"Name","value":"reportHeader"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetAccountBrandingQuery, GetAccountBrandingQueryVariables>;
export const UpdateAccountBrandingDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateAccountBranding"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"branding"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AccountBrandingDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateAccountBranding"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"branding"},"value":{"kind":"Variable","name":{"kind":"Name","value":"branding"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountBrandingItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountBrandingItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountBrandingVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"displayName"}},{"kind":"Field","name":{"kind":"Name","value":"logoDocumentId"}},{"kind":"Field","name":{"kind":"Name","value":"primaryColor"}},{"kind":"Field","name":{"kind":"Name","value":"reportHeader"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<UpdateAccountBrandingMutation, UpdateAccountBrandingMutationVariables>;
export const GetCredentialByOperatorDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetCredentialByOperator"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"credentialByOperator"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"operatorId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"CredentialFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"CredentialFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CredentialVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"credentialId"}},{"kind":"Field","name":{"kind":"Name","value":"key"}},{"kind":"Field","name":{"kind":"Name","value":"key2"}},{"kind":"Field","name":{"kind":"Name","value":"password"}},{"kind":"Field","name":{"kind":"Name","value":"uri"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]} as unknown as DocumentNode<GetCredentialByOperatorQuery, GetCredentialByOperatorQueryVariables>;
export const CreateCredentialDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateCredential"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"credential"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CredentialDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createCredential"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"credential"},"value":{"kind":"Variable","name":{"kind":"Name","value":"credential"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"CredentialFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"CredentialFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CredentialVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"credentialId"}},{"kind":"Field","name":{"kind":"Name","value":"key"}},{"kind":"Field","name":{"kind":"Name","value":"key2"}},{"kind":"Field","name":{"kind":"Name","value":"password"}},{"kind":"Field","name":{"kind":"Name","value":"uri"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]} as unknown as DocumentNode<CreateCredentialMutation, CreateCredentialMutationVariables>;
export const UpdateCredentialDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateCredential"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"credential"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateCredentialDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateCredential"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"credential"},"value":{"kind":"Variable","name":{"kind":"Name","value":"credential"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateCredentialMutation, UpdateCredentialMutationVariables>;
export const GetDevicesByAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDevicesByAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"devicesByAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DeviceItem"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DeviceItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeviceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"serial"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"deviceType"}},{"kind":"Field","name":{"kind":"Name","value":"deviceTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"identifier"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}}]}}]} as unknown as DocumentNode<GetDevicesByAccountQuery, GetDevicesByAccountQueryVariables>;
export const GetDeviceLookupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDeviceLookup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deviceLookup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}}]}}]}}]} as unknown as DocumentNode<GetDeviceLookupQuery, GetDeviceLookupQueryVariables>;
export const DeleteDeviceDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteDevice"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"deviceId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteDevice"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"deviceId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"deviceId"}}}]}]}}]} as unknown as DocumentNode<DeleteDeviceMutation, DeleteDeviceMutationVariables>;
export const GetSynchronizedDevicesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetSynchronizedDevices"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"detectedStatus"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"DetectedStatus"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"unassignedOnly"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Boolean"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"recentOnly"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Boolean"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"synchronizedDevices"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"detectedStatus"},"value":{"kind":"Variable","name":{"kind":"Name","value":"detectedStatus"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"operatorId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"unassignedOnly"},"value":{"kind":"Variable","name":{"kind":"Name","value":"unassignedOnly"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"recentOnly"},"value":{"kind":"Variable","name":{"kind":"Name","value":"recentOnly"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SynchronizedDevice"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SynchronizedDevice"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeviceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"serial"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"identifier"}},{"kind":"Field","name":{"kind":"Name","value":"providerDisplayName"}},{"kind":"Field","name":{"kind":"Name","value":"providerStatus"}},{"kind":"Field","name":{"kind":"Name","value":"detectedStatus"}},{"kind":"Field","name":{"kind":"Name","value":"firstSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSyncedAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastAssignedAt"}},{"kind":"Field","name":{"kind":"Name","value":"ignoredAt"}}]}}]} as unknown as DocumentNode<GetSynchronizedDevicesQuery, GetSynchronizedDevicesQueryVariables>;
export const GetUnassignedSynchronizedDevicesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUnassignedSynchronizedDevices"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"unassignedSynchronizedDevices"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SynchronizedDevice"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SynchronizedDevice"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeviceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"serial"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"identifier"}},{"kind":"Field","name":{"kind":"Name","value":"providerDisplayName"}},{"kind":"Field","name":{"kind":"Name","value":"providerStatus"}},{"kind":"Field","name":{"kind":"Name","value":"detectedStatus"}},{"kind":"Field","name":{"kind":"Name","value":"firstSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSyncedAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastAssignedAt"}},{"kind":"Field","name":{"kind":"Name","value":"ignoredAt"}}]}}]} as unknown as DocumentNode<GetUnassignedSynchronizedDevicesQuery, GetUnassignedSynchronizedDevicesQueryVariables>;
export const SetSynchronizedDeviceIgnoredDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SetSynchronizedDeviceIgnored"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"deviceId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"ignored"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Boolean"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setSynchronizedDeviceIgnored"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"deviceId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"deviceId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"ignored"},"value":{"kind":"Variable","name":{"kind":"Name","value":"ignored"}}}]}}]}]}}]} as unknown as DocumentNode<SetSynchronizedDeviceIgnoredMutation, SetSynchronizedDeviceIgnoredMutationVariables>;
export const RegisterManualDeviceDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RegisterManualDevice"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"device"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DeviceDtoInput"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"autoAssign"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Boolean"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"registerManualDevice"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"device"},"value":{"kind":"Variable","name":{"kind":"Name","value":"device"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"autoAssign"},"value":{"kind":"Variable","name":{"kind":"Name","value":"autoAssign"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SynchronizedDevice"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SynchronizedDevice"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeviceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"serial"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"identifier"}},{"kind":"Field","name":{"kind":"Name","value":"providerDisplayName"}},{"kind":"Field","name":{"kind":"Name","value":"providerStatus"}},{"kind":"Field","name":{"kind":"Name","value":"detectedStatus"}},{"kind":"Field","name":{"kind":"Name","value":"firstSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSeenAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastSyncedAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastAssignedAt"}},{"kind":"Field","name":{"kind":"Name","value":"ignoredAt"}}]}}]} as unknown as DocumentNode<RegisterManualDeviceMutation, RegisterManualDeviceMutationVariables>;
export const GetDocumentsForOwnerDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDocumentsForOwner"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"ownerEntityType"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"ownerEntityId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentsForOwner"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"ownerEntityType"},"value":{"kind":"Variable","name":{"kind":"Name","value":"ownerEntityType"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"ownerEntityId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"ownerEntityId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DocumentFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityType"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityId"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"fileName"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"contentType"}},{"kind":"Field","name":{"kind":"Name","value":"sizeBytes"}},{"kind":"Field","name":{"kind":"Name","value":"sha256Hash"}},{"kind":"Field","name":{"kind":"Name","value":"classification"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"visibilityScope"}},{"kind":"Field","name":{"kind":"Name","value":"scanStatus"}},{"kind":"Field","name":{"kind":"Name","value":"currentVersion"}},{"kind":"Field","name":{"kind":"Name","value":"downloadUrl"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetDocumentsForOwnerQuery, GetDocumentsForOwnerQueryVariables>;
export const GetDocumentDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDocument"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"documentId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"document"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"documentId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"documentId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DocumentFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityType"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityId"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"fileName"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"contentType"}},{"kind":"Field","name":{"kind":"Name","value":"sizeBytes"}},{"kind":"Field","name":{"kind":"Name","value":"sha256Hash"}},{"kind":"Field","name":{"kind":"Name","value":"classification"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"visibilityScope"}},{"kind":"Field","name":{"kind":"Name","value":"scanStatus"}},{"kind":"Field","name":{"kind":"Name","value":"currentVersion"}},{"kind":"Field","name":{"kind":"Name","value":"downloadUrl"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetDocumentQuery, GetDocumentQueryVariables>;
export const SearchDocumentsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchDocuments"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"filter"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentSearchFilterInput"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchDocuments"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"filter"},"value":{"kind":"Variable","name":{"kind":"Name","value":"filter"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DocumentFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityType"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityId"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"fileName"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"contentType"}},{"kind":"Field","name":{"kind":"Name","value":"sizeBytes"}},{"kind":"Field","name":{"kind":"Name","value":"sha256Hash"}},{"kind":"Field","name":{"kind":"Name","value":"classification"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"visibilityScope"}},{"kind":"Field","name":{"kind":"Name","value":"scanStatus"}},{"kind":"Field","name":{"kind":"Name","value":"currentVersion"}},{"kind":"Field","name":{"kind":"Name","value":"downloadUrl"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<SearchDocumentsQuery, SearchDocumentsQueryVariables>;
export const GetExpiringDocumentsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetExpiringDocuments"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"withinDays"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"expiringDocuments"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"withinDays"},"value":{"kind":"Variable","name":{"kind":"Name","value":"withinDays"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DocumentFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityType"}},{"kind":"Field","name":{"kind":"Name","value":"ownerEntityId"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"uploadedByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"fileName"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"contentType"}},{"kind":"Field","name":{"kind":"Name","value":"sizeBytes"}},{"kind":"Field","name":{"kind":"Name","value":"sha256Hash"}},{"kind":"Field","name":{"kind":"Name","value":"classification"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"visibilityScope"}},{"kind":"Field","name":{"kind":"Name","value":"scanStatus"}},{"kind":"Field","name":{"kind":"Name","value":"currentVersion"}},{"kind":"Field","name":{"kind":"Name","value":"downloadUrl"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetExpiringDocumentsQuery, GetExpiringDocumentsQueryVariables>;
export const GetDocumentTypesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDocumentTypes"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"includeDisabled"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Boolean"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentTypes"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"includeDisabled"},"value":{"kind":"Variable","name":{"kind":"Name","value":"includeDisabled"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DocumentTypeFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentTypeFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentTypeVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"displayName"}},{"kind":"Field","name":{"kind":"Name","value":"required"}},{"kind":"Field","name":{"kind":"Name","value":"expiring"}},{"kind":"Field","name":{"kind":"Name","value":"defaultValidityDays"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]} as unknown as DocumentNode<GetDocumentTypesQuery, GetDocumentTypesQueryVariables>;
export const VoidDocumentDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"VoidDocument"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"documentId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"reason"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"voidDocument"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"documentId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"documentId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"reason"},"value":{"kind":"Variable","name":{"kind":"Name","value":"reason"}}}]}}]}]}}]} as unknown as DocumentNode<VoidDocumentMutation, VoidDocumentMutationVariables>;
export const ExpireDocumentDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ExpireDocument"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"documentId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"expiresAt"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"expireDocument"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"documentId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"documentId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"expiresAt"},"value":{"kind":"Variable","name":{"kind":"Name","value":"expiresAt"}}}]}}]}]}}]} as unknown as DocumentNode<ExpireDocumentMutation, ExpireDocumentMutationVariables>;
export const DeleteDocumentReferenceDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteDocumentReference"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"documentId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteDocumentReference"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"documentId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"documentId"}}}]}}]}]}}]} as unknown as DocumentNode<DeleteDocumentReferenceMutation, DeleteDocumentReferenceMutationVariables>;
export const SignDocumentDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SignDocument"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"signature"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentSignatureDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"signDocument"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"signature"},"value":{"kind":"Variable","name":{"kind":"Name","value":"signature"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DocumentSignatureFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentSignatureFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentSignatureVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentSignatureId"}},{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"signerPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"signerPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"signerName"}},{"kind":"Field","name":{"kind":"Name","value":"signatureImageDocumentId"}},{"kind":"Field","name":{"kind":"Name","value":"legalTextAccepted"}},{"kind":"Field","name":{"kind":"Name","value":"latitude"}},{"kind":"Field","name":{"kind":"Name","value":"longitude"}},{"kind":"Field","name":{"kind":"Name","value":"signedAt"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]} as unknown as DocumentNode<SignDocumentMutation, SignDocumentMutationVariables>;
export const ConfigureDocumentTypeDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ConfigureDocumentType"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"documentType"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentTypeDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"configureDocumentType"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"documentType"},"value":{"kind":"Variable","name":{"kind":"Name","value":"documentType"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DocumentTypeFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DocumentTypeFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DocumentTypeVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"documentTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"displayName"}},{"kind":"Field","name":{"kind":"Name","value":"required"}},{"kind":"Field","name":{"kind":"Name","value":"expiring"}},{"kind":"Field","name":{"kind":"Name","value":"defaultValidityDays"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]} as unknown as DocumentNode<ConfigureDocumentTypeMutation, ConfigureDocumentTypeMutationVariables>;
export const DisableDocumentTypeDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DisableDocumentType"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"documentTypeId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"disableDocumentType"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"documentTypeId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"documentTypeId"}}}]}}]}]}}]} as unknown as DocumentNode<DisableDocumentTypeMutation, DisableDocumentTypeMutationVariables>;
export const GetDriversByAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDriversByAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driversByAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DriverItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"phone"}},{"kind":"Field","name":{"kind":"Name","value":"documentType"}},{"kind":"Field","name":{"kind":"Name","value":"documentNumber"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"employeeCode"}},{"kind":"Field","name":{"kind":"Name","value":"licenseNumber"}},{"kind":"Field","name":{"kind":"Name","value":"licenseExpiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"defaultTransporterId"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetDriversByAccountQuery, GetDriversByAccountQueryVariables>;
export const CreateDriverDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateDriver"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driver"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DriverDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createDriver"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"driver"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driver"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DriverItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"phone"}},{"kind":"Field","name":{"kind":"Name","value":"documentType"}},{"kind":"Field","name":{"kind":"Name","value":"documentNumber"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"employeeCode"}},{"kind":"Field","name":{"kind":"Name","value":"licenseNumber"}},{"kind":"Field","name":{"kind":"Name","value":"licenseExpiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"defaultTransporterId"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<CreateDriverMutation, CreateDriverMutationVariables>;
export const UpdateDriverDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateDriver"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driver"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DriverDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateDriver"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"driverId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"driver"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driver"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateDriverMutation, UpdateDriverMutationVariables>;
export const DeactivateDriverDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeactivateDriver"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deactivateDriver"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"driverId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}}}]}}]}]}}]} as unknown as DocumentNode<DeactivateDriverMutation, DeactivateDriverMutationVariables>;
export const GetDriverAssignmentsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDriverAssignments"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverAssignments"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"driverId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DriverActiveAssignmentItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverActiveAssignmentItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverAssignmentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"assignmentType"}}]}}]} as unknown as DocumentNode<GetDriverAssignmentsQuery, GetDriverAssignmentsQueryVariables>;
export const GetDriverQualificationsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDriverQualifications"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"expiringWithinDays"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverQualifications"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"driverId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"expiringWithinDays"},"value":{"kind":"Variable","name":{"kind":"Name","value":"expiringWithinDays"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DriverQualificationItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverQualificationItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverQualificationVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverQualificationId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"qualificationType"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"number"}},{"kind":"Field","name":{"kind":"Name","value":"issuedAt"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"issuingAuthority"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"notes"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetDriverQualificationsQuery, GetDriverQualificationsQueryVariables>;
export const GetDriverAssignmentHistoryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDriverAssignmentHistory"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"transporterId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"from"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"to"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverAssignmentHistory"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"driverId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"transporterId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"transporterId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"from"},"value":{"kind":"Variable","name":{"kind":"Name","value":"from"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"to"},"value":{"kind":"Variable","name":{"kind":"Name","value":"to"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DriverTransporterAssignmentItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverTransporterAssignmentItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverTransporterAssignmentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverTransporterAssignmentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"transporterName"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"assignmentType"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"createdByPrincipal"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetDriverAssignmentHistoryQuery, GetDriverAssignmentHistoryQueryVariables>;
export const CreateDriverQualificationDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateDriverQualification"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"qualification"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DriverQualificationDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createDriverQualification"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"qualification"},"value":{"kind":"Variable","name":{"kind":"Name","value":"qualification"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DriverQualificationItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverQualificationItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverQualificationVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverQualificationId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"qualificationType"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"number"}},{"kind":"Field","name":{"kind":"Name","value":"issuedAt"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"issuingAuthority"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"documentId"}},{"kind":"Field","name":{"kind":"Name","value":"notes"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<CreateDriverQualificationMutation, CreateDriverQualificationMutationVariables>;
export const UpdateDriverQualificationDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateDriverQualification"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverQualificationId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"qualification"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DriverQualificationDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateDriverQualification"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"driverQualificationId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverQualificationId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"qualification"},"value":{"kind":"Variable","name":{"kind":"Name","value":"qualification"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateDriverQualificationMutation, UpdateDriverQualificationMutationVariables>;
export const DeleteDriverQualificationDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteDriverQualification"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverQualificationId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteDriverQualification"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"driverQualificationId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverQualificationId"}}}]}}]}]}}]} as unknown as DocumentNode<DeleteDriverQualificationMutation, DeleteDriverQualificationMutationVariables>;
export const AssignDriverToTransporterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AssignDriverToTransporter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"transporterId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"startsAt"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"assignmentType"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"assignDriverToTransporter"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"driverId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"transporterId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"transporterId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"startsAt"},"value":{"kind":"Variable","name":{"kind":"Name","value":"startsAt"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"assignmentType"},"value":{"kind":"Variable","name":{"kind":"Name","value":"assignmentType"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DriverTransporterAssignmentItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DriverTransporterAssignmentItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DriverTransporterAssignmentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driverTransporterAssignmentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"transporterName"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"assignmentType"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"createdByPrincipal"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<AssignDriverToTransporterMutation, AssignDriverToTransporterMutationVariables>;
export const EndDriverAssignmentDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"EndDriverAssignment"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverTransporterAssignmentId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"endsAt"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"endDriverAssignment"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"driverTransporterAssignmentId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverTransporterAssignmentId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"endsAt"},"value":{"kind":"Variable","name":{"kind":"Name","value":"endsAt"}}}]}}]}]}}]} as unknown as DocumentNode<EndDriverAssignmentMutation, EndDriverAssignmentMutationVariables>;
export const GetGeocodingProvidersDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetGeocodingProviders"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"geocodingProviders"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"GeocodingProviderItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"GeocodingProviderItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"GeocodingProviderVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"geocodingProviderId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"endpointUri"}},{"kind":"Field","name":{"kind":"Name","value":"apiKey"}},{"kind":"Field","name":{"kind":"Name","value":"requestsPerSecond"}},{"kind":"Field","name":{"kind":"Name","value":"timeoutSeconds"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"active"}}]}}]} as unknown as DocumentNode<GetGeocodingProvidersQuery, GetGeocodingProvidersQueryVariables>;
export const CreateGeocodingProviderDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateGeocodingProvider"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"geocodingProvider"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"GeocodingProviderDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createGeocodingProvider"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"geocodingProvider"},"value":{"kind":"Variable","name":{"kind":"Name","value":"geocodingProvider"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"GeocodingProviderItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"GeocodingProviderItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"GeocodingProviderVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"geocodingProviderId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"endpointUri"}},{"kind":"Field","name":{"kind":"Name","value":"apiKey"}},{"kind":"Field","name":{"kind":"Name","value":"requestsPerSecond"}},{"kind":"Field","name":{"kind":"Name","value":"timeoutSeconds"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"active"}}]}}]} as unknown as DocumentNode<CreateGeocodingProviderMutation, CreateGeocodingProviderMutationVariables>;
export const UpdateGeocodingProviderDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateGeocodingProvider"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"geocodingProvider"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateGeocodingProviderDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateGeocodingProvider"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"geocodingProvider"},"value":{"kind":"Variable","name":{"kind":"Name","value":"geocodingProvider"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateGeocodingProviderMutation, UpdateGeocodingProviderMutationVariables>;
export const DeleteGeocodingProviderDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteGeocodingProvider"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteGeocodingProvider"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteGeocodingProviderMutation, DeleteGeocodingProviderMutationVariables>;
export const SetActiveGeocodingProviderDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SetActiveGeocodingProvider"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setActiveGeocodingProvider"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<SetActiveGeocodingProviderMutation, SetActiveGeocodingProviderMutationVariables>;
export const GetGpsIntegrationDashboardDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetGpsIntegrationDashboard"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"gpsIntegrationDashboard"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorsTotal"}},{"kind":"Field","name":{"kind":"Name","value":"operatorsEnabled"}},{"kind":"Field","name":{"kind":"Name","value":"operatorsHealthy"}},{"kind":"Field","name":{"kind":"Name","value":"operatorsDegraded"}},{"kind":"Field","name":{"kind":"Name","value":"operatorsOffline"}},{"kind":"Field","name":{"kind":"Name","value":"devicesTotal"}},{"kind":"Field","name":{"kind":"Name","value":"devicesNew"}},{"kind":"Field","name":{"kind":"Name","value":"devicesAvailable"}},{"kind":"Field","name":{"kind":"Name","value":"devicesAssigned"}},{"kind":"Field","name":{"kind":"Name","value":"devicesIgnored"}},{"kind":"Field","name":{"kind":"Name","value":"devicesRemoved"}},{"kind":"Field","name":{"kind":"Name","value":"recentlyAddedDevicesLast24h"}},{"kind":"Field","name":{"kind":"Name","value":"unassignedDevicesCount"}},{"kind":"Field","name":{"kind":"Name","value":"syncRunsSucceededLast24h"}},{"kind":"Field","name":{"kind":"Name","value":"syncRunsFailedLast24h"}},{"kind":"Field","name":{"kind":"Name","value":"lastAutomaticSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastManualSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"averageSyncDurationSeconds"}},{"kind":"Field","name":{"kind":"Name","value":"deviceCountsByProviderStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"operatorName"}},{"kind":"Field","name":{"kind":"Name","value":"detectedStatus"}},{"kind":"Field","name":{"kind":"Name","value":"count"}}]}}]}}]}}]} as unknown as DocumentNode<GetGpsIntegrationDashboardQuery, GetGpsIntegrationDashboardQueryVariables>;
export const GetGroupsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetGroups"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"groupsByAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"GroupItem"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"GroupItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"GroupVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"groupId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}}]}}]} as unknown as DocumentNode<GetGroupsQuery, GetGroupsQueryVariables>;
export const GetGroupLookupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetGroupLookup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"groupLookup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"groupId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]} as unknown as DocumentNode<GetGroupLookupQuery, GetGroupLookupQueryVariables>;
export const CreateGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"group"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"GroupDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"group"},"value":{"kind":"Variable","name":{"kind":"Name","value":"group"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"GroupItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"GroupItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"GroupVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"groupId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}}]}}]} as unknown as DocumentNode<CreateGroupMutation, CreateGroupMutationVariables>;
export const UpdateGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Long"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"group"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateGroupDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"group"},"value":{"kind":"Variable","name":{"kind":"Name","value":"group"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateGroupMutation, UpdateGroupMutationVariables>;
export const DeleteGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Long"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteGroupMutation, DeleteGroupMutationVariables>;
export const GetUsersByGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUsersByGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"groupId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Long"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"usersByGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"groupId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"groupId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}}]} as unknown as DocumentNode<GetUsersByGroupQuery, GetUsersByGroupQueryVariables>;
export const CreateUserGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateUserGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userGroup"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UserGroupDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createUserGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"userGroup"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userGroup"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"groupId"}}]}}]}}]} as unknown as DocumentNode<CreateUserGroupMutation, CreateUserGroupMutationVariables>;
export const DeleteUserGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteUserGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"groupId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Long"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteUserGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}},{"kind":"Argument","name":{"kind":"Name","value":"groupId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"groupId"}}}]}]}}]} as unknown as DocumentNode<DeleteUserGroupMutation, DeleteUserGroupMutationVariables>;
export const CreateTransporterGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateTransporterGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"transporterGroup"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterGroupDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createTransporterGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"transporterGroup"},"value":{"kind":"Variable","name":{"kind":"Name","value":"transporterGroup"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"groupId"}}]}}]}}]} as unknown as DocumentNode<CreateTransporterGroupMutation, CreateTransporterGroupMutationVariables>;
export const DeleteTransporterGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteTransporterGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"transporterId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"groupId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Long"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteTransporterGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"transporterId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"transporterId"}}},{"kind":"Argument","name":{"kind":"Name","value":"groupId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"groupId"}}}]}]}}]} as unknown as DocumentNode<DeleteTransporterGroupMutation, DeleteTransporterGroupMutationVariables>;
export const GetNotificationDeliveriesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetNotificationDeliveries"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"status"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"channel"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"from"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"to"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationDeliveries"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"status"},"value":{"kind":"Variable","name":{"kind":"Name","value":"status"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"channel"},"value":{"kind":"Variable","name":{"kind":"Name","value":"channel"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"from"},"value":{"kind":"Variable","name":{"kind":"Name","value":"from"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"to"},"value":{"kind":"Variable","name":{"kind":"Name","value":"to"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"NotificationDeliveryItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"NotificationDeliveryItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationDeliveryVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationDeliveryId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"notificationRuleId"}},{"kind":"Field","name":{"kind":"Name","value":"alertEventId"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"recipientPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"recipient"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"attempts"}},{"kind":"Field","name":{"kind":"Name","value":"providerMessageId"}},{"kind":"Field","name":{"kind":"Name","value":"error"}},{"kind":"Field","name":{"kind":"Name","value":"sentAt"}},{"kind":"Field","name":{"kind":"Name","value":"readAt"}}]}}]} as unknown as DocumentNode<GetNotificationDeliveriesQuery, GetNotificationDeliveriesQueryVariables>;
export const GetDeliveryHealthDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDeliveryHealth"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"from"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"to"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deliveryHealth"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"from"},"value":{"kind":"Variable","name":{"kind":"Name","value":"from"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"to"},"value":{"kind":"Variable","name":{"kind":"Name","value":"to"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"count"}},{"kind":"Field","name":{"kind":"Name","value":"averageAttempts"}}]}}]}}]} as unknown as DocumentNode<GetDeliveryHealthQuery, GetDeliveryHealthQueryVariables>;
export const GetMyNotificationsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetMyNotifications"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"unreadOnly"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Boolean"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"myNotifications"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"unreadOnly"},"value":{"kind":"Variable","name":{"kind":"Name","value":"unreadOnly"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"MyNotificationItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MyNotificationItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MyNotificationVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationDeliveryId"}},{"kind":"Field","name":{"kind":"Name","value":"alertEventId"}},{"kind":"Field","name":{"kind":"Name","value":"eventType"}},{"kind":"Field","name":{"kind":"Name","value":"severity"}},{"kind":"Field","name":{"kind":"Name","value":"sourceModule"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"payloadJson"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"readAt"}}]}}]} as unknown as DocumentNode<GetMyNotificationsQuery, GetMyNotificationsQueryVariables>;
export const RetryNotificationDeliveryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RetryNotificationDelivery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"notificationDeliveryId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"retryNotificationDelivery"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"notificationDeliveryId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"notificationDeliveryId"}}}]}}]}]}}]} as unknown as DocumentNode<RetryNotificationDeliveryMutation, RetryNotificationDeliveryMutationVariables>;
export const MarkNotificationReadDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"MarkNotificationRead"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"notificationDeliveryId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"markNotificationRead"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"notificationDeliveryId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"notificationDeliveryId"}}}]}}]}]}}]} as unknown as DocumentNode<MarkNotificationReadMutation, MarkNotificationReadMutationVariables>;
export const SendTestNotificationDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SendTestNotification"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"channel"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"contact"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"sendTestNotification"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"channel"},"value":{"kind":"Variable","name":{"kind":"Name","value":"channel"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"contact"},"value":{"kind":"Variable","name":{"kind":"Name","value":"contact"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationDeliveryId"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}}]} as unknown as DocumentNode<SendTestNotificationMutation, SendTestNotificationMutationVariables>;
export const GetNotificationRulesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetNotificationRules"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationRules"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"NotificationRuleItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"NotificationRuleItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationRuleVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationRuleId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"ruleKey"}},{"kind":"Field","name":{"kind":"Name","value":"ruleType"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"triggerEvent"}},{"kind":"Field","name":{"kind":"Name","value":"recipientSelector"}},{"kind":"Field","name":{"kind":"Name","value":"channelsJson"}},{"kind":"Field","name":{"kind":"Name","value":"throttlingJson"}},{"kind":"Field","name":{"kind":"Name","value":"configurationJson"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetNotificationRulesQuery, GetNotificationRulesQueryVariables>;
export const CreateNotificationRuleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateNotificationRule"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"notificationRule"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationRuleDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createNotificationRule"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"notificationRule"},"value":{"kind":"Variable","name":{"kind":"Name","value":"notificationRule"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationRuleId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"ruleKey"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]}}]} as unknown as DocumentNode<CreateNotificationRuleMutation, CreateNotificationRuleMutationVariables>;
export const UpdateNotificationRuleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateNotificationRule"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"notificationRuleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"notificationRule"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationRuleDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateNotificationRule"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"notificationRuleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"notificationRuleId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"notificationRule"},"value":{"kind":"Variable","name":{"kind":"Name","value":"notificationRule"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateNotificationRuleMutation, UpdateNotificationRuleMutationVariables>;
export const DisableNotificationRuleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DisableNotificationRule"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"notificationRuleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"disableNotificationRule"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"notificationRuleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"notificationRuleId"}}}]}}]}]}}]} as unknown as DocumentNode<DisableNotificationRuleMutation, DisableNotificationRuleMutationVariables>;
export const GetNotificationTemplatesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetNotificationTemplates"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationTemplates"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"NotificationTemplateItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"NotificationTemplateItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationTemplateVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationTemplateId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"templateKey"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"locale"}},{"kind":"Field","name":{"kind":"Name","value":"subject"}},{"kind":"Field","name":{"kind":"Name","value":"body"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetNotificationTemplatesQuery, GetNotificationTemplatesQueryVariables>;
export const CreateNotificationTemplateDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateNotificationTemplate"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"template"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationTemplateDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createNotificationTemplate"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"template"},"value":{"kind":"Variable","name":{"kind":"Name","value":"template"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"notificationTemplateId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"templateKey"}},{"kind":"Field","name":{"kind":"Name","value":"channel"}},{"kind":"Field","name":{"kind":"Name","value":"locale"}},{"kind":"Field","name":{"kind":"Name","value":"active"}}]}}]}}]} as unknown as DocumentNode<CreateNotificationTemplateMutation, CreateNotificationTemplateMutationVariables>;
export const UpdateNotificationTemplateDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateNotificationTemplate"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"notificationTemplateId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"template"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"NotificationTemplateDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateNotificationTemplate"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"notificationTemplateId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"notificationTemplateId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"template"},"value":{"kind":"Variable","name":{"kind":"Name","value":"template"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateNotificationTemplateMutation, UpdateNotificationTemplateMutationVariables>;
export const DeleteNotificationTemplateDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteNotificationTemplate"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"notificationTemplateId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteNotificationTemplate"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"notificationTemplateId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"notificationTemplateId"}}}]}}]}]}}]} as unknown as DocumentNode<DeleteNotificationTemplateMutation, DeleteNotificationTemplateMutationVariables>;
export const GetOperatorsByCurrentAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetOperatorsByCurrentAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorsByCurrentAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"OperatorDetail"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"OperatorDetail"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"OperatorVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"address"}},{"kind":"Field","name":{"kind":"Name","value":"contactName"}},{"kind":"Field","name":{"kind":"Name","value":"protocolType"}},{"kind":"Field","name":{"kind":"Name","value":"protocolTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"syncIntervalMinutes"}},{"kind":"Field","name":{"kind":"Name","value":"healthStatus"}},{"kind":"Field","name":{"kind":"Name","value":"lastSuccessfulSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailedSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailureCode"}},{"kind":"Field","name":{"kind":"Name","value":"lastLatencyMs"}},{"kind":"Field","name":{"kind":"Name","value":"lastDeviceSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastPositionSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetOperatorsByCurrentAccountQuery, GetOperatorsByCurrentAccountQueryVariables>;
export const GetOperatorLookupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetOperatorLookup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorLookup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]} as unknown as DocumentNode<GetOperatorLookupQuery, GetOperatorLookupQueryVariables>;
export const GetGpsOperatorsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetGpsOperators"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorsByCurrentAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"OperatorGps"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"OperatorGps"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"OperatorVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"protocolType"}},{"kind":"Field","name":{"kind":"Name","value":"protocolTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"lastDeviceSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastPositionSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"syncIntervalMinutes"}}]}}]} as unknown as DocumentNode<GetGpsOperatorsQuery, GetGpsOperatorsQueryVariables>;
export const CreateOperatorDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateOperator"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"operator"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"OperatorDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createOperator"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"operator"},"value":{"kind":"Variable","name":{"kind":"Name","value":"operator"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"OperatorDetail"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"OperatorDetail"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"OperatorVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"operatorId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"address"}},{"kind":"Field","name":{"kind":"Name","value":"contactName"}},{"kind":"Field","name":{"kind":"Name","value":"protocolType"}},{"kind":"Field","name":{"kind":"Name","value":"protocolTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"enabled"}},{"kind":"Field","name":{"kind":"Name","value":"syncIntervalMinutes"}},{"kind":"Field","name":{"kind":"Name","value":"healthStatus"}},{"kind":"Field","name":{"kind":"Name","value":"lastSuccessfulSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailedSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastFailureCode"}},{"kind":"Field","name":{"kind":"Name","value":"lastLatencyMs"}},{"kind":"Field","name":{"kind":"Name","value":"lastDeviceSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastPositionSyncAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<CreateOperatorMutation, CreateOperatorMutationVariables>;
export const UpdateOperatorDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateOperator"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"operator"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateOperatorDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateOperator"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"operator"},"value":{"kind":"Variable","name":{"kind":"Name","value":"operator"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateOperatorMutation, UpdateOperatorMutationVariables>;
export const DeleteOperatorDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteOperator"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteOperator"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteOperatorMutation, DeleteOperatorMutationVariables>;
export const SetOperatorEnabledDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SetOperatorEnabled"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"enabled"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Boolean"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setOperatorEnabled"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"operatorId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"operatorId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"enabled"},"value":{"kind":"Variable","name":{"kind":"Name","value":"enabled"}}}]}}]}]}}]} as unknown as DocumentNode<SetOperatorEnabledMutation, SetOperatorEnabledMutationVariables>;
export const TriggerOperatorDeviceSyncDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"TriggerOperatorDeviceSync"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"command"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"TriggerOperatorDeviceSyncCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"triggerOperatorDeviceSync"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"Variable","name":{"kind":"Name","value":"command"}}}]}]}}]} as unknown as DocumentNode<TriggerOperatorDeviceSyncMutation, TriggerOperatorDeviceSyncMutationVariables>;
export const GetPlatformAnnouncementsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetPlatformAnnouncements"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"platformAnnouncements"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlatformAnnouncementItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlatformAnnouncementItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PlatformAnnouncementVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"platformAnnouncementId"}},{"kind":"Field","name":{"kind":"Name","value":"messageEn"}},{"kind":"Field","name":{"kind":"Name","value":"messageEs"}},{"kind":"Field","name":{"kind":"Name","value":"severity"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetPlatformAnnouncementsQuery, GetPlatformAnnouncementsQueryVariables>;
export const GetBackgroundJobStatusDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetBackgroundJobStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgroundJobStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"jobKey"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"startedAt"}},{"kind":"Field","name":{"kind":"Name","value":"completedAt"}},{"kind":"Field","name":{"kind":"Name","value":"attempts"}},{"kind":"Field","name":{"kind":"Name","value":"errorCode"}},{"kind":"Field","name":{"kind":"Name","value":"recordsEveryCycle"}}]}}]}}]} as unknown as DocumentNode<GetBackgroundJobStatusQuery, GetBackgroundJobStatusQueryVariables>;
export const CreatePlatformAnnouncementDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreatePlatformAnnouncement"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"announcement"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"PlatformAnnouncementDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createPlatformAnnouncement"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"announcement"},"value":{"kind":"Variable","name":{"kind":"Name","value":"announcement"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlatformAnnouncementItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlatformAnnouncementItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PlatformAnnouncementVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"platformAnnouncementId"}},{"kind":"Field","name":{"kind":"Name","value":"messageEn"}},{"kind":"Field","name":{"kind":"Name","value":"messageEs"}},{"kind":"Field","name":{"kind":"Name","value":"severity"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<CreatePlatformAnnouncementMutation, CreatePlatformAnnouncementMutationVariables>;
export const UpdatePlatformAnnouncementDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdatePlatformAnnouncement"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"platformAnnouncementId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"announcement"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"PlatformAnnouncementDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updatePlatformAnnouncement"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"platformAnnouncementId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"platformAnnouncementId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"announcement"},"value":{"kind":"Variable","name":{"kind":"Name","value":"announcement"}}}]}}]}]}}]} as unknown as DocumentNode<UpdatePlatformAnnouncementMutation, UpdatePlatformAnnouncementMutationVariables>;
export const DeletePlatformAnnouncementDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeletePlatformAnnouncement"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"platformAnnouncementId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deletePlatformAnnouncement"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"platformAnnouncementId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"platformAnnouncementId"}}}]}}]}]}}]} as unknown as DocumentNode<DeletePlatformAnnouncementMutation, DeletePlatformAnnouncementMutationVariables>;
export const GetPointsOfInterestByAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetPointsOfInterestByAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"pointsOfInterestByAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"PointOfInterestItem"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PointOfInterestItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PointOfInterestVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"pointOfInterestId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"latitude"}},{"kind":"Field","name":{"kind":"Name","value":"longitude"}},{"kind":"Field","name":{"kind":"Name","value":"address"}},{"kind":"Field","name":{"kind":"Name","value":"color"}},{"kind":"Field","name":{"kind":"Name","value":"groupId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}}]}}]} as unknown as DocumentNode<GetPointsOfInterestByAccountQuery, GetPointsOfInterestByAccountQueryVariables>;
export const GetPointOfInterestLookupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetPointOfInterestLookup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"pointOfInterestLookup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"pointOfInterestId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"latitude"}},{"kind":"Field","name":{"kind":"Name","value":"longitude"}},{"kind":"Field","name":{"kind":"Name","value":"color"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"address"}},{"kind":"Field","name":{"kind":"Name","value":"active"}}]}}]}}]} as unknown as DocumentNode<GetPointOfInterestLookupQuery, GetPointOfInterestLookupQueryVariables>;
export const CreatePointOfInterestDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreatePointOfInterest"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"pointOfInterest"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"PointOfInterestDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createPointOfInterest"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"pointOfInterest"},"value":{"kind":"Variable","name":{"kind":"Name","value":"pointOfInterest"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"PointOfInterestItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PointOfInterestItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PointOfInterestVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"pointOfInterestId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"latitude"}},{"kind":"Field","name":{"kind":"Name","value":"longitude"}},{"kind":"Field","name":{"kind":"Name","value":"address"}},{"kind":"Field","name":{"kind":"Name","value":"color"}},{"kind":"Field","name":{"kind":"Name","value":"groupId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}}]}}]} as unknown as DocumentNode<CreatePointOfInterestMutation, CreatePointOfInterestMutationVariables>;
export const UpdatePointOfInterestDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdatePointOfInterest"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"pointOfInterest"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdatePointOfInterestDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updatePointOfInterest"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"pointOfInterest"},"value":{"kind":"Variable","name":{"kind":"Name","value":"pointOfInterest"}}}]}}]}]}}]} as unknown as DocumentNode<UpdatePointOfInterestMutation, UpdatePointOfInterestMutationVariables>;
export const DeletePointOfInterestDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeletePointOfInterest"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deletePointOfInterest"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeletePointOfInterestMutation, DeletePointOfInterestMutationVariables>;
export const GetCurrentPrincipalDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetCurrentPrincipal"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"currentPrincipal"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"CurrentPrincipalItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"CurrentPrincipalItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CurrentPrincipalVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"subjectId"}},{"kind":"Field","name":{"kind":"Name","value":"principalType"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"clientId"}},{"kind":"Field","name":{"kind":"Name","value":"publicLinkGrantId"}},{"kind":"Field","name":{"kind":"Name","value":"role"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"scopes"}},{"kind":"Field","name":{"kind":"Name","value":"correlationId"}}]}}]} as unknown as DocumentNode<GetCurrentPrincipalQuery, GetCurrentPrincipalQueryVariables>;
export const GetPublicLinkGrantsByAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetPublicLinkGrantsByAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"publicLinkGrantsByAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"PublicLinkGrantFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PublicLinkGrantFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PublicLinkGrantVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"publicLinkGrantId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"scopes"}},{"kind":"Field","name":{"kind":"Name","value":"purpose"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedBy"}},{"kind":"Field","name":{"kind":"Name","value":"createdByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"accessCount"}},{"kind":"Field","name":{"kind":"Name","value":"lastAccessedAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}},{"kind":"Field","name":{"kind":"Name","value":"token"}}]}}]} as unknown as DocumentNode<GetPublicLinkGrantsByAccountQuery, GetPublicLinkGrantsByAccountQueryVariables>;
export const CreatePublicLinkGrantDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreatePublicLinkGrant"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"publicLinkGrant"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"PublicLinkGrantDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createPublicLinkGrant"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"publicLinkGrant"},"value":{"kind":"Variable","name":{"kind":"Name","value":"publicLinkGrant"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"PublicLinkGrantFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PublicLinkGrantFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PublicLinkGrantVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"publicLinkGrantId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceType"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"scopes"}},{"kind":"Field","name":{"kind":"Name","value":"purpose"}},{"kind":"Field","name":{"kind":"Name","value":"expiresAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedBy"}},{"kind":"Field","name":{"kind":"Name","value":"createdByPrincipalId"}},{"kind":"Field","name":{"kind":"Name","value":"accessCount"}},{"kind":"Field","name":{"kind":"Name","value":"lastAccessedAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}},{"kind":"Field","name":{"kind":"Name","value":"token"}}]}}]} as unknown as DocumentNode<CreatePublicLinkGrantMutation, CreatePublicLinkGrantMutationVariables>;
export const RevokePublicLinkGrantDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RevokePublicLinkGrant"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"publicLinkGrantId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"revokedBy"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"revokePublicLinkGrant"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"publicLinkGrantId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"publicLinkGrantId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"revokedBy"},"value":{"kind":"Variable","name":{"kind":"Name","value":"revokedBy"}}}]}}]}]}}]} as unknown as DocumentNode<RevokePublicLinkGrantMutation, RevokePublicLinkGrantMutationVariables>;
export const GetReportsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetReports"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"reports"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"ReportFields"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReportFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReportVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"reportId"}},{"kind":"Field","name":{"kind":"Name","value":"code"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"typeId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"category"}},{"kind":"Field","name":{"kind":"Name","value":"requiredFeatureKey"}},{"kind":"Field","name":{"kind":"Name","value":"managerOnly"}},{"kind":"Field","name":{"kind":"Name","value":"supportsPdf"}},{"kind":"Field","name":{"kind":"Name","value":"sortOrder"}},{"kind":"Field","name":{"kind":"Name","value":"filters"}}]}}]} as unknown as DocumentNode<GetReportsQuery, GetReportsQueryVariables>;
export const UpdateReportDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateReport"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"report"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateReportDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateReport"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"report"},"value":{"kind":"Variable","name":{"kind":"Name","value":"report"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateReportMutation, UpdateReportMutationVariables>;
export const GetAccountSettingsByUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAccountSettingsByUser"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountSettingsByUser"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountSettingsItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountSettingsItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountSettingsVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"maps"}},{"kind":"Field","name":{"kind":"Name","value":"mapsKey"}},{"kind":"Field","name":{"kind":"Name","value":"onlineInterval"}},{"kind":"Field","name":{"kind":"Name","value":"refreshMap"}},{"kind":"Field","name":{"kind":"Name","value":"refreshMapInterval"}}]}}]} as unknown as DocumentNode<GetAccountSettingsByUserQuery, GetAccountSettingsByUserQueryVariables>;
export const GetUserSettingsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUserSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"UserSettingsItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserSettingsItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UserSettingsVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"style"}},{"kind":"Field","name":{"kind":"Name","value":"language"}},{"kind":"Field","name":{"kind":"Name","value":"navbar"}}]}}]} as unknown as DocumentNode<GetUserSettingsQuery, GetUserSettingsQueryVariables>;
export const UpdateAccountSettingsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateAccountSettings"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountSettings"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AccountSettingsDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateAccountSettings"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountSettings"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountSettings"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateAccountSettingsMutation, UpdateAccountSettingsMutationVariables>;
export const UpdateUserSettingsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateUserSettings"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userSettings"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UserSettingsDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateUserSettings"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"userSettings"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userSettings"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateUserSettingsMutation, UpdateUserSettingsMutationVariables>;
export const GetAccountSupportGrantsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAccountSupportGrants"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountSupportGrants"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountSupportGrantItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountSupportGrantItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountSupportGrantVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountSupportGrantId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"supportUserId"}},{"kind":"Field","name":{"kind":"Name","value":"reason"}},{"kind":"Field","name":{"kind":"Name","value":"ticketReference"}},{"kind":"Field","name":{"kind":"Name","value":"approvedBy"}},{"kind":"Field","name":{"kind":"Name","value":"approvedAt"}},{"kind":"Field","name":{"kind":"Name","value":"accessLevel"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedBy"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetAccountSupportGrantsQuery, GetAccountSupportGrantsQueryVariables>;
export const CreateAccountSupportGrantDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateAccountSupportGrant"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountSupportGrant"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AccountSupportGrantDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createAccountSupportGrant"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountSupportGrant"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountSupportGrant"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AccountSupportGrantItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AccountSupportGrantItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AccountSupportGrantVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"accountSupportGrantId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"supportUserId"}},{"kind":"Field","name":{"kind":"Name","value":"reason"}},{"kind":"Field","name":{"kind":"Name","value":"ticketReference"}},{"kind":"Field","name":{"kind":"Name","value":"approvedBy"}},{"kind":"Field","name":{"kind":"Name","value":"approvedAt"}},{"kind":"Field","name":{"kind":"Name","value":"accessLevel"}},{"kind":"Field","name":{"kind":"Name","value":"startsAt"}},{"kind":"Field","name":{"kind":"Name","value":"endsAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedAt"}},{"kind":"Field","name":{"kind":"Name","value":"revokedBy"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<CreateAccountSupportGrantMutation, CreateAccountSupportGrantMutationVariables>;
export const ApproveAccountSupportGrantDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ApproveAccountSupportGrant"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountSupportGrantId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"approvedBy"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"approveAccountSupportGrant"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountSupportGrantId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountSupportGrantId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"approvedBy"},"value":{"kind":"Variable","name":{"kind":"Name","value":"approvedBy"}}}]}}]}]}}]} as unknown as DocumentNode<ApproveAccountSupportGrantMutation, ApproveAccountSupportGrantMutationVariables>;
export const RevokeAccountSupportGrantDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RevokeAccountSupportGrant"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountSupportGrantId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"revokedBy"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"revokeAccountSupportGrant"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountSupportGrantId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountSupportGrantId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"revokedBy"},"value":{"kind":"Variable","name":{"kind":"Name","value":"revokedBy"}}}]}}]}]}}]} as unknown as DocumentNode<RevokeAccountSupportGrantMutation, RevokeAccountSupportGrantMutationVariables>;
export const GetTransportersByAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetTransportersByAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transportersByAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TransporterItem"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TransporterItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"transporterType"}},{"kind":"Field","name":{"kind":"Name","value":"transporterTypeId"}}]}}]} as unknown as DocumentNode<GetTransportersByAccountQuery, GetTransportersByAccountQueryVariables>;
export const GetTransportersByGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetTransportersByGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"groupId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Long"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"search"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transportersByGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"groupId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"groupId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"search"},"value":{"kind":"Variable","name":{"kind":"Name","value":"search"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TransporterItem"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TransporterItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"transporterType"}},{"kind":"Field","name":{"kind":"Name","value":"transporterTypeId"}}]}}]} as unknown as DocumentNode<GetTransportersByGroupQuery, GetTransportersByGroupQueryVariables>;
export const GetTransporterLookupByAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetTransporterLookupByAccount"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterLookupByAccount"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"transporterType"}},{"kind":"Field","name":{"kind":"Name","value":"transporterTypeId"}}]}}]}}]} as unknown as DocumentNode<GetTransporterLookupByAccountQuery, GetTransporterLookupByAccountQueryVariables>;
export const GetTransporterLookupByUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetTransporterLookupByUser"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterLookupByUser"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"transporterType"}},{"kind":"Field","name":{"kind":"Name","value":"transporterTypeId"}}]}}]}}]} as unknown as DocumentNode<GetTransporterLookupByUserQuery, GetTransporterLookupByUserQueryVariables>;
export const CreateTransporterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateTransporter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"transporter"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createTransporter"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"transporter"},"value":{"kind":"Variable","name":{"kind":"Name","value":"transporter"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TransporterItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TransporterItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"transporterType"}},{"kind":"Field","name":{"kind":"Name","value":"transporterTypeId"}}]}}]} as unknown as DocumentNode<CreateTransporterMutation, CreateTransporterMutationVariables>;
export const UpdateTransporterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateTransporter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"transporter"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateTransporterDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateTransporter"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"transporter"},"value":{"kind":"Variable","name":{"kind":"Name","value":"transporter"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateTransporterMutation, UpdateTransporterMutationVariables>;
export const DeleteTransporterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteTransporter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteTransporter"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteTransporterMutation, DeleteTransporterMutationVariables>;
export const GetTransporterDeviceAssignmentsByAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetTransporterDeviceAssignmentsByAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"activeOnly"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Boolean"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterDeviceAssignmentsByAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"activeOnly"},"value":{"kind":"Variable","name":{"kind":"Name","value":"activeOnly"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"AssignmentFields"}},{"kind":"Field","name":{"kind":"Name","value":"createdByPrincipalType"}},{"kind":"Field","name":{"kind":"Name","value":"createdByPrincipalId"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AssignmentFields"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterDeviceAssignmentVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterDeviceAssignmentId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"priority"}},{"kind":"Field","name":{"kind":"Name","value":"isPrimary"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"assignmentReason"}}]}}]} as unknown as DocumentNode<GetTransporterDeviceAssignmentsByAccountQuery, GetTransporterDeviceAssignmentsByAccountQueryVariables>;
export const AssignDeviceToTransporterDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AssignDeviceToTransporter"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"assignment"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterDeviceAssignmentDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"assignDeviceToTransporter"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"assignment"},"value":{"kind":"Variable","name":{"kind":"Name","value":"assignment"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterDeviceAssignmentId"}},{"kind":"Field","name":{"kind":"Name","value":"deviceId"}},{"kind":"Field","name":{"kind":"Name","value":"transporterId"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"isPrimary"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}}]} as unknown as DocumentNode<AssignDeviceToTransporterMutation, AssignDeviceToTransporterMutationVariables>;
export const EndDeviceTransporterAssignmentDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"EndDeviceTransporterAssignment"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"assignmentId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"reason"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"endDeviceTransporterAssignment"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"assignmentId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"assignmentId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"reason"},"value":{"kind":"Variable","name":{"kind":"Name","value":"reason"}}}]}}]}]}}]} as unknown as DocumentNode<EndDeviceTransporterAssignmentMutation, EndDeviceTransporterAssignmentMutationVariables>;
export const GetTransporterTypesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetTransporterTypes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterTypes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TransporterTypeItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TransporterTypeItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterTypeVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"transporterTypeId"}},{"kind":"Field","name":{"kind":"Name","value":"accBased"}},{"kind":"Field","name":{"kind":"Name","value":"stoppedGap"}},{"kind":"Field","name":{"kind":"Name","value":"maxDistance"}},{"kind":"Field","name":{"kind":"Name","value":"maxTimeGap"}},{"kind":"Field","name":{"kind":"Name","value":"type"}}]}}]} as unknown as DocumentNode<GetTransporterTypesQuery, GetTransporterTypesQueryVariables>;
export const UpdateTransporterTypeDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateTransporterType"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Short"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"transporterType"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"TransporterTypeDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateTransporterType"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"transporterType"},"value":{"kind":"Variable","name":{"kind":"Name","value":"transporterType"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateTransporterTypeMutation, UpdateTransporterTypeMutationVariables>;