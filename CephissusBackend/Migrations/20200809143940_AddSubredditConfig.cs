using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CephissusBackend.Migrations
{
    public partial class AddSubredditConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubredditConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SubredditName = table.Column<string>(nullable: false),
                    MinScore = table.Column<int>(nullable: false),
                    MaxFetch = table.Column<int>(nullable: false),
                    MinAspectRatio = table.Column<double>(nullable: false),
                    MinHeight = table.Column<int>(nullable: false),
                    AllowNsfw = table.Column<bool>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubredditConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubredditConfigs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubredditConfigs_UserId",
                table: "SubredditConfigs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubredditConfigs");
        }
    }
}
