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

import { useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import RouteMap from 'controls/Maps/RouteMap';
import type { MapProvider } from 'controls/Maps/core/MapProviderContext';
import type { RoutePoint } from 'controls/Maps/core/mapTypes';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';

/** Dialog/form state for a toll station. */
export interface TollStationFormValues {
  tollStationId?: string;
  name?: string;
  code?: string | null;
  latitude?: number | string;
  longitude?: number | string;
  country?: string | null;
  region?: string | null;
  roadName?: string | null;
  direction?: string | null;
  operator?: string | null;
  notes?: string | null;
}

interface TollStationDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: TollStationFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  /** Account map provider, so the picker matches the rest of the portal. */
  mapType?: MapProvider;
  mapKey?: string | null;
  darkMode?: boolean;
}

/**
 * Station editor with both entry paths the spec requires: a map picker and
 * direct coordinate entry. The map is opt-in so the dialog stays light for the
 * common case of pasting coordinates from an operator's own catalog.
 */
function TollStationDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
  mapType,
  mapKey,
  darkMode = false,
}: TollStationDialogProps) {
  const { t } = useTranslation();
  const [showMap, setShowMap] = useState(false);

  const latitude = Number(values.latitude);
  const longitude = Number(values.longitude);
  const hasPoint = Number.isFinite(latitude) && Number.isFinite(longitude) && (latitude !== 0 || longitude !== 0);

  const handleMapClick = (point: RoutePoint) => {
    handleChange({ target: { name: 'latitude', value: point.lat.toFixed(6) } });
    handleChange({ target: { name: 'longitude', value: point.lng.toFixed(6) } });
  };

  return (
    <FormDialog
      title={t('tolls.catalog.stationDetails')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="md"
    >
      <form>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 8 }}>
            <CustomTextField
              autoFocus
              margin="dense"
              name="name"
              id="name"
              label={t('tolls.catalog.name')}
              type="text"
              value={values.name || ''}
              onChange={handleChange}
              errorMsg={errors.name}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomTextField
              margin="dense"
              name="code"
              id="code"
              label={t('tolls.catalog.code')}
              type="text"
              value={values.code || ''}
              onChange={handleChange}
              errorMsg={errors.code}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="latitude"
              id="latitude"
              label={t('tolls.catalog.latitude')}
              type="number"
              value={values.latitude ?? ''}
              onChange={handleChange}
              errorMsg={errors.latitude}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="longitude"
              id="longitude"
              label={t('tolls.catalog.longitude')}
              type="number"
              value={values.longitude ?? ''}
              onChange={handleChange}
              errorMsg={errors.longitude}
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <ArgonBox display="flex" alignItems="center" gap={1} flexWrap="wrap">
              <ArgonButton variant="outlined" color="info" size="small" onClick={() => setShowMap((open) => !open)}>
                {t('tolls.catalog.pickOnMap')}
              </ArgonButton>
              <ArgonTypography variant="caption" color="secondary">
                {t('tolls.catalog.pickOnMapHint')}
              </ArgonTypography>
            </ArgonBox>
            {showMap && (
              <ArgonBox mt={1}>
                <RouteMap
                  mapType={mapType}
                  mapKey={mapKey}
                  darkMode={darkMode}
                  height="320px"
                  onMapClick={handleMapClick}
                  stops={
                    hasPoint
                      ? [{ sequence: 1, name: values.name || t('tolls.catalog.addStation'), lat: latitude, lng: longitude }]
                      : []
                  }
                />
              </ArgonBox>
            )}
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="country"
              id="country"
              label={t('tolls.catalog.country')}
              type="text"
              value={values.country || ''}
              onChange={handleChange}
              errorMsg={errors.country}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="region"
              id="region"
              label={t('tolls.catalog.region')}
              type="text"
              value={values.region || ''}
              onChange={handleChange}
              errorMsg={errors.region}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="roadName"
              id="roadName"
              label={t('tolls.catalog.roadName')}
              type="text"
              value={values.roadName || ''}
              onChange={handleChange}
              errorMsg={errors.roadName}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="direction"
              id="direction"
              label={t('tolls.catalog.direction')}
              type="text"
              value={values.direction || ''}
              onChange={handleChange}
              errorMsg={errors.direction}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="operator"
              id="operator"
              label={t('tolls.catalog.operator')}
              type="text"
              value={values.operator || ''}
              onChange={handleChange}
              errorMsg={errors.operator}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <CustomTextField
              margin="dense"
              name="notes"
              id="notes"
              label={t('tolls.catalog.notes')}
              type="text"
              multiline
              rows={2}
              value={values.notes || ''}
              onChange={handleChange}
              errorMsg={errors.notes}
            />
          </Grid>
        </Grid>
      </form>
    </FormDialog>
  );
}

export default TollStationDialog;
