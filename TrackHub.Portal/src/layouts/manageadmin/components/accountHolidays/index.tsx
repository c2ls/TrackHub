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

import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import TableAccordion from 'controls/Accordions/TableAccordion';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import { usePermissions } from 'context/permissions';
import { useAccountCalendar } from 'context/account';
import { PermissionActions, PermissionResources } from 'constants/permissions';
import { useAccountByUser } from 'queries/accounts';
import { useAccountHolidays, useSetAccountHolidays } from 'queries/accountHolidays';
import { formatDateOnly } from 'utils/dateUtils';

function ManageAccountHolidays() {
  const { t } = useTranslation();
  const { can } = usePermissions();
  const [expanded, setExpanded] = useState(false);
  const calendar = useAccountCalendar();
  const currentYear = Number(calendar.today().slice(0, 4));
  const [year, setYear] = useState(currentYear);
  const [edits, setEdits] = useState<{ date: string; name: string }[]>([]);
  const [date, setDate] = useState('');
  const [name, setName] = useState('');

  const accountQuery = useAccountByUser({ enabled: expanded });
  const accountId = accountQuery.data?.accountId;
  const canWrite = can(PermissionResources.Accounts, PermissionActions.Edit);
  const holidaysQuery = useAccountHolidays(accountId, year, { enabled: expanded });
  const saveHolidays = useSetAccountHolidays();

  useEffect(() => {
    setEdits((holidaysQuery.data ?? []).map((holiday) => ({ date: holiday.date, name: holiday.name })));
  }, [holidaysQuery.data]);

  const yearOptions = useMemo(
    () => [currentYear - 1, currentYear, currentYear + 1, currentYear + 2].map((value) => ({ value, label: String(value) })),
    [currentYear]
  );

  const inYear = date.startsWith(String(year));
  const add = () => {
    if (!date || !name.trim() || !inYear) return;
    setEdits((previous) => [...previous.filter((holiday) => holiday.date !== date), { date, name: name.trim() }]);
    setDate('');
    setName('');
  };

  return (
    <TableAccordion sectionKey="account-holidays" title={t('accountHolidays.title')} expanded={expanded} setExpanded={setExpanded}>
      <ArgonBox display="flex" alignItems="center" gap={1} flexWrap="wrap" mb={1}>
        <CustomSelect
          list={yearOptions}
          handleChange={(event) => setYear(Number(event.target.value))}
          name="holidayYear"
          id="holidayYear"
          label={t('accountHolidays.year')}
          value={year}
          fullWidth={false}
        />
        <ArgonBox flexGrow={1} />
        {canWrite && (
          <ArgonButton
            variant="gradient"
            color="info"
            size="small"
            disabled={saveHolidays.isPending || !accountId}
            onClick={() => accountId && saveHolidays.mutate({ accountId, year, dates: edits })}
          >
            <Icon>save</Icon>&nbsp;{t('accountHolidays.save')}
          </ArgonButton>
        )}
      </ArgonBox>
      <ArgonTypography variant="caption" color="secondary" display="block" mb={1}>
        {t('accountHolidays.hint')}
      </ArgonTypography>
      {canWrite && (
        <Grid container spacing={1} sx={{ alignItems: 'flex-end' }}>
          <Grid size={{ xs: 12, sm: 3 }}>
            <CustomTextField
              margin="none"
              name="holidayDate"
              id="holidayDate"
              label={t('accountHolidays.date')}
              type="date"
              slotProps={{ inputLabel: { shrink: true } }}
              value={date}
              onChange={(event) => setDate(event.target.value)}
              errorMsg={date && !inYear ? t('accountHolidays.invalidDate') : undefined}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 5 }}>
            <CustomTextField
              margin="none"
              name="holidayName"
              id="holidayName"
              label={t('accountHolidays.name')}
              type="text"
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 2 }}>
            <ArgonButton variant="outlined" color="info" size="small" disabled={!date || !name.trim() || !inYear} onClick={add}>
              <Icon>add</Icon>&nbsp;{t('accountHolidays.add')}
            </ArgonButton>
          </Grid>
        </Grid>
      )}
      <ArgonBox mt={1}>
        {edits.length === 0 ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('accountHolidays.none')}
          </ArgonTypography>
        ) : (
          [...edits]
            .sort((a, b) => a.date.localeCompare(b.date))
            .map((holiday) => (
              <ArgonBox key={holiday.date} display="flex" alignItems="center" gap={1}>
                <ArgonTypography variant="caption" fontWeight="medium">
                  {formatDateOnly(holiday.date)}
                </ArgonTypography>
                <ArgonTypography variant="caption">{holiday.name}</ArgonTypography>
                {canWrite && (
                  <ArgonButton
                    variant="text"
                    color="error"
                    size="small"
                    iconOnly
                    aria-label={t('generic.delete')}
                    onClick={() => setEdits((previous) => previous.filter((item) => item.date !== holiday.date))}
                  >
                    <Icon>close</Icon>
                  </ArgonButton>
                )}
              </ArgonBox>
            ))
        )}
      </ArgonBox>
    </TableAccordion>
  );
}

export default ManageAccountHolidays;
