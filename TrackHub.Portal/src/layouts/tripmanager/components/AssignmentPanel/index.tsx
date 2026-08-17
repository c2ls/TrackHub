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
import Icon from '@mui/material/Icon';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import { formatDateTime } from 'utils/dateUtils';
import type { Driver } from 'api/manager/drivers';
import type { Transporter } from 'api/manager/transporters';
import type { TripDetail } from 'api/tripManagement/trips';

interface AssignmentPanelProps {
  detail: TripDetail;
  drivers: Driver[];
  transporters: Transporter[];
  onAssign: (driverId: string, transporterId: string | null) => void;
  assigning: boolean;
  editable: boolean;
}

/** Driver assignment for a trip, plus the current assignment's acknowledgement state. */
function AssignmentPanel({
  detail,
  drivers,
  transporters,
  onAssign,
  assigning,
  editable,
}: AssignmentPanelProps) {
  const { t } = useTranslation();
  const assignment = detail.assignment;
  const [driverId, setDriverId] = useState<string>(assignment?.driverId ?? '');
  const [transporterId, setTransporterId] = useState<string>(
    assignment?.transporterId ?? detail.trip.transporterId
  );

  const driverName = (id?: string | null) =>
    drivers.find((driver) => driver.driverId === id)?.name ?? t('trips.unassigned');
  const transporterName = (id?: string | null) =>
    transporters.find((transporter) => transporter.transporterId === id)?.name ?? '-';

  return (
    <ArgonBox>
      {assignment ? (
        <ArgonBox display="flex" gap={2} flexWrap="wrap" alignItems="center" mb={1}>
          <ArgonBox>
            <ArgonTypography variant="caption" color="secondary" display="block">
              {t('trips.assignment.driver')}
            </ArgonTypography>
            <ArgonTypography variant="button" fontWeight="medium">
              {driverName(assignment.driverId)}
            </ArgonTypography>
          </ArgonBox>
          <ArgonBox>
            <ArgonTypography variant="caption" color="secondary" display="block">
              {t('trips.assignment.transporter')}
            </ArgonTypography>
            <ArgonTypography variant="button" fontWeight="medium">
              {transporterName(assignment.transporterId)}
            </ArgonTypography>
          </ArgonBox>
          <ArgonBox>
            <ArgonTypography variant="caption" color="secondary" display="block">
              {t('trips.assignment.assignedAt')}
            </ArgonTypography>
            <ArgonTypography variant="button" fontWeight="medium">
              {formatDateTime(assignment.assignedAt)}
            </ArgonTypography>
          </ArgonBox>
          <ArgonBadge
            variant="gradient"
            color={assignment.acknowledgedAt ? 'success' : 'warning'}
            size="xs"
            container
            badgeContent={
              assignment.acknowledgedAt
                ? formatDateTime(assignment.acknowledgedAt)
                : t('trips.assignment.notAcknowledged')
            }
          />
        </ArgonBox>
      ) : (
        <ArgonTypography variant="caption" color="secondary" display="block" mb={1}>
          {t('trips.assignment.none')}
        </ArgonTypography>
      )}

      {editable && (
        <ArgonBox display="flex" gap={1} alignItems="flex-end" flexWrap="wrap">
          <ArgonBox width="240px">
            <CustomSelect
              list={drivers
                .filter((driver) => driver.active)
                .map((driver) => ({ value: driver.driverId, label: driver.name }))}
              handleChange={(event) => setDriverId(String(event.target.value ?? ''))}
              name="assignDriverId"
              id="assignDriverId"
              label={t('trips.assignment.driver')}
              value={driverId}
              numericValue={false}
              placeholder={t('trips.assignment.selectDriver')}
            />
          </ArgonBox>
          <ArgonBox width="240px">
            <CustomSelect
              list={transporters.map((transporter) => ({
                value: transporter.transporterId,
                label: transporter.name,
              }))}
              handleChange={(event) => setTransporterId(String(event.target.value ?? ''))}
              name="assignTransporterId"
              id="assignTransporterId"
              label={t('trips.assignment.transporter')}
              value={transporterId}
              numericValue={false}
            />
          </ArgonBox>
          <ArgonButton
            variant="gradient"
            color="info"
            size="small"
            disabled={!driverId || assigning}
            onClick={() => onAssign(driverId, transporterId || null)}
          >
            <Icon>person_add</Icon>&nbsp;{t('trips.assignment.assign')}
          </ArgonButton>
        </ArgonBox>
      )}
    </ArgonBox>
  );
}

export default AssignmentPanel;
