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

import { createContext, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import Card from '@mui/material/Card';
import { useLocation } from 'react-router';
import { useTranslation } from 'react-i18next';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';

interface SectionEntry {
  key: string;
  title: string;
  group?: string;
  /** Position of the group on the screen, so a group whose sections arrive late keeps its place. */
  groupOrder?: number;
}

interface SectionNavValue {
  active: string | null;
  register: (entry: SectionEntry) => void;
  unregister: (key: string) => void;
}

/**
 * Present on screens laid out as a section list beside one open section. A `TableAccordion`
 * rendered under it lists itself in the navigation and shows its content only while selected.
 */
export const SectionNavContext = createContext<SectionNavValue | null>(null);

export const SectionGroupContext = createContext<{ label: string; order: number } | undefined>(undefined);

/** Names the navigation group of the sections rendered inside it; `order` is its place on the screen. */
export function SectionGroup({ label, order, children }: { label: string; order: number; children: ReactNode }) {
  const value = useMemo(() => ({ label, order }), [label, order]);
  return <SectionGroupContext.Provider value={value}>{children}</SectionGroupContext.Provider>;
}

const readRemembered = (storageKey: string): string | null => {
  try {
    return window.sessionStorage.getItem(storageKey);
  } catch {
    return null;
  }
};

export function SectionNavLayout({ children }: { children: ReactNode }) {
  const { t } = useTranslation();
  const [entries, setEntries] = useState<SectionEntry[]>([]);
  const [active, setActive] = useState<string | null>(null);
  // Until the user picks (or closes) a section, the one they last had open on this screen is
  // shown, or the first one listed.
  const chosenRef = useRef(false);
  const storageKey = `trackhub.section.${useLocation().pathname}`;
  const rememberedRef = useRef(readRemembered(storageKey));

  const register = useCallback((entry: SectionEntry) => {
    setEntries((current) =>
      current.some((item) => item.key === entry.key)
        ? current.map((item) => (item.key === entry.key ? entry : item))
        : [...current, entry]
    );
  }, []);

  const unregister = useCallback(
    (key: string) => setEntries((current) => current.filter((item) => item.key !== key)),
    []);

  useEffect(() => {
    if (chosenRef.current || entries.length === 0) return;
    const remembered = entries.find((item) => item.key === rememberedRef.current);
    if (remembered) {
      if (active !== remembered.key) setActive(remembered.key);
    } else if (!entries.some((item) => item.key === active)) {
      setActive(entries[0].key);
    }
  }, [entries, active]);

  const value = useMemo(() => ({ active, register, unregister }), [active, register, unregister]);

  // Sections register as they mount; a group whose sections arrive late (feature flags) is
  // still listed as one block, at its own place on the screen.
  const groups = useMemo(() => {
    const byGroup = new Map<string, SectionEntry[]>();
    [...entries]
      .sort((a, b) => (a.groupOrder ?? 0) - (b.groupOrder ?? 0))
      .forEach((entry) => {
        const label = entry.group ?? '';
        byGroup.set(label, [...(byGroup.get(label) ?? []), entry]);
      });
    return [...byGroup.entries()];
  }, [entries]);

  const select = (key: string) => {
    chosenRef.current = true;
    try {
      window.sessionStorage.setItem(storageKey, key);
    } catch {
      // Storage blocked: the choice simply is not remembered.
    }
    setActive((current) => (current === key ? null : key));
  };

  return (
    <SectionNavContext.Provider value={value}>
      <ArgonBox display="flex" flexWrap="wrap" alignItems="flex-start" gap={3}>
        <Card
          component="nav"
          sx={({ breakpoints }) => ({
            flex: '1 1 14rem',
            p: 1,
            maxHeight: '40vh',
            overflowY: 'auto',
            [breakpoints.up('md')]: {
              flex: '0 0 15rem',
              position: 'sticky',
              top: 80,
              maxHeight: 'calc(100vh - 104px)',
            },
          })}
        >
          {groups.map(([label, items]) => (
            <ArgonBox key={label} mb={1}>
              {label && (
                <ArgonTypography
                  variant="caption"
                  fontWeight="bold"
                  textTransform="uppercase"
                  color="secondary"
                  display="block"
                  sx={{ letterSpacing: '0.06em', px: 1.25, pt: 1, pb: 0.5 }}
                >
                  {label}
                </ArgonTypography>
              )}
              {items.map(({ key, title }) => {
                const selected = key === active;
                return (
                  <ArgonBox
                    key={key}
                    component="button"
                    onClick={() => select(key)}
                    aria-expanded={selected}
                    data-testid={`section-${key}-header`}
                    sx={({ palette, borders, typography, functions: { rgba, pxToRem } }) => ({
                      display: 'block',
                      width: '100%',
                      minHeight: pxToRem(34),
                      px: 1.25,
                      py: 0.75,
                      border: 0,
                      borderRadius: borders.borderRadius.md,
                      background: selected ? rgba(palette.info.main, 0.12) : 'transparent',
                      color: selected ? palette.info.main : palette.text.main,
                      fontFamily: 'inherit',
                      fontSize: typography.size.sm,
                      fontWeight: selected ? typography.fontWeightBold : typography.fontWeightMedium,
                      textAlign: 'left',
                      cursor: 'pointer',
                      '&:hover': {
                        background: selected ? rgba(palette.info.main, 0.12) : palette.grey[100],
                      },
                    })}
                  >
                    {title}
                  </ArgonBox>
                );
              })}
            </ArgonBox>
          ))}
        </Card>
        <ArgonBox flex="999 1 28rem" minWidth={0}>
          {children}
          {active === null && entries.length > 0 && (
            <ArgonTypography variant="button" fontWeight="regular" color="secondary">
              {t('generic.selectSection')}
            </ArgonTypography>
          )}
        </ArgonBox>
      </ArgonBox>
    </SectionNavContext.Provider>
  );
}
