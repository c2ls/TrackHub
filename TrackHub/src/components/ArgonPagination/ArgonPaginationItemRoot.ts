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

// Argon Dashboard 2 MUI components
import ArgonButton from "components/ArgonButton";

export interface ArgonPaginationItemOwnerState {
  variant?: "gradient" | "contained" | "outlined";
  paginationSize?: "small" | "medium" | "large" | null;
  active?: boolean;
}

export default styled(ArgonButton)<{ ownerState: ArgonPaginationItemOwnerState }>(
  ({ theme, ownerState }) => {
    const { borders, functions, typography, palette } = theme;
    const { variant, paginationSize, active } = ownerState;

    const { borderColor } = borders;
    const { pxToRem } = functions;
    const { fontWeightRegular, size: fontSize } = typography;
    const { light } = palette;

    // width, height, minWidth and minHeight values
    let sizeValue = pxToRem(36);

    if (paginationSize === "small") {
      sizeValue = pxToRem(30);
    } else if (paginationSize === "large") {
      sizeValue = pxToRem(46);
    }

    return {
      borderColor,
      margin: `0 ${pxToRem(2)}`,
      pointerEvents: active ? "none" : "auto",
      fontWeight: fontWeightRegular,
      fontSize: fontSize.sm,
      width: sizeValue,
      minWidth: sizeValue,
      height: sizeValue,
      minHeight: sizeValue,

      "&:hover, &:focus, &:active": {
        transform: "none",
        // Vendored source condition `variant !== "gradient" || variant !== "contained"` is a
        // tautology (the operands are `||`-ed) that always evaluates to "none !important";
        // inlined verbatim to preserve runtime output byte-for-byte.
        boxShadow: "none !important",
        opacity: "1 !important",
      },

      "&:hover": {
        backgroundColor: light.main,
        borderColor,
      },
    };
  }
);
