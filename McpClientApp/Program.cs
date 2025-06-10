using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

class Program
{
    static async Task Main(string[] args)
    {
        // 讀取設定檔
        // 注意：這裡假設 appsettings.json 檔案在執行目錄下
        var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

        // 準備 Semantic Kernel
        var builder = Kernel.CreateBuilder();
        // builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

        // 設定 OpenAI 的 API 金鑰和模型 ID
        builder.Services.AddOpenAIChatCompletion(
            modelId: config["OpenAI:ModelId"] ?? "gpt-4o",
            apiKey: config["OpenAI:API_KEY"] ?? ""
        );
        var kernel = builder.Build();

        // 建立 MCP client
        // 注意：這裡假設 MCP Server 可在本地端運行，並且可以透過 StdioClientTransport 連接
        await using IMcpClient mcpClient = await McpClientFactory.CreateAsync(
            new StdioClientTransport(
                // 注意：這裡假設 MCP Server 的執行檔在 "../McpServer/McpServer.csproj" 路徑下
                new()
                {
                    Name = "LocalMcpServer",
                    Command = "dotnet",
                    Arguments = ["run", "--project", "../McpServer/McpServer.csproj"],
                }
            )
        );

        // 取得 tools 並註冊到 Semantic Kernel
        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
        // 列出 MCP工具名稱和描述

        Console.WriteLine("\n\nAvailable MCP Tools:");
        foreach (var tool in tools)
        {
            Console.WriteLine($"{tool.Name}: {tool.Description}");
        }
        // 將 MCP 工具轉換為 Semantic Kernel 函數
        // 並加入到 Kernel 中
        kernel.Plugins.AddFromFunctions("McpTools", tools.Select(t => t.AsKernelFunction()));

        // Create chat history 物件，並且加入系統訊息
        var history = new ChatHistory();
        history.AddSystemMessage("你是一位 MCP 工具助理，會根據使用者輸入決定是否要使用 tool 來回答問題。");

        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // 開始對談
        Console.Write("User > ");
        string? userInput;
        while (!string.IsNullOrEmpty(userInput = Console.ReadLine()))
        {
            // Add user input
            history.AddUserMessage(userInput);

            // Enable auto function calling
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            // Print the results
            Console.WriteLine("Assistant > " + result);

            // Add the message from the agent to the chat history
            history.AddMessage(result.Role, result.Content ?? string.Empty);

            // Get user input again
            Console.Write("User > ");
        }
    }

    // // 建立 Agent
    // ChatCompletionAgent agent = new()
    // {
    //     Instructions = "你是一位 MCP 工具助理，會根據使用者輸入決定是否要使用 tool 來回答問題。",
    //     Name = "McpAgent",
    //     Kernel = kernel,
    //     Arguments = new KernelArguments(new PromptExecutionSettings
    //     {
    //         FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
    //     })
    // };

    // // 互動對話（多輪 + 上下文）
    // Console.WriteLine("💬 MCP Chat Agent 啟動，輸入 exit 離開。");
    // while (true)
    // {
    //     Console.Write("你：");
    //     var input = Console.ReadLine();
    //     if (string.IsNullOrWhiteSpace(input) || input.Trim().ToLower() == "exit") break;

    //     var response = await agent.InvokeAsync(input).FirstAsync();
    //     Console.WriteLine($"🤖 回應：{response.Content}\n");
    // }
}