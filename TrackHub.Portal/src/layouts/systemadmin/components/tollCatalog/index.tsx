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

import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import Table from 'controls/Tables/Table';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useForm from 'controls/Dialogs/useForm';
import { Name, Description } from 'controls/Tables/components/tableComponents';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import TollVehicleClassDialog from './TollVehicleClassDialog';
import type { TollVehicleClassFormValues } from './TollVehicleClassDialog';
import TollStationDialog from './TollStationDialog';
import type { TollStationFormValues } from './TollStationDialog';
import TollTariffDialog from './TollTariffDialog';
import type { TollTariffFormValues } from './TollTariffDialog';
import TollImportDialog from './TollImportDialog';
import {
  useTollVehicleClasses,
  useTollStations,
  useTollStationDetail,
  useCreateTollVehicleClass,
  useUpdateTollVehicleClass,
  useDeactivateTollVehicleClass,
  useCreateTollStation,
  useUpdateTollStation,
  useDeactivateTollStation,
  useCreateTollTariff,
  useUpdateTollTariff,
  useDeleteTollTariff,
} from 'queries/trips';
import type { TollStation, TollTariff, TollVehicleClass } from 'api/tripManagement/trips';

/** One page of stations; the catalog is platform reference data, not a large dataset. */
const STATION_PAGE_SIZE = 100;

/**
 * SuperAdministrator toll catalog. Platform reference data — no `AccountId` —
 * so it lives in the system admin screen alongside the geocoding providers,
 * whose accordion + table + dialog conventions it mirrors.
 *
 * The platform ships an EMPTY catalog on purpose (spec 11 §7.7): an operator
 * enters the classes, stations and tariffs their own routes need. The empty
 * states here say so rather than looking like a failed load.
 */
function ManageTollCatalog() {
  const { t } = useTranslation();
  const [expanded, setExpanded] = useState(false);
  const [selectedStationId, setSelectedStationId] = useState<string | null>(null);

  const classesQuery = useTollVehicleClasses({ enabled: expanded });
  const stationsQuery = useTollStations({ take: STATION_PAGE_SIZE }, { enabled: expanded });
  const stationDetailQuery = useTollStationDetail(selectedStationId);

  const vehicleClasses = useMemo(() => classesQuery.data ?? [], [classesQuery.data]);
  const stations = useMemo(() => stationsQuery.data?.items ?? [], [stationsQuery.data]);
  const tariffs = useMemo(
    () => stationDetailQuery.data?.tariffs ?? [],
    [stationDetailQuery.data]
  );

  const createClass = useCreateTollVehicleClass();
  const updateClass = useUpdateTollVehicleClass();
  const deactivateClass = useDeactivateTollVehicleClass();
  const createStation = useCreateTollStation();
  const updateStation = useUpdateTollStation();
  const deactivateStation = useDeactivateTollStation();
  const createTariff = useCreateTollTariff();
  const updateTariff = useUpdateTollTariff();
  const deleteTariff = useDeleteTollTariff();

  /* ------------------------------------------------------- vehicle classes */

  const [classOpen, setClassOpen] = useState(false);
  const [classValues, classChange, setClassValues, setClassErrors, validateClass, classErrors] =
    useForm<TollVehicleClassFormValues>({});
  const [classToDeactivate, setClassToDeactivate] = useState<string | null>(null);

  const openClass = (vehicleClass?: TollVehicleClass) => {
    setClassValues(
      vehicleClass
        ? {
            tollVehicleClassId: vehicleClass.tollVehicleClassId,
            code: vehicleClass.code,
            name: vehicleClass.name,
            description: vehicleClass.description,
            sortOrder: vehicleClass.sortOrder,
          }
        : { sortOrder: 0 }
    );
    setClassErrors({});
    setClassOpen(true);
  };

  const saveClass = async () => {
    if (!validateClass(['code', 'name'])) return;
    const payload = {
      code: classValues.code as string,
      name: classValues.name as string,
      description: classValues.description ?? null,
      sortOrder: Number(classValues.sortOrder) || 0,
    };
    try {
      if (classValues.tollVehicleClassId) {
        await updateClass.mutateAsync({
          tollVehicleClassId: classValues.tollVehicleClassId,
          vehicleClass: payload,
        });
      } else {
        await createClass.mutateAsync(payload);
      }
      setClassOpen(false);
    } catch {
      // Surfaced by the global toast; keep the dialog open so the entry survives.
    }
  };

  /* --------------------------------------------------------------- stations */

  const [stationOpen, setStationOpen] = useState(false);
  const [
    stationValues,
    stationChange,
    setStationValues,
    setStationErrors,
    validateStation,
    stationErrors,
  ] = useForm<TollStationFormValues>({});
  const [stationToDeactivate, setStationToDeactivate] = useState<string | null>(null);

  const openStation = (station?: TollStation) => {
    setStationValues(
      station
        ? {
            tollStationId: station.tollStationId,
            name: station.name,
            code: station.code,
            latitude: station.latitude,
            longitude: station.longitude,
            country: station.country,
            region: station.region,
            roadName: station.roadName,
            direction: station.direction,
            operator: station.operator,
            notes: station.notes,
          }
        : {}
    );
    setStationErrors({});
    setStationOpen(true);
  };

  const saveStation = async () => {
    if (!validateStation(['name', 'latitude', 'longitude'])) return;
    const payload = {
      name: stationValues.name as string,
      code: stationValues.code ?? null,
      latitude: Number(stationValues.latitude),
      longitude: Number(stationValues.longitude),
      country: stationValues.country ?? null,
      region: stationValues.region ?? null,
      roadName: stationValues.roadName ?? null,
      direction: stationValues.direction ?? null,
      operator: stationValues.operator ?? null,
      notes: stationValues.notes ?? null,
    };
    try {
      if (stationValues.tollStationId) {
        await updateStation.mutateAsync({
          tollStationId: stationValues.tollStationId,
          station: payload,
        });
      } else {
        await createStation.mutateAsync(payload);
      }
      setStationOpen(false);
    } catch {
      // Surfaced by the global toast.
    }
  };

  /* ---------------------------------------------------------------- tariffs */

  const [tariffOpen, setTariffOpen] = useState(false);
  const [
    tariffValues,
    tariffChange,
    setTariffValues,
    setTariffErrors,
    validateTariff,
    tariffErrors,
  ] = useForm<TollTariffFormValues>({});
  const [tariffToDelete, setTariffToDelete] = useState<string | null>(null);

  const openTariff = (tariff?: TollTariff) => {
    setTariffValues(
      tariff
        ? {
            tollTariffId: tariff.tollTariffId,
            tollStationId: tariff.tollStationId,
            tollVehicleClassCode: tariff.tollVehicleClassCode,
            amount: tariff.amount,
            currency: tariff.currency,
            effectiveFrom: tariff.effectiveFrom,
            effectiveTo: tariff.effectiveTo,
          }
        : { tollStationId: selectedStationId ?? undefined, currency: 'COP' }
    );
    setTariffErrors({});
    setTariffOpen(true);
  };

  const saveTariff = async () => {
    if (!selectedStationId) return;
    if (!validateTariff(['tollVehicleClassCode', 'amount', 'currency', 'effectiveFrom'])) return;
    const payload = {
      tollStationId: selectedStationId,
      tollVehicleClassCode: tariffValues.tollVehicleClassCode as string,
      amount: Number(tariffValues.amount),
      currency: tariffValues.currency as string,
      effectiveFrom: tariffValues.effectiveFrom as string,
      // An empty end date means "still in force" — send null, not an empty string.
      effectiveTo: tariffValues.effectiveTo ? tariffValues.effectiveTo : null,
    };
    try {
      if (tariffValues.tollTariffId) {
        await updateTariff.mutateAsync({ tollTariffId: tariffValues.tollTariffId, tariff: payload });
      } else {
        await createTariff.mutateAsync(payload);
      }
      setTariffOpen(false);
    } catch {
      // Overlapping windows come back as TOLL_OVERLAPPING_TARIFF in the toast.
    }
  };

  /* ----------------------------------------------------------------- tables */

  const classData = useMemo(
    () => ({
      columns: [
        { name: 'code', title: t('tolls.catalog.code'), align: 'left' as const },
        { name: 'name', title: t('tolls.catalog.name'), align: 'left' as const },
        { name: 'sortOrder', title: t('tolls.catalog.sortOrder'), align: 'center' as const },
        { name: 'active', title: t('tolls.catalog.active'), align: 'center' as const },
        { name: 'action', title: t('generic.action'), align: 'center' as const },
        { name: 'id' },
      ],
      rows: vehicleClasses.map((vehicleClass) => ({
        code: <Name name={vehicleClass.code} />,
        name: <Description description={vehicleClass.name} />,
        sortOrder: <Name name={vehicleClass.sortOrder} />,
        active: (
          <ArgonBadge
            variant="gradient"
            color={vehicleClass.active ? 'success' : 'secondary'}
            size="xs"
            container
            badgeContent={vehicleClass.active ? t('generic.yes') : t('generic.no')}
          />
        ),
        action: (
          <>
            <ArgonButton variant="text" color="dark" onClick={() => openClass(vehicleClass)}>
              <Icon>edit</Icon>&nbsp;{t('generic.edit')}
            </ArgonButton>
            {vehicleClass.active && (
              <ArgonButton
                variant="text"
                color="error"
                onClick={() => setClassToDeactivate(vehicleClass.tollVehicleClassId)}
              >
                <Icon>block</Icon>&nbsp;{t('tolls.catalog.deactivate')}
              </ArgonButton>
            )}
          </>
        ),
        id: vehicleClass.tollVehicleClassId,
      })),
    }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [vehicleClasses, t]
  );

  const stationData = useMemo(
    () => ({
      columns: [
        { name: 'name', title: t('tolls.catalog.name'), align: 'left' as const },
        { name: 'roadName', title: t('tolls.catalog.roadName'), align: 'left' as const },
        { name: 'country', title: t('tolls.catalog.country'), align: 'left' as const },
        { name: 'active', title: t('tolls.catalog.active'), align: 'center' as const },
        { name: 'action', title: t('generic.action'), align: 'center' as const },
        { name: 'id' },
      ],
      rows: stations.map((station) => ({
        name: <Name name={station.name} />,
        roadName: <Description description={station.roadName ?? '-'} />,
        country: <Description description={station.country ?? '-'} />,
        active: (
          <ArgonBadge
            variant="gradient"
            color={station.active ? 'success' : 'secondary'}
            size="xs"
            container
            badgeContent={station.active ? t('generic.yes') : t('generic.no')}
          />
        ),
        action: (
          <>
            <ArgonButton
              variant="text"
              color="info"
              onClick={() => setSelectedStationId(station.tollStationId)}
            >
              <Icon>receipt_long</Icon>&nbsp;{t('tolls.catalog.tariffs')}
            </ArgonButton>
            <ArgonButton variant="text" color="dark" onClick={() => openStation(station)}>
              <Icon>edit</Icon>&nbsp;{t('generic.edit')}
            </ArgonButton>
            {station.active && (
              <ArgonButton
                variant="text"
                color="error"
                onClick={() => setStationToDeactivate(station.tollStationId)}
              >
                <Icon>block</Icon>&nbsp;{t('tolls.catalog.deactivate')}
              </ArgonButton>
            )}
          </>
        ),
        id: station.tollStationId,
      })),
    }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [stations, t]
  );

  const tariffData = useMemo(
    () => ({
      columns: [
        { name: 'vehicleClass', title: t('tolls.vehicleClass'), align: 'left' as const },
        { name: 'amount', title: t('tolls.amount'), align: 'right' as const },
        { name: 'effectiveFrom', title: t('tolls.catalog.effectiveFrom'), align: 'left' as const },
        { name: 'effectiveTo', title: t('tolls.catalog.effectiveTo'), align: 'left' as const },
        { name: 'action', title: t('generic.action'), align: 'center' as const },
        { name: 'id' },
      ],
      // Newest window first; superseded rows stay listed so a past estimate stays reproducible.
      rows: [...tariffs]
        .sort((a, b) => b.effectiveFrom.localeCompare(a.effectiveFrom))
        .map((tariff) => ({
          vehicleClass: <Name name={tariff.tollVehicleClassCode} />,
          amount: <Description description={`${tariff.amount} ${tariff.currency}`} />,
          effectiveFrom: <Description description={tariff.effectiveFrom} />,
          effectiveTo: (
            <Description description={tariff.effectiveTo ?? t('tolls.catalog.openEnded')} />
          ),
          action: (
            <>
              <ArgonButton variant="text" color="dark" onClick={() => openTariff(tariff)}>
                <Icon>edit</Icon>&nbsp;{t('generic.edit')}
              </ArgonButton>
              <ArgonButton
                variant="text"
                color="error"
                onClick={() => setTariffToDelete(tariff.tollTariffId)}
              >
                <Icon>delete</Icon>&nbsp;{t('generic.delete')}
              </ArgonButton>
            </>
          ),
          id: tariff.tollTariffId,
        })),
    }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [tariffs, t]
  );

  const [importOpen, setImportOpen] = useState(false);
  const selectedStation = stations.find((station) => station.tollStationId === selectedStationId);

  return (
    <>
      <TableAccordion
        title={t('tolls.catalog.title')}
        expanded={expanded}
        setExpanded={setExpanded}
      >
        <Grid container spacing={2}>
          <Grid size={{ xs: 12 }}>
            <ArgonBox display="flex" alignItems="center" justifyContent="space-between" flexWrap="wrap" gap={1}>
              <ArgonTypography variant="button" fontWeight="medium">
                {t('tolls.catalog.vehicleClasses')}
              </ArgonTypography>
              <ArgonButton variant="outlined" color="info" size="small" onClick={() => openClass()}>
                <Icon>add</Icon>&nbsp;{t('tolls.catalog.addClass')}
              </ArgonButton>
            </ArgonBox>
            {vehicleClasses.length === 0 ? (
              <ArgonTypography variant="caption" color="secondary">
                {t('tolls.catalog.emptyClasses')}
              </ArgonTypography>
            ) : (
              <Table columns={classData.columns} rows={classData.rows} compact selectedField="code" />
            )}
          </Grid>

          <Grid size={{ xs: 12, lg: 7 }}>
            <ArgonBox display="flex" alignItems="center" justifyContent="space-between" flexWrap="wrap" gap={1}>
              <ArgonTypography variant="button" fontWeight="medium">
                {t('tolls.catalog.stations')}
              </ArgonTypography>
              <ArgonBox display="flex" gap={1}>
                <ArgonButton
                  variant="outlined"
                  color="secondary"
                  size="small"
                  onClick={() => setImportOpen(true)}
                >
                  <Icon>upload_file</Icon>&nbsp;{t('tolls.catalog.import')}
                </ArgonButton>
                <ArgonButton variant="outlined" color="info" size="small" onClick={() => openStation()}>
                  <Icon>add</Icon>&nbsp;{t('tolls.catalog.addStation')}
                </ArgonButton>
              </ArgonBox>
            </ArgonBox>
            {stations.length === 0 ? (
              <ArgonTypography variant="caption" color="secondary">
                {t('tolls.catalog.emptyStations')}
              </ArgonTypography>
            ) : (
              <Table
                columns={stationData.columns}
                rows={stationData.rows}
                compact
                selectedField="name"
              />
            )}
          </Grid>

          <Grid size={{ xs: 12, lg: 5 }}>
            <ArgonBox display="flex" alignItems="center" justifyContent="space-between" flexWrap="wrap" gap={1}>
              <ArgonTypography variant="button" fontWeight="medium">
                {selectedStation
                  ? `${t('tolls.catalog.history')} — ${selectedStation.name}`
                  : t('tolls.catalog.tariffs')}
              </ArgonTypography>
              {selectedStationId && (
                <ArgonButton variant="outlined" color="info" size="small" onClick={() => openTariff()}>
                  <Icon>add</Icon>&nbsp;{t('tolls.catalog.addTariff')}
                </ArgonButton>
              )}
            </ArgonBox>
            {!selectedStationId ? (
              <ArgonTypography variant="caption" color="secondary">
                {t('tolls.catalog.selectStation')}
              </ArgonTypography>
            ) : tariffs.length === 0 ? (
              <ArgonTypography variant="caption" color="secondary">
                {t('tolls.catalog.emptyTariffs')}
              </ArgonTypography>
            ) : (
              <>
                <Table
                  columns={tariffData.columns}
                  rows={tariffData.rows}
                  compact
                  selectedField="vehicleClass"
                />
                <ArgonTypography variant="caption" color="secondary">
                  {t('tolls.catalog.historyHint')}
                </ArgonTypography>
              </>
            )}
          </Grid>
        </Grid>
      </TableAccordion>

      <TollVehicleClassDialog
        open={classOpen}
        setOpen={setClassOpen}
        handleSubmit={saveClass}
        values={classValues}
        handleChange={classChange}
        errors={classErrors}
      />
      <TollStationDialog
        open={stationOpen}
        setOpen={setStationOpen}
        handleSubmit={saveStation}
        values={stationValues}
        handleChange={stationChange}
        errors={stationErrors}
      />
      <TollTariffDialog
        open={tariffOpen}
        setOpen={setTariffOpen}
        handleSubmit={saveTariff}
        values={tariffValues}
        handleChange={tariffChange}
        errors={tariffErrors}
        vehicleClasses={vehicleClasses}
      />
      <TollImportDialog open={importOpen} setOpen={setImportOpen} />

      <ConfirmDialog
        title={t('tolls.catalog.deactivateClassTitle')}
        message={t('tolls.catalog.deactivateClassMessage')}
        open={!!classToDeactivate}
        setOpen={() => setClassToDeactivate(null)}
        onConfirm={async () => {
          await deactivateClass.mutateAsync(classToDeactivate as string).catch(() => undefined);
          setClassToDeactivate(null);
        }}
      />
      <ConfirmDialog
        title={t('tolls.catalog.deactivateStationTitle')}
        message={t('tolls.catalog.deactivateStationMessage')}
        open={!!stationToDeactivate}
        setOpen={() => setStationToDeactivate(null)}
        onConfirm={async () => {
          await deactivateStation.mutateAsync(stationToDeactivate as string).catch(() => undefined);
          setStationToDeactivate(null);
        }}
      />
      <ConfirmDialog
        title={t('tolls.catalog.deleteTariffTitle')}
        message={t('tolls.catalog.deleteTariffMessage')}
        open={!!tariffToDelete}
        setOpen={() => setTariffToDelete(null)}
        onConfirm={async () => {
          await deleteTariff.mutateAsync(tariffToDelete as string).catch(() => undefined);
          setTariffToDelete(null);
        }}
      />
    </>
  );
}

export default ManageTollCatalog;
