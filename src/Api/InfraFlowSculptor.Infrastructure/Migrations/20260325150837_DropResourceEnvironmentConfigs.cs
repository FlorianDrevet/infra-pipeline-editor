using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropResourceEnvironmentConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceEnvironmentConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResourceEnvironmentConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Properties = table.Column<string>(type: "jsonb", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceEnvironmentConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceEnvironmentConfigs_ResourceId_EnvironmentName",
                table: "ResourceEnvironmentConfigs",
                columns: new[] { "ResourceId", "EnvironmentName" },
                unique: true);
        }
    }
}
