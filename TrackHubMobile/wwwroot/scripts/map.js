window.initMap = (positions) => {
    var map = L.map('map').setView([37.7749, -122.4194], 5);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    positions.forEach(p => {
        L.marker([p.lat, p.lng]).addTo(map).bindPopup('Marker');
    });
};