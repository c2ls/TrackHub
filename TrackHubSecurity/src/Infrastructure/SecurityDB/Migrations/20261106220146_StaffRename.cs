using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.Security.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StaffRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "driver_credentials", schema: "security", newName: "staff_credentials", newSchema: "security");
            migrationBuilder.RenameTable(name: "driver_device_registrations", schema: "security", newName: "staff_device_registrations", newSchema: "security");
            migrationBuilder.RenameColumn(name: "driverid", schema: "security", table: "staff_credentials", newName: "staffid");
            migrationBuilder.RenameColumn(name: "driverid", schema: "security", table: "staff_device_registrations", newName: "staffid");
            migrationBuilder.RenameIndex(name: "IX_driver_credentials_accountid_normalizedlogin", schema: "security", table: "staff_credentials", newName: "IX_staff_credentials_accountid_normalizedlogin");
            migrationBuilder.RenameIndex(name: "IX_driver_device_registrations_refreshtokenfamilyid", schema: "security", table: "staff_device_registrations", newName: "IX_staff_device_registrations_refreshtokenfamilyid");
            migrationBuilder.RenameIndex(name: "IX_driver_device_registrations_driverid_deviceid", schema: "security", table: "staff_device_registrations", newName: "IX_staff_device_registrations_staffid_deviceid");
            migrationBuilder.Sql("ALTER TABLE security.staff_credentials RENAME CONSTRAINT \"PK_driver_credentials\" TO \"PK_staff_credentials\";");
            migrationBuilder.Sql("ALTER TABLE security.staff_device_registrations RENAME CONSTRAINT \"PK_driver_device_registrations\" TO \"PK_staff_device_registrations\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "staff_credentials", schema: "security", newName: "driver_credentials", newSchema: "security");
            migrationBuilder.RenameTable(name: "staff_device_registrations", schema: "security", newName: "driver_device_registrations", newSchema: "security");
            migrationBuilder.RenameColumn(name: "staffid", schema: "security", table: "driver_credentials", newName: "driverid");
            migrationBuilder.RenameColumn(name: "staffid", schema: "security", table: "driver_device_registrations", newName: "driverid");
            migrationBuilder.RenameIndex(name: "IX_staff_credentials_accountid_normalizedlogin", schema: "security", table: "driver_credentials", newName: "IX_driver_credentials_accountid_normalizedlogin");
            migrationBuilder.RenameIndex(name: "IX_staff_device_registrations_refreshtokenfamilyid", schema: "security", table: "driver_device_registrations", newName: "IX_driver_device_registrations_refreshtokenfamilyid");
            migrationBuilder.RenameIndex(name: "IX_staff_device_registrations_staffid_deviceid", schema: "security", table: "driver_device_registrations", newName: "IX_driver_device_registrations_driverid_deviceid");
            migrationBuilder.Sql("ALTER TABLE security.driver_credentials RENAME CONSTRAINT \"PK_staff_credentials\" TO \"PK_driver_credentials\";");
            migrationBuilder.Sql("ALTER TABLE security.driver_device_registrations RENAME CONSTRAINT \"PK_staff_device_registrations\" TO \"PK_driver_device_registrations\";");
        }
    }
}
