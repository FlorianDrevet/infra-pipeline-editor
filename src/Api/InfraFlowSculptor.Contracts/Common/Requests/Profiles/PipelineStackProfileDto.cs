using System.Text.Json.Serialization;

namespace InfraFlowSculptor.Contracts.Common.Requests.Profiles;

/// <summary>
/// Base polymorphic DTO for stack-specific pipeline profile configuration.
/// The <c>kind</c> discriminator selects the concrete profile type.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(DotNetProfileDto), "DotNet")]
[JsonDerivedType(typeof(NodeJsProfileDto), "NodeJs")]
[JsonDerivedType(typeof(AngularProfileDto), "Angular")]
[JsonDerivedType(typeof(JavaProfileDto), "Java")]
[JsonDerivedType(typeof(PythonProfileDto), "Python")]
[JsonDerivedType(typeof(StaticSiteProfileDto), "StaticSite")]
[JsonDerivedType(typeof(CustomProfileDto), "Custom")]
public abstract class PipelineStackProfileDto;
