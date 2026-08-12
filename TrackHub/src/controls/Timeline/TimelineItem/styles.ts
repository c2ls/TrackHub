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

import type { Theme, CSSObject } from "@mui/material/styles";

interface TimelineItemOwnerState {
  lastItem?: boolean;
}

function timelineItem(theme: Theme, ownerState: TimelineItemOwnerState): CSSObject {
  const { borders } = theme;
  const { lastItem } = ownerState;

  const { borderWidth, borderColor } = borders;

  return {
    "&:after": {
      content: !lastItem ? "''" : undefined,
      position: "absolute",
      top: "10%",
      left: "14px",
      height: "100%",
      borderRight: `${borderWidth[2]} solid ${borderColor}`,
    },
  };
}

interface TimelineItemIconOwnerState {
  color: string;
}

function timelineItemIcon(theme: Theme, ownerState: TimelineItemIconOwnerState): CSSObject {
  const { palette, functions } = theme;
  const { color } = ownerState;

  const { gradients, transparent } = palette;
  const { size } = theme.typography;
  const { linearGradient } = functions;

  return {
    position: "absolute",
    top: "50%",
    left: "50%",
    transform: "translate(-52%, -50%)",
    fontSize: size.xl,
    fontWeight: theme.typography.fontWeightMedium,
    zIndex: 1,
    backgroundImage: linearGradient(gradients[color].main, gradients[color].state),
    WebkitBackgroundClip: "text",
    WebkitTextFillColor: transparent.main,
  };
}

export { timelineItem, timelineItemIcon };
