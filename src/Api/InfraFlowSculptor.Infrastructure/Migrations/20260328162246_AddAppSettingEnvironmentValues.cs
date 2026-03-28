using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettingEnvironmentValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StaticValue",
                table: "AppSettings");

            migrationBuilder.CreateTable(
                name: "AppSettingEnvironmentValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppSettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettingEnvironmentValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSettingEnvironmentValues_AppSettings_AppSettingId",
                        column: x => x.AppSettingId,
                        principalTable: "AppSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettingEnvironmentValues_AppSettingId_EnvironmentName",
                table: "AppSettingEnvironmentValues",
                columns: new[] { "AppSettingId", "EnvironmentName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettingEnvironmentValues");

            migrationBuilder.AddColumn<string>(
                name: "StaticValue",
                table: "AppSettings",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }
    }
}
