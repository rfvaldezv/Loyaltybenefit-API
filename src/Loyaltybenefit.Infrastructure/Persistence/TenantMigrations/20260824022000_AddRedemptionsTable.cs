using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loyaltybenefit.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddRedemptionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Redemptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    LoyaltyAccountId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    Points = table.Column<int>(
                        type: "int",
                        nullable: false),

                    MonetaryValue = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Reference = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    Status = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: false),

                    CreatedAtUtc = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Redemptions",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_Redemptions_LoyaltyAccounts_LoyaltyAccountId",
                        column: x => x.LoyaltyAccountId,
                        principalTable: "LoyaltyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_LoyaltyAccountId",
                table: "Redemptions",
                column: "LoyaltyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_Reference",
                table: "Redemptions",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Redemptions");
        }
    }
}
