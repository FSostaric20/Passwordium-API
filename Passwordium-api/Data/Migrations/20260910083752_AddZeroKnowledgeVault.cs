using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordium_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddZeroKnowledgeVault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "Users",
                newName: "VaultSalt");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "Users",
                newName: "RefreshTokenExpiresAt");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Accounts",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Accounts",
                newName: "Nonce");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Accounts",
                newName: "EncryptedData");

            migrationBuilder.AddColumn<string>(
                name: "Challenge",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedVaultKey",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VaultKeyNonce",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VaultKeyTag",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Challenge",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EncryptedVaultKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VaultKeyNonce",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VaultKeyTag",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "VaultSalt",
                table: "Users",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiresAt",
                table: "Users",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "Accounts",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "Nonce",
                table: "Accounts",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "EncryptedData",
                table: "Accounts",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Accounts",
                type: "text",
                nullable: true);
        }
    }
}
