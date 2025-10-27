# Claude Code Documentation

Welcome! This directory contains documentation for AI assistants working on the Christmas List Management App.

## Start Here

**New to this project?** Read these in order:

1. **[AGENT_ONBOARDING.md](AGENT_ONBOARDING.md)** - Start here for project context, architecture overview, development patterns, and key decisions
2. **[git-workflow.md](git-workflow.md)** - Reference this when making commits (workflow, message format, branch strategy)

## Project Documentation

Additional documentation in the root directory:

- **[FEATURES_AND_REQUIREMENTS.md](../FEATURES_AND_REQUIREMENTS.md)** - Comprehensive feature catalog with user stories, technical requirements, and architecture details
- **[REFACTOR_BACKLOG.md](../REFACTOR_BACKLOG.md)** - Working task list prioritized by High/Medium/Low priority

## Quick Reference

**Tech Stack:**
- .NET 9.0 Blazor Server
- Bootstrap 5 (jQuery removed)
- Google Sheets API (backend)
- Google Custom Search API (product images)

**Key Services:**
- `googleSheetsListService` - CRUD operations
- `allListsService` - Global state cache
- `ProductMetadataService` - OpenGraph scraping
- `GoogleImageSearchService` - Product image retrieval
- `LocalStorageService` - Remember Me functionality

**Components:**
- `Login.razor` - Authentication
- `MyList.razor` - Personal gift list management
- `ListReview.razor` - Browse & claim family members' gifts
- `MyGifts.razor` - Track claimed gifts

## Development Workflow

1. Use `TodoWrite` tool to track multi-step tasks
2. Follow commit conventions in [git-workflow.md](git-workflow.md)
3. Update [REFACTOR_BACKLOG.md](../REFACTOR_BACKLOG.md) when completing tasks
4. Mark items with `[x]` when done

## Notes

- This is a **family project** - pragmatic decisions over enterprise patterns
- Security trade-offs are intentional (plain text passwords, Google Sheets backend)
- Test coverage is 0% - be aware when making changes
- Focus on UX polish and user experience
