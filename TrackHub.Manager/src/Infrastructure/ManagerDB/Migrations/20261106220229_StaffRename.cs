using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.Manager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StaffRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "drivers", schema: "app", newName: "staff_members", newSchema: "app");
            migrationBuilder.RenameTable(name: "driver_qualifications", schema: "app", newName: "staff_qualifications", newSchema: "app");
            migrationBuilder.RenameTable(name: "driver_transporter_assignments", schema: "app", newName: "staff_transporter_assignments", newSchema: "app");
            migrationBuilder.RenameColumn(name: "driverid", schema: "app", table: "staff_qualifications", newName: "staffid");
            migrationBuilder.RenameColumn(name: "driverid", schema: "app", table: "staff_transporter_assignments", newName: "staffid");
            migrationBuilder.RenameIndex(name: "ux_drivers_accountid_documentnumber", schema: "app", table: "staff_members", newName: "ux_staff_members_accountid_documentnumber");
            migrationBuilder.RenameIndex(name: "IX_driver_qualifications_accountid_driverid", schema: "app", table: "staff_qualifications", newName: "IX_staff_qualifications_accountid_staffid");
            migrationBuilder.RenameIndex(name: "IX_driver_qualifications_accountid_expiresat", schema: "app", table: "staff_qualifications", newName: "IX_staff_qualifications_accountid_expiresat");
            migrationBuilder.RenameIndex(name: "ix_driver_assignments_account_driver_status", schema: "app", table: "staff_transporter_assignments", newName: "ix_staff_assignments_account_staff_status");
            migrationBuilder.RenameIndex(name: "ix_driver_assignments_account_transporter_status", schema: "app", table: "staff_transporter_assignments", newName: "ix_staff_assignments_account_transporter_status");
            migrationBuilder.Sql("ALTER TABLE app.staff_members RENAME CONSTRAINT \"PK_drivers\" TO \"PK_staff_members\";");
            migrationBuilder.Sql("ALTER TABLE app.staff_qualifications RENAME CONSTRAINT \"PK_driver_qualifications\" TO \"PK_staff_qualifications\";");
            migrationBuilder.Sql("ALTER TABLE app.staff_transporter_assignments RENAME CONSTRAINT \"PK_driver_transporter_assignments\" TO \"PK_staff_transporter_assignments\";");
            migrationBuilder.Sql("ALTER TABLE app.staff_members RENAME CONSTRAINT \"FK_drivers_accounts_accountid\" TO \"FK_staff_members_accounts_accountid\";");
            migrationBuilder.AddColumn<string>(name: "stafftype", schema: "app", table: "staff_members", type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Driver");
            migrationBuilder.CreateTable(
                name: "account_holidays",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_holidays", x => x.id);
                });
            migrationBuilder.CreateIndex(
                name: "ux_account_holidays_accountid_date",
                schema: "app",
                table: "account_holidays",
                columns: new[] { "accountid", "date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "account_holidays", schema: "app");
            migrationBuilder.DropColumn(name: "stafftype", schema: "app", table: "staff_members");
            migrationBuilder.Sql("ALTER TABLE app.staff_members RENAME CONSTRAINT \"PK_staff_members\" TO \"PK_drivers\";");
            migrationBuilder.Sql("ALTER TABLE app.staff_qualifications RENAME CONSTRAINT \"PK_staff_qualifications\" TO \"PK_driver_qualifications\";");
            migrationBuilder.Sql("ALTER TABLE app.staff_transporter_assignments RENAME CONSTRAINT \"PK_staff_transporter_assignments\" TO \"PK_driver_transporter_assignments\";");
            migrationBuilder.Sql("ALTER TABLE app.staff_members RENAME CONSTRAINT \"FK_staff_members_accounts_accountid\" TO \"FK_drivers_accounts_accountid\";");
            migrationBuilder.RenameIndex(name: "ux_staff_members_accountid_documentnumber", schema: "app", table: "staff_members", newName: "ux_drivers_accountid_documentnumber");
            migrationBuilder.RenameIndex(name: "IX_staff_qualifications_accountid_staffid", schema: "app", table: "staff_qualifications", newName: "IX_driver_qualifications_accountid_driverid");
            migrationBuilder.RenameIndex(name: "IX_staff_qualifications_accountid_expiresat", schema: "app", table: "staff_qualifications", newName: "IX_driver_qualifications_accountid_expiresat");
            migrationBuilder.RenameIndex(name: "ix_staff_assignments_account_staff_status", schema: "app", table: "staff_transporter_assignments", newName: "ix_driver_assignments_account_driver_status");
            migrationBuilder.RenameIndex(name: "ix_staff_assignments_account_transporter_status", schema: "app", table: "staff_transporter_assignments", newName: "ix_driver_assignments_account_transporter_status");
            migrationBuilder.RenameColumn(name: "staffid", schema: "app", table: "staff_qualifications", newName: "driverid");
            migrationBuilder.RenameColumn(name: "staffid", schema: "app", table: "staff_transporter_assignments", newName: "driverid");
            migrationBuilder.RenameTable(name: "staff_transporter_assignments", schema: "app", newName: "driver_transporter_assignments", newSchema: "app");
            migrationBuilder.RenameTable(name: "staff_qualifications", schema: "app", newName: "driver_qualifications", newSchema: "app");
            migrationBuilder.RenameTable(name: "staff_members", schema: "app", newName: "drivers", newSchema: "app");
        }
    }
}
