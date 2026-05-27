using FluentAssertions;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Common.Helpers;

public sealed class AppSettingPipelineParameterNameHelperTests
{
    [Fact]
    public void Given_KeyVaultSettingViaVariableGroupAndBicepparam_When_ResolvingParameterName_Then_UsesCanonicalResourceIdentifier()
    {
        // Arrange
        var appSetting = new AppSettingReadModel(
            Guid.NewGuid(),
            "web-api",
            "WebApp",
            "JwtSecret",
            EnvironmentValues: null,
            SourceResourceId: null,
            SourceResourceName: null,
            SourceResourceType: null,
            SourceOutputName: null,
            IsOutputReference: false,
            KeyVaultResourceId: Guid.NewGuid(),
            KeyVaultResourceName: "shared-kv",
            SecretName: "JWT_SECRET",
            IsKeyVaultReference: true,
            SecretValueAssignment: nameof(SecretValueAssignment.ViaBicepparam),
            VariableGroupId: Guid.NewGuid(),
            PipelineVariableName: "jwt-secret",
            VariableGroupName: "shared-secrets",
            IsViaVariableGroup: true);

        // Act
        var result = AppSettingPipelineParameterNameHelper.ResolveBicepParameterName(appSetting);

        // Assert
        result.Should().Be("webApiJwtSecretSecretValue");
    }

    [Fact]
    public void Given_NonSecureAppSetting_When_ResolvingParameterName_Then_ReturnsOriginalSettingName()
    {
        // Arrange
        var appSetting = new AppSettingReadModel(
            Guid.NewGuid(),
            "web-api",
            "WebApp",
            "PlainSetting",
            EnvironmentValues: null,
            SourceResourceId: null,
            SourceResourceName: null,
            SourceResourceType: null,
            SourceOutputName: null,
            IsOutputReference: false,
            KeyVaultResourceId: null,
            KeyVaultResourceName: null,
            SecretName: null,
            IsKeyVaultReference: false);

        // Act
        var result = AppSettingPipelineParameterNameHelper.ResolveBicepParameterName(appSetting);

        // Assert
        result.Should().Be("PlainSetting");
    }
}
