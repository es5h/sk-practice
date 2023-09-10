using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using static Microsoft.SemanticKernel.SemanticFunctions.PromptTemplateConfig;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole()
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

var orchestratorPlugin = kernel
    .ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");

var result = await kernel.RunAsync(
    "es5h github의 sk-practice 에 pr을 날린다.",
    orchestratorPlugin["GetIntent"]
);

Console.WriteLine(result);