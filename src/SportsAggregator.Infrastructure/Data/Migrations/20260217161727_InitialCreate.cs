using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsAggregator.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sport_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    competition_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    home_team = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    away_team = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    match_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_games_competition_name",
                table: "games",
                column: "competition_name");

            migrationBuilder.CreateIndex(
                name: "IX_games_match_key_scheduled_at_utc",
                table: "games",
                columns: new[] { "match_key", "scheduled_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_games_scheduled_at_utc",
                table: "games",
                column: "scheduled_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_games_sport_type",
                table: "games",
                column: "sport_type");

            migrationBuilder.CreateIndex(
                name: "IX_games_sport_type_scheduled_at_utc",
                table: "games",
                columns: new[] { "sport_type", "scheduled_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "games");
        }
    }
}
