using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TrackHub.Manager.Domain.Models;

#nullable disable

namespace TrackHub.Manager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.EnsureSchema(
                name: "map");

            migrationBuilder.EnsureSchema(
                name: "telemetry");

            migrationBuilder.CreateTable(
                name: "account_support_grants",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    supportuserid = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ticketreference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    approvedby = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    approvedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accesslevel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    startsat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    endsat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revokedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revokedby = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_support_grants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false, comment: "Commercial classification of the tenant. Values: 1=Personal, 2=Business, 3=Associate."),
                    active = table.Column<bool>(type: "boolean", nullable: false, comment: "Legacy on/off flag derived from status; true exactly when status is Trial or Active."),
                    status = table.Column<short>(type: "smallint", nullable: false, comment: "Authoritative operational state of the tenant. Values: 1=Trial, 2=Active, 3=Suspended, 4=Cancelled, 5=Archived."),
                    statuschangedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Timestamp of the last status transition; null until the first transition."),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.CheckConstraint("ck_accounts_status", "status in (1, 2, 3, 4, 5)");
                    table.CheckConstraint("ck_accounts_type", "type in (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "alert_events",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    eventtype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    severity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sourcemodule = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    resourcetype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    resourceid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    firstseenat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lastseenat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    payloadjson = table.Column<string>(type: "text", nullable: true),
                    deduplicationkey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "alert_subscriptions",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    principaltype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    principalid = table.Column<Guid>(type: "uuid", nullable: false),
                    eventtypefilter = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    channel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    actortype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actorid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    resourcetype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    resourceid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    result = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    oldvaluesjson = table.Column<string>(type: "text", nullable: true),
                    newvaluesjson = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ipaddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    useragent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    correlationid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    occurredat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "background_job_runs",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    jobkey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: true),
                    resourcekey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    idempotencykey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    startedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    errorcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    errormessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_background_job_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_signatures",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documentid = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    signerprincipaltype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    signerprincipalid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    signername = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    signatureimagedocumentid = table.Column<Guid>(type: "uuid", nullable: true),
                    legaltextaccepted = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    signedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_signatures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    displayname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    required = table.Column<bool>(type: "boolean", nullable: false),
                    expiring = table.Column<bool>(type: "boolean", nullable: false),
                    defaultvaliditydays = table.Column<int>(type: "integer", nullable: true),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_versions",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documentid = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    versionnumber = table.Column<int>(type: "integer", nullable: false),
                    storageprovider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    storagekey = table.Column<string>(type: "text", nullable: false),
                    sha256hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sizebytes = table.Column<long>(type: "bigint", nullable: false),
                    contenttype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    filename = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    scanstatus = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    replacedbyprincipaltype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    replacedbyprincipalid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    bytespurgedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_versions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    ownerentitytype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ownerentityid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    uploadedbyprincipaltype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    uploadedbyprincipalid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    storageprovider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    storagekey = table.Column<string>(type: "text", nullable: false),
                    contenttype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sizebytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    classification = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    expiresat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    visibilityscope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    scanstatus = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    filename = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    currentversion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    capturedlatitude = table.Column<double>(type: "double precision", nullable: true),
                    capturedlongitude = table.Column<double>(type: "double precision", nullable: true),
                    capturedatdevicetime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sourcedeviceregistrationid = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "driver_qualifications",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    driverid = table.Column<Guid>(type: "uuid", nullable: false),
                    qualificationtype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    issuedat = table.Column<DateOnly>(type: "date", nullable: true),
                    expiresat = table.Column<DateOnly>(type: "date", nullable: true),
                    issuingauthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    documentid = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver_qualifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "driver_transporter_assignments",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    driverid = table.Column<Guid>(type: "uuid", nullable: false),
                    transporterid = table.Column<Guid>(type: "uuid", nullable: false),
                    startsat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    endsat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    assignmenttype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    createdbyprincipal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver_transporter_assignments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "geocoding_providers",
                schema: "map",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false, comment: "Geocoding backend this provider row configures. Values: 1=Nominatim, 2=OpenRouteService, 3=Google."),
                    endpointuri = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    apikey = table.Column<string>(type: "text", nullable: true),
                    salt = table.Column<string>(type: "text", nullable: true),
                    requestspersecond = table.Column<int>(type: "integer", nullable: false),
                    timeoutseconds = table.Column<int>(type: "integer", nullable: false),
                    configurationjson = table.Column<string>(type: "text", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geocoding_providers", x => x.id);
                    table.CheckConstraint("ck_geocoding_providers_type", "type in (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "notification_deliveries",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    notificationruleid = table.Column<Guid>(type: "uuid", nullable: true),
                    alerteventid = table.Column<Guid>(type: "uuid", nullable: true),
                    channel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recipientprincipaltype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recipient = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    providermessageid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    sentat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    readat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    payloadjson = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_deliveries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_rules",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    rulekey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ruletype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    triggerevent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recipientselector = table.Column<string>(type: "text", nullable: false),
                    channelsjson = table.Column<string>(type: "text", nullable: false),
                    throttlingjson = table.Column<string>(type: "text", nullable: true),
                    configurationjson = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_templates",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: true),
                    templatekey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    channel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    locale = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    body = table.Column<string>(type: "text", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operator_health_checks",
                schema: "telemetry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    operatorid = table.Column<Guid>(type: "uuid", nullable: false),
                    checktype = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    latencyms = table.Column<int>(type: "integer", nullable: true),
                    startedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    errorcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    errormessage = table.Column<string>(type: "text", nullable: true),
                    retrycount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    correlationid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operator_health_checks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operator_sync_runs",
                schema: "telemetry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    operatorid = table.Column<Guid>(type: "uuid", nullable: false),
                    triggertype = table.Column<int>(type: "integer", nullable: false),
                    result = table.Column<int>(type: "integer", nullable: false),
                    startedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    devicesseen = table.Column<int>(type: "integer", nullable: false),
                    devicesadded = table.Column<int>(type: "integer", nullable: false),
                    devicesupdated = table.Column<int>(type: "integer", nullable: false),
                    devicesremoved = table.Column<int>(type: "integer", nullable: false),
                    devicesignored = table.Column<int>(type: "integer", nullable: false),
                    positionsread = table.Column<int>(type: "integer", nullable: false),
                    positionsaccepted = table.Column<int>(type: "integer", nullable: false),
                    positionsrejected = table.Column<int>(type: "integer", nullable: false),
                    errorcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    errormessage = table.Column<string>(type: "text", nullable: true),
                    correlationid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operator_sync_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "platform_announcements",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    messageen = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    messagees = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    severity = table.Column<int>(type: "integer", nullable: false, comment: "Display severity of the announcement. Values: 0=Info, 1=Warning, 2=Critical."),
                    startsat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    endsat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_announcements", x => x.id);
                    table.CheckConstraint("ck_platform_announcements_severity", "severity in (0, 1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "public_link_grants",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    resourcetype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    resourceid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    scopes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    subjecttokenidhash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    expiresat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revokedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revokedby = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    createdbyprincipalid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    accesscount = table.Column<int>(type: "integer", nullable: false),
                    lastaccessedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_link_grants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false, comment: "Provenance of the report definition. Values: 1=Basic, 2=Custom, 3=External."),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    requiredfeaturekey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    manageronly = table.Column<bool>(type: "boolean", nullable: false),
                    supportspdf = table.Column<bool>(type: "boolean", nullable: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                    table.CheckConstraint("ck_reports_type", "type in (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "transporter_position_history",
                schema: "telemetry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    operatorid = table.Column<Guid>(type: "uuid", nullable: false),
                    deviceid = table.Column<Guid>(type: "uuid", nullable: false),
                    transporterid = table.Column<Guid>(type: "uuid", nullable: false),
                    sourcetimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    receivedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    altitude = table.Column<double>(type: "double precision", nullable: true),
                    speed = table.Column<double>(type: "double precision", nullable: false),
                    course = table.Column<double>(type: "double precision", nullable: true),
                    eventid = table.Column<int>(type: "integer", nullable: true),
                    address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    state = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    country = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    attributes = table.Column<string>(type: "text", nullable: true),
                    idempotencykey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transporter_position_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transporter_type",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accbased = table.Column<bool>(type: "boolean", nullable: false),
                    stoppedgap = table.Column<double>(type: "double precision", nullable: false),
                    maxdistance = table.Column<double>(type: "double precision", nullable: false),
                    maxtimegap = table.Column<double>(type: "double precision", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transporter_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_branding",
                schema: "app",
                columns: table => new
                {
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    displayname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    logodocumentid = table.Column<Guid>(type: "uuid", nullable: true),
                    primarycolor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    reportheader = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_branding", x => x.accountid);
                    table.ForeignKey(
                        name: "FK_account_branding_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "account_features",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    featurekey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    tier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    effectivefrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effectiveto = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    configurationjson = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_features", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_features_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_settings",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maps = table.Column<string>(type: "text", nullable: false),
                    mapskey = table.Column<string>(type: "text", nullable: true),
                    onlineinterval = table.Column<int>(type: "integer", nullable: false),
                    refreshmap = table.Column<bool>(type: "boolean", nullable: false),
                    refreshmapinterval = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_settings_accounts_id",
                        column: x => x.id,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "drivers",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    documenttype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    documentnumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    employeecode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    licensenumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    licenseexpiresat = table.Column<DateOnly>(type: "date", nullable: true),
                    defaulttransporterid = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drivers", x => x.id);
                    table.ForeignKey(
                        name: "FK_drivers_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_groups_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operators",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    phonenumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    emailaddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    contactname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    protocoltype = table.Column<int>(type: "integer", nullable: false, comment: "Telematics protocol this operator speaks; selects the Router provider client. Values: 1=CommandTrack, 2=Traccar, 3=Flespi, 4=GeoTab, 5=GpsGate, 6=Navixy, 7=Samsara, 8=Wialon, 9=Protrack, 10=Mettax."),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    syncintervalminutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    lastsuccessfulsyncat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lastmanualsyncat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lastdevicesyncat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operators", x => x.id);
                    table.CheckConstraint("ck_operators_protocoltype", "protocoltype in (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)");
                    table.ForeignKey(
                        name: "FK_operators_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transporters",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    transportertypeid = table.Column<short>(type: "smallint", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transporters", x => x.id);
                    table.ForeignKey(
                        name: "FK_transporters_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transporters_transporter_type_transportertypeid",
                        column: x => x.transportertypeid,
                        principalSchema: "app",
                        principalTable: "transporter_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "points_of_interest",
                schema: "map",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<short>(type: "smallint", nullable: false, comment: "Category of the point of interest. Values: 1=ClientSite, 2=Warehouse, 3=FuelStation, 4=TollBooth, 5=RestArea, 6=Workshop, 7=Port, 8=Other."),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    color = table.Column<short>(type: "smallint", nullable: true),
                    groupid = table.Column<long>(type: "bigint", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_points_of_interest", x => x.id);
                    table.CheckConstraint("ck_pointsofinterest_type", "type in (1, 2, 3, 4, 5, 6, 7, 8)");
                    table.ForeignKey(
                        name: "FK_points_of_interest_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_points_of_interest_groups_groupid",
                        column: x => x.groupid,
                        principalSchema: "app",
                        principalTable: "groups",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "credentials",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    uri = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    key = table.Column<string>(type: "text", nullable: true, comment: "Provider-specific auxiliary credential field 1. GpsGate: application id. Unused by every other protocol."),
                    key2 = table.Column<string>(type: "text", nullable: true, comment: "Provider-specific auxiliary credential field 2. GpsGate: user id, consumed only by TrackHubRouter GpsGateReaderBase.Init. Unused by every other protocol."),
                    salt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    token = table.Column<string>(type: "text", nullable: true),
                    tokenexpiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    refreshtoken = table.Column<string>(type: "text", nullable: true),
                    refreshtokenexpiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    credentialversion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    rotatedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rotatedbyprincipaltype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    rotatedbyprincipalid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    operatorid = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credentials", x => x.id);
                    table.ForeignKey(
                        name: "FK_credentials_operators_operatorid",
                        column: x => x.operatorid,
                        principalSchema: "app",
                        principalTable: "operators",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    identifier = table.Column<int>(type: "integer", nullable: false),
                    serial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    devicetypeid = table.Column<short>(type: "smallint", nullable: false, comment: "Hardware class of the tracked device. Values: 1=Aviation, 2=Camera, 3=Cycling, 4=Cellular, 5=Drones, 6=EmergencyLocator, 7=Fitness, 8=Handheld, 9=Marine, 10=OBDScanner, 11=PetTracking, 12=Phone, 13=Satellite, 14=Smartwatch, 15=Wearable."),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    providerdisplayname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    providermetadatahash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    providerstatus = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    detectedstatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    operatorid = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    firstseenat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lastseenat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lastsyncedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lastassignedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ignoredat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                    table.CheckConstraint("ck_devices_devicetypeid", "devicetypeid in (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)");
                    table.ForeignKey(
                        name: "FK_devices_accounts_accountid",
                        column: x => x.accountid,
                        principalSchema: "app",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_devices_operators_operatorid",
                        column: x => x.operatorid,
                        principalSchema: "app",
                        principalTable: "operators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_group",
                schema: "app",
                columns: table => new
                {
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    groupid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_group", x => new { x.groupid, x.userid });
                    table.ForeignKey(
                        name: "FK_user_group_groups_groupid",
                        column: x => x.groupid,
                        principalSchema: "app",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_group_users_userid",
                        column: x => x.userid,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    style = table.Column<string>(type: "text", nullable: false),
                    navbar = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_settings_users_id",
                        column: x => x.id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transporter_group",
                schema: "app",
                columns: table => new
                {
                    transporterid = table.Column<Guid>(type: "uuid", nullable: false),
                    groupid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transporter_group", x => new { x.groupid, x.transporterid });
                    table.ForeignKey(
                        name: "FK_transporter_group_groups_groupid",
                        column: x => x.groupid,
                        principalSchema: "app",
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transporter_group_transporters_transporterid",
                        column: x => x.transporterid,
                        principalSchema: "app",
                        principalTable: "transporters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transporter_position",
                schema: "telemetry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporterid = table.Column<Guid>(type: "uuid", nullable: false),
                    geometryid = table.Column<Guid>(type: "uuid", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    altitude = table.Column<double>(type: "double precision", nullable: true),
                    datetime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    speed = table.Column<double>(type: "double precision", nullable: false),
                    course = table.Column<double>(type: "double precision", nullable: true),
                    eventId = table.Column<int>(type: "integer", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    state = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    attributes = table.Column<AttributesVm>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transporter_position", x => x.id);
                    table.ForeignKey(
                        name: "FK_transporter_position_transporters_transporterid",
                        column: x => x.transporterid,
                        principalSchema: "app",
                        principalTable: "transporters",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transporter_device_assignments",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountid = table.Column<Guid>(type: "uuid", nullable: false),
                    transporterid = table.Column<Guid>(type: "uuid", nullable: false),
                    deviceid = table.Column<Guid>(type: "uuid", nullable: false),
                    effectivefrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effectiveto = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    isprimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    assignmentreason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    createdbyprincipaltype = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Kind of principal that created the assignment; the principal identity is in CreatedBy."),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transporter_device_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_transporter_device_assignments_devices_deviceid",
                        column: x => x.deviceid,
                        principalSchema: "app",
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transporter_device_assignments_transporters_transporterid",
                        column: x => x.transporterid,
                        principalSchema: "app",
                        principalTable: "transporters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_features_accountid_featurekey",
                schema: "app",
                table: "account_features",
                columns: new[] { "accountid", "featurekey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_support_grants_accountid_supportuserid_startsat_end~",
                schema: "app",
                table: "account_support_grants",
                columns: new[] { "accountid", "supportuserid", "startsat", "endsat" });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_name",
                schema: "app",
                table: "accounts",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_status",
                schema: "app",
                table: "accounts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_alert_events_accountid_status_lastseenat",
                schema: "app",
                table: "alert_events",
                columns: new[] { "accountid", "status", "lastseenat" });

            migrationBuilder.CreateIndex(
                name: "IX_alert_events_deduplicationkey",
                schema: "app",
                table: "alert_events",
                column: "deduplicationkey");

            migrationBuilder.CreateIndex(
                name: "IX_alert_subscriptions_accountid_eventtypefilter_enabled",
                schema: "app",
                table: "alert_subscriptions",
                columns: new[] { "accountid", "eventtypefilter", "enabled" });

            migrationBuilder.CreateIndex(
                name: "ix_alert_subscriptions_principal_filter_channel",
                schema: "app",
                table: "alert_subscriptions",
                columns: new[] { "accountid", "principaltype", "principalid", "eventtypefilter", "channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_accountid_occurredat",
                schema: "app",
                table: "audit_events",
                columns: new[] { "accountid", "occurredat" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_correlationid",
                schema: "app",
                table: "audit_events",
                column: "correlationid");

            migrationBuilder.CreateIndex(
                name: "IX_background_job_runs_accountid_startedat",
                schema: "app",
                table: "background_job_runs",
                columns: new[] { "accountid", "startedat" });

            migrationBuilder.CreateIndex(
                name: "IX_background_job_runs_jobkey_idempotencykey",
                schema: "app",
                table: "background_job_runs",
                columns: new[] { "jobkey", "idempotencykey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_credentials_operatorid",
                schema: "app",
                table: "credentials",
                column: "operatorid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_devices_accountid",
                schema: "app",
                table: "devices",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_devices_accountid_operatorid_detectedstatus_lastsyncedat",
                schema: "app",
                table: "devices",
                columns: new[] { "accountid", "operatorid", "detectedstatus", "lastsyncedat" });

            migrationBuilder.CreateIndex(
                name: "IX_devices_accountid_operatorid_identifier",
                schema: "app",
                table: "devices",
                columns: new[] { "accountid", "operatorid", "identifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_devices_operatorid",
                schema: "app",
                table: "devices",
                column: "operatorid");

            migrationBuilder.CreateIndex(
                name: "IX_document_signatures_accountid",
                schema: "app",
                table: "document_signatures",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_document_signatures_documentid",
                schema: "app",
                table: "document_signatures",
                column: "documentid");

            migrationBuilder.CreateIndex(
                name: "IX_document_types_accountid_category",
                schema: "app",
                table: "document_types",
                columns: new[] { "accountid", "category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_accountid",
                schema: "app",
                table: "document_versions",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_documentid_versionnumber",
                schema: "app",
                table: "document_versions",
                columns: new[] { "documentid", "versionnumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_accountid_expiresat_status",
                schema: "app",
                table: "documents",
                columns: new[] { "accountid", "expiresat", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_accountid_ownerentitytype_ownerentityid",
                schema: "app",
                table: "documents",
                columns: new[] { "accountid", "ownerentitytype", "ownerentityid" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_accountid_ownerentitytype_ownerentityid_category_~",
                schema: "app",
                table: "documents",
                columns: new[] { "accountid", "ownerentitytype", "ownerentityid", "category", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_sha256hash",
                schema: "app",
                table: "documents",
                column: "sha256hash");

            migrationBuilder.CreateIndex(
                name: "IX_driver_qualifications_accountid_driverid",
                schema: "app",
                table: "driver_qualifications",
                columns: new[] { "accountid", "driverid" });

            migrationBuilder.CreateIndex(
                name: "IX_driver_qualifications_accountid_expiresat",
                schema: "app",
                table: "driver_qualifications",
                columns: new[] { "accountid", "expiresat" });

            migrationBuilder.CreateIndex(
                name: "ix_driver_assignments_account_driver_status",
                schema: "app",
                table: "driver_transporter_assignments",
                columns: new[] { "accountid", "driverid", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_driver_assignments_account_transporter_status",
                schema: "app",
                table: "driver_transporter_assignments",
                columns: new[] { "accountid", "transporterid", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_drivers_accountid_documentnumber",
                schema: "app",
                table: "drivers",
                columns: new[] { "accountid", "documentnumber" });

            migrationBuilder.CreateIndex(
                name: "ix_geocoding_providers_single_active",
                schema: "map",
                table: "geocoding_providers",
                column: "active",
                unique: true,
                filter: "active = true");

            migrationBuilder.CreateIndex(
                name: "IX_groups_accountid",
                schema: "app",
                table: "groups",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_account_status_created",
                schema: "app",
                table: "notification_deliveries",
                columns: new[] { "accountid", "status", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_accountid_status_channel",
                schema: "app",
                table: "notification_deliveries",
                columns: new[] { "accountid", "status", "channel" });

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_recipient_readat",
                schema: "app",
                table: "notification_deliveries",
                columns: new[] { "recipientprincipaltype", "recipient", "readat" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_rules_accountid_rulekey",
                schema: "app",
                table: "notification_rules",
                columns: new[] { "accountid", "rulekey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_accountid_templatekey_channel_locale",
                schema: "app",
                table: "notification_templates",
                columns: new[] { "accountid", "templatekey", "channel", "locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operator_health_checks_accountid_operatorid_startedat",
                schema: "telemetry",
                table: "operator_health_checks",
                columns: new[] { "accountid", "operatorid", "startedat" });

            migrationBuilder.CreateIndex(
                name: "IX_operator_sync_runs_accountid_operatorid_startedat",
                schema: "telemetry",
                table: "operator_sync_runs",
                columns: new[] { "accountid", "operatorid", "startedat" });

            migrationBuilder.CreateIndex(
                name: "IX_operator_sync_runs_accountid_startedat",
                schema: "telemetry",
                table: "operator_sync_runs",
                columns: new[] { "accountid", "startedat" });

            migrationBuilder.CreateIndex(
                name: "IX_operators_accountid_enabled_protocoltype",
                schema: "app",
                table: "operators",
                columns: new[] { "accountid", "enabled", "protocoltype" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_announcements_active_startsat_endsat",
                schema: "app",
                table: "platform_announcements",
                columns: new[] { "active", "startsat", "endsat" });

            migrationBuilder.CreateIndex(
                name: "IX_points_of_interest_accountid",
                schema: "map",
                table: "points_of_interest",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_points_of_interest_accountid_active",
                schema: "map",
                table: "points_of_interest",
                columns: new[] { "accountid", "active" });

            migrationBuilder.CreateIndex(
                name: "IX_points_of_interest_groupid",
                schema: "map",
                table: "points_of_interest",
                column: "groupid");

            migrationBuilder.CreateIndex(
                name: "IX_public_link_grants_accountid_resourcetype_resourceid",
                schema: "app",
                table: "public_link_grants",
                columns: new[] { "accountid", "resourcetype", "resourceid" });

            migrationBuilder.CreateIndex(
                name: "IX_public_link_grants_subjecttokenidhash",
                schema: "app",
                table: "public_link_grants",
                column: "subjecttokenidhash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transporter_device_assignments_accountid_deviceid_effective~",
                schema: "app",
                table: "transporter_device_assignments",
                columns: new[] { "accountid", "deviceid", "effectivefrom" });

            migrationBuilder.CreateIndex(
                name: "IX_transporter_device_assignments_accountid_transporterid_effe~",
                schema: "app",
                table: "transporter_device_assignments",
                columns: new[] { "accountid", "transporterid", "effectivefrom" });

            migrationBuilder.CreateIndex(
                name: "ix_transporter_device_assignments_active_device",
                schema: "app",
                table: "transporter_device_assignments",
                columns: new[] { "accountid", "deviceid" },
                unique: true,
                filter: "status = 0");

            migrationBuilder.CreateIndex(
                name: "IX_transporter_device_assignments_deviceid",
                schema: "app",
                table: "transporter_device_assignments",
                column: "deviceid");

            migrationBuilder.CreateIndex(
                name: "IX_transporter_device_assignments_transporterid",
                schema: "app",
                table: "transporter_device_assignments",
                column: "transporterid");

            migrationBuilder.CreateIndex(
                name: "IX_transporter_group_transporterid",
                schema: "app",
                table: "transporter_group",
                column: "transporterid");

            migrationBuilder.CreateIndex(
                name: "IX_transporter_position_transporterid",
                schema: "telemetry",
                table: "transporter_position",
                column: "transporterid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transporter_position_history_accountid_operatorid_sourcetim~",
                schema: "telemetry",
                table: "transporter_position_history",
                columns: new[] { "accountid", "operatorid", "sourcetimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_transporter_position_history_accountid_transporterid_source~",
                schema: "telemetry",
                table: "transporter_position_history",
                columns: new[] { "accountid", "transporterid", "sourcetimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_transporter_position_history_idempotencykey",
                schema: "telemetry",
                table: "transporter_position_history",
                column: "idempotencykey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transporters_accountid",
                schema: "app",
                table: "transporters",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_transporters_transportertypeid",
                schema: "app",
                table: "transporters",
                column: "transportertypeid");

            migrationBuilder.CreateIndex(
                name: "IX_user_group_userid",
                schema: "app",
                table: "user_group",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "ix_users_accountid_username",
                schema: "app",
                table: "users",
                columns: new[] { "accountid", "username" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_branding",
                schema: "app");

            migrationBuilder.DropTable(
                name: "account_features",
                schema: "app");

            migrationBuilder.DropTable(
                name: "account_settings",
                schema: "app");

            migrationBuilder.DropTable(
                name: "account_support_grants",
                schema: "app");

            migrationBuilder.DropTable(
                name: "alert_events",
                schema: "app");

            migrationBuilder.DropTable(
                name: "alert_subscriptions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "audit_events",
                schema: "app");

            migrationBuilder.DropTable(
                name: "background_job_runs",
                schema: "app");

            migrationBuilder.DropTable(
                name: "credentials",
                schema: "app");

            migrationBuilder.DropTable(
                name: "document_signatures",
                schema: "app");

            migrationBuilder.DropTable(
                name: "document_types",
                schema: "app");

            migrationBuilder.DropTable(
                name: "document_versions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "app");

            migrationBuilder.DropTable(
                name: "driver_qualifications",
                schema: "app");

            migrationBuilder.DropTable(
                name: "driver_transporter_assignments",
                schema: "app");

            migrationBuilder.DropTable(
                name: "drivers",
                schema: "app");

            migrationBuilder.DropTable(
                name: "geocoding_providers",
                schema: "map");

            migrationBuilder.DropTable(
                name: "notification_deliveries",
                schema: "app");

            migrationBuilder.DropTable(
                name: "notification_rules",
                schema: "app");

            migrationBuilder.DropTable(
                name: "notification_templates",
                schema: "app");

            migrationBuilder.DropTable(
                name: "operator_health_checks",
                schema: "telemetry");

            migrationBuilder.DropTable(
                name: "operator_sync_runs",
                schema: "telemetry");

            migrationBuilder.DropTable(
                name: "platform_announcements",
                schema: "app");

            migrationBuilder.DropTable(
                name: "points_of_interest",
                schema: "map");

            migrationBuilder.DropTable(
                name: "public_link_grants",
                schema: "app");

            migrationBuilder.DropTable(
                name: "reports",
                schema: "app");

            migrationBuilder.DropTable(
                name: "transporter_device_assignments",
                schema: "app");

            migrationBuilder.DropTable(
                name: "transporter_group",
                schema: "app");

            migrationBuilder.DropTable(
                name: "transporter_position",
                schema: "telemetry");

            migrationBuilder.DropTable(
                name: "transporter_position_history",
                schema: "telemetry");

            migrationBuilder.DropTable(
                name: "user_group",
                schema: "app");

            migrationBuilder.DropTable(
                name: "user_settings",
                schema: "app");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "app");

            migrationBuilder.DropTable(
                name: "transporters",
                schema: "app");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "app");

            migrationBuilder.DropTable(
                name: "users",
                schema: "app");

            migrationBuilder.DropTable(
                name: "operators",
                schema: "app");

            migrationBuilder.DropTable(
                name: "transporter_type",
                schema: "app");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "app");
        }
    }
}
