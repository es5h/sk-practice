using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Skills.Core;
using skpractice.Plugins.OrchestratorPlugin;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .AddDebug();
});

var configuration = new ConfigurationBuilder()
    .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(path: "appsettings.development.json", optional: true, reloadOnChange: true)
    .Build();

var aiOptions = configuration.GetRequiredSection("AIService").Get<AiServiceOptions>()
                ?? throw new InvalidOperationException($"Missing configuration for AIService.");

var kernel = Kernel.Builder
    .WithLoggerFactory(loggerFactory)
    .WithOpenAIChatCompletionService(
        modelId: "gpt-3.5-turbo",
        apiKey: aiOptions.Key
    )
    .Build();

var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");

var mathPlugin = kernel.ImportSkill(new skpractice.Plugins.MathPlugin.Math(), "MathPlugin");
var orchestratorPlugin = kernel.ImportSkill(new Orchestrator(kernel), "OrchestratorPlugin");
var conversationSummaryPlugin = kernel.ImportSkill(new ConversationSummarySkill(kernel), "ConversationSummarySkill");

var planner = new SequentialPlanner(kernel);

// Create a plan for the ask
var ask = "If my investment of 2130.23 dollars increased by 23%, how much would I have after I spent $5 on a latte?";
var plan = await planner.CreatePlanAsync(ask);
Console.WriteLine(plan.ToJson(true));

// Execute the plan
var result = await plan.InvokeAsync();

Console.WriteLine("Plan results:");
Console.WriteLine(result.Result);

