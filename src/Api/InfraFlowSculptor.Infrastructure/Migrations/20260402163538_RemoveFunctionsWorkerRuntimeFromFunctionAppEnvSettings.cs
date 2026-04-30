using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFunctionsWorkerRuntimeFromFunctionAppEnvSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No schema changes: FunctionsWorkerRuntime is now derived from RuntimeStack at the
            // application layer, the column was dropped in a prior migration.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op rollback: matches the empty Up so reverting requires no schema changes.
        }
    }
}
