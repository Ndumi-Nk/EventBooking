using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.PackageId);
                    table.ForeignKey(
                        name: "FK_Packages_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageCaterings",
                columns: table => new
                {
                    PackageCateringId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    CateringMenuMenuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageCaterings", x => x.PackageCateringId);
                    table.ForeignKey(
                        name: "FK_PackageCaterings_CateringMenus_CateringMenuMenuId",
                        column: x => x.CateringMenuMenuId,
                        principalTable: "CateringMenus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageCaterings_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageServices",
                columns: table => new
                {
                    PackageServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    AdditionalServiceServiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageServices", x => x.PackageServiceId);
                    table.ForeignKey(
                        name: "FK_PackageServices_AdditionalServices_AdditionalServiceServiceId",
                        column: x => x.AdditionalServiceServiceId,
                        principalTable: "AdditionalServices",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageServices_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageCaterings_CateringMenuMenuId",
                table: "PackageCaterings",
                column: "CateringMenuMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageCaterings_PackageId",
                table: "PackageCaterings",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_EventId",
                table: "Packages",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageServices_AdditionalServiceServiceId",
                table: "PackageServices",
                column: "AdditionalServiceServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageServices_PackageId",
                table: "PackageServices",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageCaterings");

            migrationBuilder.DropTable(
                name: "PackageServices");

            migrationBuilder.DropTable(
                name: "Packages");
        }
    }
}
