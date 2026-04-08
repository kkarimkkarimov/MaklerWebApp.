using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaklerWebApp.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase1ListingAttributesAndEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentStatus",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsMortgageEligible",
                table: "Listings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RepairStatus",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_RepairStatus_DocumentStatus_IsMortgageEligible",
                table: "Listings",
                columns: new[] { "RepairStatus", "DocumentStatus", "IsMortgageEligible" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Listings_RepairStatus_DocumentStatus_IsMortgageEligible",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "DocumentStatus",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "IsMortgageEligible",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RepairStatus",
                table: "Listings");
        }
    }
}
