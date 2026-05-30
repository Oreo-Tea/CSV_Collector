PLC Data Collector (WinForms)

概要
- 複数 PLC と接続し、PLC から送信される CSV を受信して保存する簡易サンプル。
- 設定画面で PLC の IP、受信ポート、保存間隔、保存保持年数、保存先フォルダを設定可能。

ビルドと実行
1. Windows 環境で .NET SDK をインストールしてください。
2. プロジェクトフォルダでビルド:

```bash
cd PLCDataCollector
dotnet build /p:EnableWindowsTargeting=true
```

3. Windows Desktop アプリとして実行:

```bash
dotnet run /p:EnableWindowsTargeting=true
```

4. もし macOS/Windows 共有環境でビルドのみ行う場合は、`EnableWindowsTargeting=true` を指定してください。

5. 実行時に `Microsoft.WindowsDesktop.App` が見つからない場合は、Windows 上で以下をインストールしてください:
   - .NET 10 Desktop Runtime
   - または Windows 用 .NET 10 SDK

```powershell
winget install Microsoft.DotNet.DesktopRuntime.10
```

```bash
# Windows 環境例
cd PLCDataCollector
dotnet run /p:EnableWindowsTargeting=true
```

使い方
- 設定ボタンで PLC IP リスト、受信ポート、保存間隔（秒）、保持年数、保存先フォルダを設定します。
- Start を押すと TCP ポートで CSV を受け付け、受信した CSV を設定間隔ごとにフォルダ（yyyy-MM-dd）に保存します。

注意
- これは骨格実装です。実運用では例外処理、ログ、セキュリティ、Modbus の双方向通信（必要なら）を追実装してください。
