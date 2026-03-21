using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberUserNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_infrastructureconfig_members_UserId",
                table: "infrastructureconfig_members",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_infrastructureconfig_members_User_UserId",
                table: "infrastructureconfig_members",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_infrastructureconfig_members_User_UserId",
                table: "infrastructureconfig_members");

            migrationBuilder.DropIndex(
                name: "IX_infrastructureconfig_members_UserId",
                table: "infrastructureconfig_members");
        }
    }
}
