# MCP Chat Console App

## 專案結構

### McpClientApp
- **appsettings.json**: 儲存應用程式設定，例如 OpenAI API 金鑰與模型 ID。
- **McpClientApp.csproj**: 專案檔案，定義此應用程式的依賴與目標框架。
- **Program.cs**: 主程式入口，包含 Semantic Kernel 與 MCP Client 的初始化邏輯。
- **bin/Debug/net8.0/**: 編譯輸出的目錄，包含應用程式的 DLL、執行檔與依賴。
- **obj/**: 編譯過程中的中間檔案目錄。

### McpServer
- **McpServer.csproj**: MCP Server 的專案檔案。
- **Program.cs**: MCP Server 的主程式入口。
- **bin/Debug/net8.0/**: MCP Server 的編譯輸出目錄。
- **obj/**: MCP Server 的中間檔案目錄。

## 使用方式

1. 確保已安裝 .NET 8.0 SDK。
2. 在 `McpClientApp` 資料夾中執行以下命令以還原依賴：
   ```shell
   dotnet restore
   ```
3. 執行 MCP Server：
   ```shell
   dotnet run --project ../McpServer/McpServer.csproj
   ```
4. 執行 MCP Client：
   ```shell
   dotnet run --project McpClientApp.csproj
   ```

## 依賴套件

- **Microsoft.SemanticKernel**: 用於整合 OpenAI 與 MCP 工具。
- **Microsoft.Extensions.Configuration.Json**: 用於載入 `appsettings.json`。
- **Microsoft.Bcl.AsyncInterfaces**: 提供非同步介面支援。

## 注意事項

- 確保 `appsettings.json` 包含正確的 OpenAI API 金鑰與模型 ID。
- MCP Server 的執行檔案路徑需正確配置。