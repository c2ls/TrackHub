using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackHub.Manager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProtocolTypeProviders : Migration
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
                    SELECT generate_series(11, 17)
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
            => migrationBuilder.Sql(Rebuild("true"));

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.Sql(Rebuild("v NOT BETWEEN 11 AND 17"));
    }
}
