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
import Box from "@mui/material/Box";
import { styled } from "@mui/material/styles";

// Dynamic palette color lookups keyed by the runtime `bgColor`/`color` string.
type ArgonColorMap = Record<string, { main: string }>;

export interface ArgonBoxOwnerState {
  variant?: "contained" | "gradient";
  bgColor?: string;
  color?: string;
  opacity?: number | string;
  borderRadius?: string;
  shadow?: string;
}

export default styled(Box)<{ ownerState: ArgonBoxOwnerState }>(({ theme, ownerState }) => {
  const { palette, functions, borders, boxShadows } = theme;
  const { variant, bgColor, color, opacity, borderRadius, shadow } = ownerState;

  const { gradients, grey, white } = palette;
  const { linearGradient } = functions;
  const { borderRadius: radius } = borders;
  const paletteColors = palette as unknown as ArgonColorMap;

  const greyColors: Record<string, string> = {
    "grey-100": grey[100],
    "grey-200": grey[200],
    "grey-300": grey[300],
    "grey-400": grey[400],
    "grey-500": grey[500],
    "grey-600": grey[600],
    "grey-700": grey[700],
    "grey-800": grey[800],
    "grey-900": grey[900],
  };

  const validGradients = [
    "primary",
    "secondary",
    "info",
    "success",
    "warning",
    "error",
    "dark",
    "light",
  ];

  const validColors = [
    "transparent",
    "white",
    "black",
    "primary",
    "secondary",
    "info",
    "success",
    "warning",
    "error",
    "light",
    "dark",
    "text",
    "grey-100",
    "grey-200",
    "grey-300",
    "grey-400",
    "grey-500",
    "grey-600",
    "grey-700",
    "grey-800",
    "grey-900",
  ];

  const validBorderRadius = ["xs", "sm", "md", "lg", "xl", "xxl", "section"];
  const validBoxShadows = ["xs", "sm", "md", "lg", "xl", "xxl", "inset"];

  // background value
  let backgroundValue = bgColor;

  if (variant === "gradient") {
    backgroundValue = validGradients.find((el) => el === bgColor)
      ? linearGradient(gradients[bgColor!].main, gradients[bgColor!].state)
      : white.main;
  } else if (validColors.find((el) => el === bgColor)) {
    backgroundValue = paletteColors[bgColor!] ? paletteColors[bgColor!].main : greyColors[bgColor!];
  } else {
    backgroundValue = bgColor;
  }

  // color value
  let colorValue = color;

  if (validColors.find((el) => el === color)) {
    colorValue = paletteColors[color!] ? paletteColors[color!].main : greyColors[color!];
  }

  // borderRadius value
  let borderRadiusValue = borderRadius;

  if (validBorderRadius.find((el) => el === borderRadius)) {
    borderRadiusValue = radius[borderRadius!];
  }

  // boxShadow value
  const argonBoxShadows = boxShadows as unknown as Record<string, string>;
  let boxShadowValue: string | Record<string, string> = argonBoxShadows;

  if (validBoxShadows.find((el) => el === shadow)) {
    boxShadowValue = argonBoxShadows[shadow!];
  }

  return {
    opacity,
    background: backgroundValue,
    color: colorValue,
    borderRadius: borderRadiusValue,
    // Preserves the vendored quirk: an unrecognized `shadow` (e.g. the "none" default)
    // leaves the whole boxShadows object here — runtime behavior kept byte-for-byte.
    boxShadow: boxShadowValue as unknown as string,
  };
});
