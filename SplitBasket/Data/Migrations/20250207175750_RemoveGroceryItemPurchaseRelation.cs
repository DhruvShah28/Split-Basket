using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitBasket.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGroceryItemPurchaseRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroceryItems_Purchases_PurchaseId",
                table: "GroceryItems");

            migrationBuilder.DropIndex(
                name: "IX_GroceryItems_PurchaseId",
                table: "GroceryItems");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "GroceryItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseId",
                table: "GroceryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GroceryItems_PurchaseId",
                table: "GroceryItems",
                column: "PurchaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroceryItems_Purchases_PurchaseId",
                table: "GroceryItems",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "PurchaseID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
