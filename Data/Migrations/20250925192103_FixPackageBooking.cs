using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPackageBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "PackageBookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "PackageBookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "PackageBookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialRequests",
                table: "PackageBookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "PackageBookings");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "PackageBookings");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "PackageBookings");

            migrationBuilder.DropColumn(
                name: "SpecialRequests",
                table: "PackageBookings");
        }
    }
}
