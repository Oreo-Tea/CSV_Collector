PLC Data Collector (WinForms)

概要
- 複数 PLC と接続し、PLC から送信される CSV を受信して保存する簡易サンプル。
- 設定画面で PLC の IP、受信ポート、保存間隔、保存保持年数、保存先フォルダを設定可能。

ビルドと実行
1. .NET 7 SDK をインストールしてください。
2. プロジェクトフォルダでビルド:

```bash
cd PLCDataCollector
dotnet build
dotnet run --project PLCDataCollector.csproj
```

使い方
- 設定ボタンで PLC IP リスト、受信ポート、保存間隔（秒）、保持年数、保存先フォルダを設定します。
- Start を押すと TCP ポートで CSV を受け付け、受信した CSV を設定間隔ごとにフォルダ（yyyy-MM-dd）に保存します。

注意
- これは骨格実装です。実運用では例外処理、ログ、セキュリティ、Modbus の双方向通信（必要なら）を追実装してください。
