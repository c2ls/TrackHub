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
    "\n  fragment ActionItem on ActionVm {\n    actionId\n    actionName\n    resourceId\n  }\n": typeof types.ActionItemFragmentDoc,
    "\n  query GetActions {\n    actions {\n      ...ActionItem\n    }\n  }\n": typeof types.GetActionsDocument,
    "\n  fragment ClientItem on ClientVm {\n    clientId\n    userId\n    name\n    description\n    processed\n    lastModified\n  }\n": typeof types.ClientItemFragmentDoc,
    "\n  query GetClients($skip: Int!, $take: Int!) {\n    clients(query: { skip: $skip, take: $take }) {\n      ...ClientItem\n    }\n  }\n": typeof types.GetClientsDocument,
    "\n  mutation CreateClient($client: ClientDtoInput!) {\n    createClient(command: { client: $client }) {\n      ...ClientItem\n      secret\n    }\n  }\n": typeof types.CreateClientDocument,
    "\n  mutation UpdateClient($id: UUID!, $client: ClientUserDtoInput!) {\n    updateClient(id: $id, command: { client: $client })\n  }\n": typeof types.UpdateClientDocument,
    "\n  mutation DeleteClient($id: UUID!) {\n    deleteClient(id: $id)\n  }\n": typeof types.DeleteClientDocument,
    "\n  fragment DriverCredentialItem on DriverCredentialVm {\n    driverCredentialId\n    driverId\n    accountId\n    normalizedLogin\n    failedAttempts\n    lockedUntil\n    verifiedAt\n    lastLoginAt\n    active\n    resetRequired\n    lastModified\n  }\n": typeof types.DriverCredentialItemFragmentDoc,
    "\n  fragment DriverDeviceItem on DriverDeviceRegistrationVm {\n    driverDeviceRegistrationId\n    driverId\n    accountId\n    deviceId\n    deviceName\n    platform\n    appVersion\n    pushToken\n    active\n    registeredAt\n    lastSeenAt\n    revokedAt\n    revokedBy\n    lastModified\n  }\n": typeof types.DriverDeviceItemFragmentDoc,
    "\n  query GetDriverCredentials($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {\n    driverCredentials(\n      query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }\n    ) {\n      ...DriverCredentialItem\n    }\n  }\n": typeof types.GetDriverCredentialsDocument,
    "\n  query GetDriverDevices($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {\n    driverDevices(query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }) {\n      ...DriverDeviceItem\n    }\n  }\n": typeof types.GetDriverDevicesDocument,
    "\n  mutation CreateDriverCredential($credential: DriverCredentialDtoInput!) {\n    createDriverCredential(command: { credential: $credential }) {\n      ...DriverCredentialItem\n    }\n  }\n": typeof types.CreateDriverCredentialDocument,
    "\n  mutation ActivateDriverCredential($driverCredentialId: UUID!, $password: String!) {\n    activateDriverCredential(\n      command: { driverCredentialId: $driverCredentialId, password: $password }\n    )\n  }\n": typeof types.ActivateDriverCredentialDocument,
    "\n  mutation LockDriverCredential($driverCredentialId: UUID!, $lockedUntil: DateTime!) {\n    lockDriverCredential(\n      command: { driverCredentialId: $driverCredentialId, lockedUntil: $lockedUntil }\n    )\n  }\n": typeof types.LockDriverCredentialDocument,
    "\n  mutation ResetDriverCredential(\n    $driverCredentialId: UUID!\n    $password: String!\n    $resetRequired: Boolean!\n  ) {\n    resetDriverCredential(\n      command: {\n        driverCredentialId: $driverCredentialId\n        password: $password\n        resetRequired: $resetRequired\n      }\n    )\n  }\n": typeof types.ResetDriverCredentialDocument,
    "\n  mutation RevokeDriverCredential($driverCredentialId: UUID!) {\n    revokeDriverCredential(command: { driverCredentialId: $driverCredentialId })\n  }\n": typeof types.RevokeDriverCredentialDocument,
    "\n  mutation RevokeDriverDevice($driverDeviceRegistrationId: UUID!, $revokedBy: String!) {\n    revokeDriverDevice(\n      command: { driverDeviceRegistrationId: $driverDeviceRegistrationId, revokedBy: $revokedBy }\n    )\n  }\n": typeof types.RevokeDriverDeviceDocument,
    "\n  query GetAuthorizedActions($userId: UUID!) {\n    authorizedActions(query: { userId: $userId }) {\n      resourceName\n      actionName\n    }\n  }\n": typeof types.GetAuthorizedActionsDocument,
    "\n  fragment PolicyItem on PolicyVm {\n    policyId\n    name\n  }\n": typeof types.PolicyItemFragmentDoc,
    "\n  fragment PolicyResourceTree on ResourceVm {\n    resourceId\n    resourceName\n    actions {\n      resourceId\n      actionName\n      actionId\n    }\n  }\n": typeof types.PolicyResourceTreeFragmentDoc,
    "\n  query GetPolicies {\n    policies {\n      ...PolicyItem\n    }\n  }\n": typeof types.GetPoliciesDocument,
    "\n  query GetResourcesByPolicy($policyId: Int!) {\n    resourcesByPolicy(query: { policyId: $policyId }) {\n      policyId\n      name\n      resources {\n        ...PolicyResourceTree\n      }\n    }\n  }\n": typeof types.GetResourcesByPolicyDocument,
    "\n  query GetUserLookupByPolicy($policyId: Int!) {\n    userLookupByPolicy(query: { policyId: $policyId }) {\n      userId\n      username\n    }\n  }\n": typeof types.GetUserLookupByPolicyDocument,
    "\n  mutation CreateResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {\n    createResourceActionPolicy(\n      command: {\n        resourceActionPolicy: { resourceId: $resourceId, actionId: $actionId, policyId: $policyId }\n      }\n    ) {\n      policyId\n    }\n  }\n": typeof types.CreateResourceActionPolicyDocument,
    "\n  mutation DeleteResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {\n    deleteResourceActionPolicy(resourceId: $resourceId, actionId: $actionId, policyId: $policyId)\n  }\n": typeof types.DeleteResourceActionPolicyDocument,
    "\n  mutation CreateUserPolicy($userId: UUID!, $policyId: Int!) {\n    createUserPolicy(command: { userPolicy: { userId: $userId, policyId: $policyId } }) {\n      userId\n      policyId\n    }\n  }\n": typeof types.CreateUserPolicyDocument,
    "\n  mutation DeleteUserPolicy($userId: UUID!, $policyId: Int!) {\n    deleteUserPolicy(userId: $userId, policyId: $policyId)\n  }\n": typeof types.DeleteUserPolicyDocument,
    "\n  fragment ResourceItem on ResourceVm {\n    resourceId\n    resourceName\n  }\n": typeof types.ResourceItemFragmentDoc,
    "\n  query GetResources {\n    resources {\n      ...ResourceItem\n    }\n  }\n": typeof types.GetResourcesDocument,
    "\n  fragment RoleItem on RoleVm {\n    roleId\n    name\n  }\n": typeof types.RoleItemFragmentDoc,
    "\n  fragment RoleResourceTree on ResourceVm {\n    resourceId\n    resourceName\n    actions {\n      actionId\n      actionName\n      resourceId\n    }\n  }\n": typeof types.RoleResourceTreeFragmentDoc,
    "\n  query GetRoles {\n    roles {\n      ...RoleItem\n    }\n  }\n": typeof types.GetRolesDocument,
    "\n  query GetResourcesByRole($roleId: Int!) {\n    resourcesByRole(query: { roleId: $roleId }) {\n      roleId\n      name\n      resources {\n        ...RoleResourceTree\n      }\n    }\n  }\n": typeof types.GetResourcesByRoleDocument,
    "\n  query GetUserLookupByRole($roleId: Int!) {\n    userLookupByRole(query: { roleId: $roleId }) {\n      userId\n      username\n    }\n  }\n": typeof types.GetUserLookupByRoleDocument,
    "\n  mutation CreateResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {\n    createResourceActionRole(\n      command: {\n        resourceActionRole: { resourceId: $resourceId, actionId: $actionId, roleId: $roleId }\n      }\n    ) {\n      roleId\n    }\n  }\n": typeof types.CreateResourceActionRoleDocument,
    "\n  mutation DeleteResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {\n    deleteResourceActionRole(resourceId: $resourceId, actionId: $actionId, roleId: $roleId)\n  }\n": typeof types.DeleteResourceActionRoleDocument,
    "\n  mutation CreateUserRole($userId: UUID!, $roleId: Int!) {\n    createUserRole(command: { userRole: { userId: $userId, roleId: $roleId } }) {\n      userId\n      roleId\n    }\n  }\n": typeof types.CreateUserRoleDocument,
    "\n  mutation DeleteUserRole($userId: UUID!, $roleId: Int!) {\n    deleteUserRole(userId: $userId, roleId: $roleId)\n  }\n": typeof types.DeleteUserRoleDocument,
    "\n  fragment ServiceClientPermissionItem on ServiceClientPermissionVm {\n    serviceClientPermissionId\n    clientId\n    accountId\n    resource\n    action\n    scope\n    audience\n    active\n    effectiveFrom\n    effectiveTo\n    lastModified\n  }\n": typeof types.ServiceClientPermissionItemFragmentDoc,
    "\n  query GetServiceClientPermissions(\n    $clientId: String\n    $accountId: UUID\n    $skip: Int!\n    $take: Int!\n  ) {\n    serviceClientPermissions(\n      query: { clientId: $clientId, accountId: $accountId, skip: $skip, take: $take }\n    ) {\n      ...ServiceClientPermissionItem\n    }\n  }\n": typeof types.GetServiceClientPermissionsDocument,
    "\n  mutation CreateServiceClientPermission($permission: ServiceClientPermissionDtoInput!) {\n    createServiceClientPermission(command: { permission: $permission }) {\n      ...ServiceClientPermissionItem\n    }\n  }\n": typeof types.CreateServiceClientPermissionDocument,
    "\n  mutation UpdateServiceClientPermission(\n    $serviceClientPermissionId: UUID!\n    $permission: ServiceClientPermissionDtoInput!\n  ) {\n    updateServiceClientPermission(\n      command: {\n        serviceClientPermissionId: $serviceClientPermissionId\n        permission: $permission\n      }\n    )\n  }\n": typeof types.UpdateServiceClientPermissionDocument,
    "\n  mutation DeleteServiceClientPermission($serviceClientPermissionId: UUID!) {\n    deleteServiceClientPermission(\n      command: { serviceClientPermissionId: $serviceClientPermissionId }\n    )\n  }\n": typeof types.DeleteServiceClientPermissionDocument,
    "\n  fragment UserDetail on UserVm {\n    userId\n    username\n    emailAddress\n    firstName\n    secondName\n    lastName\n    secondSurname\n    dob\n    loginAttempts\n    accountId\n    active\n    integrationUser\n  }\n": typeof types.UserDetailFragmentDoc,
    "\n  query GetCurrentUser {\n    currentUser {\n      userId\n      username\n      emailAddress\n      firstName\n      secondName\n      lastName\n      secondSurname\n      dob\n      loginAttempts\n      accountId\n      active\n      roles {\n        roleId\n        name\n      }\n      profiles {\n        policyId\n        name\n      }\n    }\n  }\n": typeof types.GetCurrentUserDocument,
    "\n  query GetIntegrationUsers {\n    users(query: { filter: { filters: [{ key: \"IntegrationUser\", value: true }] } }) {\n      userId\n      username\n      emailAddress\n    }\n  }\n": typeof types.GetIntegrationUsersDocument,
    "\n  query GetUsersByAccount($skip: Int, $take: Int, $search: String) {\n    usersByAccount(query: { skip: $skip, take: $take, search: $search }) {\n      items {\n        ...UserDetail\n        lockedUntil\n      }\n      totalCount\n    }\n  }\n": typeof types.GetUsersByAccountDocument,
    "\n  query GetUserLookupByAccount {\n    userLookupByAccount {\n      userId\n      username\n    }\n  }\n": typeof types.GetUserLookupByAccountDocument,
    "\n  mutation CreateUser($user: CreateUserDtoInput!) {\n    createUser(command: { user: $user }) {\n      ...UserDetail\n    }\n  }\n": typeof types.CreateUserDocument,
    "\n  mutation CreateManager($user: CreateUserDtoInput!, $accountId: UUID!) {\n    createManager(command: { user: $user, accountId: $accountId }) {\n      userId\n    }\n  }\n": typeof types.CreateManagerDocument,
    "\n  mutation UpdateUser($id: UUID!, $user: UpdateUserDtoInput!) {\n    updateUser(id: $id, command: { user: $user })\n  }\n": typeof types.UpdateUserDocument,
    "\n  mutation UpdateCurrentUser($user: UpdateCurrentUserDtoInput!) {\n    updateCurrentUser(command: { user: $user })\n  }\n": typeof types.UpdateCurrentUserDocument,
    "\n  mutation UpdatePassword($id: UUID!, $user: UserPasswordDtoInput!) {\n    updatePassword(id: $id, command: { user: $user })\n  }\n": typeof types.UpdatePasswordDocument,
    "\n  mutation DeleteUser($id: UUID!) {\n    deleteUser(id: $id)\n  }\n": typeof types.DeleteUserDocument,
    "\n  mutation UnlockUser($id: UUID!) {\n    unlockUser(id: $id)\n  }\n": typeof types.UnlockUserDocument,
    "\n  query UserIsAdmin {\n    userIsAdmin\n  }\n": typeof types.UserIsAdminDocument,
    "\n  query UserIsManager {\n    userIsManager\n  }\n": typeof types.UserIsManagerDocument,
};
const documents: Documents = {
    "\n  fragment ActionItem on ActionVm {\n    actionId\n    actionName\n    resourceId\n  }\n": types.ActionItemFragmentDoc,
    "\n  query GetActions {\n    actions {\n      ...ActionItem\n    }\n  }\n": types.GetActionsDocument,
    "\n  fragment ClientItem on ClientVm {\n    clientId\n    userId\n    name\n    description\n    processed\n    lastModified\n  }\n": types.ClientItemFragmentDoc,
    "\n  query GetClients($skip: Int!, $take: Int!) {\n    clients(query: { skip: $skip, take: $take }) {\n      ...ClientItem\n    }\n  }\n": types.GetClientsDocument,
    "\n  mutation CreateClient($client: ClientDtoInput!) {\n    createClient(command: { client: $client }) {\n      ...ClientItem\n      secret\n    }\n  }\n": types.CreateClientDocument,
    "\n  mutation UpdateClient($id: UUID!, $client: ClientUserDtoInput!) {\n    updateClient(id: $id, command: { client: $client })\n  }\n": types.UpdateClientDocument,
    "\n  mutation DeleteClient($id: UUID!) {\n    deleteClient(id: $id)\n  }\n": types.DeleteClientDocument,
    "\n  fragment DriverCredentialItem on DriverCredentialVm {\n    driverCredentialId\n    driverId\n    accountId\n    normalizedLogin\n    failedAttempts\n    lockedUntil\n    verifiedAt\n    lastLoginAt\n    active\n    resetRequired\n    lastModified\n  }\n": types.DriverCredentialItemFragmentDoc,
    "\n  fragment DriverDeviceItem on DriverDeviceRegistrationVm {\n    driverDeviceRegistrationId\n    driverId\n    accountId\n    deviceId\n    deviceName\n    platform\n    appVersion\n    pushToken\n    active\n    registeredAt\n    lastSeenAt\n    revokedAt\n    revokedBy\n    lastModified\n  }\n": types.DriverDeviceItemFragmentDoc,
    "\n  query GetDriverCredentials($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {\n    driverCredentials(\n      query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }\n    ) {\n      ...DriverCredentialItem\n    }\n  }\n": types.GetDriverCredentialsDocument,
    "\n  query GetDriverDevices($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {\n    driverDevices(query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }) {\n      ...DriverDeviceItem\n    }\n  }\n": types.GetDriverDevicesDocument,
    "\n  mutation CreateDriverCredential($credential: DriverCredentialDtoInput!) {\n    createDriverCredential(command: { credential: $credential }) {\n      ...DriverCredentialItem\n    }\n  }\n": types.CreateDriverCredentialDocument,
    "\n  mutation ActivateDriverCredential($driverCredentialId: UUID!, $password: String!) {\n    activateDriverCredential(\n      command: { driverCredentialId: $driverCredentialId, password: $password }\n    )\n  }\n": types.ActivateDriverCredentialDocument,
    "\n  mutation LockDriverCredential($driverCredentialId: UUID!, $lockedUntil: DateTime!) {\n    lockDriverCredential(\n      command: { driverCredentialId: $driverCredentialId, lockedUntil: $lockedUntil }\n    )\n  }\n": types.LockDriverCredentialDocument,
    "\n  mutation ResetDriverCredential(\n    $driverCredentialId: UUID!\n    $password: String!\n    $resetRequired: Boolean!\n  ) {\n    resetDriverCredential(\n      command: {\n        driverCredentialId: $driverCredentialId\n        password: $password\n        resetRequired: $resetRequired\n      }\n    )\n  }\n": types.ResetDriverCredentialDocument,
    "\n  mutation RevokeDriverCredential($driverCredentialId: UUID!) {\n    revokeDriverCredential(command: { driverCredentialId: $driverCredentialId })\n  }\n": types.RevokeDriverCredentialDocument,
    "\n  mutation RevokeDriverDevice($driverDeviceRegistrationId: UUID!, $revokedBy: String!) {\n    revokeDriverDevice(\n      command: { driverDeviceRegistrationId: $driverDeviceRegistrationId, revokedBy: $revokedBy }\n    )\n  }\n": types.RevokeDriverDeviceDocument,
    "\n  query GetAuthorizedActions($userId: UUID!) {\n    authorizedActions(query: { userId: $userId }) {\n      resourceName\n      actionName\n    }\n  }\n": types.GetAuthorizedActionsDocument,
    "\n  fragment PolicyItem on PolicyVm {\n    policyId\n    name\n  }\n": types.PolicyItemFragmentDoc,
    "\n  fragment PolicyResourceTree on ResourceVm {\n    resourceId\n    resourceName\n    actions {\n      resourceId\n      actionName\n      actionId\n    }\n  }\n": types.PolicyResourceTreeFragmentDoc,
    "\n  query GetPolicies {\n    policies {\n      ...PolicyItem\n    }\n  }\n": types.GetPoliciesDocument,
    "\n  query GetResourcesByPolicy($policyId: Int!) {\n    resourcesByPolicy(query: { policyId: $policyId }) {\n      policyId\n      name\n      resources {\n        ...PolicyResourceTree\n      }\n    }\n  }\n": types.GetResourcesByPolicyDocument,
    "\n  query GetUserLookupByPolicy($policyId: Int!) {\n    userLookupByPolicy(query: { policyId: $policyId }) {\n      userId\n      username\n    }\n  }\n": types.GetUserLookupByPolicyDocument,
    "\n  mutation CreateResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {\n    createResourceActionPolicy(\n      command: {\n        resourceActionPolicy: { resourceId: $resourceId, actionId: $actionId, policyId: $policyId }\n      }\n    ) {\n      policyId\n    }\n  }\n": types.CreateResourceActionPolicyDocument,
    "\n  mutation DeleteResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {\n    deleteResourceActionPolicy(resourceId: $resourceId, actionId: $actionId, policyId: $policyId)\n  }\n": types.DeleteResourceActionPolicyDocument,
    "\n  mutation CreateUserPolicy($userId: UUID!, $policyId: Int!) {\n    createUserPolicy(command: { userPolicy: { userId: $userId, policyId: $policyId } }) {\n      userId\n      policyId\n    }\n  }\n": types.CreateUserPolicyDocument,
    "\n  mutation DeleteUserPolicy($userId: UUID!, $policyId: Int!) {\n    deleteUserPolicy(userId: $userId, policyId: $policyId)\n  }\n": types.DeleteUserPolicyDocument,
    "\n  fragment ResourceItem on ResourceVm {\n    resourceId\n    resourceName\n  }\n": types.ResourceItemFragmentDoc,
    "\n  query GetResources {\n    resources {\n      ...ResourceItem\n    }\n  }\n": types.GetResourcesDocument,
    "\n  fragment RoleItem on RoleVm {\n    roleId\n    name\n  }\n": types.RoleItemFragmentDoc,
    "\n  fragment RoleResourceTree on ResourceVm {\n    resourceId\n    resourceName\n    actions {\n      actionId\n      actionName\n      resourceId\n    }\n  }\n": types.RoleResourceTreeFragmentDoc,
    "\n  query GetRoles {\n    roles {\n      ...RoleItem\n    }\n  }\n": types.GetRolesDocument,
    "\n  query GetResourcesByRole($roleId: Int!) {\n    resourcesByRole(query: { roleId: $roleId }) {\n      roleId\n      name\n      resources {\n        ...RoleResourceTree\n      }\n    }\n  }\n": types.GetResourcesByRoleDocument,
    "\n  query GetUserLookupByRole($roleId: Int!) {\n    userLookupByRole(query: { roleId: $roleId }) {\n      userId\n      username\n    }\n  }\n": types.GetUserLookupByRoleDocument,
    "\n  mutation CreateResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {\n    createResourceActionRole(\n      command: {\n        resourceActionRole: { resourceId: $resourceId, actionId: $actionId, roleId: $roleId }\n      }\n    ) {\n      roleId\n    }\n  }\n": types.CreateResourceActionRoleDocument,
    "\n  mutation DeleteResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {\n    deleteResourceActionRole(resourceId: $resourceId, actionId: $actionId, roleId: $roleId)\n  }\n": types.DeleteResourceActionRoleDocument,
    "\n  mutation CreateUserRole($userId: UUID!, $roleId: Int!) {\n    createUserRole(command: { userRole: { userId: $userId, roleId: $roleId } }) {\n      userId\n      roleId\n    }\n  }\n": types.CreateUserRoleDocument,
    "\n  mutation DeleteUserRole($userId: UUID!, $roleId: Int!) {\n    deleteUserRole(userId: $userId, roleId: $roleId)\n  }\n": types.DeleteUserRoleDocument,
    "\n  fragment ServiceClientPermissionItem on ServiceClientPermissionVm {\n    serviceClientPermissionId\n    clientId\n    accountId\n    resource\n    action\n    scope\n    audience\n    active\n    effectiveFrom\n    effectiveTo\n    lastModified\n  }\n": types.ServiceClientPermissionItemFragmentDoc,
    "\n  query GetServiceClientPermissions(\n    $clientId: String\n    $accountId: UUID\n    $skip: Int!\n    $take: Int!\n  ) {\n    serviceClientPermissions(\n      query: { clientId: $clientId, accountId: $accountId, skip: $skip, take: $take }\n    ) {\n      ...ServiceClientPermissionItem\n    }\n  }\n": types.GetServiceClientPermissionsDocument,
    "\n  mutation CreateServiceClientPermission($permission: ServiceClientPermissionDtoInput!) {\n    createServiceClientPermission(command: { permission: $permission }) {\n      ...ServiceClientPermissionItem\n    }\n  }\n": types.CreateServiceClientPermissionDocument,
    "\n  mutation UpdateServiceClientPermission(\n    $serviceClientPermissionId: UUID!\n    $permission: ServiceClientPermissionDtoInput!\n  ) {\n    updateServiceClientPermission(\n      command: {\n        serviceClientPermissionId: $serviceClientPermissionId\n        permission: $permission\n      }\n    )\n  }\n": types.UpdateServiceClientPermissionDocument,
    "\n  mutation DeleteServiceClientPermission($serviceClientPermissionId: UUID!) {\n    deleteServiceClientPermission(\n      command: { serviceClientPermissionId: $serviceClientPermissionId }\n    )\n  }\n": types.DeleteServiceClientPermissionDocument,
    "\n  fragment UserDetail on UserVm {\n    userId\n    username\n    emailAddress\n    firstName\n    secondName\n    lastName\n    secondSurname\n    dob\n    loginAttempts\n    accountId\n    active\n    integrationUser\n  }\n": types.UserDetailFragmentDoc,
    "\n  query GetCurrentUser {\n    currentUser {\n      userId\n      username\n      emailAddress\n      firstName\n      secondName\n      lastName\n      secondSurname\n      dob\n      loginAttempts\n      accountId\n      active\n      roles {\n        roleId\n        name\n      }\n      profiles {\n        policyId\n        name\n      }\n    }\n  }\n": types.GetCurrentUserDocument,
    "\n  query GetIntegrationUsers {\n    users(query: { filter: { filters: [{ key: \"IntegrationUser\", value: true }] } }) {\n      userId\n      username\n      emailAddress\n    }\n  }\n": types.GetIntegrationUsersDocument,
    "\n  query GetUsersByAccount($skip: Int, $take: Int, $search: String) {\n    usersByAccount(query: { skip: $skip, take: $take, search: $search }) {\n      items {\n        ...UserDetail\n        lockedUntil\n      }\n      totalCount\n    }\n  }\n": types.GetUsersByAccountDocument,
    "\n  query GetUserLookupByAccount {\n    userLookupByAccount {\n      userId\n      username\n    }\n  }\n": types.GetUserLookupByAccountDocument,
    "\n  mutation CreateUser($user: CreateUserDtoInput!) {\n    createUser(command: { user: $user }) {\n      ...UserDetail\n    }\n  }\n": types.CreateUserDocument,
    "\n  mutation CreateManager($user: CreateUserDtoInput!, $accountId: UUID!) {\n    createManager(command: { user: $user, accountId: $accountId }) {\n      userId\n    }\n  }\n": types.CreateManagerDocument,
    "\n  mutation UpdateUser($id: UUID!, $user: UpdateUserDtoInput!) {\n    updateUser(id: $id, command: { user: $user })\n  }\n": types.UpdateUserDocument,
    "\n  mutation UpdateCurrentUser($user: UpdateCurrentUserDtoInput!) {\n    updateCurrentUser(command: { user: $user })\n  }\n": types.UpdateCurrentUserDocument,
    "\n  mutation UpdatePassword($id: UUID!, $user: UserPasswordDtoInput!) {\n    updatePassword(id: $id, command: { user: $user })\n  }\n": types.UpdatePasswordDocument,
    "\n  mutation DeleteUser($id: UUID!) {\n    deleteUser(id: $id)\n  }\n": types.DeleteUserDocument,
    "\n  mutation UnlockUser($id: UUID!) {\n    unlockUser(id: $id)\n  }\n": types.UnlockUserDocument,
    "\n  query UserIsAdmin {\n    userIsAdmin\n  }\n": types.UserIsAdminDocument,
    "\n  query UserIsManager {\n    userIsManager\n  }\n": types.UserIsManagerDocument,
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
export function graphql(source: "\n  fragment ActionItem on ActionVm {\n    actionId\n    actionName\n    resourceId\n  }\n"): (typeof documents)["\n  fragment ActionItem on ActionVm {\n    actionId\n    actionName\n    resourceId\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetActions {\n    actions {\n      ...ActionItem\n    }\n  }\n"): (typeof documents)["\n  query GetActions {\n    actions {\n      ...ActionItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ClientItem on ClientVm {\n    clientId\n    userId\n    name\n    description\n    processed\n    lastModified\n  }\n"): (typeof documents)["\n  fragment ClientItem on ClientVm {\n    clientId\n    userId\n    name\n    description\n    processed\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetClients($skip: Int!, $take: Int!) {\n    clients(query: { skip: $skip, take: $take }) {\n      ...ClientItem\n    }\n  }\n"): (typeof documents)["\n  query GetClients($skip: Int!, $take: Int!) {\n    clients(query: { skip: $skip, take: $take }) {\n      ...ClientItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateClient($client: ClientDtoInput!) {\n    createClient(command: { client: $client }) {\n      ...ClientItem\n      secret\n    }\n  }\n"): (typeof documents)["\n  mutation CreateClient($client: ClientDtoInput!) {\n    createClient(command: { client: $client }) {\n      ...ClientItem\n      secret\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateClient($id: UUID!, $client: ClientUserDtoInput!) {\n    updateClient(id: $id, command: { client: $client })\n  }\n"): (typeof documents)["\n  mutation UpdateClient($id: UUID!, $client: ClientUserDtoInput!) {\n    updateClient(id: $id, command: { client: $client })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteClient($id: UUID!) {\n    deleteClient(id: $id)\n  }\n"): (typeof documents)["\n  mutation DeleteClient($id: UUID!) {\n    deleteClient(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DriverCredentialItem on DriverCredentialVm {\n    driverCredentialId\n    driverId\n    accountId\n    normalizedLogin\n    failedAttempts\n    lockedUntil\n    verifiedAt\n    lastLoginAt\n    active\n    resetRequired\n    lastModified\n  }\n"): (typeof documents)["\n  fragment DriverCredentialItem on DriverCredentialVm {\n    driverCredentialId\n    driverId\n    accountId\n    normalizedLogin\n    failedAttempts\n    lockedUntil\n    verifiedAt\n    lastLoginAt\n    active\n    resetRequired\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DriverDeviceItem on DriverDeviceRegistrationVm {\n    driverDeviceRegistrationId\n    driverId\n    accountId\n    deviceId\n    deviceName\n    platform\n    appVersion\n    pushToken\n    active\n    registeredAt\n    lastSeenAt\n    revokedAt\n    revokedBy\n    lastModified\n  }\n"): (typeof documents)["\n  fragment DriverDeviceItem on DriverDeviceRegistrationVm {\n    driverDeviceRegistrationId\n    driverId\n    accountId\n    deviceId\n    deviceName\n    platform\n    appVersion\n    pushToken\n    active\n    registeredAt\n    lastSeenAt\n    revokedAt\n    revokedBy\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDriverCredentials($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {\n    driverCredentials(\n      query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }\n    ) {\n      ...DriverCredentialItem\n    }\n  }\n"): (typeof documents)["\n  query GetDriverCredentials($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {\n    driverCredentials(\n      query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }\n    ) {\n      ...DriverCredentialItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetDriverDevices($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {\n    driverDevices(query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }) {\n      ...DriverDeviceItem\n    }\n  }\n"): (typeof documents)["\n  query GetDriverDevices($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {\n    driverDevices(query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }) {\n      ...DriverDeviceItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateDriverCredential($credential: DriverCredentialDtoInput!) {\n    createDriverCredential(command: { credential: $credential }) {\n      ...DriverCredentialItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreateDriverCredential($credential: DriverCredentialDtoInput!) {\n    createDriverCredential(command: { credential: $credential }) {\n      ...DriverCredentialItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation ActivateDriverCredential($driverCredentialId: UUID!, $password: String!) {\n    activateDriverCredential(\n      command: { driverCredentialId: $driverCredentialId, password: $password }\n    )\n  }\n"): (typeof documents)["\n  mutation ActivateDriverCredential($driverCredentialId: UUID!, $password: String!) {\n    activateDriverCredential(\n      command: { driverCredentialId: $driverCredentialId, password: $password }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation LockDriverCredential($driverCredentialId: UUID!, $lockedUntil: DateTime!) {\n    lockDriverCredential(\n      command: { driverCredentialId: $driverCredentialId, lockedUntil: $lockedUntil }\n    )\n  }\n"): (typeof documents)["\n  mutation LockDriverCredential($driverCredentialId: UUID!, $lockedUntil: DateTime!) {\n    lockDriverCredential(\n      command: { driverCredentialId: $driverCredentialId, lockedUntil: $lockedUntil }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation ResetDriverCredential(\n    $driverCredentialId: UUID!\n    $password: String!\n    $resetRequired: Boolean!\n  ) {\n    resetDriverCredential(\n      command: {\n        driverCredentialId: $driverCredentialId\n        password: $password\n        resetRequired: $resetRequired\n      }\n    )\n  }\n"): (typeof documents)["\n  mutation ResetDriverCredential(\n    $driverCredentialId: UUID!\n    $password: String!\n    $resetRequired: Boolean!\n  ) {\n    resetDriverCredential(\n      command: {\n        driverCredentialId: $driverCredentialId\n        password: $password\n        resetRequired: $resetRequired\n      }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation RevokeDriverCredential($driverCredentialId: UUID!) {\n    revokeDriverCredential(command: { driverCredentialId: $driverCredentialId })\n  }\n"): (typeof documents)["\n  mutation RevokeDriverCredential($driverCredentialId: UUID!) {\n    revokeDriverCredential(command: { driverCredentialId: $driverCredentialId })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation RevokeDriverDevice($driverDeviceRegistrationId: UUID!, $revokedBy: String!) {\n    revokeDriverDevice(\n      command: { driverDeviceRegistrationId: $driverDeviceRegistrationId, revokedBy: $revokedBy }\n    )\n  }\n"): (typeof documents)["\n  mutation RevokeDriverDevice($driverDeviceRegistrationId: UUID!, $revokedBy: String!) {\n    revokeDriverDevice(\n      command: { driverDeviceRegistrationId: $driverDeviceRegistrationId, revokedBy: $revokedBy }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetAuthorizedActions($userId: UUID!) {\n    authorizedActions(query: { userId: $userId }) {\n      resourceName\n      actionName\n    }\n  }\n"): (typeof documents)["\n  query GetAuthorizedActions($userId: UUID!) {\n    authorizedActions(query: { userId: $userId }) {\n      resourceName\n      actionName\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment PolicyItem on PolicyVm {\n    policyId\n    name\n  }\n"): (typeof documents)["\n  fragment PolicyItem on PolicyVm {\n    policyId\n    name\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment PolicyResourceTree on ResourceVm {\n    resourceId\n    resourceName\n    actions {\n      resourceId\n      actionName\n      actionId\n    }\n  }\n"): (typeof documents)["\n  fragment PolicyResourceTree on ResourceVm {\n    resourceId\n    resourceName\n    actions {\n      resourceId\n      actionName\n      actionId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetPolicies {\n    policies {\n      ...PolicyItem\n    }\n  }\n"): (typeof documents)["\n  query GetPolicies {\n    policies {\n      ...PolicyItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetResourcesByPolicy($policyId: Int!) {\n    resourcesByPolicy(query: { policyId: $policyId }) {\n      policyId\n      name\n      resources {\n        ...PolicyResourceTree\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetResourcesByPolicy($policyId: Int!) {\n    resourcesByPolicy(query: { policyId: $policyId }) {\n      policyId\n      name\n      resources {\n        ...PolicyResourceTree\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetUserLookupByPolicy($policyId: Int!) {\n    userLookupByPolicy(query: { policyId: $policyId }) {\n      userId\n      username\n    }\n  }\n"): (typeof documents)["\n  query GetUserLookupByPolicy($policyId: Int!) {\n    userLookupByPolicy(query: { policyId: $policyId }) {\n      userId\n      username\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {\n    createResourceActionPolicy(\n      command: {\n        resourceActionPolicy: { resourceId: $resourceId, actionId: $actionId, policyId: $policyId }\n      }\n    ) {\n      policyId\n    }\n  }\n"): (typeof documents)["\n  mutation CreateResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {\n    createResourceActionPolicy(\n      command: {\n        resourceActionPolicy: { resourceId: $resourceId, actionId: $actionId, policyId: $policyId }\n      }\n    ) {\n      policyId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {\n    deleteResourceActionPolicy(resourceId: $resourceId, actionId: $actionId, policyId: $policyId)\n  }\n"): (typeof documents)["\n  mutation DeleteResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {\n    deleteResourceActionPolicy(resourceId: $resourceId, actionId: $actionId, policyId: $policyId)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateUserPolicy($userId: UUID!, $policyId: Int!) {\n    createUserPolicy(command: { userPolicy: { userId: $userId, policyId: $policyId } }) {\n      userId\n      policyId\n    }\n  }\n"): (typeof documents)["\n  mutation CreateUserPolicy($userId: UUID!, $policyId: Int!) {\n    createUserPolicy(command: { userPolicy: { userId: $userId, policyId: $policyId } }) {\n      userId\n      policyId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteUserPolicy($userId: UUID!, $policyId: Int!) {\n    deleteUserPolicy(userId: $userId, policyId: $policyId)\n  }\n"): (typeof documents)["\n  mutation DeleteUserPolicy($userId: UUID!, $policyId: Int!) {\n    deleteUserPolicy(userId: $userId, policyId: $policyId)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ResourceItem on ResourceVm {\n    resourceId\n    resourceName\n  }\n"): (typeof documents)["\n  fragment ResourceItem on ResourceVm {\n    resourceId\n    resourceName\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetResources {\n    resources {\n      ...ResourceItem\n    }\n  }\n"): (typeof documents)["\n  query GetResources {\n    resources {\n      ...ResourceItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment RoleItem on RoleVm {\n    roleId\n    name\n  }\n"): (typeof documents)["\n  fragment RoleItem on RoleVm {\n    roleId\n    name\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment RoleResourceTree on ResourceVm {\n    resourceId\n    resourceName\n    actions {\n      actionId\n      actionName\n      resourceId\n    }\n  }\n"): (typeof documents)["\n  fragment RoleResourceTree on ResourceVm {\n    resourceId\n    resourceName\n    actions {\n      actionId\n      actionName\n      resourceId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetRoles {\n    roles {\n      ...RoleItem\n    }\n  }\n"): (typeof documents)["\n  query GetRoles {\n    roles {\n      ...RoleItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetResourcesByRole($roleId: Int!) {\n    resourcesByRole(query: { roleId: $roleId }) {\n      roleId\n      name\n      resources {\n        ...RoleResourceTree\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetResourcesByRole($roleId: Int!) {\n    resourcesByRole(query: { roleId: $roleId }) {\n      roleId\n      name\n      resources {\n        ...RoleResourceTree\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetUserLookupByRole($roleId: Int!) {\n    userLookupByRole(query: { roleId: $roleId }) {\n      userId\n      username\n    }\n  }\n"): (typeof documents)["\n  query GetUserLookupByRole($roleId: Int!) {\n    userLookupByRole(query: { roleId: $roleId }) {\n      userId\n      username\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {\n    createResourceActionRole(\n      command: {\n        resourceActionRole: { resourceId: $resourceId, actionId: $actionId, roleId: $roleId }\n      }\n    ) {\n      roleId\n    }\n  }\n"): (typeof documents)["\n  mutation CreateResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {\n    createResourceActionRole(\n      command: {\n        resourceActionRole: { resourceId: $resourceId, actionId: $actionId, roleId: $roleId }\n      }\n    ) {\n      roleId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {\n    deleteResourceActionRole(resourceId: $resourceId, actionId: $actionId, roleId: $roleId)\n  }\n"): (typeof documents)["\n  mutation DeleteResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {\n    deleteResourceActionRole(resourceId: $resourceId, actionId: $actionId, roleId: $roleId)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateUserRole($userId: UUID!, $roleId: Int!) {\n    createUserRole(command: { userRole: { userId: $userId, roleId: $roleId } }) {\n      userId\n      roleId\n    }\n  }\n"): (typeof documents)["\n  mutation CreateUserRole($userId: UUID!, $roleId: Int!) {\n    createUserRole(command: { userRole: { userId: $userId, roleId: $roleId } }) {\n      userId\n      roleId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteUserRole($userId: UUID!, $roleId: Int!) {\n    deleteUserRole(userId: $userId, roleId: $roleId)\n  }\n"): (typeof documents)["\n  mutation DeleteUserRole($userId: UUID!, $roleId: Int!) {\n    deleteUserRole(userId: $userId, roleId: $roleId)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ServiceClientPermissionItem on ServiceClientPermissionVm {\n    serviceClientPermissionId\n    clientId\n    accountId\n    resource\n    action\n    scope\n    audience\n    active\n    effectiveFrom\n    effectiveTo\n    lastModified\n  }\n"): (typeof documents)["\n  fragment ServiceClientPermissionItem on ServiceClientPermissionVm {\n    serviceClientPermissionId\n    clientId\n    accountId\n    resource\n    action\n    scope\n    audience\n    active\n    effectiveFrom\n    effectiveTo\n    lastModified\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetServiceClientPermissions(\n    $clientId: String\n    $accountId: UUID\n    $skip: Int!\n    $take: Int!\n  ) {\n    serviceClientPermissions(\n      query: { clientId: $clientId, accountId: $accountId, skip: $skip, take: $take }\n    ) {\n      ...ServiceClientPermissionItem\n    }\n  }\n"): (typeof documents)["\n  query GetServiceClientPermissions(\n    $clientId: String\n    $accountId: UUID\n    $skip: Int!\n    $take: Int!\n  ) {\n    serviceClientPermissions(\n      query: { clientId: $clientId, accountId: $accountId, skip: $skip, take: $take }\n    ) {\n      ...ServiceClientPermissionItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateServiceClientPermission($permission: ServiceClientPermissionDtoInput!) {\n    createServiceClientPermission(command: { permission: $permission }) {\n      ...ServiceClientPermissionItem\n    }\n  }\n"): (typeof documents)["\n  mutation CreateServiceClientPermission($permission: ServiceClientPermissionDtoInput!) {\n    createServiceClientPermission(command: { permission: $permission }) {\n      ...ServiceClientPermissionItem\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateServiceClientPermission(\n    $serviceClientPermissionId: UUID!\n    $permission: ServiceClientPermissionDtoInput!\n  ) {\n    updateServiceClientPermission(\n      command: {\n        serviceClientPermissionId: $serviceClientPermissionId\n        permission: $permission\n      }\n    )\n  }\n"): (typeof documents)["\n  mutation UpdateServiceClientPermission(\n    $serviceClientPermissionId: UUID!\n    $permission: ServiceClientPermissionDtoInput!\n  ) {\n    updateServiceClientPermission(\n      command: {\n        serviceClientPermissionId: $serviceClientPermissionId\n        permission: $permission\n      }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteServiceClientPermission($serviceClientPermissionId: UUID!) {\n    deleteServiceClientPermission(\n      command: { serviceClientPermissionId: $serviceClientPermissionId }\n    )\n  }\n"): (typeof documents)["\n  mutation DeleteServiceClientPermission($serviceClientPermissionId: UUID!) {\n    deleteServiceClientPermission(\n      command: { serviceClientPermissionId: $serviceClientPermissionId }\n    )\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment UserDetail on UserVm {\n    userId\n    username\n    emailAddress\n    firstName\n    secondName\n    lastName\n    secondSurname\n    dob\n    loginAttempts\n    accountId\n    active\n    integrationUser\n  }\n"): (typeof documents)["\n  fragment UserDetail on UserVm {\n    userId\n    username\n    emailAddress\n    firstName\n    secondName\n    lastName\n    secondSurname\n    dob\n    loginAttempts\n    accountId\n    active\n    integrationUser\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetCurrentUser {\n    currentUser {\n      userId\n      username\n      emailAddress\n      firstName\n      secondName\n      lastName\n      secondSurname\n      dob\n      loginAttempts\n      accountId\n      active\n      roles {\n        roleId\n        name\n      }\n      profiles {\n        policyId\n        name\n      }\n    }\n  }\n"): (typeof documents)["\n  query GetCurrentUser {\n    currentUser {\n      userId\n      username\n      emailAddress\n      firstName\n      secondName\n      lastName\n      secondSurname\n      dob\n      loginAttempts\n      accountId\n      active\n      roles {\n        roleId\n        name\n      }\n      profiles {\n        policyId\n        name\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetIntegrationUsers {\n    users(query: { filter: { filters: [{ key: \"IntegrationUser\", value: true }] } }) {\n      userId\n      username\n      emailAddress\n    }\n  }\n"): (typeof documents)["\n  query GetIntegrationUsers {\n    users(query: { filter: { filters: [{ key: \"IntegrationUser\", value: true }] } }) {\n      userId\n      username\n      emailAddress\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetUsersByAccount($skip: Int, $take: Int, $search: String) {\n    usersByAccount(query: { skip: $skip, take: $take, search: $search }) {\n      items {\n        ...UserDetail\n        lockedUntil\n      }\n      totalCount\n    }\n  }\n"): (typeof documents)["\n  query GetUsersByAccount($skip: Int, $take: Int, $search: String) {\n    usersByAccount(query: { skip: $skip, take: $take, search: $search }) {\n      items {\n        ...UserDetail\n        lockedUntil\n      }\n      totalCount\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query GetUserLookupByAccount {\n    userLookupByAccount {\n      userId\n      username\n    }\n  }\n"): (typeof documents)["\n  query GetUserLookupByAccount {\n    userLookupByAccount {\n      userId\n      username\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateUser($user: CreateUserDtoInput!) {\n    createUser(command: { user: $user }) {\n      ...UserDetail\n    }\n  }\n"): (typeof documents)["\n  mutation CreateUser($user: CreateUserDtoInput!) {\n    createUser(command: { user: $user }) {\n      ...UserDetail\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation CreateManager($user: CreateUserDtoInput!, $accountId: UUID!) {\n    createManager(command: { user: $user, accountId: $accountId }) {\n      userId\n    }\n  }\n"): (typeof documents)["\n  mutation CreateManager($user: CreateUserDtoInput!, $accountId: UUID!) {\n    createManager(command: { user: $user, accountId: $accountId }) {\n      userId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateUser($id: UUID!, $user: UpdateUserDtoInput!) {\n    updateUser(id: $id, command: { user: $user })\n  }\n"): (typeof documents)["\n  mutation UpdateUser($id: UUID!, $user: UpdateUserDtoInput!) {\n    updateUser(id: $id, command: { user: $user })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateCurrentUser($user: UpdateCurrentUserDtoInput!) {\n    updateCurrentUser(command: { user: $user })\n  }\n"): (typeof documents)["\n  mutation UpdateCurrentUser($user: UpdateCurrentUserDtoInput!) {\n    updateCurrentUser(command: { user: $user })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdatePassword($id: UUID!, $user: UserPasswordDtoInput!) {\n    updatePassword(id: $id, command: { user: $user })\n  }\n"): (typeof documents)["\n  mutation UpdatePassword($id: UUID!, $user: UserPasswordDtoInput!) {\n    updatePassword(id: $id, command: { user: $user })\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeleteUser($id: UUID!) {\n    deleteUser(id: $id)\n  }\n"): (typeof documents)["\n  mutation DeleteUser($id: UUID!) {\n    deleteUser(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UnlockUser($id: UUID!) {\n    unlockUser(id: $id)\n  }\n"): (typeof documents)["\n  mutation UnlockUser($id: UUID!) {\n    unlockUser(id: $id)\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query UserIsAdmin {\n    userIsAdmin\n  }\n"): (typeof documents)["\n  query UserIsAdmin {\n    userIsAdmin\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query UserIsManager {\n    userIsManager\n  }\n"): (typeof documents)["\n  query UserIsManager {\n    userIsManager\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;