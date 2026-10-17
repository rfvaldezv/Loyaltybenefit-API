using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loyaltybenefit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Port",
                table: "TenantDatabases");

            migrationBuilder.DropColumn(
                name: "ServerName",
                table: "TenantDatabases");

            migrationBuilder.RenameColumn(
                name: "DatabaseName",
                table: "TenantDatabases",
                newName: "PhysicalDatabaseName");

            migrationBuilder.AddColumn<Guid>(
                name: "DatabaseServerId",
                table: "TenantDatabases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "LogicalName",
                table: "TenantDatabases",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DatabaseServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EngineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DatabaseServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordSecret = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatabaseCredentials_DatabaseServers_DatabaseServerId",
                        column: x => x.DatabaseServerId,
                        principalTable: "DatabaseServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantDatabases_DatabaseServerId",
                table: "TenantDatabases",
                column: "DatabaseServerId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseCredentials_DatabaseServerId",
                table: "DatabaseCredentials",
                column: "DatabaseServerId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseServers_Code",
                table: "DatabaseServers",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantDatabases_DatabaseServers_DatabaseServerId",
                table: "TenantDatabases",
                column: "DatabaseServerId",
                principalTable: "DatabaseServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantDatabases_DatabaseServers_DatabaseServerId",
                table: "TenantDatabases");

            migrationBuilder.DropTable(
                name: "DatabaseCredentials");

            migrationBuilder.DropTable(
                name: "DatabaseServers");

            migrationBuilder.DropIndex(
                name: "IX_TenantDatabases_DatabaseServerId",
                table: "TenantDatabases");

            migrationBuilder.DropColumn(
                name: "DatabaseServerId",
                table: "TenantDatabases");

            migrationBuilder.DropColumn(
                name: "LogicalName",
                table: "TenantDatabases");

            migrationBuilder.RenameColumn(
                name: "PhysicalDatabaseName",
                table: "TenantDatabases",
                newName: "DatabaseName");

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "TenantDatabases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ServerName",
                table: "TenantDatabases",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
