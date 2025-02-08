using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitBasket.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGroupPurchasesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupPurchases_Purchases_PurchaseId",
                table: "GroupPurchases");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPurchases_Purchases_PurchaseId",
                table: "GroupPurchases",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "PurchaseID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupPurchases_Purchases_PurchaseId",
                table: "GroupPurchases");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPurchases_Purchases_PurchaseId",
                table: "GroupPurchases",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "PurchaseID");
        }
    }
}
