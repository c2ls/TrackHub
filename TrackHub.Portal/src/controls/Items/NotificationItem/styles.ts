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

import type { Theme } from "@mui/material/styles";

function menuItem(theme: Theme) {
  const { palette, borders, transitions } = theme;

  const { secondary, light } = palette;
  const { borderRadius } = borders;

  return {
    display: "flex",
    alignItems: "center",
    width: "100%",
    color: secondary.main,
    py: 1,
    px: 2,
    borderRadius: borderRadius.md,
    transition: transitions.create("background-color", {
      easing: transitions.easing.easeInOut,
      duration: transitions.duration.standard,
    }),

    "&:not(:last-child)": {
      mb: 1.25,
    },

    "&:hover": {
      backgroundColor: light.main,
    },
  };
}

interface MenuImageOwnerState {
  color: string;
}

function menuImage(theme: Theme, ownerState: MenuImageOwnerState) {
  const { functions, palette, borders } = theme;
  const { color } = ownerState;

  const { linearGradient } = functions;
  const { gradients } = palette;
  const { borderRadius } = borders;

  return {
    display: "grid",
    placeItems: "center",
    backgroundImage: gradients[color]
      ? linearGradient(gradients[color].main, gradients[color].state)
      : linearGradient(gradients.dark.main, gradients.dark.state),

    "& img": {
      width: "100%",
      borderRadius: borderRadius.lg,
    },
  };
}

export { menuItem, menuImage };
