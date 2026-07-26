#!/usr/bin/env node
/**
 * Builds a Colombian toll catalog for TrackHub from the official INVÍAS open data,
 * as a single PostgreSQL script you run once against the `TrackHub` database.
 *
 * WHY A SCRIPT AND NOT SEED DATA
 * ------------------------------
 * Spec 11 §7.7 ships ZERO toll rows on purpose: tariffs are set by resolution and
 * change at least annually (INVÍAS raised them 5.10% by IPC in January 2026), so
 * anything baked into a migration is wrong the moment it ships. This fetches the
 * current published figures instead.
 *
 * SOURCE
 *   https://www.datos.gov.co/Transporte/Peajes/68qj-5xux
 *   "Peajes registrados sobre La Red Vial Nacional" — Instituto Nacional de Vías
 *   (INVÍAS), flagged "Oficial". Covers INVÍAS-operated and ANI-concession
 *   stations. Rates in COP.
 *
 * USAGE
 *   node fetch-toll-catalog.mjs [--effective-from YYYY-MM-DD] [--out FILE]
 *
 *   psql -h localhost -U postgres -d TrackHub -f toll-catalog.sql
 *
 * The script is one transaction. Running it twice fails on the unique indexes and
 * rolls back rather than duplicating the catalog — a duplicated station would be
 * matched twice by ST_DWithin and charge its toll twice in every estimate.
 */

import { writeFileSync } from 'node:fs';
import { randomUUID } from 'node:crypto';

const DATASET = 'https://www.datos.gov.co/resource/68qj-5xux.json';
const PAGE_SIZE = 1000;

/**
 * Colombia's bounding box, San Andrés and Providencia included. A station outside
 * it is a data error, and it matters more than it looks: `ST_DWithin` simply never
 * matches a station in the wrong place, so a bad coordinate does not raise an
 * error — it silently drops that toll out of every estimate.
 */
const BOUNDS = { minLat: -4.3, maxLat: 13.5, minLng: -82.0, maxLng: -66.8 };

/**
 * The five standard INVÍAS categories, plus VI and VII which the dataset prices at
 * 88 of its 179 stations.
 *
 * I–V are the definitions INVÍAS publishes. VI and VII are deliberately vague:
 * they cover special and articulated freight and their exact axle rules vary by
 * concession, so they are left for the operator to confirm against the resolution
 * that applies rather than guessed at here.
 */
const VEHICLE_CLASSES = [
  ['I', 'Categoría I', 'Automóviles, camperos, camionetas y microbuses con ejes de llanta sencilla.', 10],
  ['II', 'Categoría II', 'Buses y busetas con eje trasero de doble llanta, y camiones de dos ejes.', 20],
  ['III', 'Categoría III', 'Camiones y vehículos de pasajeros de tres y cuatro ejes.', 30],
  ['IV', 'Categoría IV', 'Camiones de cinco ejes.', 40],
  ['V', 'Categoría V', 'Camiones de seis ejes.', 50],
  ['VI', 'Categoría VI', 'Carga especial. CONFIRMAR la definición contra la resolución vigente.', 60],
  ['VII', 'Categoría VII', 'Carga especial. CONFIRMAR la definición contra la resolución vigente.', 70],
];

/** Dataset column → vehicle class code. */
const TARIFF_COLUMNS = [
  ['categoria_i', 'I'],
  ['categoria_ii', 'II'],
  ['categoria_iii', 'III'],
  ['categoria_iv', 'IV'],
  ['categoria_v', 'V'],
  ['categoria_vi', 'VI'],
  ['categoria_vii', 'VII'],
];

/** Column limits, from the entity configuration (spec 11 §6.2). */
const MAX = { code: 40, name: 200, roadName: 200, direction: 50, operator: 200, notes: 1000 };

function arg(flag, fallback) {
  const i = process.argv.indexOf(flag);
  return i !== -1 && process.argv[i + 1] ? process.argv[i + 1] : fallback;
}

/** A SQL string literal, or NULL for anything blank. Quotes are doubled, not stripped. */
function sql(value, maxLength) {
  if (value === null || value === undefined) return 'NULL';
  const cleaned = String(value).replace(/\s+/g, ' ').trim();
  if (!cleaned) return 'NULL';
  const clipped = cleaned.length > maxLength ? cleaned.slice(0, maxLength).trim() : cleaned;
  return `'${clipped.replace(/'/g, "''")}'`;
}

/**
 * Reads a station's position, preferring the GeoJSON `point` over the scalar
 * `latitud`/`longitud` columns.
 *
 * THE SCALAR COLUMNS ARE CORRUPT IN THE SOURCE for a large minority of records:
 * both hold the LATITUDE. GUAICO publishes `latitud = longitud = 5.10567543`
 * while its `point` correctly reads [-75.760278, 5.1059170]. Trusting the scalars
 * put 145 of 179 stations on the prime meridian in Africa — caught only because
 * the bounds check rejected them. `point` is GeoJSON, so its order is
 * [longitude, latitude]; reading it the other way round plants the whole catalog
 * in the wrong hemisphere.
 */
function readPosition(record) {
  const geo = record.point?.coordinates;
  if (Array.isArray(geo) && geo.length >= 2) {
    const [lng, lat] = geo.map(Number);
    if (Number.isFinite(lat) && Number.isFinite(lng)) return { lat, lng };
  }

  const lat = Number.parseFloat(record.latitud);
  const lng = Number.parseFloat(record.longitud);

  // The corruption signature is identical values. Rejected rather than guessed at.
  if (Number.isFinite(lat) && Number.isFinite(lng) && lat !== lng) return { lat, lng };

  return null;
}

/** Socrata pages at 1000; loop until a short page comes back. */
async function fetchAll() {
  const records = [];
  for (let offset = 0; ; offset += PAGE_SIZE) {
    const url = `${DATASET}?$limit=${PAGE_SIZE}&$offset=${offset}`;
    const response = await fetch(url, { headers: { accept: 'application/json' } });
    if (!response.ok) {
      throw new Error(`datos.gov.co returned ${response.status} ${response.statusText} for ${url}`);
    }
    const page = await response.json();
    records.push(...page);
    if (page.length < PAGE_SIZE) return records;
  }
}

const effectiveFrom = arg('--effective-from', '2026-01-01');
if (!/^\d{4}-\d{2}-\d{2}$/.test(effectiveFrom)) {
  console.error(`--effective-from must be YYYY-MM-DD, got "${effectiveFrom}"`);
  process.exit(1);
}
const outFile = arg('--out', 'toll-catalog.sql');

console.log(`Fetching ${DATASET} …`);
const records = await fetchAll();
console.log(`  ${records.length} records published.`);

const stationValues = [];
const tariffValues = [];
const skipped = [];
const seenCodes = new Set();

for (const record of records) {
  const rawName = String(record.nombre_peaje ?? '').trim();
  if (!rawName) {
    skipped.push('(unnamed record) — no nombre_peaje');
    continue;
  }

  const position = readPosition(record);
  if (!position) {
    skipped.push(`${rawName} — no usable coordinates (point absent, scalars missing or duplicated)`);
    continue;
  }

  const { lat, lng } = position;
  if (lat === 0 && lng === 0) {
    skipped.push(`${rawName} — null island (0, 0)`);
    continue;
  }
  if (lat < BOUNDS.minLat || lat > BOUNDS.maxLat || lng < BOUNDS.minLng || lng > BOUNDS.maxLng) {
    skipped.push(`${rawName} — coordinates outside Colombia (${lat}, ${lng})`);
    continue;
  }

  const priced = TARIFF_COLUMNS.map(([column, code]) => [code, Number.parseFloat(record[column])]).filter(
    ([, amount]) => Number.isFinite(amount) && amount > 0
  );

  if (priced.length === 0) {
    // Importing a station with nothing priced would produce a station that matches
    // routes and prices nothing — the PartialNoTariff signal, caused by us rather
    // than by a real gap in the catalog. Several of these are named NO OPERATIVO.
    skipped.push(`${rawName} — no category is priced above zero`);
    continue;
  }

  // The dataset's own toll code, de-duplicated: the station key is (name, code),
  // so a repeat would collide. A second station keeps its name and loses the code.
  let code = String(record.c_digo_peaje ?? '').trim();
  if (code && seenCodes.has(code)) {
    skipped.push(`${rawName} — duplicate station code ${code}, inserted without a code`);
    code = '';
  }
  if (code) seenCodes.add(code);

  const stationId = randomUUID();

  stationValues.push(
    `  ('${stationId}', ${sql(rawName, MAX.name)}, ${sql(code, MAX.code)}, ` +
      `ST_SetSRID(ST_MakePoint(${lng.toFixed(7)}, ${lat.toFixed(7)}), 4326), ` +
      `'CO', NULL, ${sql(record.sector, MAX.roadName)}, ${sql(record.sentido, MAX.direction)}, ` +
      `${sql(record.responsable, MAX.operator)}, ${sql(record.ubicaci_n, MAX.notes)})`
  );

  for (const [classCode, amount] of priced) {
    // Whole pesos: COP has no minor unit in practice and the source publishes
    // integers, so a decimal here would only invite rounding noise.
    tariffValues.push(`  ('${randomUUID()}', '${stationId}', '${classCode}', ${Math.round(amount)})`);
  }
}

const classValues = VEHICLE_CLASSES.map(
  ([code, name, description, sortOrder]) =>
    `  ('${randomUUID()}', '${code}', ${sql(name, 100)}, ${sql(description, 500)}, ${sortOrder})`
).join(',\n');

const script = `-- TrackHub toll catalog — Colombia
--
-- Source:        ${DATASET}
--                "Peajes registrados sobre La Red Vial Nacional" (INVÍAS, Oficial)
-- Generated:     ${new Date().toISOString()}
-- effectiveFrom: ${effectiveFrom}
--
-- Stations: ${stationValues.length}   Tariffs: ${tariffValues.length}   Skipped: ${skipped.length}
--
-- Run once:  psql -h localhost -U postgres -d TrackHub -f ${outFile}
--
-- One transaction. Running it twice fails on the unique indexes and rolls back
-- rather than duplicating the catalog — a duplicated station is matched twice by
-- ST_DWithin and charges its toll twice in every estimate.
--
-- Tariffs change by resolution. To load new rates later, use the admin UI or the
-- CSV import: createTollTariff CLOSES the open row and inserts a new one, so a
-- past trip's estimate stays reproducible. Re-running THIS script is not the way
-- to update prices.
${skipped.length ? `--\n-- Skipped records:\n${skipped.map((s) => `--   - ${s}`).join('\n')}\n` : ''}
BEGIN;

INSERT INTO trip.toll_vehicle_classes
  (id, code, name, description, sortorder, active, "Created", "CreatedBy", "LastModified", "LastModifiedBy")
SELECT v.id::uuid, v.code, v.name, v.description, v.sortorder, TRUE, now(), 'invias-open-data', now(), 'invias-open-data'
FROM (VALUES
${classValues}
) AS v(id, code, name, description, sortorder);

INSERT INTO trip.toll_stations
  (id, name, code, point, country, region, roadname, direction, operator, notes, active,
   "Created", "CreatedBy", "LastModified", "LastModifiedBy")
SELECT s.id::uuid, s.name, s.code, s.point, s.country, s.region, s.roadname, s.direction, s.operator, s.notes, TRUE,
       now(), 'invias-open-data', now(), 'invias-open-data'
FROM (VALUES
${stationValues.join(',\n')}
) AS s(id, name, code, point, country, region, roadname, direction, operator, notes);

INSERT INTO trip.toll_tariffs
  (id, tollstationid, tollvehicleclasscode, amount, currency, effectivefrom, effectiveto,
   "Created", "CreatedBy", "LastModified", "LastModifiedBy")
SELECT t.id::uuid, t.tollstationid::uuid, t.tollvehicleclasscode, t.amount::numeric(18,2), 'COP', DATE '${effectiveFrom}', NULL,
       now(), 'invias-open-data', now(), 'invias-open-data'
FROM (VALUES
${tariffValues.join(',\n')}
) AS t(id, tollstationid, tollvehicleclasscode, amount);

COMMIT;

-- Verify:
--   SELECT count(*) FROM trip.toll_stations;        -- expect ${stationValues.length}
--   SELECT count(*) FROM trip.toll_tariffs;         -- expect ${tariffValues.length}
--   SELECT count(*) FROM trip.toll_vehicle_classes; -- expect ${VEHICLE_CLASSES.length}
`;

writeFileSync(outFile, script, 'utf8');

console.log(
  `  ${stationValues.length} stations, ${tariffValues.length} tariffs, ` +
    `${VEHICLE_CLASSES.length} vehicle classes -> ${outFile}`
);
if (skipped.length) console.log(`  ${skipped.length} skipped (listed in the script header)`);
