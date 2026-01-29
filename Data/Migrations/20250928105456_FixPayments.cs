using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedPaymentType",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedPaymentType",
                table: "Payment");
        }
    }
}
