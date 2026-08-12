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

import { useState, useEffect, useContext } from 'react';
import DashboardTabbar from "controls/Navbars/DashboardTabbar";
import Transporters from "layouts/dashboard/components/Transporters";
import Positions from "layouts/dashboard/components/Positions";
import { getAccountSettings } from 'api/manager/settings';
import type { AccountSettings } from 'api/manager/settings';
import { notifyApiError } from 'api/core/errors';
import { useAllGeofences } from 'queries/geofences';
import { LoadingContext } from 'LoadingContext';
import { useTranslation } from 'react-i18next';
import { useAuth } from "AuthContext";

function Default() {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const { setLoading } = useContext(LoadingContext);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedTab, setSelectedTab] = useState(0);
  const [showGeofence, setShowGeofence] = useState(false);
  const [searchVisibility, setSearchVisibility] = useState(true);
  // The bootstrap value is a partial AccountSettings (the map defaults) until
  // getAccountSettings() resolves; cast keeps the exact runtime shape.
  const [settings, setSettings] = useState<AccountSettings>({ maps: 'OSM', mapsKey: '', refreshMapInterval: 60 } as AccountSettings);

  // Geofences are loaded (cached, all pages drained) only once the overlay is
  // toggled on.
  const geofencesQuery = useAllGeofences(true, {}, { enabled: isAuthenticated && showGeofence });
  const geofences = geofencesQuery.data ?? [];

  const fetchSettings = async () => {
    setLoading(true);
    try {
      const settings = await getAccountSettings();
      setSettings(settings);
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isAuthenticated) {
      fetchSettings();
    }
  }, [isAuthenticated]);

  // Keep the global spinner UX while the geofence overlay loads.
  useEffect(() => {
    setLoading(geofencesQuery.isFetching);
  }, [geofencesQuery.isFetching, setLoading]);

  const handleSearchChange = (event: { target: { value: string } }) => {
    setSearchQuery(event.target.value);
  };

  const handleTabChange = (newValue: number) => {
    setSelectedTab(newValue);
    setSearchVisibility(newValue === 0);
  };

  const renderContent = () => {
    switch (selectedTab) {
      case 0:
        return <Transporters
          searchQuery={searchQuery}
          settings={settings}
          geofences={geofences}
          setShowGeofence={setShowGeofence}
          showGeofence={showGeofence} />;
      case 1:
        return <Positions
          settings={settings}
          geofences={geofences}
          showGeofence={showGeofence}/>;
      default:
        return null;
    }
  };

  return (
    <DashboardTabbar
      stickyNavbar
      searchQuery={searchQuery}
      handleSearch={handleSearchChange}
      searchVisibility={searchVisibility}
      tabs={[t("dashboard.transportersTitle"), t("dashboard.positionsTitle")]}
      onTabChange={handleTabChange}>
      {renderContent()}
    </DashboardTabbar>
  );
}

export default Default;
