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

import type { Dispatch, SetStateAction } from 'react';
import Slider from '@mui/material/Slider';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import { useTranslation } from 'react-i18next';

const SPEED_MULTIPLIERS = [1, 2, 4, 8];

interface PlaybackControlsProps {
  playing: boolean;
  toggle: () => void;
  speed: number;
  setSpeed: Dispatch<SetStateAction<number>>;
  progress: number;
  seek: (value: number) => void;
  disabled?: boolean;
}

// Playback controls for the selected trip: play/pause, speed multiplier and
// a timeline slider driving the playback marker. Purely client-side.
function PlaybackControls({ playing, toggle, speed, setSpeed, progress, seek, disabled = false }: PlaybackControlsProps) {
  const { t } = useTranslation();

  return (
    <ArgonBox display="flex" alignItems="center" gap={2} px={1} py={1} flexWrap="wrap">
      <ArgonButton
        size="small"
        color="primary"
        onClick={toggle}
        disabled={disabled}>
        {playing ? t('replay.pause') : t('replay.play')}
      </ArgonButton>
      <ToggleButtonGroup
        size="small"
        exclusive
        value={speed}
        aria-label={t('replay.speed')}
        onChange={(_event, value: number | null) => value && setSpeed(value)}>
        {SPEED_MULTIPLIERS.map(multiplier => (
          <ToggleButton key={multiplier} value={multiplier}>
            {multiplier}x
          </ToggleButton>
        ))}
      </ToggleButtonGroup>
      <ArgonBox flex={1} minWidth="160px" px={1}>
        <Slider
          size="small"
          min={0}
          max={100}
          value={Math.round(progress * 100)}
          disabled={disabled}
          aria-label={t('replay.timeline')}
          // A single Slider always emits a number; narrow from MUI's number | number[].
          onChange={(_event, value) => seek((value as number) / 100)}
        />
      </ArgonBox>
    </ArgonBox>
  );
}

export default PlaybackControls;
