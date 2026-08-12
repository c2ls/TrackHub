import { useState } from 'react';
import { getColor } from 'data/colors';
import { Polygon, Circle, InfoWindow } from '@react-google-maps/api';
import type { MapGeofence } from 'controls/Maps/core/mapTypes';

export interface GeofencePolygonProps {
    geofence: MapGeofence;
}

const GeofencePolygon = ({ geofence }: GeofencePolygonProps) => {
    const [tooltipPosition, setTooltipPosition] = useState<{ lat: number; lng: number } | null>(null);

    const shapeOptions = {
        strokeColor: '#FF0000',
        strokeOpacity: 0.8,
        strokeWeight: 1,
        fillColor: getColor(geofence.color),
        fillOpacity: 0.35
    };

    const isCircle = !!(geofence.circleCenter && geofence.circleRadiusMeters);

    return (
        <>
            {isCircle ? (
                <Circle
                    center={{ lat: geofence.circleCenter!.latitude, lng: geofence.circleCenter!.longitude }}
                    radius={geofence.circleRadiusMeters!}
                    options={shapeOptions}
                    onMouseOver={(e) => setTooltipPosition({ lat: e.latLng!.lat(), lng: e.latLng!.lng() })}
                    onMouseOut={() => setTooltipPosition(null)} />
            ) : (
                <Polygon
                    path={geofence.geom.coordinates.map(({ latitude, longitude }) => ({ lat: latitude, lng: longitude }))}
                    options={shapeOptions}
                    onMouseOver={(e) => setTooltipPosition({ lat: e.latLng!.lat(), lng: e.latLng!.lng() })}
                    onMouseOut={() => setTooltipPosition(null)} />
            )}
            {tooltipPosition && (
                <InfoWindow position={tooltipPosition} options={{ disableAutoPan: true }}>
                    <div style={{ fontSize: '12px', fontWeight: 'bold' }}>
                        {geofence.name}
                    </div>
                </InfoWindow>
            )}
        </>
    );
};

export default GeofencePolygon;
