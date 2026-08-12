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

/**
=========================================================
* Argon Dashboard 2 MUI - v3.0.1
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-material-ui
* Copyright 2023 Creative Tim (https://www.creative-tim.com)

Coded by www.creative-tim.com

 =========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*/

// @mui material components
import { styled } from "@mui/material/styles";
import LinearProgress from "@mui/material/LinearProgress";

// Dynamic palette color lookups keyed by the runtime `color` string.
type ArgonColorMap = Record<string, { main: string }>;

export interface ArgonProgressOwnerState {
  color?: string;
  value?: number;
  variant?: "contained" | "gradient";
}

export default styled(LinearProgress)<{ ownerState: ArgonProgressOwnerState }>(
  ({ theme, ownerState }) => {
    const { palette, functions } = theme;
    const { color, value, variant } = ownerState;

    const { text, gradients } = palette;
    const { linearGradient } = functions;
    const paletteColors = palette as unknown as ArgonColorMap;

    // background value
    let backgroundValue;

    if (variant === "gradient") {
      backgroundValue = color && gradients[color]
        ? linearGradient(gradients[color].main, gradients[color].state)
        : linearGradient(gradients.info.main, gradients.info.state);
    } else {
      backgroundValue = color && paletteColors[color] ? paletteColors[color].main : paletteColors.info.main;
    }

    return {
      "& .MuiLinearProgress-bar": {
        background: backgroundValue,
        width: `${value}%`,
        color: text.main,
      },
    };
  }
);
