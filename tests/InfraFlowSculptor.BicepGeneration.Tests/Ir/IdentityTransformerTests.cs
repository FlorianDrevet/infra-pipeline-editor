using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Transformations;

namespace InfraFlowSculptor.BicepGeneration.Tests.Ir;

public sealed class IdentityTransformerTests
{
    private static BicepModuleSpec BaseSpec() => new()
    {
        ModuleName = "webApp",
        ModuleFolderName = "web-app",
        ResourceTypeName = "Web App",
        Resource = new BicepResourceDeclaration
        {
            Symbol = "app",
            ArmTypeWithApiVersion = "Microsoft.Web/sites@2023-12-01",
            Body =
            [
                new BicepPropertyAssignment("name", new BicepReference("name")),
                new BicepPropertyAssignment("location", new BicepReference("location")),
                new BicepPropertyAssignment("properties", BicepObjectExpression.Empty),
            ],
        },
    };

    [Fact]
    public void Given_Spec_When_WithSystemAssignedIdentity_Then_AddsIdentityBlock()
    {
        var spec = BaseSpec();

        var result = spec.WithSystemAssignedIdentity();

        result.Resource.Body.Should().Contain(p => p.Key == "identity");
        var identityProp = result.Resource.Body.First(p => p.Key == "identity");
        identityProp.Value.Should().BeOfType<BicepObjectExpression>();
    }

    [Fact]
    public void Given_Spec_When_WithSystemAssignedIdentity_Then_AddsPrincipalIdOutput()
    {
        var spec = BaseSpec();

        var result = spec.WithSystemAssignedIdentity();

        result.Outputs.Should().Contain(o => o.Name == "principalId");
    }

    [Fact]
    public void Given_Spec_When_WithUserAssignedIdentity_Then_AddsUaiParam()
    {
        var spec = BaseSpec();

        var result = spec.WithUserAssignedIdentity(alsoHasSystemAssigned: false);

        result.Parameters.Should().Contain(p => p.Name == "userAssignedIdentityId");
    }

    [Fact]
    public void Given_Spec_When_WithUserAssignedIdentityAndSystem_Then_IdentityTypeIsBoth()
    {
        var spec = BaseSpec().WithSystemAssignedIdentity();

        var result = spec.WithUserAssignedIdentity(alsoHasSystemAssigned: true);

        var identityProp = result.Resource.Body.First(p => p.Key == "identity");
        var identityObj = (BicepObjectExpression)identityProp.Value;
        var typeProp = identityObj.Properties.First(p => p.Key == "type");
        typeProp.Value.Should().BeOfType<BicepStringLiteral>()
            .Which.Value.Should().Be("SystemAssigned, UserAssigned");
    }

    [Fact]
    public void Given_Spec_When_WithParameterizedIdentity_Then_AddsImportAndParam()
    {
        var spec = BaseSpec();

        var result = spec.WithParameterizedIdentity(hasAnyUai: false);

        result.Imports.Should().Contain(i => i.Symbols != null && i.Symbols.Contains("ManagedIdentityType"));
        result.Parameters.Should().Contain(p => p.Name == "identityType");
    }

    [Fact]
    public void Given_Spec_When_WithParameterizedIdentityWithUai_Then_AddsUaiParam()
    {
        var spec = BaseSpec();

        var result = spec.WithParameterizedIdentity(hasAnyUai: true);

        result.Parameters.Should().Contain(p => p.Name == "userAssignedIdentityId");
    }

    [Fact]
    public void Given_SpecWithExistingIdentity_When_WithSystemAssigned_Then_DoesNotDuplicate()
    {
        var spec = BaseSpec().WithSystemAssignedIdentity();

        var result = spec.WithSystemAssignedIdentity();

        result.Resource.Body.Count(p => p.Key == "identity").Should().Be(1);
    }

    [Fact]
    public void Given_Spec_When_WithParameterizedIdentity_Then_AddsExportedType()
    {
        var spec = BaseSpec();

        var result = spec.WithParameterizedIdentity(hasAnyUai: false);

        result.ExportedTypes.Should().Contain(t => t.Name == "ManagedIdentityType");
    }

    [Fact]
    public void Given_Spec_When_WithParameterizedIdentity_Then_UsesExpectedPrincipalIdExpression()
    {
        var spec = BaseSpec();

        var result = spec.WithParameterizedIdentity(hasAnyUai: false);

        result.Outputs.Should().ContainSingle(output => output.Name == "principalId");
        result.Outputs.Single(output => output.Name == "principalId")
            .Expression.Should().BeOfType<BicepRawExpression>()
            .Which.RawBicep.Should().Be("contains(identityType, 'SystemAssigned') ? app.identity.principalId : ''");
    }
}
