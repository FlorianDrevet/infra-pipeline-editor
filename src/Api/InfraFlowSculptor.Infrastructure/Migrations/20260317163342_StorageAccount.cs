using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StorageAccount : Migration
    {
        /// <inheritdoc />
        /// <remarks>
        /// This migration is intentionally empty. The tables (StorageAccounts, BlobContainers,
        /// StorageQueues, StorageTables, RoleAssignments) were already created by earlier migrations
        /// (20260315000000_AddRoleAssignmentsTable, 20260315100910_AddStorageAccountTable).
        /// This migration solely advances the EF Core model snapshot to a consistent state
        /// including those entities.
        /// </remarks>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
