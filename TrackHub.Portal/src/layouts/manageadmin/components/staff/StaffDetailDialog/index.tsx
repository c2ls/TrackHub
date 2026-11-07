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

/**
 * Everything about ONE staff that is not the profile form: credentials and devices. Opened from
 * the staff table so the table itself stays one row per staff with a single action.
 */

import { useTranslation } from 'react-i18next';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import StaffAccessPanel from 'layouts/manageadmin/components/staff/StaffAccessPanel';
import type { Staff } from 'api/manager/staff';

interface StaffDetailDialogProps {
  accountId: string;
  staff: Staff | null;
  onClose: () => void;
}

function StaffDetailDialog({ accountId, staff, onClose }: StaffDetailDialogProps) {
  const { t } = useTranslation();

  return (
    <Dialog open={staff !== null} onClose={onClose} fullWidth maxWidth="md">
      <DialogTitle>
        <ArgonTypography variant="h5">{staff?.name}</ArgonTypography>
        <ArgonTypography variant="caption" color="text">
          {[staff?.documentNumber, staff?.phone].filter(Boolean).join(' · ')}
        </ArgonTypography>
      </DialogTitle>
      <DialogContent>
        {staff && <StaffAccessPanel accountId={accountId} staffId={staff.staffId} />}
      </DialogContent>
      <DialogActions>
        <ArgonButton color="secondary" onClick={onClose}>
          {t('generic.close')}
        </ArgonButton>
      </DialogActions>
    </Dialog>
  );
}

export default StaffDetailDialog;
