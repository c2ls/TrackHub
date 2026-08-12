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
import Icon from '@mui/material/Icon';
import Table from "controls/Tables/Table";
import TableAccordion from "controls/Accordions/TableAccordion";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import useForm from "controls/Dialogs/useForm";
import AccountFeatureDialog from "layouts/systemadmin/components/accountFeatures/AccountFeatureDialog";
import { getAccounts } from "api/manager/accounts";
import type { Account } from "api/manager/accounts";
import { getAccountFeaturesMaster, setAccountFeatureMaster } from "api/manager/accountFeatures";
import type { AccountFeature, AccountFeatureDtoInput } from "api/manager/accountFeatures";
import { notifyApiError } from "api/core/errors";
import { parseJson } from 'utils/jsonUtils';
import { toCamelCase } from "utils/stringUtils";
import { LoadingContext } from 'LoadingContext';

/** Editable storage/cost configuration bound to a specific feature key. */
export interface ConfigFieldDef { name: string; labelKey: string; default: number }

/**
 * SuperAdministrator editor state for a single account feature (loose until the
 * add-mode guard / save). In "add" mode account+feature are chosen; in "edit"
 * mode they are fixed.
 */
export interface FeatureFormValues {
  accountId?: string;
  accountName?: string;
  featureKey?: string;
  enabled?: boolean;
  tier?: string;
  source?: string;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  existingConfigurationJson?: string | null;
  configValue?: number;
}

/** Option row for the account/feature selects. */
export interface FeatureSelectOption { value: string; label: string }

// Billing-owned features the SuperAdministrator can assign to an account.
const knownFeatures = [
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

// Storage/cost features carry an editable configuration value stored in configurationJson.
const configField: Record<string, ConfigFieldDef> = {
  'gps.integration': { name: 'storingIntervalSeconds', labelKey: 'accountFeatures.config.storingIntervalSeconds', default: 360 },
  'gps.positionHistory': { name: 'retentionDays', labelKey: 'accountFeatures.config.retentionDays', default: 30 }
};

const featureOptions: FeatureSelectOption[] = knownFeatures.map(key => ({ value: key, label: key }));

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

function SystemAccountFeatures() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [featuresByAccount, setFeaturesByAccount] = useState<Record<string, AccountFeature[]>>({});
  const [open, setOpen] = useState(false);
  const [isAdd, setIsAdd] = useState(false);
  const loaded = useRef(false);
  const [values, handleChange, setValues, setErrors, , errors] = useForm<FeatureFormValues>({});

  const loadFeatures = async () => {
    setLoading(true);
    try {
      const accountList = await getAccounts() || [];
      setAccounts(accountList);
      const map: Record<string, AccountFeature[]> = {};
      for (const account of accountList) {
        map[account.accountId] = await getAccountFeaturesMaster(account.accountId) || [];
      }
      setFeaturesByAccount(map);
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [expanded]);

  const handleAddClick = () => {
    setIsAdd(true);
    setErrors({});
    setValues({ accountId: '', featureKey: '', enabled: true, tier: 'default', source: 'superadmin', configValue: undefined });
  };

  const handleEditClick = (account: Account, feature: AccountFeature) => {
    const cfg = configField[feature.featureKey];
    setIsAdd(false);
    setErrors({});
    setValues({
      accountId: account.accountId,
      accountName: account.name,
      featureKey: feature.featureKey,
      enabled: !!feature.enabled,
      tier: feature.tier || 'default',
      source: feature.source || 'superadmin',
      effectiveFrom: feature.effectiveFrom,
      effectiveTo: feature.effectiveTo,
      existingConfigurationJson: feature.configurationJson,
      configValue: cfg ? ((parseJson<Record<string, unknown>>(feature.configurationJson)[cfg.name] as number | undefined) ?? cfg.default) : undefined
    });
    setOpen(true);
  };

  const handleSubmit = async () => {
    if (isAdd && (!values.accountId || !values.featureKey)) {
      setErrors({
        accountId: !values.accountId ? t('validation.required', { field: t('account.title') }) : undefined,
        featureKey: !values.featureKey ? t('validation.required', { field: t('accountFeatures.feature') }) : undefined
      });
      return;
    }

    setLoading(true);
    try {
      const cfg = configField[values.featureKey ?? ''];
      const configurationJson = cfg
        ? JSON.stringify({ [cfg.name]: parseInt(String(values.configValue ?? cfg.default), 10) || 0 })
        : values.existingConfigurationJson;
      await setAccountFeatureMaster({
        accountId: values.accountId,
        featureKey: values.featureKey,
        enabled: !!values.enabled,
        tier: values.tier || 'default',
        source: values.source || 'superadmin',
        effectiveFrom: values.effectiveFrom,
        effectiveTo: values.effectiveTo,
        configurationJson
      } as AccountFeatureDtoInput);
      setOpen(false);
      await loadFeatures();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const accountOptions: FeatureSelectOption[] = accounts.map(account => ({ value: account.accountId, label: account.name }));

  const rows = accounts.flatMap(account =>
    (featuresByAccount[account.accountId] || []).map(feature => ({
      account: <TextCell>{account.name}</TextCell>,
      feature: <TextCell>{t(`resources.${toCamelCase(feature.featureKey || '')}` as 'resources.geofencing', { defaultValue: feature.featureKey })}</TextCell>,
      enabled: <ArgonBadge variant="gradient" color={feature.enabled ? 'success' : 'secondary'} size="xs" container badgeContent={feature.enabled ? t('generic.yes') : t('generic.no')} />,
      tier: <TextCell>{feature.tier}</TextCell>,
      source: <TextCell>{feature.source}</TextCell>,
      action: (
        <ArgonButton variant="text" color="dark" onClick={() => handleEditClick(account, feature)}>
          <Icon>edit</Icon>&nbsp;{t('generic.edit')}
        </ArgonButton>
      ),
      id: `${account.accountId}-${feature.featureKey}`
    }))
  );

  return (
    <>
      <TableAccordion
        title={t('accountFeatures.title')}
        expanded={expanded}
        setExpanded={setExpanded}
        showAddIcon
        setOpen={setOpen}
        handleAddClick={handleAddClick}>
        <Table
          columns={[
            { name: 'account', title: t('account.title'), align: 'left' },
            { name: 'feature', title: t('accountFeatures.feature'), align: 'left' },
            { name: 'enabled', title: t('accountFeatures.enabled'), align: 'center' },
            { name: 'tier', title: t('accountFeatures.tier'), align: 'center' },
            { name: 'source', title: t('accountFeatures.source'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' }
          ]}
          rows={rows}
          selectedField="feature"
        />
      </TableAccordion>

      <AccountFeatureDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
        isAdd={isAdd}
        accountOptions={accountOptions}
        featureOptions={featureOptions}
        configField={configField}
      />
    </>
  );
}

export default SystemAccountFeatures;
