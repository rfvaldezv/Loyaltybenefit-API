using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loyaltybenefit.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddRedemptionPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RedemptionPoints",
                table: "LoyaltyRules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedemptionPoints",
                table: "LoyaltyRules");
        }
    }
}
