using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
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

// Make a request that runs the Sqrt function
var result1 = await kernel.RunAsync("What is the square root of 634?", orchestratorPlugin["RouteRequest"]);
Console.WriteLine(result1);

// Make a request that runs the Multiply function
var result2 = await kernel.RunAsync("What is 12.34 times 56.78?", orchestratorPlugin["RouteRequest"]);
Console.WriteLine(result2);