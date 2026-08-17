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
* Argon Dashboard 2 MUI - v1.0.0
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-material-ui
* Copyright 2021 Creative Tim (https://www.creative-tim.com)

Coded by www.creative-tim.com

=========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*/

// @mui material components
import Button from "@mui/material/Button";
import { styled } from "@mui/material/styles";

// Dynamic palette color lookups keyed by the runtime `color` string.
type ArgonColorMap = Record<string, { main: string; focus: string }>;

export interface ArgonButtonOwnerState {
  color?: string;
  variant?: "text" | "contained" | "outlined" | "gradient";
  size?: "xsmall" | "small" | "medium" | "large";
  circular?: boolean;
  iconOnly?: boolean;
}

export default styled(Button)<{ ownerState: ArgonButtonOwnerState }>(({ theme, ownerState }) => {
  const { palette, functions, borders } = theme;
  const { color, variant, size, circular, iconOnly } = ownerState;

  const { white, dark, text, transparent, gradients } = palette;
  const { boxShadow, linearGradient, pxToRem, rgba } = functions;
  const { borderRadius } = borders;
  const paletteColors = palette as unknown as ArgonColorMap;

  // styles for the button with variant="contained"
  const containedStyles = () => {
    // background color value
    const backgroundValue = paletteColors[color!] ? paletteColors[color!].main : white.main;

    // backgroundColor value when button is focused
    const focusedBackgroundValue = paletteColors[color!] ? paletteColors[color!].focus : white.focus;

    // boxShadow value
    const boxShadowValue = paletteColors[color!]
      ? boxShadow([0, 0], [0, 3.2], paletteColors[color!].main, 0.5)
      : boxShadow([0, 0], [0, 3.2], dark.main, 0.5);

    // color value
    let colorValue = white.main;

    if (color === "white" || !paletteColors[color!]) {
      colorValue = gradients.dark.main;
    } else if (color === "light") {
      colorValue = gradients.dark.state;
    }

    // color value when button is focused
    let focusedColorValue = white.main;

    if (color === "white") {
      focusedColorValue = gradients.dark.main;
    } else if (color === "primary" || color === "error" || color === "dark") {
      focusedColorValue = white.main;
    }

    return {
      background: backgroundValue,
      color: colorValue,

      "&:hover": {
        backgroundColor: backgroundValue,
      },

      "&:focus:not(:hover)": {
        backgroundColor: focusedBackgroundValue,
        boxShadow: boxShadowValue,
      },

      "&:disabled": {
        backgroundColor: backgroundValue,
        color: focusedColorValue,
      },
    };
  };

  // styles for the button with variant="outlined"
  const outliedStyles = () => {
    // background color value
    const backgroundValue = color === "white" ? rgba(white.main, 0.1) : transparent.main;

    // color value
    const colorValue = paletteColors[color!] ? paletteColors[color!].main : white.main;

    // boxShadow value
    const boxShadowValue = paletteColors[color!]
      ? boxShadow([0, 0], [0, 3.2], paletteColors[color!].main, 0.5)
      : boxShadow([0, 0], [0, 3.2], white.main, 0.5);

    // border color value
    let borderColorValue = paletteColors[color!] ? paletteColors[color!].main : rgba(white.main, 0.75);

    if (color === "white") {
      borderColorValue = rgba(white.main, 0.75);
    }

    return {
      background: backgroundValue,
      color: colorValue,
      borderColor: borderColorValue,

      "&:hover": {
        background: transparent.main,
        borderColor: colorValue,
      },

      "&:focus:not(:hover)": {
        background: transparent.main,
        boxShadow: boxShadowValue,
      },

      "&:active:not(:hover)": {
        backgroundColor: colorValue,
        color: white.main,
        opacity: 0.85,
      },

      "&:disabled": {
        color: colorValue,
        borderColor: colorValue,
      },
    };
  };

  // styles for the button with variant="gradient"
  const gradientStyles = () => {
    // background value
    const backgroundValue =
      color === "white" || !gradients[color!]
        ? white.main
        : linearGradient(gradients[color!].main, gradients[color!].state);

    // color value
    let colorValue = white.main;

    if (color === "white") {
      colorValue = gradients.dark.main;
    } else if (color === "light") {
      colorValue = gradients.dark.state;
    }

    return {
      background: backgroundValue,
      color: colorValue,

      "&:focus:not(:hover)": {
        boxShadow: "none",
      },

      "&:disabled": {
        background: backgroundValue,
        color: colorValue,
      },
    };
  };

  // styles for the button with variant="text"
  const textStyles = () => {
    // color value
    const colorValue = paletteColors[color!] ? paletteColors[color!].main : white.main;

    // color value when button is focused
    const focusedColorValue = paletteColors[color!] ? paletteColors[color!].focus : white.focus;

    return {
      color: colorValue,

      "&:hover": {
        color: focusedColorValue,
      },

      "&:focus:not(:hover)": {
        color: focusedColorValue,
      },
    };
  };

  // styles for the button with circular={true}
  const circularStyles = () => ({
    borderRadius: borderRadius.section,
  });

  // styles for the button with iconOnly={true}
  const iconOnlyStyles = () => {
    // width, height, minWidth and minHeight values
    let sizeValue = pxToRem(38);

    if (size === "small") {
      sizeValue = pxToRem(25.4);
    } else if (size === "xsmall") {
      sizeValue = pxToRem(12.7);
    }
    else if (size === "large") {
      sizeValue = pxToRem(52);
    }

    // padding value
    let paddingValue = `${pxToRem(11)} ${pxToRem(11)} ${pxToRem(10)}`;

    if (size === "small") {
      paddingValue = pxToRem(4.5);
    } else if (size === "xsmall") {
      paddingValue = pxToRem(2);
    } else if (size === "large") {
      paddingValue = pxToRem(16);
    }

    return {
      width: sizeValue,
      minWidth: sizeValue,
      height: sizeValue,
      minHeight: sizeValue,
      padding: paddingValue,

      "& .material-icons": {
        marginTop: 0,
      },

      "&:hover, &:focus, &:active": {
        transform: "none",
      },
    };
  };

  return {
    ...(variant === "contained" && containedStyles()),
    ...(variant === "outlined" && outliedStyles()),
    ...(variant === "gradient" && gradientStyles()),
    ...(variant === "text" && textStyles()),
    ...(circular && circularStyles()),
    ...(iconOnly && iconOnlyStyles()),
  };
});
