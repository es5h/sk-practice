using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Skills.Core;

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
var conversationSummaryPlugin = kernel.ImportSkill(new ConversationSummarySkill(kernel), "ConversationSummarySkill");

var variables = new ContextVariables
{
    ["input"] = "Yes",
    ["history"] = """
                  Bot: 무엇을 도와드릴까요?
                  User: 오늘 날씨가 어때요?
                  Bot: 현재 위치가 어디세요?
                  User: 서울입니다.
                  Bot: 오늘 서울 날씨는 섭씨 70도입니다.
                  User: 고맙습니다.
                  Bot: 천만에요.
                  User: 오늘 캘린더에서 일정알려주세요
                  Bot: 오후 2시에 팀미팅 있습니다.
                  User: 우리 팀이 큰 성과를 달성해서 축하 이메일을 보내겠습니다..
                  Bot: "제가 대신 이메일을 작성해 드릴까요?",
                  """,
    ["options"] = "이메일 발송, 이메일 수신, 미팅 발송, 미팅 수신, 채팅 발송",
};

var result = await kernel.RunAsync(
    variables,
    orchestratorPlugin["GetIntent"]
);

Console.WriteLine(result);