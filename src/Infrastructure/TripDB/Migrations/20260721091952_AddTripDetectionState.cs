using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.TripManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripDetectionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "consecutiveoutsidefixes",
                schema: "trip",
                table: "trips",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "outsidesinceat",
                schema: "trip",
                table: "trip_stops",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "consecutiveoutsidefixes",
                schema: "trip",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "outsidesinceat",
                schema: "trip",
                table: "trip_stops");
        }
    }
}
