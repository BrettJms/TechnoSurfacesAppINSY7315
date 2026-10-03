using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoSurfaces.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRateDerivationFactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DerivationFactor",
                table: "RateItems",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DerivationFactor",
                table: "CostingLines",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DerivationFactor",
                table: "RateItems");

            migrationBuilder.DropColumn(
                name: "DerivationFactor",
                table: "CostingLines");
        }
    }
}
