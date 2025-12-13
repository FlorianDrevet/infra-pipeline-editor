using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name_Value",
                table: "ResourceGroup",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Name_Value",
                table: "InfrastructureConfig",
                newName: "Name");

            migrationBuilder.CreateTable(
                name: "infrastructureconfig_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    InfraConfigId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_infrastructureconfig_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_infrastructureconfig_members_InfrastructureConfig_InfraConf~",
                        column: x => x.InfraConfigId,
                        principalTable: "InfrastructureConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntraId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name_FirstName = table.Column<string>(type: "text", nullable: false),
                    Name_LastName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_infrastructureconfig_members_InfraConfigId_UserId",
                table: "infrastructureconfig_members",
                columns: new[] { "InfraConfigId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_EntraId",
                table: "User",
                column: "EntraId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "infrastructureconfig_members");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ResourceGroup",
                newName: "Name_Value");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "InfrastructureConfig",
                newName: "Name_Value");
        }
    }
}
