using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitBasket.Data.Migrations
{
    /// <inheritdoc />
    public partial class memberxpurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_MemberId",
                table: "Purchases",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Members_MemberId",
                table: "Purchases",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Members_MemberId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_MemberId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Purchases");
        }
    }
}
