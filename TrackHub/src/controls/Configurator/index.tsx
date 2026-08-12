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

import { useContext, useState, useEffect } from "react";
import type { ChangeEvent } from "react";

// @mui material components
import Divider from "@mui/material/Divider";
import Switch from "@mui/material/Switch";
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import CustomTextField from "controls/Dialogs/CustomTextField";

// Custom styles for the Configurator
import ConfiguratorRoot from "controls/Configurator/ConfiguratorRoot";
import { useTranslation } from "react-i18next";
import CustomSelect from "controls/Dialogs/CustomSelect";
import { LoadingContext } from "LoadingContext";
import maps from "data/maps";
import type { FormChangeEvent } from "controls/Dialogs/useForm";
import type { AccountSettings, AccountSettingsDtoInput } from "api/manager/settings";

// Argon Dashboard 2 MUI context
import { useArgonController, setOpenConfigurator } from "context";

// Local, deliberately loose form state: number fields are stored as raw input
// strings (the native inputs emit strings) until persisted, matching the legacy
// runtime behavior.
interface ConfiguratorState {
  maps?: string;
  mapsKey?: string | null;
  onlineInterval?: string | number;
  refreshMap?: boolean;
  refreshMapInterval?: string | number;
  accountId?: string;
}

export interface ConfiguratorProps {
  settings: Partial<AccountSettings>;
  updateSettings: (
    accountId: string,
    settings: Omit<AccountSettingsDtoInput, "accountId">
  ) => void;
}

function Configurator({ settings, updateSettings }: ConfiguratorProps) {
  const { setLoading } = useContext(LoadingContext);
  const [controller, dispatch] = useArgonController();
  const { openConfigurator, darkMode } = controller;
  const { t } = useTranslation();
  const mapOptions = maps.map((type) => ({ value: type, label: type }));

  const [accountSettings, setAccountSettings] = useState<ConfiguratorState>({
    maps: "OSM",
    refreshMap: false,
  });
  const handleCloseConfigurator = () => setOpenConfigurator(dispatch, false);

  useEffect(() => {
    const fetchAccountSettings = async () => {
      if (settings && settings.maps) setAccountSettings(settings);
    };

    fetchAccountSettings();
  }, [settings]);

  function handleMapsChange(e: FormChangeEvent) {
    setAccountSettings((prevSettings) => ({
      ...prevSettings,
      maps: e.target.value as string,
    }));
  }

  const handleMapsKeyChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setAccountSettings((prevSettings) => ({
      ...prevSettings,
      mapsKey: event.target.value,
    }));
  };

  const handleOnlineIntervalChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    setAccountSettings((prevSettings) => ({
      ...prevSettings,
      onlineInterval: event.target.value,
    }));
  };

  function handleRefreshMapChange() {
    setAccountSettings((prevSettings) => ({
      ...prevSettings,
      refreshMap: !settings.refreshMap,
    }));
  }

  const handleRefreshMapIntervalChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    setAccountSettings((prevSettings) => ({
      ...prevSettings,
      refreshMapInterval: event.target.value,
    }));
  };

  async function onSaveSettings() {
    setLoading(true);
    updateSettings(
      accountSettings.accountId as string,
      accountSettings as Omit<AccountSettingsDtoInput, "accountId">
    );
    setLoading(false);
    alert(t("settings.saveMessage"));
  }

  return (
    <ConfiguratorRoot variant="permanent" ownerState={{ openConfigurator }}>
      <ArgonBox
        display="flex"
        justifyContent="space-between"
        alignItems="baseline"
        pt={3}
        pb={0.8}
        px={3}
      >
        <ArgonBox>
          <ArgonTypography variant="h5">{t("settings.title")}</ArgonTypography>
          <ArgonTypography variant="body2" color="text">
            {t("settings.detail")}
          </ArgonTypography>
        </ArgonBox>

        <Icon
          sx={(theme) => {
            const { fontWeightBold, size } = theme.typography;
            const { dark, white } = theme.palette;
            return {
              fontSize: `${size.md} !important`,
              fontWeight: `${fontWeightBold} !important`,
              color: darkMode ? white.main : dark.main,
              stroke: darkMode ? white.main : dark.main,
              strokeWidth: "2px",
              cursor: "pointer",
              mt: 2,
            };
          }}
          onClick={handleCloseConfigurator}
        >
          close
        </Icon>
      </ArgonBox>

      <Divider />

      <ArgonBox pt={1.25} pb={3} px={3}>
        <ArgonBox display="flex" justifyContent="space-between" lineHeight={1}>
          <CustomSelect
            list={mapOptions}
            handleChange={handleMapsChange}
            name="map"
            id="map"
            label={t("settings.maps")}
            value={accountSettings.maps}
            numericValue={false}
            required
          />
        </ArgonBox>

        {accountSettings.maps !== "OSM" && (
          <ArgonBox display="flex" justifyContent="space-between" lineHeight={1}>
            <CustomTextField
              margin="dense"
              name="mapsKey"
              id="mapsKey"
              label={t("settings.mapsKey")}
              type="text"
              fullWidth
              value={accountSettings.mapsKey || ""}
              onChange={handleMapsKeyChange}
            />
          </ArgonBox>
        )}

        <ArgonBox display="flex" justifyContent="space-between" lineHeight={1}>
          <CustomTextField
            margin="dense"
            name="onlineInterval"
            id="onlineInterval"
            label={t("settings.onlineInterval")}
            type="number"
            fullWidth
            value={accountSettings.onlineInterval || 60}
            onChange={handleOnlineIntervalChange}
          />
        </ArgonBox>

        <ArgonBox display="flex" justifyContent="space-between" mt={3} lineHeight={1}>
          <ArgonTypography variant="h6">{t("settings.refreshMap")}</ArgonTypography>
          <Switch checked={accountSettings.refreshMap} onChange={handleRefreshMapChange} />
        </ArgonBox>

        <ArgonBox display="flex" justifyContent="space-between" lineHeight={1}>
          <CustomTextField
            margin="dense"
            name="refreshMapInterval"
            id="refreshMapInterval"
            label={t("settings.refreshMapInterval")}
            type="number"
            fullWidth
            value={accountSettings.refreshMapInterval || 60}
            onChange={handleRefreshMapIntervalChange}
          />
        </ArgonBox>

        <Divider />

        <ArgonButton variant="gradient" onClick={onSaveSettings} color="dark">
          <Icon sx={{ fontWeight: "bold" }}>save</Icon>
          &nbsp;{t("generic.save")}
        </ArgonButton>
      </ArgonBox>
    </ConfiguratorRoot>
  );
}

export default Configurator;
