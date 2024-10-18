using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtNaxiApi.Migrations
{
    /// <inheritdoc />
    public partial class Update_SDRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_SDRequests_SDRequestId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_SDRequestId",
                table: "Images");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "SDRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SDRequests_ImageId",
                table: "SDRequests",
                column: "ImageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SDRequests_Images_ImageId",
                table: "SDRequests",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SDRequests_Images_ImageId",
                table: "SDRequests");

            migrationBuilder.DropIndex(
                name: "IX_SDRequests_ImageId",
                table: "SDRequests");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "SDRequests");

            migrationBuilder.CreateIndex(
                name: "IX_Images_SDRequestId",
                table: "Images",
                column: "SDRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_SDRequests_SDRequestId",
                table: "Images",
                column: "SDRequestId",
                principalTable: "SDRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
