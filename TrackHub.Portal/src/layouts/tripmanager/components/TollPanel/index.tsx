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
import { useTranslation } from 'react-i18next';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonTypography from 'components/ArgonTypography';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import Table from 'controls/Tables/Table';
import { Name, Description } from 'controls/Tables/components/tableComponents';
import { useTollEstimate } from 'queries/trips';
import type { RoutePlan, TollVehicleClass } from 'api/tripManagement/trips';

interface TollPanelProps {
  routePlan?: RoutePlan | null;
  vehicleClasses: TollVehicleClass[];
  /** The trip's own toll class, used as the initial selection. */
  tripVehicleClass?: string | null;
}

/**
 * Toll estimate with a per-station breakdown and a vehicle-class selector.
 *
 * Three honesty rules the backend encodes and this panel must not paper over:
 * an empty catalog gives `NoStations` and a NULL amount (never a zero cost);
 * `PartialNoTariff` means at least one station on the route has no tariff for
 * the class, so the figure understates the real cost and says so; and a null
 * amount is rendered as "—", never as 0.
 */
function TollPanel({ routePlan, vehicleClasses, tripVehicleClass }: TollPanelProps) {
  const { t } = useTranslation();
  const [selectedClass, setSelectedClass] = useState<string>(tripVehicleClass ?? '');

  // Re-price on demand for another class; with no explicit selection the query
  // returns the plan's own stored estimate.
  const estimateQuery = useTollEstimate(routePlan?.routePlanId, selectedClass || null, {
    enabled: !!routePlan?.routePlanId,
  });

  if (!routePlan?.routePlanId) {
    return (
      <ArgonTypography variant="caption" color="secondary">
        {t('tolls.noEstimateYet')}
      </ArgonTypography>
    );
  }

  const estimate = estimateQuery.data;
  const stations = estimate?.stations ?? [];
  const untariffed = stations.filter((station) => !station.hasTariff).length;
  const status = estimate?.tollStatus ?? routePlan.tollStatus;

  const classOptions = vehicleClasses
    .filter((vehicleClass) => vehicleClass.active || vehicleClass.code === selectedClass)
    .map((vehicleClass) => ({
      value: vehicleClass.code,
      label: `${vehicleClass.code} — ${vehicleClass.name}`,
    }));

  const statusColor =
    status === 'Computed'
      ? 'success'
      : status === 'PartialNoTariff' || status === 'MixedCurrency'
        ? 'warning'
        : 'secondary';

  const columns = [
    { name: 'station', title: t('tolls.station'), align: 'left' as const },
    { name: 'road', title: t('tolls.road'), align: 'left' as const },
    { name: 'amount', title: t('tolls.amount'), align: 'right' as const },
    { name: 'id' },
  ];

  const rows = stations.map((station) => ({
    station: <Name name={station.name} />,
    road: (
      <Description
        description={[station.roadName, station.direction].filter(Boolean).join(' · ') || '-'}
      />
    ),
    amount: station.hasTariff ? (
      <Description description={`${station.amount ?? '-'} ${station.currency ?? ''}`} />
    ) : (
      <ArgonBadge variant="gradient" color="warning" size="xs" container badgeContent={t('tolls.noTariff')} />
    ),
    id: station.tollStationId,
  }));

  return (
    <ArgonBox>
      <ArgonBox display="flex" alignItems="center" gap={2} flexWrap="wrap">
        <ArgonBox width="260px">
          <CustomSelect
            list={classOptions}
            handleChange={(event) => setSelectedClass(String(event.target.value ?? ''))}
            name="tollVehicleClass"
            id="tollVehicleClass"
            label={t('tolls.vehicleClass')}
            value={selectedClass}
            numericValue={false}
            placeholder={t('tolls.selectVehicleClass')}
          />
        </ArgonBox>
        <ArgonBox>
          <ArgonTypography variant="caption" color="secondary" display="block">
            {t('tolls.estimated')}
          </ArgonTypography>
          <ArgonTypography variant="h5" fontWeight="medium">
            {/* A null amount is an ABSENT estimate, not a zero cost. */}
            {estimate?.estimatedTollAmount == null
              ? '—'
              : `${estimate.estimatedTollAmount} ${estimate.currency ?? ''}`}
          </ArgonTypography>
        </ArgonBox>
        <ArgonBadge
          variant="gradient"
          color={statusColor}
          size="sm"
          container
          badgeContent={t(`tolls.statuses.${status}` as 'tolls.statuses.Computed')}
        />
      </ArgonBox>

      {status === 'PartialNoTariff' && (
        <ArgonTypography variant="caption" color="warning" display="block" mt={1}>
          {t('tolls.partialWarning', { count: untariffed })}
        </ArgonTypography>
      )}
      {status === 'NoStations' && (
        <ArgonTypography variant="caption" color="secondary" display="block" mt={1}>
          {t('tolls.noStationsHint')}
        </ArgonTypography>
      )}
      {status === 'NotComputed' && (
        <ArgonTypography variant="caption" color="secondary" display="block" mt={1}>
          {t('tolls.notComputedHint')}
        </ArgonTypography>
      )}
      {/*
        A cross-border route matches stations priced in different currencies, so the backend
        refuses a total rather than adding COP to USD and labelling the sum with whichever it
        matched first. The per-station table below is the honest form of the answer.
      */}
      {status === 'MixedCurrency' && (
        <ArgonTypography variant="caption" color="warning" display="block" mt={1}>
          {t('tolls.mixedCurrencyHint')}
        </ArgonTypography>
      )}

      {stations.length > 0 && (
        <ArgonBox mt={1}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('tolls.stations')}
          </ArgonTypography>
          <Table columns={columns} rows={rows} compact selectedField="station" />
        </ArgonBox>
      )}
    </ArgonBox>
  );
}

export default TollPanel;
