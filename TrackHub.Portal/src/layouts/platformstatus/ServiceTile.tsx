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
import Card from '@mui/material/Card';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import type { ServiceState } from 'api/core/healthProbe';

export interface ServiceTileProps {
  /** Plain-language service name, already localized (never a codename — ST-08). */
  name: string;
  /** One-line description of what the service does for the user. */
  description: string;
  state: ServiceState;
  /** Optional technical detail line (e.g. "reachable, database not"). */
  detail?: string;
}

const STATE_COLOR: Record<ServiceState, 'success' | 'error' | 'secondary'> = {
  up: 'success',
  down: 'error',
  unknown: 'secondary',
};

const ServiceTile = ({ name, description, state, detail }: ServiceTileProps) => {
  const { t } = useTranslation();
  const color = STATE_COLOR[state];

  return (
    <Card sx={{ height: '100%' }} data-testid="service-tile" data-state={state}>
      <ArgonBox p={2} display="flex" alignItems="flex-start" gap={1.5}>
        <ArgonBox
          flexShrink={0}
          mt={0.5}
          width="0.75rem"
          height="0.75rem"
          borderRadius="50%"
          bgColor={color}
          // The dot is decorative; the state is always also stated in words below.
          aria-hidden="true"
        />
        <ArgonBox flex={1} minWidth={0}>
          <ArgonTypography variant="button" fontWeight="medium" textTransform="none">
            {name}
          </ArgonTypography>
          <ArgonTypography variant="caption" color="text" display="block">
            {description}
          </ArgonTypography>
          <ArgonTypography variant="button" color={color} fontWeight="bold" textTransform="none" display="block" mt={0.5}>
            {t(`platformStatus.state.${state}` as const)}
          </ArgonTypography>
          {detail && (
            <ArgonTypography variant="caption" color="text" display="block">
              {detail}
            </ArgonTypography>
          )}
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
};

export default ServiceTile;
