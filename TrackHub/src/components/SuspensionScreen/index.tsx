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

import { useTranslation } from 'react-i18next';
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import { ACCOUNT_STATUS_I18N } from 'data/accountStatuses';
import type { AccountStatus, AccountContext } from 'api/manager/accounts';

interface SuspensionScreenProps {
  status: AccountStatus | null;
  branding: AccountContext['branding'] | null;
}

// Full-screen state rendered instead of the app when the account is non-operational.
function SuspensionScreen({ status, branding }: SuspensionScreenProps) {
  const { t } = useTranslation();
  const statusKey = status ? ACCOUNT_STATUS_I18N[status] : undefined;

  return (
    <ArgonBox
      display="flex"
      flexDirection="column"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      textAlign="center"
      px={3}>
      {branding?.displayName && (
        <ArgonTypography variant="h4" fontWeight="bold" mb={1}>
          {branding.displayName}
        </ArgonTypography>
      )}
      <ArgonTypography variant="h3" fontWeight="bold" color="error" mb={2}>
        {t('suspension.title')}
      </ArgonTypography>
      <ArgonTypography variant="body1" color="secondary" mb={2}>
        {t('suspension.message')}
      </ArgonTypography>
      <ArgonTypography variant="button" color="text">
        {t('suspension.statusLabel')}: {statusKey ? t(statusKey) : status}
      </ArgonTypography>
    </ArgonBox>
  );
}

export default SuspensionScreen;
