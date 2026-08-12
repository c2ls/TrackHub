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
import { Name, Description } from "controls/Tables/components/tableComponents";
import ArgonTypography from "components/ArgonTypography";
import Icon from "@mui/material/Icon";
import ArgonButton from "components/ArgonButton";
import { formatDateTime } from "utils/dateUtils";
import type { Position } from "api/router/router";

/** A column descriptor consumed by the vendored `Table` control. */
export interface TransporterColumn { name: string; title?: string; align?: "left" | "right" | "center"; width?: string; }
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

function useTransportersTableData(transporters: Position[]): UseTransportersTableData {
  const { t } = useTranslation();
  const [data, setData] = useState<TransporterTableData>({ columns: [], rows: [] });

  const buildTableData = (transporters: Position[]): TransporterTableData => ({
    columns: [
      { name: "status", title:t('transporterMap.status'), align: "center", width: "10%" },
      { name: "name", title:t('transporterMap.name'), align: "left", width: "35%" },
      { name: "datetime", title:t('transporterMap.dateTime'), align: "left", width: "35%" },
      { name: "speed", title:t('transporterMap.speed'), align: "left", width: "20%" },
      { name: "id" }
    ],
    rows: transporters.map(transporter => ({
      status: (
        <ArgonButton variant="outlined" color={transporter.speed > 0 ? 'success' : 'error'} size="xsmall" iconOnly circular>
          <Icon sx={{ fontWeight: "bold", fontSize: "0.75rem" }}>{transporter.speed > 0 ? 'radio_button_checked' : 'radio_button_unchecked'}</Icon>
        </ArgonButton>
        ),
      name: <Name name={transporter.deviceName} />,
      datetime: (
        <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
          {formatDateTime(transporter.deviceDateTime)}
        </ArgonTypography>
      ),
      speed: <Description description={transporter.speed} />,
      id: transporter.transporterId
    })),
  });

  useEffect(() => {
      function fetchData() {
        if (transporters) {
          setData(buildTableData(transporters));
        }
      }
      fetchData();
      // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [transporters]);

  return {
    data
  };
}

export default useTransportersTableData;
