using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace skpractice.Plugins.OrchestratorPlugin;

public class Orchestrator
{
    private readonly IKernel _kernel;

    public Orchestrator(IKernel kernel)
    {
        _kernel = kernel;
    }

    [SKFunction]
    public async Task<string> RouteRequestAsync(SKContext context)
    {
        var request = context.Variables["input"];

        var getIntent = _kernel.Skills.GetFunction("OrchestratorPlugin", "GetIntent");
        var getIntentVariables = new ContextVariables
        {
            ["input"] = context.Variables["input"],
            ["options"] = "Sqrt, Multiply",
        };
        var intent = (await _kernel.RunAsync(getIntentVariables, getIntent)).Result.Trim();

        var getNumbers = _kernel.Skills.GetFunction("OrchestratorPlugin", "GetNumbers");
        var numbersJson = (await _kernel.RunAsync(context.Variables, getNumbers)).Result;
        var numbers = JsonSerializer.Deserialize<NumberJson>(numbersJson);

        return intent switch
        {
            "Sqrt" => await ExecuteMathFunctionAsync("MathPlugin", "Sqrt",
                numbers!.Number1!.ToString() ?? string.Empty),
            "Multiply" => await ExecuteMultiplyFunctionAsync(numbers!),
            _ => "연산 불가",
        };
    }

    private async Task<string> ExecuteMathFunctionAsync(string pluginName, string functionName, string input)
    {
        var function = _kernel.Skills.GetFunction(pluginName, functionName);
        return (await _kernel.RunAsync(input, function)).Result;
    }

    private async Task<string> ExecuteMultiplyFunctionAsync(NumberJson numbers)
    {
        var multiply = _kernel.Skills.GetFunction("MathPlugin", "Multiply");
        var multiplyVariables = new ContextVariables
        {
            ["number1"] = numbers!.Number1!.ToString() ?? string.Empty,
            ["number2"] = numbers!.Number2!.ToString() ?? string.Empty
        };
        return (await _kernel.RunAsync(multiplyVariables, multiply)).Result;
    }

    private sealed record NumberJson
    {
        [JsonPropertyName("number1")]
        public double? Number1 { get; init; }

        [JsonPropertyName("number2")]
        public double? Number2 { get; init; }
    }
}