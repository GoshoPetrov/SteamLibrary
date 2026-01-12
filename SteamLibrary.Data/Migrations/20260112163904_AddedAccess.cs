using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccessId",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Accesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accesses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_AccessId",
                table: "Users",
                column: "AccessId");

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_Name",
                table: "Accesses",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Accesses_AccessId",
                table: "Users",
                column: "AccessId",
                principalTable: "Accesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Accesses_AccessId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Accesses");

            migrationBuilder.DropIndex(
                name: "IX_Users_AccessId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AccessId",
                table: "Users");
        }
    }
}
