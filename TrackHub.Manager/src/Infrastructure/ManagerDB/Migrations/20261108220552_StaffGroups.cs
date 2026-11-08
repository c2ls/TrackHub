using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.Manager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StaffGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staff_group",
                schema: "app",
                columns: table => new
                {
                    staffid = table.Column<Guid>(type: "uuid", nullable: false),
                    groupid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_group", x => new { x.groupid, x.staffid });
                    table.ForeignKey(
                        name: "FK_staff_group_groups_groupid",
                        column: x => x.groupid,
                        principalSchema: "app",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staff_group_staff_members_staffid",
                        column: x => x.staffid,
                        principalSchema: "app",
                        principalTable: "staff_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staff_group_staffid",
                schema: "app",
                table: "staff_group",
                column: "staffid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staff_group",
                schema: "app");
        }
    }
}
