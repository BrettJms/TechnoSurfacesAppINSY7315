using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoSurfaces.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCostingLineCatalogueForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CostingLines_MaterialPriceId",
                table: "CostingLines",
                column: "MaterialPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_CostingLines_RateItemId",
                table: "CostingLines",
                column: "RateItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_CostingLines_MaterialPrices_MaterialPriceId",
                table: "CostingLines",
                column: "MaterialPriceId",
                principalTable: "MaterialPrices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CostingLines_RateItems_RateItemId",
                table: "CostingLines",
                column: "RateItemId",
                principalTable: "RateItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CostingLines_MaterialPrices_MaterialPriceId",
                table: "CostingLines");

            migrationBuilder.DropForeignKey(
                name: "FK_CostingLines_RateItems_RateItemId",
                table: "CostingLines");

            migrationBuilder.DropIndex(
                name: "IX_CostingLines_MaterialPriceId",
                table: "CostingLines");

            migrationBuilder.DropIndex(
                name: "IX_CostingLines_RateItemId",
                table: "CostingLines");
        }
    }
}
