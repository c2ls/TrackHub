/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

// AccountStatus enum mirror (Common.Domain.Enums.AccountStatus). Backend is authoritative; these are
// cosmetic helpers for the SuperAdministrator accounts panel.
export const ACCOUNT_STATUS_NAME = {
  1: 'TRIAL',
  2: 'ACTIVE',
  3: 'SUSPENDED',
  4: 'CANCELLED',
  5: 'ARCHIVED'
} as const;

// Numeric statusId union (mirrors Common.Domain.Enums.AccountStatus values).
export type AccountStatusId = keyof typeof ACCOUNT_STATUS_NAME;
// Status enum name union (TRIAL | ACTIVE | SUSPENDED | CANCELLED | ARCHIVED).
export type AccountStatusName = (typeof ACCOUNT_STATUS_NAME)[AccountStatusId];

// Colored badge per status (ArgonBadge color).
export const ACCOUNT_STATUS_COLOR = {
  TRIAL: 'info',
  ACTIVE: 'success',
  SUSPENDED: 'warning',
  CANCELLED: 'error',
  ARCHIVED: 'secondary'
} as const;

// ArgonBadge color token union.
export type AccountStatusColor = (typeof ACCOUNT_STATUS_COLOR)[AccountStatusName];

// i18n key suffix per status enum name.
export const ACCOUNT_STATUS_I18N = {
  TRIAL: 'account.statusTrial',
  ACTIVE: 'account.statusActive',
  SUSPENDED: 'account.statusSuspended',
  CANCELLED: 'account.statusCancelled',
  ARCHIVED: 'account.statusArchived'
} as const;

// i18n key union for the status labels.
export type AccountStatusI18nKey = (typeof ACCOUNT_STATUS_I18N)[AccountStatusName];

// Allowed lifecycle transitions by current statusId.
export const ALLOWED_TRANSITIONS = {
  1: ['ACTIVE', 'SUSPENDED', 'CANCELLED'],
  2: ['SUSPENDED', 'CANCELLED'],
  3: ['ACTIVE', 'CANCELLED', 'ARCHIVED'],
  4: ['ACTIVE', 'ARCHIVED'],
  5: []
} as const;

// A non-empty reason is required when suspending or cancelling.
export const requiresReason = (targetStatus: AccountStatusName): boolean =>
  targetStatus === 'SUSPENDED' || targetStatus === 'CANCELLED';
