using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chaldea.Fate.RhoAias.Repository.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Update_Proxy_Add_Route : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClusterConfig",
                table: "Proxies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "Proxies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RouteConfig",
                table: "Proxies",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClusterConfig",
                table: "Proxies");

            migrationBuilder.DropColumn(
                name: "Destination",
                table: "Proxies");

            migrationBuilder.DropColumn(
                name: "RouteConfig",
                table: "Proxies");
        }
    }
}
