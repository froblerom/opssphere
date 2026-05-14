using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpsSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserScopeUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UQ_UserScopes_User_Account",
                table: "UserScopes",
                columns: new[] { "UserId", "ScopeType", "AccountId" },
                unique: true,
                filter: "[AccountId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_UserScopes_User_Campaign",
                table: "UserScopes",
                columns: new[] { "UserId", "ScopeType", "CampaignId" },
                unique: true,
                filter: "[CampaignId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_UserScopes_User_Country",
                table: "UserScopes",
                columns: new[] { "UserId", "ScopeType", "CountryId" },
                unique: true,
                filter: "[CountryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_UserScopes_User_Region",
                table: "UserScopes",
                columns: new[] { "UserId", "ScopeType", "RegionId" },
                unique: true,
                filter: "[RegionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_UserScopes_User_Account",
                table: "UserScopes");

            migrationBuilder.DropIndex(
                name: "UQ_UserScopes_User_Campaign",
                table: "UserScopes");

            migrationBuilder.DropIndex(
                name: "UQ_UserScopes_User_Country",
                table: "UserScopes");

            migrationBuilder.DropIndex(
                name: "UQ_UserScopes_User_Region",
                table: "UserScopes");
        }
    }
}
