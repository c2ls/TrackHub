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

import Card from "@mui/material/Card";
import Icon from "@mui/material/Icon";
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import SingleTrip from "layouts/dashboard/components/TripList/SingleTrip";
import { formatDateTime } from "utils/dateUtils";
import { useTheme } from '@mui/material/styles';
import type { Trip } from "api/router/router";

/** The (partial) replay filter values consumed for the date-range header. */
interface TripListFilters {
  startDate?: string;
  endDate?: string;
}

interface TripListProps {
  filters: TripListFilters;
  trips?: Trip[];
  selectedTrip?: string | null;
  handleSelected: (value: string | null) => void;
  maxHeight?: string;
}

function TripList({ filters, trips = [], selectedTrip, handleSelected, maxHeight = "70vh" }: TripListProps) {
  const theme = useTheme();
  return (
    <Card sx={{ height: maxHeight, overflow: "auto" }}>
      <ArgonBox display="flex" justifyContent="space-between" alignItems="center" pt={3} px={2}>
        <ArgonBox display="flex" alignItems="flex-start">
          <ArgonBox color="text" mr={0.5} lineHeight={0}>
            <Icon color="inherit" fontSize="small">
              date_range
            </Icon>
          </ArgonBox>
          <ArgonTypography variant="button" color="text" fontWeight="regular">
            {formatDateTime(filters.startDate)} - {formatDateTime(filters.endDate)}
          </ArgonTypography>
        </ArgonBox>
      </ArgonBox>
      <ArgonBox pt={3} pb={2} px={2}>
        <ArgonBox
          component="ul"
          display="flex"
          flexDirection="column"
          p={0}
          m={0}
          sx={{ listStyle: "none", height: '100%', overflowY: 'auto' }}
        >
          {trips.map((trip) => (
            <ArgonBox
              key={`p-${trip.tripId}`}
              onClick={() => handleSelected(trip.tripId)}
              sx={{
                cursor: 'pointer',
                backgroundColor: selectedTrip === trip.tripId
                  ? theme.palette.mode === 'dark'
                    ? theme.palette.primary.dark
                    : theme.palette.primary.light
                  : 'inherit',
              }}>
              <SingleTrip
                id={trip.tripId}
                from={trip.from}
                to={trip.to}
                event={trip.type}
                duration={trip.duration as string}
                distance={trip.totalDistance}
              />
            </ArgonBox>
          ))}
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
}

export default TripList;
