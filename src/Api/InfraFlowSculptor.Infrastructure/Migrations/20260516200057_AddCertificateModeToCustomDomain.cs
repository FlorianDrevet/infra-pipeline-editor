using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateModeToCustomDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BindingType",
                table: "CustomDomains",
                newName: "CertificateMode");

            migrationBuilder.Sql(
                """
                UPDATE "CustomDomains"
                SET "CertificateMode" = CASE
                    WHEN "CertificateMode" = 'Disabled' THEN 'Disabled'
                    WHEN "CertificateMode" = 'SniEnabled' THEN 'ManagedCertificate'
                    WHEN "CertificateMode" = 'Auto' THEN 'ManagedCertificate'
                    ELSE 'ManagedCertificate'
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CertificateMode",
                table: "CustomDomains",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ManagedCertificate",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "SniEnabled");

            migrationBuilder.AddColumn<string>(
                name: "CertificateName",
                table: "CustomDomains",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyVaultUrl",
                table: "CustomDomains",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagedIdentityResourceId",
                table: "CustomDomains",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateName",
                table: "CustomDomains");

            migrationBuilder.DropColumn(
                name: "KeyVaultUrl",
                table: "CustomDomains");

            migrationBuilder.DropColumn(
                name: "ManagedIdentityResourceId",
                table: "CustomDomains");

            migrationBuilder.Sql(
                """
                UPDATE "CustomDomains"
                SET "CertificateMode" = CASE
                    WHEN "CertificateMode" = 'Disabled' THEN 'Disabled'
                    ELSE 'SniEnabled'
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CertificateMode",
                table: "CustomDomains",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "SniEnabled",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "ManagedCertificate");

            migrationBuilder.RenameColumn(
                name: "CertificateMode",
                table: "CustomDomains",
                newName: "BindingType");
        }
    }
}
