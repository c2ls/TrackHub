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

/**
 * Transporter API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer
 * (src/queries handles both for components).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  TransporterItemFragment as TransporterItemType,
  AssignmentFieldsFragment as AssignmentFieldsType,
  TransporterDtoInput,
  UpdateTransporterDtoInput,
  TransporterDeviceAssignmentDtoInput,
  GetTransporterDeviceAssignmentsByAccountQuery,
} from './generated/graphql';
import {
  GetTransporterDocument,
  GetTransportersByAccountDocument,
  GetTransportersByUserDocument,
  GetTransportersByGroupDocument,
  CreateTransporterDocument,
  UpdateTransporterDocument,
  DeleteTransporterDocument,
  GetTransporterDeviceAssignmentsByAccountDocument,
  GetTransporterDeviceAssignmentsByTransporterDocument,
  AssignDeviceToTransporterDocument,
  EndDeviceTransporterAssignmentDocument,
} from './transporterOperations';

export type Transporter = TransporterItemType;
export type TransporterAssignment = AssignmentFieldsType;
export type TransporterAssignmentWithAudit =
  GetTransporterDeviceAssignmentsByAccountQuery['transporterDeviceAssignmentsByAccount'][number];
export type { TransporterDtoInput, UpdateTransporterDtoInput, TransporterDeviceAssignmentDtoInput };

export async function getTransporter(transporterId: string): Promise<Transporter> {
  const data = await executeGraphQL('manager', GetTransporterDocument, { id: transporterId });
  return data.transporter;
}

export async function getTransportersByAccount(): Promise<Transporter[]> {
  const data = await executeGraphQL('manager', GetTransportersByAccountDocument);
  return data.transportersByAccount;
}

export async function getTransportersByUser(): Promise<Transporter[]> {
  const data = await executeGraphQL('manager', GetTransportersByUserDocument);
  return data.transportersByUser;
}

export async function getTransportersByGroup(groupId: number): Promise<Transporter[]> {
  const data = await executeGraphQL('manager', GetTransportersByGroupDocument, { groupId });
  return data.transportersByGroup;
}

export async function createTransporter(transporter: TransporterDtoInput): Promise<Transporter> {
  const data = await executeGraphQL('manager', CreateTransporterDocument, { transporter });
  return data.createTransporter;
}

export async function updateTransporter(
  transporterId: string,
  transporter: Omit<UpdateTransporterDtoInput, 'transporterId'>
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateTransporterDocument, {
    id: transporterId,
    transporter: { ...transporter, transporterId },
  });
  return data.updateTransporter;
}

export async function deleteTransporter(transporterId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeleteTransporterDocument, { id: transporterId });
  return data.deleteTransporter;
}

export async function getTransporterDeviceAssignmentsByAccount(
  accountId: string,
  activeOnly = false
): Promise<TransporterAssignmentWithAudit[]> {
  const data = await executeGraphQL('manager', GetTransporterDeviceAssignmentsByAccountDocument, {
    accountId,
    activeOnly,
  });
  return data.transporterDeviceAssignmentsByAccount;
}

export async function getTransporterDeviceAssignmentsByTransporter(
  transporterId: string,
  activeOnly = false
): Promise<TransporterAssignment[]> {
  const data = await executeGraphQL(
    'manager',
    GetTransporterDeviceAssignmentsByTransporterDocument,
    { transporterId, activeOnly }
  );
  return data.transporterDeviceAssignmentsByTransporter;
}

export async function assignDeviceToTransporter(
  assignment: TransporterDeviceAssignmentDtoInput
): Promise<{ transporterDeviceAssignmentId: string }> {
  const data = await executeGraphQL('manager', AssignDeviceToTransporterDocument, { assignment });
  return data.assignDeviceToTransporter;
}

export async function endDeviceTransporterAssignment(
  assignmentId: string,
  reason: string | null = null
): Promise<boolean> {
  const data = await executeGraphQL('manager', EndDeviceTransporterAssignmentDocument, {
    assignmentId,
    reason,
  });
  return data.endDeviceTransporterAssignment;
}
