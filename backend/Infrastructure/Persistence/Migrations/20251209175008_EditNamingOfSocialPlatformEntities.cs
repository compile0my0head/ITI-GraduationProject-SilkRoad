using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EditNamingOfSocialPlatformEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExternalAccountId",
                table: "SocialPlatforms",
                newName: "UserId");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "SocialPlatforms",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "SocialPlatforms",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "SocialPlatforms",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FollowerCount",
                table: "SocialPlatforms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrantedPermissions",
                table: "SocialPlatforms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "SocialPlatforms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PageAccessToken",
                table: "SocialPlatforms",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PageId",
                table: "SocialPlatforms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PageName",
                table: "SocialPlatforms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "SocialPlatforms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialPlatforms_IsConnected",
                table: "SocialPlatforms",
                column: "IsConnected");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPlatforms_PageId",
                table: "SocialPlatforms",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPlatforms_StoreId_PlatformName",
                table: "SocialPlatforms",
                columns: new[] { "StoreId", "PlatformName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SocialPlatforms_IsConnected",
                table: "SocialPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_SocialPlatforms_PageId",
                table: "SocialPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_SocialPlatforms_StoreId_PlatformName",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "FollowerCount",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "GrantedPermissions",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "PageAccessToken",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "PageId",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "PageName",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "SocialPlatforms");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SocialPlatforms",
                newName: "ExternalAccountId");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "SocialPlatforms",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "SocialPlatforms",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
