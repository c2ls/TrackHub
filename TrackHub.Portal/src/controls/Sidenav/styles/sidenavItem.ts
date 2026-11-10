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
  miniSidenav?: boolean;
}

function item(theme: Theme, ownerState: SidenavItemOwnerState) {
  const { palette, borders, functions } = theme;
  const { active, miniSidenav } = ownerState;

  const { borderRadius } = borders;
  const { pxToRem, rgba } = functions;
  // Surfaces Argon's `text.main` shade, which MUI's TypeText omits.
  const text = (palette as unknown as Record<string, { main: string }>).text;

  return {
    background: active ? rgba(palette.info.main, 0.12) : palette.transparent.main,
    color: active ? palette.info.main : text.main,
    display: "flex",
    alignItems: "center",
    justifyContent: miniSidenav ? "center" : "flex-start",
    width: "100%",
    minHeight: pxToRem(38),
    padding: `0 ${pxToRem(10)}`,
    margin: `${pxToRem(1)} ${pxToRem(12)}`,
    borderRadius: borderRadius.md,
    cursor: "pointer",
    userSelect: "none",
    whiteSpace: "nowrap",

    "&:hover": {
      background: active ? rgba(palette.info.main, 0.12) : palette.grey[100],
      color: active ? palette.info.main : palette.dark.main,
    },
  };
}

function itemIconBox(theme: Theme) {
  const { pxToRem } = theme.functions;

  return {
    color: "inherit",
    minWidth: pxToRem(28),
    display: "grid",
    placeItems: "center",
  };
}

const itemIcon = ({ functions: { pxToRem } }: Theme) => ({
  color: "inherit",
  fontSize: `${pxToRem(20)} !important`,
});

function itemText(theme: Theme, ownerState: SidenavItemOwnerState) {
  const { typography, breakpoints, functions } = theme;
  const { miniSidenav, active } = ownerState;

  const { fontWeightBold, fontWeightMedium, size } = typography;
  const { pxToRem } = functions;

  return {
    color: "inherit",
    marginLeft: pxToRem(6),

    [breakpoints.up("xl")]: {
      display: miniSidenav ? "none" : "block",
    },

    "& span": {
      color: "inherit",
      fontWeight: active ? fontWeightBold : fontWeightMedium,
      fontSize: size.sm,
      lineHeight: 1.4,
    },
  };
}

export { item, itemIconBox, itemIcon, itemText };
