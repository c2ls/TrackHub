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

// react-router components
import { useLocation, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "AuthContext";
import { useState } from "react";
import type { ChangeEventHandler, MouseEvent } from "react";
// @mui core components
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import IconButton from "@mui/material/IconButton";
import Icon from "@mui/material/Icon";
import Badge from "@mui/material/Badge";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import Tooltip from "@mui/material/Tooltip";

// Contextual help
import { useHelp } from "context/help/HelpContext";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonInput from "components/ArgonInput";

// In-app notification feed
import { useMyNotifications, useMarkNotificationRead } from "queries/notifications";
import { formatDateTime } from "utils/dateUtils";
import { toCamelCase } from "utils/stringUtils";

// Argon Dashboard 2 MUI example components
import Breadcrumbs from "controls/Breadcrumbs";

// Custom styles for DashboardNavbar
import {
  navbar,
  navbarContainer,
  navbarRow,
  navbarIconButton,
  navbarDesktopMenu,
  navbarMobileMenu,
} from "controls/Navbars/DashboardNavbar/styles";

// Argon Dashboard 2 MUI context
import { useArgonController, setMiniSidenav } from "context";

export interface DashboardNavbarProps {
  absolute?: boolean;
  light?: boolean;
  isMini?: boolean;
  searchVisibility?: boolean;
  searchQuery?: string;
  handleSearch?: ChangeEventHandler<HTMLInputElement | HTMLTextAreaElement>;
}

function DashboardNavbar({
  absolute = false,
  light = true,
  isMini = false,
  searchVisibility = false,
  searchQuery = "",
  handleSearch = () => {},
}: DashboardNavbarProps) {
  const [controller, dispatch] = useArgonController();
  const { miniSidenav, transparentNavbar } = controller;
  const route = useLocation().pathname.split("/").slice(1);
  const { logoff, isAuthenticated } = useAuth();
  const { t } = useTranslation();
  const { openHelp } = useHelp();
  const [bellAnchor, setBellAnchor] = useState<HTMLElement | null>(null);

  // A failed feed read (e.g. permissions) renders the bell with a zero badge
  // and an empty menu; the global query-cache handler owns the error toast.
  const { data: myNotifications, isError: feedError } = useMyNotifications(false, {
    enabled: isAuthenticated,
  });
  const markRead = useMarkNotificationRead();
  const notifications = feedError ? [] : myNotifications ?? [];
  const unreadCount = notifications.filter((item) => !item.readAt).length;

  const handleMiniSidenav = () => setMiniSidenav(dispatch, !miniSidenav);

  const handleBellOpen = (event: MouseEvent<HTMLElement>) => setBellAnchor(event.currentTarget);
  const handleBellClose = () => setBellAnchor(null);

  const handleNotificationClick = (notificationDeliveryId: string, readAt?: string | null) => {
    if (!readAt) {
      markRead.mutate(notificationDeliveryId);
    }
    handleBellClose();
  };

  const severityColor = (severity?: string | null): "error" | "warning" | "info" => {
    const normalized = (severity || "").toLowerCase();
    if (normalized === "critical") return "error";
    if (normalized === "warning") return "warning";
    return "info";
  };

  return (
    <AppBar
      position={absolute ? "absolute" : "static"}
      color="inherit"
      sx={(theme) => navbar(theme, { transparentNavbar, absolute, light })}
    >
      <Toolbar sx={(theme) => navbarContainer(theme, "static")}>
        <ArgonBox
          color={light && transparentNavbar ? "white" : "dark"}
          mb={{ xs: 1, md: 0 }}
          sx={(theme) => navbarRow(theme, { isMini })}
        >
          <Breadcrumbs
            icon="home"
            title={route[route.length - 1]}
            route={route}
            light={transparentNavbar ? light : false}
          />
          <Icon fontSize="medium" sx={navbarDesktopMenu} onClick={handleMiniSidenav}>
            {miniSidenav ? "menu_open" : "menu"}
          </Icon>
        </ArgonBox>
        {isMini ? null : (
          <ArgonBox sx={(theme) => navbarRow(theme, { isMini })}>
            <ArgonBox pr={1}>
              {searchVisibility && (
                <ArgonInput
                  value={searchQuery}
                  onChange={handleSearch}
                  placeholder={t("navbar.searchText")}
                  startAdornment={
                    <Icon fontSize="small" style={{ marginRight: "6px" }}>
                      search
                    </Icon>
                  }
                />
              )}
            </ArgonBox>
            <ArgonBox color={light ? "white" : "inherit"}>
              <Tooltip title={t("help.open")}>
                <IconButton
                  sx={navbarIconButton}
                  size="small"
                  onClick={() => openHelp()}
                  aria-label={t("help.open")}
                >
                  <Icon
                    sx={({ palette: { dark, white } }) => ({
                      color: light && transparentNavbar ? white.main : dark.main,
                    })}
                  >
                    help_outline
                  </Icon>
                </IconButton>
              </Tooltip>
              <IconButton
                sx={navbarIconButton}
                size="small"
                onClick={handleBellOpen}
                aria-label={t("notificationBell.title")}
              >
                <Badge badgeContent={unreadCount} color="error">
                  <Icon
                    sx={({ palette: { dark, white } }) => ({
                      color: light && transparentNavbar ? white.main : dark.main,
                    })}
                  >
                    notifications
                  </Icon>
                </Badge>
              </IconButton>
              <Menu
                anchorEl={bellAnchor}
                open={Boolean(bellAnchor)}
                onClose={handleBellClose}
                anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
                transformOrigin={{ vertical: "top", horizontal: "right" }}
              >
                {notifications.length === 0 ? (
                  <MenuItem disabled>
                    <ArgonTypography variant="caption" color="secondary">
                      {t("notificationBell.empty")}
                    </ArgonTypography>
                  </MenuItem>
                ) : (
                  notifications.map((item) => (
                    <MenuItem
                      key={item.notificationDeliveryId}
                      onClick={() =>
                        handleNotificationClick(item.notificationDeliveryId, item.readAt)
                      }
                    >
                      <ArgonBox display="flex" flexDirection="column" lineHeight={1.25}>
                        <ArgonTypography
                          variant="button"
                          fontWeight={item.readAt ? "regular" : "bold"}
                        >
                          {item.eventType
                            ? t(
                                `alertEventTypes.${toCamelCase(item.eventType)}` as 'alertEventTypes.geofenceEntered',
                                { defaultValue: item.eventType }
                              )
                            : t("notificationBell.title")}
                        </ArgonTypography>
                        <ArgonTypography variant="caption" color={severityColor(item.severity)}>
                          {item.severity || "-"} &middot; {formatDateTime(item.createdAt)}
                        </ArgonTypography>
                      </ArgonBox>
                    </MenuItem>
                  ))
                )}
              </Menu>
              <Link
                to={undefined as never}
                onClick={(e) => {
                  e.preventDefault();
                  logoff();
                }}
              >
                <IconButton sx={navbarIconButton} size="small">
                  <Icon
                    sx={({ palette: { dark, white } }) => ({
                      color: light && transparentNavbar ? white.main : dark.main,
                    })}
                  >
                    account_circle
                  </Icon>
                  <ArgonTypography
                    variant="button"
                    fontWeight="medium"
                    color={light && transparentNavbar ? "white" : "dark"}
                  >
                    {t("navbar.signOff")}
                  </ArgonTypography>
                </IconButton>
              </Link>
              <IconButton
                size="small"
                color={(light && transparentNavbar ? "white" : "dark") as never}
                sx={navbarMobileMenu}
                onClick={handleMiniSidenav}
              >
                <Icon>{miniSidenav ? "menu_open" : "menu"}</Icon>
              </IconButton>
            </ArgonBox>
          </ArgonBox>
        )}
      </Toolbar>
    </AppBar>
  );
}

export default DashboardNavbar;
