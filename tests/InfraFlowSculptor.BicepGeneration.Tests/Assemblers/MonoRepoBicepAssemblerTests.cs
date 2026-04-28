using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Assemblers;

public sealed class MonoRepoBicepAssemblerTests
{
    [Fact]
    public void Given_ModuleAndSharedImports_When_Assemble_Then_RewritesPathsToCommonFolder()
    {
        // Arrange
        var perConfigResults = new Dictionary<string, GenerationResult>
        {
            ["dev"] = new GenerationResult
            {
                MainBicep = "import { EnvType } from 'types.bicep'\nimport { makeName } from 'functions.bicep'\nimport { RoleIds } from 'constants.bicep'\nmodule storage './modules/StorageAccount/storage.module.bicep' = {}\n",
                ModuleFiles = new Dictionary<string, string>
                {
                    ["modules/StorageAccount/storage.module.bicep"] = "module storage content",
                },
            },
        };

        // Act
        var result = MonoRepoBicepAssembler.Assemble(
            perConfigResults,
            new NamingContext(),
            Array.Empty<EnvironmentDefinition>(),
            hasAnyRoleAssignments: true,
            flattenShared: false);

        // Assert
        var rewrittenMain = result.ConfigFiles["dev"]["main.bicep"];
        rewrittenMain.Should().Contain("from '../Common/types.bicep'");
        rewrittenMain.Should().Contain("from '../Common/functions.bicep'");
        rewrittenMain.Should().Contain("from '../Common/constants.bicep'");
        rewrittenMain.Should().Contain("module storage '../Common/modules/StorageAccount/storage.module.bicep' = {}");
    }

    [Fact]
    public void Given_FlattenSharedEnabled_When_Assemble_Then_RewritesPathsToParentFolder()
    {
        // Arrange
        var perConfigResults = new Dictionary<string, GenerationResult>
        {
            ["dev"] = new GenerationResult
            {
                MainBicep = "module storage './modules/StorageAccount/storage.module.bicep' = {}\n",
                ModuleFiles = new Dictionary<string, string>
                {
                    ["modules/StorageAccount/storage.module.bicep"] = "module storage content",
                },
            },
        };

        // Act
        var result = MonoRepoBicepAssembler.Assemble(
            perConfigResults,
            new NamingContext(),
            Array.Empty<EnvironmentDefinition>(),
            hasAnyRoleAssignments: false,
            flattenShared: true);

        // Assert
        result.ConfigFiles["dev"]["main.bicep"]
            .Should().Contain("module storage '../modules/StorageAccount/storage.module.bicep' = {}");
    }
}