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

import { useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Table from "controls/Tables/Table";
import TableAccordion from "controls/Accordions/TableAccordion";
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import useForm from "controls/Dialogs/useForm";
import DriverDialog from "layouts/manageadmin/components/drivers/DriverDialog";
import type { DriverFormValues } from "layouts/manageadmin/components/drivers/DriverDialog";
import ManageDriverCredentials from "layouts/manageadmin/components/drivers/DriverCredentials";
import ManageDriverQualifications from "layouts/manageadmin/components/drivers/DriverQualifications";
import ManageDriverAssignments from "layouts/manageadmin/components/drivers/DriverAssignments";
import QualificationExpirations from "layouts/manageadmin/components/drivers/QualificationExpirations";
import { useFeatures } from "context/features";
import { useAccountByUser } from "queries/accounts";
import { useDriversByAccount, useCreateDriver, useUpdateDriver, useDeactivateDriver } from 'queries/drivers';
import type { Driver, DriverDtoInput } from 'api/manager/drivers';
import { LoadingContext } from 'LoadingContext';

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

/**
 * The extended workforce capabilities (qualifications, assignment history,
 * expirations) are billable; the driver registry and credential/device
 * administration are core platform and stay visible regardless (spec 09 §3, §8).
 */
const WORKFORCE_FEATURE_KEY = 'workforce';

function ManageDrivers() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const { isFeatureEnabled } = useFeatures();
  const workforceEnabled = isFeatureEnabled(WORKFORCE_FEATURE_KEY);
  const [expanded, setExpanded] = useState(false);
  const [open, setOpen] = useState(false);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<DriverFormValues>({ active: true });

  // Account id comes from the query layer; the driver list is then keyed on it.
  const accountQuery = useAccountByUser({ enabled: expanded });
  const account = accountQuery.data ?? null;
  const driversQuery = useDriversByAccount(account?.accountId, { enabled: expanded && !!account?.accountId });
  const drivers = driversQuery.data ?? [];
  const createDriver = useCreateDriver();
  const updateDriver = useUpdateDriver();
  const deactivateDriver = useDeactivateDriver();

  // Keep the global spinner UX while the account/driver list loads/refreshes.
  useEffect(() => {
    setLoading(accountQuery.isFetching || driversQuery.isFetching);
  }, [accountQuery.isFetching, driversQuery.isFetching, setLoading]);

  const handleAddClick = () => {
    setValues({ accountId: account?.accountId, active: true });
    setErrors({});
  };

  const handleEdit = (driver: Driver) => {
    setValues({ ...driver, accountId: account?.accountId || driver.accountId });
    setErrors({});
    setOpen(true);
  };

  const handleSubmit = async () => {
    if (!validate(['name']) || !account?.accountId) return;
    setLoading(true);
    try {
      // validate(['name']) + the account?.accountId guard above ensure the
      // required DriverDtoInput fields are present — assert at the boundary.
      const driver = { ...values, accountId: account.accountId, active: values.active !== false };
      if (driver.driverId) {
        await updateDriver.mutateAsync({ driverId: driver.driverId, driver: driver as DriverDtoInput });
      } else {
        await createDriver.mutateAsync(driver as DriverDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const handleDeactivate = async (driver: Driver) => {
    if (!driver?.driverId || !window.confirm(t('driver.deactivateConfirmation'))) return;
    setLoading(true);
    try {
      await deactivateDriver.mutateAsync(driver.driverId);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('driver.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table
          columns={[
            { name: 'name', title: t('driver.name'), align: 'left' },
            { name: 'phone', title: t('driver.phone'), align: 'center' },
            { name: 'document', title: t('driver.document'), align: 'center' },
            { name: 'active', title: t('generic.active'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' }
          ]}
          rows={drivers.map(driver => ({
            name: <TextCell>{driver.name}</TextCell>,
            phone: <TextCell>{driver.phone}</TextCell>,
            document: <TextCell>{driver.documentNumber}</TextCell>,
            active: <TextCell>{driver.active ? t('generic.yes') : t('generic.no')}</TextCell>,
            action: (
              <>
                <ArgonButton variant="text" color="dark" onClick={() => handleEdit(driver)}>
                  <Icon>edit</Icon>&nbsp;{t('generic.edit')}
                </ArgonButton>
                {driver.active && (
                  <ArgonButton variant="text" color="error" onClick={() => handleDeactivate(driver)}>
                    <Icon>block</Icon>&nbsp;{t('driver.deactivate')}
                  </ArgonButton>
                )}
              </>
            ),
            id: driver.driverId
          }))}
          selectedField="name"
        />
      </TableAccordion>

      {/* Core: driver identity administration is never feature-gated. */}
      <ManageDriverCredentials />

      {/* Billable workforce surfaces — hidden without the feature (cosmetic
          only; the backend gate is authoritative). */}
      {workforceEnabled && (
        <>
          <ManageDriverQualifications />
          <ManageDriverAssignments />
          <QualificationExpirations />
        </>
      )}

      <DriverDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />
    </>
  );
}

export default ManageDrivers;
