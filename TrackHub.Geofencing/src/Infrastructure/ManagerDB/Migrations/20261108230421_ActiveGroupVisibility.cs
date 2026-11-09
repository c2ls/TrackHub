using Microsoft.EntityFrameworkCore.Migrations;
using TrackHub.Geofencing.Infrastructure.Resources;

#nullable disable

namespace TrackHub.Geofencing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActiveGroupVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Views.vw_visible_transporter);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW geofencing.vw_visible_transporter AS
                SELECT transporters.id AS transporterid, transporters.accountid, user_group.userid
                FROM app.transporters
                JOIN app.transporter_group ON transporter_group.transporterid = transporters.id
                JOIN app.user_group ON user_group.groupid = transporter_group.groupid;
                """);
        }
    }
}
