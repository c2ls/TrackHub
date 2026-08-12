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

import { forwardRef, createContext, useContext, useMemo } from "react";
import type { ReactNode } from "react";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import type { ArgonButtonProps } from "components/ArgonButton";

// Custom styles for ArgonPagination
import ArgonPaginationItemRoot from "components/ArgonPagination/ArgonPaginationItemRoot";

type PaginationColor = NonNullable<ArgonButtonProps["color"]>;

interface PaginationContextValue {
  variant: "gradient" | "contained";
  color: PaginationColor;
  size: "small" | "medium" | "large";
}

// The Pagination main context
const Context = createContext<PaginationContextValue | null>(null);

export interface ArgonPaginationProps
  extends Omit<ArgonButtonProps, "color" | "variant" | "size" | "children"> {
  item?: boolean;
  variant?: "gradient" | "contained";
  color?: PaginationColor;
  size?: "small" | "medium" | "large";
  active?: boolean;
  children: ReactNode;
}

const ArgonPagination = forwardRef<HTMLButtonElement, ArgonPaginationProps>(
  (
    {
      item = false,
      variant = "gradient",
      color = "info",
      size = "medium",
      active = false,
      children,
      ...rest
    },
    ref
  ) => {
    const context = useContext(Context);
    const paginationSize = context ? context.size : null;
    const value = useMemo<PaginationContextValue>(
      () => ({ variant, color, size }),
      [variant, color, size]
    );

    return (
      <Context.Provider value={value}>
        {item ? (
          <ArgonPaginationItemRoot
            {...rest}
            ref={ref}
            variant={active ? context!.variant : "outlined"}
            color={active ? context!.color : "secondary"}
            iconOnly
            circular
            ownerState={{ variant, active, paginationSize }}
          >
            {children}
          </ArgonPaginationItemRoot>
        ) : (
          <ArgonBox
            display="flex"
            justifyContent="flex-end"
            alignItems="center"
            sx={{ listStyle: "none" }}
          >
            {children}
          </ArgonBox>
        )}
      </Context.Provider>
    );
  }
);

export default ArgonPagination;
