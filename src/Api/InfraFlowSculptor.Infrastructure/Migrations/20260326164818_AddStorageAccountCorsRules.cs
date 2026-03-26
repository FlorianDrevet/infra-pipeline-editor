using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageAccountCorsRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorageAccountCorsRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowedOrigins = table.Column<List<string>>(type: "text[]", nullable: false),
                    AllowedMethods = table.Column<List<string>>(type: "text[]", nullable: false),
                    AllowedHeaders = table.Column<List<string>>(type: "text[]", nullable: false),
                    ExposedHeaders = table.Column<List<string>>(type: "text[]", nullable: false),
                    MaxAgeInSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageAccountCorsRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageAccountCorsRules_StorageAccounts_StorageAccountId",
                        column: x => x.StorageAccountId,
                        principalTable: "StorageAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorageAccountCorsRules_StorageAccountId",
                table: "StorageAccountCorsRules",
                column: "StorageAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorageAccountCorsRules");
        }
    }
}
