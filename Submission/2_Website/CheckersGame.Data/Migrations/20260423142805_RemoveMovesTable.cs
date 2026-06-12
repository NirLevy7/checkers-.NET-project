using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckersGame.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMovesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Moves");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Moves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    CapturedCol = table.Column<int>(type: "int", nullable: true),
                    CapturedRow = table.Column<int>(type: "int", nullable: true),
                    FromCol = table.Column<int>(type: "int", nullable: false),
                    FromRow = table.Column<int>(type: "int", nullable: false),
                    IsBackward = table.Column<bool>(type: "bit", nullable: false),
                    MoveNumber = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    Side = table.Column<int>(type: "int", nullable: false),
                    ToCol = table.Column<int>(type: "int", nullable: false),
                    ToRow = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Moves_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_GameId",
                table: "Moves",
                column: "GameId");
        }
    }
}
