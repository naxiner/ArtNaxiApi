using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtNaxiApi.Migrations
{
    /// <inheritdoc />
    public partial class Add_SDRequest_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SDRequestId",
                table: "Images",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SDRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NegativePrompt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Styles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SamplerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scheduler = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Steps = table.Column<int>(type: "int", nullable: false),
                    CfgScale = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDRequests", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_SDRequests_SDRequestId",
                table: "Images");

            migrationBuilder.DropTable(
                name: "SDRequests");

            migrationBuilder.DropIndex(
                name: "IX_Images_SDRequestId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "SDRequestId",
                table: "Images");
        }
    }
}
