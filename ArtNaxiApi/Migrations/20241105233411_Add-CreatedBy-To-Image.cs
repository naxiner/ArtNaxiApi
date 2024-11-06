using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtNaxiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Images",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Images");
        }
    }
}
