# AI Chat Platform — Step-by-Step Learning Project

## 1. Project Goal

Build a ChatGPT-like AI application incrementally.

The project must start with a **small, working chat application** before adding advanced AI features. Each new phase should be implemented only after the previous phase is stable and understood.

The long-term progression is:

```text
Phase 1  Basic AI Chat
Phase 2  Conversation Persistence + Context Management
Phase 3  RAG / Knowledge Chat
Phase 4  Tool / Function Calling
Phase 5  AI Agent
Phase 6  Multi-step Agent Workflows
Phase 7  Observability, Evaluation, Guardrails
```

Do **not** implement later phases during the initial build.

---

# 2. Technology Stack

## Frontend

- React
- JavaScript or TypeScript
- React Router
- Axios or Fetch API
- Simple CSS / Material UI if needed

## Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- REST APIs
- Dependency Injection
- Clean separation between controllers, services, repositories, and AI integration

## Database

- SQL Server

Use SQL Server for:

- users if authentication is added later
- conversations
- messages
- AI request metadata
- future document metadata
- future agent/tool execution logs

## LLM Provider

Use **OpenRouter** instead of calling OpenAI directly.

Base API URL:

```text
https://openrouter.ai/api/v1
```

OpenRouter provides an OpenAI-compatible API, so the backend should be designed behind an abstraction such as `ILlmService` rather than coupling the application directly to one model/provider.

For the initial learning project, use:

```text
openrouter/free
```

This router automatically selects an available free model that satisfies the request capabilities.

The selected model must be configurable through application settings so that it can be changed later without code changes.

Example configuration:

```json
{
  "OpenRouter": {
    "BaseUrl": "https://openrouter.ai/api/v1",
    "Model": "openrouter/free"
  }
}
```

Never commit the OpenRouter API key to source control.

Use environment variables or .NET user secrets.

Example environment variable:

```text
OPENROUTER_API_KEY=your_key_here
```

---

# 3. Development Rules for Codex

When working on this repository, follow these rules.

1. Build one phase at a time.
2. Do not implement future phases unless explicitly requested.
3. Keep the application runnable after every development step.
4. Prefer simple, readable code over premature abstractions.
5. Keep AI-provider logic outside controllers.
6. Keep business logic outside prompts.
7. Never allow the LLM to access the database directly.
8. All database access must happen through backend code.
9. Keep API keys and secrets outside source control.
10. Add useful error handling and logging.
11. Do not introduce LangChain, LangGraph, Semantic Kernel, vector databases, or agents during Phase 1 unless explicitly requested.
12. Before making large architectural changes, explain why they are required.

The goal is not only to create the product but also to understand each AI concept while building it.

---

# 4. Target Architecture

The initial architecture should be:

```text
┌───────────────────────┐
│       React UI        │
└───────────┬───────────┘
            │ HTTP
            ▼
┌───────────────────────┐
│  ASP.NET Core Web API │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│      Chat Service     │
└───────┬────────┬──────┘
        │        │
        │        └──────────────┐
        ▼                       ▼
┌───────────────┐      ┌─────────────────┐
│ SQL Server    │      │ OpenRouter API  │
│ Conversations │      │ Free LLM        │
│ Messages      │      └─────────────────┘
└───────────────┘
```

Future versions will evolve into:

```text
User
 ↓
React
 ↓
.NET API
 ↓
Context Builder
 ├── Conversation History
 ├── RAG Retrieval
 └── Tool Results
 ↓
Agent Runtime
 ↓
LLM
 ↓
Tool / RAG / Response
 ↓
Observability
```

Do not build this future architecture immediately.

---

# 5. Suggested Repository Structure

```text
ai-chat-platform/
│
├── client/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── hooks/
│   │   └── models/
│   ├── package.json
│   └── README.md
│
├── server/
│   ├── Controllers/
│   ├── Services/
│   │   ├── Interfaces/
│   │   └── Implementations/
│   ├── Models/
│   ├── DTOs/
│   ├── Data/
│   ├── Repositories/
│   ├── Configuration/
│   ├── Middleware/
│   └── Program.cs
│
├── docs/
│
├── .gitignore
└── README.md
```

Do not create unnecessary layers if they are not yet useful.

---

# 6. Phase 1 — Basic Working AI Chat

## Objective

Create the smallest usable ChatGPT-style application.

At the end of Phase 1 a user must be able to:

1. open the application
2. type a message
3. send the message to the .NET backend
4. backend sends it to OpenRouter
5. receive an LLM response
6. display the response in the chat UI
7. continue the conversation with conversation history

Do not add RAG or agents yet.

---

## 6.1 Frontend Requirements

Build a simple ChatGPT-like interface.

Required UI:

```text
-----------------------------------------
| AI Chat                               |
-----------------------------------------
|                                       |
| User: What is RAG?                    |
|                                       |
| AI: RAG stands for...                 |
|                                       |
|                                       |
-----------------------------------------
| Ask something...               Send   |
-----------------------------------------
```

Required features:

- chat message list
- user message bubble
- assistant message bubble
- text input
- Send button
- Enter to send
- loading indicator
- disabled Send button during request
- auto-scroll to latest message
- basic error message display

Optional only after the basic UI works:

- New Chat button
- conversation sidebar
- markdown rendering
- code block rendering

Do not spend significant time copying ChatGPT's visual design exactly.

---

# 7. Backend API

Create an ASP.NET Core Web API.

Initial endpoint:

```http
POST /api/chat
```

Example request:

```json
{
  "conversationId": null,
  "message": "Explain RAG in simple terms."
}
```

Example response:

```json
{
  "conversationId": 1,
  "message": {
    "role": "assistant",
    "content": "RAG stands for Retrieval-Augmented Generation..."
  }
}
```

The controller must not directly contain OpenRouter HTTP logic.

Use:

```text
ChatController
      ↓
ChatService
      ↓
ILlmService
      ↓
OpenRouterLlmService
```

---

# 8. LLM Service Design

Create an interface similar to:

```csharp
public interface ILlmService
{
    Task<LlmResponse> GetChatCompletionAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}
```

OpenRouter implementation:

```text
OpenRouterLlmService
```

Responsibilities:

- read API configuration
- construct chat request
- attach Bearer token
- call OpenRouter
- deserialize response
- return normalized response to application code
- log errors safely

The rest of the application should not care whether the provider is OpenRouter, Azure OpenAI, OpenAI, or something else.

---

# 9. Roles

The application must support the standard conversation roles:

```text
system
user
assistant
```

Example context sent to the LLM:

```json
[
  {
    "role": "system",
    "content": "You are a helpful AI assistant."
  },
  {
    "role": "user",
    "content": "What is RAG?"
  },
  {
    "role": "assistant",
    "content": "RAG stands for Retrieval-Augmented Generation..."
  },
  {
    "role": "user",
    "content": "Give me an example."
  }
]
```

This part is important because it demonstrates context and conversation history.

---

# 10. Conversation Persistence

Once the first single-turn API request works, add SQL Server persistence.

Create these initial tables/entities.

## Conversation

```text
Id
Title
CreatedAt
UpdatedAt
```

## Message

```text
Id
ConversationId
Role
Content
CreatedAt
```

Relationship:

```text
Conversation
     │
     ├── Message
     ├── Message
     ├── Message
     └── Message
```

When a user sends a message:

```text
Receive Request
      ↓
Get/Create Conversation
      ↓
Save User Message
      ↓
Load Relevant Conversation History
      ↓
Call OpenRouter
      ↓
Save Assistant Message
      ↓
Return Response
```

Do not load unlimited history forever. Initially a simple last-N-messages limit is acceptable.

Example:

```text
Last 20 messages
```

Later this will become a proper context-management component.

---

# 11. Phase 1 Completion Criteria

Phase 1 is complete only when all of the following work:

- React application starts successfully
- ASP.NET Core API starts successfully
- SQL Server connection works
- OpenRouter configuration works
- user sends a message
- backend calls OpenRouter
- assistant response appears in UI
- system/user/assistant roles work
- conversation is stored in SQL Server
- existing conversation can be continued
- refresh does not destroy saved conversations
- API errors are displayed gracefully
- API key is not present in Git

Once these are stable, stop Phase 1.

Do not automatically start implementing RAG.

---

# 12. Phase 2 — Context Management

Start this only after Phase 1 is complete.

Goal:

Understand what information is sent to the LLM and control it deliberately.

Build a `ContextBuilder` service.

```text
ContextBuilder
│
├── System Instructions
├── Current User Message
└── Relevant Conversation History
```

Future version:

```text
ContextBuilder
│
├── System Instructions
├── Current User Message
├── Conversation History
├── RAG Results
└── Tool Results
```

Experiments to perform:

- last 5 messages vs last 20
- long conversation
- irrelevant history
- changing system prompts
- structured output prompts

Phase 2 teaches:

- context
- context windows
- history
- system/user/assistant roles
- prompt engineering
- context prioritization

---

# 13. Phase 3 — RAG

Do not implement until requested.

Goal:

Allow users to upload their own documents and ask questions from them.

Initial document formats can be limited to:

- `.txt`
- `.md`

PDF support can be added later.

RAG ingestion architecture:

```text
Document Upload
      ↓
Extract Text
      ↓
Chunk Text
      ↓
Generate Embeddings
      ↓
Store Vectors
```

RAG query architecture:

```text
User Question
      ↓
Query Embedding
      ↓
Vector Search
      ↓
Metadata Filtering
      ↓
Top-K Chunks
      ↓
Context Builder
      ↓
OpenRouter LLM
      ↓
Grounded Answer
      ↓
Citations
```

Concepts to learn in this phase:

- embeddings
- semantic search
- vector databases
- chunk size
- chunk overlap
- Top-K retrieval
- metadata filtering
- grounding
- citations
- retrieval quality

The vector database will be selected when this phase begins. Do not introduce one in Phase 1.

---

# 14. Phase 4 — Tool / Function Calling

Do not implement until requested.

Add backend-controlled tools such as:

```text
GetCurrentDate
SearchKnowledgeBase
GetConversation
SearchMeeting
GetMeetingParticipants
```

Flow:

```text
User
 ↓
LLM
 ↓
Tool Decision
 ↓
Schema Validation
 ↓
Authorization
 ↓
Backend Function
 ↓
Tool Result
 ↓
LLM
 ↓
Final Response
```

Important rule:

```text
LLM → requests action
Backend → validates and executes action
```

Never:

```text
LLM → directly modifies database
```

---

# 15. Phase 5 — AI Agent

Do not implement until requested.

Once chat + RAG + tools are working, build the first real agent.

Example request:

```text
Find the meeting where Google transcription credentials were discussed,
retrieve the relevant transcript,
show the participants,
and summarize the decisions.
```

Possible execution:

```text
User Request
      ↓
Understand Goal
      ↓
Search Meeting
      ↓
Get Participants
      ↓
Retrieve Transcript via RAG
      ↓
Analyze Retrieved Evidence
      ↓
Generate Answer
      ↓
Return Citations
```

At this stage evaluate whether Semantic Kernel or another agent framework is useful.

Do not introduce a framework merely because it exists.

---

# 16. Phase 6 — Advanced Agent Workflow

Possible later functionality:

```text
Start
 ↓
Understand Request
 ↓
Need Knowledge?
 ├── Yes → RAG
 └── No
 ↓
Need Action?
 ├── Yes → Tool
 └── No
 ↓
Sensitive Action?
 ├── Yes → Human Approval
 └── No
 ↓
Execute
 ↓
Final Response
```

Potential concepts:

- agent state
- retries
- branching
- fallback strategy
- human-in-the-loop
- approvals
- workflow persistence

Only add LangGraph or another workflow framework if these requirements justify it.

---

# 17. Phase 7 — Production Engineering

Later add:

- token tracking
- request latency
- model name
- errors
- retry logic
- fallback models
- rate-limit handling
- prompt versioning
- agent versioning
- RAG evaluation
- hallucination evaluation
- tool-call logs
- approval logs
- observability platform

Possible observability options can be evaluated later, including LangSmith, Langfuse, or custom telemetry.

---

# 18. AI Safety / Architecture Rules

Never put critical business rules only inside prompts.

Bad:

```text
System prompt:
Never delete meetings unless the user is an administrator.
```

Better:

```text
LLM requests DeleteMeeting
          ↓
Backend authentication
          ↓
Backend authorization
          ↓
Business rule validation
          ↓
Database
```

Prompt instructions improve model behavior.

Backend validation enforces system behavior.

Use both where appropriate.

---

# 19. Configuration

Recommended development configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
  },
  "OpenRouter": {
    "BaseUrl": "https://openrouter.ai/api/v1",
    "Model": "openrouter/free",
    "ApplicationName": "AI Chat Learning Project"
  }
}
```

API key should come from:

```text
OPENROUTER_API_KEY
```

or .NET user secrets.

Do not put it in `appsettings.json` committed to Git.

---

# 20. Suggested First Codex Task

Give Codex the following instruction after adding this README to the repository:

```text
Read README.md completely before making changes.

Implement only Phase 1.

First create the initial project structure with:
- React frontend
- ASP.NET Core Web API backend
- SQL Server / Entity Framework Core setup
- OpenRouter integration using a configurable ILlmService

Use the OpenRouter free model router configured as `openrouter/free`.

Do not implement RAG, embeddings, vector databases, tools, agents,
LangChain, LangGraph, LangFlow, LangSmith, Semantic Kernel, or any
future phase yet.

Build the smallest working vertical slice first:
React message input → .NET API → OpenRouter → assistant response → UI.

After that works, add SQL Server conversation persistence.

Keep the solution buildable and runnable after each change.
Explain major architecture decisions in the implementation summary.
```

---

# 21. Recommended Learning Checkpoints

After Phase 1, be able to explain:

```text
What is an LLM request?
What are system/user/assistant roles?
What information is sent as context?
How is conversation history maintained?
Why shouldn't the frontend call OpenRouter directly?
Why is ILlmService useful?
```

After Phase 2:

```text
What is context management?
Why can't we send unlimited conversation history?
What should be prioritized?
```

After Phase 3:

```text
What is RAG?
What are embeddings?
What does vector similarity mean?
What is Top-K?
Why is chunking important?
What does metadata filtering do?
Why are citations important?
```

After Phase 4:

```text
What is tool calling?
How does the LLM select a tool?
What is a tool schema?
Why must the backend validate tool calls?
```

After Phase 5:

```text
What makes this an agent instead of a chatbot?
What are goal, context, tools, constraints, and state?
```

---

# 22. Final Target

The long-term application should eventually look like:

```text
                       React Client
                            ↓
                      ASP.NET Core API
                            ↓
                     Authentication
                            ↓
                      Context Builder
                ┌───────────┼───────────┐
                ↓           ↓           ↓
              History      RAG       Tool Results
                └───────────┼───────────┘
                            ↓
                       Agent Runtime
                            ↓
                       OpenRouter LLM
                     ↙             ↘
                   RAG              Tools
                    ↓                 ↓
              Vector Store        Backend APIs
                     ↘             ↙
                       Final Answer
                            ↓
                    Logs / Evaluation
```

But reach this architecture **incrementally**.

The immediate target is only:

```text
React
  ↓
.NET API
  ↓
OpenRouter
  ↓
Response
  ↓
SQL Server conversation history
```

Make that reliable first. Then start the next development phase.

---

# 23. OpenRouter References

Official resources:

- Developer documentation: https://openrouter.ai/developers
- Free models collection: https://openrouter.ai/collections/free-models
- Free model router: https://openrouter.ai/openrouter/free
- Models browser: https://openrouter.ai/models

Free-model availability and limits can change, so confirm current OpenRouter availability before depending on a particular free model in production.
