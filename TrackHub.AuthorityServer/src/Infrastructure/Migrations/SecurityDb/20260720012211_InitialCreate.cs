using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrackHub.AuthorityServer.Infrastructure.Migrations.SecurityDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "driver_credentials",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    driverid = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    normalizedlogin = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    failedattempts = table.Column<int>(type: "integer", nullable: false),
                    lockeduntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verifiedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lastloginat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    resetrequired = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver_credentials", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "security",
                columns: table => new
                {
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.userid, x.roleid });
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    emailaddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    verified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    loginattempts = table.Column<int>(type: "integer", nullable: false),
                    lockeduntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    userid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                    table.ForeignKey(
                        name: "FK_clients_users_userid",
                        column: x => x.userid,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_clients_userid",
                schema: "security",
                table: "clients",
                column: "userid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clients",
                schema: "security");

            migrationBuilder.DropTable(
                name: "driver_credentials",
                schema: "security");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "security");

            migrationBuilder.DropTable(
                name: "users",
                schema: "security");
        }
    }
}
