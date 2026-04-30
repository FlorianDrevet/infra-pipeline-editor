using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecrets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No schema changes: this migration is intentionally a no-op recorded as a checkpoint
            // when the secrets feature switched to using existing parameter-definition tables.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op rollback: matches the empty Up so reverting requires no schema changes.
        }
    }
}
