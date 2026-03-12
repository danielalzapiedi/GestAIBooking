using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAI.Infrastructure.Persistence.Migrations
{
    public partial class AddPropertyFeatureSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyFeatureSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    EnableHousekeeping = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableAgenda = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableQuotes = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableSavedQuotes = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnablePromotions = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableAdvancedRates = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnablePayments = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableDirectBooking = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableExternalCalendarSync = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableReports = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableTemplates = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableAuditView = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UseSimpleGuestMode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyFeatureSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyFeatureSettings_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFeatureSettings_PropertyId",
                table: "PropertyFeatureSettings",
                column: "PropertyId",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO PropertyFeatureSettings
                (PropertyId, EnableHousekeeping, EnableAgenda, EnableQuotes, EnableSavedQuotes, EnablePromotions, EnableAdvancedRates,
                 EnablePayments, EnableDirectBooking, EnableExternalCalendarSync, EnableReports, EnableTemplates, EnableAuditView, UseSimpleGuestMode, CreatedAtUtc)
                SELECT p.Id, 1,1,1,1,1,1,1,1,1,1,1,1,0, SYSUTCDATETIME()
                FROM Properties p
                WHERE NOT EXISTS (SELECT 1 FROM PropertyFeatureSettings fs WHERE fs.PropertyId = p.Id);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PropertyFeatureSettings");
        }
    }
}
