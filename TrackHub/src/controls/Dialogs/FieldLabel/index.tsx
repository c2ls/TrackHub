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

import type { ComponentType, ReactNode } from 'react';
import ArgonTypography from 'components/ArgonTypography';
import type { ArgonTypographyProps } from 'components/ArgonTypography';

interface FieldLabelProps {
  htmlFor?: string;
  required?: boolean;
  children: ReactNode;
}

// ArgonTypography renders as a <label> here; augment its props with the
// label-only `htmlFor` attribute (absent from the default span typing).
const LabelTypography = ArgonTypography as ComponentType<ArgonTypographyProps & { htmlFor?: string }>;

const FieldLabel = ({ htmlFor, required = false, children }: FieldLabelProps) => (
  <LabelTypography
    component="label"
    htmlFor={htmlFor}
    variant="caption"
    fontWeight="bold"
    color="dark"
    sx={{ display: 'block', mb: 0.5, lineHeight: 1.2 }}
  >
    {children}
    {required && <span aria-hidden="true" style={{ marginLeft: 2 }}>*</span>}
  </LabelTypography>
);

export default FieldLabel;
