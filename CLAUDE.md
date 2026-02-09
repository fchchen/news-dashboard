# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Build and Run Commands

```bash
# Build entire solution
dotnet build

# Run API (from project root)
dotnet run --project src/Api/NewsDashboard.Api.csproj

# Run Azure Functions locally
cd src/Functions && func start

# Run Angular frontend
cd frontend && npm start

# Run tests
dotnet test

# Run single test file
dotnet test --filter "FullyQualifiedName~HackerNewsServiceTests"
```

## Architecture Overview

AI News Intelligence Dashboard tracking Anthropic, OpenAI, and AI CLI tool news:

- **src/Api** - ASP.NET Core 8 Minimal API
  - `Endpoints/` - API route handlers (Dashboard, HackerNews, GitHubReleases, RssFeeds)
  - `Services/` - Business logic with interface abstractions
  - `Middleware/` - Global exception handling
  - Background service for periodic news fetching (every 15 min)

- **src/Functions** - Azure Functions (isolated worker model) with timer triggers
  - Fetch HN stories, GitHub releases, RSS feeds on schedule

- **src/Shared** - Shared library: models, DTOs, constants (AI keywords, feed sources, tracked repos)

- **frontend** - Angular 21 standalone components with Material Design
  - Dashboard overview, Hacker News, GitHub Releases, RSS Feeds pages
  - Signals for state management

## Data Sources (All Free, No API Keys Required)

1. **Hacker News** - Firebase API, filtered by AI keywords
2. **GitHub Releases** - REST API for AI CLI tool repos (claude-code, codex, cursor, aider, cline, continue)
3. **RSS Feeds** - Anthropic blog, OpenAI blog, TechCrunch AI, The Verge AI, Ars Technica, VentureBeat AI

## Key Integration Points

1. **Cosmos DB**: Partition key `/source`, camelCase naming, 14-day TTL on NewsItems
2. **CORS**: API allows `http://localhost:4200` for Angular dev server
3. **AI Keyword Filter**: Applied to HN titles and tech news RSS items
4. **Company Detection**: Items tagged as Anthropic, OpenAI, or Other based on content

## Testing Patterns

- xUnit + Moq + FluentAssertions
- Services use interface abstractions for mockability
- Integration tests use `WebApplicationFactory<Program>`
