# Agent Onboarding Guide

Welcome to the Christmas List Management App! This guide helps AI assistants quickly understand the project architecture, patterns, and context.

---

## Project Overview

### What This App Does
A family-focused web application for managing Christmas gift lists. Family members can:
- Create and manage their own wish lists
- Browse other family members' lists
- Claim gifts they plan to purchase
- Track what they've claimed across all family members
- View product images and metadata automatically scraped from links

### Key Context
- **Audience**: Single family (not multi-tenant)
- **Scale**: ~10-15 users, ~100-200 items
- **Philosophy**: Pragmatic family project, not enterprise software
- **Security**: Intentional trade-offs (plain text passwords, Google Sheets backend with manual admin access)
- **Focus**: User experience and polish over enterprise patterns

---

## Technology Stack

### Core Technologies
- **.NET 9.0** - Upgraded from .NET 5.0 (commit f6eee88)
- **Blazor Server** - Server-side rendering with SignalR
- **Bootstrap 5** - Upgraded from Bootstrap 4, jQuery removed (commit ebb2f8a)
- **C# / Razor Components** - Component-based UI

### Backend / Data
- **Google Sheets API v4** - Primary data store (Users sheet, Items sheet)
- **OAuth2 Authentication** - `app_client_secret.json` for Google Sheets access
- **No traditional database** - Google Sheets provides collaborative editing and manual admin access

### External APIs
- **Google Custom Search API** - Product image retrieval (commit 87102af)
- **OpenGraph Protocol** - Metadata scraping from product URLs (commit ee6c781)

### UI Framework
- **Bootstrap 5** - Modern component library
- **Custom CSS Variables** - Modern Christmas theme (Pine Green, Cranberry Red, Gold) (commit accbed4)
- **Font**: Inter Bold - Clean, modern sans-serif
- **Blazored.Toast** - User notifications

---

## Architecture Overview

### Component Structure

**Authentication:**
- `Login.razor` - User authentication, "Remember Me" functionality

**Main Views:**
- `MyList.razor` - Personal gift list CRUD operations
- `ListReview.razor` - Browse family members' lists, claim/unclaim items
- `MyGifts.razor` - View all claimed gifts grouped by family member

**Legacy:**
- `SpiceList.razor` - Appears unused, potentially legacy feature

### Service Layer

**Core Services:**
- `googleSheetsListService` - Primary CRUD operations for users and items
- `allListsService` - Global state cache (scoped), stores all lists for session
- `userIdService` - Session state for current user (scoped)

**Feature Services:**
- `ProductMetadataService` - OpenGraph scraping, orchestrates image search
- `GoogleImageSearchService` - Google Custom Search API integration for product images
- `LocalStorageService` - Browser localStorage for "Remember Me" feature

**Legacy Services:**
- `googleSheetsSpiceService` - Secondary spreadsheet, appears unused

### Data Models

**UserModel:**
```csharp
int Id
string Family
string Name
string Password        // Plain text (intentional)
string Notes           // Personal notes shown to family
```

**ItemModel:**
```csharp
int ItemId
string Name            // Owner's name
string Item            // Item description
string Link            // Product URL
string Notes           // Item-specific notes
string DateUpdated     // String format (matches Google Sheets)
string Claimer         // Who claimed it
string DateClaimed     // String format
int Active             // Soft delete flag (1=active, 0=deleted)

// Product metadata (added commit 737ef6d)
string ImageUrl
decimal? Price         // Nullable
string MetadataFetchedDate
```

**ListModel:**
```csharp
List<ItemModel> List
string Name
int itemsListed
int itemsClaimed
DateTime lastUpdated
string dropDownStr     // Display string (presentation concern in model)
```

### Data Flow

1. **Login** → `googleSheetsListService.Users_GetList()` → Validate password → Set `userIdService.UserId`
2. **Bulk Load** → `googleSheetsListService.GetAllItems()` → Cache in `allListsService.AllLists`
3. **My List** → Filter `allListsService` by current user → Display/Edit
4. **List Review** → Group `allListsService` by family member → Display person cards + items
5. **Claims** → Update Google Sheets → Update `allListsService` cache → Update UI

---

## Key Design Patterns

### State Management
- **Scoped Services**: `allListsService` and `userIdService` maintain session state
- **Bulk Loading**: All items loaded once at login, cached in `allListsService` (commit 7e947e8)
- **Optimistic Updates**: UI updates immediately, then syncs with Google Sheets
- **Conflict Detection**: Claim/unclaim operations verify data hasn't changed (commit e8f7740)

### Component Patterns
- **No Inheritance**: Previously used component inheritance (anti-pattern), removed in commit 15c2b8c
- **Cascading Parameters**: Shared state passed through service injection
- **Conditional Rendering**: Edit mode toggles via state flags (`editItemId`, `editNotesFlag`)

### Error Handling
- **Retry Logic**: Exponential backoff for Google Sheets API failures (commit 7a8f089)
- **User-Friendly Messages**: Toast notifications for success/error/info
- **Graceful Degradation**: OpenGraph failures don't block image search (commit 67cdc43)

### UI/UX Patterns
- **Loading Spinners**: Visual feedback during async operations (commit bd1f135)
- **Confirmation Dialogs**: Destructive actions require confirmation (commit 1ea7d76)
- **Cancel Buttons**: Exit edit mode without saving (commit 620e2c0)
- **Input Validation**: Client-side validation for URLs, required fields, character limits (commit 06004b6)
- **Responsive Design**: Side-by-side desktop layout, vertical mobile stack (commit 71fb8ea)

---

## Codebase Navigation

### Directory Structure
```
WebApplication1/
├── Components/           # Razor components (UI)
│   ├── Login.razor
│   ├── MyList.razor
│   ├── ListReview.razor
│   └── MyGifts.razor
├── Models/
│   └── ListModel.cs      # All data models
├── Services/             # Business logic & API integration
│   ├── googleSheetsListService.cs
│   ├── allListsService.cs
│   ├── userIdService.cs
│   ├── ProductMetadataService.cs
│   ├── GoogleImageSearchService.cs
│   └── LocalStorageService.cs
├── Pages/
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/
│   ├── css/
│   │   ├── site.css
│   │   ├── theme.css
│   │   └── components.css
│   └── js/
│       └── site.js
├── Startup.cs            # Service registration
├── appsettings.json      # Configuration (SpreadsheetIds, API keys)
└── app_client_secret.json # Google OAuth credentials (gitignored)
```

### Key Files to Read First

**Understanding the app:**
1. [Login.razor](../WebApplication1/Components/Login.razor) - Entry point, authentication flow
2. [googleSheetsListService.cs](../WebApplication1/Services/googleSheetsListService.cs) - Core data operations
3. [ListModel.cs](../WebApplication1/Models/ListModel.cs) - Data structure
4. [MyList.razor](../WebApplication1/Components/MyList.razor) - Primary CRUD example
5. [ListReview.razor](../WebApplication1/Components/ListReview.razor) - Complex UI with claims

**Configuration:**
- [appsettings.json](../WebApplication1/appsettings.json) - Google Sheets IDs, API keys
- [Startup.cs](../WebApplication1/Startup.cs) - Service registration

**Styling:**
- [theme.css](../WebApplication1/wwwroot/css/theme.css) - CSS variables, color scheme
- [components.css](../WebApplication1/wwwroot/css/components.css) - Component-specific styles

---

## Development Guidelines

### Commit Workflow
See [git-workflow.md](git-workflow.md) for detailed commit conventions.

**Summary:**
- Ask before committing completed features
- One commit per backlog item
- Include backlog checkbox update in same commit
- Use structured commit messages with category tags

### Task Management
- Use `TodoWrite` tool for multi-step tasks (3+ steps)
- Mark tasks `in_progress` before starting (one at a time)
- Mark `completed` immediately after finishing
- Update [REFACTOR_BACKLOG.md](../REFACTOR_BACKLOG.md) when completing items

### Testing Strategy
- **Current State**: 0% test coverage
- **Approach**: Manual testing during development
- **Be Careful**: No safety net, test changes thoroughly before committing

### Code Style
- Follow existing patterns in the codebase
- Maintain Bootstrap 5 conventions (no Bootstrap 4 classes)
- Use Modern Christmas color theme (CSS variables in theme.css)
- Keep components focused (single responsibility)

---

## Important Decisions & Context

### "Won't Do (For Now)" Items

These have been consciously deferred or rejected:

**Password Security:**
- **Decision**: Keep plain text passwords
- **Rationale**: Family app with manual Google Sheets admin access; adding hashing complicates family management
- **Trade-off**: Acceptable for trusted family environment

**Date Types:**
- **Decision**: Keep DateUpdated/DateClaimed as strings
- **Rationale**: Google Sheets loads as text; changing would require data migration and risk breaking existing data
- **Trade-off**: String format works fine for display purposes

**Database Migration:**
- **Decision**: Continue using Google Sheets
- **Rationale**: Provides collaborative editing, manual admin access, and simplicity
- **Trade-off**: Not scalable, but sufficient for family use

**Email Notifications:**
- **Decision**: Deferred
- **Rationale**: Not essential for family coordination

**Dark Mode:**
- **Decision**: Deferred
- **Rationale**: Modern Christmas theme is well-established; dark mode is low priority

### Accepted Technical Debt

**Known Issues (Low Priority):**
- `dropDownStr` in ListModel - Presentation concern in domain model
- SpiceModel/SpiceService - Appears unused but not removed
- No pagination - Could be slow with 500+ items (unlikely for family use)
- Magic numbers - Some hard-coded values in styling

**Why These Are Acceptable:**
- Family app with limited scale
- Performance is adequate for current use case
- Refactoring these provides minimal user benefit

---

## Recent Major Changes

### Completed in Recent Sessions (Last 28 Commits)

**Infrastructure:**
- Upgraded to .NET 9.0 (commit f6eee88)
- Upgraded Bootstrap 4 → 5, removed jQuery (commit ebb2f8a)
- Moved Google Sheets credentials to configuration (commit ce3b34a)
- Removed component inheritance anti-pattern (commit 15c2b8c)

**Performance:**
- Bulk data loading (90% reduction in API calls) (commit 7e947e8)
- Comprehensive error handling with retry logic (commit 7a8f089)

**Features:**
- Product metadata: OpenGraph scraping + Google Image Search (commits 737ef6d, ee6c781, 87102af, 67cdc43)
- "Remember Me" with LocalStorage (commit 41e21fa)
- Delete confirmations (commit 1ea7d76)
- Cancel buttons and single-item edit enforcement (commit 620e2c0)
- Input validation (commit 06004b6)

**UX/UI:**
- Modern Christmas theme with CSS variables (commit accbed4)
- Family-based List Review with color-coded person cards (commit d2f49ef)
- Item card claiming workflow with animations (commit 7450565)
- Robust claim/unclaim with conflict detection (commit e8f7740)
- Datetime formatting and unclaim management in My Gifts (commit 09d4b8c)
- Side-by-side responsive layout (commit 71fb8ea)
- Inter font, custom navigation, QOL improvements (commit 68265e7)
- Loading spinners and accessibility (commit bd1f135)

---

## Common Tasks

### Adding a New Feature

1. **Read the backlog**: Check [REFACTOR_BACKLOG.md](../REFACTOR_BACKLOG.md) for context
2. **Use TodoWrite**: Break down multi-step tasks
3. **Follow patterns**: Look at similar existing features
4. **Test manually**: No automated tests, verify all scenarios
5. **Update docs**: Update FEATURES_AND_REQUIREMENTS.md if adding/changing user stories
6. **Commit**: Follow [git-workflow.md](git-workflow.md) conventions

### Updating Google Sheets Data Model

**Items Sheet Columns (in order):**
1. ItemId (int)
2. Name (string) - Owner's name
3. Item (string) - Item description
4. Link (string) - Product URL
5. Notes (string)
6. DateUpdated (string)
7. Claimer (string)
8. DateClaimed (string)
9. Active (int) - 1=active, 0=deleted
10. ImageUrl (string) - Added commit 737ef6d
11. Price (decimal) - Added commit 737ef6d
12. MetadataFetchedDate (string) - Added commit 737ef6d

**Users Sheet Columns:**
1. Id (int)
2. Family (string)
3. Name (string)
4. Password (string)
5. Notes (string)

**Important:** Changes to sheet structure require updates in `googleSheetsListService.cs` data parsing methods.

### Working with Product Metadata

**Flow:**
1. User enters item with Link in MyList.razor
2. `ProductMetadataService.FetchMetadataAsync(link)` called
3. Try OpenGraph scraping first (og:image, og:title, og:price)
4. Fallback to Google Custom Search API if needed
5. Progressive fallback: Try full URL → domain-only → skip
6. Store ImageUrl, Price, MetadataFetchedDate in Google Sheets

**Configuration:**
- Google Custom Search API key: Environment variable or appsettings.json
- SearchEngineId: `appsettings.json` → `GoogleCustomSearch:SearchEngineId`

---

## Troubleshooting

### Common Issues

**Google Sheets API Errors:**
- Check `app_client_secret.json` exists and is valid
- Verify SpreadsheetId in `appsettings.json`
- Check Google Cloud Console API quotas

**Product Images Not Loading:**
- Check Google Custom Search API key is set
- Verify SearchEngineId in appsettings.json
- Check browser console for CORS/CSP errors
- OpenGraph scraping may fail for sites blocking bots (graceful degradation)

**LocalStorage "Remember Me" Not Working:**
- Requires JavaScript interop (site.js)
- Check browser console for errors
- Clear localStorage and test: `localStorage.clear()`

**Claim Conflicts:**
- Conflict detection implemented (commit e8f7740)
- If item already claimed by another user, shows error toast
- Refresh data from Google Sheets on conflict

---

## Useful Commands

### Development
```bash
dotnet run                    # Run app locally
dotnet build                  # Build project
```

### Git
```bash
git status                    # Check current state
git log --oneline -10         # Recent commits
git diff                      # See changes
```

### Finding Code
```bash
# Use Grep tool for code search (better than grep/rg commands)
# Use Glob tool for file patterns (better than find)
# Use Read tool for reading files (better than cat)
```

---

## Questions to Ask User

If uncertain about implementation choices:

1. **Feature Scope**: "Should this work for all family members or just the current user?"
2. **UX Decision**: "Would you like a confirmation dialog for this action?"
3. **Performance Trade-off**: "This could be faster with caching but more complex. What do you prefer?"
4. **Technical Debt**: "This works but could be refactored. Is it worth doing now?"
5. **Backlog Priority**: "Should this be High, Medium, or Low priority?"

---

## Additional Resources

- [FEATURES_AND_REQUIREMENTS.md](../FEATURES_AND_REQUIREMENTS.md) - Comprehensive feature documentation
- [REFACTOR_BACKLOG.md](../REFACTOR_BACKLOG.md) - Current task list
- [git-workflow.md](git-workflow.md) - Commit conventions

---

**Last Updated**: Session ending 2025-10-27 (28 commits reviewed)
