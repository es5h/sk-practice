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

const string prompt = """
                      Bot: 어떻게 도와드릴까요?
                      User: {{$input}}

                      ---------------------------------------

                      5글자 이내로 표현한 사용자의 의도:
                      """;

var promptConfig = new PromptTemplateConfig
{
    Schema = 1,
    Type = "Completion",
    Description = "User의 의도를 가져온다.",
    Completion =
    {
        MaxTokens = 500,
        Temperature = 0.0,
        TopP = 0.0,
        PresencePenalty = 0.0,
        FrequencyPenalty = 0.0,
    },
    Input =
    {
        Parameters = new List<InputParameter>
        {
            new()
            {
                Name = "input",
                Description = "User의 요구",
                DefaultValue = "",
            }
        }
    },
};

var promptTemplate = new PromptTemplate(
    prompt,
    promptConfig,
    kernel
);
var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
var getIntentFunction = kernel.RegisterSemanticFunction("OrchestratorPlugin", "GetIntent", functionConfig);

var result = await kernel.RunAsync(
    "es5h github의 sk-practice 에 pr을 날린다.",
    getIntentFunction
);

Console.WriteLine(result);