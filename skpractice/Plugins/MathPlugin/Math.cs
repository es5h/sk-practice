using System.ComponentModel;
using System.Globalization;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace skpractice.Plugins.MathPlugin;

public class Math
{
    [SKFunction, Description("Take the square root of a number")]
    public string Sqrt(string number) =>
        System.Math.Sqrt(Convert.ToDouble(number, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture);

    [SKFunction, Description("Multiply two numbers")]
    [SKParameter("number1", "first number to multiply")]
    [SKParameter("number2", "second number to multiply")]
    public string Multiply(SKContext context) =>
        (Convert.ToDouble(context.Variables["number1"], CultureInfo.InvariantCulture)
         *
         Convert.ToDouble(context.Variables["number2"], CultureInfo.InvariantCulture))
        .ToString(CultureInfo.InvariantCulture);
}