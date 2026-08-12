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

import { useState } from "react";
import type { ReactNode } from "react";

// react-router components
import { Link } from "react-router-dom";

// @mui material components
import Collapse from "@mui/material/Collapse";
import MuiLink from "@mui/material/Link";
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// Argon Dashboard 2 MUI controlss
import DefaultNavbarDropdown from "controls/Navbars/DefaultNavbar/DefaultNavbarDropdown";

// Argon Dashboard 2 PRO React contexts
import { useArgonController } from "context";

export interface NavbarMobileRoute {
  name?: string;
  key?: string;
  icon?: ReactNode;
  href?: string;
  route?: string;
  description?: ReactNode;
  collapse?: NavbarMobileRoute[];
}

export interface DefaultNavbarMobileProps {
  routes?: NavbarMobileRoute[];
  open?: boolean | object | null;
  close?: () => void;
}

function DefaultNavbarMobile({ routes = [], open }: DefaultNavbarMobileProps) {
  const [collapse, setCollapse] = useState<string | boolean>("");
  const [controller] = useArgonController();
  const { darkMode } = controller;

  const handleSetCollapse = (name: string | undefined) =>
    collapse === name ? setCollapse(false) : setCollapse(name ?? "");

  const renderNavbarItems = routes.map(
    ({ name, icon, collapse: routeCollapses, href, route, collapse: navCollapse }) => (
      <DefaultNavbarDropdown
        key={name}
        name={name ?? ""}
        icon={icon}
        collapseStatus={name === collapse}
        onClick={() => handleSetCollapse(name)}
        href={href}
        route={route}
        collapse={Boolean(navCollapse)}
      >
        <ArgonBox sx={{ height: "15rem", maxHeight: "15rem", overflowY: "scroll" }}>
          {routeCollapses &&
            routeCollapses.map((item) => (
              <ArgonBox key={item.name} px={item.icon ? 1 : 2}>
                {item.collapse ? (
                  <>
                    <ArgonBox width="100%" display="flex" alignItems="center" p={1}>
                      {item.icon && (
                        <ArgonBox
                          display="flex"
                          justifyContent="center"
                          alignItems="center"
                          width="1.5rem"
                          height="1.5rem"
                          borderRadius="md"
                          color="text"
                          mr={1}
                          fontSize="1rem"
                          lineHeight={1}
                        >
                          {typeof item.icon === "string" ? <Icon>{item.icon}</Icon> : item.icon}
                        </ArgonBox>
                      )}
                      <ArgonTypography
                        display="block"
                        variant="button"
                        fontWeight="bold"
                        textTransform="capitalize"
                      >
                        {item.name}
                      </ArgonTypography>
                    </ArgonBox>
                    {item.collapse.map((el, index) => {
                      const elLinkProps = {
                        component: el.route ? Link : MuiLink,
                        to: el.route ? el.route : "",
                        href: el.href ? el.href : "",
                        target: el.href ? "_blank" : "",
                        rel: el.href ? "noreferrer" : "noreferrer",
                      };
                      return (
                        <ArgonTypography
                          {...elLinkProps}
                          key={el.name}
                          minWidth="11.25rem"
                          display="block"
                          variant="button"
                          color="text"
                          textTransform="capitalize"
                          fontWeight="regular"
                          py={0.625}
                          px={item.icon ? 5 : 2}
                          mb={index === item.collapse!.length - 1 ? 2 : 0}
                          sx={({
                            palette: { white, grey, dark },
                            borders: { borderRadius },
                            functions: { rgba },
                          }) => ({
                            borderRadius: borderRadius.md,
                            cursor: "pointer",
                            transition: "all 300ms linear",

                            "&:hover": {
                              backgroundColor: rgba(grey[200], darkMode ? 0.1 : 1),
                              color: darkMode ? white.main : dark.main,
                            },
                          })}
                        >
                          {el.name}
                        </ArgonTypography>
                      );
                    })}
                  </>
                ) : (
                  (() => {
                    const itemLinkProps = {
                      component: item.route ? Link : MuiLink,
                      to: item.route ? item.route : "",
                      href: item.href ? item.href : "",
                      target: item.href ? "_blank" : "",
                      rel: item.href ? "noreferrer" : "noreferrer",
                    };
                    return (
                      <ArgonBox
                        {...itemLinkProps}
                        key={item.key}
                        display="flex"
                        sx={({
                          palette: { white, grey, dark },
                          borders: { borderRadius },
                          functions: { rgba },
                        }) => ({
                          borderRadius: borderRadius.md,
                          cursor: "pointer",
                          transition: "all 300ms linear",
                          py: 1,
                          px: 1.625,

                          "&:hover": {
                            backgroundColor: rgba(grey[200], darkMode ? 0.1 : 1),
                            color: darkMode ? white.main : dark.main,
                          },
                        })}
                      >
                        <ArgonBox
                          display="flex"
                          justifyContent="center"
                          alignItems="center"
                          width="1.5rem"
                          height="1.5rem"
                          borderRadius="md"
                          color="text"
                          mr={1}
                          fontSize="1rem"
                          lineHeight={1}
                        >
                          {typeof item.icon === "string" ? <Icon>{item.icon}</Icon> : item.icon}
                        </ArgonBox>
                        <ArgonBox>
                          <ArgonTypography
                            display="block"
                            variant="button"
                            fontWeight={!item.description ? "regular" : "bold"}
                            mt={!item.description ? 0.25 : 0}
                            textTransform="capitalize"
                          >
                            {item.name || "&nbsp"}
                          </ArgonTypography>
                          {item.description && (
                            <ArgonTypography
                              display="block"
                              variant="button"
                              color="text"
                              fontWeight="regular"
                              sx={{ transition: "all 300ms linear" }}
                            >
                              {item.description}
                            </ArgonTypography>
                          )}
                        </ArgonBox>
                      </ArgonBox>
                    );
                  })()
                )}
              </ArgonBox>
            ))}
        </ArgonBox>
      </DefaultNavbarDropdown>
    )
  );

  return (
    <Collapse in={Boolean(open)} timeout="auto" unmountOnExit>
      <ArgonBox width="calc(100% + 1.625rem)" my={2} ml={-2}>
        {renderNavbarItems}
      </ArgonBox>
    </Collapse>
  );
}

export default DefaultNavbarMobile;
