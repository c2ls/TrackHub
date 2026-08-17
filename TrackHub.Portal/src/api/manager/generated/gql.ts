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
    "\n  fragment AccountFeatureItem on AccountFeatureVm {\n    accountFeatureId\n    accountId\n    featureKey\n    enabled\n    tier\n    source\n    effectiveFrom\n    effectiveTo\n    configurationJson\n    lastModified\n  }\n": typeof types.AccountFeatureItemFragmentDoc,
    "\n  query GetAccountFeatures($accountId: UUID!) {\n    accountFeatures(query: { accountId: $accountId }) {\n      ...AccountFeatureItem\n    }\n  }\n": typeof types.GetAccountFeaturesDocument,
    "\n  mutation SetAccountFeature($feature: AccountFeatureDtoInput!) {\n    setAccountFeature(command: { feature: $feature }) {\n      ...AccountFeatureItem\n    }\n  }\n": typeof types.SetAccountFeatureDocument,
    "\n  query GetAccountFeaturesMaster($accountId: UUID!) {\n    accountFeaturesMaster(query: { accountId: $accountId }) {\n      ...AccountFeatureItem\n    }\n  }\n": typeof types.GetAccountFeaturesMasterDocument,
    "\n  mutation SetAccountFeatureMaster($feature: AccountFeatureDtoInput!) {\n    setAccountFeatureMaster(command: { feature: $feature }) {\n      ...AccountFeatureItem\n    }\n  }\n": typeof types.SetAccountFeatureMasterDocument,
    "\n  fragment AccountItem on AccountVm {\n    accountId\n    name\n    description\n    type\n    typeId\n    status\n    statusId\n    active\n    lastModified\n  }\n": typeof types.AccountItemFragmentDoc,
    "\n  query GetAccount($id: UUID!) {\n    account(query: { id: $id }) {\n      ...AccountItem\n    }\n  }\n": typeof types.GetAccountDocument,
    "\n  query GetAccountByUser {\n    accountByUser {\n      ...AccountItem\n    }\n  }\n": typeof types.GetAccountByUserDocument,
    "\n  query GetAccounts {\n    accounts {\n      ...AccountItem\n    }\n  }\n": typeof types.GetAccountsDocument,
    "\n  mutation CreateAccount($account: AccountDtoInput!) {\n    createAccount(command: { account: $account }) {\n      ...AccountItem\n    }\n  }\n": typeof types.CreateAccountDocument,
    "\n  mutation UpdateAccount($id: UUID!, $account: UpdateAccountDtoInput!) {\n    updateAccount(id: $id, command: { account: $account })\n  }\n": typeof types.UpdateAccountDocument,
    "\n  mutation ChangeAccountStatus($accountId: UUID!, $targetStatus: AccountStatus!, $reason: String) {\n    changeAccountStatus(\n      command: { accountId: $accountId, targetStatus: $targetStatus, reason: $reason }\n    ) {\n      ...AccountItem\n    }\n  }\n": typeof types.ChangeAccountStatusDocument,
    "\n  query GetAccountContext {\n    accountContext {\n      status\n      statusId\n      branding {\n        accountId\n        displayName\n        logoDocumentId\n        primaryColor\n        reportHeader\n        lastModified\n      }\n      features {\n        accountFeatureId\n        accountId\n        featureKey\n        enabled\n        tier\n        source\n        effectiveFrom\n        effectiveTo\n        configurationJson\n        lastModified\n      }\n    }\n  }\n": typeof types.GetAccountContextDocument,
    "\n  fragment AlertEventItem on AlertEventVm {\n    alertEventId\n    accountId\n    eventType\n    severity\n    sourceModule\n    resourceType\n    resourceId\n    status\n    firstSeenAt\n    lastSeenAt\n    deduplicationKey\n  }\n": typeof types.AlertEventItemFragmentDoc,
    "\n  query GetAlertEvents($accountId: UUID!, $skip: Int!, $take: Int!) {\n    alertEvents(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AlertEventItem\n    }\n  }\n": typeof types.GetAlertEventsDocument,
    "\n  mutation AcknowledgeAlertEvent($alertEventId: UUID!) {\n    acknowledgeAlertEvent(command: { alertEventId: $alertEventId })\n  }\n": typeof types.AcknowledgeAlertEventDocument,
    "\n  mutation ResolveAlertEvent($alertEventId: UUID!) {\n    resolveAlertEvent(command: { alertEventId: $alertEventId })\n  }\n": typeof types.ResolveAlertEventDocument,
    "\n  fragment AlertSubscriptionItem on AlertSubscriptionVm {\n    alertSubscriptionId\n    accountId\n    principalType\n    principalId\n    eventTypeFilter\n    channel\n    contact\n    enabled\n    lastModified\n  }\n": typeof types.AlertSubscriptionItemFragmentDoc,
    "\n  query GetAlertSubscriptions($accountId: UUID!, $principalId: UUID, $skip: Int!, $take: Int!) {\n    alertSubscriptions(\n      query: { accountId: $accountId, principalId: $principalId, skip: $skip, take: $take }\n    ) {\n      ...AlertSubscriptionItem\n    }\n  }\n": typeof types.GetAlertSubscriptionsDocument,
    "\n  mutation CreateAlertSubscription($subscription: AlertSubscriptionDtoInput!) {\n    createAlertSubscription(command: { subscription: $subscription }) {\n      alertSubscriptionId\n      accountId\n      principalType\n      principalId\n      channel\n      enabled\n    }\n  }\n": typeof types.CreateAlertSubscriptionDocument,
    "\n  mutation UpdateAlertSubscription(\n    $alertSubscriptionId: UUID!\n    $subscription: AlertSubscriptionDtoInput!\n  ) {\n    updateAlertSubscription(\n      command: { alertSubscriptionId: $alertSubscriptionId, subscription: $subscription }\n    )\n  }\n": typeof types.UpdateAlertSubscriptionDocument,
    "\n  mutation DeleteAlertSubscription($alertSubscriptionId: UUID!) {\n    deleteAlertSubscription(command: { alertSubscriptionId: $alertSubscriptionId })\n  }\n": typeof types.DeleteAlertSubscriptionDocument,
    "\n  fragment AuditEventItem on AuditEventVm {\n    auditEventId\n    accountId\n    actorType\n    actorId\n    action\n    resourceType\n    resourceId\n    result\n    reason\n    correlationId\n    occurredAt\n  }\n": typeof types.AuditEventItemFragmentDoc,
    "\n  query GetAuditTrail($accountId: UUID!, $skip: Int!, $take: Int!) {\n    auditTrail(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AuditEventItem\n    }\n  }\n": typeof types.GetAuditTrailDocument,
    "\n  fragment BackgroundJobRunItem on BackgroundJobRunVm {\n    backgroundJobRunId\n    jobKey\n    accountId\n    resourceKey\n    idempotencyKey\n    status\n    attempts\n    startedAt\n    completedAt\n    errorCode\n    errorMessage\n  }\n": typeof types.BackgroundJobRunItemFragmentDoc,
    "\n  query GetBackgroundJobRuns($accountId: UUID!, $skip: Int!, $take: Int!) {\n    backgroundJobRuns(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...BackgroundJobRunItem\n    }\n  }\n": typeof types.GetBackgroundJobRunsDocument,
    "\n  fragment AccountBrandingItem on AccountBrandingVm {\n    accountId\n    displayName\n    logoDocumentId\n    primaryColor\n    reportHeader\n    lastModified\n  }\n": typeof types.AccountBrandingItemFragmentDoc,
    "\n  query GetAccountBranding($accountId: UUID!) {\n    accountBranding(query: { accountId: $accountId }) {\n      ...AccountBrandingItem\n    }\n  }\n": typeof types.GetAccountBrandingDocument,
    "\n  mutation UpdateAccountBranding($branding: AccountBrandingDtoInput!) {\n    updateAccountBranding(command: { branding: $branding }) {\n      ...AccountBrandingItem\n    }\n  }\n": typeof types.UpdateAccountBrandingDocument,
    "\n  fragment CredentialFields on CredentialVm {\n    credentialId\n    key\n    key2\n    password\n    uri\n    username\n  }\n": typeof types.CredentialFieldsFragmentDoc,
    "\n  query GetCredentialByOperator($operatorId: UUID!) {\n    credentialByOperator(query: { operatorId: $operatorId }) {\n      ...CredentialFields\n    }\n  }\n": typeof types.GetCredentialByOperatorDocument,
    "\n  mutation CreateCredential($credential: CredentialDtoInput!) {\n    createCredential(command: { credential: $credential }) {\n      ...CredentialFields\n    }\n  }\n": typeof types.CreateCredentialDocument,
    "\n  mutation UpdateCredential($id: UUID!, $credential: UpdateCredentialDtoInput!) {\n    updateCredential(id: $id, command: { credential: $credential })\n  }\n": typeof types.UpdateCredentialDocument,
    "\n  fragment DeviceItem on DeviceVm {\n    deviceId\n    name\n    serial\n    description\n    deviceType\n    deviceTypeId\n    identifier\n    operatorId\n  }\n": typeof types.DeviceItemFragmentDoc,
    "\n  fragment SynchronizedDevice on DeviceVm {\n    deviceId\n    accountId\n    operatorId\n    serial\n    name\n    identifier\n    providerDisplayName\n    providerStatus\n    detectedStatus\n    firstSeenAt\n    lastSeenAt\n    lastSyncedAt\n    lastAssignedAt\n    ignoredAt\n  }\n": typeof types.SynchronizedDeviceFragmentDoc,
    "\n  query GetDevicesByAccount {\n    devicesByAccount {\n      ...DeviceItem\n    }\n  }\n": typeof types.GetDevicesByAccountDocument,
    "\n  mutation DeleteDevice($deviceId: UUID!) {\n    deleteDevice(deviceId: $deviceId)\n  }\n": typeof types.DeleteDeviceDocument,
    "\n  query GetSynchronizedDevices($accountId: UUID!, $detectedStatus: DetectedStatus, $operatorId: UUID) {\n    synchronizedDevices(\n      query: { accountId: $accountId, detectedStatus: $detectedStatus, operatorId: $operatorId }\n    ) {\n      ...SynchronizedDevice\n    }\n  }\n": typeof types.GetSynchronizedDevicesDocument,
    "\n  query GetUnassignedSynchronizedDevices($accountId: UUID!) {\n    unassignedSynchronizedDevices(query: { accountId: $accountId }) {\n      ...SynchronizedDevice\n    }\n  }\n": typeof types.GetUnassignedSynchronizedDevicesDocument,
    "\n  mutation SetSynchronizedDeviceIgnored($deviceId: UUID!, $ignored: Boolean!) {\n    setSynchronizedDeviceIgnored(command: { deviceId: $deviceId, ignored: $ignored })\n  }\n": typeof types.SetSynchronizedDeviceIgnoredDocument,
    "\n  fragment DocumentFields on DocumentVm {\n    documentId\n    accountId\n    ownerEntityType\n    ownerEntityId\n    uploadedByPrincipalType\n    uploadedByPrincipalId\n    fileName\n    category\n    title\n    description\n    contentType\n    sizeBytes\n    sha256Hash\n    classification\n    status\n    expiresAt\n    visibilityScope\n    scanStatus\n    currentVersion\n    downloadUrl\n    lastModified\n  }\n": typeof types.DocumentFieldsFragmentDoc,
    "\n  fragment DocumentVersionFields on DocumentVersionVm {\n    documentVersionId\n    documentId\n    accountId\n    versionNumber\n    contentType\n    fileName\n    sizeBytes\n    sha256Hash\n    scanStatus\n    replacedByPrincipalType\n    replacedByPrincipalId\n    reason\n    createdAt\n  }\n": typeof types.DocumentVersionFieldsFragmentDoc,
    "\n  fragment DocumentTypeFields on DocumentTypeVm {\n    documentTypeId\n    accountId\n    category\n    displayName\n    required\n    expiring\n    defaultValidityDays\n    enabled\n    createdAt\n  }\n": typeof types.DocumentTypeFieldsFragmentDoc,
    "\n  fragment DocumentSignatureFields on DocumentSignatureVm {\n    documentSignatureId\n    documentId\n    accountId\n    signerPrincipalType\n    signerPrincipalId\n    signerName\n    signatureImageDocumentId\n    legalTextAccepted\n    latitude\n    longitude\n    signedAt\n    createdAt\n  }\n": typeof types.DocumentSignatureFieldsFragmentDoc,
    "\n  query GetDocumentsForOwner(\n    $accountId: UUID!\n    $ownerEntityType: String!\n    $ownerEntityId: String!\n    $skip: Int!\n    $take: Int!\n  ) {\n    documentsForOwner(\n      query: {\n        accountId: $accountId\n        ownerEntityType: $ownerEntityType\n        ownerEntityId: $ownerEntityId\n        skip: $skip\n        take: $take\n      }\n    ) {\n      ...DocumentFields\n    }\n  }\n": typeof types.GetDocumentsForOwnerDocument,
    "\n  query GetDocument($documentId: UUID!) {\n    document(query: { documentId: $documentId }) {\n      ...DocumentFields\n    }\n  }\n": typeof types.GetDocumentDocument,
    "\n  query GetDocumentVersions($documentId: UUID!, $skip: Int!, $take: Int!) {\n    documentVersions(query: { documentId: $documentId, skip: $skip, take: $take }) {\n      ...DocumentVersionFields\n    }\n  }\n": typeof types.GetDocumentVersionsDocument,
    "\n  query GetDocumentSignatures($documentId: UUID!) {\n    documentSignatures(query: { documentId: $documentId }) {\n      ...DocumentSignatureFields\n    }\n  }\n": typeof types.GetDocumentSignaturesDocument,
    "\n  query GetActiveDocumentByCategory(\n    $ownerEntityType: String!\n    $ownerEntityId: String!\n    $category: String!\n  ) {\n    activeDocumentByCategory(\n      query: { ownerEntityType: $ownerEntityType, ownerEntityId: $ownerEntityId, category: $category }\n    ) {\n      ...DocumentFields\n    }\n  }\n": typeof types.GetActiveDocumentByCategoryDocument,
    "\n  query SearchDocuments($filter: DocumentSearchFilterInput!, $skip: Int!, $take: Int!) {\n    searchDocuments(query: { filter: $filter, skip: $skip, take: $take }) {\n      ...DocumentFields\n    }\n  }\n": typeof types.SearchDocumentsDocument,
    "\n  query GetExpiringDocuments($withinDays: Int!, $skip: Int!, $take: Int!) {\n    expiringDocuments(query: { withinDays: $withinDays, skip: $skip, take: $take }) {\n      ...DocumentFields\n    }\n  }\n": typeof types.GetExpiringDocumentsDocument,
    "\n  query GetDocumentShares($documentId: UUID!) {\n    documentShares(query: { documentId: $documentId }) {\n      ...PublicLinkGrantFields\n    }\n  }\n": typeof types.GetDocumentSharesDocument,
    "\n  query GetDocumentTypes($accountId: UUID!, $includeDisabled: Boolean!) {\n    documentTypes(query: { accountId: $accountId, includeDisabled: $includeDisabled }) {\n      ...DocumentTypeFields\n    }\n  }\n": typeof types.GetDocumentTypesDocument,
    "\n  mutation VoidDocument($documentId: UUID!, $reason: String!) {\n    voidDocument(command: { documentId: $documentId, reason: $reason })\n  }\n": typeof types.VoidDocumentDocument,
    "\n  mutation ExpireDocument($documentId: UUID!, $expiresAt: DateTime!) {\n    expireDocument(command: { documentId: $documentId, expiresAt: $expiresAt })\n  }\n": typeof types.ExpireDocumentDocument,
    "\n  mutation DeleteDocumentReference($documentId: UUID!) {\n    deleteDocumentReference(command: { documentId: $documentId })\n  }\n": typeof types.DeleteDocumentReferenceDocument,
    "\n  mutation SignDocument($signature: DocumentSignatureDtoInput!) {\n    signDocument(command: { signature: $signature }) {\n      ...DocumentSignatureFields\n    }\n  }\n": typeof types.SignDocumentDocument,
    "\n  mutation ConfigureDocumentType($documentType: DocumentTypeDtoInput!) {\n    configureDocumentType(command: { documentType: $documentType }) {\n      ...DocumentTypeFields\n    }\n  }\n": typeof types.ConfigureDocumentTypeDocument,
    "\n  mutation DisableDocumentType($documentTypeId: UUID!) {\n    disableDocumentType(command: { documentTypeId: $documentTypeId })\n  }\n": typeof types.DisableDocumentTypeDocument,
    "\n  fragment DriverItem on DriverVm {\n    driverId\n    accountId\n    name\n    phone\n    documentType\n    documentNumber\n    active\n    employeeCode\n    licenseNumber\n    licenseExpiresAt\n    defaultTransporterId\n    lastModified\n  }\n": typeof types.DriverItemFragmentDoc,
    "\n  query GetDriversByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {\n    driversByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...DriverItem\n    }\n  }\n": typeof types.GetDriversByAccountDocument,
    "\n  mutation CreateDriver($driver: DriverDtoInput!) {\n    createDriver(command: { driver: $driver }) {\n      ...DriverItem\n    }\n  }\n": typeof types.CreateDriverDocument,
    "\n  mutation UpdateDriver($driverId: UUID!, $driver: DriverDtoInput!) {\n    updateDriver(command: { driverId: $driverId, driver: $driver })\n  }\n": typeof types.UpdateDriverDocument,
    "\n  mutation DeactivateDriver($driverId: UUID!) {\n    deactivateDriver(command: { driverId: $driverId })\n  }\n": typeof types.DeactivateDriverDocument,
    "\n  fragment GeocodingProviderItem on GeocodingProviderVm {\n    geocodingProviderId\n    name\n    type\n    endpointUri\n    apiKey\n    requestsPerSecond\n    timeoutSeconds\n    configurationJson\n    active\n  }\n": typeof types.GeocodingProviderItemFragmentDoc,
    "\n  query GetGeocodingProviders {\n    geocodingProviders {\n      ...GeocodingProviderItem\n    }\n  }\n": typeof types.GetGeocodingProvidersDocument,
    "\n  mutation CreateGeocodingProvider($geocodingProvider: GeocodingProviderDtoInput!) {\n    createGeocodingProvider(command: { geocodingProvider: $geocodingProvider }) {\n      ...GeocodingProviderItem\n    }\n  }\n": typeof types.CreateGeocodingProviderDocument,
    "\n  mutation UpdateGeocodingProvider($id: UUID!, $geocodingProvider: UpdateGeocodingProviderDtoInput!) {\n    updateGeocodingProvider(id: $id, command: { id: $id, geocodingProvider: $geocodingProvider })\n  }\n": typeof types.UpdateGeocodingProviderDocument,
    "\n  mutation DeleteGeocodingProvider($id: UUID!) {\n    deleteGeocodingProvider(id: $id)\n  }\n": typeof types.DeleteGeocodingProviderDocument,
    "\n  mutation SetActiveGeocodingProvider($id: UUID!) {\n    setActiveGeocodingProvider(id: $id)\n  }\n": typeof types.SetActiveGeocodingProviderDocument,
    "\n  query GetGpsIntegrationDashboard($accountId: UUID!) {\n    gpsIntegrationDashboard(query: { accountId: $accountId }) {\n      operatorsTotal\n      operatorsEnabled\n      operatorsHealthy\n      operatorsDegraded\n      operatorsOffline\n      devicesTotal\n      devicesNew\n      devicesAvailable\n      devicesAssigned\n      devicesIgnored\n      devicesRemoved\n      recentlyAddedDevicesLast24h\n      unassignedDevicesCount\n      syncRunsSucceededLast24h\n      syncRunsFailedLast24h\n      lastAutomaticSyncAt\n      lastManualSyncAt\n      averageSyncDurationSeconds\n      deviceCountsByProviderStatus {\n        operatorId\n        operatorName\n        detectedStatus\n        count\n      }\n    }\n  }\n": typeof types.GetGpsIntegrationDashboardDocument,
    "\n  fragment GroupItem on GroupVm {\n    groupId\n    name\n    description\n    active\n    accountId\n  }\n": typeof types.GroupItemFragmentDoc,
    "\n  query GetGroups {\n    groupsByAccount {\n      ...GroupItem\n    }\n  }\n": typeof types.GetGroupsDocument,
    "\n  mutation CreateGroup($group: GroupDtoInput!) {\n    createGroup(command: { group: $group }) {\n      ...GroupItem\n    }\n  }\n": typeof types.CreateGroupDocument,
    "\n  mutation UpdateGroup($id: Long!, $group: UpdateGroupDtoInput!) {\n    updateGroup(id: $id, command: { group: $group })\n  }\n": typeof types.UpdateGroupDocument,
    "\n  mutation DeleteGroup($id: Long!) {\n    deleteGroup(id: $id)\n  }\n": typeof types.DeleteGroupDocument,
    "\n  query GetUsersByGroup($groupId: Long!) {\n    usersByGroup(query: { groupId: $groupId }) {\n      userId\n      username\n      active\n      accountId\n    }\n  }\n": typeof types.GetUsersByGroupDocument,
    "\n  mutation CreateUserGroup($userGroup: UserGroupDtoInput!) {\n    createUserGroup(command: { userGroup: $userGroup }) {\n      userId\n      groupId\n    }\n  }\n": typeof types.CreateUserGroupDocument,
    "\n  mutation DeleteUserGroup($userId: UUID!, $groupId: Long!) {\n    deleteUserGroup(userId: $userId, groupId: $groupId)\n  }\n": typeof types.DeleteUserGroupDocument,
    "\n  mutation CreateTransporterGroup($transporterGroup: TransporterGroupDtoInput!) {\n    createTransporterGroup(command: { transporterGroup: $transporterGroup }) {\n      transporterId\n      groupId\n    }\n  }\n": typeof types.CreateTransporterGroupDocument,
    "\n  mutation DeleteTransporterGroup($transporterId: UUID!, $groupId: Long!) {\n    deleteTransporterGroup(transporterId: $transporterId, groupId: $groupId)\n  }\n": typeof types.DeleteTransporterGroupDocument,
    "\n  fragment NotificationDeliveryItem on NotificationDeliveryVm {\n    notificationDeliveryId\n    accountId\n    notificationRuleId\n    alertEventId\n    channel\n    recipientPrincipalType\n    recipient\n    status\n    attempts\n    providerMessageId\n    error\n    sentAt\n    readAt\n  }\n": typeof types.NotificationDeliveryItemFragmentDoc,
    "\n  query GetNotificationDeliveries(\n    $accountId: UUID!\n    $status: String\n    $channel: String\n    $from: DateTime\n    $to: DateTime\n    $skip: Int!\n    $take: Int!\n  ) {\n    notificationDeliveries(\n      query: {\n        accountId: $accountId\n        status: $status\n        channel: $channel\n        from: $from\n        to: $to\n        skip: $skip\n        take: $take\n      }\n    ) {\n      ...NotificationDeliveryItem\n    }\n  }\n": typeof types.GetNotificationDeliveriesDocument,
    "\n  query GetDeliveryHealth($accountId: UUID!, $from: DateTime!, $to: DateTime!) {\n    deliveryHealth(query: { accountId: $accountId, from: $from, to: $to }) {\n      channel\n      status\n      count\n      averageAttempts\n    }\n  }\n": typeof types.GetDeliveryHealthDocument,
    "\n  fragment MyNotificationItem on MyNotificationVm {\n    notificationDeliveryId\n    alertEventId\n    eventType\n    severity\n    sourceModule\n    resourceType\n    resourceId\n    payloadJson\n    createdAt\n    readAt\n  }\n": typeof types.MyNotificationItemFragmentDoc,
    "\n  query GetMyNotifications($unreadOnly: Boolean!, $skip: Int!, $take: Int!) {\n    myNotifications(query: { unreadOnly: $unreadOnly, skip: $skip, take: $take }) {\n      ...MyNotificationItem\n    }\n  }\n": typeof types.GetMyNotificationsDocument,
    "\n  mutation RetryNotificationDelivery($notificationDeliveryId: UUID!) {\n    retryNotificationDelivery(command: { notificationDeliveryId: $notificationDeliveryId })\n  }\n": typeof types.RetryNotificationDeliveryDocument,
    "\n  mutation MarkNotificationRead($notificationDeliveryId: UUID!) {\n    markNotificationRead(command: { notificationDeliveryId: $notificationDeliveryId })\n  }\n": typeof types.MarkNotificationReadDocument,
    "\n  mutation SendTestNotification($accountId: UUID!, $channel: String!, $contact: String) {\n    sendTestNotification(command: { accountId: $accountId, channel: $channel, contact: $contact }) {\n      notificationDeliveryId\n      channel\n      status\n    }\n  }\n": typeof types.SendTestNotificationDocument,
    "\n  fragment NotificationRuleItem on NotificationRuleVm {\n    notificationRuleId\n    accountId\n    ruleKey\n    ruleType\n    enabled\n    triggerEvent\n    recipientSelector\n    channelsJson\n    throttlingJson\n    configurationJson\n    lastModified\n  }\n": typeof types.NotificationRuleItemFragmentDoc,
    "\n  query GetNotificationRules($accountId: UUID!, $skip: Int!, $take: Int!) {\n    notificationRules(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...NotificationRuleItem\n    }\n  }\n": typeof types.GetNotificationRulesDocument,
    "\n  mutation CreateNotificationRule($notificationRule: NotificationRuleDtoInput!) {\n    createNotificationRule(command: { notificationRule: $notificationRule }) {\n      notificationRuleId\n      accountId\n      ruleKey\n      enabled\n      lastModified\n    }\n  }\n": typeof types.CreateNotificationRuleDocument,
    "\n  mutation UpdateNotificationRule(\n    $notificationRuleId: UUID!\n    $notificationRule: NotificationRuleDtoInput!\n  ) {\n    updateNotificationRule(\n      command: { notificationRuleId: $notificationRuleId, notificationRule: $notificationRule }\n    )\n  }\n": typeof types.UpdateNotificationRuleDocument,
    "\n  mutation DisableNotificationRule($notificationRuleId: UUID!) {\n    disableNotificationRule(command: { notificationRuleId: $notificationRuleId })\n  }\n": typeof types.DisableNotificationRuleDocument,
    "\n  fragment NotificationTemplateItem on NotificationTemplateVm {\n    notificationTemplateId\n    accountId\n    templateKey\n    channel\n    locale\n    subject\n    body\n    active\n    lastModified\n  }\n": typeof types.NotificationTemplateItemFragmentDoc,
    "\n  query GetNotificationTemplates($accountId: UUID!) {\n    notificationTemplates(query: { accountId: $accountId }) {\n      ...NotificationTemplateItem\n    }\n  }\n": typeof types.GetNotificationTemplatesDocument,
    "\n  mutation CreateNotificationTemplate($template: NotificationTemplateDtoInput!) {\n    createNotificationTemplate(command: { template: $template }) {\n      notificationTemplateId\n      accountId\n      templateKey\n      channel\n      locale\n      active\n    }\n  }\n": typeof types.CreateNotificationTemplateDocument,
    "\n  mutation UpdateNotificationTemplate(\n    $notificationTemplateId: UUID!\n    $template: NotificationTemplateDtoInput!\n  ) {\n    updateNotificationTemplate(\n      command: { notificationTemplateId: $notificationTemplateId, template: $template }\n    )\n  }\n": typeof types.UpdateNotificationTemplateDocument,
    "\n  mutation DeleteNotificationTemplate($notificationTemplateId: UUID!) {\n    deleteNotificationTemplate(command: { notificationTemplateId: $notificationTemplateId })\n  }\n": typeof types.DeleteNotificationTemplateDocument,
    "\n  fragment OperatorDetail on OperatorVm {\n    operatorId\n    name\n    description\n    phoneNumber\n    emailAddress\n    address\n    contactName\n    protocolType\n    protocolTypeId\n    enabled\n    syncIntervalMinutes\n    healthStatus\n    lastSuccessfulSyncAt\n    lastFailedSyncAt\n    lastFailureCode\n    lastLatencyMs\n    lastDeviceSyncAt\n    lastPositionSyncAt\n    lastModified\n  }\n": typeof types.OperatorDetailFragmentDoc,
    "\n  fragment OperatorSummary on OperatorVm {\n    operatorId\n    name\n  }\n": typeof types.OperatorSummaryFragmentDoc,
    "\n  fragment OperatorGps on OperatorVm {\n    operatorId\n    name\n    protocolType\n    enabled\n    lastDeviceSyncAt\n    lastPositionSyncAt\n    syncIntervalMinutes\n  }\n": typeof types.OperatorGpsFragmentDoc,
    "\n  query GetOperator($id: UUID!) {\n    operator(query: { id: $id }) {\n      ...OperatorDetail\n    }\n  }\n": typeof types.GetOperatorDocument,
    "\n  query GetOperatorsByCurrentAccount {\n    operatorsByCurrentAccount {\n      ...OperatorDetail\n    }\n  }\n": typeof types.GetOperatorsByCurrentAccountDocument,
    "\n  query GetOperatorsSummary {\n    operatorsByCurrentAccount {\n      ...OperatorSummary\n    }\n  }\n": typeof types.GetOperatorsSummaryDocument,
    "\n  query GetGpsOperators {\n    operatorsByCurrentAccount {\n      ...OperatorGps\n    }\n  }\n": typeof types.GetGpsOperatorsDocument,
    "\n  mutation CreateOperator($operator: OperatorDtoInput!) {\n    createOperator(command: { operator: $operator }) {\n      ...OperatorDetail\n    }\n  }\n": typeof types.CreateOperatorDocument,
    "\n  mutation UpdateOperator($id: UUID!, $operator: UpdateOperatorDtoInput!) {\n    updateOperator(id: $id, command: { operator: $operator })\n  }\n": typeof types.UpdateOperatorDocument,
    "\n  mutation DeleteOperator($id: UUID!) {\n    deleteOperator(id: $id)\n  }\n": typeof types.DeleteOperatorDocument,
    "\n  mutation SetOperatorEnabled($operatorId: UUID!, $enabled: Boolean!) {\n    setOperatorEnabled(command: { operatorId: $operatorId, enabled: $enabled })\n  }\n": typeof types.SetOperatorEnabledDocument,
    "\n  mutation TriggerOperatorDeviceSync($command: TriggerOperatorDeviceSyncCommandInput!) {\n    triggerOperatorDeviceSync(command: $command)\n  }\n": typeof types.TriggerOperatorDeviceSyncDocument,
    "\n  fragment PlatformAnnouncementItem on PlatformAnnouncementVm {\n    platformAnnouncementId\n    messageEn\n    messageEs\n    severity\n    startsAt\n    endsAt\n    active\n    lastModified\n  }\n": typeof types.PlatformAnnouncementItemFragmentDoc,
    "\n  query GetPlatformAnnouncements($skip: Int!, $take: Int!) {\n    platformAnnouncements(query: { skip: $skip, take: $take }) {\n      ...PlatformAnnouncementItem\n    }\n  }\n": typeof types.GetPlatformAnnouncementsDocument,
    "\n  query GetBackgroundJobStatus {\n    backgroundJobStatus {\n      jobKey\n      status\n      startedAt\n      completedAt\n      attempts\n      errorCode\n      recordsEveryCycle\n    }\n  }\n": typeof types.GetBackgroundJobStatusDocument,
    "\n  mutation CreatePlatformAnnouncement($announcement: PlatformAnnouncementDtoInput!) {\n    createPlatformAnnouncement(command: { announcement: $announcement }) {\n      ...PlatformAnnouncementItem\n    }\n  }\n": typeof types.CreatePlatformAnnouncementDocument,
    "\n  mutation UpdatePlatformAnnouncement($platformAnnouncementId: UUID!, $announcement: PlatformAnnouncementDtoInput!) {\n    updatePlatformAnnouncement(\n      command: { platformAnnouncementId: $platformAnnouncementId, announcement: $announcement }\n    )\n  }\n": typeof types.UpdatePlatformAnnouncementDocument,
    "\n  mutation DeletePlatformAnnouncement($platformAnnouncementId: UUID!) {\n    deletePlatformAnnouncement(command: { platformAnnouncementId: $platformAnnouncementId })\n  }\n": typeof types.DeletePlatformAnnouncementDocument,
    "\n  fragment PointOfInterestItem on PointOfInterestVm {\n    pointOfInterestId\n    accountId\n    name\n    description\n    type\n    latitude\n    longitude\n    address\n    color\n    groupId\n    active\n  }\n": typeof types.PointOfInterestItemFragmentDoc,
    "\n  query GetPointsOfInterestByAccount {\n    pointsOfInterestByAccount {\n      ...PointOfInterestItem\n    }\n  }\n": typeof types.GetPointsOfInterestByAccountDocument,
    "\n  mutation CreatePointOfInterest($pointOfInterest: PointOfInterestDtoInput!) {\n    createPointOfInterest(command: { pointOfInterest: $pointOfInterest }) {\n      ...PointOfInterestItem\n    }\n  }\n": typeof types.CreatePointOfInterestDocument,
    "\n  mutation UpdatePointOfInterest($id: UUID!, $pointOfInterest: UpdatePointOfInterestDtoInput!) {\n    updatePointOfInterest(id: $id, command: { id: $id, pointOfInterest: $pointOfInterest })\n  }\n": typeof types.UpdatePointOfInterestDocument,
    "\n  mutation DeletePointOfInterest($id: UUID!) {\n    deletePointOfInterest(id: $id)\n  }\n": typeof types.DeletePointOfInterestDocument,
    "\n  fragment CurrentPrincipalItem on CurrentPrincipalVm {\n    subjectId\n    principalType\n    userId\n    driverId\n    clientId\n    publicLinkGrantId\n    role\n    accountId\n    scopes\n    correlationId\n  }\n": typeof types.CurrentPrincipalItemFragmentDoc,
    "\n  query GetCurrentPrincipal {\n    currentPrincipal {\n      ...CurrentPrincipalItem\n    }\n  }\n": typeof types.GetCurrentPrincipalDocument,
    "\n  fragment PublicLinkGrantFields on PublicLinkGrantVm {\n    publicLinkGrantId\n    accountId\n    resourceType\n    resourceId\n    scopes\n    purpose\n    expiresAt\n    revokedAt\n    revokedBy\n    createdByPrincipalId\n    accessCount\n    lastAccessedAt\n    lastModified\n    token\n  }\n": typeof types.PublicLinkGrantFieldsFragmentDoc,
    "\n  query GetPublicLinkGrant($publicLinkGrantId: UUID!) {\n    publicLinkGrant(query: { publicLinkGrantId: $publicLinkGrantId }) {\n      ...PublicLinkGrantFields\n    }\n  }\n": typeof types.GetPublicLinkGrantDocument,
    "\n  query GetPublicLinkGrantsByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {\n    publicLinkGrantsByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...PublicLinkGrantFields\n    }\n  }\n": typeof types.GetPublicLinkGrantsByAccountDocument,
    "\n  mutation CreatePublicLinkGrant($publicLinkGrant: PublicLinkGrantDtoInput!) {\n    createPublicLinkGrant(command: { publicLinkGrant: $publicLinkGrant }) {\n      ...PublicLinkGrantFields\n    }\n  }\n": typeof types.CreatePublicLinkGrantDocument,
    "\n  mutation RevokePublicLinkGrant($publicLinkGrantId: UUID!, $revokedBy: String!) {\n    revokePublicLinkGrant(command: { publicLinkGrantId: $publicLinkGrantId, revokedBy: $revokedBy })\n  }\n": typeof types.RevokePublicLinkGrantDocument,
    "\n  fragment ReportFields on ReportVm {\n    reportId\n    code\n    description\n    type\n    typeId\n    active\n    category\n    requiredFeatureKey\n    managerOnly\n    supportsPdf\n    sortOrder\n  }\n": typeof types.ReportFieldsFragmentDoc,
    "\n  query GetReports {\n    reports {\n      ...ReportFields\n    }\n  }\n": typeof types.GetReportsDocument,
    "\n  mutation UpdateReport($id: UUID!, $report: UpdateReportDtoInput!) {\n    updateReport(id: $id, command: { report: $report })\n  }\n": typeof types.UpdateReportDocument,
    "\n  fragment AccountSettingsItem on AccountSettingsVm {\n    accountId\n    maps\n    mapsKey\n    onlineInterval\n    refreshMap\n    refreshMapInterval\n  }\n": typeof types.AccountSettingsItemFragmentDoc,
    "\n  fragment UserSettingsItem on UserSettingsVm {\n    userId\n    style\n    language\n    navbar\n  }\n": typeof types.UserSettingsItemFragmentDoc,
    "\n  query GetAccountSettingsByUser {\n    accountSettingsByUser {\n      ...AccountSettingsItem\n    }\n  }\n": typeof types.GetAccountSettingsByUserDocument,
    "\n  query GetUserSettings {\n    userSettings {\n      ...UserSettingsItem\n    }\n  }\n": typeof types.GetUserSettingsDocument,
    "\n  mutation UpdateAccountSettings($id: UUID!, $accountSettings: AccountSettingsDtoInput!) {\n    updateAccountSettings(id: $id, command: { accountSettings: $accountSettings })\n  }\n": typeof types.UpdateAccountSettingsDocument,
    "\n  mutation UpdateUserSettings($id: UUID!, $userSettings: UserSettingsDtoInput!) {\n    updateUserSettings(id: $id, command: { userSettings: $userSettings })\n  }\n": typeof types.UpdateUserSettingsDocument,
    "\n  fragment AccountSupportGrantItem on AccountSupportGrantVm {\n    accountSupportGrantId\n    accountId\n    supportUserId\n    reason\n    ticketReference\n    approvedBy\n    approvedAt\n    accessLevel\n    startsAt\n    endsAt\n    revokedAt\n    revokedBy\n    lastModified\n  }\n": typeof types.AccountSupportGrantItemFragmentDoc,
    "\n  query GetSupportGrantStatus($accountSupportGrantId: UUID!) {\n    supportGrantStatus(query: { accountSupportGrantId: $accountSupportGrantId }) {\n      ...AccountSupportGrantItem\n    }\n  }\n": typeof types.GetSupportGrantStatusDocument,
    "\n  query GetAccountSupportGrants($accountId: UUID, $skip: Int!, $take: Int!) {\n    accountSupportGrants(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AccountSupportGrantItem\n    }\n  }\n": typeof types.GetAccountSupportGrantsDocument,
    "\n  mutation CreateAccountSupportGrant($accountSupportGrant: AccountSupportGrantDtoInput!) {\n    createAccountSupportGrant(command: { accountSupportGrant: $accountSupportGrant }) {\n      ...AccountSupportGrantItem\n    }\n  }\n": typeof types.CreateAccountSupportGrantDocument,
    "\n  mutation ApproveAccountSupportGrant($accountSupportGrantId: UUID!, $approvedBy: String!) {\n    approveAccountSupportGrant(\n      command: { accountSupportGrantId: $accountSupportGrantId, approvedBy: $approvedBy }\n    )\n  }\n": typeof types.ApproveAccountSupportGrantDocument,
    "\n  mutation RevokeAccountSupportGrant($accountSupportGrantId: UUID!, $revokedBy: String!) {\n    revokeAccountSupportGrant(\n      command: { accountSupportGrantId: $accountSupportGrantId, revokedBy: $revokedBy }\n    )\n  }\n": typeof types.RevokeAccountSupportGrantDocument,
    "\n  fragment TransporterItem on TransporterVm {\n    transporterId\n    name\n    transporterType\n    transporterTypeId\n  }\n": typeof types.TransporterItemFragmentDoc,
    "\n  query GetTransporter($id: UUID!) {\n    transporter(query: { id: $id }) {\n      ...TransporterItem\n    }\n  }\n": typeof types.GetTransporterDocument,
    "\n  query GetTransportersByAccount {\n    transportersByAccount {\n      ...TransporterItem\n    }\n  }\n": typeof types.GetTransportersByAccountDocument,
    "\n  query GetTransportersByUser {\n    transportersByUser {\n      ...TransporterItem\n    }\n  }\n": typeof types.GetTransportersByUserDocument,
    "\n  query GetTransportersByGroup($groupId: Long!) {\n    transportersByGroup(query: { groupId: $groupId }) {\n      ...TransporterItem\n    }\n  }\n": typeof types.GetTransportersByGroupDocument,
    "\n  mutation CreateTransporter($transporter: TransporterDtoInput!) {\n    createTransporter(command: { transporter: $transporter }) {\n      ...TransporterItem\n    }\n  }\n": typeof types.CreateTransporterDocument,
    "\n  mutation UpdateTransporter($id: UUID!, $transporter: UpdateTransporterDtoInput!) {\n    updateTransporter(id: $id, command: { transporter: $transporter })\n  }\n": typeof types.UpdateTransporterDocument,
    "\n  mutation DeleteTransporter($id: UUID!) {\n    deleteTransporter(id: $id)\n  }\n": typeof types.DeleteTransporterDocument,
    "\n  fragment AssignmentFields on TransporterDeviceAssignmentVm {\n    transporterDeviceAssignmentId\n    accountId\n    transporterId\n    deviceId\n    effectiveFrom\n    effectiveTo\n    priority\n    isPrimary\n    status\n    assignmentReason\n  }\n": typeof types.AssignmentFieldsFragmentDoc,
    "\n  query GetTransporterDeviceAssignmentsByAccount($accountId: UUID!, $activeOnly: Boolean!) {\n    transporterDeviceAssignmentsByAccount(query: { accountId: $accountId, activeOnly: $activeOnly }) {\n      ...AssignmentFields\n      createdByPrincipalType\n      createdByPrincipalId\n    }\n  }\n": typeof types.GetTransporterDeviceAssignmentsByAccountDocument,
    "\n  query GetTransporterDeviceAssignmentsByTransporter($transporterId: UUID!, $activeOnly: Boolean!) {\n    transporterDeviceAssignmentsByTransporter(\n      query: { transporterId: $transporterId, activeOnly: $activeOnly }\n    ) {\n      ...AssignmentFields\n    }\n  }\n": typeof types.GetTransporterDeviceAssignmentsByTransporterDocument,
    "\n  mutation AssignDeviceToTransporter($assignment: TransporterDeviceAssignmentDtoInput!) {\n    assignDeviceToTransporter(command: { assignment: $assignment }) {\n      transporterDeviceAssignmentId\n      deviceId\n      transporterId\n      effectiveFrom\n      isPrimary\n      status\n    }\n  }\n": typeof types.AssignDeviceToTransporterDocument,
    "\n  mutation EndDeviceTransporterAssignment($assignmentId: UUID!, $reason: String) {\n    endDeviceTransporterAssignment(command: { assignmentId: $assignmentId, reason: $reason })\n  }\n": typeof types.EndDeviceTransporterAssignmentDocument,
    "\n  fragment TransporterTypeItem on TransporterTypeVm {\n    transporterTypeId\n    accBased\n    stoppedGap\n    maxDistance\n    maxTimeGap\n    type\n  }\n": typeof types.TransporterTypeItemFragmentDoc,
    "\n  query GetTransporterTypes {\n    transporterTypes {\n      ...TransporterTypeItem\n    }\n  }\n": typeof types.GetTransporterTypesDocument,
    "\n  mutation UpdateTransporterType($id: Short!, $transporterType: TransporterTypeDtoInput!) {\n    updateTransporterType(id: $id, command: { transporterType: $transporterType })\n  }\n": typeof types.UpdateTransporterTypeDocument,
};
const documents: Documents = {
    "\n  fragment AccountFeatureItem on AccountFeatureVm {\n    accountFeatureId\n    accountId\n    featureKey\n    enabled\n    tier\n    source\n    effectiveFrom\n    effectiveTo\n    configurationJson\n    lastModified\n  }\n": types.AccountFeatureItemFragmentDoc,
    "\n  query GetAccountFeatures($accountId: UUID!) {\n    accountFeatures(query: { accountId: $accountId }) {\n      ...AccountFeatureItem\n    }\n  }\n": types.GetAccountFeaturesDocument,
    "\n  mutation SetAccountFeature($feature: AccountFeatureDtoInput!) {\n    setAccountFeature(command: { feature: $feature }) {\n      ...AccountFeatureItem\n    }\n  }\n": types.SetAccountFeatureDocument,
    "\n  query GetAccountFeaturesMaster($accountId: UUID!) {\n    accountFeaturesMaster(query: { accountId: $accountId }) {\n      ...AccountFeatureItem\n    }\n  }\n": types.GetAccountFeaturesMasterDocument,
    "\n  mutation SetAccountFeatureMaster($feature: AccountFeatureDtoInput!) {\n    setAccountFeatureMaster(command: { feature: $feature }) {\n      ...AccountFeatureItem\n    }\n  }\n": types.SetAccountFeatureMasterDocument,
    "\n  fragment AccountItem on AccountVm {\n    accountId\n    name\n    description\n    type\n    typeId\n    status\n    statusId\n    active\n    lastModified\n  }\n": types.AccountItemFragmentDoc,
    "\n  query GetAccount($id: UUID!) {\n    account(query: { id: $id }) {\n      ...AccountItem\n    }\n  }\n": types.GetAccountDocument,
    "\n  query GetAccountByUser {\n    accountByUser {\n      ...AccountItem\n    }\n  }\n": types.GetAccountByUserDocument,
    "\n  query GetAccounts {\n    accounts {\n      ...AccountItem\n    }\n  }\n": types.GetAccountsDocument,
    "\n  mutation CreateAccount($account: AccountDtoInput!) {\n    createAccount(command: { account: $account }) {\n      ...AccountItem\n    }\n  }\n": types.CreateAccountDocument,
    "\n  mutation UpdateAccount($id: UUID!, $account: UpdateAccountDtoInput!) {\n    updateAccount(id: $id, command: { account: $account })\n  }\n": types.UpdateAccountDocument,
    "\n  mutation ChangeAccountStatus($accountId: UUID!, $targetStatus: AccountStatus!, $reason: String) {\n    changeAccountStatus(\n      command: { accountId: $accountId, targetStatus: $targetStatus, reason: $reason }\n    ) {\n      ...AccountItem\n    }\n  }\n": types.ChangeAccountStatusDocument,
    "\n  query GetAccountContext {\n    accountContext {\n      status\n      statusId\n      branding {\n        accountId\n        displayName\n        logoDocumentId\n        primaryColor\n        reportHeader\n        lastModified\n      }\n      features {\n        accountFeatureId\n        accountId\n        featureKey\n        enabled\n        tier\n        source\n        effectiveFrom\n        effectiveTo\n        configurationJson\n        lastModified\n      }\n    }\n  }\n": types.GetAccountContextDocument,
    "\n  fragment AlertEventItem on AlertEventVm {\n    alertEventId\n    accountId\n    eventType\n    severity\n    sourceModule\n    resourceType\n    resourceId\n    status\n    firstSeenAt\n    lastSeenAt\n    deduplicationKey\n  }\n": types.AlertEventItemFragmentDoc,
    "\n  query GetAlertEvents($accountId: UUID!, $skip: Int!, $take: Int!) {\n    alertEvents(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AlertEventItem\n    }\n  }\n": types.GetAlertEventsDocument,
    "\n  mutation AcknowledgeAlertEvent($alertEventId: UUID!) {\n    acknowledgeAlertEvent(command: { alertEventId: $alertEventId })\n  }\n": types.AcknowledgeAlertEventDocument,
    "\n  mutation ResolveAlertEvent($alertEventId: UUID!) {\n    resolveAlertEvent(command: { alertEventId: $alertEventId })\n  }\n": types.ResolveAlertEventDocument,
    "\n  fragment AlertSubscriptionItem on AlertSubscriptionVm {\n    alertSubscriptionId\n    accountId\n    principalType\n    principalId\n    eventTypeFilter\n    channel\n    contact\n    enabled\n    lastModified\n  }\n": types.AlertSubscriptionItemFragmentDoc,
    "\n  query GetAlertSubscriptions($accountId: UUID!, $principalId: UUID, $skip: Int!, $take: Int!) {\n    alertSubscriptions(\n      query: { accountId: $accountId, principalId: $principalId, skip: $skip, take: $take }\n    ) {\n      ...AlertSubscriptionItem\n    }\n  }\n": types.GetAlertSubscriptionsDocument,
    "\n  mutation CreateAlertSubscription($subscription: AlertSubscriptionDtoInput!) {\n    createAlertSubscription(command: { subscription: $subscription }) {\n      alertSubscriptionId\n      accountId\n      principalType\n      principalId\n      channel\n      enabled\n    }\n  }\n": types.CreateAlertSubscriptionDocument,
    "\n  mutation UpdateAlertSubscription(\n    $alertSubscriptionId: UUID!\n    $subscription: AlertSubscriptionDtoInput!\n  ) {\n    updateAlertSubscription(\n      command: { alertSubscriptionId: $alertSubscriptionId, subscription: $subscription }\n    )\n  }\n": types.UpdateAlertSubscriptionDocument,
    "\n  mutation DeleteAlertSubscription($alertSubscriptionId: UUID!) {\n    deleteAlertSubscription(command: { alertSubscriptionId: $alertSubscriptionId })\n  }\n": types.DeleteAlertSubscriptionDocument,
    "\n  fragment AuditEventItem on AuditEventVm {\n    auditEventId\n    accountId\n    actorType\n    actorId\n    action\n    resourceType\n    resourceId\n    result\n    reason\n    correlationId\n    occurredAt\n  }\n": types.AuditEventItemFragmentDoc,
    "\n  query GetAuditTrail($accountId: UUID!, $skip: Int!, $take: Int!) {\n    auditTrail(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AuditEventItem\n    }\n  }\n": types.GetAuditTrailDocument,
    "\n  fragment BackgroundJobRunItem on BackgroundJobRunVm {\n    backgroundJobRunId\n    jobKey\n    accountId\n    resourceKey\n    idempotencyKey\n    status\n    attempts\n    startedAt\n    completedAt\n    errorCode\n    errorMessage\n  }\n": types.BackgroundJobRunItemFragmentDoc,
    "\n  query GetBackgroundJobRuns($accountId: UUID!, $skip: Int!, $take: Int!) {\n    backgroundJobRuns(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...BackgroundJobRunItem\n    }\n  }\n": types.GetBackgroundJobRunsDocument,
    "\n  fragment AccountBrandingItem on AccountBrandingVm {\n    accountId\n    displayName\n    logoDocumentId\n    primaryColor\n    reportHeader\n    lastModified\n  }\n": types.AccountBrandingItemFragmentDoc,
    "\n  query GetAccountBranding($accountId: UUID!) {\n    accountBranding(query: { accountId: $accountId }) {\n      ...AccountBrandingItem\n    }\n  }\n": types.GetAccountBrandingDocument,
    "\n  mutation UpdateAccountBranding($branding: AccountBrandingDtoInput!) {\n    updateAccountBranding(command: { branding: $branding }) {\n      ...AccountBrandingItem\n    }\n  }\n": types.UpdateAccountBrandingDocument,
    "\n  fragment CredentialFields on CredentialVm {\n    credentialId\n    key\n    key2\n    password\n    uri\n    username\n  }\n": types.CredentialFieldsFragmentDoc,
    "\n  query GetCredentialByOperator($operatorId: UUID!) {\n    credentialByOperator(query: { operatorId: $operatorId }) {\n      ...CredentialFields\n    }\n  }\n": types.GetCredentialByOperatorDocument,
    "\n  mutation CreateCredential($credential: CredentialDtoInput!) {\n    createCredential(command: { credential: $credential }) {\n      ...CredentialFields\n    }\n  }\n": types.CreateCredentialDocument,
    "\n  mutation UpdateCredential($id: UUID!, $credential: UpdateCredentialDtoInput!) {\n    updateCredential(id: $id, command: { credential: $credential })\n  }\n": types.UpdateCredentialDocument,
    "\n  fragment DeviceItem on DeviceVm {\n    deviceId\n    name\n    serial\n    description\n    deviceType\n    deviceTypeId\n    identifier\n    operatorId\n  }\n": types.DeviceItemFragmentDoc,
    "\n  fragment SynchronizedDevice on DeviceVm {\n    deviceId\n    accountId\n    operatorId\n    serial\n    name\n    identifier\n    providerDisplayName\n    providerStatus\n    detectedStatus\n    firstSeenAt\n    lastSeenAt\n    lastSyncedAt\n    lastAssignedAt\n    ignoredAt\n  }\n": types.SynchronizedDeviceFragmentDoc,
    "\n  query GetDevicesByAccount {\n    devicesByAccount {\n      ...DeviceItem\n    }\n  }\n": types.GetDevicesByAccountDocument,
    "\n  mutation DeleteDevice($deviceId: UUID!) {\n    deleteDevice(deviceId: $deviceId)\n  }\n": types.DeleteDeviceDocument,
    "\n  query GetSynchronizedDevices($accountId: UUID!, $detectedStatus: DetectedStatus, $operatorId: UUID) {\n    synchronizedDevices(\n      query: { accountId: $accountId, detectedStatus: $detectedStatus, operatorId: $operatorId }\n    ) {\n      ...SynchronizedDevice\n    }\n  }\n": types.GetSynchronizedDevicesDocument,
    "\n  query GetUnassignedSynchronizedDevices($accountId: UUID!) {\n    unassignedSynchronizedDevices(query: { accountId: $accountId }) {\n      ...SynchronizedDevice\n    }\n  }\n": types.GetUnassignedSynchronizedDevicesDocument,
    "\n  mutation SetSynchronizedDeviceIgnored($deviceId: UUID!, $ignored: Boolean!) {\n    setSynchronizedDeviceIgnored(command: { deviceId: $deviceId, ignored: $ignored })\n  }\n": types.SetSynchronizedDeviceIgnoredDocument,
    "\n  fragment DocumentFields on DocumentVm {\n    documentId\n    accountId\n    ownerEntityType\n    ownerEntityId\n    uploadedByPrincipalType\n    uploadedByPrincipalId\n    fileName\n    category\n    title\n    description\n    contentType\n    sizeBytes\n    sha256Hash\n    classification\n    status\n    expiresAt\n    visibilityScope\n    scanStatus\n    currentVersion\n    downloadUrl\n    lastModified\n  }\n": types.DocumentFieldsFragmentDoc,
    "\n  fragment DocumentVersionFields on DocumentVersionVm {\n    documentVersionId\n    documentId\n    accountId\n    versionNumber\n    contentType\n    fileName\n    sizeBytes\n    sha256Hash\n    scanStatus\n    replacedByPrincipalType\n    replacedByPrincipalId\n    reason\n    createdAt\n  }\n": types.DocumentVersionFieldsFragmentDoc,
    "\n  fragment DocumentTypeFields on DocumentTypeVm {\n    documentTypeId\n    accountId\n    category\n    displayName\n    required\n    expiring\n    defaultValidityDays\n    enabled\n    createdAt\n  }\n": types.DocumentTypeFieldsFragmentDoc,
    "\n  fragment DocumentSignatureFields on DocumentSignatureVm {\n    documentSignatureId\n    documentId\n    accountId\n    signerPrincipalType\n    signerPrincipalId\n    signerName\n    signatureImageDocumentId\n    legalTextAccepted\n    latitude\n    longitude\n    signedAt\n    createdAt\n  }\n": types.DocumentSignatureFieldsFragmentDoc,
    "\n  query GetDocumentsForOwner(\n    $accountId: UUID!\n    $ownerEntityType: String!\n    $ownerEntityId: String!\n    $skip: Int!\n    $take: Int!\n  ) {\n    documentsForOwner(\n      query: {\n        accountId: $accountId\n        ownerEntityType: $ownerEntityType\n        ownerEntityId: $ownerEntityId\n        skip: $skip\n        take: $take\n      }\n    ) {\n      ...DocumentFields\n    }\n  }\n": types.GetDocumentsForOwnerDocument,
    "\n  query GetDocument($documentId: UUID!) {\n    document(query: { documentId: $documentId }) {\n      ...DocumentFields\n    }\n  }\n": types.GetDocumentDocument,
    "\n  query GetDocumentVersions($documentId: UUID!, $skip: Int!, $take: Int!) {\n    documentVersions(query: { documentId: $documentId, skip: $skip, take: $take }) {\n      ...DocumentVersionFields\n    }\n  }\n": types.GetDocumentVersionsDocument,
    "\n  query GetDocumentSignatures($documentId: UUID!) {\n    documentSignatures(query: { documentId: $documentId }) {\n      ...DocumentSignatureFields\n    }\n  }\n": types.GetDocumentSignaturesDocument,
    "\n  query GetActiveDocumentByCategory(\n    $ownerEntityType: String!\n    $ownerEntityId: String!\n    $category: String!\n  ) {\n    activeDocumentByCategory(\n      query: { ownerEntityType: $ownerEntityType, ownerEntityId: $ownerEntityId, category: $category }\n    ) {\n      ...DocumentFields\n    }\n  }\n": types.GetActiveDocumentByCategoryDocument,
    "\n  query SearchDocuments($filter: DocumentSearchFilterInput!, $skip: Int!, $take: Int!) {\n    searchDocuments(query: { filter: $filter, skip: $skip, take: $take }) {\n      ...DocumentFields\n    }\n  }\n": types.SearchDocumentsDocument,
    "\n  query GetExpiringDocuments($withinDays: Int!, $skip: Int!, $take: Int!) {\n    expiringDocuments(query: { withinDays: $withinDays, skip: $skip, take: $take }) {\n      ...DocumentFields\n    }\n  }\n": types.GetExpiringDocumentsDocument,
    "\n  query GetDocumentShares($documentId: UUID!) {\n    documentShares(query: { documentId: $documentId }) {\n      ...PublicLinkGrantFields\n    }\n  }\n": types.GetDocumentSharesDocument,
    "\n  query GetDocumentTypes($accountId: UUID!, $includeDisabled: Boolean!) {\n    documentTypes(query: { accountId: $accountId, includeDisabled: $includeDisabled }) {\n      ...DocumentTypeFields\n    }\n  }\n": types.GetDocumentTypesDocument,
    "\n  mutation VoidDocument($documentId: UUID!, $reason: String!) {\n    voidDocument(command: { documentId: $documentId, reason: $reason })\n  }\n": types.VoidDocumentDocument,
    "\n  mutation ExpireDocument($documentId: UUID!, $expiresAt: DateTime!) {\n    expireDocument(command: { documentId: $documentId, expiresAt: $expiresAt })\n  }\n": types.ExpireDocumentDocument,
    "\n  mutation DeleteDocumentReference($documentId: UUID!) {\n    deleteDocumentReference(command: { documentId: $documentId })\n  }\n": types.DeleteDocumentReferenceDocument,
    "\n  mutation SignDocument($signature: DocumentSignatureDtoInput!) {\n    signDocument(command: { signature: $signature }) {\n      ...DocumentSignatureFields\n    }\n  }\n": types.SignDocumentDocument,
    "\n  mutation ConfigureDocumentType($documentType: DocumentTypeDtoInput!) {\n    configureDocumentType(command: { documentType: $documentType }) {\n      ...DocumentTypeFields\n    }\n  }\n": types.ConfigureDocumentTypeDocument,
    "\n  mutation DisableDocumentType($documentTypeId: UUID!) {\n    disableDocumentType(command: { documentTypeId: $documentTypeId })\n  }\n": types.DisableDocumentTypeDocument,
    "\n  fragment DriverItem on DriverVm {\n    driverId\n    accountId\n    name\n    phone\n    documentType\n    documentNumber\n    active\n    employeeCode\n    licenseNumber\n    licenseExpiresAt\n    defaultTransporterId\n    lastModified\n  }\n": types.DriverItemFragmentDoc,
    "\n  query GetDriversByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {\n    driversByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...DriverItem\n    }\n  }\n": types.GetDriversByAccountDocument,
    "\n  mutation CreateDriver($driver: DriverDtoInput!) {\n    createDriver(command: { driver: $driver }) {\n      ...DriverItem\n    }\n  }\n": types.CreateDriverDocument,
    "\n  mutation UpdateDriver($driverId: UUID!, $driver: DriverDtoInput!) {\n    updateDriver(command: { driverId: $driverId, driver: $driver })\n  }\n": types.UpdateDriverDocument,
    "\n  mutation DeactivateDriver($driverId: UUID!) {\n    deactivateDriver(command: { driverId: $driverId })\n  }\n": types.DeactivateDriverDocument,
    "\n  fragment GeocodingProviderItem on GeocodingProviderVm {\n    geocodingProviderId\n    name\n    type\n    endpointUri\n    apiKey\n    requestsPerSecond\n    timeoutSeconds\n    configurationJson\n    active\n  }\n": types.GeocodingProviderItemFragmentDoc,
    "\n  query GetGeocodingProviders {\n    geocodingProviders {\n      ...GeocodingProviderItem\n    }\n  }\n": types.GetGeocodingProvidersDocument,
    "\n  mutation CreateGeocodingProvider($geocodingProvider: GeocodingProviderDtoInput!) {\n    createGeocodingProvider(command: { geocodingProvider: $geocodingProvider }) {\n      ...GeocodingProviderItem\n    }\n  }\n": types.CreateGeocodingProviderDocument,
    "\n  mutation UpdateGeocodingProvider($id: UUID!, $geocodingProvider: UpdateGeocodingProviderDtoInput!) {\n    updateGeocodingProvider(id: $id, command: { id: $id, geocodingProvider: $geocodingProvider })\n  }\n": types.UpdateGeocodingProviderDocument,
    "\n  mutation DeleteGeocodingProvider($id: UUID!) {\n    deleteGeocodingProvider(id: $id)\n  }\n": types.DeleteGeocodingProviderDocument,
    "\n  mutation SetActiveGeocodingProvider($id: UUID!) {\n    setActiveGeocodingProvider(id: $id)\n  }\n": types.SetActiveGeocodingProviderDocument,
    "\n  query GetGpsIntegrationDashboard($accountId: UUID!) {\n    gpsIntegrationDashboard(query: { accountId: $accountId }) {\n      operatorsTotal\n      operatorsEnabled\n      operatorsHealthy\n      operatorsDegraded\n      operatorsOffline\n      devicesTotal\n      devicesNew\n      devicesAvailable\n      devicesAssigned\n      devicesIgnored\n      devicesRemoved\n      recentlyAddedDevicesLast24h\n      unassignedDevicesCount\n      syncRunsSucceededLast24h\n      syncRunsFailedLast24h\n      lastAutomaticSyncAt\n      lastManualSyncAt\n      averageSyncDurationSeconds\n      deviceCountsByProviderStatus {\n        operatorId\n        operatorName\n        detectedStatus\n        count\n      }\n    }\n  }\n": types.GetGpsIntegrationDashboardDocument,
    "\n  fragment GroupItem on GroupVm {\n    groupId\n    name\n    description\n    active\n    accountId\n  }\n": types.GroupItemFragmentDoc,
    "\n  query GetGroups {\n    groupsByAccount {\n      ...GroupItem\n    }\n  }\n": types.GetGroupsDocument,
    "\n  mutation CreateGroup($group: GroupDtoInput!) {\n    createGroup(command: { group: $group }) {\n      ...GroupItem\n    }\n  }\n": types.CreateGroupDocument,
    "\n  mutation UpdateGroup($id: Long!, $group: UpdateGroupDtoInput!) {\n    updateGroup(id: $id, command: { group: $group })\n  }\n": types.UpdateGroupDocument,
    "\n  mutation DeleteGroup($id: Long!) {\n    deleteGroup(id: $id)\n  }\n": types.DeleteGroupDocument,
    "\n  query GetUsersByGroup($groupId: Long!) {\n    usersByGroup(query: { groupId: $groupId }) {\n      userId\n      username\n      active\n      accountId\n    }\n  }\n": types.GetUsersByGroupDocument,
    "\n  mutation CreateUserGroup($userGroup: UserGroupDtoInput!) {\n    createUserGroup(command: { userGroup: $userGroup }) {\n      userId\n      groupId\n    }\n  }\n": types.CreateUserGroupDocument,
    "\n  mutation DeleteUserGroup($userId: UUID!, $groupId: Long!) {\n    deleteUserGroup(userId: $userId, groupId: $groupId)\n  }\n": types.DeleteUserGroupDocument,
    "\n  mutation CreateTransporterGroup($transporterGroup: TransporterGroupDtoInput!) {\n    createTransporterGroup(command: { transporterGroup: $transporterGroup }) {\n      transporterId\n      groupId\n    }\n  }\n": types.CreateTransporterGroupDocument,
    "\n  mutation DeleteTransporterGroup($transporterId: UUID!, $groupId: Long!) {\n    deleteTransporterGroup(transporterId: $transporterId, groupId: $groupId)\n  }\n": types.DeleteTransporterGroupDocument,
    "\n  fragment NotificationDeliveryItem on NotificationDeliveryVm {\n    notificationDeliveryId\n    accountId\n    notificationRuleId\n    alertEventId\n    channel\n    recipientPrincipalType\n    recipient\n    status\n    attempts\n    providerMessageId\n    error\n    sentAt\n    readAt\n  }\n": types.NotificationDeliveryItemFragmentDoc,
    "\n  query GetNotificationDeliveries(\n    $accountId: UUID!\n    $status: String\n    $channel: String\n    $from: DateTime\n    $to: DateTime\n    $skip: Int!\n    $take: Int!\n  ) {\n    notificationDeliveries(\n      query: {\n        accountId: $accountId\n        status: $status\n        channel: $channel\n        from: $from\n        to: $to\n        skip: $skip\n        take: $take\n      }\n    ) {\n      ...NotificationDeliveryItem\n    }\n  }\n": types.GetNotificationDeliveriesDocument,
    "\n  query GetDeliveryHealth($accountId: UUID!, $from: DateTime!, $to: DateTime!) {\n    deliveryHealth(query: { accountId: $accountId, from: $from, to: $to }) {\n      channel\n      status\n      count\n      averageAttempts\n    }\n  }\n": types.GetDeliveryHealthDocument,
    "\n  fragment MyNotificationItem on MyNotificationVm {\n    notificationDeliveryId\n    alertEventId\n    eventType\n    severity\n    sourceModule\n    resourceType\n    resourceId\n    payloadJson\n    createdAt\n    readAt\n  }\n": types.MyNotificationItemFragmentDoc,
    "\n  query GetMyNotifications($unreadOnly: Boolean!, $skip: Int!, $take: Int!) {\n    myNotifications(query: { unreadOnly: $unreadOnly, skip: $skip, take: $take }) {\n      ...MyNotificationItem\n    }\n  }\n": types.GetMyNotificationsDocument,
    "\n  mutation RetryNotificationDelivery($notificationDeliveryId: UUID!) {\n    retryNotificationDelivery(command: { notificationDeliveryId: $notificationDeliveryId })\n  }\n": types.RetryNotificationDeliveryDocument,
    "\n  mutation MarkNotificationRead($notificationDeliveryId: UUID!) {\n    markNotificationRead(command: { notificationDeliveryId: $notificationDeliveryId })\n  }\n": types.MarkNotificationReadDocument,
    "\n  mutation SendTestNotification($accountId: UUID!, $channel: String!, $contact: String) {\n    sendTestNotification(command: { accountId: $accountId, channel: $channel, contact: $contact }) {\n      notificationDeliveryId\n      channel\n      status\n    }\n  }\n": types.SendTestNotificationDocument,
    "\n  fragment NotificationRuleItem on NotificationRuleVm {\n    notificationRuleId\n    accountId\n    ruleKey\n    ruleType\n    enabled\n    triggerEvent\n    recipientSelector\n    channelsJson\n    throttlingJson\n    configurationJson\n    lastModified\n  }\n": types.NotificationRuleItemFragmentDoc,
    "\n  query GetNotificationRules($accountId: UUID!, $skip: Int!, $take: Int!) {\n    notificationRules(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...NotificationRuleItem\n    }\n  }\n": types.GetNotificationRulesDocument,
    "\n  mutation CreateNotificationRule($notificationRule: NotificationRuleDtoInput!) {\n    createNotificationRule(command: { notificationRule: $notificationRule }) {\n      notificationRuleId\n      accountId\n      ruleKey\n      enabled\n      lastModified\n    }\n  }\n": types.CreateNotificationRuleDocument,
    "\n  mutation UpdateNotificationRule(\n    $notificationRuleId: UUID!\n    $notificationRule: NotificationRuleDtoInput!\n  ) {\n    updateNotificationRule(\n      command: { notificationRuleId: $notificationRuleId, notificationRule: $notificationRule }\n    )\n  }\n": types.UpdateNotificationRuleDocument,
    "\n  mutation DisableNotificationRule($notificationRuleId: UUID!) {\n    disableNotificationRule(command: { notificationRuleId: $notificationRuleId })\n  }\n": types.DisableNotificationRuleDocument,
    "\n  fragment NotificationTemplateItem on NotificationTemplateVm {\n    notificationTemplateId\n    accountId\n    templateKey\n    channel\n    locale\n    subject\n    body\n    active\n    lastModified\n  }\n": types.NotificationTemplateItemFragmentDoc,
    "\n  query GetNotificationTemplates($accountId: UUID!) {\n    notificationTemplates(query: { accountId: $accountId }) {\n      ...NotificationTemplateItem\n    }\n  }\n": types.GetNotificationTemplatesDocument,
    "\n  mutation CreateNotificationTemplate($template: NotificationTemplateDtoInput!) {\n    createNotificationTemplate(command: { template: $template }) {\n      notificationTemplateId\n      accountId\n      templateKey\n      channel\n      locale\n      active\n    }\n  }\n": types.CreateNotificationTemplateDocument,
    "\n  mutation UpdateNotificationTemplate(\n    $notificationTemplateId: UUID!\n    $template: NotificationTemplateDtoInput!\n  ) {\n    updateNotificationTemplate(\n      command: { notificationTemplateId: $notificationTemplateId, template: $template }\n    )\n  }\n": types.UpdateNotificationTemplateDocument,
    "\n  mutation DeleteNotificationTemplate($notificationTemplateId: UUID!) {\n    deleteNotificationTemplate(command: { notificationTemplateId: $notificationTemplateId })\n  }\n": types.DeleteNotificationTemplateDocument,
    "\n  fragment OperatorDetail on OperatorVm {\n    operatorId\n    name\n    description\n    phoneNumber\n    emailAddress\n    address\n    contactName\n    protocolType\n    protocolTypeId\n    enabled\n    syncIntervalMinutes\n    healthStatus\n    lastSuccessfulSyncAt\n    lastFailedSyncAt\n    lastFailureCode\n    lastLatencyMs\n    lastDeviceSyncAt\n    lastPositionSyncAt\n    lastModified\n  }\n": types.OperatorDetailFragmentDoc,
    "\n  fragment OperatorSummary on OperatorVm {\n    operatorId\n    name\n  }\n": types.OperatorSummaryFragmentDoc,
    "\n  fragment OperatorGps on OperatorVm {\n    operatorId\n    name\n    protocolType\n    enabled\n    lastDeviceSyncAt\n    lastPositionSyncAt\n    syncIntervalMinutes\n  }\n": types.OperatorGpsFragmentDoc,
    "\n  query GetOperator($id: UUID!) {\n    operator(query: { id: $id }) {\n      ...OperatorDetail\n    }\n  }\n": types.GetOperatorDocument,
    "\n  query GetOperatorsByCurrentAccount {\n    operatorsByCurrentAccount {\n      ...OperatorDetail\n    }\n  }\n": types.GetOperatorsByCurrentAccountDocument,
    "\n  query GetOperatorsSummary {\n    operatorsByCurrentAccount {\n      ...OperatorSummary\n    }\n  }\n": types.GetOperatorsSummaryDocument,
    "\n  query GetGpsOperators {\n    operatorsByCurrentAccount {\n      ...OperatorGps\n    }\n  }\n": types.GetGpsOperatorsDocument,
    "\n  mutation CreateOperator($operator: OperatorDtoInput!) {\n    createOperator(command: { operator: $operator }) {\n      ...OperatorDetail\n    }\n  }\n": types.CreateOperatorDocument,
    "\n  mutation UpdateOperator($id: UUID!, $operator: UpdateOperatorDtoInput!) {\n    updateOperator(id: $id, command: { operator: $operator })\n  }\n": types.UpdateOperatorDocument,
    "\n  mutation DeleteOperator($id: UUID!) {\n    deleteOperator(id: $id)\n  }\n": types.DeleteOperatorDocument,
    "\n  mutation SetOperatorEnabled($operatorId: UUID!, $enabled: Boolean!) {\n    setOperatorEnabled(command: { operatorId: $operatorId, enabled: $enabled })\n  }\n": types.SetOperatorEnabledDocument,
    "\n  mutation TriggerOperatorDeviceSync($command: TriggerOperatorDeviceSyncCommandInput!) {\n    triggerOperatorDeviceSync(command: $command)\n  }\n": types.TriggerOperatorDeviceSyncDocument,
    "\n  fragment PlatformAnnouncementItem on PlatformAnnouncementVm {\n    platformAnnouncementId\n    messageEn\n    messageEs\n    severity\n    startsAt\n    endsAt\n    active\n    lastModified\n  }\n": types.PlatformAnnouncementItemFragmentDoc,
    "\n  query GetPlatformAnnouncements($skip: Int!, $take: Int!) {\n    platformAnnouncements(query: { skip: $skip, take: $take }) {\n      ...PlatformAnnouncementItem\n    }\n  }\n": types.GetPlatformAnnouncementsDocument,
    "\n  query GetBackgroundJobStatus {\n    backgroundJobStatus {\n      jobKey\n      status\n      startedAt\n      completedAt\n      attempts\n      errorCode\n      recordsEveryCycle\n    }\n  }\n": types.GetBackgroundJobStatusDocument,
    "\n  mutation CreatePlatformAnnouncement($announcement: PlatformAnnouncementDtoInput!) {\n    createPlatformAnnouncement(command: { announcement: $announcement }) {\n      ...PlatformAnnouncementItem\n    }\n  }\n": types.CreatePlatformAnnouncementDocument,
    "\n  mutation UpdatePlatformAnnouncement($platformAnnouncementId: UUID!, $announcement: PlatformAnnouncementDtoInput!) {\n    updatePlatformAnnouncement(\n      command: { platformAnnouncementId: $platformAnnouncementId, announcement: $announcement }\n    )\n  }\n": types.UpdatePlatformAnnouncementDocument,
    "\n  mutation DeletePlatformAnnouncement($platformAnnouncementId: UUID!) {\n    deletePlatformAnnouncement(command: { platformAnnouncementId: $platformAnnouncementId })\n  }\n": types.DeletePlatformAnnouncementDocument,
    "\n  fragment PointOfInterestItem on PointOfInterestVm {\n    pointOfInterestId\n    accountId\n    name\n    description\n    type\n    latitude\n    longitude\n    address\n    color\n    groupId\n    active\n  }\n": types.PointOfInterestItemFragmentDoc,
    "\n  query GetPointsOfInterestByAccount {\n    pointsOfInterestByAccount {\n      ...PointOfInterestItem\n    }\n  }\n": types.GetPointsOfInterestByAccountDocument,
    "\n  mutation CreatePointOfInterest($pointOfInterest: PointOfInterestDtoInput!) {\n    createPointOfInterest(command: { pointOfInterest: $pointOfInterest }) {\n      ...PointOfInterestItem\n    }\n  }\n": types.CreatePointOfInterestDocument,
    "\n  mutation UpdatePointOfInterest($id: UUID!, $pointOfInterest: UpdatePointOfInterestDtoInput!) {\n    updatePointOfInterest(id: $id, command: { id: $id, pointOfInterest: $pointOfInterest })\n  }\n": types.UpdatePointOfInterestDocument,
    "\n  mutation DeletePointOfInterest($id: UUID!) {\n    deletePointOfInterest(id: $id)\n  }\n": types.DeletePointOfInterestDocument,
    "\n  fragment CurrentPrincipalItem on CurrentPrincipalVm {\n    subjectId\n    principalType\n    userId\n    driverId\n    clientId\n    publicLinkGrantId\n    role\n    accountId\n    scopes\n    correlationId\n  }\n": types.CurrentPrincipalItemFragmentDoc,
    "\n  query GetCurrentPrincipal {\n    currentPrincipal {\n      ...CurrentPrincipalItem\n    }\n  }\n": types.GetCurrentPrincipalDocument,
    "\n  fragment PublicLinkGrantFields on PublicLinkGrantVm {\n    publicLinkGrantId\n    accountId\n    resourceType\n    resourceId\n    scopes\n    purpose\n    expiresAt\n    revokedAt\n    revokedBy\n    createdByPrincipalId\n    accessCount\n    lastAccessedAt\n    lastModified\n    token\n  }\n": types.PublicLinkGrantFieldsFragmentDoc,
    "\n  query GetPublicLinkGrant($publicLinkGrantId: UUID!) {\n    publicLinkGrant(query: { publicLinkGrantId: $publicLinkGrantId }) {\n      ...PublicLinkGrantFields\n    }\n  }\n": types.GetPublicLinkGrantDocument,
    "\n  query GetPublicLinkGrantsByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {\n    publicLinkGrantsByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...PublicLinkGrantFields\n    }\n  }\n": types.GetPublicLinkGrantsByAccountDocument,
    "\n  mutation CreatePublicLinkGrant($publicLinkGrant: PublicLinkGrantDtoInput!) {\n    createPublicLinkGrant(command: { publicLinkGrant: $publicLinkGrant }) {\n      ...PublicLinkGrantFields\n    }\n  }\n": types.CreatePublicLinkGrantDocument,
    "\n  mutation RevokePublicLinkGrant($publicLinkGrantId: UUID!, $revokedBy: String!) {\n    revokePublicLinkGrant(command: { publicLinkGrantId: $publicLinkGrantId, revokedBy: $revokedBy })\n  }\n": types.RevokePublicLinkGrantDocument,
    "\n  fragment ReportFields on ReportVm {\n    reportId\n    code\n    description\n    type\n    typeId\n    active\n    category\n    requiredFeatureKey\n    managerOnly\n    supportsPdf\n    sortOrder\n  }\n": types.ReportFieldsFragmentDoc,
    "\n  query GetReports {\n    reports {\n      ...ReportFields\n    }\n  }\n": types.GetReportsDocument,
    "\n  mutation UpdateReport($id: UUID!, $report: UpdateReportDtoInput!) {\n    updateReport(id: $id, command: { report: $report })\n  }\n": types.UpdateReportDocument,
    "\n  fragment AccountSettingsItem on AccountSettingsVm {\n    accountId\n    maps\n    mapsKey\n    onlineInterval\n    refreshMap\n    refreshMapInterval\n  }\n": types.AccountSettingsItemFragmentDoc,
    "\n  fragment UserSettingsItem on UserSettingsVm {\n    userId\n    style\n    language\n    navbar\n  }\n": types.UserSettingsItemFragmentDoc,
    "\n  query GetAccountSettingsByUser {\n    accountSettingsByUser {\n      ...AccountSettingsItem\n    }\n  }\n": types.GetAccountSettingsByUserDocument,
    "\n  query GetUserSettings {\n    userSettings {\n      ...UserSettingsItem\n    }\n  }\n": types.GetUserSettingsDocument,
    "\n  mutation UpdateAccountSettings($id: UUID!, $accountSettings: AccountSettingsDtoInput!) {\n    updateAccountSettings(id: $id, command: { accountSettings: $accountSettings })\n  }\n": types.UpdateAccountSettingsDocument,
    "\n  mutation UpdateUserSettings($id: UUID!, $userSettings: UserSettingsDtoInput!) {\n    updateUserSettings(id: $id, command: { userSettings: $userSettings })\n  }\n": types.UpdateUserSettingsDocument,
    "\n  fragment AccountSupportGrantItem on AccountSupportGrantVm {\n    accountSupportGrantId\n    accountId\n    supportUserId\n    reason\n    ticketReference\n    approvedBy\n    approvedAt\n    accessLevel\n    startsAt\n    endsAt\n    revokedAt\n    revokedBy\n    lastModified\n  }\n": types.AccountSupportGrantItemFragmentDoc,
    "\n  query GetSupportGrantStatus($accountSupportGrantId: UUID!) {\n    supportGrantStatus(query: { accountSupportGrantId: $accountSupportGrantId }) {\n      ...AccountSupportGrantItem\n    }\n  }\n": types.GetSupportGrantStatusDocument,
    "\n  query GetAccountSupportGrants($accountId: UUID, $skip: Int!, $take: Int!) {\n    accountSupportGrants(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AccountSupportGrantItem\n    }\n  }\n": types.GetAccountSupportGrantsDocument,
    "\n  mutation CreateAccountSupportGrant($accountSupportGrant: AccountSupportGrantDtoInput!) {\n    createAccountSupportGrant(command: { accountSupportGrant: $accountSupportGrant }) {\n      ...AccountSupportGrantItem\n    }\n  }\n": types.CreateAccountSupportGrantDocument,
    "\n  mutation ApproveAccountSupportGrant($accountSupportGrantId: UUID!, $approvedBy: String!) {\n    approveAccountSupportGrant(\n      command: { accountSupportGrantId: $accountSupportGrantId, approvedBy: $approvedBy }\n    )\n  }\n": types.ApproveAccountSupportGrantDocument,
    "\n  mutation RevokeAccountSupportGrant($accountSupportGrantId: UUID!, $revokedBy: String!) {\n    revokeAccountSupportGrant(\n      command: { accountSupportGrantId: $accountSupportGrantId, revokedBy: $revokedBy }\n    )\n  }\n": types.RevokeAccountSupportGrantDocument,
    "\n  fragment TransporterItem on TransporterVm {\n    transporterId\n    name\n    transporterType\n    transporterTypeId\n  }\n": types.TransporterItemFragmentDoc,
    "\n  query GetTransporter($id: UUID!) {\n    transporter(query: { id: $id }) {\n      ...TransporterItem\n    }\n  }\n": types.GetTransporterDocument,
    "\n  query GetTransportersByAccount {\n    transportersByAccount {\n      ...TransporterItem\n    }\n  }\n": types.GetTransportersByAccountDocument,
    "\n  query GetTransportersByUser {\n    transportersByUser {\n      ...TransporterItem\n    }\n  }\n": types.GetTransportersByUserDocument,
    "\n  query GetTransportersByGroup($groupId: Long!) {\n    transportersByGroup(query: { groupId: $groupId }) {\n      ...TransporterItem\n    }\n  }\n": types.GetTransportersByGroupDocument,
    "\n  mutation CreateTransporter($transporter: TransporterDtoInput!) {\n    createTransporter(command: { transporter: $transporter }) {\n      ...TransporterItem\n    }\n  }\n": types.CreateTransporterDocument,
    "\n  mutation UpdateTransporter($id: UUID!, $transporter: UpdateTransporterDtoInput!) {\n    updateTransporter(id: $id, command: { transporter: $transporter })\n  }\n": types.UpdateTransporterDocument,
    "\n  mutation DeleteTransporter($id: UUID!) {\n    deleteTransporter(id: $id)\n  }\n": types.DeleteTransporterDocument,
    "\n  fragment AssignmentFields on TransporterDeviceAssignmentVm {\n    transporterDeviceAssignmentId\n    accountId\n    transporterId\n    deviceId\n    effectiveFrom\n    effectiveTo\n    priority\n    isPrimary\n    status\n    assignmentReason\n  }\n": types.AssignmentFieldsFragmentDoc,
    "\n  query GetTransporterDeviceAssignmentsByAccount($accountId: UUID!, $activeOnly: Boolean!) {\n    transporterDeviceAssignmentsByAccount(query: { accountId: $accountId, activeOnly: $activeOnly }) {\n      ...AssignmentFields\n      createdByPrincipalType\n      createdByPrincipalId\n    }\n  }\n": types.GetTransporterDeviceAssignmentsByAccountDocument,
    "\n  query GetTransporterDeviceAssignmentsByTransporter($transporterId: UUID!, $activeOnly: Boolean!) {\n    transporterDeviceAssignmentsByTransporter(\n      query: { transporterId: $transporterId, activeOnly: $activeOnly }\n    ) {\n      ...AssignmentFields\n    }\n  }\n": types.GetTransporterDeviceAssignmentsByTransporterDocument,
    "\n  mutation AssignDeviceToTransporter($assignment: TransporterDeviceAssignmentDtoInput!) {\n    assignDeviceToTransporter(command: { assignment: $assignment }) {\n      transporterDeviceAssignmentId\n      deviceId\n      transporterId\n      effectiveFrom\n      isPrimary\n      status\n    }\n  }\n": types.AssignDeviceToTransporterDocument,
    "\n  mutation EndDeviceTransporterAssignment($assignmentId: UUID!, $reason: String) {\n    endDeviceTransporterAssignment(command: { assignmentId: $assignmentId, reason: $reason })\n  }\n": types.EndDeviceTransporterAssignmentDocument,
    "\n  fragment TransporterTypeItem on TransporterTypeVm {\n    transporterTypeId\n    accBased\n    stoppedGap\n    maxDistance\n    maxTimeGap\n    type\n  }\n": types.TransporterTypeItemFragmentDoc,
    "\n  query GetTransporterTypes {\n    transporterTypes {\n      ...TransporterTypeItem\n    }\n  }\n": types.GetTransporterTypesDocument,
    "\n  mutation UpdateTransporterType($id: Short!, $transporterType: TransporterTypeDtoInput!) {\n    updateTransporterType(id: $id, command: { transporterType: $transporterType })\n  }\n": types.UpdateTransporterTypeDocument,
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
export function graphql(source: "\n  fragment AccountFeatureItem on AccountFeatureVm {\n    accountFeatureId\n    accountId\n    featureKey\n    enabled\n    tier\n    source\n    effectiveFrom\n    effectiveTo\n    configurationJson\n    lastModified\n  }\n"): (typeof documents)["\n  fragment AccountFeatureItem on AccountFeatureVm {\n    accountFeatureId\n    accountId\n    featureKey\n    enabled\n    tier\n    source\n    effectiveFrom\n    effectiveTo\n    configurationJson\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccountFeatures($accountId: UUID!) {\n    accountFeatures(query: { accountId: $accountId }) {\n      ...AccountFeatureItem\n    }\n  }\n"): (typeof documents)["\n  query GetAccountFeatures($accountId: UUID!) {\n    accountFeatures(query: { accountId: $accountId }) {\n      ...AccountFeatureItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SetAccountFeature($feature: AccountFeatureDtoInput!) {\n    setAccountFeature(command: { feature: $feature }) {\n      ...AccountFeatureItem\n    }\n  }\n"): (typeof documents)["\n  mutation SetAccountFeature($feature: AccountFeatureDtoInput!) {\n    setAccountFeature(command: { feature: $feature }) {\n      ...AccountFeatureItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccountFeaturesMaster($accountId: UUID!) {\n    accountFeaturesMaster(query: { accountId: $accountId }) {\n      ...AccountFeatureItem\n    }\n  }\n"): (typeof documents)["\n  query GetAccountFeaturesMaster($accountId: UUID!) {\n    accountFeaturesMaster(query: { accountId: $accountId }) {\n      ...AccountFeatureItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SetAccountFeatureMaster($feature: AccountFeatureDtoInput!) {\n    setAccountFeatureMaster(command: { feature: $feature }) {\n      ...AccountFeatureItem\n    }\n  }\n"): (typeof documents)["\n  mutation SetAccountFeatureMaster($feature: AccountFeatureDtoInput!) {\n    setAccountFeatureMaster(command: { feature: $feature }) {\n      ...AccountFeatureItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AccountItem on AccountVm {\n    accountId\n    name\n    description\n    type\n    typeId\n    status\n    statusId\n    active\n    lastModified\n  }\n"): (typeof documents)["\n  fragment AccountItem on AccountVm {\n    accountId\n    name\n    description\n    type\n    typeId\n    status\n    statusId\n    active\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccount($id: UUID!) {\n    account(query: { id: $id }) {\n      ...AccountItem\n    }\n  }\n"): (typeof documents)["\n  query GetAccount($id: UUID!) {\n    account(query: { id: $id }) {\n      ...AccountItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccountByUser {\n    accountByUser {\n      ...AccountItem\n    }\n  }\n"): (typeof documents)["\n  query GetAccountByUser {\n    accountByUser {\n      ...AccountItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccounts {\n    accounts {\n      ...AccountItem\n    }\n  }\n"): (typeof documents)["\n  query GetAccounts {\n    accounts {\n      ...AccountItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateAccount($account: AccountDtoInput!) {\n    createAccount(command: { account: $account }) {\n      ...AccountItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreateAccount($account: AccountDtoInput!) {\n    createAccount(command: { account: $account }) {\n      ...AccountItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateAccount($id: UUID!, $account: UpdateAccountDtoInput!) {\n    updateAccount(id: $id, command: { account: $account })\n  }\n"): (typeof documents)["\n  mutation UpdateAccount($id: UUID!, $account: UpdateAccountDtoInput!) {\n    updateAccount(id: $id, command: { account: $account })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation ChangeAccountStatus($accountId: UUID!, $targetStatus: AccountStatus!, $reason: String) {\n    changeAccountStatus(\n      command: { accountId: $accountId, targetStatus: $targetStatus, reason: $reason }\n    ) {\n      ...AccountItem\n    }\n  }\n"): (typeof documents)["\n  mutation ChangeAccountStatus($accountId: UUID!, $targetStatus: AccountStatus!, $reason: String) {\n    changeAccountStatus(\n      command: { accountId: $accountId, targetStatus: $targetStatus, reason: $reason }\n    ) {\n      ...AccountItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccountContext {\n    accountContext {\n      status\n      statusId\n      branding {\n        accountId\n        displayName\n        logoDocumentId\n        primaryColor\n        reportHeader\n        lastModified\n      }\n      features {\n        accountFeatureId\n        accountId\n        featureKey\n        enabled\n        tier\n        source\n        effectiveFrom\n        effectiveTo\n        configurationJson\n        lastModified\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetAccountContext {\n    accountContext {\n      status\n      statusId\n      branding {\n        accountId\n        displayName\n        logoDocumentId\n        primaryColor\n        reportHeader\n        lastModified\n      }\n      features {\n        accountFeatureId\n        accountId\n        featureKey\n        enabled\n        tier\n        source\n        effectiveFrom\n        effectiveTo\n        configurationJson\n        lastModified\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlertEventItem on AlertEventVm {\n    alertEventId\n    accountId\n    eventType\n    severity\n    sourceModule\n    resourceType\n    resourceId\n    status\n    firstSeenAt\n    lastSeenAt\n    deduplicationKey\n  }\n"): (typeof documents)["\n  fragment AlertEventItem on AlertEventVm {\n    alertEventId\n    accountId\n    eventType\n    severity\n    sourceModule\n    resourceType\n    resourceId\n    status\n    firstSeenAt\n    lastSeenAt\n    deduplicationKey\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAlertEvents($accountId: UUID!, $skip: Int!, $take: Int!) {\n    alertEvents(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AlertEventItem\n    }\n  }\n"): (typeof documents)["\n  query GetAlertEvents($accountId: UUID!, $skip: Int!, $take: Int!) {\n    alertEvents(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AlertEventItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation AcknowledgeAlertEvent($alertEventId: UUID!) {\n    acknowledgeAlertEvent(command: { alertEventId: $alertEventId })\n  }\n"): (typeof documents)["\n  mutation AcknowledgeAlertEvent($alertEventId: UUID!) {\n    acknowledgeAlertEvent(command: { alertEventId: $alertEventId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation ResolveAlertEvent($alertEventId: UUID!) {\n    resolveAlertEvent(command: { alertEventId: $alertEventId })\n  }\n"): (typeof documents)["\n  mutation ResolveAlertEvent($alertEventId: UUID!) {\n    resolveAlertEvent(command: { alertEventId: $alertEventId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlertSubscriptionItem on AlertSubscriptionVm {\n    alertSubscriptionId\n    accountId\n    principalType\n    principalId\n    eventTypeFilter\n    channel\n    contact\n    enabled\n    lastModified\n  }\n"): (typeof documents)["\n  fragment AlertSubscriptionItem on AlertSubscriptionVm {\n    alertSubscriptionId\n    accountId\n    principalType\n    principalId\n    eventTypeFilter\n    channel\n    contact\n    enabled\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAlertSubscriptions($accountId: UUID!, $principalId: UUID, $skip: Int!, $take: Int!) {\n    alertSubscriptions(\n      query: { accountId: $accountId, principalId: $principalId, skip: $skip, take: $take }\n    ) {\n      ...AlertSubscriptionItem\n    }\n  }\n"): (typeof documents)["\n  query GetAlertSubscriptions($accountId: UUID!, $principalId: UUID, $skip: Int!, $take: Int!) {\n    alertSubscriptions(\n      query: { accountId: $accountId, principalId: $principalId, skip: $skip, take: $take }\n    ) {\n      ...AlertSubscriptionItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateAlertSubscription($subscription: AlertSubscriptionDtoInput!) {\n    createAlertSubscription(command: { subscription: $subscription }) {\n      alertSubscriptionId\n      accountId\n      principalType\n      principalId\n      channel\n      enabled\n    }\n  }\n"): (typeof documents)["\n  mutation CreateAlertSubscription($subscription: AlertSubscriptionDtoInput!) {\n    createAlertSubscription(command: { subscription: $subscription }) {\n      alertSubscriptionId\n      accountId\n      principalType\n      principalId\n      channel\n      enabled\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateAlertSubscription(\n    $alertSubscriptionId: UUID!\n    $subscription: AlertSubscriptionDtoInput!\n  ) {\n    updateAlertSubscription(\n      command: { alertSubscriptionId: $alertSubscriptionId, subscription: $subscription }\n    )\n  }\n"): (typeof documents)["\n  mutation UpdateAlertSubscription(\n    $alertSubscriptionId: UUID!\n    $subscription: AlertSubscriptionDtoInput!\n  ) {\n    updateAlertSubscription(\n      command: { alertSubscriptionId: $alertSubscriptionId, subscription: $subscription }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteAlertSubscription($alertSubscriptionId: UUID!) {\n    deleteAlertSubscription(command: { alertSubscriptionId: $alertSubscriptionId })\n  }\n"): (typeof documents)["\n  mutation DeleteAlertSubscription($alertSubscriptionId: UUID!) {\n    deleteAlertSubscription(command: { alertSubscriptionId: $alertSubscriptionId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AuditEventItem on AuditEventVm {\n    auditEventId\n    accountId\n    actorType\n    actorId\n    action\n    resourceType\n    resourceId\n    result\n    reason\n    correlationId\n    occurredAt\n  }\n"): (typeof documents)["\n  fragment AuditEventItem on AuditEventVm {\n    auditEventId\n    accountId\n    actorType\n    actorId\n    action\n    resourceType\n    resourceId\n    result\n    reason\n    correlationId\n    occurredAt\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAuditTrail($accountId: UUID!, $skip: Int!, $take: Int!) {\n    auditTrail(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AuditEventItem\n    }\n  }\n"): (typeof documents)["\n  query GetAuditTrail($accountId: UUID!, $skip: Int!, $take: Int!) {\n    auditTrail(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AuditEventItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment BackgroundJobRunItem on BackgroundJobRunVm {\n    backgroundJobRunId\n    jobKey\n    accountId\n    resourceKey\n    idempotencyKey\n    status\n    attempts\n    startedAt\n    completedAt\n    errorCode\n    errorMessage\n  }\n"): (typeof documents)["\n  fragment BackgroundJobRunItem on BackgroundJobRunVm {\n    backgroundJobRunId\n    jobKey\n    accountId\n    resourceKey\n    idempotencyKey\n    status\n    attempts\n    startedAt\n    completedAt\n    errorCode\n    errorMessage\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetBackgroundJobRuns($accountId: UUID!, $skip: Int!, $take: Int!) {\n    backgroundJobRuns(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...BackgroundJobRunItem\n    }\n  }\n"): (typeof documents)["\n  query GetBackgroundJobRuns($accountId: UUID!, $skip: Int!, $take: Int!) {\n    backgroundJobRuns(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...BackgroundJobRunItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AccountBrandingItem on AccountBrandingVm {\n    accountId\n    displayName\n    logoDocumentId\n    primaryColor\n    reportHeader\n    lastModified\n  }\n"): (typeof documents)["\n  fragment AccountBrandingItem on AccountBrandingVm {\n    accountId\n    displayName\n    logoDocumentId\n    primaryColor\n    reportHeader\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccountBranding($accountId: UUID!) {\n    accountBranding(query: { accountId: $accountId }) {\n      ...AccountBrandingItem\n    }\n  }\n"): (typeof documents)["\n  query GetAccountBranding($accountId: UUID!) {\n    accountBranding(query: { accountId: $accountId }) {\n      ...AccountBrandingItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateAccountBranding($branding: AccountBrandingDtoInput!) {\n    updateAccountBranding(command: { branding: $branding }) {\n      ...AccountBrandingItem\n    }\n  }\n"): (typeof documents)["\n  mutation UpdateAccountBranding($branding: AccountBrandingDtoInput!) {\n    updateAccountBranding(command: { branding: $branding }) {\n      ...AccountBrandingItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment CredentialFields on CredentialVm {\n    credentialId\n    key\n    key2\n    password\n    uri\n    username\n  }\n"): (typeof documents)["\n  fragment CredentialFields on CredentialVm {\n    credentialId\n    key\n    key2\n    password\n    uri\n    username\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetCredentialByOperator($operatorId: UUID!) {\n    credentialByOperator(query: { operatorId: $operatorId }) {\n      ...CredentialFields\n    }\n  }\n"): (typeof documents)["\n  query GetCredentialByOperator($operatorId: UUID!) {\n    credentialByOperator(query: { operatorId: $operatorId }) {\n      ...CredentialFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateCredential($credential: CredentialDtoInput!) {\n    createCredential(command: { credential: $credential }) {\n      ...CredentialFields\n    }\n  }\n"): (typeof documents)["\n  mutation CreateCredential($credential: CredentialDtoInput!) {\n    createCredential(command: { credential: $credential }) {\n      ...CredentialFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateCredential($id: UUID!, $credential: UpdateCredentialDtoInput!) {\n    updateCredential(id: $id, command: { credential: $credential })\n  }\n"): (typeof documents)["\n  mutation UpdateCredential($id: UUID!, $credential: UpdateCredentialDtoInput!) {\n    updateCredential(id: $id, command: { credential: $credential })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DeviceItem on DeviceVm {\n    deviceId\n    name\n    serial\n    description\n    deviceType\n    deviceTypeId\n    identifier\n    operatorId\n  }\n"): (typeof documents)["\n  fragment DeviceItem on DeviceVm {\n    deviceId\n    name\n    serial\n    description\n    deviceType\n    deviceTypeId\n    identifier\n    operatorId\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment SynchronizedDevice on DeviceVm {\n    deviceId\n    accountId\n    operatorId\n    serial\n    name\n    identifier\n    providerDisplayName\n    providerStatus\n    detectedStatus\n    firstSeenAt\n    lastSeenAt\n    lastSyncedAt\n    lastAssignedAt\n    ignoredAt\n  }\n"): (typeof documents)["\n  fragment SynchronizedDevice on DeviceVm {\n    deviceId\n    accountId\n    operatorId\n    serial\n    name\n    identifier\n    providerDisplayName\n    providerStatus\n    detectedStatus\n    firstSeenAt\n    lastSeenAt\n    lastSyncedAt\n    lastAssignedAt\n    ignoredAt\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDevicesByAccount {\n    devicesByAccount {\n      ...DeviceItem\n    }\n  }\n"): (typeof documents)["\n  query GetDevicesByAccount {\n    devicesByAccount {\n      ...DeviceItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteDevice($deviceId: UUID!) {\n    deleteDevice(deviceId: $deviceId)\n  }\n"): (typeof documents)["\n  mutation DeleteDevice($deviceId: UUID!) {\n    deleteDevice(deviceId: $deviceId)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetSynchronizedDevices($accountId: UUID!, $detectedStatus: DetectedStatus, $operatorId: UUID) {\n    synchronizedDevices(\n      query: { accountId: $accountId, detectedStatus: $detectedStatus, operatorId: $operatorId }\n    ) {\n      ...SynchronizedDevice\n    }\n  }\n"): (typeof documents)["\n  query GetSynchronizedDevices($accountId: UUID!, $detectedStatus: DetectedStatus, $operatorId: UUID) {\n    synchronizedDevices(\n      query: { accountId: $accountId, detectedStatus: $detectedStatus, operatorId: $operatorId }\n    ) {\n      ...SynchronizedDevice\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetUnassignedSynchronizedDevices($accountId: UUID!) {\n    unassignedSynchronizedDevices(query: { accountId: $accountId }) {\n      ...SynchronizedDevice\n    }\n  }\n"): (typeof documents)["\n  query GetUnassignedSynchronizedDevices($accountId: UUID!) {\n    unassignedSynchronizedDevices(query: { accountId: $accountId }) {\n      ...SynchronizedDevice\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SetSynchronizedDeviceIgnored($deviceId: UUID!, $ignored: Boolean!) {\n    setSynchronizedDeviceIgnored(command: { deviceId: $deviceId, ignored: $ignored })\n  }\n"): (typeof documents)["\n  mutation SetSynchronizedDeviceIgnored($deviceId: UUID!, $ignored: Boolean!) {\n    setSynchronizedDeviceIgnored(command: { deviceId: $deviceId, ignored: $ignored })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DocumentFields on DocumentVm {\n    documentId\n    accountId\n    ownerEntityType\n    ownerEntityId\n    uploadedByPrincipalType\n    uploadedByPrincipalId\n    fileName\n    category\n    title\n    description\n    contentType\n    sizeBytes\n    sha256Hash\n    classification\n    status\n    expiresAt\n    visibilityScope\n    scanStatus\n    currentVersion\n    downloadUrl\n    lastModified\n  }\n"): (typeof documents)["\n  fragment DocumentFields on DocumentVm {\n    documentId\n    accountId\n    ownerEntityType\n    ownerEntityId\n    uploadedByPrincipalType\n    uploadedByPrincipalId\n    fileName\n    category\n    title\n    description\n    contentType\n    sizeBytes\n    sha256Hash\n    classification\n    status\n    expiresAt\n    visibilityScope\n    scanStatus\n    currentVersion\n    downloadUrl\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DocumentVersionFields on DocumentVersionVm {\n    documentVersionId\n    documentId\n    accountId\n    versionNumber\n    contentType\n    fileName\n    sizeBytes\n    sha256Hash\n    scanStatus\n    replacedByPrincipalType\n    replacedByPrincipalId\n    reason\n    createdAt\n  }\n"): (typeof documents)["\n  fragment DocumentVersionFields on DocumentVersionVm {\n    documentVersionId\n    documentId\n    accountId\n    versionNumber\n    contentType\n    fileName\n    sizeBytes\n    sha256Hash\n    scanStatus\n    replacedByPrincipalType\n    replacedByPrincipalId\n    reason\n    createdAt\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DocumentTypeFields on DocumentTypeVm {\n    documentTypeId\n    accountId\n    category\n    displayName\n    required\n    expiring\n    defaultValidityDays\n    enabled\n    createdAt\n  }\n"): (typeof documents)["\n  fragment DocumentTypeFields on DocumentTypeVm {\n    documentTypeId\n    accountId\n    category\n    displayName\n    required\n    expiring\n    defaultValidityDays\n    enabled\n    createdAt\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DocumentSignatureFields on DocumentSignatureVm {\n    documentSignatureId\n    documentId\n    accountId\n    signerPrincipalType\n    signerPrincipalId\n    signerName\n    signatureImageDocumentId\n    legalTextAccepted\n    latitude\n    longitude\n    signedAt\n    createdAt\n  }\n"): (typeof documents)["\n  fragment DocumentSignatureFields on DocumentSignatureVm {\n    documentSignatureId\n    documentId\n    accountId\n    signerPrincipalType\n    signerPrincipalId\n    signerName\n    signatureImageDocumentId\n    legalTextAccepted\n    latitude\n    longitude\n    signedAt\n    createdAt\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDocumentsForOwner(\n    $accountId: UUID!\n    $ownerEntityType: String!\n    $ownerEntityId: String!\n    $skip: Int!\n    $take: Int!\n  ) {\n    documentsForOwner(\n      query: {\n        accountId: $accountId\n        ownerEntityType: $ownerEntityType\n        ownerEntityId: $ownerEntityId\n        skip: $skip\n        take: $take\n      }\n    ) {\n      ...DocumentFields\n    }\n  }\n"): (typeof documents)["\n  query GetDocumentsForOwner(\n    $accountId: UUID!\n    $ownerEntityType: String!\n    $ownerEntityId: String!\n    $skip: Int!\n    $take: Int!\n  ) {\n    documentsForOwner(\n      query: {\n        accountId: $accountId\n        ownerEntityType: $ownerEntityType\n        ownerEntityId: $ownerEntityId\n        skip: $skip\n        take: $take\n      }\n    ) {\n      ...DocumentFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDocument($documentId: UUID!) {\n    document(query: { documentId: $documentId }) {\n      ...DocumentFields\n    }\n  }\n"): (typeof documents)["\n  query GetDocument($documentId: UUID!) {\n    document(query: { documentId: $documentId }) {\n      ...DocumentFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDocumentVersions($documentId: UUID!, $skip: Int!, $take: Int!) {\n    documentVersions(query: { documentId: $documentId, skip: $skip, take: $take }) {\n      ...DocumentVersionFields\n    }\n  }\n"): (typeof documents)["\n  query GetDocumentVersions($documentId: UUID!, $skip: Int!, $take: Int!) {\n    documentVersions(query: { documentId: $documentId, skip: $skip, take: $take }) {\n      ...DocumentVersionFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDocumentSignatures($documentId: UUID!) {\n    documentSignatures(query: { documentId: $documentId }) {\n      ...DocumentSignatureFields\n    }\n  }\n"): (typeof documents)["\n  query GetDocumentSignatures($documentId: UUID!) {\n    documentSignatures(query: { documentId: $documentId }) {\n      ...DocumentSignatureFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetActiveDocumentByCategory(\n    $ownerEntityType: String!\n    $ownerEntityId: String!\n    $category: String!\n  ) {\n    activeDocumentByCategory(\n      query: { ownerEntityType: $ownerEntityType, ownerEntityId: $ownerEntityId, category: $category }\n    ) {\n      ...DocumentFields\n    }\n  }\n"): (typeof documents)["\n  query GetActiveDocumentByCategory(\n    $ownerEntityType: String!\n    $ownerEntityId: String!\n    $category: String!\n  ) {\n    activeDocumentByCategory(\n      query: { ownerEntityType: $ownerEntityType, ownerEntityId: $ownerEntityId, category: $category }\n    ) {\n      ...DocumentFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query SearchDocuments($filter: DocumentSearchFilterInput!, $skip: Int!, $take: Int!) {\n    searchDocuments(query: { filter: $filter, skip: $skip, take: $take }) {\n      ...DocumentFields\n    }\n  }\n"): (typeof documents)["\n  query SearchDocuments($filter: DocumentSearchFilterInput!, $skip: Int!, $take: Int!) {\n    searchDocuments(query: { filter: $filter, skip: $skip, take: $take }) {\n      ...DocumentFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetExpiringDocuments($withinDays: Int!, $skip: Int!, $take: Int!) {\n    expiringDocuments(query: { withinDays: $withinDays, skip: $skip, take: $take }) {\n      ...DocumentFields\n    }\n  }\n"): (typeof documents)["\n  query GetExpiringDocuments($withinDays: Int!, $skip: Int!, $take: Int!) {\n    expiringDocuments(query: { withinDays: $withinDays, skip: $skip, take: $take }) {\n      ...DocumentFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDocumentShares($documentId: UUID!) {\n    documentShares(query: { documentId: $documentId }) {\n      ...PublicLinkGrantFields\n    }\n  }\n"): (typeof documents)["\n  query GetDocumentShares($documentId: UUID!) {\n    documentShares(query: { documentId: $documentId }) {\n      ...PublicLinkGrantFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDocumentTypes($accountId: UUID!, $includeDisabled: Boolean!) {\n    documentTypes(query: { accountId: $accountId, includeDisabled: $includeDisabled }) {\n      ...DocumentTypeFields\n    }\n  }\n"): (typeof documents)["\n  query GetDocumentTypes($accountId: UUID!, $includeDisabled: Boolean!) {\n    documentTypes(query: { accountId: $accountId, includeDisabled: $includeDisabled }) {\n      ...DocumentTypeFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation VoidDocument($documentId: UUID!, $reason: String!) {\n    voidDocument(command: { documentId: $documentId, reason: $reason })\n  }\n"): (typeof documents)["\n  mutation VoidDocument($documentId: UUID!, $reason: String!) {\n    voidDocument(command: { documentId: $documentId, reason: $reason })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation ExpireDocument($documentId: UUID!, $expiresAt: DateTime!) {\n    expireDocument(command: { documentId: $documentId, expiresAt: $expiresAt })\n  }\n"): (typeof documents)["\n  mutation ExpireDocument($documentId: UUID!, $expiresAt: DateTime!) {\n    expireDocument(command: { documentId: $documentId, expiresAt: $expiresAt })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteDocumentReference($documentId: UUID!) {\n    deleteDocumentReference(command: { documentId: $documentId })\n  }\n"): (typeof documents)["\n  mutation DeleteDocumentReference($documentId: UUID!) {\n    deleteDocumentReference(command: { documentId: $documentId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SignDocument($signature: DocumentSignatureDtoInput!) {\n    signDocument(command: { signature: $signature }) {\n      ...DocumentSignatureFields\n    }\n  }\n"): (typeof documents)["\n  mutation SignDocument($signature: DocumentSignatureDtoInput!) {\n    signDocument(command: { signature: $signature }) {\n      ...DocumentSignatureFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation ConfigureDocumentType($documentType: DocumentTypeDtoInput!) {\n    configureDocumentType(command: { documentType: $documentType }) {\n      ...DocumentTypeFields\n    }\n  }\n"): (typeof documents)["\n  mutation ConfigureDocumentType($documentType: DocumentTypeDtoInput!) {\n    configureDocumentType(command: { documentType: $documentType }) {\n      ...DocumentTypeFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DisableDocumentType($documentTypeId: UUID!) {\n    disableDocumentType(command: { documentTypeId: $documentTypeId })\n  }\n"): (typeof documents)["\n  mutation DisableDocumentType($documentTypeId: UUID!) {\n    disableDocumentType(command: { documentTypeId: $documentTypeId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DriverItem on DriverVm {\n    driverId\n    accountId\n    name\n    phone\n    documentType\n    documentNumber\n    active\n    employeeCode\n    licenseNumber\n    licenseExpiresAt\n    defaultTransporterId\n    lastModified\n  }\n"): (typeof documents)["\n  fragment DriverItem on DriverVm {\n    driverId\n    accountId\n    name\n    phone\n    documentType\n    documentNumber\n    active\n    employeeCode\n    licenseNumber\n    licenseExpiresAt\n    defaultTransporterId\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDriversByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {\n    driversByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...DriverItem\n    }\n  }\n"): (typeof documents)["\n  query GetDriversByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {\n    driversByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...DriverItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateDriver($driver: DriverDtoInput!) {\n    createDriver(command: { driver: $driver }) {\n      ...DriverItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreateDriver($driver: DriverDtoInput!) {\n    createDriver(command: { driver: $driver }) {\n      ...DriverItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateDriver($driverId: UUID!, $driver: DriverDtoInput!) {\n    updateDriver(command: { driverId: $driverId, driver: $driver })\n  }\n"): (typeof documents)["\n  mutation UpdateDriver($driverId: UUID!, $driver: DriverDtoInput!) {\n    updateDriver(command: { driverId: $driverId, driver: $driver })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeactivateDriver($driverId: UUID!) {\n    deactivateDriver(command: { driverId: $driverId })\n  }\n"): (typeof documents)["\n  mutation DeactivateDriver($driverId: UUID!) {\n    deactivateDriver(command: { driverId: $driverId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment GeocodingProviderItem on GeocodingProviderVm {\n    geocodingProviderId\n    name\n    type\n    endpointUri\n    apiKey\n    requestsPerSecond\n    timeoutSeconds\n    configurationJson\n    active\n  }\n"): (typeof documents)["\n  fragment GeocodingProviderItem on GeocodingProviderVm {\n    geocodingProviderId\n    name\n    type\n    endpointUri\n    apiKey\n    requestsPerSecond\n    timeoutSeconds\n    configurationJson\n    active\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetGeocodingProviders {\n    geocodingProviders {\n      ...GeocodingProviderItem\n    }\n  }\n"): (typeof documents)["\n  query GetGeocodingProviders {\n    geocodingProviders {\n      ...GeocodingProviderItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateGeocodingProvider($geocodingProvider: GeocodingProviderDtoInput!) {\n    createGeocodingProvider(command: { geocodingProvider: $geocodingProvider }) {\n      ...GeocodingProviderItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreateGeocodingProvider($geocodingProvider: GeocodingProviderDtoInput!) {\n    createGeocodingProvider(command: { geocodingProvider: $geocodingProvider }) {\n      ...GeocodingProviderItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateGeocodingProvider($id: UUID!, $geocodingProvider: UpdateGeocodingProviderDtoInput!) {\n    updateGeocodingProvider(id: $id, command: { id: $id, geocodingProvider: $geocodingProvider })\n  }\n"): (typeof documents)["\n  mutation UpdateGeocodingProvider($id: UUID!, $geocodingProvider: UpdateGeocodingProviderDtoInput!) {\n    updateGeocodingProvider(id: $id, command: { id: $id, geocodingProvider: $geocodingProvider })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteGeocodingProvider($id: UUID!) {\n    deleteGeocodingProvider(id: $id)\n  }\n"): (typeof documents)["\n  mutation DeleteGeocodingProvider($id: UUID!) {\n    deleteGeocodingProvider(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SetActiveGeocodingProvider($id: UUID!) {\n    setActiveGeocodingProvider(id: $id)\n  }\n"): (typeof documents)["\n  mutation SetActiveGeocodingProvider($id: UUID!) {\n    setActiveGeocodingProvider(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetGpsIntegrationDashboard($accountId: UUID!) {\n    gpsIntegrationDashboard(query: { accountId: $accountId }) {\n      operatorsTotal\n      operatorsEnabled\n      operatorsHealthy\n      operatorsDegraded\n      operatorsOffline\n      devicesTotal\n      devicesNew\n      devicesAvailable\n      devicesAssigned\n      devicesIgnored\n      devicesRemoved\n      recentlyAddedDevicesLast24h\n      unassignedDevicesCount\n      syncRunsSucceededLast24h\n      syncRunsFailedLast24h\n      lastAutomaticSyncAt\n      lastManualSyncAt\n      averageSyncDurationSeconds\n      deviceCountsByProviderStatus {\n        operatorId\n        operatorName\n        detectedStatus\n        count\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetGpsIntegrationDashboard($accountId: UUID!) {\n    gpsIntegrationDashboard(query: { accountId: $accountId }) {\n      operatorsTotal\n      operatorsEnabled\n      operatorsHealthy\n      operatorsDegraded\n      operatorsOffline\n      devicesTotal\n      devicesNew\n      devicesAvailable\n      devicesAssigned\n      devicesIgnored\n      devicesRemoved\n      recentlyAddedDevicesLast24h\n      unassignedDevicesCount\n      syncRunsSucceededLast24h\n      syncRunsFailedLast24h\n      lastAutomaticSyncAt\n      lastManualSyncAt\n      averageSyncDurationSeconds\n      deviceCountsByProviderStatus {\n        operatorId\n        operatorName\n        detectedStatus\n        count\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment GroupItem on GroupVm {\n    groupId\n    name\n    description\n    active\n    accountId\n  }\n"): (typeof documents)["\n  fragment GroupItem on GroupVm {\n    groupId\n    name\n    description\n    active\n    accountId\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetGroups {\n    groupsByAccount {\n      ...GroupItem\n    }\n  }\n"): (typeof documents)["\n  query GetGroups {\n    groupsByAccount {\n      ...GroupItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateGroup($group: GroupDtoInput!) {\n    createGroup(command: { group: $group }) {\n      ...GroupItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreateGroup($group: GroupDtoInput!) {\n    createGroup(command: { group: $group }) {\n      ...GroupItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateGroup($id: Long!, $group: UpdateGroupDtoInput!) {\n    updateGroup(id: $id, command: { group: $group })\n  }\n"): (typeof documents)["\n  mutation UpdateGroup($id: Long!, $group: UpdateGroupDtoInput!) {\n    updateGroup(id: $id, command: { group: $group })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteGroup($id: Long!) {\n    deleteGroup(id: $id)\n  }\n"): (typeof documents)["\n  mutation DeleteGroup($id: Long!) {\n    deleteGroup(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetUsersByGroup($groupId: Long!) {\n    usersByGroup(query: { groupId: $groupId }) {\n      userId\n      username\n      active\n      accountId\n    }\n  }\n"): (typeof documents)["\n  query GetUsersByGroup($groupId: Long!) {\n    usersByGroup(query: { groupId: $groupId }) {\n      userId\n      username\n      active\n      accountId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateUserGroup($userGroup: UserGroupDtoInput!) {\n    createUserGroup(command: { userGroup: $userGroup }) {\n      userId\n      groupId\n    }\n  }\n"): (typeof documents)["\n  mutation CreateUserGroup($userGroup: UserGroupDtoInput!) {\n    createUserGroup(command: { userGroup: $userGroup }) {\n      userId\n      groupId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteUserGroup($userId: UUID!, $groupId: Long!) {\n    deleteUserGroup(userId: $userId, groupId: $groupId)\n  }\n"): (typeof documents)["\n  mutation DeleteUserGroup($userId: UUID!, $groupId: Long!) {\n    deleteUserGroup(userId: $userId, groupId: $groupId)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateTransporterGroup($transporterGroup: TransporterGroupDtoInput!) {\n    createTransporterGroup(command: { transporterGroup: $transporterGroup }) {\n      transporterId\n      groupId\n    }\n  }\n"): (typeof documents)["\n  mutation CreateTransporterGroup($transporterGroup: TransporterGroupDtoInput!) {\n    createTransporterGroup(command: { transporterGroup: $transporterGroup }) {\n      transporterId\n      groupId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteTransporterGroup($transporterId: UUID!, $groupId: Long!) {\n    deleteTransporterGroup(transporterId: $transporterId, groupId: $groupId)\n  }\n"): (typeof documents)["\n  mutation DeleteTransporterGroup($transporterId: UUID!, $groupId: Long!) {\n    deleteTransporterGroup(transporterId: $transporterId, groupId: $groupId)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment NotificationDeliveryItem on NotificationDeliveryVm {\n    notificationDeliveryId\n    accountId\n    notificationRuleId\n    alertEventId\n    channel\n    recipientPrincipalType\n    recipient\n    status\n    attempts\n    providerMessageId\n    error\n    sentAt\n    readAt\n  }\n"): (typeof documents)["\n  fragment NotificationDeliveryItem on NotificationDeliveryVm {\n    notificationDeliveryId\n    accountId\n    notificationRuleId\n    alertEventId\n    channel\n    recipientPrincipalType\n    recipient\n    status\n    attempts\n    providerMessageId\n    error\n    sentAt\n    readAt\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetNotificationDeliveries(\n    $accountId: UUID!\n    $status: String\n    $channel: String\n    $from: DateTime\n    $to: DateTime\n    $skip: Int!\n    $take: Int!\n  ) {\n    notificationDeliveries(\n      query: {\n        accountId: $accountId\n        status: $status\n        channel: $channel\n        from: $from\n        to: $to\n        skip: $skip\n        take: $take\n      }\n    ) {\n      ...NotificationDeliveryItem\n    }\n  }\n"): (typeof documents)["\n  query GetNotificationDeliveries(\n    $accountId: UUID!\n    $status: String\n    $channel: String\n    $from: DateTime\n    $to: DateTime\n    $skip: Int!\n    $take: Int!\n  ) {\n    notificationDeliveries(\n      query: {\n        accountId: $accountId\n        status: $status\n        channel: $channel\n        from: $from\n        to: $to\n        skip: $skip\n        take: $take\n      }\n    ) {\n      ...NotificationDeliveryItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDeliveryHealth($accountId: UUID!, $from: DateTime!, $to: DateTime!) {\n    deliveryHealth(query: { accountId: $accountId, from: $from, to: $to }) {\n      channel\n      status\n      count\n      averageAttempts\n    }\n  }\n"): (typeof documents)["\n  query GetDeliveryHealth($accountId: UUID!, $from: DateTime!, $to: DateTime!) {\n    deliveryHealth(query: { accountId: $accountId, from: $from, to: $to }) {\n      channel\n      status\n      count\n      averageAttempts\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment MyNotificationItem on MyNotificationVm {\n    notificationDeliveryId\n    alertEventId\n    eventType\n    severity\n    sourceModule\n    resourceType\n    resourceId\n    payloadJson\n    createdAt\n    readAt\n  }\n"): (typeof documents)["\n  fragment MyNotificationItem on MyNotificationVm {\n    notificationDeliveryId\n    alertEventId\n    eventType\n    severity\n    sourceModule\n    resourceType\n    resourceId\n    payloadJson\n    createdAt\n    readAt\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetMyNotifications($unreadOnly: Boolean!, $skip: Int!, $take: Int!) {\n    myNotifications(query: { unreadOnly: $unreadOnly, skip: $skip, take: $take }) {\n      ...MyNotificationItem\n    }\n  }\n"): (typeof documents)["\n  query GetMyNotifications($unreadOnly: Boolean!, $skip: Int!, $take: Int!) {\n    myNotifications(query: { unreadOnly: $unreadOnly, skip: $skip, take: $take }) {\n      ...MyNotificationItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation RetryNotificationDelivery($notificationDeliveryId: UUID!) {\n    retryNotificationDelivery(command: { notificationDeliveryId: $notificationDeliveryId })\n  }\n"): (typeof documents)["\n  mutation RetryNotificationDelivery($notificationDeliveryId: UUID!) {\n    retryNotificationDelivery(command: { notificationDeliveryId: $notificationDeliveryId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation MarkNotificationRead($notificationDeliveryId: UUID!) {\n    markNotificationRead(command: { notificationDeliveryId: $notificationDeliveryId })\n  }\n"): (typeof documents)["\n  mutation MarkNotificationRead($notificationDeliveryId: UUID!) {\n    markNotificationRead(command: { notificationDeliveryId: $notificationDeliveryId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SendTestNotification($accountId: UUID!, $channel: String!, $contact: String) {\n    sendTestNotification(command: { accountId: $accountId, channel: $channel, contact: $contact }) {\n      notificationDeliveryId\n      channel\n      status\n    }\n  }\n"): (typeof documents)["\n  mutation SendTestNotification($accountId: UUID!, $channel: String!, $contact: String) {\n    sendTestNotification(command: { accountId: $accountId, channel: $channel, contact: $contact }) {\n      notificationDeliveryId\n      channel\n      status\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment NotificationRuleItem on NotificationRuleVm {\n    notificationRuleId\n    accountId\n    ruleKey\n    ruleType\n    enabled\n    triggerEvent\n    recipientSelector\n    channelsJson\n    throttlingJson\n    configurationJson\n    lastModified\n  }\n"): (typeof documents)["\n  fragment NotificationRuleItem on NotificationRuleVm {\n    notificationRuleId\n    accountId\n    ruleKey\n    ruleType\n    enabled\n    triggerEvent\n    recipientSelector\n    channelsJson\n    throttlingJson\n    configurationJson\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetNotificationRules($accountId: UUID!, $skip: Int!, $take: Int!) {\n    notificationRules(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...NotificationRuleItem\n    }\n  }\n"): (typeof documents)["\n  query GetNotificationRules($accountId: UUID!, $skip: Int!, $take: Int!) {\n    notificationRules(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...NotificationRuleItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateNotificationRule($notificationRule: NotificationRuleDtoInput!) {\n    createNotificationRule(command: { notificationRule: $notificationRule }) {\n      notificationRuleId\n      accountId\n      ruleKey\n      enabled\n      lastModified\n    }\n  }\n"): (typeof documents)["\n  mutation CreateNotificationRule($notificationRule: NotificationRuleDtoInput!) {\n    createNotificationRule(command: { notificationRule: $notificationRule }) {\n      notificationRuleId\n      accountId\n      ruleKey\n      enabled\n      lastModified\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateNotificationRule(\n    $notificationRuleId: UUID!\n    $notificationRule: NotificationRuleDtoInput!\n  ) {\n    updateNotificationRule(\n      command: { notificationRuleId: $notificationRuleId, notificationRule: $notificationRule }\n    )\n  }\n"): (typeof documents)["\n  mutation UpdateNotificationRule(\n    $notificationRuleId: UUID!\n    $notificationRule: NotificationRuleDtoInput!\n  ) {\n    updateNotificationRule(\n      command: { notificationRuleId: $notificationRuleId, notificationRule: $notificationRule }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DisableNotificationRule($notificationRuleId: UUID!) {\n    disableNotificationRule(command: { notificationRuleId: $notificationRuleId })\n  }\n"): (typeof documents)["\n  mutation DisableNotificationRule($notificationRuleId: UUID!) {\n    disableNotificationRule(command: { notificationRuleId: $notificationRuleId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment NotificationTemplateItem on NotificationTemplateVm {\n    notificationTemplateId\n    accountId\n    templateKey\n    channel\n    locale\n    subject\n    body\n    active\n    lastModified\n  }\n"): (typeof documents)["\n  fragment NotificationTemplateItem on NotificationTemplateVm {\n    notificationTemplateId\n    accountId\n    templateKey\n    channel\n    locale\n    subject\n    body\n    active\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetNotificationTemplates($accountId: UUID!) {\n    notificationTemplates(query: { accountId: $accountId }) {\n      ...NotificationTemplateItem\n    }\n  }\n"): (typeof documents)["\n  query GetNotificationTemplates($accountId: UUID!) {\n    notificationTemplates(query: { accountId: $accountId }) {\n      ...NotificationTemplateItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateNotificationTemplate($template: NotificationTemplateDtoInput!) {\n    createNotificationTemplate(command: { template: $template }) {\n      notificationTemplateId\n      accountId\n      templateKey\n      channel\n      locale\n      active\n    }\n  }\n"): (typeof documents)["\n  mutation CreateNotificationTemplate($template: NotificationTemplateDtoInput!) {\n    createNotificationTemplate(command: { template: $template }) {\n      notificationTemplateId\n      accountId\n      templateKey\n      channel\n      locale\n      active\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateNotificationTemplate(\n    $notificationTemplateId: UUID!\n    $template: NotificationTemplateDtoInput!\n  ) {\n    updateNotificationTemplate(\n      command: { notificationTemplateId: $notificationTemplateId, template: $template }\n    )\n  }\n"): (typeof documents)["\n  mutation UpdateNotificationTemplate(\n    $notificationTemplateId: UUID!\n    $template: NotificationTemplateDtoInput!\n  ) {\n    updateNotificationTemplate(\n      command: { notificationTemplateId: $notificationTemplateId, template: $template }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteNotificationTemplate($notificationTemplateId: UUID!) {\n    deleteNotificationTemplate(command: { notificationTemplateId: $notificationTemplateId })\n  }\n"): (typeof documents)["\n  mutation DeleteNotificationTemplate($notificationTemplateId: UUID!) {\n    deleteNotificationTemplate(command: { notificationTemplateId: $notificationTemplateId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment OperatorDetail on OperatorVm {\n    operatorId\n    name\n    description\n    phoneNumber\n    emailAddress\n    address\n    contactName\n    protocolType\n    protocolTypeId\n    enabled\n    syncIntervalMinutes\n    healthStatus\n    lastSuccessfulSyncAt\n    lastFailedSyncAt\n    lastFailureCode\n    lastLatencyMs\n    lastDeviceSyncAt\n    lastPositionSyncAt\n    lastModified\n  }\n"): (typeof documents)["\n  fragment OperatorDetail on OperatorVm {\n    operatorId\n    name\n    description\n    phoneNumber\n    emailAddress\n    address\n    contactName\n    protocolType\n    protocolTypeId\n    enabled\n    syncIntervalMinutes\n    healthStatus\n    lastSuccessfulSyncAt\n    lastFailedSyncAt\n    lastFailureCode\n    lastLatencyMs\n    lastDeviceSyncAt\n    lastPositionSyncAt\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment OperatorSummary on OperatorVm {\n    operatorId\n    name\n  }\n"): (typeof documents)["\n  fragment OperatorSummary on OperatorVm {\n    operatorId\n    name\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment OperatorGps on OperatorVm {\n    operatorId\n    name\n    protocolType\n    enabled\n    lastDeviceSyncAt\n    lastPositionSyncAt\n    syncIntervalMinutes\n  }\n"): (typeof documents)["\n  fragment OperatorGps on OperatorVm {\n    operatorId\n    name\n    protocolType\n    enabled\n    lastDeviceSyncAt\n    lastPositionSyncAt\n    syncIntervalMinutes\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetOperator($id: UUID!) {\n    operator(query: { id: $id }) {\n      ...OperatorDetail\n    }\n  }\n"): (typeof documents)["\n  query GetOperator($id: UUID!) {\n    operator(query: { id: $id }) {\n      ...OperatorDetail\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetOperatorsByCurrentAccount {\n    operatorsByCurrentAccount {\n      ...OperatorDetail\n    }\n  }\n"): (typeof documents)["\n  query GetOperatorsByCurrentAccount {\n    operatorsByCurrentAccount {\n      ...OperatorDetail\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetOperatorsSummary {\n    operatorsByCurrentAccount {\n      ...OperatorSummary\n    }\n  }\n"): (typeof documents)["\n  query GetOperatorsSummary {\n    operatorsByCurrentAccount {\n      ...OperatorSummary\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetGpsOperators {\n    operatorsByCurrentAccount {\n      ...OperatorGps\n    }\n  }\n"): (typeof documents)["\n  query GetGpsOperators {\n    operatorsByCurrentAccount {\n      ...OperatorGps\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateOperator($operator: OperatorDtoInput!) {\n    createOperator(command: { operator: $operator }) {\n      ...OperatorDetail\n    }\n  }\n"): (typeof documents)["\n  mutation CreateOperator($operator: OperatorDtoInput!) {\n    createOperator(command: { operator: $operator }) {\n      ...OperatorDetail\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateOperator($id: UUID!, $operator: UpdateOperatorDtoInput!) {\n    updateOperator(id: $id, command: { operator: $operator })\n  }\n"): (typeof documents)["\n  mutation UpdateOperator($id: UUID!, $operator: UpdateOperatorDtoInput!) {\n    updateOperator(id: $id, command: { operator: $operator })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteOperator($id: UUID!) {\n    deleteOperator(id: $id)\n  }\n"): (typeof documents)["\n  mutation DeleteOperator($id: UUID!) {\n    deleteOperator(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SetOperatorEnabled($operatorId: UUID!, $enabled: Boolean!) {\n    setOperatorEnabled(command: { operatorId: $operatorId, enabled: $enabled })\n  }\n"): (typeof documents)["\n  mutation SetOperatorEnabled($operatorId: UUID!, $enabled: Boolean!) {\n    setOperatorEnabled(command: { operatorId: $operatorId, enabled: $enabled })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation TriggerOperatorDeviceSync($command: TriggerOperatorDeviceSyncCommandInput!) {\n    triggerOperatorDeviceSync(command: $command)\n  }\n"): (typeof documents)["\n  mutation TriggerOperatorDeviceSync($command: TriggerOperatorDeviceSyncCommandInput!) {\n    triggerOperatorDeviceSync(command: $command)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment PlatformAnnouncementItem on PlatformAnnouncementVm {\n    platformAnnouncementId\n    messageEn\n    messageEs\n    severity\n    startsAt\n    endsAt\n    active\n    lastModified\n  }\n"): (typeof documents)["\n  fragment PlatformAnnouncementItem on PlatformAnnouncementVm {\n    platformAnnouncementId\n    messageEn\n    messageEs\n    severity\n    startsAt\n    endsAt\n    active\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetPlatformAnnouncements($skip: Int!, $take: Int!) {\n    platformAnnouncements(query: { skip: $skip, take: $take }) {\n      ...PlatformAnnouncementItem\n    }\n  }\n"): (typeof documents)["\n  query GetPlatformAnnouncements($skip: Int!, $take: Int!) {\n    platformAnnouncements(query: { skip: $skip, take: $take }) {\n      ...PlatformAnnouncementItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetBackgroundJobStatus {\n    backgroundJobStatus {\n      jobKey\n      status\n      startedAt\n      completedAt\n      attempts\n      errorCode\n      recordsEveryCycle\n    }\n  }\n"): (typeof documents)["\n  query GetBackgroundJobStatus {\n    backgroundJobStatus {\n      jobKey\n      status\n      startedAt\n      completedAt\n      attempts\n      errorCode\n      recordsEveryCycle\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreatePlatformAnnouncement($announcement: PlatformAnnouncementDtoInput!) {\n    createPlatformAnnouncement(command: { announcement: $announcement }) {\n      ...PlatformAnnouncementItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreatePlatformAnnouncement($announcement: PlatformAnnouncementDtoInput!) {\n    createPlatformAnnouncement(command: { announcement: $announcement }) {\n      ...PlatformAnnouncementItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdatePlatformAnnouncement($platformAnnouncementId: UUID!, $announcement: PlatformAnnouncementDtoInput!) {\n    updatePlatformAnnouncement(\n      command: { platformAnnouncementId: $platformAnnouncementId, announcement: $announcement }\n    )\n  }\n"): (typeof documents)["\n  mutation UpdatePlatformAnnouncement($platformAnnouncementId: UUID!, $announcement: PlatformAnnouncementDtoInput!) {\n    updatePlatformAnnouncement(\n      command: { platformAnnouncementId: $platformAnnouncementId, announcement: $announcement }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeletePlatformAnnouncement($platformAnnouncementId: UUID!) {\n    deletePlatformAnnouncement(command: { platformAnnouncementId: $platformAnnouncementId })\n  }\n"): (typeof documents)["\n  mutation DeletePlatformAnnouncement($platformAnnouncementId: UUID!) {\n    deletePlatformAnnouncement(command: { platformAnnouncementId: $platformAnnouncementId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment PointOfInterestItem on PointOfInterestVm {\n    pointOfInterestId\n    accountId\n    name\n    description\n    type\n    latitude\n    longitude\n    address\n    color\n    groupId\n    active\n  }\n"): (typeof documents)["\n  fragment PointOfInterestItem on PointOfInterestVm {\n    pointOfInterestId\n    accountId\n    name\n    description\n    type\n    latitude\n    longitude\n    address\n    color\n    groupId\n    active\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetPointsOfInterestByAccount {\n    pointsOfInterestByAccount {\n      ...PointOfInterestItem\n    }\n  }\n"): (typeof documents)["\n  query GetPointsOfInterestByAccount {\n    pointsOfInterestByAccount {\n      ...PointOfInterestItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreatePointOfInterest($pointOfInterest: PointOfInterestDtoInput!) {\n    createPointOfInterest(command: { pointOfInterest: $pointOfInterest }) {\n      ...PointOfInterestItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreatePointOfInterest($pointOfInterest: PointOfInterestDtoInput!) {\n    createPointOfInterest(command: { pointOfInterest: $pointOfInterest }) {\n      ...PointOfInterestItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdatePointOfInterest($id: UUID!, $pointOfInterest: UpdatePointOfInterestDtoInput!) {\n    updatePointOfInterest(id: $id, command: { id: $id, pointOfInterest: $pointOfInterest })\n  }\n"): (typeof documents)["\n  mutation UpdatePointOfInterest($id: UUID!, $pointOfInterest: UpdatePointOfInterestDtoInput!) {\n    updatePointOfInterest(id: $id, command: { id: $id, pointOfInterest: $pointOfInterest })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeletePointOfInterest($id: UUID!) {\n    deletePointOfInterest(id: $id)\n  }\n"): (typeof documents)["\n  mutation DeletePointOfInterest($id: UUID!) {\n    deletePointOfInterest(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment CurrentPrincipalItem on CurrentPrincipalVm {\n    subjectId\n    principalType\n    userId\n    driverId\n    clientId\n    publicLinkGrantId\n    role\n    accountId\n    scopes\n    correlationId\n  }\n"): (typeof documents)["\n  fragment CurrentPrincipalItem on CurrentPrincipalVm {\n    subjectId\n    principalType\n    userId\n    driverId\n    clientId\n    publicLinkGrantId\n    role\n    accountId\n    scopes\n    correlationId\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetCurrentPrincipal {\n    currentPrincipal {\n      ...CurrentPrincipalItem\n    }\n  }\n"): (typeof documents)["\n  query GetCurrentPrincipal {\n    currentPrincipal {\n      ...CurrentPrincipalItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment PublicLinkGrantFields on PublicLinkGrantVm {\n    publicLinkGrantId\n    accountId\n    resourceType\n    resourceId\n    scopes\n    purpose\n    expiresAt\n    revokedAt\n    revokedBy\n    createdByPrincipalId\n    accessCount\n    lastAccessedAt\n    lastModified\n    token\n  }\n"): (typeof documents)["\n  fragment PublicLinkGrantFields on PublicLinkGrantVm {\n    publicLinkGrantId\n    accountId\n    resourceType\n    resourceId\n    scopes\n    purpose\n    expiresAt\n    revokedAt\n    revokedBy\n    createdByPrincipalId\n    accessCount\n    lastAccessedAt\n    lastModified\n    token\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetPublicLinkGrant($publicLinkGrantId: UUID!) {\n    publicLinkGrant(query: { publicLinkGrantId: $publicLinkGrantId }) {\n      ...PublicLinkGrantFields\n    }\n  }\n"): (typeof documents)["\n  query GetPublicLinkGrant($publicLinkGrantId: UUID!) {\n    publicLinkGrant(query: { publicLinkGrantId: $publicLinkGrantId }) {\n      ...PublicLinkGrantFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetPublicLinkGrantsByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {\n    publicLinkGrantsByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...PublicLinkGrantFields\n    }\n  }\n"): (typeof documents)["\n  query GetPublicLinkGrantsByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {\n    publicLinkGrantsByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...PublicLinkGrantFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreatePublicLinkGrant($publicLinkGrant: PublicLinkGrantDtoInput!) {\n    createPublicLinkGrant(command: { publicLinkGrant: $publicLinkGrant }) {\n      ...PublicLinkGrantFields\n    }\n  }\n"): (typeof documents)["\n  mutation CreatePublicLinkGrant($publicLinkGrant: PublicLinkGrantDtoInput!) {\n    createPublicLinkGrant(command: { publicLinkGrant: $publicLinkGrant }) {\n      ...PublicLinkGrantFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation RevokePublicLinkGrant($publicLinkGrantId: UUID!, $revokedBy: String!) {\n    revokePublicLinkGrant(command: { publicLinkGrantId: $publicLinkGrantId, revokedBy: $revokedBy })\n  }\n"): (typeof documents)["\n  mutation RevokePublicLinkGrant($publicLinkGrantId: UUID!, $revokedBy: String!) {\n    revokePublicLinkGrant(command: { publicLinkGrantId: $publicLinkGrantId, revokedBy: $revokedBy })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ReportFields on ReportVm {\n    reportId\n    code\n    description\n    type\n    typeId\n    active\n    category\n    requiredFeatureKey\n    managerOnly\n    supportsPdf\n    sortOrder\n  }\n"): (typeof documents)["\n  fragment ReportFields on ReportVm {\n    reportId\n    code\n    description\n    type\n    typeId\n    active\n    category\n    requiredFeatureKey\n    managerOnly\n    supportsPdf\n    sortOrder\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetReports {\n    reports {\n      ...ReportFields\n    }\n  }\n"): (typeof documents)["\n  query GetReports {\n    reports {\n      ...ReportFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateReport($id: UUID!, $report: UpdateReportDtoInput!) {\n    updateReport(id: $id, command: { report: $report })\n  }\n"): (typeof documents)["\n  mutation UpdateReport($id: UUID!, $report: UpdateReportDtoInput!) {\n    updateReport(id: $id, command: { report: $report })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AccountSettingsItem on AccountSettingsVm {\n    accountId\n    maps\n    mapsKey\n    onlineInterval\n    refreshMap\n    refreshMapInterval\n  }\n"): (typeof documents)["\n  fragment AccountSettingsItem on AccountSettingsVm {\n    accountId\n    maps\n    mapsKey\n    onlineInterval\n    refreshMap\n    refreshMapInterval\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment UserSettingsItem on UserSettingsVm {\n    userId\n    style\n    language\n    navbar\n  }\n"): (typeof documents)["\n  fragment UserSettingsItem on UserSettingsVm {\n    userId\n    style\n    language\n    navbar\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccountSettingsByUser {\n    accountSettingsByUser {\n      ...AccountSettingsItem\n    }\n  }\n"): (typeof documents)["\n  query GetAccountSettingsByUser {\n    accountSettingsByUser {\n      ...AccountSettingsItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetUserSettings {\n    userSettings {\n      ...UserSettingsItem\n    }\n  }\n"): (typeof documents)["\n  query GetUserSettings {\n    userSettings {\n      ...UserSettingsItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateAccountSettings($id: UUID!, $accountSettings: AccountSettingsDtoInput!) {\n    updateAccountSettings(id: $id, command: { accountSettings: $accountSettings })\n  }\n"): (typeof documents)["\n  mutation UpdateAccountSettings($id: UUID!, $accountSettings: AccountSettingsDtoInput!) {\n    updateAccountSettings(id: $id, command: { accountSettings: $accountSettings })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateUserSettings($id: UUID!, $userSettings: UserSettingsDtoInput!) {\n    updateUserSettings(id: $id, command: { userSettings: $userSettings })\n  }\n"): (typeof documents)["\n  mutation UpdateUserSettings($id: UUID!, $userSettings: UserSettingsDtoInput!) {\n    updateUserSettings(id: $id, command: { userSettings: $userSettings })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AccountSupportGrantItem on AccountSupportGrantVm {\n    accountSupportGrantId\n    accountId\n    supportUserId\n    reason\n    ticketReference\n    approvedBy\n    approvedAt\n    accessLevel\n    startsAt\n    endsAt\n    revokedAt\n    revokedBy\n    lastModified\n  }\n"): (typeof documents)["\n  fragment AccountSupportGrantItem on AccountSupportGrantVm {\n    accountSupportGrantId\n    accountId\n    supportUserId\n    reason\n    ticketReference\n    approvedBy\n    approvedAt\n    accessLevel\n    startsAt\n    endsAt\n    revokedAt\n    revokedBy\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetSupportGrantStatus($accountSupportGrantId: UUID!) {\n    supportGrantStatus(query: { accountSupportGrantId: $accountSupportGrantId }) {\n      ...AccountSupportGrantItem\n    }\n  }\n"): (typeof documents)["\n  query GetSupportGrantStatus($accountSupportGrantId: UUID!) {\n    supportGrantStatus(query: { accountSupportGrantId: $accountSupportGrantId }) {\n      ...AccountSupportGrantItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAccountSupportGrants($accountId: UUID, $skip: Int!, $take: Int!) {\n    accountSupportGrants(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AccountSupportGrantItem\n    }\n  }\n"): (typeof documents)["\n  query GetAccountSupportGrants($accountId: UUID, $skip: Int!, $take: Int!) {\n    accountSupportGrants(query: { accountId: $accountId, skip: $skip, take: $take }) {\n      ...AccountSupportGrantItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateAccountSupportGrant($accountSupportGrant: AccountSupportGrantDtoInput!) {\n    createAccountSupportGrant(command: { accountSupportGrant: $accountSupportGrant }) {\n      ...AccountSupportGrantItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreateAccountSupportGrant($accountSupportGrant: AccountSupportGrantDtoInput!) {\n    createAccountSupportGrant(command: { accountSupportGrant: $accountSupportGrant }) {\n      ...AccountSupportGrantItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation ApproveAccountSupportGrant($accountSupportGrantId: UUID!, $approvedBy: String!) {\n    approveAccountSupportGrant(\n      command: { accountSupportGrantId: $accountSupportGrantId, approvedBy: $approvedBy }\n    )\n  }\n"): (typeof documents)["\n  mutation ApproveAccountSupportGrant($accountSupportGrantId: UUID!, $approvedBy: String!) {\n    approveAccountSupportGrant(\n      command: { accountSupportGrantId: $accountSupportGrantId, approvedBy: $approvedBy }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation RevokeAccountSupportGrant($accountSupportGrantId: UUID!, $revokedBy: String!) {\n    revokeAccountSupportGrant(\n      command: { accountSupportGrantId: $accountSupportGrantId, revokedBy: $revokedBy }\n    )\n  }\n"): (typeof documents)["\n  mutation RevokeAccountSupportGrant($accountSupportGrantId: UUID!, $revokedBy: String!) {\n    revokeAccountSupportGrant(\n      command: { accountSupportGrantId: $accountSupportGrantId, revokedBy: $revokedBy }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment TransporterItem on TransporterVm {\n    transporterId\n    name\n    transporterType\n    transporterTypeId\n  }\n"): (typeof documents)["\n  fragment TransporterItem on TransporterVm {\n    transporterId\n    name\n    transporterType\n    transporterTypeId\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTransporter($id: UUID!) {\n    transporter(query: { id: $id }) {\n      ...TransporterItem\n    }\n  }\n"): (typeof documents)["\n  query GetTransporter($id: UUID!) {\n    transporter(query: { id: $id }) {\n      ...TransporterItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTransportersByAccount {\n    transportersByAccount {\n      ...TransporterItem\n    }\n  }\n"): (typeof documents)["\n  query GetTransportersByAccount {\n    transportersByAccount {\n      ...TransporterItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTransportersByUser {\n    transportersByUser {\n      ...TransporterItem\n    }\n  }\n"): (typeof documents)["\n  query GetTransportersByUser {\n    transportersByUser {\n      ...TransporterItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTransportersByGroup($groupId: Long!) {\n    transportersByGroup(query: { groupId: $groupId }) {\n      ...TransporterItem\n    }\n  }\n"): (typeof documents)["\n  query GetTransportersByGroup($groupId: Long!) {\n    transportersByGroup(query: { groupId: $groupId }) {\n      ...TransporterItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateTransporter($transporter: TransporterDtoInput!) {\n    createTransporter(command: { transporter: $transporter }) {\n      ...TransporterItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreateTransporter($transporter: TransporterDtoInput!) {\n    createTransporter(command: { transporter: $transporter }) {\n      ...TransporterItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateTransporter($id: UUID!, $transporter: UpdateTransporterDtoInput!) {\n    updateTransporter(id: $id, command: { transporter: $transporter })\n  }\n"): (typeof documents)["\n  mutation UpdateTransporter($id: UUID!, $transporter: UpdateTransporterDtoInput!) {\n    updateTransporter(id: $id, command: { transporter: $transporter })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteTransporter($id: UUID!) {\n    deleteTransporter(id: $id)\n  }\n"): (typeof documents)["\n  mutation DeleteTransporter($id: UUID!) {\n    deleteTransporter(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AssignmentFields on TransporterDeviceAssignmentVm {\n    transporterDeviceAssignmentId\n    accountId\n    transporterId\n    deviceId\n    effectiveFrom\n    effectiveTo\n    priority\n    isPrimary\n    status\n    assignmentReason\n  }\n"): (typeof documents)["\n  fragment AssignmentFields on TransporterDeviceAssignmentVm {\n    transporterDeviceAssignmentId\n    accountId\n    transporterId\n    deviceId\n    effectiveFrom\n    effectiveTo\n    priority\n    isPrimary\n    status\n    assignmentReason\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTransporterDeviceAssignmentsByAccount($accountId: UUID!, $activeOnly: Boolean!) {\n    transporterDeviceAssignmentsByAccount(query: { accountId: $accountId, activeOnly: $activeOnly }) {\n      ...AssignmentFields\n      createdByPrincipalType\n      createdByPrincipalId\n    }\n  }\n"): (typeof documents)["\n  query GetTransporterDeviceAssignmentsByAccount($accountId: UUID!, $activeOnly: Boolean!) {\n    transporterDeviceAssignmentsByAccount(query: { accountId: $accountId, activeOnly: $activeOnly }) {\n      ...AssignmentFields\n      createdByPrincipalType\n      createdByPrincipalId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTransporterDeviceAssignmentsByTransporter($transporterId: UUID!, $activeOnly: Boolean!) {\n    transporterDeviceAssignmentsByTransporter(\n      query: { transporterId: $transporterId, activeOnly: $activeOnly }\n    ) {\n      ...AssignmentFields\n    }\n  }\n"): (typeof documents)["\n  query GetTransporterDeviceAssignmentsByTransporter($transporterId: UUID!, $activeOnly: Boolean!) {\n    transporterDeviceAssignmentsByTransporter(\n      query: { transporterId: $transporterId, activeOnly: $activeOnly }\n    ) {\n      ...AssignmentFields\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation AssignDeviceToTransporter($assignment: TransporterDeviceAssignmentDtoInput!) {\n    assignDeviceToTransporter(command: { assignment: $assignment }) {\n      transporterDeviceAssignmentId\n      deviceId\n      transporterId\n      effectiveFrom\n      isPrimary\n      status\n    }\n  }\n"): (typeof documents)["\n  mutation AssignDeviceToTransporter($assignment: TransporterDeviceAssignmentDtoInput!) {\n    assignDeviceToTransporter(command: { assignment: $assignment }) {\n      transporterDeviceAssignmentId\n      deviceId\n      transporterId\n      effectiveFrom\n      isPrimary\n      status\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation EndDeviceTransporterAssignment($assignmentId: UUID!, $reason: String) {\n    endDeviceTransporterAssignment(command: { assignmentId: $assignmentId, reason: $reason })\n  }\n"): (typeof documents)["\n  mutation EndDeviceTransporterAssignment($assignmentId: UUID!, $reason: String) {\n    endDeviceTransporterAssignment(command: { assignmentId: $assignmentId, reason: $reason })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment TransporterTypeItem on TransporterTypeVm {\n    transporterTypeId\n    accBased\n    stoppedGap\n    maxDistance\n    maxTimeGap\n    type\n  }\n"): (typeof documents)["\n  fragment TransporterTypeItem on TransporterTypeVm {\n    transporterTypeId\n    accBased\n    stoppedGap\n    maxDistance\n    maxTimeGap\n    type\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetTransporterTypes {\n    transporterTypes {\n      ...TransporterTypeItem\n    }\n  }\n"): (typeof documents)["\n  query GetTransporterTypes {\n    transporterTypes {\n      ...TransporterTypeItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateTransporterType($id: Short!, $transporterType: TransporterTypeDtoInput!) {\n    updateTransporterType(id: $id, command: { transporterType: $transporterType })\n  }\n"): (typeof documents)["\n  mutation UpdateTransporterType($id: Short!, $transporterType: TransporterTypeDtoInput!) {\n    updateTransporterType(id: $id, command: { transporterType: $transporterType })\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;