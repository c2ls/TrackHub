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

// Argon Dashboard 2 MUI base styles
import colors from "assets/theme/base/colors";
import borders from "assets/theme/base/borders";

// Argon Dashboard 2 MUI helper functions
import pxToRem from "assets/theme/functions/pxToRem";

const { primary, dark, white, transparent } = colors;
const { borderWidth, borderColor } = borders;

// Every dialog lists its buttons as plain text buttons with the confirming action last, so the
// hierarchy is drawn here once: the last button is the primary action, the others are secondary.
const dialogActions = {
  styleOverrides: {
    root: {
      padding: `${pxToRem(12)} ${pxToRem(16)}`,
      gap: pxToRem(8),
      borderTop: `${borderWidth[1]} solid ${borderColor}`,

      "& > :not(style) ~ :not(style)": {
        marginLeft: 0,
      },

      "& > .MuiButton-text": {
        minHeight: pxToRem(36),
        padding: `${pxToRem(6)} ${pxToRem(16)}`,
        border: `${borderWidth[1]} solid ${borderColor}`,
        backgroundColor: transparent.main,
        color: dark.main,

        "&:hover, &:focus": {
          backgroundColor: transparent.main,
          color: dark.main,
        },
      },

      "& > .MuiButton-text:last-of-type": {
        borderColor: primary.main,
        backgroundColor: primary.main,
        color: white.main,

        "&:hover, &:focus": {
          backgroundColor: primary.main,
          color: white.main,
        },
      },
    },
  },
};

export default dialogActions;
