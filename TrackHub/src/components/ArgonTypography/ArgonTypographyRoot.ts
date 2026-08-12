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
import Typography from "@mui/material/Typography";
import { styled } from "@mui/material/styles";
import type { CSSObject } from "@mui/material/styles";

// Dynamic palette color lookups keyed by the runtime `color` string.
type ArgonColorMap = Record<string, { main: string }>;

export type ArgonFontWeight = false | "light" | "regular" | "medium" | "bold";

export interface ArgonTypographyOwnerState {
  color?: string;
  textTransform?: "none" | "capitalize" | "uppercase" | "lowercase";
  verticalAlign?:
    | "unset"
    | "baseline"
    | "sub"
    | "super"
    | "text-top"
    | "text-bottom"
    | "middle"
    | "top"
    | "bottom";
  fontWeight?: ArgonFontWeight;
  opacity?: number;
  textGradient?: boolean;
  darkMode?: boolean;
}

export default styled(Typography)<{ ownerState: ArgonTypographyOwnerState }>(
  ({ theme, ownerState }): CSSObject => {
    const { palette, typography, functions } = theme;
    const { color, textTransform, verticalAlign, fontWeight, opacity, textGradient, darkMode } =
      ownerState;

    const { gradients, transparent, white } = palette;
    const { fontWeightLight, fontWeightRegular, fontWeightMedium, fontWeightBold } = typography;
    const { linearGradient } = functions;
    const paletteColors = palette as unknown as ArgonColorMap;

    // fontWeight styles
    const fontWeights: Record<string, number | string | undefined> = {
      light: fontWeightLight,
      regular: fontWeightRegular,
      medium: fontWeightMedium,
      bold: fontWeightBold,
    };

    // styles for the typography with textGradient={true}
    const gradientStyles = (): CSSObject => ({
      backgroundImage:
        color !== "inherit" && color !== "text" && color !== "white" && color && gradients[color]
          ? linearGradient(gradients[color].main, gradients[color].state)
          : linearGradient(gradients.dark.main, gradients.dark.state),
      display: "inline-block",
      WebkitBackgroundClip: "text",
      WebkitTextFillColor: transparent.main,
      position: "relative",
      zIndex: 1,
    });

    // color value
    let colorValue =
      color === "inherit" || !paletteColors[color!] ? "inherit" : paletteColors[color!].main;

    if (darkMode && (color === "inherit" || !paletteColors[color!])) {
      colorValue = "inherit";
    } else if (darkMode && color === "dark") colorValue = white.main;

    return {
      opacity,
      textTransform,
      verticalAlign,
      textDecoration: "none",
      color: colorValue,
      fontWeight: fontWeight ? fontWeights[fontWeight] : undefined,
      ...(textGradient && gradientStyles()),
    };
  }
);
