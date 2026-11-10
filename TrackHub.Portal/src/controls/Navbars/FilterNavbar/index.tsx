/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

import { useTranslation } from "react-i18next";
import ArgonBox from "components/ArgonBox";
import ArgonButton from "components/ArgonButton";
import type { ReactNode } from "react";
import CustomTextField from "controls/Dialogs/CustomTextField";
import type { FormChangeHandler } from "controls/Dialogs/useForm";

export interface FilterNavbarProps {
  picker: ReactNode;
  values: Record<string, string | number | undefined>;
  handleChange: FormChangeHandler;
  errors: Record<string, string | undefined>;
  handleSearch: () => void;
  /** Extra controls placed after the Search button, on the same row. */
  children?: ReactNode;
}

// Buttons sit beside labelled fields, so they are pushed down by the label's height.
const BESIDE_FIELDS = { mt: "1.375rem" };

function FilterNavbar({ picker, values, handleChange, errors, handleSearch, children }: FilterNavbarProps) {
  const { t } = useTranslation();

  return (
    <ArgonBox display="flex" flexWrap="wrap" alignItems="flex-start" columnGap={2} rowGap={1} mb={2}>
      <ArgonBox width={{ xs: "100%", sm: "13.5rem" }}>
        <CustomTextField
          label={t("filters.startDate")}
          type="datetime-local"
          name="startDate"
          id="startDate"
          value={values.startDate || ""}
          onChange={handleChange}
          slotProps={{ inputLabel: { shrink: true } }}
          errorMsg={errors.startDate}
          fullWidth
        />
      </ArgonBox>
      <ArgonBox width={{ xs: "100%", sm: "13.5rem" }}>
        <CustomTextField
          label={t("filters.endDate")}
          type="datetime-local"
          name="endDate"
          id="endDate"
          value={values.endDate || ""}
          onChange={handleChange}
          slotProps={{ inputLabel: { shrink: true } }}
          errorMsg={errors.endDate}
          fullWidth
        />
      </ArgonBox>
      <ArgonBox width={{ xs: "100%", sm: "15rem" }}>{picker}</ArgonBox>
      <ArgonBox sx={BESIDE_FIELDS}>
        <ArgonButton variant="contained" color="primary" onClick={handleSearch}>
          {t("filters.search")}
        </ArgonButton>
      </ArgonBox>
      {children && (
        <ArgonBox display="flex" alignItems="center" gap={1} ml="auto" sx={BESIDE_FIELDS}>
          {children}
        </ArgonBox>
      )}
    </ArgonBox>
  );
}

export default FilterNavbar;
