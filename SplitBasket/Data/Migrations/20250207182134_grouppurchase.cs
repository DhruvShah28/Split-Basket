using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitBasket.Data.Migrations
{
    /// <inheritdoc />
    public partial class grouppurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupPurchases",
                columns: table => new
                {
                    GroupPurchaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroceryItemId = table.Column<int>(type: "int", nullable: false),
                    PurchaseId = table.Column<int>(type: "int", nullable: true),
                    IsBought = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPurchases", x => x.GroupPurchaseId);
                    table.ForeignKey(
                        name: "FK_GroupPurchases_GroceryItems_GroceryItemId",
                        column: x => x.GroceryItemId,
                        principalTable: "GroceryItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPurchases_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupPurchases_GroceryItemId",
                table: "GroupPurchases",
                column: "GroceryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPurchases_PurchaseId",
                table: "GroupPurchases",
                column: "PurchaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupPurchases");
        }
    }
}
