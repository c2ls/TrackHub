using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.TripManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTollStationNameUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_toll_stations_name_code",
                schema: "trip",
                table: "toll_stations");

            migrationBuilder.CreateIndex(
                name: "ux_toll_stations_name_code",
                schema: "trip",
                table: "toll_stations",
                columns: new[] { "name", "code" },
                unique: true,
                filter: "code is not null");

            migrationBuilder.CreateIndex(
                name: "ux_toll_stations_name_nocode",
                schema: "trip",
                table: "toll_stations",
                column: "name",
                unique: true,
                filter: "code is null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_toll_stations_name_code",
                schema: "trip",
                table: "toll_stations");

            migrationBuilder.DropIndex(
                name: "ux_toll_stations_name_nocode",
                schema: "trip",
                table: "toll_stations");

            migrationBuilder.CreateIndex(
                name: "ux_toll_stations_name_code",
                schema: "trip",
                table: "toll_stations",
                columns: new[] { "name", "code" },
                unique: true);
        }
    }
}
