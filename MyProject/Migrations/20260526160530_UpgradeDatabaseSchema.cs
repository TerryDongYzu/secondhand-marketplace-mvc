using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TableItems1131609",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "IsSold",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TableItems1131609");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "TableItems1131609",
                newName: "Owner_ID");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "TableItems1131609",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TableItems1131609",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Owner_ID",
                table: "TableItems1131609",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "Item_ID",
                table: "TableItems1131609",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category_ID",
                table: "TableItems1131609",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created_At",
                table: "TableItems1131609",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Item_Condition",
                table: "TableItems1131609",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TableItems1131609",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TableItems1131609",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableItems1131609",
                table: "TableItems1131609",
                column: "Item_ID");

            migrationBuilder.CreateTable(
                name: "TableCategorys1131609",
                columns: table => new
                {
                    Category_ID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableCategorys1131609", x => x.Category_ID);
                });

            migrationBuilder.CreateTable(
                name: "TableComments1131609",
                columns: table => new
                {
                    Comment_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Item_ID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableComments1131609", x => x.Comment_ID);
                    table.ForeignKey(
                        name: "FK_TableComments1131609_TableItems1131609_Item_ID",
                        column: x => x.Item_ID,
                        principalTable: "TableItems1131609",
                        principalColumn: "Item_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableComments1131609_TableMembers1131609_User_ID",
                        column: x => x.User_ID,
                        principalTable: "TableMembers1131609",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableItemImages1131609",
                columns: table => new
                {
                    Item_ID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Image_URL = table.Column<string>(type: "nvarchar(2083)", maxLength: 2083, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableItemImages1131609", x => new { x.Item_ID, x.Image_URL });
                    table.ForeignKey(
                        name: "FK_TableItemImages1131609_TableItems1131609_Item_ID",
                        column: x => x.Item_ID,
                        principalTable: "TableItems1131609",
                        principalColumn: "Item_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableItems1131609_Category_ID",
                table: "TableItems1131609",
                column: "Category_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TableComments1131609_Item_ID",
                table: "TableComments1131609",
                column: "Item_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TableComments1131609_User_ID",
                table: "TableComments1131609",
                column: "User_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TableItems1131609_TableCategorys1131609_Category_ID",
                table: "TableItems1131609",
                column: "Category_ID",
                principalTable: "TableCategorys1131609",
                principalColumn: "Category_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableItems1131609_TableCategorys1131609_Category_ID",
                table: "TableItems1131609");

            migrationBuilder.DropTable(
                name: "TableCategorys1131609");

            migrationBuilder.DropTable(
                name: "TableComments1131609");

            migrationBuilder.DropTable(
                name: "TableItemImages1131609");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TableItems1131609",
                table: "TableItems1131609");

            migrationBuilder.DropIndex(
                name: "IX_TableItems1131609_Category_ID",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "Item_ID",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "Category_ID",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "Created_At",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "Item_Condition",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TableItems1131609");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TableItems1131609");

            migrationBuilder.RenameColumn(
                name: "Owner_ID",
                table: "TableItems1131609",
                newName: "OwnerId");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "TableItems1131609",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TableItems1131609",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "TableItems1131609",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TableItems1131609",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsSold",
                table: "TableItems1131609",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TableItems1131609",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableItems1131609",
                table: "TableItems1131609",
                column: "Id");
        }
    }
}
