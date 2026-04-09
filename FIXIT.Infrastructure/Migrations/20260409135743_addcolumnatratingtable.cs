using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIXIT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addcolumnatratingtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "ProviderRates",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRates_CustomerId",
                table: "ProviderRates",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderRates_Customers_CustomerId",
                table: "ProviderRates",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderRates_Customers_CustomerId",
                table: "ProviderRates");

            migrationBuilder.DropIndex(
                name: "IX_ProviderRates_CustomerId",
                table: "ProviderRates");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "ProviderRates");
        }
    }
}
