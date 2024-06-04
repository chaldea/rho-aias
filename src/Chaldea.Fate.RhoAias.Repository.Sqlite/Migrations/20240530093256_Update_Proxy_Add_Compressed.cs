using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chaldea.Fate.RhoAias.Repository.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Update_Proxy_Add_Compressed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Compressed",
                table: "Proxies",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Compressed",
                table: "Proxies");
        }
    }
}
