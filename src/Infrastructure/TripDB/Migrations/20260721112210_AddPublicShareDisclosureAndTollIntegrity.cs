using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.TripManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicShareDisclosureAndTollIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_transporter_toll_classes_acct_type_transporter",
                schema: "trip",
                table: "transporter_toll_classes");

            migrationBuilder.DropIndex(
                name: "ux_toll_vehicle_classes_code",
                schema: "trip",
                table: "toll_vehicle_classes");

            migrationBuilder.AddColumn<string>(
                name: "city",
                schema: "trip",
                table: "trip_stops",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "includeroute",
                schema: "trip",
                table: "trip_shares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddUniqueConstraint(
                name: "ux_toll_vehicle_classes_code",
                schema: "trip",
                table: "toll_vehicle_classes",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_transporter_toll_classes_tollvehicleclasscode",
                schema: "trip",
                table: "transporter_toll_classes",
                column: "tollvehicleclasscode");

            migrationBuilder.CreateIndex(
                name: "ux_transporter_toll_classes_acct_transporter",
                schema: "trip",
                table: "transporter_toll_classes",
                columns: new[] { "accountid", "transporterid" },
                unique: true,
                filter: "transportertypeid is null");

            migrationBuilder.CreateIndex(
                name: "ux_transporter_toll_classes_acct_type",
                schema: "trip",
                table: "transporter_toll_classes",
                columns: new[] { "accountid", "transportertypeid" },
                unique: true,
                filter: "transporterid is null");

            migrationBuilder.AddCheckConstraint(
                name: "ck_transporter_toll_classes_type_xor_transporter",
                schema: "trip",
                table: "transporter_toll_classes",
                sql: "(transportertypeid is null) <> (transporterid is null)");

            migrationBuilder.CreateIndex(
                name: "ix_toll_tariffs_tollvehicleclasscode",
                schema: "trip",
                table: "toll_tariffs",
                column: "tollvehicleclasscode");

            migrationBuilder.AddForeignKey(
                name: "fk_toll_tariffs_tollvehicleclasscode",
                schema: "trip",
                table: "toll_tariffs",
                column: "tollvehicleclasscode",
                principalSchema: "trip",
                principalTable: "toll_vehicle_classes",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_transporter_toll_classes_tollvehicleclasscode",
                schema: "trip",
                table: "transporter_toll_classes",
                column: "tollvehicleclasscode",
                principalSchema: "trip",
                principalTable: "toll_vehicle_classes",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_toll_tariffs_tollvehicleclasscode",
                schema: "trip",
                table: "toll_tariffs");

            migrationBuilder.DropForeignKey(
                name: "fk_transporter_toll_classes_tollvehicleclasscode",
                schema: "trip",
                table: "transporter_toll_classes");

            migrationBuilder.DropIndex(
                name: "ix_transporter_toll_classes_tollvehicleclasscode",
                schema: "trip",
                table: "transporter_toll_classes");

            migrationBuilder.DropIndex(
                name: "ux_transporter_toll_classes_acct_transporter",
                schema: "trip",
                table: "transporter_toll_classes");

            migrationBuilder.DropIndex(
                name: "ux_transporter_toll_classes_acct_type",
                schema: "trip",
                table: "transporter_toll_classes");

            migrationBuilder.DropCheckConstraint(
                name: "ck_transporter_toll_classes_type_xor_transporter",
                schema: "trip",
                table: "transporter_toll_classes");

            migrationBuilder.DropUniqueConstraint(
                name: "ux_toll_vehicle_classes_code",
                schema: "trip",
                table: "toll_vehicle_classes");

            migrationBuilder.DropIndex(
                name: "ix_toll_tariffs_tollvehicleclasscode",
                schema: "trip",
                table: "toll_tariffs");

            migrationBuilder.DropColumn(
                name: "city",
                schema: "trip",
                table: "trip_stops");

            migrationBuilder.DropColumn(
                name: "includeroute",
                schema: "trip",
                table: "trip_shares");

            migrationBuilder.CreateIndex(
                name: "ux_transporter_toll_classes_acct_type_transporter",
                schema: "trip",
                table: "transporter_toll_classes",
                columns: new[] { "accountid", "transportertypeid", "transporterid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_toll_vehicle_classes_code",
                schema: "trip",
                table: "toll_vehicle_classes",
                column: "code",
                unique: true);
        }
    }
}
