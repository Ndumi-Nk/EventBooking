using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAdditional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "AdditionalServices",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "AdditionalServices");
        }
    }
}
