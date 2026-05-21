using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class AppPipelinePhase6LicenseNotificationTests
{
    private const string LicenseCheckStepPath = ".azuredevops/steps/app-license-check.step.yml";
    private const string NotificationStepPath = ".azuredevops/steps/app-notification.step.yml";
    private const string CiCodePipelinePath = ".azuredevops/pipelines/app-ci-code.pipeline.yml";
    private const string CiCodeJobPath = ".azuredevops/jobs/app-ci-code.job.yml";
    private const string ReleaseContainerJobPath = ".azuredevops/jobs/app-release-container.job.yml";
    private const string ReleaseCodeJobPath = ".azuredevops/jobs/app-release-code.job.yml";
    private const string ReleaseContainerPipelinePath = ".azuredevops/pipelines/app-release-container.pipeline.yml";
    private const string ReleaseCodePipelinePath = ".azuredevops/pipelines/app-release-code.pipeline.yml";

    private readonly AppPipelineGenerationEngine _sut = new(new IAppPipelineGenerator[]
    {
        new WebAppCodePipelineGenerator(),
    });

    // ── Step template registration ──────────────────────────────────────────

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ContainsLicenseCheckStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files.Should().ContainKey(LicenseCheckStepPath);
        files[LicenseCheckStepPath].Should()
            .Contain("licenseCheckTool")
            .And.Contain("License compliance");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ContainsNotificationStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files.Should().ContainKey(NotificationStepPath);
        files[NotificationStepPath].Should()
            .Contain("notificationWebhookUrl")
            .And.Contain("Send notification");
    }

    // ── CI code pipeline parameter forwarding ───────────────────────────────

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_CiCodePipelineDeclaresPhase6Parameters()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[CiCodePipelinePath].Should()
            .Contain("- name: runLicenseCheck")
            .And.Contain("- name: licenseCheckTool")
            .And.Contain("- name: enableNotifications")
            .And.Contain("- name: notificationWebhookUrl")
            .And.Contain("runLicenseCheck: ${{ parameters.runLicenseCheck }}")
            .And.Contain("enableNotifications: ${{ parameters.enableNotifications }}");
    }

    // ── CI code job parameter declarations and step references ───────────────

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_CiCodeJobDeclaresPhase6Parameters()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[CiCodeJobPath].Should()
            .Contain("- name: runLicenseCheck")
            .And.Contain("- name: licenseCheckTool")
            .And.Contain("- name: enableNotifications")
            .And.Contain("- name: notificationWebhookUrl");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_CiCodeJobReferencesPhase6Steps()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[CiCodeJobPath].Should()
            .Contain("${{ if parameters.runLicenseCheck }}")
            .And.Contain("../steps/app-license-check.step.yml")
            .And.Contain("${{ if parameters.enableNotifications }}")
            .And.Contain("../steps/app-notification.step.yml");
    }

    // ── Release job notification integration ────────────────────────────────

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ReleaseContainerJobDeclaresNotificationParams()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[ReleaseContainerJobPath].Should()
            .Contain("- name: enableNotifications")
            .And.Contain("- name: notificationWebhookUrl")
            .And.Contain("${{ if parameters.enableNotifications }}")
            .And.Contain("../steps/app-notification.step.yml");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ReleaseCodeJobDeclaresNotificationParams()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[ReleaseCodeJobPath].Should()
            .Contain("- name: enableNotifications")
            .And.Contain("- name: notificationWebhookUrl")
            .And.Contain("${{ if parameters.enableNotifications }}")
            .And.Contain("../steps/app-notification.step.yml");
    }

    // ── Release pipeline notification forwarding ────────────────────────────

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ReleasePipelinesForwardNotificationParams()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[ReleaseContainerPipelinePath].Should()
            .Contain("- name: enableNotifications")
            .And.Contain("enableNotifications: ${{ parameters.enableNotifications }}");

        files[ReleaseCodePipelinePath].Should()
            .Contain("- name: enableNotifications")
            .And.Contain("enableNotifications: ${{ parameters.enableNotifications }}");
    }

    // ── CI wrapper forwarding ───────────────────────────────────────────────

    [Fact]
    public void Given_WebAppCodeWithLicenseCheck_When_Generate_Then_CiWrapperForwardsLicenseParams()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.RunLicenseCheck = true;
        request.LicenseCheckTool = "license-checker";

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .Contain("runLicenseCheck: true")
            .And.Contain("licenseCheckTool: 'license-checker'");
    }

    [Fact]
    public void Given_WebAppCodeWithNotifications_When_Generate_Then_CiWrapperForwardsNotificationParams()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.EnableNotifications = true;
        request.NotificationWebhookUrl = "https://hooks.slack.com/services/T00/B00/xxx";

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .Contain("enableNotifications: true")
            .And.Contain("notificationWebhookUrl: 'https://hooks.slack.com/services/T00/B00/xxx'");
    }

    [Fact]
    public void Given_WebAppCodeWithNotifications_When_Generate_Then_ReleaseWrapperForwardsNotificationParams()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.EnableNotifications = true;
        request.NotificationWebhookUrl = "https://hooks.slack.com/services/T00/B00/xxx";

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Release].Should()
            .Contain("enableNotifications: true")
            .And.Contain("notificationWebhookUrl: 'https://hooks.slack.com/services/T00/B00/xxx'");
    }

    [Fact]
    public void Given_WebAppCodeWithoutPhase6_When_Generate_Then_OmitsLicenseAndNotificationParams()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .NotContain("runLicenseCheck: true")
            .And.NotContain("enableNotifications: true");
    }
}
