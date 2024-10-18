using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtNaxiApi.Migrations
{
    /// <inheritdoc />
    public partial class Update_Image : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SDRequests_ImageId",
                table: "SDRequests");

            migrationBuilder.DropColumn(
                name: "SDRequestId",
                table: "Images");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "SDRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_SDRequests_ImageId",
                table: "SDRequests",
                column: "ImageId",
                unique: true,
                filter: "[ImageId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SDRequests_ImageId",
                table: "SDRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "SDRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SDRequestId",
                table: "Images",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SDRequests_ImageId",
                table: "SDRequests",
                column: "ImageId",
                unique: true);
        }
    }
}
