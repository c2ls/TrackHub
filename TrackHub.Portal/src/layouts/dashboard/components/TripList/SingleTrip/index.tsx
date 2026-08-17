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

import Icon from "@mui/material/Icon";
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonButton from "components/ArgonButton";
import { useTranslation } from 'react-i18next';
import { formatISODuration, formatTime } from "utils/timeUtils";
import { formatDistance } from "utils/distanceUtils";

interface SingleTripProps {
  id: string;
  from: string;
  to: string;
  event: number;
  duration: string;
  distance: number;
}

function SingleTrip({ id, from, to, event, duration, distance }: SingleTripProps) {
  const { t } = useTranslation();

  return (
    <ArgonBox key={id} component="li" py={1} pr={2} mb={1}>
      <ArgonBox display="flex" justifyContent="space-between" alignItems="center">
        <ArgonBox display="flex" alignItems="center">
          <ArgonBox mr={2}>
            <ArgonButton variant="outlined" color={event === 1 ? "error" : "success"} size="small" iconOnly circular>
              <Icon sx={{ fontWeight: "bold" }}>
                {event === 1 ? "stop_circle" : "arrow_forward"}
              </Icon>
            </ArgonButton>
          </ArgonBox>
          <ArgonBox display="flex" flexDirection="column">
            <ArgonTypography variant="button" fontWeight="medium" gutterBottom>
                {event === 1 ? t("tripPanel.stopLabel") : `${t("tripPanel.transitLabel")} (${formatDistance(distance)})`}
            </ArgonTypography>
            <ArgonTypography variant="caption" color="text">
              {t("tripPanel.fromLabel")}: {formatTime(from)} {t("tripPanel.toLabel")}: {formatTime(to)}
            </ArgonTypography>
          </ArgonBox>
        </ArgonBox>
        <ArgonTypography variant="button" color={event === 1 ? "error" : "success"} fontWeight="medium" textGradient>
          {formatISODuration(duration)}
        </ArgonTypography>
      </ArgonBox>
    </ArgonBox>
  );
}

export default SingleTrip;
