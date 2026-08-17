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

import { useEffect, useMemo, useState, useContext } from "react";
import type { ReactNode } from "react";
import { useTranslation } from 'react-i18next';
import { Name, Description } from "controls/Tables/components/tableComponents";
import Icon from "@mui/material/Icon";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import { useAccountByUser } from "queries/accounts";
import {
  usePointsOfInterestByAccount,
  useCreatePointOfInterest,
  useUpdatePointOfInterest,
  useDeletePointOfInterest,
} from 'queries/pointsOfInterest';
import { useGroups } from 'queries/groups';
import type { Group } from 'api/manager/groups';
import type {
  PointOfInterest,
  PointOfInterestDtoInput,
  UpdatePointOfInterestDtoInput,
} from 'api/manager/pointsOfInterest';
import { getPoiType } from "data/poiTypes";
import { toCamelCase } from 'utils/stringUtils';
import { LoadingContext } from 'LoadingContext';
import { useAuth } from "AuthContext";

/**
 * Dialog/form state for a POI. Merges an API {@link PointOfInterest} (when
 * editing) with the loose fresh-add shape; select/text controls may hand back
 * `type`/`color`/`groupId` as strings, hence the widened unions. The
 * `name`/`type`/`latitude`/`longitude` requirement is enforced by the dialog's
 * validate() gate before save.
 */
export interface PoiFormValues {
  pointOfInterestId?: string;
  name?: string;
  description?: string | null;
  type?: number | string;
  latitude?: number | string;
  longitude?: number | string;
  address?: string | null;
  color?: number | string | null;
  groupId?: number | string | null;
  active?: boolean;
}

/** Group option for the POI dialog's group select. */
export interface PoiGroupOption { value: number; label: string; }

/** A column descriptor / rendered row for the vendored POIs `Table`. */
export interface PoiColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export interface PoiRow {
  [key: string]: ReactNode;
  name: ReactNode;
  type: ReactNode;
  coordinates: ReactNode;
  group: ReactNode;
  active: ReactNode;
  action: ReactNode;
  id: string;
}
export interface PoiTableData { columns: PoiColumn[]; rows: PoiRow[]; }

function usePoiTableData(
  fetchData: boolean,
  handleEditClick: (poi: PoiFormValues) => void,
  handleDeleteClick: (pointOfInterestId: string) => void
) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();

  const enabled = !!fetchData && isAuthenticated;
  const poisQuery = usePointsOfInterestByAccount({ enabled });
  const pois = poisQuery.data ?? [];
  const groupsQuery = useGroups({ enabled });
  const groups = groupsQuery.data ?? [];
  const createPoi = useCreatePointOfInterest();
  const updatePoi = useUpdatePointOfInterest();
  const deletePoi = useDeletePointOfInterest();

  // Current account id, required to create a POI (PointOfInterestDtoInput.accountId).
  const accountQuery = useAccountByUser({ enabled });
  const accountId = accountQuery.data?.accountId ?? null;

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(poisQuery.isFetching || groupsQuery.isFetching);
  }, [poisQuery.isFetching, groupsQuery.isFetching, setLoading]);

  const onSave = async (poi: PoiFormValues) => {
    setLoading(true);
    try {
      // validate + coordinate checks gate this call, so the required create/update
      // input fields are present — assert them at the boundary.
      if (poi.pointOfInterestId) {
        await updatePoi.mutateAsync({
          pointOfInterestId: poi.pointOfInterestId,
          name: poi.name,
          description: poi.description,
          type: poi.type,
          latitude: poi.latitude,
          longitude: poi.longitude,
          address: poi.address,
          color: poi.color,
          groupId: poi.groupId,
          active: poi.active,
        } as UpdatePointOfInterestDtoInput & { pointOfInterestId: string });
      } else {
        await createPoi.mutateAsync({
          accountId,
          name: poi.name,
          description: poi.description,
          type: poi.type,
          latitude: poi.latitude,
          longitude: poi.longitude,
          address: poi.address,
          color: poi.color,
          groupId: poi.groupId,
          active: poi.active,
        } as PointOfInterestDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const onDelete = async (pointOfInterestId: string) => {
    setLoading(true);
    try {
      await deletePoi.mutateAsync(pointOfInterestId);
      setConfirmOpen(false);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const handleOpen = (poi: PointOfInterest) => {
    handleEditClick(poi);
    setOpen(true);
  };

  const handleOpenDelete = (pointOfInterestId: string) => {
    handleDeleteClick(pointOfInterestId);
    setConfirmOpen(true);
  };

  const buildTableData = (poiList: PointOfInterest[], groupList: Group[]): PoiTableData => ({
    columns: [
      { name: "name", title:t('poi.name'), align: "left" },
      { name: "type", title:t('poi.type'), align: "left" },
      { name: "coordinates", title:t('poi.coordinates'), align: "center" },
      { name: "group", title:t('poi.group'), align: "left" },
      { name: "active", title:t('poi.active'), align: "center" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "id" }
    ],
    rows: poiList.map(poi => {
      const group = groupList.find(g => g.groupId === poi.groupId);
      // Unknown type values must not render a dangling i18n key.
      const typeLabel = getPoiType(poi.type);
      return {
        name: <Name name={poi.name} />,
        type: <Description description={typeLabel ? t(`poi.types.${toCamelCase(typeLabel)}` as 'poi.types.clientSite') : '-'} />,
        coordinates: <Description description={`${poi.latitude}, ${poi.longitude}`} />,
        group: <Description description={group ? group.name : '-'} />,
        active: (
          <ArgonBadge
            variant="gradient"
            color={poi.active ? 'success' : 'secondary'}
            size="xs"
            container
            badgeContent={poi.active ? t('generic.yes') : t('generic.no')} />
        ),
        action: (
          <>
            <ArgonButton
                variant="text"
                color="dark"
                onClick={() => handleOpen(poi)}>
              <Icon>edit</Icon>&nbsp;{t('generic.edit')}
            </ArgonButton>
            <ArgonButton
              variant="text"
              color="error"
              onClick={() => handleOpenDelete(poi.pointOfInterestId)}>
              <Icon>delete</Icon>&nbsp;{t('generic.delete')}
            </ArgonButton>
          </>
        ),
        id: poi.pointOfInterestId
      };
    }),
  });

  const data = useMemo(
    () => buildTableData(pois, groups),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [pois, groups, t]
  );

  const groupOptions: PoiGroupOption[] = groups.map(group => ({ value: group.groupId, label: group.name }));

  return {
    data,
    groupOptions,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen };
}

export default usePoiTableData;
