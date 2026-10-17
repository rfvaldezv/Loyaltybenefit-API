using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loyaltybenefit.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddUniqueStayEarnTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_StayId",
                table: "PointTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_StayId",
                table: "PointTransactions",
                column: "StayId",
                unique: true,
                filter: "[StayId] IS NOT NULL AND [Type] = 'STAY_EARN'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_StayId",
                table: "PointTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_StayId",
                table: "PointTransactions",
                column: "StayId");
        }
    }
}
