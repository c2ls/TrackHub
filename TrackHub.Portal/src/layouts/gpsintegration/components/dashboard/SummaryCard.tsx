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

import type { ReactNode } from 'react';
import Card from '@mui/material/Card';
import Divider from '@mui/material/Divider';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';

type RowColor = 'success' | 'warning' | 'error' | 'info' | 'text' | 'secondary';

export interface SummaryRow {
  label: string;
  value: ReactNode;
  /** Emphasis color for the value; defaults to plain text. */
  color?: RowColor;
}

interface SummaryCardProps {
  title: string;
  /** Headline figure of the card (e.g. "3/5"). */
  primary: ReactNode;
  /** Caption under the headline explaining what it counts. */
  primaryLabel: string;
  rows: SummaryRow[];
}

/** Dense dashboard card: one headline figure plus compact label/value rows. */
function SummaryCard({ title, primary, primaryLabel, rows }: SummaryCardProps) {
  return (
    <Card sx={{ height: '100%' }}>
      <ArgonBox p={2} display="flex" flexDirection="column" height="100%">
        <ArgonTypography
          variant="caption"
          color="secondary"
          fontWeight="medium"
          textTransform="uppercase"
          sx={{ letterSpacing: 0.5 }}
        >
          {title}
        </ArgonTypography>
        <ArgonBox display="flex" alignItems="baseline" gap={1} mt={0.5}>
          <ArgonTypography variant="h4" fontWeight="bold">
            {primary ?? '-'}
          </ArgonTypography>
          <ArgonTypography variant="caption" color="secondary">
            {primaryLabel}
          </ArgonTypography>
        </ArgonBox>
        <Divider sx={{ my: 1 }} />
        <ArgonBox display="flex" flexDirection="column" gap={0.25}>
          {rows.map(({ label, value, color }) => (
            <ArgonBox key={label} display="flex" justifyContent="space-between" alignItems="center">
              <ArgonTypography variant="caption" color="secondary">
                {label}
              </ArgonTypography>
              <ArgonTypography variant="caption" fontWeight="bold" color={color ?? 'text'}>
                {value ?? '-'}
              </ArgonTypography>
            </ArgonBox>
          ))}
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
}

export default SummaryCard;
