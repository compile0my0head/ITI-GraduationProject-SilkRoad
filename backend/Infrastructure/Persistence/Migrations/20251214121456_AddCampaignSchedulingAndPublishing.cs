using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignSchedulingAndPublishing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSchedulingEnabled",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledEndAt",
                table: "Campaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledStartAt",
                table: "Campaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPublishError",
                table: "CampaignPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishStatus",
                table: "CampaignPosts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "CampaignPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignPostPlatforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalPostId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublishStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPostPlatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignPostPlatforms_CampaignPosts_CampaignPostId",
                        column: x => x.CampaignPostId,
                        principalTable: "CampaignPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignPostPlatforms_SocialPlatforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "SocialPlatforms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPostPlatforms_CampaignPostId",
                table: "CampaignPostPlatforms",
                column: "CampaignPostId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPostPlatforms_CampaignPostId_PlatformId",
                table: "CampaignPostPlatforms",
                columns: new[] { "CampaignPostId", "PlatformId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPostPlatforms_PlatformId",
                table: "CampaignPostPlatforms",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPostPlatforms_PublishStatus",
                table: "CampaignPostPlatforms",
                column: "PublishStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPostPlatforms_ScheduledAt",
                table: "CampaignPostPlatforms",
                column: "ScheduledAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignPostPlatforms");

            migrationBuilder.DropColumn(
                name: "IsSchedulingEnabled",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "ScheduledEndAt",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "ScheduledStartAt",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "LastPublishError",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "CampaignPosts");
        }
    }
}
