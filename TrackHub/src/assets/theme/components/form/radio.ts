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
import borders from "assets/theme/base/borders";
import colors from "assets/theme/base/colors";

// Argon Dashboard 2 MUI helper functions
import pxToRem from "assets/theme/functions/pxToRem";
import linearGradient from "assets/theme/functions/linearGradient";

const { borderWidth, borderColor } = borders;
const { transparent, gradients, info } = colors;

const radio = {
  styleOverrides: {
    root: {
      backgroundPosition: "center",
      backgroundSize: "contain",
      backgroundRepeat: "no-repeat",
      width: pxToRem(20),
      height: pxToRem(20),
      marginRight: pxToRem(6),
      padding: 0,
      color: transparent.main,
      border: `${borderWidth[1]} solid ${borderColor}`,
      borderRadius: "50%",
      transition: "all 250ms ease",

      "&:hover": {
        backgroundColor: transparent.main,
      },

      "& .MuiSvgIcon-root": {
        fill: transparent.main,
      },

      "&.Mui-focusVisible": {
        border: `${borderWidth[2]} solid ${info.main} !important`,
      },
    },

    colorPrimary: {
      backgroundColor: transparent.main,

      "&.Mui-checked": {
        backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' height='15px' width='15px'%3E%3Ccircle cx='50%' cy='50%' r='3' fill='%23fff' /%3E%3C/svg%3E"), ${linearGradient(
          gradients.info.main,
          gradients.info.state
        )}`,
        borderColor: gradients.info.state,
      },

      "&:hover": {
        backgroundColor: transparent.main,
      },
    },

    colorSecondary: {
      backgroundColor: transparent.main,

      "&.Mui-checked": {
        backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' height='15px' width='15px'%3E%3Ccircle cx='50%' cy='50%' r='3' fill='%23fff' /%3E%3C/svg%3E"), ${linearGradient(
          gradients.info.main,
          gradients.info.state
        )}`,
        borderColor: gradients.info.state,
      },

      "&:hover": {
        backgroundColor: transparent.main,
      },
    },
  },
};

export default radio;
