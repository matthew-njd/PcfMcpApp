# PCF MCP APP

A natural language sales query system built with **.NET 10**, **Model Context Protocol (MCP)**, and **Angular**. Users ask plain English questions about sales data — Claude interprets the intent, calls the appropriate MCP tools, and returns a human-readable answer.

---

## How It Works

```
Angular UI → your .NET backend (authenticated)
  → Anthropic API (server-side, key stays hidden)
    → your MCP server /mcp endpoint (internal, localhost or same network)
      → EF Core → SQL Server
  ← formatted response
← displayed in UI
```

---

## Tech Stack

| Layer                | Technology                               |
| -------------------- | ---------------------------------------- |
| Backend / MCP Server | .NET 10 Minimal API (ASP.NET Core)       |
| ORM                  | Entity Framework Core 10                 |
| MCP Library          | `ModelContextProtocol.AspNetCore` v1.0.0 |
| AI / LLM             | Anthropic Claude                         |
| Database (Dev)       | SQLite                                   |
| Database (Prod)      | SQL Server                               |
| Frontend             | Angular                                  |

---

### Example Natural Language Questions

- _"How much has Stark Enterprises spent this year?"_
- _"Who are our top 3 customers last quarter?"_
- _"How did we do overall in January?"_
- _"When did Wayne Technologies last make a purchase?"_
- _"Show me the 5 most recent sales across all customers"_

---

## Roadmap

- [ ] Angular frontend (chat UI + Anthropic API integration)
- [ ] Authentication / API key protection on `/mcp`
- [ ] Additional tools (e.g. sales by product, customer detail lookup)
- [ ] Docker / Azure App Service deployment config
- [ ] EF Core migrations for production schema management
