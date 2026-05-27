using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Commands.ApplyImportPreview;
using InfraFlowSculptor.Application.Imports.Common.Analysis;

namespace InfraFlowSculptor.Application.Tests.Imports.Commands.ApplyImportPreview;

public sealed class ApplyImportPreviewCommandValidatorTests
{
    private readonly ApplyImportPreviewCommandValidator _sut = new();

    private static ImportPreviewAnalysisResult ValidPreview() => new()
    {
        SourceFormat = "arm-json",
        Summary = "1 resource parsed"
    };

    private static ApplyImportPreviewCommand ValidCommand() => new(
        ProjectName: "MyProject",
        LayoutPreset: "AllInOne",
        Preview: ValidPreview());

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyProjectName_When_Validate_Then_FailsOnProjectName()
    {
        var command = ValidCommand() with { ProjectName = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ApplyImportPreviewCommand.ProjectName));
    }

    [Fact]
    public void Given_ProjectNameTooShort_When_Validate_Then_FailsOnProjectName()
    {
        var command = ValidCommand() with { ProjectName = "AB" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ApplyImportPreviewCommand.ProjectName));
    }

    [Fact]
    public void Given_ProjectNameTooLong_When_Validate_Then_FailsOnProjectName()
    {
        var command = ValidCommand() with { ProjectName = new string('A', 81) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ApplyImportPreviewCommand.ProjectName));
    }

    [Fact]
    public void Given_EmptyLayoutPreset_When_Validate_Then_FailsOnLayoutPreset()
    {
        var command = ValidCommand() with { LayoutPreset = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ApplyImportPreviewCommand.LayoutPreset));
    }

    [Fact]
    public void Given_NullPreview_When_Validate_Then_FailsOnPreview()
    {
        var command = ValidCommand() with { Preview = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ApplyImportPreviewCommand.Preview));
    }

    [Fact]
    public void Given_EmptySourceFormat_When_Validate_Then_FailsOnSourceFormat()
    {
        var preview = ValidPreview() with { SourceFormat = "" };
        var command = ValidCommand() with { Preview = preview };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Preview.SourceFormat");
    }

    [Fact]
    public void Given_EmptySummary_When_Validate_Then_FailsOnSummary()
    {
        var preview = ValidPreview() with { Summary = "" };
        var command = ValidCommand() with { Preview = preview };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Preview.Summary");
    }
}
