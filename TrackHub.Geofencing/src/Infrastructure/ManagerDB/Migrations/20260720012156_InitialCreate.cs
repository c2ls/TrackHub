using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using TrackHub.Geofencing.Infrastructure.Resources;

#nullable disable

namespace TrackHub.Geofencing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "geofencing");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "geofences",
                schema: "geofencing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    geom = table.Column<Polygon>(type: "geometry (Polygon, 4326)", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    color = table.Column<short>(type: "smallint", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    circlecenter = table.Column<Point>(type: "geometry (Point, 4326)", nullable: true),
                    circleradiusmeters = table.Column<double>(type: "double precision", nullable: true),
                    alertonentry = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    alertonexit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    dwellthresholdminutes = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geofences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "geofenceevents",
                schema: "geofencing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporterid = table.Column<Guid>(type: "uuid", nullable: false),
                    geofenceid = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    datetime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    departuretimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    dwellalertedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geofenceevents", x => x.id);
                    table.ForeignKey(
                        name: "FK_geofenceevents_geofences_geofenceid",
                        column: x => x.geofenceid,
                        principalSchema: "geofencing",
                        principalTable: "geofences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_geofenceevent_accountid_datetime",
                schema: "geofencing",
                table: "geofenceevents",
                columns: new[] { "accountid", "datetime" });

            migrationBuilder.CreateIndex(
                name: "ix_geofenceevent_geofenceid",
                schema: "geofencing",
                table: "geofenceevents",
                column: "geofenceid");

            migrationBuilder.CreateIndex(
                name: "ix_geofenceevent_open_events",
                schema: "geofencing",
                table: "geofenceevents",
                columns: new[] { "transporterid", "geofenceid", "departuretimestamp" },
                filter: "departuretimestamp IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_geofenceevent_transporterid",
                schema: "geofencing",
                table: "geofenceevents",
                column: "transporterid");

            migrationBuilder.CreateIndex(
                name: "geofence_idx",
                schema: "geofencing",
                table: "geofences",
                column: "geom")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_geofences_accountid",
                schema: "geofencing",
                table: "geofences",
                column: "accountid");

            migrationBuilder.Sql(Views.vw_users);
            migrationBuilder.Sql(Views.vw_transporter_position);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS geofencing.vw_transporter_position;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS geofencing.vw_users;");

            migrationBuilder.DropTable(
                name: "geofenceevents",
                schema: "geofencing");

            migrationBuilder.DropTable(
                name: "geofences",
                schema: "geofencing");
        }
    }
}
