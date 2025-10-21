# Git Workflow Configuration

This document defines the Git workflow preferences for this project when working with Claude Code.

---

## Commit Strategy

### Hybrid Approach
- **During Active Work**: Claude does NOT automatically commit while implementing features
- **When Item Complete**: Claude asks for approval before committing
- **One Commit Per Feature**: Each completed backlog item gets one logical commit
- **Backlog Updates Included**: REFACTOR_BACKLOG.md checkbox updates are included in the feature commit (not separate)

---

## Commit Message Format

```
[Category] Brief description

- Detailed change 1
- Detailed change 2
- Detailed change 3
- Closes backlog item: [item name]

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

### Categories
- `[Feature]` - New functionality
- `[Fix]` - Bug fixes
- `[Refactor]` - Code improvements without behavior change
- `[UX]` - User experience improvements
- `[Docs]` - Documentation only
- `[Config]` - Configuration changes
- `[Security]` - Security improvements
- `[Performance]` - Performance optimizations
- `[Chore]` - Maintenance tasks

---

## Example Commit Messages

### Feature Addition
```
[Feature] Add delete confirmation dialogs

- Added confirmation modal in MyList.razor before deleting items
- Prevents accidental deletions
- Uses Bootstrap modal for consistency with existing UI
- Closes backlog item: Add delete confirmations

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

### Bug Fix
```
[Fix] Correct typo in GetAllItems method name

- Renamed GetAllItmes → GetAllItems in googleSheetsListService
- Updated all references in MyGifts.razor
- Closes backlog item: Fix typo in method name

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

### Refactoring
```
[Refactor] Remove component inheritance antipattern

- Extracted shared authentication logic to AuthenticationStateProvider
- Replaced @inherits Login with cascading parameters
- Updated MyList, ListReview, and MyGifts components
- Improved component reusability and testability
- Closes backlog item: Remove component inheritance

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Workflow Steps

1. **Start Work**
   - User selects backlog item to work on
   - Claude uses TodoWrite to track implementation progress

2. **Implementation**
   - Claude makes code changes
   - Uses multiple tools as needed
   - Keeps user informed of progress

3. **Completion Check**
   - Claude announces: "Feature is complete. Would you like me to commit these changes?"
   - User reviews and approves

4. **Commit**
   - Claude creates commit with structured message
   - Includes backlog checkbox update in same commit
   - Confirms commit was successful

5. **Continue**
   - Move to next backlog item or take a break

---

## Review Before Commit

Before committing, user may want to:
- Review git diff
- Test the changes locally
- Request adjustments
- See which files changed

Claude will accommodate any review requests before finalizing the commit.

---

## Branch Strategy

- **Main branch**: `main` (current)
- **Work directly on main**: Yes, for now (small family project)
- **Feature branches**: Optional, can be used for larger refactors if desired

---

## Notes

- This is a small family project, so workflow is kept simple
- Adjust this configuration as project needs evolve
- Claude will follow these guidelines unless explicitly instructed otherwise
