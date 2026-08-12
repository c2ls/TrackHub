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

import { useState, useEffect, useContext, useMemo, useRef } from 'react';
import Grid from "@mui/material/Grid";
import Icon from "@mui/material/Icon";
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonPagination from "components/ArgonPagination";
import DashboardLayout from "controls/LayoutContainers/DashboardLayout";
import DashboardNavbar from "controls/Navbars/DashboardNavbar";
import GeofenceEditor from 'layouts/geofencemanager/components/GeofenceEditor';
import type {
  AddGeofenceHandler,
  AddCircleGeofenceHandler,
  SaveGeofenceHandler,
  CancelGeofenceHandler,
  EditGeofenceHandler,
  RemoveGeofenceHandler,
} from 'layouts/geofencemanager/components/GeofenceEditor';
import Table from "controls/Tables/Table";
import CompactSelect from 'controls/Selects/CompactSelect';
import useForm from 'controls/Dialogs/useForm';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import GeofenceFormDialog from 'layouts/geofencemanager/GeofenceFormDialog';
import useGeofencesTableData from "layouts/geofencemanager/data/geofencesData";
import type { GeofenceFormValues, GeofenceListParams } from "layouts/geofencemanager/data/geofencesData";
import { geofenceTypes } from 'data/geofenceTypes';
import { toCamelCase } from 'utils/stringUtils';
import { getAccountSettings } from 'api/manager/settings';
import type { AccountSettings } from 'api/manager/settings';
import { notifyApiError } from 'api/core/errors';
import { LoadingContext } from 'LoadingContext';
import { useTranslation } from 'react-i18next';
import { useAuth } from "AuthContext";

/** Map-related account settings the geofence editor reads. */
interface GeofenceMapSettings {
  maps: string;
  mapsKey: string | null;
  refreshMapInterval: number;
}

const PAGE_SIZE = 10;
const ALL = 'all';

function GeofenceManager() {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const { setLoading } = useContext(LoadingContext);
  const [settings, setSettings] = useState<GeofenceMapSettings>({maps:'OSM', mapsKey:'', refreshMapInterval: 60});

  // List filters + paging (server-side).
  const [searchDraft, setSearchDraft] = useState('');
  // The status filter defaults to Active — retired (inactive) zones stay one click away.
  const [filters, setFilters] = useState<{ search: string; type: string; active: string }>({ search: '', type: ALL, active: 'active' });
  const [page, setPage] = useState(0);

  const listParams = useMemo<GeofenceListParams>(() => ({
    skip: page * PAGE_SIZE,
    take: PAGE_SIZE,
    type: filters.type === ALL ? null : Number(filters.type),
    active: filters.active === ALL ? null : filters.active === 'active',
    search: filters.search.trim() === '' ? null : filters.search.trim(),
  }), [page, filters]);

  const handleEditClick = (rowData: GeofenceFormValues) => {
    setValues(rowData);
    setErrors({});
  };

  const handleDeleteClick = (geofenceId: string) => {
    setToDelete(geofenceId);
  };

  const {
    data,
    totalCount,
    open,
    confirmOpen,
    onGet,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen} = useGeofencesTableData(handleEditClick, handleDeleteClick, listParams);

  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<GeofenceFormValues>({});
  const [toDelete, setToDelete] = useState<string | null>(null);
  const [selectedGeofence, setSelectedGeofence] = useState<string | null>(null);
  const { geofences, columns, rows } = data;

  const fetchSettings = async () => {
    setLoading(true);
    try {
      const settings: AccountSettings = await getAccountSettings();
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

  const addRef = useRef<AddGeofenceHandler | null>(null);
  const addCircleRef = useRef<AddCircleGeofenceHandler | null>(null);
  const saveRef = useRef<SaveGeofenceHandler | null>(null);
  const cancelRef = useRef<CancelGeofenceHandler | null>(null);
  const editingRef = useRef<EditGeofenceHandler | null>(null);
  const removeRef = useRef<RemoveGeofenceHandler | null>(null);

  const handleSave = () => {
    if (!validate(['name'])) return;

    const fieldErrors: Record<string, string> = {};
    if (values.dwellThresholdMinutes !== undefined && values.dwellThresholdMinutes !== null && values.dwellThresholdMinutes !== '') {
      const dwell = Number(values.dwellThresholdMinutes);
      if (!Number.isFinite(dwell) || dwell < 1 || dwell > 10080) {
        fieldErrors.dwellThresholdMinutes = t('geofence.validation.dwellRange');
      }
    }
    // For existing circles the radius comes from the (editable) dialog field, so
    // validate it up front. New circles get their radius from the map drag and
    // are bounds-checked server-side.
    if (!values.new && values.shape === 'circle') {
      const radius = Number(values.circleRadiusMeters);
      if (!Number.isFinite(radius) || radius < 10 || radius > 100000) {
        fieldErrors.circleRadiusMeters = t('geofence.validation.radiusRange');
      }
    }
    if (Object.keys(fieldErrors).length > 0) {
      setErrors(fieldErrors);
      return;
    }

    const next: GeofenceFormValues = { ...values };
    if (values.new) {
      const result = saveRef.current!(values.name!);
      next.geofenceId = result.id;
      next.shape = result.shape;
      if (result.shape === 'circle') {
        // leaflet-editable commits a circle on a single click (radius 0). Reject
        // a degenerate radius here instead of letting it fall through to the
        // polygon save path (which dereferences an undefined geom and throws).
        const radius = Number(result.radiusMeters);
        if (!Number.isFinite(radius) || radius < 10 || radius > 100000) {
          setErrors({ circleRadiusMeters: t('geofence.validation.radiusRange') });
          setOpen(true);
          return;
        }
        next.circleCenter = result.center ? { latitude: result.center.lat, longitude: result.center.lng } : null;
        next.circleRadiusMeters = result.radiusMeters ?? null;
        next.geom = undefined;
      } else {
        next.geom = { srid: 4326, coordinates: result.latlngs.map(coord => ({ latitude: coord.lat, longitude: coord.lng })) };
      }
    } else if (values.shape === 'circle') {
      next.geom = undefined;
    } else {
      next.geom = { srid: 4326, coordinates: values.coordinates!.map(coord => ({ latitude: coord.lat, longitude: coord.lng })) };
    }
    onSave(next);
  };

  const handleAdd = () => {
    if (addRef.current) {
      setValues({ color: 2, type: 1, active: true, new: true, shape: 'polygon', alertOnEntry: false, alertOnExit: false });
      setErrors({});
      addRef.current();
    }
  };

  const handleAddCircle = () => {
    if (addCircleRef.current) {
      setValues({ color: 2, type: 1, active: true, new: true, shape: 'circle', alertOnEntry: false, alertOnExit: false });
      setErrors({});
      addCircleRef.current();
    }
  };

  const handleCancel = () => {
    if (cancelRef.current) {
      cancelRef.current();
    }
  };

  const handleDelete = async () => {
    if (removeRef.current) {
      await onDelete(toDelete!)
      removeRef.current(toDelete!);
    }
  };

  const handleEdit = async () => {
    if (editingRef.current) {
      const handle = editingRef.current();
      if (!handle) return;
      setLoading(true);
      const geofence: GeofenceFormValues = await onGet(handle.id);
      geofence.new = false;
      geofence.shape = handle.shape;
      if (handle.shape === 'circle') {
        if (handle.center) geofence.circleCenter = { latitude: handle.center.lat, longitude: handle.center.lng };
        if (handle.radiusMeters != null) geofence.circleRadiusMeters = handle.radiusMeters;
      } else {
        geofence.coordinates = handle.latlngs;
      }
      handleEditClick(geofence);
      setLoading(false);
      setOpen(true);
    }
  };

  const handleFilterChange = (name: string, value: string | number) => {
    setPage(0);
    setFilters(prev => ({ ...prev, [name]: String(value) }));
  };

  // Debounce the free-text search into the server-side query params: the list is
  // server-paged, so apply ~350ms after the last keystroke (no button) and jump
  // back to the first page so the new results start from the top. The ref guards
  // against re-applying an unchanged value (e.g. the initial empty draft).
  const appliedSearchRef = useRef('');
  useEffect(() => {
    const handle = setTimeout(() => {
      if (appliedSearchRef.current === searchDraft) return;
      appliedSearchRef.current = searchDraft;
      setPage(0);
      setFilters(prev => ({ ...prev, search: searchDraft }));
    }, 350);
    return () => clearTimeout(handle);
  }, [searchDraft]);

  const typeOptions = useMemo(() => ([
    { value: ALL, label: t('geofence.allTypes') },
    ...geofenceTypes.map(type => ({
      value: String(type.value),
      label: t(`geofenceTypes.${toCamelCase(type.label)}` as 'geofenceTypes.office'),
    })),
  ]), [t]);

  const activeOptions = useMemo(() => ([
    { value: ALL, label: t('geofence.allActive') },
    { value: 'active', label: t('geofence.onlyActive') },
    { value: 'inactive', label: t('geofence.onlyInactive') },
  ]), [t]);

  // Deleting the last row of a non-first page shrinks totalCount below the
  // current page's start; clamp back to the last page that still has rows.
  useEffect(() => {
    if (page > 0 && page * PAGE_SIZE >= totalCount) {
      setPage(Math.max(0, Math.ceil(totalCount / PAGE_SIZE) - 1));
    }
  }, [totalCount, page]);

  const pageStart = totalCount === 0 ? 0 : page * PAGE_SIZE + 1;
  const pageEnd = Math.min((page + 1) * PAGE_SIZE, totalCount);
  const hasNext = pageEnd < totalCount;

  return (
    <DashboardLayout>
      <DashboardNavbar
        searchVisibility
        searchQuery={searchDraft}
        handleSearch={(e) => setSearchDraft(e.target.value)}
      />
      <ArgonBox py={1}>
        <Grid container spacing={3}>
          <Grid size={{xs: 12, lg: 9}} sx={{ position: 'relative', zIndex: 1 }}>
            {/* Compact single-row filter toolbar across the top of the map (same
                CompactSelect look as the dashboard filter bar). The filters drive
                the server-paged LIST (right column); the map keeps showing every
                geofence via the full (unpaged) set. The name search lives in the
                navbar search box, mirroring the dashboard. */}
            <ArgonBox
              display="flex"
              flexWrap="wrap"
              alignItems="center"
              gap={1}
              mb={1}
              sx={{ position: 'relative', zIndex: 2 }}>
              <CompactSelect
                name="type"
                value={filters.type}
                options={typeOptions}
                label={t('geofence.filterType')}
                onChange={handleFilterChange}
              />
              <CompactSelect
                name="active"
                value={filters.active}
                options={activeOptions}
                label={t('geofence.filterActive')}
                onChange={handleFilterChange}
              />
            </ArgonBox>
            <GeofenceEditor
               mapType={settings.maps as 'OSM' | 'Google'}
               mapKey={settings.mapsKey}
               geofences={geofences}
               selectedGeofence={selectedGeofence}
               handleSelected={setSelectedGeofence}
               setOpen={setOpen}
               addRef={addRef}
               addCircleRef={addCircleRef}
               saveRef={saveRef}
               cancelRef={cancelRef}
               editingRef={editingRef}
               removeRef={removeRef}
               handleAdd={handleAdd}
               handleAddCircle={handleAddCircle}
               handleEdit={handleEdit}
               height="calc(100vh - 236px)"
            />
          </Grid>
          <Grid size={{xs: 12, lg: 3}} sx={{ position: 'relative', zIndex: 2 }}>
            <Table
              columns={columns}
              rows={rows}
              selectedField="name"
              selected={selectedGeofence}
              handleSelected={setSelectedGeofence}
              scrollable={true}
              maxHeight="calc(100vh - 276px)"
              compact={true} />
            <ArgonBox display="flex" justifyContent="space-between" alignItems="center" mt={1}>
              <ArgonTypography variant="caption" color="secondary">
                {t('geofence.showing', { from: pageStart, to: pageEnd, total: totalCount })}
              </ArgonTypography>
              <ArgonPagination>
                <ArgonPagination item onClick={() => setPage(p => Math.max(0, p - 1))} disabled={page === 0}>
                  <Icon>keyboard_arrow_left</Icon>
                </ArgonPagination>
                <ArgonPagination item onClick={() => hasNext && setPage(p => p + 1)} disabled={!hasNext}>
                  <Icon>keyboard_arrow_right</Icon>
                </ArgonPagination>
              </ArgonPagination>
            </ArgonBox>
          </Grid>
        </Grid>
      </ArgonBox>
      <GeofenceFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSave}
        handleCancel={handleCancel}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />
      <ConfirmDialog
        title={t('geofence.deleteTitle')}
        message={t('geofence.deleteMessage')}
        open={confirmOpen}
        setOpen={setConfirmOpen}
        onConfirm={async() => await handleDelete()} />
    </DashboardLayout>
  );
}

export default GeofenceManager;
