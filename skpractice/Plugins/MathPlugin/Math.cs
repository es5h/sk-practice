using System.ComponentModel;
using System.Globalization;
using Microsoft.SemanticKernel.SkillDefinition;

namespace skpractice.Plugins.MathPlugin;

public class Math
{
    [SKFunction, Description("Take the square root of a number")]
    public string Sqrt(string number) =>
        System.Math.Sqrt(Convert.ToDouble(number, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture);
}