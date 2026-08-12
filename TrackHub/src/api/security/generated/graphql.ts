/* eslint-disable */
/** Internal type. DO NOT USE DIRECTLY. */
type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
/** Internal type. DO NOT USE DIRECTLY. */
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type ClientDtoInput = {
  description: string;
  name: string;
  secret: string;
  userId?: string | null | undefined;
};

export type ClientUserDtoInput = {
  clientId: string;
  userId: string;
};

export type CreateUserDtoInput = {
  active: boolean;
  dob?: string | null | undefined;
  emailAddress: string;
  firstName: string;
  integrationUser: boolean;
  lastName: string;
  password: string;
  secondName?: string | null | undefined;
  secondSurname?: string | null | undefined;
  username: string;
};

export type ServiceClientPermissionDtoInput = {
  accountId?: string | null | undefined;
  action: string;
  active: boolean;
  audience: string;
  clientId: string;
  effectiveFrom?: string | null | undefined;
  effectiveTo?: string | null | undefined;
  resource: string;
  scope: string;
};

export type UpdateCurrentUserDtoInput = {
  dob?: string | null | undefined;
  firstName: string;
  lastName: string;
  secondName?: string | null | undefined;
  secondSurname?: string | null | undefined;
};

export type UpdateUserDtoInput = {
  active: boolean;
  dob?: string | null | undefined;
  emailAddress: string;
  firstName: string;
  integrationUser: boolean;
  lastName: string;
  secondName?: string | null | undefined;
  secondSurname?: string | null | undefined;
  userId: string;
  username: string;
};

export type UserPasswordDtoInput = {
  password: string;
  userId: string;
};

export type ActionItemFragment = { actionId: number, actionName: string, resourceId: number };

export type GetActionsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetActionsQuery = { actions: Array<{ actionId: number, actionName: string, resourceId: number }> };

export type ClientItemFragment = { clientId: string, userId: string | null, name: string, description: string, processed: boolean, lastModified: string };

export type GetClientsQueryVariables = Exact<{
  skip: number;
  take: number;
}>;


export type GetClientsQuery = { clients: Array<{ clientId: string, userId: string | null, name: string, description: string, processed: boolean, lastModified: string }> };

export type CreateClientMutationVariables = Exact<{
  client: ClientDtoInput;
}>;


export type CreateClientMutation = { createClient: { secret: string, clientId: string, userId: string | null, name: string, description: string, processed: boolean, lastModified: string } };

export type UpdateClientMutationVariables = Exact<{
  id: string;
  client: ClientUserDtoInput;
}>;


export type UpdateClientMutation = { updateClient: boolean };

export type DeleteClientMutationVariables = Exact<{
  id: string;
}>;


export type DeleteClientMutation = { deleteClient: string };

export type PolicyItemFragment = { policyId: number, name: string };

export type PolicyResourceTreeFragment = { resourceId: number, resourceName: string, actions: Array<{ resourceId: number, actionName: string, actionId: number }> | null };

export type GetPoliciesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetPoliciesQuery = { policies: Array<{ policyId: number, name: string }> };

export type GetResourcesByPolicyQueryVariables = Exact<{
  policyId: number;
}>;


export type GetResourcesByPolicyQuery = { resourcesByPolicy: { policyId: number, name: string, resources: Array<{ resourceId: number, resourceName: string, actions: Array<{ resourceId: number, actionName: string, actionId: number }> | null }> } };

export type GetUsersByPolicyQueryVariables = Exact<{
  policyId: number;
}>;


export type GetUsersByPolicyQuery = { usersByPolicy: Array<{ userId: string, username: string, firstName: string, lastName: string }> };

export type CreateResourceActionPolicyMutationVariables = Exact<{
  resourceId: number;
  actionId: number;
  policyId: number;
}>;


export type CreateResourceActionPolicyMutation = { createResourceActionPolicy: { policyId: number } };

export type DeleteResourceActionPolicyMutationVariables = Exact<{
  resourceId: number;
  actionId: number;
  policyId: number;
}>;


export type DeleteResourceActionPolicyMutation = { deleteResourceActionPolicy: number };

export type CreateUserPolicyMutationVariables = Exact<{
  userId: string;
  policyId: number;
}>;


export type CreateUserPolicyMutation = { createUserPolicy: { userId: string, policyId: number } };

export type DeleteUserPolicyMutationVariables = Exact<{
  userId: string;
  policyId: number;
}>;


export type DeleteUserPolicyMutation = { deleteUserPolicy: string };

export type ResourceItemFragment = { resourceId: number, resourceName: string };

export type GetResourcesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetResourcesQuery = { resources: Array<{ resourceId: number, resourceName: string }> };

export type RoleItemFragment = { roleId: number, name: string };

export type RoleResourceTreeFragment = { resourceId: number, resourceName: string, actions: Array<{ actionId: number, actionName: string, resourceId: number }> | null };

export type GetRolesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetRolesQuery = { roles: Array<{ roleId: number, name: string }> };

export type GetResourcesByRoleQueryVariables = Exact<{
  roleId: number;
}>;


export type GetResourcesByRoleQuery = { resourcesByRole: { roleId: number, name: string, resources: Array<{ resourceId: number, resourceName: string, actions: Array<{ actionId: number, actionName: string, resourceId: number }> | null }> } };

export type GetUsersByRoleQueryVariables = Exact<{
  roleId: number;
}>;


export type GetUsersByRoleQuery = { usersByRole: Array<{ userId: string, username: string, emailAddress: string, firstName: string, lastName: string }> };

export type CreateResourceActionRoleMutationVariables = Exact<{
  resourceId: number;
  actionId: number;
  roleId: number;
}>;


export type CreateResourceActionRoleMutation = { createResourceActionRole: { roleId: number } };

export type DeleteResourceActionRoleMutationVariables = Exact<{
  resourceId: number;
  actionId: number;
  roleId: number;
}>;


export type DeleteResourceActionRoleMutation = { deleteResourceActionRole: number };

export type CreateUserRoleMutationVariables = Exact<{
  userId: string;
  roleId: number;
}>;


export type CreateUserRoleMutation = { createUserRole: { userId: string, roleId: number } };

export type DeleteUserRoleMutationVariables = Exact<{
  userId: string;
  roleId: number;
}>;


export type DeleteUserRoleMutation = { deleteUserRole: string };

export type ServiceClientPermissionItemFragment = { serviceClientPermissionId: string, clientId: string, accountId: string | null, resource: string, action: string, scope: string, audience: string, active: boolean, effectiveFrom: string | null, effectiveTo: string | null, lastModified: string };

export type GetServiceClientPermissionsQueryVariables = Exact<{
  clientId?: string | null | undefined;
  accountId?: string | null | undefined;
  skip: number;
  take: number;
}>;


export type GetServiceClientPermissionsQuery = { serviceClientPermissions: Array<{ serviceClientPermissionId: string, clientId: string, accountId: string | null, resource: string, action: string, scope: string, audience: string, active: boolean, effectiveFrom: string | null, effectiveTo: string | null, lastModified: string }> };

export type CreateServiceClientPermissionMutationVariables = Exact<{
  permission: ServiceClientPermissionDtoInput;
}>;


export type CreateServiceClientPermissionMutation = { createServiceClientPermission: { serviceClientPermissionId: string, clientId: string, accountId: string | null, resource: string, action: string, scope: string, audience: string, active: boolean, effectiveFrom: string | null, effectiveTo: string | null, lastModified: string } };

export type UpdateServiceClientPermissionMutationVariables = Exact<{
  serviceClientPermissionId: string;
  permission: ServiceClientPermissionDtoInput;
}>;


export type UpdateServiceClientPermissionMutation = { updateServiceClientPermission: boolean };

export type DeleteServiceClientPermissionMutationVariables = Exact<{
  serviceClientPermissionId: string;
}>;


export type DeleteServiceClientPermissionMutation = { deleteServiceClientPermission: string };

export type UserDetailFragment = { userId: string, username: string, emailAddress: string, firstName: string, secondName: string | null, lastName: string, secondSurname: string | null, dob: string | null, loginAttempts: number, accountId: string, active: boolean, integrationUser: boolean };

export type GetUserQueryVariables = Exact<{
  id: string;
}>;


export type GetUserQuery = { user: { userId: string, username: string, emailAddress: string, firstName: string, secondName: string | null, lastName: string, secondSurname: string | null, dob: string | null, loginAttempts: number, accountId: string, active: boolean, integrationUser: boolean } };

export type GetCurrentUserQueryVariables = Exact<{ [key: string]: never; }>;


export type GetCurrentUserQuery = { currentUser: { userId: string, username: string, emailAddress: string, firstName: string, secondName: string | null, lastName: string, secondSurname: string | null, dob: string | null, loginAttempts: number, accountId: string, active: boolean, roles: Array<{ roleId: number, name: string }> | null, profiles: Array<{ policyId: number, name: string }> | null } };

export type GetIntegrationUsersQueryVariables = Exact<{ [key: string]: never; }>;


export type GetIntegrationUsersQuery = { users: Array<{ userId: string, username: string, emailAddress: string }> };

export type GetUsersByAccountQueryVariables = Exact<{
  skip: number;
  take: number;
}>;


export type GetUsersByAccountQuery = { usersByAccount: Array<{ lockedUntil: string | null, userId: string, username: string, emailAddress: string, firstName: string, secondName: string | null, lastName: string, secondSurname: string | null, dob: string | null, loginAttempts: number, accountId: string, active: boolean, integrationUser: boolean }> };

export type CreateUserMutationVariables = Exact<{
  user: CreateUserDtoInput;
}>;


export type CreateUserMutation = { createUser: { userId: string, username: string, emailAddress: string, firstName: string, secondName: string | null, lastName: string, secondSurname: string | null, dob: string | null, loginAttempts: number, accountId: string, active: boolean, integrationUser: boolean } };

export type CreateManagerMutationVariables = Exact<{
  user: CreateUserDtoInput;
  accountId: string;
}>;


export type CreateManagerMutation = { createManager: { userId: string } };

export type UpdateUserMutationVariables = Exact<{
  id: string;
  user: UpdateUserDtoInput;
}>;


export type UpdateUserMutation = { updateUser: boolean };

export type UpdateCurrentUserMutationVariables = Exact<{
  user: UpdateCurrentUserDtoInput;
}>;


export type UpdateCurrentUserMutation = { updateCurrentUser: boolean };

export type UpdatePasswordMutationVariables = Exact<{
  id: string;
  user: UserPasswordDtoInput;
}>;


export type UpdatePasswordMutation = { updatePassword: boolean };

export type DeleteUserMutationVariables = Exact<{
  id: string;
}>;


export type DeleteUserMutation = { deleteUser: string };

export type UnlockUserMutationVariables = Exact<{
  id: string;
}>;


export type UnlockUserMutation = { unlockUser: boolean };

export type UserIsAdminQueryVariables = Exact<{ [key: string]: never; }>;


export type UserIsAdminQuery = { userIsAdmin: boolean };

export type UserIsManagerQueryVariables = Exact<{ [key: string]: never; }>;


export type UserIsManagerQuery = { userIsManager: boolean };

export const ActionItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ActionItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ActionVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"actionId"}},{"kind":"Field","name":{"kind":"Name","value":"actionName"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}}]}}]} as unknown as DocumentNode<ActionItemFragment, unknown>;
export const ClientItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ClientItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ClientVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"clientId"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"processed"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<ClientItemFragment, unknown>;
export const PolicyItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PolicyItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PolicyVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"policyId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]} as unknown as DocumentNode<PolicyItemFragment, unknown>;
export const PolicyResourceTreeFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PolicyResourceTree"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ResourceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceName"}},{"kind":"Field","name":{"kind":"Name","value":"actions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"actionName"}},{"kind":"Field","name":{"kind":"Name","value":"actionId"}}]}}]}}]} as unknown as DocumentNode<PolicyResourceTreeFragment, unknown>;
export const ResourceItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ResourceItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ResourceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceName"}}]}}]} as unknown as DocumentNode<ResourceItemFragment, unknown>;
export const RoleItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RoleItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RoleVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]} as unknown as DocumentNode<RoleItemFragment, unknown>;
export const RoleResourceTreeFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RoleResourceTree"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ResourceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceName"}},{"kind":"Field","name":{"kind":"Name","value":"actions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"actionId"}},{"kind":"Field","name":{"kind":"Name","value":"actionName"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}}]}}]}}]} as unknown as DocumentNode<RoleResourceTreeFragment, unknown>;
export const ServiceClientPermissionItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ServiceClientPermissionItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServiceClientPermissionVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serviceClientPermissionId"}},{"kind":"Field","name":{"kind":"Name","value":"clientId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resource"}},{"kind":"Field","name":{"kind":"Name","value":"action"}},{"kind":"Field","name":{"kind":"Name","value":"scope"}},{"kind":"Field","name":{"kind":"Name","value":"audience"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<ServiceClientPermissionItemFragment, unknown>;
export const UserDetailFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserDetail"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UserVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"secondName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"secondSurname"}},{"kind":"Field","name":{"kind":"Name","value":"dob"}},{"kind":"Field","name":{"kind":"Name","value":"loginAttempts"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"integrationUser"}}]}}]} as unknown as DocumentNode<UserDetailFragment, unknown>;
export const GetActionsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetActions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"actions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"ActionItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ActionItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ActionVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"actionId"}},{"kind":"Field","name":{"kind":"Name","value":"actionName"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}}]}}]} as unknown as DocumentNode<GetActionsQuery, GetActionsQueryVariables>;
export const GetClientsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetClients"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"clients"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"ClientItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ClientItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ClientVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"clientId"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"processed"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetClientsQuery, GetClientsQueryVariables>;
export const CreateClientDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateClient"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"client"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ClientDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createClient"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"client"},"value":{"kind":"Variable","name":{"kind":"Name","value":"client"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"ClientItem"}},{"kind":"Field","name":{"kind":"Name","value":"secret"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ClientItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ClientVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"clientId"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"processed"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<CreateClientMutation, CreateClientMutationVariables>;
export const UpdateClientDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateClient"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"client"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ClientUserDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateClient"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"client"},"value":{"kind":"Variable","name":{"kind":"Name","value":"client"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateClientMutation, UpdateClientMutationVariables>;
export const DeleteClientDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteClient"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteClient"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteClientMutation, DeleteClientMutationVariables>;
export const GetPoliciesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetPolicies"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"policies"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"PolicyItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PolicyItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PolicyVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"policyId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]} as unknown as DocumentNode<GetPoliciesQuery, GetPoliciesQueryVariables>;
export const GetResourcesByPolicyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetResourcesByPolicy"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourcesByPolicy"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"policyId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"policyId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"resources"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"PolicyResourceTree"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PolicyResourceTree"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ResourceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceName"}},{"kind":"Field","name":{"kind":"Name","value":"actions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"actionName"}},{"kind":"Field","name":{"kind":"Name","value":"actionId"}}]}}]}}]} as unknown as DocumentNode<GetResourcesByPolicyQuery, GetResourcesByPolicyQueryVariables>;
export const GetUsersByPolicyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUsersByPolicy"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"usersByPolicy"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"policyId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}}]}}]}}]} as unknown as DocumentNode<GetUsersByPolicyQuery, GetUsersByPolicyQueryVariables>;
export const CreateResourceActionPolicyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateResourceActionPolicy"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"resourceId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"actionId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createResourceActionPolicy"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"resourceActionPolicy"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"resourceId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"resourceId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"actionId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"actionId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"policyId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}}}]}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"policyId"}}]}}]}}]} as unknown as DocumentNode<CreateResourceActionPolicyMutation, CreateResourceActionPolicyMutationVariables>;
export const DeleteResourceActionPolicyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteResourceActionPolicy"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"resourceId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"actionId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteResourceActionPolicy"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"resourceId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"resourceId"}}},{"kind":"Argument","name":{"kind":"Name","value":"actionId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"actionId"}}},{"kind":"Argument","name":{"kind":"Name","value":"policyId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}}}]}]}}]} as unknown as DocumentNode<DeleteResourceActionPolicyMutation, DeleteResourceActionPolicyMutationVariables>;
export const CreateUserPolicyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateUserPolicy"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createUserPolicy"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"userPolicy"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"policyId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}}}]}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"policyId"}}]}}]}}]} as unknown as DocumentNode<CreateUserPolicyMutation, CreateUserPolicyMutationVariables>;
export const DeleteUserPolicyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteUserPolicy"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteUserPolicy"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}},{"kind":"Argument","name":{"kind":"Name","value":"policyId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"policyId"}}}]}]}}]} as unknown as DocumentNode<DeleteUserPolicyMutation, DeleteUserPolicyMutationVariables>;
export const GetResourcesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetResources"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resources"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"ResourceItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ResourceItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ResourceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceName"}}]}}]} as unknown as DocumentNode<GetResourcesQuery, GetResourcesQueryVariables>;
export const GetRolesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetRoles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"roles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"RoleItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RoleItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RoleVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]} as unknown as DocumentNode<GetRolesQuery, GetRolesQueryVariables>;
export const GetResourcesByRoleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetResourcesByRole"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourcesByRole"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"roleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"resources"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"RoleResourceTree"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RoleResourceTree"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ResourceVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resourceId"}},{"kind":"Field","name":{"kind":"Name","value":"resourceName"}},{"kind":"Field","name":{"kind":"Name","value":"actions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"actionId"}},{"kind":"Field","name":{"kind":"Name","value":"actionName"}},{"kind":"Field","name":{"kind":"Name","value":"resourceId"}}]}}]}}]} as unknown as DocumentNode<GetResourcesByRoleQuery, GetResourcesByRoleQueryVariables>;
export const GetUsersByRoleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUsersByRole"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"usersByRole"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"roleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}}]}}]}}]} as unknown as DocumentNode<GetUsersByRoleQuery, GetUsersByRoleQueryVariables>;
export const CreateResourceActionRoleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateResourceActionRole"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"resourceId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"actionId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createResourceActionRole"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"resourceActionRole"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"resourceId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"resourceId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"actionId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"actionId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"roleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}}}]}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"roleId"}}]}}]}}]} as unknown as DocumentNode<CreateResourceActionRoleMutation, CreateResourceActionRoleMutationVariables>;
export const DeleteResourceActionRoleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteResourceActionRole"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"resourceId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"actionId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteResourceActionRole"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"resourceId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"resourceId"}}},{"kind":"Argument","name":{"kind":"Name","value":"actionId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"actionId"}}},{"kind":"Argument","name":{"kind":"Name","value":"roleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}}}]}]}}]} as unknown as DocumentNode<DeleteResourceActionRoleMutation, DeleteResourceActionRoleMutationVariables>;
export const CreateUserRoleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateUserRole"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createUserRole"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"userRole"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"roleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}}}]}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"roleId"}}]}}]}}]} as unknown as DocumentNode<CreateUserRoleMutation, CreateUserRoleMutationVariables>;
export const DeleteUserRoleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteUserRole"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteUserRole"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}},{"kind":"Argument","name":{"kind":"Name","value":"roleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}}}]}]}}]} as unknown as DocumentNode<DeleteUserRoleMutation, DeleteUserRoleMutationVariables>;
export const GetServiceClientPermissionsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetServiceClientPermissions"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"clientId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serviceClientPermissions"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"clientId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"clientId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"ServiceClientPermissionItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ServiceClientPermissionItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServiceClientPermissionVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serviceClientPermissionId"}},{"kind":"Field","name":{"kind":"Name","value":"clientId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resource"}},{"kind":"Field","name":{"kind":"Name","value":"action"}},{"kind":"Field","name":{"kind":"Name","value":"scope"}},{"kind":"Field","name":{"kind":"Name","value":"audience"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<GetServiceClientPermissionsQuery, GetServiceClientPermissionsQueryVariables>;
export const CreateServiceClientPermissionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateServiceClientPermission"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"permission"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ServiceClientPermissionDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createServiceClientPermission"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"permission"},"value":{"kind":"Variable","name":{"kind":"Name","value":"permission"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"ServiceClientPermissionItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ServiceClientPermissionItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServiceClientPermissionVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serviceClientPermissionId"}},{"kind":"Field","name":{"kind":"Name","value":"clientId"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"resource"}},{"kind":"Field","name":{"kind":"Name","value":"action"}},{"kind":"Field","name":{"kind":"Name","value":"scope"}},{"kind":"Field","name":{"kind":"Name","value":"audience"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveFrom"}},{"kind":"Field","name":{"kind":"Name","value":"effectiveTo"}},{"kind":"Field","name":{"kind":"Name","value":"lastModified"}}]}}]} as unknown as DocumentNode<CreateServiceClientPermissionMutation, CreateServiceClientPermissionMutationVariables>;
export const UpdateServiceClientPermissionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateServiceClientPermission"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"serviceClientPermissionId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"permission"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ServiceClientPermissionDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateServiceClientPermission"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"serviceClientPermissionId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"serviceClientPermissionId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"permission"},"value":{"kind":"Variable","name":{"kind":"Name","value":"permission"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateServiceClientPermissionMutation, UpdateServiceClientPermissionMutationVariables>;
export const DeleteServiceClientPermissionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteServiceClientPermission"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"serviceClientPermissionId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteServiceClientPermission"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"serviceClientPermissionId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"serviceClientPermissionId"}}}]}}]}]}}]} as unknown as DocumentNode<DeleteServiceClientPermissionMutation, DeleteServiceClientPermissionMutationVariables>;
export const GetUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"UserDetail"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserDetail"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UserVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"secondName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"secondSurname"}},{"kind":"Field","name":{"kind":"Name","value":"dob"}},{"kind":"Field","name":{"kind":"Name","value":"loginAttempts"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"integrationUser"}}]}}]} as unknown as DocumentNode<GetUserQuery, GetUserQueryVariables>;
export const GetCurrentUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetCurrentUser"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"currentUser"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"secondName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"secondSurname"}},{"kind":"Field","name":{"kind":"Name","value":"dob"}},{"kind":"Field","name":{"kind":"Name","value":"loginAttempts"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"roles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"profiles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"policyId"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]} as unknown as DocumentNode<GetCurrentUserQuery, GetCurrentUserQueryVariables>;
export const GetIntegrationUsersDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetIntegrationUsers"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"users"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"filter"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"filters"},"value":{"kind":"ListValue","values":[{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"key"},"value":{"kind":"StringValue","value":"IntegrationUser","block":false}},{"kind":"ObjectField","name":{"kind":"Name","value":"value"},"value":{"kind":"BooleanValue","value":true}}]}]}}]}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}}]}}]}}]} as unknown as DocumentNode<GetIntegrationUsersQuery, GetIntegrationUsersQueryVariables>;
export const GetUsersByAccountDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUsersByAccount"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"skip"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"take"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"usersByAccount"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"query"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"skip"},"value":{"kind":"Variable","name":{"kind":"Name","value":"skip"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"take"},"value":{"kind":"Variable","name":{"kind":"Name","value":"take"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"UserDetail"}},{"kind":"Field","name":{"kind":"Name","value":"lockedUntil"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserDetail"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UserVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"secondName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"secondSurname"}},{"kind":"Field","name":{"kind":"Name","value":"dob"}},{"kind":"Field","name":{"kind":"Name","value":"loginAttempts"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"integrationUser"}}]}}]} as unknown as DocumentNode<GetUsersByAccountQuery, GetUsersByAccountQueryVariables>;
export const CreateUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"user"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateUserDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"user"},"value":{"kind":"Variable","name":{"kind":"Name","value":"user"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"UserDetail"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserDetail"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UserVm"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"emailAddress"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"secondName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"secondSurname"}},{"kind":"Field","name":{"kind":"Name","value":"dob"}},{"kind":"Field","name":{"kind":"Name","value":"loginAttempts"}},{"kind":"Field","name":{"kind":"Name","value":"accountId"}},{"kind":"Field","name":{"kind":"Name","value":"active"}},{"kind":"Field","name":{"kind":"Name","value":"integrationUser"}}]}}]} as unknown as DocumentNode<CreateUserMutation, CreateUserMutationVariables>;
export const CreateManagerDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateManager"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"user"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateUserDtoInput"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createManager"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"user"},"value":{"kind":"Variable","name":{"kind":"Name","value":"user"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"accountId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"accountId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userId"}}]}}]}}]} as unknown as DocumentNode<CreateManagerMutation, CreateManagerMutationVariables>;
export const UpdateUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"user"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateUserDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"user"},"value":{"kind":"Variable","name":{"kind":"Name","value":"user"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateUserMutation, UpdateUserMutationVariables>;
export const UpdateCurrentUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateCurrentUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"user"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateCurrentUserDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateCurrentUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"user"},"value":{"kind":"Variable","name":{"kind":"Name","value":"user"}}}]}}]}]}}]} as unknown as DocumentNode<UpdateCurrentUserMutation, UpdateCurrentUserMutationVariables>;
export const UpdatePasswordDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdatePassword"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"user"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UserPasswordDtoInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updatePassword"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"user"},"value":{"kind":"Variable","name":{"kind":"Name","value":"user"}}}]}}]}]}}]} as unknown as DocumentNode<UpdatePasswordMutation, UpdatePasswordMutationVariables>;
export const DeleteUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteUserMutation, DeleteUserMutationVariables>;
export const UnlockUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UnlockUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"unlockUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<UnlockUserMutation, UnlockUserMutationVariables>;
export const UserIsAdminDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"UserIsAdmin"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userIsAdmin"}}]}}]} as unknown as DocumentNode<UserIsAdminQuery, UserIsAdminQueryVariables>;
export const UserIsManagerDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"UserIsManager"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userIsManager"}}]}}]} as unknown as DocumentNode<UserIsManagerQuery, UserIsManagerQueryVariables>;