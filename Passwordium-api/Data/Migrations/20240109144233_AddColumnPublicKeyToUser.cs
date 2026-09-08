using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordium_api.Data.Migrations {
    /// <inheritdoc />
    public partial class AddColumnPublicKeyToUser : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "Users");
        }
    }
}
