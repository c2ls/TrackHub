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

import type { ComponentType } from "react";
import { useTranslation } from "react-i18next";
import ManageAccount from "layouts/manageadmin/components/account";
import ManageDevices from "layouts/manageadmin/components/devices";
import ManageTransporters from "layouts/manageadmin/components/transporters";
import ManageUsers from "layouts/manageadmin/components/users";
import ManageRoles from "layouts/manageadmin/components/roles";
import ManagePolicies from "layouts/manageadmin/components/policies";
import ManageGroups from "layouts/manageadmin/components/groups";
import ManagePois from "layouts/manageadmin/components/pois";
import ManageAccountFeatures from "layouts/manageadmin/components/accountFeatures";
import ManageBranding from "layouts/manageadmin/components/branding";
import ManageDrivers from "layouts/manageadmin/components/drivers";
import ManageAuditTrail from "layouts/manageadmin/components/auditTrail";
import ManageNotificationRules from "layouts/manageadmin/components/notificationRules";
import ManageAlertEvents from "layouts/manageadmin/components/alertEvents";
import ManageAlertSubscriptions from "layouts/manageadmin/components/alertSubscriptions";
import ManageNotificationDeliveries from "layouts/manageadmin/components/notificationDeliveries";
import ManageNotificationTemplates from "layouts/manageadmin/components/notificationTemplates";
import ManagePublicLinks from "layouts/manageadmin/components/publicLinks";
import ManageDocuments from "layouts/manageadmin/components/documents";
import ManageBackgroundJobs from "layouts/manageadmin/components/backgroundJobs";
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import DashboardLayout from "controls/LayoutContainers/DashboardLayout";
import DashboardNavbar from "controls/Navbars/DashboardNavbar";
import Footer from "controls/Footer";
import { SECTION_GROUP_KEYS } from "layouts/manageadmin/data/sectionGroups";
import type { SectionGroupKey } from "layouts/manageadmin/data/sectionGroups";
import { useFeatures } from "context/features";

/** A Manage section, optionally gated by an account feature flag. */
interface SectionEntry {
  Section: ComponentType;
  /** Feature key gating the section's backend queries; ungated sections omit it. */
  featureKey?: string;
}

// Sections per functional group; the group order lives in sectionGroups.ts.
// Feature keys mirror the backend [RequireFeature] gates so disabled features
// hide their sections instead of erroring server-side. Alert events stay
// visible: their read path is deliberately ungated (other modules emit them).
const SECTION_GROUPS: Record<SectionGroupKey, SectionEntry[]> = {
  account: [{ Section: ManageAccount }, { Section: ManageBranding }, { Section: ManageAccountFeatures }],
  fleet: [
    { Section: ManageDevices },
    { Section: ManageTransporters },
    { Section: ManageDrivers },
    { Section: ManageGroups },
    { Section: ManagePois },
  ],
  access: [{ Section: ManageUsers }, { Section: ManageRoles }, { Section: ManagePolicies }],
  alerts: [
    { Section: ManageNotificationRules, featureKey: 'notifications' },
    { Section: ManageAlertSubscriptions, featureKey: 'notifications' },
    { Section: ManageNotificationTemplates, featureKey: 'notifications' },
    { Section: ManageAlertEvents },
    { Section: ManageNotificationDeliveries, featureKey: 'notifications' },
  ],
  documents: [
    { Section: ManageDocuments, featureKey: 'documents' },
    { Section: ManagePublicLinks, featureKey: 'public-links' },
  ],
  operations: [{ Section: ManageAuditTrail }, { Section: ManageBackgroundJobs }],
};

function ManageAdmin() {
  const { t } = useTranslation();
  const { isFeatureEnabled } = useFeatures();

  return (
    <DashboardLayout>
      <DashboardNavbar />
        <ArgonBox py={3}>
          {SECTION_GROUP_KEYS.map((groupKey) => {
            const sections = SECTION_GROUPS[groupKey].filter(
              ({ featureKey }) => isFeatureEnabled(featureKey)
            );
            if (sections.length === 0) return null;
            return (
              <ArgonBox key={groupKey} mb={4}>
                <ArgonTypography
                  variant="button"
                  fontWeight="bold"
                  textTransform="uppercase"
                  color="text"
                  sx={{ letterSpacing: 1 }}
                >
                  {t(`manageAdmin.groups.${groupKey}` as 'manageAdmin.groups.account')}
                </ArgonTypography>
                <ArgonBox mt={1.5}>
                  {sections.map(({ Section }) => (
                    <Section key={Section.displayName ?? Section.name} />
                  ))}
                </ArgonBox>
              </ArgonBox>
            );
          })}
        </ArgonBox>
      <Footer />
    </DashboardLayout>
  );
}

export default ManageAdmin;
