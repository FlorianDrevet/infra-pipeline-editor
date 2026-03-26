using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlobLifecycleRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlobLifecycleRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContainerNames = table.Column<string>(type: "jsonb", nullable: false),
                    TimeToLiveInDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlobLifecycleRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlobLifecycleRules_StorageAccounts_StorageAccountId",
                        column: x => x.StorageAccountId,
                        principalTable: "StorageAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlobLifecycleRules_StorageAccountId",
                table: "BlobLifecycleRules",
                column: "StorageAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlobLifecycleRules");
        }
    }
}
