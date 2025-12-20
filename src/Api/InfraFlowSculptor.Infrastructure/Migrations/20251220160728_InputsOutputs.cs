using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InputsOutputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResourceLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputType = table.Column<int>(type: "integer", nullable: false),
                    InputType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceLinks_AzureResource_SourceResourceId",
                        column: x => x.SourceResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceLinks_AzureResource_TargetResourceId",
                        column: x => x.TargetResourceId,
                        principalTable: "AzureResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceLinks_SourceResourceId_TargetResourceId_OutputType_~",
                table: "ResourceLinks",
                columns: new[] { "SourceResourceId", "TargetResourceId", "OutputType", "InputType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceLinks_TargetResourceId",
                table: "ResourceLinks",
                column: "TargetResourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceLinks");
        }
    }
}
