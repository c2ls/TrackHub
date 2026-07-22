using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using TrackHub.TripManagement.Infrastructure.Resources;

#nullable disable

namespace TrackHub.TripManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trip");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "toll_stations",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    point = table.Column<Point>(type: "geometry (Point, 4326)", nullable: false),
                    country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    roadname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    direction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    @operator = table.Column<string>(name: "operator", type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toll_stations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "toll_vehicle_classes",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sortorder = table.Column<int>(type: "integer", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toll_vehicle_classes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transporter_toll_classes",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    transportertypeid = table.Column<short>(type: "smallint", nullable: true),
                    transporterid = table.Column<Guid>(type: "uuid", nullable: true),
                    tollvehicleclasscode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transporter_toll_classes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    transporterid = table.Column<Guid>(type: "uuid", nullable: false),
                    driverid = table.Column<Guid>(type: "uuid", nullable: true),
                    routeplanid = table.Column<Guid>(type: "uuid", nullable: true),
                    serviceorderid = table.Column<Guid>(type: "uuid", nullable: true),
                    externalreference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    customername = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    originname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    originpoint = table.Column<Point>(type: "geometry (Point, 4326)", nullable: false),
                    plannedstartat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    plannedendat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    actualstartat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    actualendat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    lastpositionat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lastpoint = table.Column<Point>(type: "geometry (Point, 4326)", nullable: true),
                    actualdistancemeters = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    tollvehicleclass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    deviationopenedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellationreason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "toll_tariffs",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tollstationid = table.Column<Guid>(type: "uuid", nullable: false),
                    tollvehicleclasscode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    effectivefrom = table.Column<DateOnly>(type: "date", nullable: false),
                    effectiveto = table.Column<DateOnly>(type: "date", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toll_tariffs", x => x.id);
                    table.ForeignKey(
                        name: "FK_toll_tariffs_toll_stations_tollstationid",
                        column: x => x.tollstationid,
                        principalSchema: "trip",
                        principalTable: "toll_stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "route_plans",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripid = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    geom = table.Column<LineString>(type: "geometry (LineString, 4326)", nullable: true),
                    corridorgeom = table.Column<Polygon>(type: "geometry (Polygon, 4326)", nullable: true),
                    corridormeters = table.Column<int>(type: "integer", nullable: false, defaultValue: 500),
                    planneddistancemeters = table.Column<double>(type: "double precision", nullable: false),
                    planneddurationseconds = table.Column<int>(type: "integer", nullable: false),
                    waypointsjson = table.Column<string>(type: "text", nullable: true),
                    legsjson = table.Column<string>(type: "text", nullable: true),
                    computedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    errorcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    errormessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tollvehicleclass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    estimatedtollamount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tollcurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    tollstationsjson = table.Column<string>(type: "text", nullable: true),
                    tollstatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_route_plans_trips_tripid",
                        column: x => x.tripid,
                        principalSchema: "trip",
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_assignments",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripid = table.Column<Guid>(type: "uuid", nullable: false),
                    driverid = table.Column<Guid>(type: "uuid", nullable: false),
                    transporterid = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    assignedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    acknowledgedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    endedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_trip_assignments_trips_tripid",
                        column: x => x.tripid,
                        principalSchema: "trip",
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_documents",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripstopid = table.Column<Guid>(type: "uuid", nullable: true),
                    proofofdeliveryid = table.Column<Guid>(type: "uuid", nullable: true),
                    documentid = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_trip_documents_trips_tripid",
                        column: x => x.tripid,
                        principalSchema: "trip",
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_events",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripstopid = table.Column<Guid>(type: "uuid", nullable: true),
                    eventtype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    occurredat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    payloadjson = table.Column<string>(type: "text", nullable: true),
                    idempotencykey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_trip_events_trips_tripid",
                        column: x => x.tripid,
                        principalSchema: "trip",
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_shares",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripid = table.Column<Guid>(type: "uuid", nullable: false),
                    publiclinkgrantid = table.Column<Guid>(type: "uuid", nullable: false),
                    includedrivername = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    includevehicle = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    includeliveposition = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    includestopdetail = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    includepodsummary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    createdbyprincipalid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    expiresat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revokedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_shares", x => x.id);
                    table.ForeignKey(
                        name: "FK_trip_shares_trips_tripid",
                        column: x => x.tripid,
                        principalSchema: "trip",
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_stops",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripid = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    point = table.Column<Point>(type: "geometry (Point, 4326)", nullable: false),
                    geofenceid = table.Column<Guid>(type: "uuid", nullable: true),
                    arrivalgeom = table.Column<Polygon>(type: "geometry (Polygon, 4326)", nullable: true),
                    arrivalradiusmeters = table.Column<int>(type: "integer", nullable: false, defaultValue: 150),
                    plannedarrivalfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    plannedarrivalto = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    actualarrivalat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    actualdepartureat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    etaat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    etasource = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delayalertedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    requirespod = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    priority = table.Column<short>(type: "smallint", nullable: false),
                    observations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_stops", x => x.id);
                    table.ForeignKey(
                        name: "FK_trip_stops_trips_tripid",
                        column: x => x.tripid,
                        principalSchema: "trip",
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_deliveries",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripstopid = table.Column<Guid>(type: "uuid", nullable: false),
                    reference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    clientname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    branchname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    productssummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    observations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sequenceindex = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_deliveries", x => x.id);
                    table.ForeignKey(
                        name: "FK_trip_deliveries_trip_stops_tripstopid",
                        column: x => x.tripstopid,
                        principalSchema: "trip",
                        principalTable: "trip_stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_pods",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    tripstopid = table.Column<Guid>(type: "uuid", nullable: false),
                    deliveryid = table.Column<Guid>(type: "uuid", nullable: true),
                    receivername = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    receiverdocument = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    capturedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    clienteventid = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_pods", x => x.id);
                    table.ForeignKey(
                        name: "FK_trip_pods_trip_stops_tripstopid",
                        column: x => x.tripstopid,
                        principalSchema: "trip",
                        principalTable: "trip_stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_route_plans_accountid_tripid",
                schema: "trip",
                table: "route_plans",
                columns: new[] { "accountid", "tripid" });

            migrationBuilder.CreateIndex(
                name: "ix_route_plans_corridorgeom_gist",
                schema: "trip",
                table: "route_plans",
                column: "corridorgeom")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_route_plans_geom_gist",
                schema: "trip",
                table: "route_plans",
                column: "geom")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_route_plans_tripid",
                schema: "trip",
                table: "route_plans",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "ix_toll_stations_point_gist",
                schema: "trip",
                table: "toll_stations",
                column: "point")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ux_toll_stations_name_code",
                schema: "trip",
                table: "toll_stations",
                columns: new[] { "name", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_toll_tariffs_tollstationid_classcode",
                schema: "trip",
                table: "toll_tariffs",
                columns: new[] { "tollstationid", "tollvehicleclasscode" });

            migrationBuilder.CreateIndex(
                name: "ux_toll_tariffs_station_class_open",
                schema: "trip",
                table: "toll_tariffs",
                columns: new[] { "tollstationid", "tollvehicleclasscode" },
                unique: true,
                filter: "effectiveto is null");

            migrationBuilder.CreateIndex(
                name: "ux_toll_vehicle_classes_code",
                schema: "trip",
                table: "toll_vehicle_classes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_transporter_toll_classes_acct_type_transporter",
                schema: "trip",
                table: "transporter_toll_classes",
                columns: new[] { "accountid", "transportertypeid", "transporterid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_assignments_accountid_tripid",
                schema: "trip",
                table: "trip_assignments",
                columns: new[] { "accountid", "tripid" });

            migrationBuilder.CreateIndex(
                name: "ux_trip_assignments_active_per_trip",
                schema: "trip",
                table: "trip_assignments",
                column: "tripid",
                unique: true,
                filter: "status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "ix_trip_deliveries_accountid_tripstopid",
                schema: "trip",
                table: "trip_deliveries",
                columns: new[] { "accountid", "tripstopid" });

            migrationBuilder.CreateIndex(
                name: "IX_trip_deliveries_tripstopid",
                schema: "trip",
                table: "trip_deliveries",
                column: "tripstopid");

            migrationBuilder.CreateIndex(
                name: "ux_trip_documents_tripid_documentid",
                schema: "trip",
                table: "trip_documents",
                columns: new[] { "tripid", "documentid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_events_accountid_tripid_occurredat",
                schema: "trip",
                table: "trip_events",
                columns: new[] { "accountid", "tripid", "occurredat" });

            migrationBuilder.CreateIndex(
                name: "IX_trip_events_tripid",
                schema: "trip",
                table: "trip_events",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "ux_trip_events_idempotencykey",
                schema: "trip",
                table: "trip_events",
                column: "idempotencykey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_trip_pods_tripstopid_clienteventid",
                schema: "trip",
                table: "trip_pods",
                columns: new[] { "tripstopid", "clienteventid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_shares_accountid_tripid",
                schema: "trip",
                table: "trip_shares",
                columns: new[] { "accountid", "tripid" });

            migrationBuilder.CreateIndex(
                name: "IX_trip_shares_tripid",
                schema: "trip",
                table: "trip_shares",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "ux_trip_shares_publiclinkgrantid",
                schema: "trip",
                table: "trip_shares",
                column: "publiclinkgrantid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_stops_accountid_tripid",
                schema: "trip",
                table: "trip_stops",
                columns: new[] { "accountid", "tripid" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_stops_arrivalgeom_gist",
                schema: "trip",
                table: "trip_stops",
                column: "arrivalgeom")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ux_trip_stops_tripid_sequence",
                schema: "trip",
                table: "trip_stops",
                columns: new[] { "tripid", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trips_accountid_driverid_status",
                schema: "trip",
                table: "trips",
                columns: new[] { "accountid", "driverid", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_accountid_status_plannedstartat",
                schema: "trip",
                table: "trips",
                columns: new[] { "accountid", "status", "plannedstartat" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_accountid_transporterid_plannedstartat",
                schema: "trip",
                table: "trips",
                columns: new[] { "accountid", "transporterid", "plannedstartat" });

            migrationBuilder.CreateIndex(
                name: "ux_trips_accountid_code",
                schema: "trip",
                table: "trips",
                columns: new[] { "accountid", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_trips_accountid_externalreference",
                schema: "trip",
                table: "trips",
                columns: new[] { "accountid", "externalreference" },
                unique: true,
                filter: "externalreference IS NOT NULL");

            // Group visibility and user-account resolution are served by two SQL views over
            // the Manager-owned app tables (spec 11 section 6). No app.* or geofencing.*
            // table is created, altered or dropped by this migration.
            migrationBuilder.Sql(Views.vw_users);
            migrationBuilder.Sql(Views.vw_visible_transporter);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS trip.vw_visible_transporter;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS trip.vw_users;");

            migrationBuilder.DropTable(
                name: "route_plans",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "toll_tariffs",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "toll_vehicle_classes",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "transporter_toll_classes",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "trip_assignments",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "trip_deliveries",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "trip_documents",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "trip_events",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "trip_pods",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "trip_shares",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "toll_stations",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "trip_stops",
                schema: "trip");

            migrationBuilder.DropTable(
                name: "trips",
                schema: "trip");
        }
    }
}
