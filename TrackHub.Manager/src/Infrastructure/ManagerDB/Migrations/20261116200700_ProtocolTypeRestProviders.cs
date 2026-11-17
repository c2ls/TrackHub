using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.Manager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProtocolTypeRestProviders : Migration
    {
        private const string Constraint = "ck_operators_protocoltype";

        // The constraint is rebuilt from what the database already allows, so values that are not
        // in this model's enum survive the migration in both directions.
        private static string Rebuild(string keep) => $$"""
            DO $do$
            DECLARE
                allowed int[];
            BEGIN
                SELECT array_agg(DISTINCT v ORDER BY v) INTO allowed
                FROM (
                    SELECT (m)[1]::int AS v
                    FROM pg_constraint c, regexp_matches(pg_get_constraintdef(c.oid), '\d+', 'g') AS m
                    WHERE c.conname = '{{Constraint}}'
                    UNION
                    SELECT generate_series(18, 20)
                ) AS protocol_values
                WHERE {{keep}};

                ALTER TABLE app.operators DROP CONSTRAINT {{Constraint}};
                EXECUTE format(
                    'ALTER TABLE app.operators ADD CONSTRAINT {{Constraint}} CHECK (protocoltype IN (%s))',
                    array_to_string(allowed, ', '));
            END $do$;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "protocoltype",
                schema: "app",
                table: "operators",
                type: "integer",
                nullable: false,
                comment: "Telematics protocol this operator speaks; selects the Router provider client. Values: 1=CommandTrack, 2=Traccar, 3=Flespi, 4=GeoTab, 5=GpsGate, 6=Navixy, 7=Samsara, 8=Wialon, 9=Protrack, 10=Mettax, 11=Tracksolid, 12=Motive, 13=Cartrack, 14=Pegasus, 15=Webfleet, 16=RedGps, 17=Plaspy, 18=TrustTrack, 19=Mapon, 20=GpsTrackit.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Telematics protocol this operator speaks; selects the Router provider client. Values: 1=CommandTrack, 2=Traccar, 3=Flespi, 4=GeoTab, 5=GpsGate, 6=Navixy, 7=Samsara, 8=Wialon, 9=Protrack, 10=Mettax, 11=Tracksolid, 12=Motive, 13=Cartrack, 14=Pegasus, 15=Webfleet, 16=RedGps, 17=Plaspy.");

            migrationBuilder.Sql(Rebuild("true"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "protocoltype",
                schema: "app",
                table: "operators",
                type: "integer",
                nullable: false,
                comment: "Telematics protocol this operator speaks; selects the Router provider client. Values: 1=CommandTrack, 2=Traccar, 3=Flespi, 4=GeoTab, 5=GpsGate, 6=Navixy, 7=Samsara, 8=Wialon, 9=Protrack, 10=Mettax, 11=Tracksolid, 12=Motive, 13=Cartrack, 14=Pegasus, 15=Webfleet, 16=RedGps, 17=Plaspy.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Telematics protocol this operator speaks; selects the Router provider client. Values: 1=CommandTrack, 2=Traccar, 3=Flespi, 4=GeoTab, 5=GpsGate, 6=Navixy, 7=Samsara, 8=Wialon, 9=Protrack, 10=Mettax, 11=Tracksolid, 12=Motive, 13=Cartrack, 14=Pegasus, 15=Webfleet, 16=RedGps, 17=Plaspy, 18=TrustTrack, 19=Mapon, 20=GpsTrackit.");

            migrationBuilder.Sql(Rebuild("v NOT BETWEEN 18 AND 20"));
        }
    }
}
