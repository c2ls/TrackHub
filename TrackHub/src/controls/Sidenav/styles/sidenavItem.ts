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

/* eslint-disable no-nested-ternary */
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

import type { Theme } from "@mui/material/styles";

interface SidenavItemOwnerState {
  active?: boolean;
  darkSidenav?: boolean;
  miniSidenav?: boolean;
}

function item(theme: Theme, ownerState: SidenavItemOwnerState) {
  const { palette, transitions, breakpoints, boxShadows, borders, functions } = theme;
  const { active, darkSidenav, miniSidenav } = ownerState;

  const { dark, transparent, white } = palette;
  const { xxl } = boxShadows;
  const { borderRadius } = borders;
  const { pxToRem, rgba } = functions;
  // Surfaces Argon's `text.main` shade, which MUI's TypeText omits.
  const paletteColors = palette as unknown as Record<string, { main: string }>;
  const text = paletteColors.text;

  return {
    background: active
      ? rgba(palette.info.main, 0.1)
      : transparent.main,
    color: () => {
      let result = text.main;

      if (darkSidenav) {
        result = white.main;
      } else if (active) {
        result = dark.main;
      }

      return result;
    },
    display: miniSidenav ? "block" : "flex",
    alignItems: "center",
    width: "100%",
    padding: `${pxToRem(10.8)} ${pxToRem(16)} ${pxToRem(10.8)} ${pxToRem(16)}`,
    margin: `0 ${pxToRem(8)}`,
    borderRadius: borderRadius.md,
    cursor: "pointer",
    userSelect: "none",
    whiteSpace: "nowrap",
    boxShadow: active && darkSidenav ? xxl : "none",

    [breakpoints.up("xl")]: {
      boxShadow: () => {
        if (active) {
          return darkSidenav ? xxl : "none";
        }

        return "none";
      },
      transition: transitions.create("box-shadow", {
        easing: transitions.easing.easeInOut,
        duration: transitions.duration.shorter,
      }),
    },
  };
}

function itemIconBox(theme: Theme, ownerState: SidenavItemOwnerState) {
  const { transitions, borders, functions } = theme;
  const { darkSidenav, active } = ownerState;

  const { borderRadius } = borders;
  const { pxToRem } = functions;

  return {
    color: "inherit",
    minWidth: pxToRem(32),
    minHeight: pxToRem(32),
    borderRadius: borderRadius.md,
    display: "grid",
    placeItems: "center",
    transition: transitions.create("margin", {
      easing: transitions.easing.easeInOut,
      duration: transitions.duration.standard,
    }),

    "& svg, svg g": {
      fill: "currentColor",
    },

    "& i": {
      color: active && darkSidenav ? "inherit" : null,
    },
  };
}

const itemIcon = ({ palette: { white, gradients } }: Theme, { active }: SidenavItemOwnerState) => ({
  color: active ? white.main : gradients.dark.state,
});

function itemText(theme: Theme, ownerState: SidenavItemOwnerState) {
  const { typography, transitions, breakpoints, functions } = theme;
  const { miniSidenav, active } = ownerState;

  const { fontWeightMedium, fontWeightRegular, size } = typography;
  const { pxToRem } = functions;

  return {
    color: "inherit",
    marginLeft: pxToRem(4),

    [breakpoints.up("xl")]: {
      opacity: miniSidenav ? 0 : 1,
      maxWidth: miniSidenav ? 0 : "100%",
      marginLeft: miniSidenav ? 0 : pxToRem(4),
      transition: transitions.create(["opacity", "margin"], {
        easing: transitions.easing.easeInOut,
        duration: transitions.duration.standard,
      }),
    },

    "& span": {
      color: "inherit",
      fontWeight: active ? fontWeightMedium : fontWeightRegular,
      fontSize: size.sm,
      lineHeight: 0,
    },
  };
}

export { item, itemIconBox, itemIcon, itemText };
