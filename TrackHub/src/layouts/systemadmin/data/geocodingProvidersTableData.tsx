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
import {
  useGeocodingProviders,
  useCreateGeocodingProvider,
  useUpdateGeocodingProvider,
  useDeleteGeocodingProvider,
  useSetActiveGeocodingProvider,
} from 'queries/geocodingProviders';
import type {
  GeocodingProvider,
  GeocodingProviderDtoInput,
  UpdateGeocodingProviderDtoInput,
} from "api/manager/geocodingProviders";
import { getGeocodingProviderType } from "data/geocodingProviderTypes";
import { toCamelCase } from 'utils/stringUtils';
import { LoadingContext } from 'LoadingContext';

/**
 * Dialog/form state for a geocoding provider. Merges an API
 * {@link GeocodingProvider} (when editing) with fresh-add defaults; all fields
 * are optional and the requirement set is enforced by the dialog's validate()
 * gate before save.
 */
export interface GeocodingProviderFormValues {
  geocodingProviderId?: string;
  name?: string;
  type?: number;
  endpointUri?: string;
  apiKey?: string | null;
  requestsPerSecond?: number;
  timeoutSeconds?: number;
  configurationJson?: string | null;
  active?: boolean;
}

/** A column descriptor / rendered row for the vendored providers `Table`. */
export interface GeocodingProviderTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type GeocodingProviderTableRow = Record<string, ReactNode>;
export interface GeocodingProviderTableData { columns: GeocodingProviderTableColumn[]; rows: GeocodingProviderTableRow[]; }

function useGeocodingProvidersTableData(
  fetchData: boolean,
  handleEditClick: (provider: GeocodingProviderFormValues) => void,
  handleDeleteClick: (geocodingProviderId: string) => void
) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const { setLoading } = useContext(LoadingContext);

  const providersQuery = useGeocodingProviders({ enabled: !!fetchData });
  const providers = providersQuery.data ?? [];
  const createProvider = useCreateGeocodingProvider();
  const updateProvider = useUpdateGeocodingProvider();
  const deleteProvider = useDeleteGeocodingProvider();
  const activateProvider = useSetActiveGeocodingProvider();

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(providersQuery.isFetching);
  }, [providersQuery.isFetching, setLoading]);

  const onSave = async (provider: GeocodingProviderFormValues) => {
    setLoading(true);
    try {
      // validate(['name','type','endpointUri']) gates this; assert the required
      // create/update input fields at the boundary.
      if (provider.geocodingProviderId) {
        await updateProvider.mutateAsync({
          geocodingProviderId: provider.geocodingProviderId,
          name: provider.name,
          type: provider.type,
          endpointUri: provider.endpointUri,
          apiKey: provider.apiKey,
          requestsPerSecond: provider.requestsPerSecond,
          timeoutSeconds: provider.timeoutSeconds,
          configurationJson: provider.configurationJson,
        } as UpdateGeocodingProviderDtoInput & { geocodingProviderId: string });
      } else {
        await createProvider.mutateAsync({
          name: provider.name,
          type: provider.type,
          endpointUri: provider.endpointUri,
          apiKey: provider.apiKey,
          requestsPerSecond: provider.requestsPerSecond,
          timeoutSeconds: provider.timeoutSeconds,
          configurationJson: provider.configurationJson,
          active: provider.active,
        } as GeocodingProviderDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const onDelete = async (geocodingProviderId: string) => {
    setLoading(true);
    try {
      await deleteProvider.mutateAsync(geocodingProviderId);
      setConfirmOpen(false);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const onActivate = async (geocodingProviderId: string) => {
    setLoading(true);
    try {
      await activateProvider.mutateAsync(geocodingProviderId);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const handleOpen = (provider: GeocodingProvider) => {
    handleEditClick(provider);
    setOpen(true);
  };

  const handleOpenDelete = (geocodingProviderId: string) => {
    handleDeleteClick(geocodingProviderId);
    setConfirmOpen(true);
  };

  const buildTableData = (rows: GeocodingProvider[]): GeocodingProviderTableData => ({
    columns: [
      { name: "name", title:t('geocodingProviders.name'), align: "left" },
      { name: "type", title:t('geocodingProviders.type'), align: "left" },
      { name: "endpointUri", title:t('geocodingProviders.endpointUri'), align: "left" },
      { name: "requestsPerSecond", title:t('geocodingProviders.requestsPerSecond'), align: "center" },
      { name: "active", title:t('geocodingProviders.active'), align: "center" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "id" }
    ],
    rows: rows.map(provider => {
      // Unknown type values must not render a dangling i18n key.
      const typeLabel = getGeocodingProviderType(provider.type);
      return {
      name: <Name name={provider.name} />,
      type: <Description description={typeLabel ? t(`geocodingProviders.types.${toCamelCase(typeLabel)}` as 'geocodingProviders.types.nominatim') : '-'} />,
      endpointUri: <Description description={provider.endpointUri} />,
      requestsPerSecond: <Name name={provider.requestsPerSecond} />,
      active: (
        <ArgonBadge
          variant="gradient"
          color={provider.active ? 'success' : 'secondary'}
          size="xs"
          container
          badgeContent={provider.active ? t('generic.yes') : t('generic.no')} />
      ),
      action: (
        <>
          <ArgonButton
              variant="text"
              color="dark"
              onClick={() => handleOpen(provider)}>
            <Icon>edit</Icon>&nbsp;{t('generic.edit')}
          </ArgonButton>
          <ArgonButton
            variant="text"
            color="error"
            onClick={() => handleOpenDelete(provider.geocodingProviderId)}>
            <Icon>delete</Icon>&nbsp;{t('generic.delete')}
          </ArgonButton>
          {!provider.active && (
            <ArgonButton
              variant="text"
              color="success"
              onClick={() => onActivate(provider.geocodingProviderId)}>
              <Icon>check_circle</Icon>&nbsp;{t('geocodingProviders.activate')}
            </ArgonButton>
          )}
        </>
      ),
      id: provider.geocodingProviderId
      };
    }),
  });

  const data = useMemo(
    () => buildTableData(providers),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [providers, t]
  );

  return {
    data,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen };
}

export default useGeocodingProvidersTableData;
