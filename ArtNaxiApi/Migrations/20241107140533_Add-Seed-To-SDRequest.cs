using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtNaxiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedToSDRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Seed",
                table: "SDRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seed",
                table: "SDRequests");
        }
    }
}
