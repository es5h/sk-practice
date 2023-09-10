using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using Newtonsoft.Json.Linq;

namespace skpractice.Plugins.OrchestratorPlugin;

public class Orchestrator
{
    private readonly IKernel _kernel;

    public Orchestrator(IKernel kernel)
    {
        _kernel = kernel;
    }

    [SKFunction, Description("Json 으로 받은 숫자를 뽑아냅니다.")]
    public SKContext ExtractNumbersFromJson(SKContext context)
    {
        var numbers = JObject.Parse(context.Variables["input"]);

        numbers.Properties().ToList().ForEach(p => context.Variables[p.Name] = p.Value?.ToString());

        return context;
    }

    [SKFunction, Description("요청을 받아서 어떤 스킬을 실행할지 결정합니다.")]
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

        var MathFunction = intent switch
        {
            "Sqrt" => _kernel.Skills.GetFunction("MathPlugin", "Sqrt"),
            "Multiply" => _kernel.Skills.GetFunction("MathPlugin", "Multiply"),
            _ => throw new InvalidOperationException($"Unknown intent: {intent}"),
        };
        
        var getNumbers = _kernel.Skills.GetFunction("OrchestratorPlugin", "GetNumbers");
        var extractNumbersFromJson = _kernel.Skills.GetFunction("OrchestratorPlugin", "ExtractNumbersFromJson");
        var createResponse = _kernel.Skills.GetFunction("OrchestratorPlugin", "CreateResponse");

        var pipelineContext = new ContextVariables(request)
        {
            ["original_request"] = request,
        };

        var output = await _kernel.RunAsync
        (
            pipelineContext,
            getNumbers,
            extractNumbersFromJson,
            MathFunction,
            createResponse
        );

        return output.Variables["input"];
    }
}