using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.TripManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StaffRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "driverid",
                schema: "trip",
                table: "trips",
                newName: "staffid");

            migrationBuilder.RenameIndex(
                name: "ix_trips_accountid_driverid_status",
                schema: "trip",
                table: "trips",
                newName: "ix_trips_accountid_staffid_status");

            migrationBuilder.RenameColumn(
                name: "includedrivername",
                schema: "trip",
                table: "trip_shares",
                newName: "includestaffname");

            migrationBuilder.RenameColumn(
                name: "driverid",
                schema: "trip",
                table: "trip_assignments",
                newName: "staffid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "staffid",
                schema: "trip",
                table: "trips",
                newName: "driverid");

            migrationBuilder.RenameIndex(
                name: "ix_trips_accountid_staffid_status",
                schema: "trip",
                table: "trips",
                newName: "ix_trips_accountid_driverid_status");

            migrationBuilder.RenameColumn(
                name: "includestaffname",
                schema: "trip",
                table: "trip_shares",
                newName: "includedrivername");

            migrationBuilder.RenameColumn(
                name: "staffid",
                schema: "trip",
                table: "trip_assignments",
                newName: "driverid");
        }
    }
}
