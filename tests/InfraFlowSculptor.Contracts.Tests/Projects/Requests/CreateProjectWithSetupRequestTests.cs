using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class CreateProjectWithSetupRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateProjectWithSetupRequest
        {
            Name = "MyProject",
            LayoutPreset = "AllInOne",
            Environments =
            [
                new EnvironmentSetupRequest
                {
                    Name = "Development",
                    ShortName = "dev",
                    Location = "WestEurope",
                },
            ],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CreateProjectWithSetupRequest
        {
            Name = null!,
            LayoutPreset = "AllInOne",
            Environments =
            [
                new EnvironmentSetupRequest
                {
                    Name = "Development",
                    ShortName = "dev",
                    Location = "WestEurope",
                },
            ],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateProjectWithSetupRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NameTooShort_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CreateProjectWithSetupRequest
        {
            Name = "ab",
            LayoutPreset = "AllInOne",
            Environments =
            [
                new EnvironmentSetupRequest
                {
                    Name = "Development",
                    ShortName = "dev",
                    Location = "WestEurope",
                },
            ],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateProjectWithSetupRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NameTooLong_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CreateProjectWithSetupRequest
        {
            Name = new string('x', 81),
            LayoutPreset = "AllInOne",
            Environments =
            [
                new EnvironmentSetupRequest
                {
                    Name = "Development",
                    ShortName = "dev",
                    Location = "WestEurope",
                },
            ],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateProjectWithSetupRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_DescriptionTooLong_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CreateProjectWithSetupRequest
        {
            Name = "MyProject",
            Description = new string('x', 1001),
            LayoutPreset = "AllInOne",
            Environments =
            [
                new EnvironmentSetupRequest
                {
                    Name = "Development",
                    ShortName = "dev",
                    Location = "WestEurope",
                },
            ],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateProjectWithSetupRequest.Description)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLayoutPreset_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CreateProjectWithSetupRequest
        {
            Name = "MyProject",
            LayoutPreset = null!,
            Environments =
            [
                new EnvironmentSetupRequest
                {
                    Name = "Development",
                    ShortName = "dev",
                    Location = "WestEurope",
                },
            ],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateProjectWithSetupRequest.LayoutPreset)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullEnvironments_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CreateProjectWithSetupRequest
        {
            Name = "MyProject",
            LayoutPreset = "AllInOne",
            Environments = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateProjectWithSetupRequest.Environments)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyEnvironments_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CreateProjectWithSetupRequest
        {
            Name = "MyProject",
            LayoutPreset = "AllInOne",
            Environments = [],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateProjectWithSetupRequest.Environments)).Should().BeTrue();
    }
}
