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
import SearchSelect from 'edition/SearchSelect';
import { useUserTransporterOptions } from 'edition/pickerOptions';
import { useStaffSearchOptions } from 'queries/staff';
import { formatDateTime } from 'utils/dateUtils';
import type { StaffLookup } from 'api/manager/staff';
import type { TransporterLookup } from 'api/manager/transporters';
import type { TripDetail } from 'api/tripManagement/trips';

interface AssignmentPanelProps {
  detail: TripDetail;
  /** Names for the staff this trip references; the picker searches the server itself. */
  staff: StaffLookup[];
  transporters: TransporterLookup[];
  onAssign: (staffId: string, transporterId: string | null) => void;
  assigning: boolean;
  editable: boolean;
}

/** Staff assignment for a trip, plus the current assignment's acknowledgement state. */
function AssignmentPanel({
  detail,
  staff,
  transporters,
  onAssign,
  assigning,
  editable,
}: AssignmentPanelProps) {
  const { t } = useTranslation();
  const assignment = detail.assignment;
  const [staffId, setStaffId] = useState<string>(assignment?.staffId ?? '');
  const [pickedStaffName, setPickedStaffName] = useState<string | null>(null);
  const [transporterId, setTransporterId] = useState<string>(
    assignment?.transporterId ?? detail.trip.transporterId
  );
  const [pickedTransporterName, setPickedTransporterName] = useState<string | null>(null);

  const staffName = (id?: string | null) =>
    staff.find((staff) => staff.staffId === id)?.name ?? t('trips.unassigned');
  const transporterName = (id?: string | null) =>
    transporters.find((transporter) => transporter.transporterId === id)?.name ?? '-';

  return (
    <ArgonBox data-testid="panel-assignment">
      {assignment ? (
        <ArgonBox display="flex" gap={2} flexWrap="wrap" alignItems="center" mb={1}>
          <ArgonBox>
            <ArgonTypography variant="caption" color="secondary" display="block">
              {t('trips.assignment.staff')}
            </ArgonTypography>
            <ArgonTypography variant="button" fontWeight="medium">
              {staffName(assignment.staffId)}
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
            <SearchSelect
              id="assignStaffId"
              label={t('trips.assignment.staff')}
              value={staffId || null}
              valueLabel={pickedStaffName ?? (staffId ? staffName(staffId) : null)}
              onChange={(option) => {
                setStaffId(option?.value ?? '');
                setPickedStaffName(option?.label ?? null);
              }}
              useOptions={useStaffSearchOptions}
              placeholder={t('trips.assignment.selectStaff')}
            />
          </ArgonBox>
          <ArgonBox width="240px">
            <SearchSelect
              id="assignTransporterId"
              label={t('trips.assignment.transporter')}
              value={transporterId || null}
              valueLabel={pickedTransporterName ?? (transporterId ? transporterName(transporterId) : null)}
              onChange={(option) => {
                setTransporterId(option?.value ?? '');
                setPickedTransporterName(option?.label ?? null);
              }}
              useOptions={useUserTransporterOptions}
            />
          </ArgonBox>
          <ArgonButton
            variant="gradient"
            color="info"
            size="small"
            disabled={!staffId || assigning}
            onClick={() => onAssign(staffId, transporterId || null)}
          >
            <Icon>person_add</Icon>&nbsp;{t('trips.assignment.assign')}
          </ArgonButton>
        </ArgonBox>
      )}
    </ArgonBox>
  );
}

export default AssignmentPanel;
