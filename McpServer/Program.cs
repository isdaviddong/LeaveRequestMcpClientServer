using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class LeaveRequestTool
{

    [McpServerTool, Description("取得請假天數")]
    public static int GetLeaveRecordAmount([Description("要查詢請假天數的員工名稱")] string employeeName)
    {
        // 模擬根據員工名稱返回請假天數
        // 在實際應用中，這可能會查詢資料庫
        if (employeeName.ToLower() == "david")
            return 5;
        else if (employeeName.ToLower() == "eric")
            return 8;
        else
            return 3;
    }


    [McpServerTool, Description("進行請假，回傳結果")]
    public static string LeaveRequest(
        [Description("請假起始日期")] string 請假起始日期,
        [Description("請假天數")] int 天數,
        [Description("請假事由")] string 請假事由,
        [Description("代理人")] string 代理人,
        [Description("請假者姓名")] string 請假者姓名)
    {
        // 模擬請假處理邏輯，除了事由之外，其他都是必填
        if (string.IsNullOrEmpty(請假起始日期) || string.IsNullOrEmpty(請假事由) || string.IsNullOrEmpty(代理人) || string.IsNullOrEmpty(請假者姓名))
        {
            return "請假失敗，請確認所有必填欄位已填寫。";
        }
        // 模擬請假成功的回應
        var result = $"{請假者姓名} 請假 {天數}天，從 {請假起始日期} 開始，事由為 {請假事由}，代理人 {代理人}";
        return result;
    }

    [McpServerTool, Description("取得今天日期")]
    public static string GetCurrentDate()
    {
        return DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
    }
}