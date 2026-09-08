using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordium_api.Data.Migrations {
    /// <inheritdoc />
    public partial class AddColumnChallengeExpiresAt : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<DateTime>(
                name: "ChallengeExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "ChallengeExpiresAt",
                table: "Users");
        }
    }
}
