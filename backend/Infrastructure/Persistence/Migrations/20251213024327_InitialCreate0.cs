using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignPosts_SocialPlatforms_PlatformId",
                table: "CampaignPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_AspNetUsers_CreatedByUserId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Products_AssignedProductId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderProducts_Products_ProductId",
                table: "OrderProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_OwnerUserId",
                table: "Stores");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_AspNetRoles_RoleId",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_AspNetUsers_UserId",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_RoleId",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_Stores_Email",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_IsActive",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_SocialPlatforms_PageId",
                table: "SocialPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_SocialPlatforms_StoreId_PlatformName",
                table: "SocialPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_Products_SKU",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_State",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_CampaignPosts_PlatformId",
                table: "CampaignPosts");

            migrationBuilder.DropIndex(
                name: "IX_CampaignPosts_Status",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "AccountUsername",
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
                name: "ProfilePictureUrl",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "FulfilledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ChatbotFAQs");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "ExternalPostId",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "PostType",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "ScheduledJobId",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CampaignPosts");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Teams",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Stores",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "TokenExpiresAt",
                table: "SocialPlatforms",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "RefreshTokens",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Products",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "SKU",
                table: "Products",
                newName: "RetailerId");

            migrationBuilder.RenameColumn(
                name: "ProductImageUrl",
                table: "Products",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Orders",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Customers",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Customers",
                newName: "PSID");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                newName: "IX_Customers_PSID");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ChatbotFAQs",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "ChatbotFAQs",
                newName: "PSID");

            migrationBuilder.RenameIndex(
                name: "IX_ChatbotFAQs_Category",
                table: "ChatbotFAQs",
                newName: "IX_ChatbotFAQs_PSID");

            migrationBuilder.RenameColumn(
                name: "TargetAudienceJson",
                table: "Campaigns",
                newName: "TargetAudience");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "CampaignPosts",
                newName: "DeletedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Teams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TeamMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "TeamMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TeamMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "TeamMembers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Stores",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SocialPlatforms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlatformName",
                table: "SocialPlatforms",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PageName",
                table: "SocialPlatforms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "SocialPlatforms",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "SocialPlatforms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalPageID",
                table: "SocialPlatforms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SocialPlatforms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalProductID",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrderProducts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrderProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Question",
                table: "ChatbotFAQs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "ChatbotFAQs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "ChatbotFAQs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChatbotFAQs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "ChatbotFAQs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Campaigns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Campaigns",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "CampaignStage",
                table: "Campaigns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Campaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Campaigns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "CampaignPosts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CampaignPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AutomationTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelatedCampaignPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CronExpression = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationTasks_CampaignPosts_RelatedCampaignPostId",
                        column: x => x.RelatedCampaignPostId,
                        principalTable: "CampaignPosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AutomationTasks_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_IsDeleted",
                table: "Teams",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_IsDeleted",
                table: "TeamMembers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IsDeleted",
                table: "Stores",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPlatforms_ExternalPageID",
                table: "SocialPlatforms",
                column: "ExternalPageID");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPlatforms_IsDeleted",
                table: "SocialPlatforms",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ExternalProductID",
                table: "Products",
                column: "ExternalProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsDeleted",
                table: "Products",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IsDeleted",
                table: "Orders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_IsDeleted",
                table: "OrderProducts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsDeleted",
                table: "Customers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotFAQs_IsDeleted",
                table: "ChatbotFAQs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotFAQs_MessageType",
                table: "ChatbotFAQs",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CampaignStage",
                table: "Campaigns",
                column: "CampaignStage");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_IsDeleted",
                table: "Campaigns",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPosts_IsDeleted",
                table: "CampaignPosts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationTasks_IsActive",
                table: "AutomationTasks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationTasks_IsDeleted",
                table: "AutomationTasks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationTasks_RelatedCampaignPostId",
                table: "AutomationTasks",
                column: "RelatedCampaignPostId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationTasks_StoreId",
                table: "AutomationTasks",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationTasks_TaskType",
                table: "AutomationTasks",
                column: "TaskType");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_AspNetUsers_CreatedByUserId",
                table: "Campaigns",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Products_AssignedProductId",
                table: "Campaigns",
                column: "AssignedProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProducts_Products_ProductId",
                table: "OrderProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_OwnerUserId",
                table: "Stores",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_AspNetUsers_UserId",
                table: "TeamMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_AspNetUsers_CreatedByUserId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Products_AssignedProductId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderProducts_Products_ProductId",
                table: "OrderProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_OwnerUserId",
                table: "Stores");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_AspNetUsers_UserId",
                table: "TeamMembers");

            migrationBuilder.DropTable(
                name: "AutomationTasks");

            migrationBuilder.DropIndex(
                name: "IX_Teams_IsDeleted",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_IsDeleted",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_Stores_IsDeleted",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_SocialPlatforms_ExternalPageID",
                table: "SocialPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_SocialPlatforms_IsDeleted",
                table: "SocialPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_Products_ExternalProductID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsDeleted",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IsDeleted",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderProducts_IsDeleted",
                table: "OrderProducts");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IsDeleted",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_ChatbotFAQs_IsDeleted",
                table: "ChatbotFAQs");

            migrationBuilder.DropIndex(
                name: "IX_ChatbotFAQs_MessageType",
                table: "ChatbotFAQs");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_CampaignStage",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_IsDeleted",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_CampaignPosts_IsDeleted",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "ExternalPageID",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SocialPlatforms");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalProductID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ChatbotFAQs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChatbotFAQs");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "ChatbotFAQs");

            migrationBuilder.DropColumn(
                name: "CampaignStage",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "CampaignPosts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CampaignPosts");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Teams",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Stores",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "SocialPlatforms",
                newName: "TokenExpiresAt");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "RefreshTokens",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Products",
                newName: "ProductImageUrl");

            migrationBuilder.RenameColumn(
                name: "RetailerId",
                table: "Products",
                newName: "SKU");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Products",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Orders",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "PSID",
                table: "Customers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Customers",
                newName: "UpdatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_PSID",
                table: "Customers",
                newName: "IX_Customers_Email");

            migrationBuilder.RenameColumn(
                name: "PSID",
                table: "ChatbotFAQs",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "ChatbotFAQs",
                newName: "UpdatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ChatbotFAQs_PSID",
                table: "ChatbotFAQs",
                newName: "IX_ChatbotFAQs_Category");

            migrationBuilder.RenameColumn(
                name: "TargetAudience",
                table: "Campaigns",
                newName: "TargetAudienceJson");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "CampaignPosts",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "TeamMembers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Stores",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Stores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SocialPlatforms",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "PlatformName",
                table: "SocialPlatforms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "PageName",
                table: "SocialPlatforms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "SocialPlatforms",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "AccountUsername",
                table: "SocialPlatforms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

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
                name: "ProfilePictureUrl",
                table: "SocialPlatforms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "SocialPlatforms",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SocialPlatforms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FulfilledAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Question",
                table: "ChatbotFAQs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "ChatbotFAQs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ChatbotFAQs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Campaigns",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Campaigns",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStage",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "CampaignPosts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalPostId",
                table: "CampaignPosts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformId",
                table: "CampaignPosts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "PostType",
                table: "CampaignPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "CampaignPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduledJobId",
                table: "CampaignPosts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CampaignPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_RoleId",
                table: "TeamMembers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Email",
                table: "Stores",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IsActive",
                table: "Stores",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPlatforms_PageId",
                table: "SocialPlatforms",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPlatforms_StoreId_PlatformName",
                table: "SocialPlatforms",
                columns: new[] { "StoreId", "PlatformName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_State",
                table: "Campaigns",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPosts_PlatformId",
                table: "CampaignPosts",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPosts_Status",
                table: "CampaignPosts",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignPosts_SocialPlatforms_PlatformId",
                table: "CampaignPosts",
                column: "PlatformId",
                principalTable: "SocialPlatforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_AspNetUsers_CreatedByUserId",
                table: "Campaigns",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Products_AssignedProductId",
                table: "Campaigns",
                column: "AssignedProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProducts_Products_ProductId",
                table: "OrderProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_OwnerUserId",
                table: "Stores",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_AspNetRoles_RoleId",
                table: "TeamMembers",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_AspNetUsers_UserId",
                table: "TeamMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
