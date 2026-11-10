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
import { useLocation, useNavigate } from "react-router";
import { useTranslation } from "react-i18next";
import { useAuth } from "AuthContext";
import { useContext, useState } from "react";
import type { ChangeEventHandler, MouseEvent, ReactNode } from "react";
// @mui core components
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

// Custom styles for DashboardNavbar
import { navbar, navbarIconButton } from "controls/Navbars/DashboardNavbar/styles";

// Argon Dashboard 2 MUI context
import { useArgonController, setMiniSidenav, setOpenConfigurator } from "context";
import { CurrentAccountContext } from "context/account";
import routes from "routes";

export interface DashboardNavbarProps {
  searchVisibility?: boolean;
  searchQuery?: string;
  handleSearch?: ChangeEventHandler<HTMLInputElement | HTMLTextAreaElement>;
  /** Screen-level tabs, rendered on the bar beside the title. */
  tabs?: ReactNode;
}

function DashboardNavbar({
  searchVisibility = false,
  searchQuery = "",
  handleSearch = () => {},
  tabs,
}: DashboardNavbarProps) {
  const [controller, dispatch] = useArgonController();
  const { miniSidenav, openConfigurator } = controller;
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const { logoff, isAuthenticated } = useAuth();
  const { isManager } = useContext(CurrentAccountContext);
  const { t } = useTranslation();
  const { openHelp } = useHelp();
  const [bellAnchor, setBellAnchor] = useState<HTMLElement | null>(null);
  const [userAnchor, setUserAnchor] = useState<HTMLElement | null>(null);

  const screen = routes.find((route) => route.route === pathname);

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
    <ArgonBox component="header" sx={navbar}>
      <Tooltip title={t("navbar.toggleMenu")}>
        <IconButton
          sx={navbarIconButton}
          onClick={handleMiniSidenav}
          aria-label={t("navbar.toggleMenu")}
        >
          <Icon>{miniSidenav ? "menu" : "menu_open"}</Icon>
        </IconButton>
      </Tooltip>
      <ArgonTypography component="h1" variant="h6" fontWeight="bold" noWrap data-testid="screen-title">
        {screen?.name ? t(screen.name as never) : ""}
      </ArgonTypography>
      {tabs && (
        <ArgonBox alignSelf="flex-end" ml={2}>
          {tabs}
        </ArgonBox>
      )}
      <ArgonBox display="flex" alignItems="center" gap={0.5} ml="auto" py={1}>
        {searchVisibility && (
          <ArgonBox width={{ xs: "9rem", md: "16rem" }} mr={1}>
            <ArgonInput
              value={searchQuery}
              onChange={handleSearch}
              placeholder={t("navbar.searchText")}
              size="small"
              startAdornment={
                <Icon fontSize="small" style={{ marginRight: "6px" }}>
                  search
                </Icon>
              }
            />
          </ArgonBox>
        )}
        <Tooltip title={t("help.open")}>
          <IconButton sx={navbarIconButton} onClick={() => openHelp()} aria-label={t("help.open")}>
            <Icon>help_outline</Icon>
          </IconButton>
        </Tooltip>
        <IconButton
          sx={navbarIconButton}
          onClick={handleBellOpen}
          aria-label={t("notificationBell.title")}
        >
          <Badge badgeContent={unreadCount} color="error">
            <Icon>notifications_none</Icon>
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
                onClick={() => handleNotificationClick(item.notificationDeliveryId, item.readAt)}
              >
                <ArgonBox display="flex" flexDirection="column" lineHeight={1.25}>
                  <ArgonTypography variant="button" fontWeight={item.readAt ? "regular" : "bold"}>
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
        {isManager && (
          <Tooltip title={t("settings.title")}>
            <IconButton
              sx={navbarIconButton}
              onClick={() => setOpenConfigurator(dispatch, !openConfigurator)}
              aria-label={t("settings.title")}
              data-testid="configurator-toggle"
            >
              <Icon>settings</Icon>
            </IconButton>
          </Tooltip>
        )}
        <IconButton
          sx={navbarIconButton}
          onClick={(event) => setUserAnchor(event.currentTarget)}
          aria-label={t("navbar.userMenu")}
          aria-haspopup="menu"
          data-testid="user-menu"
        >
          <Icon>account_circle</Icon>
        </IconButton>
        <Menu
          anchorEl={userAnchor}
          open={Boolean(userAnchor)}
          onClose={() => setUserAnchor(null)}
          anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
          transformOrigin={{ vertical: "top", horizontal: "right" }}
        >
          <MenuItem
            onClick={() => {
              setUserAnchor(null);
              navigate("/profile");
            }}
          >
            {t("screen.profile")}
          </MenuItem>
          <MenuItem
            onClick={() => {
              setUserAnchor(null);
              logoff();
            }}
          >
            {t("navbar.signOff")}
          </MenuItem>
        </Menu>
      </ArgonBox>
    </ArgonBox>
  );
}

export default DashboardNavbar;
