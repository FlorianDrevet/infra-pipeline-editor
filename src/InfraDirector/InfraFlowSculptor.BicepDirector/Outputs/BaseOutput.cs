using System.Text;
using InfraFlowSculptor.BicepDirector.Interfaces;

namespace InfraFlowSculptor.BicepDirector.Outputs;

public abstract class BaseOutput: IBicep
{
    public static bool IsSecret { get; set; }
    public static string Name { get;} = String.Empty;

    public abstract StringBuilder ToBicep();
}