using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaklerWebApp.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase1ListingAttributeDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Listings] SET [RepairStatus] = 1 WHERE [RepairStatus] = 0;");
            migrationBuilder.Sql("UPDATE [Listings] SET [DocumentStatus] = 1 WHERE [DocumentStatus] = 0;");

            migrationBuilder.AlterColumn<int>(
                name: "RepairStatus",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsMortgageEligible",
                table: "Listings",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentStatus",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RepairStatus",
                table: "Listings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsMortgageEligible",
                table: "Listings",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "DocumentStatus",
                table: "Listings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);
        }
    }
}
