using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemId",
                table: "TableOrders1131609",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                table: "TableOrders1131609",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "TableOrders1131609",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MeetLocation",
                table: "TableOrders1131609",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MeetTime",
                table: "TableOrders1131609",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TableOrders1131609_ItemId",
                table: "TableOrders1131609",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableOrders1131609_TableItems1131609_ItemId",
                table: "TableOrders1131609",
                column: "ItemId",
                principalTable: "TableItems1131609",
                principalColumn: "Item_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableOrders1131609_TableItems1131609_ItemId",
                table: "TableOrders1131609");

            migrationBuilder.DropIndex(
                name: "IX_TableOrders1131609_ItemId",
                table: "TableOrders1131609");

            migrationBuilder.DropColumn(
                name: "BuyerName",
                table: "TableOrders1131609");

            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "TableOrders1131609");

            migrationBuilder.DropColumn(
                name: "MeetLocation",
                table: "TableOrders1131609");

            migrationBuilder.DropColumn(
                name: "MeetTime",
                table: "TableOrders1131609");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "TableOrders1131609",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
        }
    }
}
