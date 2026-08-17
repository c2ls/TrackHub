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

import { useEffect, useMemo, useState, useContext } from "react";
import type { ReactNode } from "react";
import { useTranslation } from 'react-i18next';
import { Name } from "controls/Tables/components/tableComponents";
import { LoadingContext } from 'LoadingContext';
import { getStringValue } from 'utils/booleanUtils';
import { cleanString } from 'utils/stringUtils';
import Icon from "@mui/material/Icon";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import { useTransporterTypes, useUpdateTransporterType } from 'queries/transporterTypes';
import type { TransporterTypeItem, TransporterTypeDtoInput } from "api/manager/transporterTypes";

/**
 * Dialog/form state for a transporter type. Merges an API
 * {@link TransporterTypeItem} (when editing) with the loose edit shape; all
 * fields are optional and the requirement set is enforced by the dialog's
 * validate() gate before save.
 */
export interface TransporterTypeFormValues {
  transporterTypeId?: number;
  accBased?: boolean;
  stoppedGap?: number;
  maxDistance?: number;
  maxTimeGap?: number;
  type?: string;
}

/** A column descriptor / rendered row for the vendored transporter-types `Table`. */
export interface TransporterTypeTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type TransporterTypeTableRow = Record<string, ReactNode>;
export interface TransporterTypeTableData { columns: TransporterTypeTableColumn[]; rows: TransporterTypeTableRow[]; }

function useTransporterTypesTableData(
  fetchData: boolean,
  handleEditClick: (transporterType: TransporterTypeFormValues) => void
) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const { setLoading } = useContext(LoadingContext);

  const transporterTypesQuery = useTransporterTypes({ enabled: !!fetchData });
  const transporterTypeList = transporterTypesQuery.data ?? [];
  const updateTransporterType = useUpdateTransporterType();

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(transporterTypesQuery.isFetching);
  }, [transporterTypesQuery.isFetching, setLoading]);

  const onSave = async (transporterType: TransporterTypeFormValues) => {
    setLoading(true);
    try {
      // validate(['accBased','stoppedGap','maxTimeGap','maxDistance']) gates this;
      // assert the required update input fields at the boundary.
      await updateTransporterType.mutateAsync({
        transporterTypeId: transporterType.transporterTypeId,
        accBased: transporterType.accBased,
        stoppedGap: transporterType.stoppedGap,
        maxTimeGap: transporterType.maxTimeGap,
        maxDistance: transporterType.maxDistance,
      } as Omit<TransporterTypeDtoInput, 'transporterTypeId'> & { transporterTypeId: number });
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const handleOpen = (transporterType: TransporterTypeItem) => {
    handleEditClick(transporterType);
    setOpen(true);
  };

  const buildTableData = (rows: TransporterTypeItem[]): TransporterTypeTableData => ({
    columns: [
      { name: "name", title:t('transporterType.name'), align: "left" },
      { name: "accBased", title:t('transporterType.accBased'), align: "left" },
      { name: "stoppedGap", title:t('transporterType.stoppedGap'), align: "center" },
      { name: "maxTimeGap", title:t('transporterType.maxTimeGap'), align: "center" },
      { name: "maxDistance", title:t('transporterType.maxDistance'), align: "center" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "id" }
    ],
    rows: rows.map(transporterType => ({
      name: (
        <ArgonBadge
          variant="gradient"
          badgeContent={t(`transporterTypes.${cleanString(transporterType.type)}` as 'transporterTypes.unknown')}
          color="success"
          size="xs" container />
      ),
      accBased: <Name name={t(`generic.${getStringValue(transporterType.accBased)}` as 'generic.yes')} />,
      stoppedGap: <Name name={transporterType.stoppedGap} />,
      maxTimeGap: <Name name={transporterType.maxTimeGap} />,
      maxDistance: <Name name={transporterType.maxDistance} />,
      action: (
        <ArgonButton
            variant="text"
            color="dark"
            onClick={() => handleOpen(transporterType)}>
          <Icon>edit</Icon>&nbsp;{t('generic.edit')}
        </ArgonButton>
      ),
      id: transporterType.transporterTypeId
    })),
  });

  const data = useMemo(
    () => buildTableData(transporterTypeList),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [transporterTypeList, t]
  );

  return {
    data,
    open,
    onSave,
    setOpen};
}

export default useTransporterTypesTableData;
