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

import { forwardRef } from "react";
import type { BoxProps } from "@mui/material/Box";
import type { Theme } from "@mui/material/styles";
import { unstable_extendSxProp as extendSxProp } from "@mui/system";
import type { SystemProps } from "@mui/system";

// Custom styles for ArgonBox
import ArgonBoxRoot from "components/ArgonBox/ArgonBoxRoot";

/**
 * Material UI v9 dropped the system shorthands (`mt`, `display`, `width`, …) from
 * Box in favour of `sx`. ArgonBox is this portal's layout primitive and the Argon
 * template uses the shorthand form at several hundred call sites, so the shorthand
 * stays part of ArgonBox's own contract instead: it is declared here and folded into
 * `sx` at render time by the very helper Box itself used to call.
 *
 * The six Argon-specific props below deliberately shadow their system namesakes
 * (`color` takes a palette key like "white", not a CSS colour) — they are
 * destructured off before the fold, so they keep reaching ArgonBoxRoot's ownerState.
 */
export interface ArgonBoxProps
  extends Omit<BoxProps, "color" | "borderRadius">,
    Omit<SystemProps<Theme>, "color" | "borderRadius" | "opacity"> {
  variant?: "contained" | "gradient";
  bgColor?: string;
  color?: string;
  opacity?: number;
  borderRadius?: string;
  shadow?: string;
  /**
   * Image attributes, valid when `component="img"`. Declared on the control
   * (rather than cast at each call site) per the portal's rule 13.
   */
  src?: string;
  alt?: string;
}

/**
 * `extendSxProp` moves every system shorthand it finds into `sx`, but its published
 * signature returns the input type unchanged — narrow the result to what Box accepts.
 */
const foldSystemPropsIntoSx = (props: Omit<ArgonBoxProps, keyof ArgonBoxOwnProps>): BoxProps =>
  extendSxProp(props) as BoxProps;

type ArgonBoxOwnProps = Pick<
  ArgonBoxProps,
  "variant" | "bgColor" | "color" | "opacity" | "borderRadius" | "shadow"
>;

const ArgonBox = forwardRef<HTMLDivElement, ArgonBoxProps>(
  (
    {
      variant = "contained",
      bgColor = "transparent",
      color = "dark",
      opacity = 1,
      borderRadius = "none",
      shadow = "none",
      ...rest
    },
    ref
  ) => (
    <ArgonBoxRoot
      {...foldSystemPropsIntoSx(rest)}
      ref={ref}
      ownerState={{ variant, bgColor, color, opacity, borderRadius, shadow }}
    />
  )
);

export default ArgonBox;
