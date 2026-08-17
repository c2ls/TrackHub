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

import type { ReactNode } from "react";

// @mui material components
import Badge from "@mui/material/Badge";
import { styled } from "@mui/material/styles";

export interface ArgonBadgeOwnerState {
  color?: string;
  variant?: "gradient" | "contained";
  size?: "xs" | "sm" | "md" | "lg";
  circular?: boolean;
  indicator?: boolean;
  border?: boolean;
  container?: boolean;
  children?: ReactNode;
}

export default styled(Badge)<{ ownerState: ArgonBadgeOwnerState }>(({ theme, ownerState }) => {
  const { palette, typography, borders, functions } = theme;
  const { color, circular, border, size, indicator, variant, container, children } = ownerState;

  const { white, dark, gradients, badgeColors } = palette;
  const { size: fontSize, fontWeightBold } = typography;
  const { borderRadius, borderWidth } = borders;
  const { pxToRem, linearGradient } = functions;

  // padding values
  const paddings: Record<string, string> = {
    xs: "0.45em 0.775em",
    sm: "0.55em 0.9em",
    md: "0.65em 1em",
    lg: "0.85em 1.375em",
  };

  // fontSize value
  const fontSizeValue = size === "xs" ? fontSize.xxs : fontSize.xs;

  // border value
  const borderValue = border ? `${borderWidth[3]} solid ${white.main}` : "none";

  // borderRadius value
  let borderRadiusValue = size === "xs" ? borderRadius.sm : borderRadius.md;

  if (circular) {
    borderRadiusValue = borderRadius.section;
  }

  // styles for the badge with indicator={true}
  const indicatorStyles = (sizeProp?: string) => {
    let widthValue = pxToRem(20);
    let heightValue = pxToRem(20);

    if (sizeProp === "medium") {
      widthValue = pxToRem(24);
      heightValue = pxToRem(24);
    } else if (sizeProp === "large") {
      widthValue = pxToRem(32);
      heightValue = pxToRem(32);
    }

    return {
      width: widthValue,
      height: heightValue,
      display: "grid",
      placeItems: "center",
      textAlign: "center",
      borderRadius: "50%",
      padding: 0,
      border: borderValue,
    };
  };

  // styles for the badge with variant="gradient"
  const gradientStyles = (colorProp?: string) => {
    const backgroundValue = colorProp && gradients[colorProp]
      ? linearGradient(gradients[colorProp].main, gradients[colorProp].state)
      : linearGradient(gradients.info.main, gradients.info.state);
    const colorValue = colorProp === "light" ? dark.main : white.main;

    return {
      background: backgroundValue,
      color: colorValue,
    };
  };

  // styles for the badge with variant="contained"
  const containedStyles = (colorProp?: string) => {
    const backgroundValue = colorProp && badgeColors[colorProp]
      ? badgeColors[colorProp].background
      : badgeColors.info.background;
    let colorValue = colorProp && badgeColors[colorProp] ? badgeColors[colorProp].text : badgeColors.info.text;

    if (colorProp === "light") {
      colorValue = dark.main;
    }
    return {
      background: backgroundValue,
      color: colorValue,
    };
  };

  // styles for the badge with no children and container={false}
  const standAloneStyles = () => ({
    position: "static",
    marginLeft: pxToRem(8),
    transform: "none",
    fontSize: pxToRem(9),
  });

  // styles for the badge with container={true}
  const containerStyles = () => ({
    position: "relative",
    transform: "none",
  });

  return {
    "& .MuiBadge-badge": {
      height: "auto",
      padding: paddings[size ?? "xs"] || paddings.xs,
      fontSize: fontSizeValue,
      fontWeight: fontWeightBold,
      textTransform: "uppercase",
      lineHeight: 1,
      textAlign: "center",
      whiteSpace: "nowrap",
      verticalAlign: "baseline",
      border: borderValue,
      borderRadius: borderRadiusValue,
      ...(indicator && indicatorStyles(size)),
      ...(variant === "gradient" && gradientStyles(color)),
      ...(variant === "contained" && containedStyles(color)),
      ...(!children && !container && standAloneStyles()),
      ...(container && containerStyles()),
    },
  };
});
