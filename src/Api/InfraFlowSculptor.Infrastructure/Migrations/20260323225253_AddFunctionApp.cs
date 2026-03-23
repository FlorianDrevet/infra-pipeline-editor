using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFunctionApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FunctionApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppServicePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuntimeStack = table.Column<string>(type: "text", nullable: false),
                    RuntimeVersion = table.Column<string>(type: "text", nullable: false),
                    HttpsOnly = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunctionApps_AzureResource_Id",
                        column: x => x.Id,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FunctionAppEnvironmentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FunctionAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnvironmentName = table.Column<string>(type: "text", nullable: false),
                    HttpsOnly = table.Column<bool>(type: "boolean", nullable: true),
                    RuntimeStack = table.Column<string>(type: "text", nullable: true),
                    RuntimeVersion = table.Column<string>(type: "text", nullable: true),
                    MaxInstanceCount = table.Column<int>(type: "integer", nullable: true),
                    FunctionsWorkerRuntime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionAppEnvironmentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunctionAppEnvironmentSettings_FunctionApps_FunctionAppId",
                        column: x => x.FunctionAppId,
                        principalTable: "FunctionApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FunctionAppEnvironmentSettings_FunctionAppId",
                table: "FunctionAppEnvironmentSettings",
                column: "FunctionAppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunctionAppEnvironmentSettings");

            migrationBuilder.DropTable(
                name: "FunctionApps");
        }
    }
}
