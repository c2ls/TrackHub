/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

import ArgonBox from "components/ArgonBox";
import DashboardLayout from "controls/LayoutContainers/DashboardLayout";
import DashboardNavbar from "controls/Navbars/DashboardNavbar";
import Footer from "controls/Footer";
import GpsDashboard from "layouts/gpsintegration/components/Dashboard";
import ManageOperators from "layouts/gpsintegration/components/operators";
import ManageSynchronizedDevices from "layouts/gpsintegration/components/SynchronizedDevices";
import ManageDeviceAssignments from "layouts/gpsintegration/components/Assignments";
import RecentSyncRuns from "layouts/gpsintegration/components/RecentSyncRuns";
import OpenAlerts from "layouts/gpsintegration/components/OpenAlerts";
import RetentionSettings from "layouts/gpsintegration/components/RetentionSettings";

function GpsIntegration() {
  return (
    <DashboardLayout>
      <DashboardNavbar />
      <ArgonBox py={3}>
        <ArgonBox mb={3}>
          <GpsDashboard />
        </ArgonBox>
        <ManageOperators />
        <RecentSyncRuns />
        <OpenAlerts />
        <ManageSynchronizedDevices />
        <ManageDeviceAssignments />
        <RetentionSettings />
      </ArgonBox>
      <Footer />
    </DashboardLayout>
  );
}

export default GpsIntegration;
