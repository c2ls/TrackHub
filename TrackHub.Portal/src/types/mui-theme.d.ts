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
 * Module augmentation for the vendored Argon Dashboard 2 MUI theme.
 *
 * Argon extends MUI's `Theme` with three extra branches (`boxShadows`, `borders`,
 * `functions`) and layers a large set of custom color groups onto the palette. These
 * shapes are derived from `assets/theme/base/colors`, `.../base/boxShadows`,
 * `.../base/borders`, `.../functions/*` and the `createTheme(...)` call in
 * `assets/theme/index`. Both the light (`theme/`) and dark (`theme-dark/`) trees share
 * this shape and differ only in values.
 */

import "@mui/material/styles";

declare module "@mui/material/styles" {
  // ---- Argon helper functions surfaced on the theme (theme.functions.*) ----------------
  interface ArgonThemeFunctions {
    boxShadow: (
      offset: number[],
      radius: number[],
      color: string,
      opacity: number,
      inset?: string
    ) => string;
    hexToRgb: (color: string) => string;
    linearGradient: (color: string, colorState: string, angle?: number) => string;
    pxToRem: (px: number, baseNumber?: number) => string;
    rgba: (color: string, opacity: number) => string;
  }

  // ---- theme.boxShadows.* (base/boxShadows) --------------------------------------------
  interface ArgonBoxShadows {
    xs: string;
    sm: string;
    md: string;
    lg: string;
    xl: string;
    xxl: string;
    inset: string;
    navbarBoxShadow: string;
    cardBoxShadow: string;
    buttonBoxShadow: { main: string; stateOf: string; stateOfNotHover: string };
    inputBoxShadow: string;
    sliderBoxShadow: { thumb: string };
    tabsBoxShadow: { indicator: string };
  }

  // ---- theme.borders.* (base/borders) --------------------------------------------------
  interface ArgonBorders {
    borderColor: string;
    borderWidth: Record<string, string | number>;
    borderRadius: Record<string, string>;
  }

  // ---- Argon custom palette color groups ------------------------------------------------
  type ArgonGradient = { main: string; state: string };
  type ArgonSocialColor = { main: string; dark: string };
  type ArgonAlertColor = { main: string; state: string; border: string };
  type ArgonBadgeColor = { background: string; text: string };

  interface Theme {
    boxShadows: ArgonBoxShadows;
    borders: ArgonBorders;
    functions: ArgonThemeFunctions;
  }

  // ---- Argon custom typography fields (theme.typography.*) ------------------------------
  // Argon layers a `size` scale and a `fontSizeRegular` alias onto MUI's typography.
  interface TypographyVariants {
    size: Record<string, string>;
    fontSizeRegular: string | number;
  }
  interface TypographyVariantsOptions {
    size?: Record<string, string>;
    fontSizeRegular?: string | number;
  }

  interface ThemeOptions {
    boxShadows?: ArgonBoxShadows;
    borders?: ArgonBorders;
    functions?: ArgonThemeFunctions;
  }

  interface Palette {
    transparent: { main: string };
    white: { main: string; focus: string };
    black: { light: string; main: string; focus: string };
    light: { main: string; focus: string };
    dark: { main: string; focus: string };
    gradients: Record<string, ArgonGradient>;
    socialMediaColors: Record<string, ArgonSocialColor>;
    alertColors: Record<string, ArgonAlertColor>;
    badgeColors: Record<string, ArgonBadgeColor>;
    inputColors: {
      borderColor: { main: string; focus: string };
      error: string;
      success: string;
    };
    sliderColors: { thumb: { borderColor: string } };
    circleSliderColors: { background: string };
    tabs: { indicator: { boxShadow: string } };
  }

  interface PaletteOptions {
    transparent?: { main: string };
    white?: { main: string; focus: string };
    black?: { light: string; main: string; focus: string };
    light?: { main: string; focus: string };
    dark?: { main: string; focus: string };
    gradients?: Record<string, ArgonGradient>;
    socialMediaColors?: Record<string, ArgonSocialColor>;
    alertColors?: Record<string, ArgonAlertColor>;
    badgeColors?: Record<string, ArgonBadgeColor>;
    inputColors?: {
      borderColor: { main: string; focus: string };
      error: string;
      success: string;
    };
    sliderColors?: { thumb: { borderColor: string } };
    circleSliderColors?: { background: string };
    tabs?: { indicator: { boxShadow: string } };
  }

  // Argon models `palette.text` as a color group with `main`/`focus` shades (used by the
  // primitives) in addition to MUI's standard `primary`/`secondary`/`disabled`.
  interface TypeText {
    main: string;
    focus: string;
  }

  // Argon adds a `focus` shade to the standard palette colors (primary.focus, info.focus…).
  interface PaletteColor {
    focus?: string;
  }
  interface SimplePaletteColorOptions {
    focus?: string;
  }
}
