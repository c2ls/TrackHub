using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrackHub.Security.Infrastructure.SecurityDB.Migrations
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
                name: "actions",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actions", x => x.id);
                });

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
                name: "driver_device_registrations",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    driverid = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    deviceid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    devicename = table.Column<string>(type: "text", nullable: true),
                    platform = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    appversion = table.Column<string>(type: "text", nullable: true),
                    pushtoken = table.Column<string>(type: "text", nullable: true),
                    refreshtokenfamilyid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    registeredat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lastseenat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revokedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revokedby = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver_device_registrations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "policies",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    parentroleid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_roles_roles_parentroleid",
                        column: x => x.parentroleid,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "service_client_permissions",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clientid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: true),
                    resource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    scope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    audience = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    effectivefrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effectiveto = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_client_permissions", x => x.id);
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
                    firstname = table.Column<string>(type: "text", nullable: false),
                    secondname = table.Column<string>(type: "text", nullable: true),
                    lastname = table.Column<string>(type: "text", nullable: false),
                    secondsurname = table.Column<string>(type: "text", nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    verified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    loginattempts = table.Column<int>(type: "integer", nullable: false),
                    lockeduntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    integrationuser = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "resource_action",
                schema: "security",
                columns: table => new
                {
                    actionid = table.Column<int>(type: "integer", nullable: false),
                    resourceid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_action", x => new { x.resourceid, x.actionid });
                    table.ForeignKey(
                        name: "FK_resource_action_actions_actionid",
                        column: x => x.actionid,
                        principalSchema: "security",
                        principalTable: "actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_action_resources_resourceid",
                        column: x => x.resourceid,
                        principalSchema: "security",
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    userid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    salt = table.Column<string>(type: "text", nullable: false),
                    processed = table.Column<bool>(type: "boolean", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "user_policy",
                schema: "security",
                columns: table => new
                {
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    policyid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_policy", x => new { x.policyid, x.userid });
                    table.ForeignKey(
                        name: "FK_user_policy_policies_policyid",
                        column: x => x.policyid,
                        principalSchema: "security",
                        principalTable: "policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_policy_users_userid",
                        column: x => x.userid,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.PrimaryKey("PK_user_role", x => new { x.roleid, x.userid });
                    table.ForeignKey(
                        name: "FK_user_role_roles_roleid",
                        column: x => x.roleid,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_role_users_userid",
                        column: x => x.userid,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_action_policy",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resourceid = table.Column<int>(type: "integer", nullable: false),
                    actionid = table.Column<int>(type: "integer", nullable: false),
                    policyid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_action_policy", x => x.id);
                    table.ForeignKey(
                        name: "FK_resource_action_policy_policies_policyid",
                        column: x => x.policyid,
                        principalSchema: "security",
                        principalTable: "policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_action_policy_resource_action_resourceid_actionid",
                        columns: x => new { x.resourceid, x.actionid },
                        principalSchema: "security",
                        principalTable: "resource_action",
                        principalColumns: new[] { "resourceid", "actionid" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_action_policy_resources_resourceid",
                        column: x => x.resourceid,
                        principalSchema: "security",
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_action_role",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resourceid = table.Column<int>(type: "integer", nullable: false),
                    actionid = table.Column<int>(type: "integer", nullable: false),
                    roleid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_action_role", x => x.id);
                    table.ForeignKey(
                        name: "FK_resource_action_role_resource_action_resourceid_actionid",
                        columns: x => new { x.resourceid, x.actionid },
                        principalSchema: "security",
                        principalTable: "resource_action",
                        principalColumns: new[] { "resourceid", "actionid" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_action_role_resources_resourceid",
                        column: x => x.resourceid,
                        principalSchema: "security",
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_action_role_roles_roleid",
                        column: x => x.roleid,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clients_name",
                schema: "security",
                table: "clients",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clients_userid",
                schema: "security",
                table: "clients",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_driver_credentials_accountid_normalizedlogin",
                schema: "security",
                table: "driver_credentials",
                columns: new[] { "accountid", "normalizedlogin" },
                unique: true,
                filter: "active = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_driver_device_registrations_driverid_deviceid",
                schema: "security",
                table: "driver_device_registrations",
                columns: new[] { "driverid", "deviceid" });

            migrationBuilder.CreateIndex(
                name: "IX_driver_device_registrations_refreshtokenfamilyid",
                schema: "security",
                table: "driver_device_registrations",
                column: "refreshtokenfamilyid");

            migrationBuilder.CreateIndex(
                name: "IX_resource_action_actionid",
                schema: "security",
                table: "resource_action",
                column: "actionid");

            migrationBuilder.CreateIndex(
                name: "IX_resource_action_policy_policyid",
                schema: "security",
                table: "resource_action_policy",
                column: "policyid");

            migrationBuilder.CreateIndex(
                name: "IX_resource_action_policy_resourceid_actionid",
                schema: "security",
                table: "resource_action_policy",
                columns: new[] { "resourceid", "actionid" });

            migrationBuilder.CreateIndex(
                name: "IX_resource_action_role_resourceid_actionid",
                schema: "security",
                table: "resource_action_role",
                columns: new[] { "resourceid", "actionid" });

            migrationBuilder.CreateIndex(
                name: "IX_resource_action_role_roleid",
                schema: "security",
                table: "resource_action_role",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "IX_roles_parentroleid",
                schema: "security",
                table: "roles",
                column: "parentroleid");

            migrationBuilder.CreateIndex(
                name: "IX_service_client_permissions_clientid_accountid_resource_acti~",
                schema: "security",
                table: "service_client_permissions",
                columns: new[] { "clientid", "accountid", "resource", "action", "scope", "audience" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_policy_userid",
                schema: "security",
                table: "user_policy",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_userid",
                schema: "security",
                table: "user_role",
                column: "userid");
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
                name: "driver_device_registrations",
                schema: "security");

            migrationBuilder.DropTable(
                name: "resource_action_policy",
                schema: "security");

            migrationBuilder.DropTable(
                name: "resource_action_role",
                schema: "security");

            migrationBuilder.DropTable(
                name: "service_client_permissions",
                schema: "security");

            migrationBuilder.DropTable(
                name: "user_policy",
                schema: "security");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "security");

            migrationBuilder.DropTable(
                name: "resource_action",
                schema: "security");

            migrationBuilder.DropTable(
                name: "policies",
                schema: "security");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "users",
                schema: "security");

            migrationBuilder.DropTable(
                name: "actions",
                schema: "security");

            migrationBuilder.DropTable(
                name: "resources",
                schema: "security");
        }
    }
}
