using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraFlowSculptor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationStackProfileToPipelineOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_AngularProfilePackageManager",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_AngularProfileProjectName",
                table: "WebApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgBuildProduction",
                table: "WebApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgLint",
                table: "WebApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgTest",
                table: "WebApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileBuildCommand",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileLintCommand",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileTestCommand",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_DotNetProfileCollectCoverage",
                table: "WebApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DotNetProfileCustomTestProjectGlob",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DotNetProfileTestFramework",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_JavaProfileBuildTool",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_JavaProfileCollectCoverage",
                table: "WebApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_JavaProfileTestFramework",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileLintScriptName",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfilePackageManager",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_NodeJsProfileRunLintScript",
                table: "WebApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileTestFramework",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileTestScriptName",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_ProfileStack",
                table: "WebApps",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PythonProfileCollectCoverage",
                table: "WebApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_PythonProfilePackageManager",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_PythonProfileTestFramework",
                table: "WebApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_Stack",
                table: "WebApps",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_StaticSiteProfileBuildCommand",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_StaticSiteProfileOutputDirectory",
                table: "WebApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_AngularProfilePackageManager",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_AngularProfileProjectName",
                table: "FunctionApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgBuildProduction",
                table: "FunctionApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgLint",
                table: "FunctionApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgTest",
                table: "FunctionApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileBuildCommand",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileLintCommand",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileTestCommand",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_DotNetProfileCollectCoverage",
                table: "FunctionApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DotNetProfileCustomTestProjectGlob",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DotNetProfileTestFramework",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_JavaProfileBuildTool",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_JavaProfileCollectCoverage",
                table: "FunctionApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_JavaProfileTestFramework",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileLintScriptName",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfilePackageManager",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_NodeJsProfileRunLintScript",
                table: "FunctionApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileTestFramework",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileTestScriptName",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_ProfileStack",
                table: "FunctionApps",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PythonProfileCollectCoverage",
                table: "FunctionApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_PythonProfilePackageManager",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_PythonProfileTestFramework",
                table: "FunctionApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_Stack",
                table: "FunctionApps",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_StaticSiteProfileBuildCommand",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_StaticSiteProfileOutputDirectory",
                table: "FunctionApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_AngularProfilePackageManager",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_AngularProfileProjectName",
                table: "ContainerApps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgBuildProduction",
                table: "ContainerApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgLint",
                table: "ContainerApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_AngularProfileRunNgTest",
                table: "ContainerApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileBuildCommand",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileLintCommand",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_CustomProfileTestCommand",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_DotNetProfileCollectCoverage",
                table: "ContainerApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DotNetProfileCustomTestProjectGlob",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_DotNetProfileTestFramework",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_JavaProfileBuildTool",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_JavaProfileCollectCoverage",
                table: "ContainerApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_JavaProfileTestFramework",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileLintScriptName",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfilePackageManager",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_NodeJsProfileRunLintScript",
                table: "ContainerApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileTestFramework",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_NodeJsProfileTestScriptName",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_ProfileStack",
                table: "ContainerApps",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PipelineStepOptions_PythonProfileCollectCoverage",
                table: "ContainerApps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_PythonProfilePackageManager",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_PythonProfileTestFramework",
                table: "ContainerApps",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_Stack",
                table: "ContainerApps",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_StaticSiteProfileBuildCommand",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineStepOptions_StaticSiteProfileOutputDirectory",
                table: "ContainerApps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfilePackageManager",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileProjectName",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgBuildProduction",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgLint",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgTest",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileBuildCommand",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileLintCommand",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileTestCommand",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileCollectCoverage",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileCustomTestProjectGlob",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileTestFramework",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileBuildTool",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileCollectCoverage",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileTestFramework",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileLintScriptName",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfilePackageManager",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileRunLintScript",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileTestFramework",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileTestScriptName",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_ProfileStack",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfileCollectCoverage",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfilePackageManager",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfileTestFramework",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_Stack",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_StaticSiteProfileBuildCommand",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_StaticSiteProfileOutputDirectory",
                table: "WebApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfilePackageManager",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileProjectName",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgBuildProduction",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgLint",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgTest",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileBuildCommand",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileLintCommand",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileTestCommand",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileCollectCoverage",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileCustomTestProjectGlob",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileTestFramework",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileBuildTool",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileCollectCoverage",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileTestFramework",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileLintScriptName",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfilePackageManager",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileRunLintScript",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileTestFramework",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileTestScriptName",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_ProfileStack",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfileCollectCoverage",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfilePackageManager",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfileTestFramework",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_Stack",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_StaticSiteProfileBuildCommand",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_StaticSiteProfileOutputDirectory",
                table: "FunctionApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfilePackageManager",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileProjectName",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgBuildProduction",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgLint",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_AngularProfileRunNgTest",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileBuildCommand",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileLintCommand",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_CustomProfileTestCommand",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileCollectCoverage",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileCustomTestProjectGlob",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_DotNetProfileTestFramework",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileBuildTool",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileCollectCoverage",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_JavaProfileTestFramework",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileLintScriptName",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfilePackageManager",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileRunLintScript",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileTestFramework",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_NodeJsProfileTestScriptName",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_ProfileStack",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfileCollectCoverage",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfilePackageManager",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_PythonProfileTestFramework",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_Stack",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_StaticSiteProfileBuildCommand",
                table: "ContainerApps");

            migrationBuilder.DropColumn(
                name: "PipelineStepOptions_StaticSiteProfileOutputDirectory",
                table: "ContainerApps");
        }
    }
}
