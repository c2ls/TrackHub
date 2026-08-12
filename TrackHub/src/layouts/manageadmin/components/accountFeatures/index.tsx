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

import { useContext, useEffect, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import Table from "controls/Tables/Table";
import TableAccordion from "controls/Accordions/TableAccordion";
import ArgonBadge from "components/ArgonBadge";
import ArgonTypography from "components/ArgonTypography";
import { getAccountByUser } from "api/manager/accounts";
import type { Account } from "api/manager/accounts";
import { getAccountFeatures } from "api/manager/accountFeatures";
import type { AccountFeature } from "api/manager/accountFeatures";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";
import { toCamelCase } from "utils/stringUtils";

// Managers only visualize their account features; enabling/disabling is a billing decision
// owned by the SuperAdministrator.
const displayFeatures = [
  'gps.integration',
  'gps.positionHistory',
  'geofencing',
  'trip-management',
  'driver-mobile',
  'public-links',
  'documents',
  'notifications',
  'notifications.email',
  'notifications.whatsapp'
];

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

function ManageAccountFeatures() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [account, setAccount] = useState<Account | null>(null);
  const [features, setFeatures] = useState<AccountFeature[]>([]);
  const loaded = useRef(false);

  const loadFeatures = async () => {
    setLoading(true);
    try {
      const currentAccount = await getAccountByUser();
      if (!currentAccount?.accountId) return;
      setAccount(currentAccount);
      const accountFeatures = await getAccountFeatures(currentAccount.accountId);
      setFeatures(accountFeatures || []);
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (expanded && !loaded.current) {
      loaded.current = true;
      loadFeatures();
    }
  }, [expanded]);

  const rows = displayFeatures.map(featureKey => {
    // The fallback (feature not yet provisioned) is a partial view — the found
    // record is a full AccountFeature; treat the merge as Partial for rendering.
    const feature: Partial<AccountFeature> = features.find(item => item.featureKey === featureKey) || {
      accountId: account?.accountId,
      featureKey,
      enabled: false,
      tier: 'default',
      source: 'superadmin'
    };

    return {
      feature: <TextCell>{t(`resources.${toCamelCase(feature.featureKey || '')}` as 'resources.geofencing', { defaultValue: feature.featureKey })}</TextCell>,
      enabled: <ArgonBadge variant="gradient" color={feature.enabled ? 'success' : 'secondary'} size="xs" container badgeContent={feature.enabled ? t('generic.yes') : t('generic.no')} />,
      tier: <TextCell>{feature.tier}</TextCell>,
      source: <TextCell>{feature.source}</TextCell>,
      modified: <TextCell>{formatDateTime(feature.lastModified)}</TextCell>,
      id: feature.featureKey
    };
  });

  return (
    <TableAccordion title={t('accountFeatures.title')} expanded={expanded} setExpanded={setExpanded}>
      <Table
        columns={[
          { name: 'feature', title: t('accountFeatures.feature'), align: 'left' },
          { name: 'enabled', title: t('accountFeatures.enabled'), align: 'center' },
          { name: 'tier', title: t('accountFeatures.tier'), align: 'center' },
          { name: 'source', title: t('accountFeatures.source'), align: 'center' },
          { name: 'modified', title: t('generic.modified'), align: 'center' },
          { name: 'id' }
        ]}
        rows={rows}
        selectedField="feature"
      />
    </TableAccordion>
  );
}

export default ManageAccountFeatures;
