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

import { useState, useEffect, useContext, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import TableAccordion from "controls/Accordions/TableAccordion";
import CustomTextField from 'controls/Dialogs/CustomTextField';
import ArgonBox from "components/ArgonBox";
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import useForm from 'controls/Dialogs/useForm';
import { getAccountByUser } from "api/manager/accounts";
import { getAccountBranding, updateAccountBranding } from "api/manager/branding";
import type { AccountBranding, AccountBrandingDtoInput } from "api/manager/branding";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';

// Dialog/form state for account branding — a partial view of the API record.
type BrandingFormValues = Partial<AccountBranding>;

const HEX_COLOR = /^#[0-9A-Fa-f]{6}$/;
const DEFAULT_COLOR = '#1A73E8';

function ManageBranding() {
  const { t } = useTranslation();
  const [expanded, setExpanded] = useState(false);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<BrandingFormValues>({});
  const { setLoading } = useContext(LoadingContext);
  const hasLoaded = useRef(false);

  useEffect(() => {
    if (expanded && !hasLoaded.current) {
      (async () => {
        setLoading(true);
        try {
          const account = await getAccountByUser();
          if (account?.accountId) {
            const branding = await getAccountBranding(account.accountId);
            setValues(branding
              ? { ...branding }
              : { accountId: account.accountId, primaryColor: DEFAULT_COLOR, displayName: account.name });
          }
          hasLoaded.current = true;
        } catch (error) {
          notifyApiError(error);
        } finally {
          setLoading(false);
        }
      })();
    }
  }, [expanded]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSave = async () => {
    const colorOk = HEX_COLOR.test(values.primaryColor || '');
    const nameOk = validate(['displayName']);
    if (!colorOk) {
      setErrors({ ...errors, primaryColor: t('branding.primaryColor') });
    }
    if (!nameOk || !colorOk) {
      return;
    }
    setLoading(true);
    try {
      // Gated by validate(['displayName']) + the hex-color test, so displayName/
      // primaryColor are present and accountId was set on load — assert the input.
      const saved = await updateAccountBranding(values as AccountBrandingDtoInput);
      if (saved) {
        setValues({ ...saved });
      }
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const previewColor = HEX_COLOR.test(values.primaryColor || '') ? values.primaryColor : 'transparent';

  return (
    <TableAccordion
        title={t('branding.title')}
        expanded={expanded}
        setExpanded={setExpanded}>
      <ArgonBox p={2}>
        <CustomTextField
          name="displayName"
          id="displayName"
          label={t('branding.displayName')}
          type="text"
          fullWidth
          value={values.displayName || ''}
          onChange={handleChange}
          errorMsg={errors.displayName}
          required
        />
        <CustomTextField
          name="logoDocumentId"
          id="logoDocumentId"
          label={t('branding.logoDocumentId')}
          type="text"
          fullWidth
          value={values.logoDocumentId || ''}
          onChange={handleChange}
        />
        <ArgonBox display="flex" alignItems="center" gap={2} mt={1}>
          <CustomTextField
            name="primaryColor"
            id="primaryColor"
            label={t('branding.primaryColor')}
            type="text"
            value={values.primaryColor || ''}
            onChange={handleChange}
            errorMsg={errors.primaryColor}
            required
          />
          <ArgonBox
            width="36px"
            height="36px"
            borderRadius="md"
            sx={{ backgroundColor: previewColor, border: '1px solid #ccc' }}
          />
          <ArgonTypography variant="caption" color="secondary">{t('branding.preview')}</ArgonTypography>
        </ArgonBox>
        <CustomTextField
          name="reportHeader"
          id="reportHeader"
          label={t('branding.reportHeader')}
          type="text"
          fullWidth
          value={values.reportHeader || ''}
          onChange={handleChange}
        />
        <ArgonBox mt={2}>
          <ArgonButton color="info" onClick={handleSave}>{t('branding.save')}</ArgonButton>
        </ArgonBox>
      </ArgonBox>
    </TableAccordion>
  );
}

export default ManageBranding;
