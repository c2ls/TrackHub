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

import type { Theme } from "@mui/material/styles";

// The bar spans the layout's full width, so it cancels the layout's side padding.
const navbar = ({ palette, borders, functions: { pxToRem }, zIndex }: Theme) => ({
  position: "sticky",
  top: 0,
  zIndex: zIndex.appBar,
  display: "flex",
  alignItems: "center",
  flexWrap: "wrap",
  columnGap: pxToRem(12),
  minHeight: pxToRem(56),
  mx: -3,
  px: 3,
  mb: 3,
  backgroundColor: palette.background.paper,
  borderBottom: `${borders.borderWidth[1]} solid ${borders.borderColor}`,
});

const navbarIconButton = ({ palette, borders, functions: { pxToRem } }: Theme) => ({
  width: pxToRem(36),
  height: pxToRem(36),
  borderRadius: borders.borderRadius.md,
  color: palette.text.main,

  "&:hover": {
    backgroundColor: palette.grey[100],
    color: palette.dark.main,
  },

  "& .material-icons, .material-icons-round": {
    fontSize: `${pxToRem(20)} !important`,
  },
});

export { navbar, navbarIconButton };
