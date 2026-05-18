using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpsSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlaStateAtRiskThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtRiskThresholdPercent",
                table: "TicketSlaStates",
                type: "int",
                nullable: false,
                defaultValue: 80);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtRiskThresholdPercent",
                table: "TicketSlaStates");
        }
    }
}
