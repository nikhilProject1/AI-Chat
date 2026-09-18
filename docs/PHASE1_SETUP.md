# Phase 1 setup

## Prerequisites

- .NET 9 SDK
- Node.js 20 or newer
- SQL Server Express available as `localhost\SQLEXPRESS`
- An OpenRouter API key

If your SQL Server instance has another name, update `DefaultConnection` in
`server/appsettings.json`.

## One-time database setup

From the repository root:

```powershell
dotnet ef database update --project server/AiChat.Api.csproj --startup-project server/AiChat.Api.csproj
```

## Configure the OpenRouter key once

Store the key in .NET User Secrets. This persists across terminal and computer
restarts but stays outside the repository:

```powershell
dotnet user-secrets set "OpenRouter:ApiKey" "paste-your-key-here" --project server/AiChat.Api.csproj
```

Run that command once. Afterward, the backend loads the key automatically whenever
it runs in the Development environment. Do not add the key to `appsettings.json`
or commit it to Git.

To verify the setting without displaying the secret:

```powershell
dotnet user-secrets list --project server/AiChat.Api.csproj |
    Select-String -Quiet '^OpenRouter:ApiKey = .+'
```

The existing `OPENROUTER_API_KEY` environment variable remains supported as an
optional override.

## Run the backend

From the repository root:

```powershell
dotnet run --project server/AiChat.Api.csproj
```

The API runs at `http://localhost:5000`.

## Run the frontend

Open a second terminal:

```powershell
cd client
npm.cmd install
npm.cmd run dev
```

Open `http://localhost:5173`.

## Useful checks

```powershell
dotnet build AiChatPlatform.sln
cd client
npm.cmd run build
npm.cmd run lint
```
