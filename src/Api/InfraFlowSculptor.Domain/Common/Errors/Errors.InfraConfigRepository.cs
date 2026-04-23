using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="InfrastructureConfigAggregate.Entities.InfraConfigRepository"/> entity.</summary>
    public static class InfraConfigRepository
    {
        private const string DuplicateAliasCode = "InfraConfigRepository.DuplicateAlias";
        private const string NotFoundCode = "InfraConfigRepository.NotFound";
        private const string LayoutModeRequiredCode = "InfraConfigRepository.LayoutModeRequired";
        private const string ProjectNotMultiRepoCode = "InfraConfigRepository.ProjectNotMultiRepo";
        private const string AllInOneRequiresOneRepoCode = "InfraConfigRepository.AllInOneRequiresOneRepository";
        private const string SplitInfraCodeRequiresInfraAndAppCode = "InfraConfigRepository.SplitInfraCodeRequiresInfraAndApp";

        /// <summary>Returns a conflict error when the same alias is reused inside an InfraConfig.</summary>
        public static Error DuplicateAlias(string alias) =>
            Error.Conflict(code: DuplicateAliasCode, description: $"A repository with alias '{alias}' already exists in this configuration.");

        /// <summary>Returns a not-found error for the given repository identifier.</summary>
        public static Error NotFound(InfraConfigRepositoryId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No infra-config repository with id '{id}' was found.");

        /// <summary>Returned when the parent project layout is not MultiRepo and per-config repositories are not allowed.</summary>
        public static Error ProjectNotMultiRepo() =>
            Error.Conflict(code: ProjectNotMultiRepoCode, description: "Per-configuration repositories can only be declared when the parent project layout is MultiRepo.");

        /// <summary>Returned when no <see cref="ConfigLayoutMode"/> is set on the configuration but a repository operation is attempted.</summary>
        public static Error LayoutModeRequired() =>
            Error.Conflict(code: LayoutModeRequiredCode, description: "The configuration must declare a layout mode (AllInOne or SplitInfraCode) before adding repositories.");

        /// <summary>Returned when AllInOne config-level layout requires exactly one repository hosting both kinds.</summary>
        public static Error AllInOneRequiresOneRepository() =>
            Error.Conflict(code: AllInOneRequiresOneRepoCode, description: "AllInOne configuration layout requires exactly one repository declaring both Infrastructure and ApplicationCode content kinds.");

        /// <summary>Returned when SplitInfraCode config-level layout requires exactly two repositories (one infra, one app).</summary>
        public static Error SplitInfraCodeRequiresInfraAndApp() =>
            Error.Conflict(code: SplitInfraCodeRequiresInfraAndAppCode, description: "SplitInfraCode configuration layout requires exactly two repositories: one with Infrastructure only, one with ApplicationCode only.");
    }
}
