import { useEffect } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import { useTranslation } from 'react-i18next';

type ControlCorner = 'topleft' | 'topright' | 'bottomleft' | 'bottomright';

interface OSMStatsToggleProps {
  position?: ControlCorner;
  toggleStats?: () => void;
  showStats?: boolean;
}

const OSMStatsToggle = ({ position = 'topleft', toggleStats, showStats }: OSMStatsToggleProps) => {
  const { t } = useTranslation();
  const map = useMap();
  useEffect(() => {
    if (!map) return;
    const StatsControl = L.Control.extend({
      options: { position },
      onAdd: function () {
        const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
        container.style.background = 'white';
        container.style.cursor = 'pointer';
        container.style.display = 'flex';
        container.style.alignItems = 'center';
        container.style.justifyContent = 'center';
        container.style.height = '40px';
        container.style.width = '40px';

        const button = L.DomUtil.create('a', '', container);
        button.innerHTML = '📊';
        button.title = showStats ? t("utilsmap.hideStatistics") : t("utilsmap.showStatistics");
        button.style.display = 'flex';
        button.style.alignItems = 'center';
        button.style.justifyContent = 'center';
        button.style.width = '100%';
        button.style.height = '100%';
        button.style.fontSize = '20px';
        button.style.textDecoration = 'none';
        button.style.color = '#000';
        button.style.background = 'transparent';
        button.style.border = 'none';

        L.DomEvent.on(button, 'click', (e) => {
          L.DomEvent.stopPropagation(e);
          L.DomEvent.preventDefault(e);
          toggleStats && toggleStats();
        });

        return container;
      }
    });

    const statsControl: L.Control = new StatsControl();
    statsControl.addTo(map);

    return () => { statsControl.remove(); };
  }, [map, toggleStats, showStats]);

  return null;
};

export default OSMStatsToggle;
