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

import { useMemo } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { TollVehicleClass, TransporterTollClass } from 'api/tripManagement/trips';
import type { Transporter } from 'api/manager/transporters';
import { TOLL_CLASS_TARGETS } from '../../tripWriteForms';
import type { TollClassFormValues } from '../../tripWriteForms';

interface TollClassDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: TollClassFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  /** The account's own transporters — also the source of the transporter-type list. */
  transporters: Transporter[];
  vehicleClasses: TollVehicleClass[];
  /** Mappings written in this session; the backend exposes no read query yet. */
  savedMappings: TransporterTollClass[];
  saving: boolean;
}

/**
 * Account-scoped transporter → toll-class mapping.
 *
 * This is NOT part of the SuperAdministrator toll catalog: the catalog is
 * platform reference data, while fleet composition is tenant data, so spec 11
 * §4/§7.6 put this write under `Resources.Trips`/`Edit` for the Account
 * Administrator. It lives on the trip manager because that route is the only
 * one already gated by `featureKey: "trip-management"` and already loads the
 * vehicle-class catalog and the account's transporters.
 *
 * Without a mapping, `CreateTripCommand` has nothing to default
 * `Trip.TollVehicleClass` from, so every route plan is priced with no class and
 * toll estimation silently never engages.
 *
 * The transporter-type list is derived from the account's own transporters
 * rather than read from the platform type catalog: it keeps the dialog
 * account-scoped and only offers types the fleet actually operates.
 */
function TollClassDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
  transporters,
  vehicleClasses,
  savedMappings,
  saving,
}: TollClassDialogProps) {
  const { t } = useTranslation();

  const typeOptions = useMemo(() => {
    const seen = new Map<number, string>();
    transporters.forEach((transporter) => {
      if (transporter.transporterTypeId != null && !seen.has(transporter.transporterTypeId)) {
        seen.set(transporter.transporterTypeId, transporter.transporterType ?? '');
      }
    });
    return [...seen.entries()].map(([value, label]) => ({ value, label: label || String(value) }));
  }, [transporters]);

  const transporterOptions = useMemo(
    () =>
      transporters.map((transporter) => ({
        value: transporter.transporterId,
        label: transporter.name,
      })),
    [transporters]
  );

  const classOptions = useMemo(
    () =>
      vehicleClasses
        .filter((vehicleClass) => vehicleClass.active)
        .map((vehicleClass) => ({
          value: vehicleClass.code,
          label: `${vehicleClass.code} · ${vehicleClass.name}`,
        })),
    [vehicleClasses]
  );

  const targetOptions = TOLL_CLASS_TARGETS.map((value) => ({
    value,
    label: t(`tolls.transporterClass.targets.${value}` as 'tolls.transporterClass.targets.transporterType'),
  }));

  const target = values.target ?? 'transporterType';

  const describeMapping = (mapping: TransporterTollClass): string => {
    if (mapping.transporterId) {
      const transporter = transporters.find(
        (candidate) => candidate.transporterId === mapping.transporterId
      );
      return transporter?.name ?? mapping.transporterId;
    }
    const typeOption = typeOptions.find((option) => option.value === mapping.transporterTypeId);
    return typeOption?.label ?? String(mapping.transporterTypeId ?? '');
  };

  return (
    <FormDialog
      title={t('tolls.transporterClass.title')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="sm"
    >
      <form>
        <ArgonBox mb={1}>
          <ArgonTypography variant="caption" color="secondary" display="block">
            {t('tolls.transporterClass.description')}
          </ArgonTypography>
        </ArgonBox>
        {classOptions.length === 0 ? (
          <ArgonTypography variant="caption" color="warning" fontWeight="medium" display="block">
            {t('tolls.transporterClass.noClasses')}
          </ArgonTypography>
        ) : (
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <CustomSelect
                list={targetOptions}
                handleChange={handleChange}
                name="target"
                id="target"
                label={t('tolls.transporterClass.target')}
                value={target}
                numericValue={false}
                required
                errorMsg={errors.target}
              />
            </Grid>
            {target === 'transporterType' ? (
              <Grid size={{ xs: 12 }}>
                <CustomSelect
                  list={typeOptions}
                  handleChange={handleChange}
                  name="transporterTypeId"
                  id="transporterTypeId"
                  label={t('tolls.transporterClass.transporterType')}
                  value={values.transporterTypeId ?? 0}
                  required
                  errorMsg={errors.transporterTypeId}
                />
                {errors.transporterTypeId && (
                  <ArgonTypography variant="caption" color="error">
                    {errors.transporterTypeId}
                  </ArgonTypography>
                )}
              </Grid>
            ) : (
              <Grid size={{ xs: 12 }}>
                <CustomSelect
                  list={transporterOptions}
                  handleChange={handleChange}
                  name="transporterId"
                  id="transporterId"
                  label={t('tolls.transporterClass.transporter')}
                  value={values.transporterId ?? ''}
                  numericValue={false}
                  required
                  errorMsg={errors.transporterId}
                />
                {errors.transporterId && (
                  <ArgonTypography variant="caption" color="error">
                    {errors.transporterId}
                  </ArgonTypography>
                )}
              </Grid>
            )}
            <Grid size={{ xs: 12 }}>
              <CustomSelect
                list={classOptions}
                handleChange={handleChange}
                name="tollVehicleClassCode"
                id="tollVehicleClassCode"
                label={t('tolls.transporterClass.vehicleClass')}
                value={values.tollVehicleClassCode ?? ''}
                numericValue={false}
                required
                errorMsg={errors.tollVehicleClassCode}
              />
              {errors.tollVehicleClassCode && (
                <ArgonTypography variant="caption" color="error">
                  {errors.tollVehicleClassCode}
                </ArgonTypography>
              )}
            </Grid>
            <Grid size={{ xs: 12 }}>
              <ArgonTypography variant="caption" color="secondary" display="block">
                {t('tolls.transporterClass.appliesHint')}
              </ArgonTypography>
            </Grid>
            {savedMappings.length > 0 && (
              <Grid size={{ xs: 12 }}>
                <ArgonTypography variant="button" fontWeight="medium">
                  {t('tolls.transporterClass.saved')}
                </ArgonTypography>
                {/* The API exposes no query for existing mappings, so only what
                    this session wrote can be listed back. */}
                <ArgonTypography variant="caption" color="secondary" display="block">
                  {t('tolls.transporterClass.savedHint')}
                </ArgonTypography>
                {savedMappings.map((mapping) => (
                  <ArgonTypography
                    key={mapping.transporterTollClassId}
                    variant="caption"
                    color="text"
                    display="block"
                  >
                    {describeMapping(mapping)} → {mapping.tollVehicleClassCode}
                  </ArgonTypography>
                ))}
              </Grid>
            )}
            {saving && (
              <Grid size={{ xs: 12 }}>
                <ArgonTypography variant="caption" color="secondary">
                  {t('tolls.transporterClass.saving')}
                </ArgonTypography>
              </Grid>
            )}
          </Grid>
        )}
      </form>
    </FormDialog>
  );
}

export default TollClassDialog;
