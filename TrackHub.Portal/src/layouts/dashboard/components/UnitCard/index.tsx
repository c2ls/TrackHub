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

import { useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import Card from '@mui/material/Card';
import Icon from '@mui/material/Icon';
import IconButton from '@mui/material/IconButton';
import { useTranslation } from 'react-i18next';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import StatusGlyph from 'layouts/dashboard/components/StatusGlyph';
import { getWhatsAppShareUrl, getStreetViewUrl, getRelativeTimeText } from 'controls/Maps/utils/popupUtils';
import { reverseGeocode } from 'api/router/router';
import type { Address, Position } from 'api/router/router';
import type { UnitStatus } from 'layouts/dashboard/utils/dashboard';
import { formatDateTime } from 'utils/dateUtils';
import { cleanString } from 'utils/stringUtils';

interface UnitCardProps {
  position: Position;
  status: UnitStatus;
  onClose: () => void;
}

const STATUS_BADGE = { moving: 'success', stopped: 'warning', offline: 'secondary' } as const;
const COMPASS = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'];

const openExternal = (url: string) => window.open(url, '_blank', 'noopener,noreferrer');

function Fact({ label, children }: { label: string; children: ReactNode }) {
  return (
    <ArgonBox minWidth={0}>
      <ArgonTypography variant="caption" color="secondary" display="block" noWrap>
        {label}
      </ArgonTypography>
      <ArgonTypography variant="button" fontWeight="medium" display="block" noWrap>
        {children}
      </ArgonTypography>
    </ArgonBox>
  );
}

function UnitCard({ position, status, onClose }: UnitCardProps) {
  const { t } = useTranslation();
  const [resolved, setResolved] = useState<Address | null>(null);
  const [resolving, setResolving] = useState<'idle' | 'pending' | 'failed'>('idle');

  useEffect(() => {
    setResolved(null);
    setResolving('idle');
  }, [position.transporterId, position.latitude, position.longitude]);

  // Silent op: a failed reverse-geocode must not toast, so the error is swallowed here and the
  // button reports it.
  const resolveAddress = async () => {
    setResolving('pending');
    const result = await reverseGeocode(position.latitude, position.longitude, position.transporterId ?? null)
      .catch(() => null);
    if (result && (result.address || result.city || result.state || result.country)) {
      setResolved(result);
      setResolving('idle');
    } else {
      setResolving('failed');
    }
  };

  const place = position.address ? position : resolved ?? position;
  const locality = [place.city, place.state, place.country].filter(Boolean).join(', ');
  const relative = getRelativeTimeText(position.deviceDateTime, t);
  const { attributes } = position;
  const course = position.course ?? 0;

  const extras: [string, string][] = [];
  if (attributes?.ignition != null) {
    extras.push([t('transporterMap.accStatus'), attributes.ignition ? t('transporterMap.accOn') : t('transporterMap.accOff')]);
  }
  if (attributes?.mileage != null) {
    extras.push([t('transporterMap.mileage'), `${(attributes.mileage / 1000).toFixed(1)} km`]);
  }
  if (attributes?.hourmeter != null) {
    // Already in hours: the only producer is an engine-hour meter reading.
    extras.push([t('transporterMap.hourmeter'), `${Number(attributes.hourmeter).toFixed(1)} h`]);
  }
  if (attributes?.temperature != null) {
    extras.push([t('transporterMap.temperature'), `${attributes.temperature} °C`]);
  }
  if (attributes?.satellites != null) {
    extras.push([t('transporterMap.satellites'), String(attributes.satellites)]);
  }

  return (
    <Card sx={{ p: 2, gap: 1.5, boxShadow: ({ boxShadows }) => boxShadows.lg }} data-testid="unit-card">
      <ArgonBox display="flex" alignItems="center" gap={1}>
        <StatusGlyph status={status} />
        <ArgonTypography variant="h6" fontWeight="bold" noWrap>
          {position.deviceName}
        </ArgonTypography>
        <ArgonBadge badgeContent={t(`transporterMap.${status}`)} color={STATUS_BADGE[status]} size="xs" container />
        {position.transporterType && (
          <ArgonTypography variant="caption" color="secondary" noWrap>
            {t(`transporterTypes.${cleanString(position.transporterType)}` as 'transporterTypes.car', {
              defaultValue: position.transporterType,
            })}
          </ArgonTypography>
        )}
        <IconButton size="small" onClick={onClose} aria-label={t('generic.close')} sx={{ ml: 'auto' }}>
          <Icon fontSize="small">close</Icon>
        </IconButton>
      </ArgonBox>

      <ArgonBox display="grid" gridTemplateColumns="repeat(4, minmax(0, 1fr))" gap={1.5}>
        <Fact label={t('transporterMap.speedTitle')}>{position.speed} km/h</Fact>
        <Fact label={t('transporterMap.heading')}>
          {status === 'moving' ? `${COMPASS[Math.round(course / 45) % 8]} ${Math.round(course)}°` : '-'}
        </Fact>
        <Fact label={t('transporterMap.altitude')}>
          {position.altitude != null ? `${Math.round(position.altitude).toLocaleString()} m` : '-'}
        </Fact>
        <Fact label={t('transporterMap.lastReport')}>{relative || '-'}</Fact>
      </ArgonBox>

      {extras.length > 0 && (
        <ArgonBox display="flex" flexWrap="wrap" columnGap={2} rowGap={0.5}>
          {extras.map(([label, value]) => (
            <ArgonTypography key={label} variant="caption" color="secondary">
              {label}: <ArgonTypography component="span" variant="caption" fontWeight="medium" color="dark">{value}</ArgonTypography>
            </ArgonTypography>
          ))}
        </ArgonBox>
      )}

      <ArgonBox display="flex" alignItems="flex-start" gap={1}>
        <Icon fontSize="small" color="secondary">place</Icon>
        <ArgonBox minWidth={0}>
          <ArgonTypography variant="button" fontWeight="regular" display="block">
            {place.address || locality || `${position.latitude.toFixed(6)}, ${position.longitude.toFixed(6)}`}
          </ArgonTypography>
          <ArgonTypography variant="caption" color="secondary" display="block">
            {formatDateTime(position.deviceDateTime)}
          </ArgonTypography>
        </ArgonBox>
      </ArgonBox>

      <ArgonBox display="flex" flexWrap="wrap" gap={1}>
        {!place.address && (
          <ArgonButton
            variant="outlined"
            color="primary"
            size="small"
            disabled={resolving === 'pending'}
            onClick={resolveAddress}
          >
            {resolving === 'pending'
              ? t('transporterMap.resolvingAddress')
              : resolving === 'failed'
                ? t('transporterMap.addressUnavailable')
                : t('transporterMap.resolveAddress')}
          </ArgonButton>
        )}
        <ArgonButton
          onClick={() => openExternal(getWhatsAppShareUrl(position.latitude, position.longitude, position.deviceName))}
          variant="outlined"
          color="dark"
          size="small"
        >
          <Icon sx={{ mr: 0.5 }}>share</Icon>
          {t('transporterMap.shareWhatsApp')}
        </ArgonButton>
        <ArgonButton
          onClick={() => openExternal(getStreetViewUrl(position.latitude, position.longitude))}
          variant="outlined"
          color="dark"
          size="small"
        >
          <Icon sx={{ mr: 0.5 }}>streetview</Icon>
          {t('transporterMap.streetView')}
        </ArgonButton>
      </ArgonBox>
    </Card>
  );
}

export default UnitCard;
