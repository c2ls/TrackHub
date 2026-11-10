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

import { useMemo } from 'react';
import type { Dispatch, PointerEvent, SetStateAction } from 'react';
import Card from '@mui/material/Card';
import Icon from '@mui/material/Icon';
import IconButton from '@mui/material/IconButton';
import Slider from '@mui/material/Slider';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import { useTheme } from '@mui/material/styles';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import { useTranslation } from 'react-i18next';
import { formatDateTime } from 'utils/dateUtils';
import type { Timeline } from 'layouts/dashboard/utils/playback';

const SPEED_MULTIPLIERS = [1, 2, 4, 8];
const CHART_HEIGHT = 30;

interface PlaybackControlsProps {
  playing: boolean;
  toggle: () => void;
  speed: number;
  setSpeed: Dispatch<SetStateAction<number>>;
  progress: number;
  seek: (value: number) => void;
  timeline: Timeline | null;
  disabled?: boolean;
}

// Playback of the selected trip: play/pause, speed multiplier, and the trip's speed profile,
// which doubles as the timeline — dragging across it moves the playback marker. Purely client-side.
function PlaybackControls({ playing, toggle, speed, setSpeed, progress, seek, timeline, disabled = false }: PlaybackControlsProps) {
  const { t } = useTranslation();
  const { palette, functions } = useTheme();

  const profile = useMemo(() => {
    if (!timeline) return null;
    const speeds = timeline.points.map((point) => point.speed ?? 0);
    const peak = Math.max(...speeds, 1);
    const line = timeline.stamps
      .map((stamp, index) =>
        `${index ? 'L' : 'M'}${(stamp * 100).toFixed(2)} ${(CHART_HEIGHT - (speeds[index] / peak) * (CHART_HEIGHT - 2)).toFixed(2)}`)
      .join(' ');
    return {
      peak,
      line,
      area: `${line} L100 ${CHART_HEIGHT} L0 ${CHART_HEIGHT} Z`,
      start: new Date(timeline.points[0].deviceDateTime).getTime(),
      end: new Date(timeline.points[timeline.points.length - 1].deviceDateTime).getTime(),
    };
  }, [timeline]);

  // The reading under the cursor: the last recorded point at or before it.
  const current = useMemo(() => {
    if (!timeline) return null;
    let index = 0;
    while (index < timeline.stamps.length - 1 && timeline.stamps[index + 1] <= progress) index += 1;
    return timeline.points[index];
  }, [timeline, progress]);

  const seekFromPointer = (event: PointerEvent<HTMLDivElement>) => {
    if (disabled || event.buttons !== 1) return;
    const box = event.currentTarget.getBoundingClientRect();
    seek((event.clientX - box.left) / box.width);
  };

  return (
    <Card sx={{ mt: 1.5, px: 2, py: 1.5, gap: 1 }}>
      <ArgonBox display="flex" alignItems="center" gap={2} flexWrap="wrap">
        <IconButton
          onClick={toggle}
          disabled={disabled}
          aria-label={playing ? t('replay.pause') : t('replay.play')}
          sx={{
            width: 40,
            height: 40,
            backgroundColor: palette.primary.main,
            color: palette.white.main,
            '&:hover': { backgroundColor: palette.primary.main },
          }}>
          <Icon>{playing ? 'pause' : 'play_arrow'}</Icon>
        </IconButton>
        {profile && (
          <ArgonBox>
            <ArgonTypography variant="h6" fontWeight="bold" lineHeight={1.2}>
              {formatDateTime(new Date(profile.start + (profile.end - profile.start) * progress))}
            </ArgonTypography>
            <ArgonTypography variant="caption" color="secondary">
              {current?.speed ?? 0} km/h
            </ArgonTypography>
          </ArgonBox>
        )}
        <ToggleButtonGroup
          size="small"
          exclusive
          value={speed}
          aria-label={t('replay.speed')}
          onChange={(_event, value: number | null) => value && setSpeed(value)}
          sx={{ ml: 'auto', '& .MuiToggleButton-root': { py: 0.5 } }}>
          {SPEED_MULTIPLIERS.map(multiplier => (
            <ToggleButton key={multiplier} value={multiplier}>
              {multiplier}x
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
      </ArgonBox>
      {profile && (
        <ArgonBox
          position="relative"
          height={64}
          onPointerDown={seekFromPointer}
          onPointerMove={seekFromPointer}
          sx={{ cursor: disabled ? 'default' : 'ew-resize', touchAction: 'none' }}>
          <svg
            viewBox={`0 0 100 ${CHART_HEIGHT}`}
            preserveAspectRatio="none"
            role="img"
            aria-label={t('replay.exportSpeed')}
            style={{ position: 'absolute', inset: 0, width: '100%', height: '100%' }}>
            <path d={profile.area} fill={functions.rgba(palette.info.main, 0.16)} />
            <path d={profile.line} fill="none" stroke={palette.info.main} strokeWidth={2} vectorEffect="non-scaling-stroke" />
          </svg>
          <ArgonBox
            position="absolute"
            top={0}
            bottom={0}
            width="2px"
            sx={{ left: `${progress * 100}%`, backgroundColor: palette.dark.main }} />
          <ArgonTypography
            variant="caption"
            color="secondary"
            sx={{ position: 'absolute', top: 0, left: 0, pointerEvents: 'none' }}>
            {profile.peak} km/h
          </ArgonTypography>
        </ArgonBox>
      )}
      <Slider
        size="small"
        min={0}
        max={100}
        value={Math.round(progress * 100)}
        disabled={disabled}
        aria-label={t('replay.timeline')}
        // A single Slider always emits a number; narrow from MUI's number | number[].
        onChange={(_event, value) => seek((value as number) / 100)}
        sx={{ py: 0.5 }}
      />
    </Card>
  );
}

export default PlaybackControls;
