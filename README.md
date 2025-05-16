# AI Chat Frontend Demo
This is a Blazor WebAssembly standalone app, that connected to AI Chat Backend app that serves as simple generative AI chat client. The real-time chat functionalities is powered by SignalR with fast binary MsgPack serialiation protocol

> Go to the [backend app](https://github.com/ahmadnazif/AiChatBackendDemo)

## Running the app
- .NET 8 SDK must be installed to debug the code. 
- Pull the source code, then open it using Visual Studio 2022.
- Navigate to inside the project folder.
- Navigate inside `wwwroot` folder and examine the `appsettings.json` file if you need to change the backend API base URL. Please make sure the backend API app has already running. 
- Run this app: `dotnet run`.
- A random port will be assigned, navigate it in your browser

## App technology & dependency
- Blazor WebAssembly Standalone (.NET 8)
- SignalR client with MsgPack serialization
- Since this app is using SignalR, the underlying WebSocket & SSE (server-sent event) is used to power the real-time interactions.

## Misc
- Backend API server (AiChatBackend) must be running in order this app to work.
- Available demo:
  - Streaming text chat.
  - Streaming text chat with attachment
  - RAG: Text similarity analysis with in-memory vector database
  - RAG: Recipe analysis with Qdrant database (upcoming)
