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

import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { useTranslation } from 'react-i18next';
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import { formatDateTime, formatShortDateTime } from "utils/dateUtils";
import { getUnitStatus } from "layouts/dashboard/utils/dashboard";
import type { UnitStatus } from "layouts/dashboard/utils/dashboard";
import type { Position } from "api/router/router";
import type { TableColumn } from "controls/Tables/Table";

/** A column descriptor consumed by the vendored `Table` control. */
export type TransporterColumn = TableColumn;
/** A rendered table row for the live transporters list. */
export interface TransporterRow {
  status: ReactNode;
  name: ReactNode;
  datetime: ReactNode;
  speed: ReactNode;
  id: string;
  [key: string]: unknown;
}
export interface TransporterTableData {
  columns: TransporterColumn[];
  rows: TransporterRow[];
}
export interface UseTransportersTableData {
  data: TransporterTableData;
}

const STATUS_RANK: Record<UnitStatus, number> = { moving: 2, stopped: 1, offline: 0 };
const STATUS_COLOR: Record<UnitStatus, "success" | "error" | "secondary"> = { moving: "success", stopped: "error", offline: "secondary" };
const STATUS_LABEL = {
  moving: "dashboard.statusMoving",
  stopped: "dashboard.statusStopped",
  offline: "dashboard.statusOffline",
} as const;
function useTransportersTableData(transporters: Position[], onlineInterval: number): UseTransportersTableData {
  const { t } = useTranslation();
  const [data, setData] = useState<TransporterTableData>({ columns: [], rows: [] });

  const buildTableData = (transporters: Position[]): TransporterTableData => ({
    columns: [
      { name: "status", title: t('transporterMap.status'), hideTitle: true, align: "center", width: "7%", sortValue: (row) => STATUS_RANK[row.statusValue as UnitStatus] },
      { name: "name", title:t('transporterMap.name'), align: "left", width: "30%" },
      // The cells render formatted text, so the columns sort on the raw values carried alongside.
      { name: "datetime", title:t('transporterMap.dateTime'), align: "left", width: "44%", sortValue: (row) => row.datetimeValue as string },
      { name: "speed", title:t('transporterMap.speedShort'), align: "right", width: "19%", sortValue: (row) => Number(row.speedValue) },
      { name: "id" }
    ],
    rows: transporters.map(transporter => {
      const status = getUnitStatus(transporter, onlineInterval);
      return {
        status: (
          <ArgonBox
            component="span"
            display="inline-block"
            width="0.625rem"
            height="0.625rem"
            borderRadius="50%"
            bgColor={STATUS_COLOR[status]}
            title={t(STATUS_LABEL[status])}
          />
        ),
        name: (
          <ArgonTypography variant="button" fontWeight="medium" noWrap display="block" title={transporter.deviceName}>
            {transporter.deviceName}
          </ArgonTypography>
        ),
        datetime: (
          <ArgonTypography variant="caption" color="secondary" fontWeight="medium" noWrap display="block" title={formatDateTime(transporter.deviceDateTime)}>
            {formatShortDateTime(transporter.deviceDateTime)}
          </ArgonTypography>
        ),
        speed: (
          <ArgonTypography variant="caption" fontWeight="medium" color="text" noWrap display="block">
            {transporter.speed}
          </ArgonTypography>
        ),
        statusValue: status,
        speedValue: transporter.speed,
        datetimeValue: transporter.deviceDateTime,
        id: transporter.transporterId
      };
    }),
  });

  useEffect(() => {
      function fetchData() {
        if (transporters) {
          setData(buildTableData(transporters));
        }
      }
      fetchData();
      // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [transporters, onlineInterval]);

  return {
    data
  };
}

export default useTransportersTableData;
