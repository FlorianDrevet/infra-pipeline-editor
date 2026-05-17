using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatScopesAndConcurrencyTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Projects",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "InfrastructureConfigs",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AzureResource",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "PersonalAccessTokenScopes",
                columns: table => new
                {
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PersonalAccessTokenId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalAccessTokenScopes", x => new { x.PersonalAccessTokenId, x.Scope });
                    table.ForeignKey(
                        name: "FK_PersonalAccessTokenScopes_PersonalAccessTokens_PersonalAcce~",
                        column: x => x.PersonalAccessTokenId,
                        principalTable: "PersonalAccessTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalAccessTokenScopes");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "InfrastructureConfigs");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "AzureResource");
        }
    }
}
