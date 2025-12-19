using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertOrderStatusToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a temporary column for the string value
            migrationBuilder.AddColumn<string>(
                name: "Status_Temp",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Step 2: Convert existing int values to string enum names
            migrationBuilder.Sql(@"
                UPDATE Orders 
                SET Status_Temp = CASE Status
                    WHEN 0 THEN 'Pending'
                    WHEN 1 THEN 'Accepted'
                    WHEN 2 THEN 'Shipped'
                    WHEN 3 THEN 'Delivered'
                    WHEN 4 THEN 'Rejected'
                    WHEN 5 THEN 'Cancelled'
                    WHEN 6 THEN 'Refunded'
                    ELSE 'Pending'
                END
            ");

            // Step 3: Drop the old int column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            // Step 4: Rename temp column to Status
            migrationBuilder.RenameColumn(
                name: "Status_Temp",
                table: "Orders",
                newName: "Status");

            // Step 5: Make the column non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            // Step 6: Create index
            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop the index
            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                table: "Orders");

            // Step 2: Add temporary int column
            migrationBuilder.AddColumn<int>(
                name: "Status_Temp",
                table: "Orders",
                type: "int",
                nullable: true);

            // Step 3: Convert string values back to int
            migrationBuilder.Sql(@"
                UPDATE Orders 
                SET Status_Temp = CASE Status
                    WHEN 'Pending' THEN 0
                    WHEN 'Accepted' THEN 1
                    WHEN 'Shipped' THEN 2
                    WHEN 'Delivered' THEN 3
                    WHEN 'Rejected' THEN 4
                    WHEN 'Cancelled' THEN 5
                    WHEN 'Refunded' THEN 6
                    ELSE 0
                END
            ");

            // Step 4: Drop the string column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            // Step 5: Rename temp column back to Status
            migrationBuilder.RenameColumn(
                name: "Status_Temp",
                table: "Orders",
                newName: "Status");

            // Step 6: Make the column non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
